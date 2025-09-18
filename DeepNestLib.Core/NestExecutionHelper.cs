namespace DeepNestLib
{
  using IO;
  using NestProject;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  public class NestExecutionHelper
  {
    private readonly IMaterialCatalog m_materialCatalog;

    public NestExecutionHelper()
    {
    }

    public NestExecutionHelper(IMaterialCatalog materialCatalog) => m_materialCatalog = materialCatalog;

    /// <summary>
    ///   Reorders the sheets based on their area and adds them to the nesting context until the total area of the raw details is covered.
    /// </summary>
    /// <typeparam name="T">The type of the raw detail.</typeparam>
    /// <param name="context">The nesting context.</param>
    /// <param name="rawDetails">The raw details to nest.</param>
    /// <param name="sheetLoadInfos">The sheet load information.</param>
    /// <param name="packingEfficiency">The packing efficiency.</param>
    private void ReorderSheetsAndAddToContext<T>(NestingContext context, IList<T> rawDetails, IList<ISheetLoadInfo> sheetLoadInfos, double packingEfficiency)
        where T : IRawDetail
    {
      // sort the sheet sizes by area (width * height) descending
      List<ISheetLoadInfo> orderedInfos = sheetLoadInfos.OrderByDescending(o => o.Width * o.Height).ToList();
      sheetLoadInfos.Clear();
      foreach (ISheetLoadInfo item in orderedInfos)
      {
        sheetLoadInfos.Add(item);
      }

      // calc the total area of the sheets
      var totalArea = rawDetails.Sum(detail => detail.ToNfp().Area * detail.Quantity * context.Config.Multiplier);
      Debug.Print($"Total area of parts: {totalArea}");

      while (totalArea >= 0)
      {
        ISheetLoadInfo bestFitSheet = m_materialCatalog.SelectBestFitSheet(totalArea, sheetLoadInfos.FirstOrDefault()?.Material, context.Config, packingEfficiency);

        var src = context.GetNextSheetSource();

        var sheetArea = (bestFitSheet.Width - context.Config.SheetSpacing * 2) * (bestFitSheet.Height - context.Config.SheetSpacing * 2) * packingEfficiency;
        Debug.Print($"Sheet area: {sheetArea}");
        totalArea -= sheetArea;
        Sheet ns = Sheet.NewSheet(context.Sheets.Count + 1, bestFitSheet.Width, bestFitSheet.Height);
        ns.Material = bestFitSheet.Material;
        ns.UniqueId = bestFitSheet.UniqueSheetId;
        context.Sheets.Add(ns);
        ns.Source = src;
        bestFitSheet.Quantity++;

        context.ReorderSheets();
        Debug.Print($"Total area left: {totalArea}");
      }
    }

    public int InitialiseNest<T>(NestingContext context, IList<ISheetLoadInfo> sheetLoadInfos, IList<T> rawDetails, IProgressDisplayer progressDisplayer, double packingEfficiency) where T : IRawDetail
    {
      context.Reset();

      ReorderSheetsAndAddToContext(context, rawDetails, sheetLoadInfos, packingEfficiency);

      // progressDisplayer.DisplayTransientMessage(string.Empty);
      var src = 0;
      foreach (IRawDetail detail in rawDetails.Where(o => o.IsIncluded))
      {
        AddToPolygons(context, src, detail, detail.Quantity, progressDisplayer, detail.IsIncluded, false, true, detail.StrictAngle);
        src++;
      }

      return src;
    }

    public void AddToPolygons(NestingContext context, int src, IRawDetail det, int quantity, IProgressDisplayer progressDisplayer, bool isIncluded = true, bool isPriority = false, bool isMultiplied = false,
      AnglesEnum strictAngles = AnglesEnum.AsPreviewed)
    {
      DetailLoadInfo item = new()
      {
        Quantity = quantity,
        IsIncluded = isIncluded,
        IsPriority = isPriority,
        IsMultiplied = isMultiplied,
        StrictAngle = strictAngles
      };
      AddToPolygons(context, src, det, item, progressDisplayer);
    }

    public void AddToPolygons(NestingContext context, int src, IRawDetail det, DetailLoadInfo item, IProgressDisplayer progressDisplayer)
    {
      INfp loadedNfp;
      if (det.TryConvertToNfp(src, out loadedNfp))
      {
        loadedNfp.IsPriority = item.IsPriority;
        loadedNfp.StrictAngle = item.StrictAngle;
        var quantity = item.Quantity * (item.IsMultiplied ? context.Config.Multiplier : 1);
        for (var i = 0; i < quantity; i++)
        {
          context.Polygons.Add(loadedNfp.Clone());
        }
      }
      else
      {
        progressDisplayer.DisplayMessageBox($"Failed to import {det.Name}.", "Load Error", MessageBoxIcon.Stop);
      }
    }
  }
}
namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using IO;
  using NestProject;

  public class NestExecutionHelper
  {
    private readonly IMaterialCatalog m_materialCatalog;

    public NestExecutionHelper()
    {
    }

    public NestExecutionHelper(IMaterialCatalog materialCatalog) => m_materialCatalog = materialCatalog;

    public void InitialiseNest(NestingContext context, IList<ISheetLoadInfo> sheetLoadInfos, IList<IRawDetail> rawDetails, IProgressDisplayer progressDisplayer)
    {
      progressDisplayer.IsVisibleSecondaryProgressBar = false;
      context.Reset();

      // sort the sheet sizes by area (width * height) descending
      List<ISheetLoadInfo> orderedInfos = sheetLoadInfos.OrderByDescending(o => o.Width * o.Height).ToList();
      sheetLoadInfos.Clear();
      foreach (ISheetLoadInfo item in orderedInfos)
      {
        sheetLoadInfos.Add(item);
      }

      // calc the total area of the sheets
      var totalArea = rawDetails.Sum(detail => detail.ToNfp().Area * detail.Quantity * SvgNest.Config.Multiplier);

      double packingEfficiency;
      if (rawDetails.All(detail => detail.IsRectangle()))
      {
        packingEfficiency = 0.9; // high efficiency for rectangles
      }
      else if (rawDetails.All(detail => detail.IsCircle()))
      {
        packingEfficiency = 0.8; // medium efficiency for circles
      }
      else
      {
        packingEfficiency = 0.7; // low efficiency for other shapes
      }

      var src = 0;
      while (totalArea > 0)
      {
        ISheetLoadInfo bestFitSheet = m_materialCatalog.SelectBestFitSheet(totalArea, packingEfficiency, sheetLoadInfos.First().Material.Name);

        // if (bestFitSheets.Count == 0)
        // {
        //   progressDisplayer.DisplayMessageBox("No sheets available to fit the parts.", "Error", MessageBoxIcon.Error);
        // }
        src = context.GetNextSheetSource();

        totalArea -= (bestFitSheet.Width - SvgNest.Config.SheetSpacing * 2) * (bestFitSheet.Height - SvgNest.Config.SheetSpacing * 2) * packingEfficiency;

        Sheet ns = Sheet.NewSheet(context.Sheets.Count + 1, bestFitSheet.Width, bestFitSheet.Height);
        context.Sheets.Add(ns);
        ns.Source = src;
        bestFitSheet.Quantity++;

        context.ReorderSheets();

        progressDisplayer.DisplayTransientMessage(string.Empty);
      }

      src = 0;
      foreach (IRawDetail detail in rawDetails.Where(o => o.IsIncluded))
      {
        AddToPolygons(context, src, detail, detail.Quantity, progressDisplayer, detail.IsIncluded, false, true, detail.StrictAngle);
        src++;
      }
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
        var quantity = item.Quantity * (item.IsMultiplied ? SvgNest.Config.Multiplier : 1);
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
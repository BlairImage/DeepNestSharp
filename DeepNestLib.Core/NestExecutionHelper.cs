namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using DeepNestLib.IO;
  using DeepNestLib.NestProject;

  public class NestExecutionHelper
  {
    public void InitialiseNest(NestingContext context, IList<ISheetLoadInfo> sheetLoadInfos, IList<IRawDetail> rawDetails, IProgressDisplayer progressDisplayer)
    {
      progressDisplayer.IsVisibleSecondaryProgressBar = false;
      context.Reset();
      int src = 0;
      foreach (var item in sheetLoadInfos)
      {
        src = context.GetNextSheetSource();
        for (int i = 0; i < item.Quantity; i++)
        {
          var ns = Sheet.NewSheet(context.Sheets.Count + 1, item.Width, item.Height);
          context.Sheets.Add(ns);
          ns.Source = src;
        }
      }

      context.ReorderSheets();
      src = 0;
      foreach (var detail in rawDetails.Where(o => o.IsIncluded))
      {
        AddToPolygons(context, src, detail, 1, progressDisplayer, detail.IsIncluded, false, true, AnglesEnum.Rotate90); // todo the last few parameters are hardcoded and temporary
        src++;
      }

      progressDisplayer.DisplayTransientMessage(string.Empty);
    }

    public void AddToPolygons(NestingContext context, int src, IRawDetail det, int quantity, IProgressDisplayer progressDisplayer, bool isIncluded = true, bool isPriority = false, bool isMultiplied = false, AnglesEnum strictAngles = AnglesEnum.AsPreviewed)
    {
      var item = new DetailLoadInfo() { Quantity = quantity, IsIncluded = isIncluded, IsPriority = isPriority, IsMultiplied = isMultiplied, StrictAngle = strictAngles };
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
        for (int i = 0; i < quantity; i++)
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

namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;
  using System.Xml.Linq;
  using IO;
  using Placement;

  public partial class NestingContext : INestingContext, INotifyPropertyChanged
  {
    private readonly ISvgNestConfig config;
    private readonly IProgressDisplayer progressDisplayer;
    private readonly NestState state;
    private readonly INestStateBackground stateBackground;
    private readonly INestStateNestingContext stateNestingContext;
    private volatile bool isStopped;
    private volatile SvgNest nest;

    public NestingContext(IMessageService messageService, IDispatcherService dispatcherService, IProgressDisplayer progressDisplayer, NestState state, ISvgNestConfig config)
    {
      MessageService = messageService;
      DispatcherService = dispatcherService;
      this.progressDisplayer = progressDisplayer;
      State = state;
      this.config = config;
      this.state = state;
      stateNestingContext = state;
      stateBackground = state;
    }

    public IMessageService MessageService { get; }
    public IDispatcherService DispatcherService { get; }

    public INestState State { get; }

    public ICollection<INfp> Polygons { get; } = new HashSet<INfp>();

    public IList<ISheet> Sheets { get; } = new List<ISheet>();

    public INestResult Current { get; private set; }

    public SvgNest Nest
    {
      get => nest;
      private set => nest = value;
    }

    /// <summary>
    ///   Reinitializes the context and start a new nest.
    /// </summary>
    /// <returns>awaitable Task.</returns>
    public async Task StartNest()
    {
      // progressDisplayer.DisplayTransientMessage("Pre-processing. . .");
      ReorderSheets();
      InternalReset();
      Current = null;

      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) nestItems = await Task.Run(() => { return SvgNestInitializer.BuildNestItems(config, Polygons, Sheets, progressDisplayer); }).ConfigureAwait(false);

      Nest = new SvgNest(MessageService, progressDisplayer, state, config, nestItems);
      isStopped = false;
    }

    public void ResumeNest()
    {
      isStopped = false;
    }

    public async Task NestIterate(ISvgNestConfig config)
    {
      try
      {
        if (!isStopped)
        {
          if (Nest.IsStopped)
          {
            StopNest();
          }
          else
          {
            Nest.LaunchWorkers(config, stateBackground);
          }
        }

        if (state.TopNestResults != null && State.TopNestResults.Count > 0)
        {
          INestResult plcpr = State.TopNestResults.Top;

          if (Current == null || plcpr.Fitness < Current.Fitness)
          {
            AssignPlacement(plcpr);
          }
        }

        stateNestingContext.IncrementIterations();
      }
      catch (Exception ex)
      {
        if (!State.IsErrored)
        {
          state.SetIsErrored();
          MessageService.DisplayMessage(ex);
        }

#if NCRUNCH
        throw;
#endif
      }
    }

    public void AssignPlacement(INestResult plcpr)
    {
      Current = plcpr;

      List<INfp> placed = new();
      foreach (INfp item in Polygons)
      {
        item.Sheet = null;
      }

      List<int> sheetsIds = new();

      foreach (ISheetPlacement sheetPlacement in plcpr.UsedSheets)
      {
        var sheetid = sheetPlacement.SheetId;
        if (!sheetsIds.Contains(sheetid))
        {
          sheetsIds.Add(sheetid);
        }

        ISheet sheet = Sheets.First(z => z.Id == sheetid);

        foreach (IPartPlacement partPlacement in sheetPlacement.PartPlacements)
        {
          INfp poly = Polygons.First(z => z.Id == partPlacement.Id);
          placed.Add(poly);
          poly.Sheet = sheet;
          poly.X = partPlacement.X + sheet.X;
          poly.Y = partPlacement.Y + sheet.Y;
          poly.PlacementOrder = sheetPlacement.PartPlacements.IndexOf(partPlacement);
        }
      }

      IEnumerable<INfp> ppps = Polygons.Where(z => !placed.Contains(z));
      foreach (INfp item in ppps)
      {
        item.X = -1000;
        item.Y = 0;
      }
    }

    public void ReorderSheets()
    {
      double x = 0;
      double y = 0;
      var gap = 10;
      for (var i = 0; i < Sheets.Count; i++)
      {
        Sheets[i].X = x;
        Sheets[i].Y = y;
        if (Sheets[i] is Sheet sheet)
        {
          x += sheet.WidthCalculated + gap;
        }
        else
        {
          var maxx = Sheets[i].Points.Max(z => z.X);
          var minx = Sheets[i].Points.Min(z => z.X);
          var w = maxx - minx;
          x += w + gap;
        }
      }
    }

    public void LoadSampleData()
    {
      Console.WriteLine("Adding sheets..");
      for (var i = 0; i < 5; i++)
      {
        AddSheet(3000, 1500, 0);
      }

      Console.WriteLine("Adding parts..");
      var src1 = GetNextSource();
      for (var i = 0; i < 200; i++)
      {
        AddRectanglePart(src1, 250, 220);
      }
    }

    public int GetNextSource()
    {
      if (Polygons.Any())
      {
        return Polygons.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public int GetNextSheetSource()
    {
      if (Sheets.Any())
      {
        return Sheets.Max(z => z.Source) + 1;
      }

      return 0;
    }

    public void AddRectanglePart(int src, int ww = 50, int hh = 80)
    {
      var xx = 0;
      var yy = 0;
      NoFitPolygon pl = new();

      Polygons.Add(pl);
      pl.Source = src;
      pl.AddPoint(new SvgPoint(xx, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy));
      pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
      pl.AddPoint(new SvgPoint(xx, yy + hh));
    }

    public void LoadXml(string v)
    {
      XDocument d = XDocument.Load(v);
      XElement f = d.Descendants().First();
      var gap = int.Parse(f.Attribute("gap").Value);
      SvgNest.Config.Spacing = gap;

      foreach (XElement item in d.Descendants("sheet"))
      {
        var src = GetNextSheetSource();
        var cnt = int.Parse(item.Attribute("count").Value);
        var ww = int.Parse(item.Attribute("width").Value);
        var hh = int.Parse(item.Attribute("height").Value);

        for (var i = 0; i < cnt; i++)
        {
          AddSheet(ww, hh, src);
        }
      }

      foreach (XElement item in d.Descendants("part"))
      {
        var cnt = int.Parse(item.Attribute("count").Value);
        var path = item.Attribute("path").Value;
        IRawDetail r = null;
        if (path.ToLower().EndsWith("svg"))
        {
          r = SvgParser.LoadSvg(path);
        }
        else if (path.ToLower().EndsWith("dxf"))
        {
          // r = DxfParser.LoadDxfFile(path).Result;
        }
        else
        {
          continue;
        }

        var src = GetNextSource();

        for (var i = 0; i < cnt; i++)
        {
          INfp loadedNfp;
          if (r.TryConvertToNfp(src, out loadedNfp))
          {
            Polygons.Add(loadedNfp);
          }
        }
      }
    }

    /// <summary>
    ///   A full reset of the Context and all internals; Polygons and Sheets will need to be reinitialized.
    ///   Caches remain intact.
    /// </summary>
    public void Reset()
    {
      Polygons.Clear();
      Sheets.Clear();
      InternalReset();
    }

    public void StopNest()
    {
      Debug.Print("NestingContext.StopNest()");
      isStopped = true;
      Nest?.Stop();
    }

    private void AddSheet(int width, int height, int src)
    {
      RectangleSheet tt = new();
      tt.Name = "sheet" + (Sheets.Count + 1);
      Sheets.Add(tt);
      tt.Source = src;
      tt.Build(width, height);
      ReorderSheets();
    }

    /// <summary>
    ///   An internal reset to facilitate restarting the nest only; won't clear down the Polygons or Sheets.
    /// </summary>
    private void InternalReset()
    {
      stateNestingContext.Reset();
      Current = null;
      Current = null;
    }
  }
}
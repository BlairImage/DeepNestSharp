namespace DeepNestLib
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Threading.Tasks;
  using ClipperLib;
  using GeneticAlgorithm;
  using Geometry;
  using Placement;
#if NCRUNCH
  using System.Diagnostics;
#endif

  public class SvgNest
  {
    private readonly IMessageService messageService;
    private readonly IMinkowskiSumService minkowskiSumService;
    private readonly Procreant procreant;

    private readonly IProgressDisplayer progressDisplayer;

    private volatile bool isStopped;

    public SvgNest(IMessageService messageService, IProgressDisplayer progressDisplayer, NestState nestState, ISvgNestConfig config,
      (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) nestItems)
    {
      State = nestState;
      this.messageService = messageService;
      this.progressDisplayer = progressDisplayer;
      minkowskiSumService = MinkowskiSum.CreateInstance(config, nestState);
      NestItems = nestItems;
      procreant = new Procreant(NestItems.PartsLocal, config, progressDisplayer);
    }

    public static ISvgNestConfig Config { get; set; }

    public bool IsStopped
    {
      get => isStopped;
      private set => isStopped = value;
    }

    private (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) NestItems { get; }

    private INestStateSvgNest State { get; }

    internal static INfp CleanPolygon2(INfp polygon)
    {
      List<IntPoint> p = SvgToClipper(polygon);

      // remove self-intersections and find the biggest polygon that's left
      List<List<IntPoint>> simple = Clipper.SimplifyPolygon(p, PolyFillType.pftNonZero);

      if (simple == null || simple.Count == 0)
      {
        return null;
      }

      List<IntPoint> biggest = simple[0];
      var biggestarea = Math.Abs(Clipper.Area(biggest));
      for (var i = 1; i < simple.Count; i++)
      {
        var area = Math.Abs(Clipper.Area(simple[i]));
        if (area > biggestarea)
        {
          biggest = simple[i];
          biggestarea = area;
        }
      }

      // clean up singularities, coincident points and edges
      List<IntPoint> clean = Clipper.CleanPolygon(biggest, 0.01 * Config.CurveTolerance * Config.ClipperScale);

      if (clean == null || clean.Count == 0)
      {
        return null;
      }

      NoFitPolygon cleaned = ClipperToSvg(clean);

      // remove duplicate endpoints
      SvgPoint start = cleaned[0];
      SvgPoint end = cleaned[cleaned.Length - 1];
      if (start == end || (GeometryUtil.AlmostEqual(start.X, end.X) && GeometryUtil.AlmostEqual(start.Y, end.Y)))
      {
        cleaned.ReplacePoints(cleaned.Points.Take(cleaned.Points.Count() - 1));
      }

      if (polygon.IsClosed)
      {
        cleaned.EnsureIsClosed();
      }

      return cleaned;
    }

    // offset tree recursively
    internal static void OffsetTree(ref INfp t, double offset, ISvgNestConfig config, bool? inside = null)
    {
      INfp simple = NfpSimplifier.SimplifyFunction(t, inside == null ? false : inside.Value, config);
      INfp[] offsetpaths = { simple };
      if (Math.Abs(offset) > 0)
      {
        offsetpaths = PolygonOffsetDeepNest(simple, offset);
      }

      if (offsetpaths.Count() > 0)
      {
        List<SvgPoint> rett = new();
        rett.AddRange(offsetpaths[0].Points);
        rett.AddRange(t.Points.Skip(t.Length));
        t.ReplacePoints(rett);

        // replace array items in place

        // Array.prototype.splice.apply(t, [0, t.length].concat(offsetpaths[0]));
      }

      if (simple.Children != null && simple.Children.Count > 0)
      {
        if (t.Children == null)
        {
          t.Children = new List<INfp>();
        }

        for (var i = 0; i < simple.Children.Count; i++)
        {
          t.Children.Add(simple.Children[i]);
        }
      }

      if (t.Children != null && t.Children.Count > 0)
      {
        for (var i = 0; i < t.Children.Count; i++)
        {
          INfp child = t.Children[i];
          OffsetTree(ref child, -offset, config, inside == null ? true : !inside);
        }
      }
    }

    // use the clipper library to return an offset to the given polygon. Positive offset expands the polygon, negative contracts
    // note that this returns an array of polygons
    internal static INfp[] PolygonOffsetDeepNest(INfp polygon, double offset)
    {
      if (offset == 0 || GeometryUtil.AlmostEqual(offset, 0))
      {
        return new[] { polygon };
      }

      List<IntPoint> p = SvgToClipper(polygon);

      var miterLimit = 4;
      ClipperOffset co = new(miterLimit, Config.CurveTolerance * Config.ClipperScale);
      co.AddPath(p.ToList(), JoinType.jtMiter, EndType.etClosedPolygon);

      List<List<IntPoint>> newpaths = new();
      co.Execute(ref newpaths, offset * Config.ClipperScale);

      List<NoFitPolygon> result = new();
      for (var i = 0; i < newpaths.Count; i++)
      {
        result.Add(ClipperToSvg(newpaths[i]));
      }

      return result.ToArray();
    }

    internal static NoFitPolygon ClipperToSvg(IList<IntPoint> polygon)
    {
      List<SvgPoint> ret = new();

      for (var i = 0; i < polygon.Count; i++)
      {
        ret.Add(new SvgPoint(polygon[i].X / Config.ClipperScale, polygon[i].Y / Config.ClipperScale));
      }

      return new NoFitPolygon(ret);
    }

    internal void Stop()
    {
      Debug.Print("SvgNest.Stop()");
      IsStopped = true;
    }

    internal void ResponseProcessor(NestResult payload)
    {
      if (procreant == null || payload == null)
      {
        // user might have quit while we're away
        return;
      }

      State.IncrementPopulation();
      State.SetLastNestTime(payload.BackgroundTime);
      State.SetLastPlacementTime(payload.PlacePartTime);
      State.IncrementNestCount();
      State.IncrementPlacementTime(payload.PlacePartTime);
      State.IncrementNestTime(payload.BackgroundTime);

#if NCRUNCH
        Trace.WriteLine("payload.Index I don't think is being set right; double check before retrying threaded execution.");
#endif
      procreant.Population[payload.Index].Processing = false;
      procreant.Population[payload.Index].Fitness = payload.Fitness;

      //int currentPlacements = 0;
      var suffix = string.Empty;
      if (!payload.IsValid || payload.UsedSheets.Count == 0)
      {
        State.IncrementRejected();
        suffix = " Rejected";
      }
      else
      {
        TryAddResult result = State.TopNestResults.TryAdd(payload);
        if (result == TryAddResult.Added)
        {
          if (State.TopNestResults.IndexOf(payload) < State.TopNestResults.EliteSurvivors)
          {
            suffix = "Elite";
          }
          else
          {
            suffix = "Top";
          }
        }
        else
        {
          if (result == TryAddResult.Duplicate)
          {
            suffix = "Duplicate";
          }
          else
          {
            suffix = "Sub-optimal";
          }
        }

        if (State.TopNestResults.Top.TotalPlacedCount > 0)
        {
          progressDisplayer.DisplayProgress(State, State.TopNestResults.Top);
        }

        Debug.Print($"Nest {payload.BackgroundTime}ms {suffix}");
      }
    }

    /// <summary>
    ///   Starts next generation if none started or prior finished. Will keep rehitting the outstanding population
    ///   set up for the generation until all have processed.
    /// </summary>
    internal void LaunchWorkers(ISvgNestConfig config, INestStateBackground nestStateBackground)
    {
      try
      {
        if (!IsStopped)
        {
          if (procreant.IsCurrentGenerationFinished)
          {
            InitialiseAnotherGeneration();
          }

          List<ISheet> sheets = new();
          var sid = 0;
          for (var i = 0; i < NestItems.SheetsLocal.Count(); i++)
          {
            ISheet poly = NestItems.SheetsLocal[i].Polygon;
            for (var j = 0; j < NestItems.SheetsLocal[i].Quantity; j++)
            {
              ISheet clone;
              if (poly is Sheet sheet)
              {
                clone = (ISheet)poly.CloneTree();
              }
              else
              {
#if DEBUG || NCRUNCH
                throw new InvalidOperationException("Sheet should have been a sheet; why wasn't it?");
#endif
                clone = new Sheet(poly.CloneTree(), WithChildren.Excluded);
              }

              clone.Id = sid; // id is the unique id of all parts that will be nested, including cloned duplicates
              clone.Source = poly.Source; // source is the id of each unique part from the main part list
              clone.Children = poly.Children.ToList();

              sheets.Add(new Sheet(clone, WithChildren.Included));
              sid++;
            }
          }

          // progressDisplayer.DisplayTransientMessage("Executing Nest. . .");
          if (config.UseParallel)
          {
            var end1 = procreant.Population.Length / 3;
            var end2 = procreant.Population.Length * 2 / 3;
            var end3 = procreant.Population.Length;
            Parallel.Invoke(() => ProcessPopulation(0, end1, config, sheets.ToArray(), nestStateBackground), () => ProcessPopulation(end1, end2, config, sheets.ToArray(), nestStateBackground),
              () => ProcessPopulation(end2, procreant.Population.Length, config, sheets.ToArray(), nestStateBackground));
          }
          else
          {
            ProcessPopulation(0, procreant.Population.Length, config, sheets.ToArray(), nestStateBackground);
          }
        }
      }
      catch (DllNotFoundException)
      {
        DisplayMinkowskiDllError();
        State.SetIsErrored();
      }
      catch (BadImageFormatException badImageEx)
      {
        if (badImageEx.StackTrace.Contains("Minkowski"))
        {
          DisplayMinkowskiDllError();
        }
        else
        {
          messageService.DisplayMessage(badImageEx);
        }

        State.SetIsErrored();
      }
      catch (Exception ex)
      {
        messageService.DisplayMessage(ex);
        State.SetIsErrored();
#if NCRUNCH
        throw;
#endif
      }
    }

    // converts a polygon from normal double coordinates to integer coordinates used by clipper, as well as x/y -> X/Y
    private static List<IntPoint> SvgToClipper(INfp polygon)
    {
      List<IntPoint> d = DeepNestClipper.ScaleUpPath(polygon.Points, Config.ClipperScale);
      return d;
    }

    private int ToTree(PolygonTreeItem[] list, int idstart = 0)
    {
      List<PolygonTreeItem> parents = new();
      int i, j;

      // assign a unique id to each leaf
      // var id = idstart || 0;
      var id = idstart;

      for (i = 0; i < list.Length; i++)
      {
        PolygonTreeItem p = list[i];

        var ischild = false;
        for (j = 0; j < list.Length; j++)
        {
          if (j == i)
          {
            continue;
          }

          if (GeometryUtil.PointInPolygon(p.Polygon.Points[0], list[j].Polygon).Value)
          {
            if (list[j].Childs == null)
            {
              list[j].Childs = new List<PolygonTreeItem>();
            }

            list[j].Childs.Add(p);
            p.Parent = list[j];
            ischild = true;
            break;
          }
        }

        if (!ischild)
        {
          parents.Add(p);
        }
      }

      for (i = 0; i < list.Length; i++)
      {
        if (parents.IndexOf(list[i]) < 0)
        {
          list = list.Skip(i).Take(1).ToArray();
          i--;
        }
      }

      for (i = 0; i < parents.Count; i++)
      {
        parents[i].Polygon.Id = id;
        id++;
      }

      for (i = 0; i < parents.Count; i++)
      {
        if (parents[i].Childs != null)
        {
          id = ToTree(parents[i].Childs.ToArray(), id);
        }
      }

      return id;
    }

    /// <summary>
    ///   All individuals have been evaluated, start next generation
    /// </summary>
    private void InitialiseAnotherGeneration()
    {
      procreant.Generate();
#if !NCRUNCH
      if (procreant.Population.Length == 0)
      {
        Stop();
        messageService.DisplayMessageBox("Terminating the nest because we're just recalculating the same nests over and over again.", "Terminating Nest", MessageBoxIcon.Information);
      }
#endif

      State.IncrementGenerations();
      State.ResetPopulation();
    }

    private void DisplayMinkowskiDllError()
    {
      messageService.DisplayMessageBox(
        "An error has occurred attempting to execute the C++ Minkowski DllImport.\r\n" + "\r\n" + "You can turn off the C++ DllImport in Settings and use the internal C# implementation " +
        "instead; but this is experimental. Alternatively try using another build (x86/x64) or " + "recreate the Minkowski.Dlls on your own system.\r\n" + "\r\n" +
        "You can continue to use the import/edit/export functionality but unless you fix " + "the problem/switch to the internal implementation you will be unable to execute " + "any nests.",
        "DeepNestSharp Error!", MessageBoxIcon.Error);
    }

    private void ProcessPopulation(int start, int end, ISvgNestConfig config, ISheet[] sheets, INestStateBackground nestStateBackground)
    {
      State.IncrementThreads();
      for (var i = start; i < end; i++)
      {
        if (IsStopped)
        {
          break;
        }

        // if(running < config.threads && !GA.population[i].processing && !GA.population[i].fitness){
        // only one background window now...
        PopulationItem individual = procreant.Population[i];
        if (!IsStopped && individual.IsPending)
        {
          individual.Processing = true;
          if (IsStopped)
          {
            ResponseProcessor(null);
          }
          else
          {
            Background background = new(progressDisplayer, this, minkowskiSumService, nestStateBackground, config.UseDllImport);
            background.BackgroundStart(individual, sheets, config);
          }
        }
      }

      State.DecrementThreads();
    }
  }
}
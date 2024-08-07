﻿namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading.Tasks;

  public class SvgNestInitializer
  {
    public static (NestItem<INfp>[] PartsLocal, List<NestItem<ISheet>> SheetsLocal) BuildNestItems(ISvgNestConfig config, ICollection<INfp> parts, IList<ISheet> sheets,
      IProgressDisplayer progressDisplayer)
    {
      List<NestItem<INfp>> partsLocal;
      List<NestItem<ISheet>> sheetsLocal;
      SetPolygonIds(parts);
      SetSheetIds(sheets);
      IList<INfp> clonedPolygons = ClonePolygons(parts);
      List<ISheet> clonedSheets = CloneSheets(sheets);

      if (config.OffsetTreePhase)
      {
        ExecuteOffsetTreePhase(config, clonedPolygons, clonedSheets, progressDisplayer).Wait();
      }

      partsLocal = GroupToNestItemList(clonedPolygons);
      sheetsLocal = GroupToNestItemList(clonedSheets);
      SetIncrementingSource(partsLocal, sheetsLocal);

      return (partsLocal.ToArray(), sheetsLocal);
    }

    /// <summary>
    ///   Set both Parts and Sheet Source to a single incrementing zero based index.
    /// </summary>
    /// <param name="partsLocal">NestItem list of parts to index.</param>
    /// <param name="sheetsLocal">NestItem list of sheets to index.</param>
    private static void SetIncrementingSource(IEnumerable<NestItem<INfp>> partsLocal, IEnumerable<NestItem<ISheet>> sheetsLocal)
    {
      var srcc = 0;
      foreach (NestItem<INfp> item in partsLocal)
      {
        item.Polygon.Source = srcc++;
      }

      foreach (NestItem<ISheet> item in sheetsLocal)
      {
        item.Polygon.Source = srcc++;
      }
    }

    private static List<NestItem<ISheet>> GroupToNestItemList(List<ISheet> clonedSheets)
    {
      IEnumerable<NestItem<ISheet>> p2 = clonedSheets.GroupBy(z => z.Source).Select(z => new NestItem<ISheet>
      {
        Polygon = z.First(),
        Quantity = z.Count()
      });

      List<NestItem<ISheet>> sheetsLocal = new(p2);
      return sheetsLocal;
    }

    private static List<NestItem<INfp>> GroupToNestItemList(IList<INfp> clonedPolygons)
    {
      IEnumerable<NestItem<INfp>> p1 = clonedPolygons.GroupBy(z => z.Source).Select(z => new NestItem<INfp>
      {
        Polygon = z.First(),
        Quantity = z.Count()
      });
      List<NestItem<INfp>> partsLocal = new(p1);
      return partsLocal;
    }

    private static async Task ExecuteOffsetTreePhase(ISvgNestConfig config, IList<INfp> clonedPolygons, List<ISheet> clonedSheets, IProgressDisplayer progressDisplayer)
    {
      IGrouping<int, INfp>[] grps = clonedPolygons.GroupBy(z => z.Source).ToArray();
      if (config.UseParallel)
      {
        Parallel.ForEach(grps, async item =>
        {
          OffsetTreeReplace(config, item);
        });
      }
      else
      {
        var idx = 0;
        foreach (IGrouping<int, INfp> item in grps)
        {
          OffsetTreeReplace(config, item);
          idx++;
        }
      }

      foreach (ISheet item in clonedSheets)
      {
        var gap = config.SheetSpacing - config.Spacing / 2;
        INfp sheet = item;
        SvgNest.OffsetTree(ref sheet, -gap, config, true);
      }
    }

    /// <summary>
    ///   Clones the Sheets for use in the nest; the original Sheets won't get used again in the nest.
    /// </summary>
    /// <returns>A cloned set of sheets so the original won't get modified.</returns>
    private static List<ISheet> CloneSheets(IList<ISheet> sheets)
    {
      List<ISheet> lsheets = new();
      foreach (ISheet item in sheets)
      {
        Sheet clone = new()
        {
          Id = item.Id,
          UniqueId = item.UniqueId,
          Source = item.Source,
          Material = item.Material
        };
        clone.ReplacePoints(item.Points.Select(z => new SvgPoint(z.X, z.Y)
        {
          Exact = z.Exact
        }));
        if (item.Children != null)
        {
          foreach (INfp citem in item.Children)
          {
            clone.Children.Add(new NoFitPolygon());
            INfp l = clone.Children.Last();
            l.Id = citem.Id;
            l.Source = citem.Source;
            l.ReplacePoints(citem.Points.Select(z => new SvgPoint(z.X, z.Y)
            {
              Exact = z.Exact
            }));
          }
        }

        lsheets.Add(clone);
      }

      return lsheets;
    }

    /// <summary>
    ///   Clones the Polygons for use in the nest; the original Polygons won't get used again in the nest.
    /// </summary>
    /// <returns>A cloned set of polygons to nest so the original won't get modified.</returns>
    private static IList<INfp> ClonePolygons(ICollection<INfp> parts)
    {
      List<INfp> result = new();
      foreach (INfp item in parts)
      {
        INfp clone = item.CloneExact();
        result.Add(clone);
      }

      return result;
    }

    /// <summary>
    ///   Set each sheet Id to an incrementing zero based index.
    /// </summary>
    private static void SetSheetIds(IList<ISheet> sheets)
    {
      for (var i = 0; i < sheets.Count; i++)
      {
        sheets[i].Id = i;
      }
    }

    /// <summary>
    ///   Set each polygon Id to an incrementing zero based index.
    /// </summary>
    private static void SetPolygonIds(ICollection<INfp> parts)
    {
      var id = 0;
      foreach (INfp item in parts)
      {
        item.Id = id;
        id++;
      }
    }

    /// <summary>
    ///   This is the only point where simplification feeds in to the process so use this in tests to apply
    ///   config-simplifications to imports.
    ///   Don't see the reason to apply to all in the group because later we regroup and just use the first again.
    /// </summary>
    /// <param name="config">Config to use when simplifying.</param>
    /// <param name="item">The item that will be modified.</param>
    private static void OffsetTreeReplace(ISvgNestConfig config, IGrouping<int, INfp> item)
    {
      INfp target = item.First();
      SvgNest.OffsetTree(ref target, 0.5 * config.Spacing, config);
      foreach (INfp zitem in item)
      {
        zitem.ReplacePoints(item.First().Points);
      }
    }
  }
}
namespace DeepNestLib.IO
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;
  using IxMilia.Dxf;
  using IxMilia.Dxf.Entities;

  public class DxfParser
  {
    private const int NumberOfRetries = 5;
    private const int DelayOnRetry = 1000;
    private const double RemoveThreshold = 10e-5;
    private const double ClosingThreshold = 10e-2;
    private const double DirectionThreshold = 10e-2;

    private static volatile object loadLock = new();

    public static async Task<IRawDetail[]> LoadRawDetailFromEntities(IList<DxfEntity> entities)
    {
      for (var i = 1; i <= NumberOfRetries; ++i)
      {
        try
        {
          lock (loadLock)
          {
            RawDetail<DxfEntity>[] rawDetails = ConvertDxfToRawDetail(entities);

            // check if any entities are inside another entity, they are children or 'holes'
            for (var j = 0; j < rawDetails.Length; j++)
            {
              for (var k = 0; k < rawDetails.Length; k++)
              {
                try
                {
                  if (rawDetails[j] == null || rawDetails[k] == null || j == k || !rawDetails[j].BoundingBox().Contains(rawDetails[k].BoundingBox()))
                  {
                    continue;
                  }
                }
                catch (Exception ex)
                {
                  // ignore any exceptions
                }

                HashSet<DxfEntity> newHash = new()
                {
                  entities[k]
                };
                rawDetails[j].AddContour(new LocalContour<DxfEntity>(rawDetails[k].Outers.First().Points.ToList(), newHash)
                {
                  IsChild = true
                });

                // remove the child from the list of rawDetails
                rawDetails[k] = null;
              }
            }

            // remove any nulls from the list;
            return rawDetails.Where(o => o != null).ToArray();
          }
        }
        catch (Exception e) when (i <= NumberOfRetries)
        {
          await Task.Delay(DelayOnRetry);
        }
      }

      return default;
    }

    public static RawDetail<DxfEntity>[] ConvertDxfToRawDetail(IEnumerable<DxfEntity> entities)
    {
      List<RawDetail<DxfEntity>> s = new();
      Dictionary<DxfEntity, IList<LineElement>> approximations = ApproximateEntities(entities);

      foreach (KeyValuePair<DxfEntity, IList<LineElement>> appro in approximations)
      {
        DxfEntity entity = appro.Key;
        IList<LineElement> lines = appro.Value;
        var indexVal = approximations.Keys.ToList().IndexOf(entity);

        // Create a new RawDetail for each entity
        // each name needs to be unique
        var name = entity.EntityTypeString + indexVal;
        RawDetail<DxfEntity> rawDetail = new()
        {
          Name = name
        };
        Dictionary<DxfEntity, IList<LineElement>> tempDict = new();
        foreach (KeyValuePair<DxfEntity, IList<LineElement>> kvp in approximations)
        {
          if (kvp.Key == entity)
          {
            tempDict.Add(kvp.Key, kvp.Value);
          }
        }

        // im thinking I will have to only pass the approximations of the current appro entity
        LocalContour<DxfEntity>[] localContours = ConnectElements(tempDict);
        foreach (LocalContour<DxfEntity> localContour in localContours)
        {
          rawDetail.AddContour(localContour);
        }

        s.Add(rawDetail);
      }

      return s.ToArray();
    }

    private static Dictionary<DxfEntity, IList<LineElement>> ApproximateEntities(IEnumerable<DxfEntity> entities)
    {
      Dictionary<DxfEntity, IList<LineElement>> approximations = new();

      foreach (DxfEntity ent in entities)
      {
        List<LineElement> elems = new();
        switch (ent.EntityType)
        {
          case DxfEntityType.LwPolyline:
          {
            DxfLwPolyline poly = (DxfLwPolyline)ent;
            if (poly.Vertices.Count() < 2)
            {
              continue;
            }

            List<PointF> localContour = new();
            foreach (DxfLwPolylineVertex vert in poly.Vertices)
            {
              localContour.Add(new PointF((float)vert.X, (float)vert.Y));
            }

            elems.AddRange(ConnectTheDots(localContour).ToList());
          }

            break;
          case DxfEntityType.Arc:
          {
            DxfArc arc = (DxfArc)ent;
            List<PointF> pp = new();

            if (arc.StartAngle > arc.EndAngle)
            {
              arc.StartAngle -= 360;
            }

            for (var i = arc.StartAngle; i < arc.EndAngle; i += 5)
            {
              DxfPoint tt = arc.GetPointFromAngle(i);
              pp.Add(new PointF((float)tt.X, (float)tt.Y));
            }

            DxfPoint t = arc.GetPointFromAngle(arc.EndAngle);
            pp.Add(new PointF((float)t.X, (float)t.Y));
            for (var j = 1; j < pp.Count; j++)
            {
              PointF p1 = pp[j - 1];
              PointF p2 = pp[j];
              elems.Add(new LineElement
              {
                Start = new PointF(p1.X, p1.Y),
                End = new PointF(p2.X, p2.Y)
              });
            }
          }

            break;
          case DxfEntityType.Circle:
          {
            DxfCircle cr = (DxfCircle)ent;
            List<PointF> cc = new();

            for (var i = 0; i <= 360; i += 5)
            {
              var ang = i * Math.PI / 180f;
              var xx = cr.Center.X + cr.Radius * Math.Cos(ang);
              var yy = cr.Center.Y + cr.Radius * Math.Sin(ang);
              cc.Add(new PointF((float)xx, (float)yy));
            }

            elems.AddRange(ConnectTheDots(cc));
          }

            break;
          case DxfEntityType.Line:
          {
            DxfLine poly = (DxfLine)ent;
            elems.Add(new LineElement
            {
              Start = new PointF((float)poly.P1.X, (float)poly.P1.Y),
              End = new PointF((float)poly.P2.X, (float)poly.P2.Y)
            });
            break;
          }

          case DxfEntityType.Polyline:
          {
            DxfPolyline poly = (DxfPolyline)ent;
            if (poly.Vertices.Count() < 2)
            {
              continue;
            }

            List<PointF> localContour = new();
            foreach (DxfVertex vert in poly.Vertices)
            {
              localContour.Add(new PointF((float)vert.Location.X, (float)vert.Location.Y));
            }

            elems.AddRange(ConnectTheDots(localContour));

            break;
          }

          default:
            throw new ArgumentException("unsupported entity type: " + ent);
        }

        elems = elems.Where(z => z.Start.DistTo(z.End) > RemoveThreshold).ToList();
        approximations.Add(ent, elems);
      }

      return approximations;
    }

    internal static RawDetail<DxfEntity> LoadDxfFileStreamAsRawDetail(string path)
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadDxfStream(path, inputStream);
      }
    }

    internal static INfp LoadDxfFileStreamAsNfp(string path)
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadDxfStream(path, inputStream).ToNfp();
      }
    }

    internal static DxfFile LoadDxfFileStream(string path)
    {
      using (Stream inputStream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return DxfFile.Load(inputStream);
      }
    }

    internal static RawDetail<DxfEntity> LoadDxfStream(string name, Stream inputStream)
    {
      DxfFile dxffile = DxfFile.Load(inputStream);
      IEnumerable<DxfEntity> entities = dxffile.Entities.ToArray();
      return ConvertDxfToRawDetail(entities).First();
    }

    /// <summary>
    ///   Returns a series of LineElements to connect the points passed in.
    /// </summary>
    /// <param name="points">List of <see cref="PointF" /> to join.</param>
    /// <returns>List of <see cref="LineElement" /> connecting the dots.</returns>
    private static IEnumerable<LineElement> ConnectTheDots(IList<PointF> points)
    {
      for (var i = 0; i < points.Count; i++)
      {
        PointF p0 = points[i];
        PointF p1 = points[(i + 1) % points.Count];
        yield return new LineElement
        {
          Start = p0,
          End = p1
        };
      }
    }

    private static LocalContour<DxfEntity>[] ConnectElements(Dictionary<DxfEntity, IList<LineElement>> approximations)
    {
      List<(DxfEntity Entity, LineElement LineElement)> allLineElements = GetAllLineElements(approximations);

      PointF prior = default;
      List<PointF> newContourPoints = new();
      HashSet<DxfEntity> newContourEntities = new();
      List<LocalContour<DxfEntity>> result = new();
      while (allLineElements.Any())
      {
        if (newContourPoints.Count == 0)
        {
          LineElement toStart = allLineElements.First().LineElement;
          newContourPoints.Add(toStart.Start);
          prior = toStart.End;
          newContourPoints.Add(prior);
          newContourEntities.Add(allLineElements.First().Entity);
          allLineElements.RemoveAt(0);
        }
        else
        {
          if (!TryGetAnotherPoint(prior, allLineElements, out (DxfEntity Entity, LineElement LineElement) next))
          {
            result.Add(new LocalContour<DxfEntity>(newContourPoints.ToList(), newContourEntities));
            newContourPoints = new List<PointF>();
            newContourEntities = new HashSet<DxfEntity>();
          }
          else
          {
            allLineElements.Remove(next);
            newContourEntities.Add(next.Entity);
            prior = EndIsClosest(prior, next) ? next.LineElement.End : next.LineElement.Start;
            newContourPoints.Add(prior);
          }
        }
      }

      if (newContourPoints.Any())
      {
        result.Add(new LocalContour<DxfEntity>(newContourPoints.ToList(), newContourEntities));
      }

      // result.OrderByDescending(o => Math.Abs(GeometryUtil.PolygonArea(o.Points))).First().IsChild = false;
      return result.ToArray();
    }

    private static List<(DxfEntity Entity, LineElement LineElement)> GetAllLineElements(Dictionary<DxfEntity, IList<LineElement>> approximations)
    {
      List<(DxfEntity Entity, LineElement LineElement)> allLineElements = new();
      foreach (KeyValuePair<DxfEntity, IList<LineElement>> kvp in approximations)
      {
        allLineElements.AddRange(kvp.Value.Select(o => (kvp.Key, o)));
      }

      return allLineElements;
    }

    private static bool EndIsClosest(PointF prior, (DxfEntity Entity, LineElement LineElement) next)
    {
      return next.LineElement.Start.DistTo(prior) < next.LineElement.End.DistTo(prior);
    }

    private static bool TryGetAnotherPoint(PointF prior, List<(DxfEntity Entity, LineElement LineElement)> allLineElements, out (DxfEntity Entity, LineElement LineElement) next)
    {
      // ((DxfEntity Entity, LineElement LineElement) candidate, double, double) match = allLineElements.Select(candidate => (candidate, MinDistance(prior, candidate), DirectionDifference(prior, candidate))).Where(o => o.Item2 <= ClosingThreshold && o.Item3 <= DirectionThreshold).OrderBy(o => o.Item2)
      //   .FirstOrDefault();
      ((DxfEntity Entity, LineElement LineElement) candidate, double) match = allLineElements.Select(candidate => (candidate, MinDistance(prior, candidate))).Where(o => o.Item2 <= ClosingThreshold).OrderBy(o => o.Item2)
        .FirstOrDefault();
      if (match != default)
      {
        next = match.candidate;
        return true;
      }

      next = default;
      return false;
    }

    private static double DirectionDifference(PointF prior, (DxfEntity Entity, LineElement LineElement) candidate)
    {
      // Calc the direction of the current line element
      double currentDir = Math.Atan2(prior.Y - candidate.LineElement.Start.Y, prior.X - candidate.LineElement.Start.X);
      
      // Calc the direction of the potential next line element
      double nextDirStart = Math.Atan2(candidate.LineElement.Start.Y - candidate.LineElement.End.Y, candidate.LineElement.Start.X - candidate.LineElement.End.X);
      double nextDirEnd = Math.Atan2(candidate.LineElement.End.Y - candidate.LineElement.Start.Y, candidate.LineElement.End.X - candidate.LineElement.Start.X);
      
      // calc the difference between the two directions
      double directionDiffStart = Math.Abs(currentDir - nextDirStart);
      double directionDiffEnd = Math.Abs(currentDir - nextDirEnd);
      
      // return the smallest difference
      return Math.Min(directionDiffStart, directionDiffEnd);
    }

    private static double MinDistance(PointF prior, (DxfEntity Entity, LineElement LineElement) candidate)
    {
      return Math.Min(candidate.LineElement.Start.DistTo(prior), candidate.LineElement.End.DistTo(prior));
    }
  }
}
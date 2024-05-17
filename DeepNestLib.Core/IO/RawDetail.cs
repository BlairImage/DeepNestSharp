namespace DeepNestLib.IO
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;
  using GeneticAlgorithm;
  using NestProject;

  public class RawDetail<TSourceEntity> : IRawDetail
  {
    public List<ILocalContour> Outers { get; set; } = new();

    public bool IsIncluded { get; set; } = true;

    public AnglesEnum StrictAngle { get; set; } = AnglesEnum.AsPreviewed;

    public int Quantity { get; set; } = 1;

    public string Name { get; set; }

    public bool TryConvertToNfp(int src, out INfp loadedNfp)
    {
      if (this == null)
      {
        loadedNfp = null;
        return false;
      }

      loadedNfp = ToNfp();
      if (loadedNfp == null)
      {
        return false;
      }

      loadedNfp.Source = src;
      return true;
    }

    public bool TryConvertToNfp(int src, out Chromosome loadedChromosome)
    {
      INfp loadedNfp;
      var result = TryConvertToNfp(src, out loadedNfp);
      loadedChromosome = new Chromosome(loadedNfp, loadedNfp?.Rotation ?? 0);
      return result;
    }

    bool IRawDetail.TryConvertToNfp(int src, int rotation, out Chromosome loadedChromosome)
    {
      var result = TryConvertToNfp(src, out loadedChromosome);
      loadedChromosome.Rotation = rotation;
      return result;
    }

    public INfp ToNfp()
    {
      NoFitPolygon result = null;
      List<NoFitPolygon> nfps = new();
      foreach (ILocalContour item in Outers)
      {
        NoFitPolygon nn = new();
        nfps.Add(nn);
        foreach (PointF pitem in item.Points)
        {
          nn.AddPoint(new SvgPoint(pitem.X, pitem.Y));
        }
      }

      if (nfps.Any())
      {
        NoFitPolygon parent = nfps.OrderByDescending(z => z.Area).First();
        result = parent; // Reference caution needed here; should be cloning not messing with the original object?
        result.Name = Name;

        foreach (NoFitPolygon child in nfps.Where(o => o != parent))
        {
          if (result.Children == null)
          {
            result.Children = new List<INfp>();
          }

          result.Children.Add(child);
        }
      }

      return result;
    }

    public ISheet ToSheet()
    {
      return new Sheet(ToNfp(), WithChildren.Excluded);
    }

    bool IRawDetail.TryConvertToSheet(int firstSheetIdSrc, out ISheet firstSheet)
    {
      INfp nfp;
      if (TryConvertToNfp(firstSheetIdSrc, out nfp))
      {
        firstSheet = new Sheet(nfp, WithChildren.Excluded);
        return true;
      }

      firstSheet = default;
      return false;
    }

    public bool IsCircle()
    {
      // calc the centroid of the points
      PointF centroid = new(Outers.First().Points.Average(z => z.X), Outers.First().Points.Average(z => z.Y));

      // calc the distances from the centroid to each point
      List<double> distances = Outers.First().Points.Select(z => Math.Sqrt(Math.Pow(z.X - centroid.X, 2) + Math.Pow(z.Y - centroid.Y, 2))).ToList();

      // calc the average distance
      var avgDistance = distances.Average();

      // check if all distances are within a tolerance of the average distance
      var tolerance = avgDistance * 0.05; // 5% tolerance
      return distances.All(d => Math.Abs(d - avgDistance) <= tolerance);
    }

    public bool IsRectangle()
    {
      if (Outers.Count != 1)
      {
        return false;
      }

      // the points of a rectangle are right angles to each other
      for (var i = 0; i < Outers.First().Points.Count; i++)
      {
        PointF p1 = Outers.First().Points[i];
        PointF p2 = Outers.First().Points[(i + 1) % Outers.First().Points.Count];
        PointF p3 = Outers.First().Points[(i + 2) % Outers.First().Points.Count];

        PointF v1 = new(p2.X - p1.X, p2.Y - p1.Y);
        PointF v2 = new(p3.X - p2.X, p3.Y - p2.Y);

        if (v1.X * v2.X + v1.Y * v2.Y != 0)
        {
          return false;
        }
      }

      return true;
    }

    public RectangleF BoundingBox()
    {
      GraphicsPath gp = new();
      foreach (ILocalContour item in Outers)
      {
        gp.AddPolygon(item.Points.ToArray());
      }

      return gp.GetBounds();
    }

    public void AddContour(LocalContour<TSourceEntity> contour)
    {
      Outers.Add(contour);
    }

    public void AddRangeContour(IEnumerable<LocalContour<TSourceEntity>> collection)
    {
      Outers.AddRange(collection);
    }

    public (INfp, double) ToChromosome()
    {
      INfp nfp = ToNfp();
      return (nfp, nfp.Rotation);
    }
  }
}
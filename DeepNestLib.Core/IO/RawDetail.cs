namespace DeepNestLib.IO
{
  using System.Collections.Generic;
  using System.Drawing;
  using System.Drawing.Drawing2D;
  using System.Linq;
  using GeneticAlgorithm;
  using NestProject;

  public class RawDetail<TSourceEntity> : IRawDetail
  {
    private readonly List<LocalContour<TSourceEntity>> outers = new();

    public IReadOnlyCollection<ILocalContour> Outers => outers;

    public bool IsIncluded { get; set; } = true;

    public AnglesEnum StrictAngle { get; set; } = AnglesEnum.AsPreviewed;

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
      outers.Add(contour);
    }

    public void AddRangeContour(IEnumerable<LocalContour<TSourceEntity>> collection)
    {
      outers.AddRange(collection);
    }

    public (INfp, double) ToChromosome()
    {
      INfp nfp = ToNfp();
      return (nfp, nfp.Rotation);
    }
  }
}
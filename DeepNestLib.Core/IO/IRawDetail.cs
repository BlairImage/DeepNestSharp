namespace DeepNestLib.IO
{
  using GeneticAlgorithm;
  using NestProject;
  using System.Collections.Generic;

  public interface IRawDetail
  {
    string Name { get; }

    List<ILocalContour> Outers { get; }

    bool IsIncluded { get; set; }

    AnglesEnum StrictAngle { get; set; }

    int Quantity { get; set; }

    bool TryConvertToNfp(int src, out INfp loadedNfp);

    bool TryConvertToNfp(int src, out Chromosome chromosome);

    bool TryConvertToSheet(int v, out ISheet firstSheet);

    INfp ToNfp();

    ISheet ToSheet();

    bool TryConvertToNfp(int firstPartIdSrc, int v, out Chromosome firstPart);

    bool IsCircle();

    bool IsRectangle();
  }
}
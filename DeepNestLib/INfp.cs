﻿namespace DeepNestLib
{
  using System.Collections.Generic;
  using DeepNestLib.NestProject;

  public interface INfp
  {
    double Area { get; }

    IList<INfp> Children { get; set; }

    bool Fitted { get; }

    double HeightCalculated { get; }

    int Id { get; set; }

    bool IsPriority { get; set; }

    int Length { get; }

    string Name { get; set; }

    double? Offsetx { get; set; }

    double? Offsety { get; set; }

    int PlacementOrder { get; set; }

    SvgPoint[] Points { get; }

    double Rotation { get; set; }

    INfp Sheet { get; set; }

    int Source { get; set; }

    AnglesEnum StrictAngle { get; set; }

    double WidthCalculated { get; }

    double X { get; set; }

    double Y { get; set; }

    SvgPoint this[int ind] { get; }

    void AddPoint(SvgPoint point);

    NFP Clone();

    NFP CloneExact();

    NFP CloneTree();

    INfp CloneTop();

    NFP GetHull();

    SvgPoint[] ReplacePoints(IEnumerable<SvgPoint> points);

    void Reverse();

    NFP Rotate(double degrees);

    INfp Slice(int v);

    string Stringify();

    string ToJson();

    string ToString();
  }
}
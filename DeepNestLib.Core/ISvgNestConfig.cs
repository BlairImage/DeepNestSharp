namespace DeepNestLib
{
  using Placement;

  public interface ISvgNestConfig : ITopNestResultsConfig, IPlacementConfig
  {
    /// <summary>
    ///   Gets or sets whether to clip the simplified polygon used in nesting by the hull.
    ///   This often improves the fit to the original part but may slightly increase the number
    ///   of points in the simplification and accordingly may marginally slow the nest.
    ///   Requires a restart of the application because it's not a part of the cache key so
    ///   you have to restart to reinitialise the cache.
    /// </summary>
    bool ClipByHull { get; set; }

    /// <summary>
    ///   Differentiate children when exporting.
    /// </summary>
    bool DifferentiateChildren { get; set; }

    bool ExploreConcave { get; set; }

    /// <summary>
    ///   Gets or sets the last path used for Nest files (dnest, dnr, dxf).
    /// </summary>
    string LastNestFilePath { get; set; }

    /// <summary>
    ///   Gets or sets the last path used for Debugging files (dnsp, dnsnfp, dnnfps).
    /// </summary>
    string LastDebugFilePath { get; set; }

    bool UseMinkowskiCache { get; set; }

    /// <summary>
    ///   Gets or sets the percentage chance that a gene will mutate during procreation. Set it too low and the nest could
    ///   stagnate. Set it too high and fittest gene sequences may not get inherited.
    /// </summary>
    int MutationRate { get; set; }

    bool OffsetTreePhase { get; set; }

    int SaveAsFileTypeIndex { get; set; }

    double SheetSpacing { get; set; }

    bool Simplify { get; set; }

    /// <summary>
    ///   Gets or sets the spacing to apply to sheet edges during the nest.
    ///   Rounding errors result in approx 1mm margin necessary even with 0 Spacing set.
    ///   If spacing set then tbc the full amount is taken off sheet width available area.
    ///   If spacing set then tbc half the amount is taken off sheet height available area.
    /// </summary>
    double Spacing { get; set; }

    /// <summary>
    ///   Gets or sets max bound for bezier->line segment conversion, in native SVG units.
    /// </summary>
    double Tolerance { get; set; }

    /// <summary>
    ///   Fudge factor for browser inaccuracy in SVG unit handling.
    /// </summary>

    double ToleranceSvg { get; set; }

    bool UseHoles { get; set; }

    bool UseParallel { get; set; }

    int Multiplier { get; set; }

    int ParallelNests { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating the Timeout for Procreation in milliseconds.
    /// </summary>
    int ProcreationTimeout { get; set; }

    bool ShowPartPositions { get; set; }

    /// <summary>
    ///   When selecting sheets, the packing efficiency to use.
    /// </summary>
    public double PackingEfficiencyMax { get; set; }

    public double PackingEfficiencyMin { get; set; }

    public double AutoPackingEfficiencyStep { get; set; }

    public bool StrictAngleOverride { get; set; }

    // string ToJson();
  }
}
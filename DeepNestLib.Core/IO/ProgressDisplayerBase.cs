namespace DeepNestLib.IO
{
  using System;
  using Placement;

  public abstract class ProgressDisplayerBase
  {
    private readonly Func<INestState> stateFactory;

    private double loopIndex;

    private double loopIndexSecondary;
    private double loopMax;
    private double loopMaxSecondary;
    private INestState state;

    protected ProgressDisplayerBase(Func<INestState> stateFactory) => this.stateFactory = stateFactory;

    protected INestState State => state ?? (state = stateFactory());

    /// <summary>
    ///   Calculates the percentage complete based on the given nest result.
    /// </summary>
    /// <param name="topNest">The nest result to calculate the percentage complete for.</param>
    /// <returns>The percentage complete.</returns>
    protected internal static double CalculatePercentageComplete(INestResult topNest)
    {
      var progressPopulation = 0.66f * topNest.MaterialUtilization;
      var progressPlacements = 0.34f * ((double)topNest.TotalPlacedCount / topNest.TotalPartsCount);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }
  }
}
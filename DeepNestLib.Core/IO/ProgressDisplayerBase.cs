namespace DeepNestLib.IO
{
  using System;
  using System.Threading.Tasks;
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

    protected internal static double CalculatePercentageComplete(int placedParts, int currentPopulation, int populationSize, int totalPartsToPlace)
    {
      var progressPopulation = 0.66f * ((double)currentPopulation / populationSize);
      var progressPlacements = 0.34f * ((double)placedParts / totalPartsToPlace);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }

    protected internal static double CalculatePercentageComplete(INestResult topNest)
    {
      var progressPopulation = 0.66f * topNest.MaterialUtilization;
      var progressPlacements = 0.34f * ((double)topNest.TotalPlacedCount / topNest.TotalPartsCount);
      var percentageComplete = progressPopulation + progressPlacements;
      return percentageComplete;
    }
  }
}
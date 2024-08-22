namespace DeepNestLib
{
  using System.ComponentModel;
  using Placement;

  public interface IProgressDisplayer : INotifyPropertyChanged
  {
    public delegate void TopNestUpdatedEventHandler(object sender, INestResult newTopNest, IMaterial material);

    double PrimaryProgressBarPercentage { get; set; }
    int Generation { get; set; }
    int Population { get; set; }
    int PlacedParts { get; set; }
    int TotalPartsToPlace { get; set; }
    double TotalPartsPercentage { get; }
    double BestFitness { get; set; }
    bool AllPartsPlaced { get; set; }
    string MaterialName { get; set; }
    INestResult TopNest { get; set; }

    event TopNestUpdatedEventHandler TopNestUpdated;

    void DisplayProgress(INestStateSvgNest state, INestResult topNest);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);
  }
}
namespace DeepNestLib
{
  using System.ComponentModel;
  using Placement;

  public interface IProgressDisplayer : INotifyPropertyChanged
  {
    double PrimaryProgressBarPercentage { get; set; }
    int Generation { get; set; }
    int Population { get; set; }
    int PlacedParts { get; set; }
    int TotalPartsToPlace { get; set; }
    double Fitness { get; set; }
    bool AllPartsPlaced { get; set; }
    string MaterialName { get; set; }

    void DisplayProgress(INestStateSvgNest state, INestResult topNest);

    void DisplayMessageBox(string text, string caption, MessageBoxIcon icon);
  }
}
﻿namespace DeepNestSharp.Ui.Models
{
  using DeepNestLib;

  public class ObservableHole : ObservableNfp
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableHole"/> class.
    /// When the parent's a PartPlacement then we need to refresh it's holes too.
    /// </summary>
    /// <param name="partPlacement"></param>
    /// <param name="child"></param>
    public ObservableHole(ObservablePartPlacement partPlacement, INfp child)
      : this(child)
    {
      partPlacement.RenderChildren += this.PartPlacement_RenderChildren;
    }

    internal ObservableHole(INfp child)
      : base(child)
    {
    }

    private void PartPlacement_RenderChildren(object? sender, System.EventArgs e)
    {
      OnPropertyChanged(nameof(Points));
      OnPropertyChanged(nameof(Y));
      OnPropertyChanged(nameof(X));
    }
  }
}
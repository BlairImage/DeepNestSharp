﻿namespace DeepNestSharp.Ui.Behaviors
{
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Interactivity;
  using System.Windows.Shapes;
  using DeepNestLib.Placement;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  public class PolygonMouseDrag : Behavior<FrameworkElement>
  {
    private MainViewModel? mainViewModel;
    private ObservablePartPlacement? partPlacement;

    protected override void OnAttached()
    {
      base.OnAttached();
      if (this.AssociatedObject.DataContext is ObservablePartPlacement partPlacement &&
          this.AssociatedObject.GetVisualParent<Window>() is Window window &&
          window.DataContext is MainViewModel mainViewModel)
      {
        this.mainViewModel = mainViewModel;
        this.partPlacement = partPlacement;
      }
    }
  }

  public class PolygonMouseHover : Behavior<FrameworkElement>
  {
    private MainViewModel? mainViewModel;
    private ObservablePartPlacement? partPlacement;

    protected override void OnAttached()
    {
      base.OnAttached();
      if (this.AssociatedObject.DataContext is ObservablePartPlacement partPlacement &&
          this.AssociatedObject.GetVisualParent<Window>() is Window window &&
          window.DataContext is MainViewModel mainViewModel)
      {
        this.mainViewModel = mainViewModel;
        this.partPlacement = partPlacement;
      }

      this.AssociatedObject.MouseEnter += this.AssociatedObject_MouseEnter;
      this.AssociatedObject.MouseLeave += this.AssociatedObject_MouseLeave;
    }

    private void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
    {
      if (sender is Polygon polygon &&
          this.AssociatedObject.GetVisualParent<Canvas>() is Canvas canvas)
      {
        canvas.Focus();
        if (this.mainViewModel != null)
        {
          mainViewModel.PreviewViewModel.HoverPartPlacement = this.partPlacement;
          System.Diagnostics.Debug.WriteLine($"Hover {this.partPlacement?.Id ?? -1}");
        }
      }
    }

    private void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
    {
      if (this.mainViewModel != null)
      {
        mainViewModel.PreviewViewModel.HoverPartPlacement = null;
      }
    }
  }
}
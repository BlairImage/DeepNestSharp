﻿namespace DeepNestSharp.Ui.UserControls
{
  using System.Reflection;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Input;
  using System.Windows.Shapes;
  using DeepNestSharp.Ui.Behaviors;
  using DeepNestSharp.Ui.Models;
  using DeepNestSharp.Ui.ViewModels;

  /// <summary>
  /// Interaction logic for Preview.xaml.
  /// </summary>
  public partial class Preview : UserControl
  {
    private Point partPlacementStartPos;
    private ObservablePartPlacement? capturePartPlacement;
    private Polygon? capturePolygon;

    public Preview()
    {
      InitializeComponent();
      this.Loaded += this.Preview_Loaded;
    }

    private static bool IsDragModifierPressed
    {
      get
      {
        return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
      }
    }

    private void Preview_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is PreviewViewModel viewModel &&
          sender is Preview preview &&
          preview.GetVisualParent<Window>() is Window window &&
          preview.GetChildOfType<Canvas>() is Canvas canvas &&
          preview.GetChildOfType<ScrollViewer>() is ScrollViewer scrollViewer)
      {
        viewModel.Canvas = canvas;
        scrollViewer.SizeChanged += this.ScrollViewer_SizeChanged;
        //scrollViewer.LayoutUpdated += this.ScrollViewer_LayoutUpdated;
        window.StateChanged += this.Window_StateChanged;
        SetViewport(viewModel, scrollViewer);
      }
    }

    private void Window_StateChanged(object? sender, System.EventArgs e)
    {
      if (this.DataContext is PreviewViewModel viewModel &&
          sender is Window window &&
          window.GetChildOfType<Preview>() is Preview preview &&
          preview.GetChildOfType<ScrollViewer>() is ScrollViewer scrollViewer)
      {
        switch (window.WindowState)
        {
          case WindowState.Maximized:
            SetViewport(viewModel, scrollViewer);
            break;
        }
      }
    }

    private void ScrollViewer_LayoutUpdated(object? sender, System.EventArgs e)
    {
      if (this.DataContext is PreviewViewModel viewModel &&
          sender is ScrollViewer scrollViewer)
      {
        SetViewport(viewModel, scrollViewer);
      }
    }

    private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this.DataContext is PreviewViewModel viewModel &&
          sender is ScrollViewer scrollViewer)
      {
        SetViewport(viewModel, scrollViewer);
      }
    }

    private static void SetViewport(PreviewViewModel viewModel, ScrollViewer scrollViewer)
    {
      viewModel.Viewport = new Point(scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);
      viewModel.Actual = new Point(scrollViewer.ActualWidth, scrollViewer.ActualHeight);
    }

    private void Polygon_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("Polygon_MouseUp");
      if (sender is Polygon polygon &&
          polygon.GetVisualParent<ItemsControl>() is ItemsControl itemsControl &&
          DataContext is PreviewViewModel vm)
      {
        MouseUpHandler(vm, itemsControl);
      }
    }

    private void Polygon_MouseDown(object sender, MouseButtonEventArgs e)
    {
      try
      {
        if (sender is Polygon polygon &&
            polygon.GetVisualParent<Grid>() is Grid grid &&
            DataContext is PreviewViewModel vm &&
            polygon.DataContext is ObservablePartPlacement partPlacement)
        {
          vm.SelectedPartPlacement = partPlacement;
          if (IsDragModifierPressed)
          {
            System.Diagnostics.Debug.WriteLine($"partPlacement:{partPlacement.Id}");
            vm.DragStart = e.GetPosition(grid);
            partPlacementStartPos = new Point(vm.SelectedPartPlacement.X, vm.SelectedPartPlacement.Y);
            System.Diagnostics.Debug.Print($"Drag start set@{vm.DragStart?.X},{vm.DragStart?.Y}. {vm.IsDragging}");
            capturePartPlacement = partPlacement;
            capturePolygon = polygon;
            capturePolygon.CaptureMouse();
            e.Handled = true;
          }
        }
      }
      catch (System.Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex);
        throw;
      }
    }

    private void ItemsControl_MouseMove(object sender, MouseEventArgs e)
    {
      if (sender is ItemsControl itemsControl &&
          itemsControl.GetChildOfType<Canvas>() is Canvas canvas &&
          DataContext is PreviewViewModel vm)
      {
        vm.MousePosition = e.GetPosition(itemsControl);
        vm.CanvasPosition = e.GetPosition(canvas);
        if (vm.IsDragging &&
            vm.DragStart != null &&
            capturePartPlacement != null)
        {
          if (IsDragModifierPressed)
          {
            var dragStart = vm.DragStart.Value;
            vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);

            // System.Diagnostics.Debug.Print($"DragOffset={vm.DragOffset:N2}");
            capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
            capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
          }
          else
          {
            System.Diagnostics.Debug.Print("Drag cancel MouseMove:IsDragModifierPressed.");
            capturePartPlacement.X = partPlacementStartPos.X;
            capturePartPlacement.Y = partPlacementStartPos.Y;
            capturePolygon?.ReleaseMouseCapture();
            vm.DragStart = null;
          }

          vm.RaiseDrawingContext();
          this.InvalidateArrange();
        }
      }
    }

    private void ItemsControl_MouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Diagnostics.Debug.Print("ItemsControl_MouseUp");
      if (sender is ItemsControl itemsControl &&
          DataContext is PreviewViewModel vm)
      {
        MouseUpHandler(vm, itemsControl);
      }
    }

    private void MouseUpHandler(PreviewViewModel vm, ItemsControl itemsControl)
    {
      System.Diagnostics.Debug.Print("Handle MouseUp");
      if (vm.IsDragging && IsDragModifierPressed && vm.DragStart.HasValue)
      {
        var dragStart = vm.DragStart.Value;
        vm.DragOffset = new Point((vm.MousePosition.X - dragStart.X) / vm.CanvasScale, (vm.MousePosition.Y - dragStart.Y) / vm.CanvasScale);
        System.Diagnostics.Debug.Print($"Drag commit@{vm.DragOffset.X:N2},{vm.DragOffset.Y:N2}");
        if (capturePartPlacement != null)
        {
          capturePartPlacement.X = partPlacementStartPos.X + vm.DragOffset.X;
          capturePartPlacement.Y = partPlacementStartPos.Y + vm.DragOffset.Y;
        }
      }

      capturePolygon?.ReleaseMouseCapture();
      vm.DragStart = null;
      vm.RaiseSelectItem();
      vm.RaiseDrawingContext();
      System.Diagnostics.Debug.Print("Force ItemsControl.UpdateTarget");
      itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget();
      itemsControl.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateSource();
      this.InvalidateVisual();
    }
  }
}
﻿namespace DeepNestSharp.Ui.Models
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using DeepNestLib;
  using DeepNestLib.GeneticAlgorithm;
  using DeepNestLib.Placement;
  using Microsoft.Toolkit.Mvvm.ComponentModel;

  public class ObservableSheetPlacement : ObservableObject, ISheetPlacement
  {
    private readonly ObservableCollection<ObservablePartPlacement> observablePartPlacements;
    private System.Windows.Media.PointCollection points;
    private ISheetPlacement? item;

    public ObservableSheetPlacement()
    {
      this.observablePartPlacements = new ObservableCollection<ObservablePartPlacement>();
    }

    public ObservableSheetPlacement(ISheetPlacement item)
      : this() => this.Set(item);

    public double X => this.Sheet.X;

    public double Y => this.Sheet.Y;

    public bool IsSet => this.item != null;

    private void Set(ISheetPlacement item)
    {
      this.item = item;
      this.observablePartPlacements.Clear();
      this.points?.Clear();
      foreach (var partPlacement in item.PartPlacements)
      {
        var obsPart = new ObservablePartPlacement(partPlacement);
        this.observablePartPlacements.Add(obsPart);
      }

      OnPropertyChanged(nameof(PartPlacements));
      OnPropertyChanged(nameof(IsSet));
      OnPropertyChanged(nameof(Sheet));
    }

    public OriginalFitnessSheet Fitness => item.Fitness;

    public INfp Hull => item.Hull;

    public double MaxX => this.item.MaxX;

    public double MaxY => this.item.MaxY;

    public double MaterialUtilization => item.MaterialUtilization;

    public double MinX => this.item.MinX;

    public double MinY => this.item.MinY;

    public IReadOnlyList<IPartPlacement> PartPlacements => this.observablePartPlacements;

    public PlacementTypeEnum PlacementType => item.PlacementType;

    public System.Windows.Media.PointCollection Points
    {
      get
      {
        if (points == null || points.Count == 0)
        {
          points = new System.Windows.Media.PointCollection();
          foreach (var p in this.Sheet.Points)
          {
            points.Add(new System.Windows.Point(p.X, p.Y));
          }
        }

        return points;
      }
    }

    public PolygonBounds RectBounds => item.RectBounds;

    public INfp Sheet => item?.Sheet;

    public int SheetId => item.SheetId;

    public int SheetSource => item.SheetSource;

    public INfp Simplify => item.Simplify;

    public double TotalPartsArea => item.TotalPartsArea;

    public string ToJson()
    {
      return this.item.ToJson();
    }
  }
}

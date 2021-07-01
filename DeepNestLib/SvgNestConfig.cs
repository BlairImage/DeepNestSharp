﻿namespace DeepNestLib
{
  public class SvgNestConfig : ISvgNestConfig
  {
    public double Scale { get; set; } = 25;

    public double ClipperScale { get; set; } = 10000000;

    public bool ExploreConcave { get; set; } = false;

    public int Rotations { get; set; } = 4;

    public double SheetSpacing { get; set; } = 0;

    public bool UseHoles { get; set; } = false;

    public double TimeRatio { get; set; } = 0.5;

    public bool MergeLines { get; set; } = false;

    /// <summary>
    /// Port feature; don't exist in the original DeepNest project.Disabled in the UI because it's not a part of the cache key so
    /// doesn't make sense to change during the litetime of the application.
    /// </summary>
    public bool ClipByHull
    {
      get
      {
        return (bool)Properties.Settings.Default["ClipByHull"];
      }

      set
      {
        Properties.Settings.Default["ClipByHull"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public double CurveTolerance
    {
      get
      {
        return (double)Properties.Settings.Default["CurveTolerance"];
      }

      set
      {
        Properties.Settings.Default["CurveTolerance"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SaveAsFileTypeIndex
    {
      get
      {
        return (int)Properties.Settings.Default["SaveAsFileTypeIndex"];
      }

      set
      {
        Properties.Settings.Default["SaveAsFileTypeIndex"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetWidth
    {
      get
      {
        return (int)Properties.Settings.Default["SheetWidth"];
      }

      set
      {
        Properties.Settings.Default["SheetWidth"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetHeight
    {
      get
      {
        return (int)Properties.Settings.Default["SheetHeight"];
      }

      set
      {
        Properties.Settings.Default["SheetHeight"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int SheetQuantity
    {
      get
      {
        return (int)Properties.Settings.Default["SheetQuantity"];
      }

      set
      {
        Properties.Settings.Default["SheetQuantity"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public PlacementTypeEnum PlacementType
    {
      get
      {
        return (PlacementTypeEnum)Properties.Settings.Default["PlacementType"];
      }

      set
      {
        Properties.Settings.Default["PlacementType"] = (int)value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool Simplify
    {
      get
      {
        return (bool)Properties.Settings.Default["Simplify"];
      }

      set
      {
        Properties.Settings.Default["Simplify"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool OffsetTreePhase
    {
      get
      {
        return (bool)Properties.Settings.Default["OffsetTreePhase"];
      }

      set
      {
        Properties.Settings.Default["OffsetTreePhase"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public double Spacing
    {
      get
      {
        return (double)Properties.Settings.Default["Spacing"];
      }

      set
      {
        Properties.Settings.Default["Spacing"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int PopulationSize
    {
      get
      {
        return (int)Properties.Settings.Default["PopulationSize"];
      }

      set
      {
        Properties.Settings.Default["PopulationSize"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public int MutationRate
    {
      get
      {
        return (int)Properties.Settings.Default["MutationRate"];
      }

      set
      {
        Properties.Settings.Default["MutationRate"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }

    public bool DrawSimplification
    {
      get
      {
        return (bool)Properties.Settings.Default["DrawSimplification"];
      }

      set
      {
        Properties.Settings.Default["DrawSimplification"] = value;
        Properties.Settings.Default.Save();
        Properties.Settings.Default.Upgrade();
      }
    }
  }
}

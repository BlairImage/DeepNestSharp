namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using NestProject;

  /// <summary>
  ///   Represents a material used in nesting.
  /// </summary>
  public interface IMaterial
  {
    /// <summary>
    ///   Gets or sets the unique identifier of the material.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///   Gets or sets the name of the material.
    /// </summary>
    public string Name { get; set; }

    public int PreviewColorInt { get; set; }

    /// <summary>
    ///   Gets or sets the brand of the material.
    /// </summary>
    public string Brand { get; set; }

    /// <summary>
    ///   Gets or sets the description of the material.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    ///   Gets or sets the cost of the material.
    /// </summary>
    public double Cost { get; set; }

    /// <summary>
    ///   Gets or sets the orientation of the material.
    /// </summary>
    public AnglesEnum Orientation { get; set; }

    /// <summary>
    ///   Gets or sets the stock sizes of the material.
    /// </summary>
    public ObservableCollection<ISheetLoadInfo> StockSize { get; set; }

    /// <summary>
    ///   Gets the display name of the material.
    /// </summary>
    public string DisplayName => $"{Name}: ${Cost}";

    /// <summary>
    ///   Adds a stock size with the specified width, height, and quantity.
    /// </summary>
    /// <param name="width">The width of the stock size.</param>
    /// <param name="height">The height of the stock size.</param>
    public void AddStockSize(int width, int height);

    /// <summary>
    ///   Adds a stock size.
    /// </summary>
    /// <param name="sheetLoadInfo">The stock size to add.</param>
    public void AddStockSize(ISheetLoadInfo sheetLoadInfo);

    /// <summary>
    ///   Adds a range of stock sizes.
    /// </summary>
    /// <param name="sheetLoadInfos">The stock sizes to add.</param>
    public void AddStockSizeRange(IEnumerable<ISheetLoadInfo> sheetLoadInfos);
  }
}
namespace DeepNestLib.NestProject
{
  /// <summary>
  ///   Represents the information about a sheet load.
  /// </summary>
  public interface ISheetLoadInfo
  {
    /// <summary>
    ///   Gets or sets the ID of the sheet load.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    ///   Gets or sets the unique ID of the sheet.
    /// </summary>
    string UniqueSheetId { get; set; }

    /// <summary>
    ///   Gets or sets the height of the sheet.
    /// </summary>
    int Height { get; set; }

    /// <summary>
    ///   Gets or sets the quantity of the sheet.
    /// </summary>
    int Quantity { get; set; }

    /// <summary>
    ///   Gets or sets the width of the sheet.
    /// </summary>
    int Width { get; set; }

    /// <summary>
    ///   Gets or sets the material of the sheet.
    /// </summary>
    IMaterial Material { get; set; }
  }
}
namespace DeepNestLib
{
  /// <summary>
  ///   Represents a sheet used in the nesting process.
  /// </summary>
  public interface ISheet : INfp
  {
    /// <summary>
    ///   Gets or sets the material of the sheet.
    /// </summary>
    IMaterial Material { get; set; }

    /// <summary>
    ///   Gets or sets the unique identifier of the sheet.
    /// </summary>
    string UniqueId { get; set; }
  }
}
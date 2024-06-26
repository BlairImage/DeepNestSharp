namespace DeepNestLib
{
  using System.Collections.Generic;

  /// <summary>
  ///   Provides extension methods for the IList<ISheet> interface.
  /// </summary>
  public static class SheetExtensions
  {
    /// <summary>
    ///   Retrieves the material of the first sheet in the list, if any.
    ///   This is mostly useful when you know that all sheets in the list are made of the same material.
    /// </summary>
    /// <param name="sheets">The list of sheets.</param>
    /// <returns>The material of the first sheet, or null if the list is empty.</returns>
    public static IMaterial GetMaterial(this IList<ISheet> sheets)
    {
      return sheets.Count > 0 ? sheets[0].Material : null;
    }
  }
}
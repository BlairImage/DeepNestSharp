namespace DeepNestLib
{
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using NestProject;

  /// <summary>
  ///   Represents a catalog of materials.
  /// </summary>
  public interface IMaterialCatalog
  {
    /// <summary>
    ///   Gets the collection of materials in the catalog.
    /// </summary>
    public ObservableCollection<IMaterial> Materials { get; }

    /// <summary>
    ///   Selects the best fit sheet for the given total area and material.
    /// </summary>
    /// <param name="totalArea">The total area required.</param>
    /// <param name="material">The material to be used.</param>
    /// <returns>The sheet load information for the best fit sheet.</returns>
    ISheetLoadInfo SelectBestFitSheet(double totalArea, IMaterial material);

    /// <summary>
    ///   Asynchronously adds a material to the catalog.
    /// </summary>
    /// <param name="material">The material to add.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the Id of the added material.</returns>
    Task<int> AddMaterialAsync(IMaterial material);

    /// <summary>
    ///   Asynchronously deletes a material from the catalog.
    /// </summary>
    /// <param name="material">The material to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteMaterialAsync(IMaterial material);

    /// <summary>
    ///   Asynchronously updates a material in the catalog.
    /// </summary>
    /// <param name="material">The material to update.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateMaterialAsync(IMaterial material);

    /// <summary>
    ///   Clears the quantity of all materials in the catalog.
    /// </summary>
    void ClearQty();

    /// <summary>
    ///   Asynchronously refreshes the materials in the catalog.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RefreshMaterials();

    /// <summary>
    ///   Asynchronously adds a sheet load information to a material in the catalog.
    /// </summary>
    /// <param name="material">The material to add the sheet load information to.</param>
    /// <param name="sheetLoadInfo">The sheet load information to add.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the Id of the added sheet load information.</returns>
    Task<int> AddSheetToMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);

    /// <summary>
    ///   Asynchronously deletes a sheet load information from a material in the catalog.
    /// </summary>
    /// <param name="material">The material to delete the sheet load information from.</param>
    /// <param name="sheetLoadInfo">The sheet load information to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteSheetToMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);

    /// <summary>
    ///   Asynchronously updates a sheet load information in a material in the catalog.
    /// </summary>
    /// <param name="material">The material to update the sheet load information in.</param>
    /// <param name="sheetLoadInfo">The updated sheet load information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateSheetInMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);
  }
}
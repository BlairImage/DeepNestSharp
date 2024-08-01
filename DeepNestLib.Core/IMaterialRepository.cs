namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Threading.Tasks;
  using NestProject;

  public interface IMaterialRepository
  {
    /// <summary>
    ///   Retrieves all materials asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the collection of materials.</returns>
    Task<IEnumerable<IMaterial>> GetAllMaterialsAsync();

    /// <summary>
    ///   Retrieves a material asynchronously.
    /// </summary>
    /// <param name="material">The material to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the retrieved material.</returns>
    Task<IMaterial> GetMaterialAsync(IMaterial material);

    /// <summary>
    ///   Adds a material asynchronously.
    /// </summary>
    /// <param name="material">The material to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the added material.</returns>
    Task<int> AddMaterialAsync(IMaterial material);

    /// <summary>
    ///   Updates a material asynchronously.
    /// </summary>
    /// <param name="material">The material to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateMaterialAsync(IMaterial material);

    /// <summary>
    ///   Deletes a material asynchronously.
    /// </summary>
    /// <param name="material">The material to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteMaterialAsync(IMaterial material);

    /// <summary>
    ///   Adds a sheet to a material asynchronously.
    /// </summary>
    /// <param name="material">The material to add the sheet to.</param>
    /// <param name="sheetLoadInfo">The sheet load information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the ID of the added sheet.</returns>
    Task<int> AddSheetToMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);

    /// <summary>
    ///   Deletes a sheet from a material asynchronously.
    /// </summary>
    /// <param name="material">The material to delete the sheet from.</param>
    /// <param name="sheetLoadInfo">The sheet load information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteSheetFromMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);

    /// <summary>
    ///   Updates a sheet in a material asynchronously.
    /// </summary>
    /// <param name="material">The material to update the sheet in.</param>
    /// <param name="sheetLoadInfo">The sheet load information.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateSheetInMaterialAsync(IMaterial material, ISheetLoadInfo sheetLoadInfo);
  }
}
namespace DeepNestLib
{
  using System.Collections.Generic;
  using NestProject;

  public interface IMaterialCatalog
  {
    ISheetLoadInfo SelectBestFitSheet(double totalArea, string materialType);
  }
}
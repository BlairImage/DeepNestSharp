namespace DeepNestLib
{
  using System.Collections.Generic;
  using NestProject;

  public interface IMaterial
  {
    public string Name { get; set; }

    public string Description { get; set; }

    public double Cost { get; set; }

    public byte[] ImageData { get; set; }

    public List<ISheetLoadInfo> StockSize { get; set; }

    public string DisplayName => $"{Name}: ${Cost}";

    public void AddStockSize(int width, int height, int quantity);

    public void AddStockSize(SheetLoadInfo sheetLoadInfo);

    public void AddStockSizeRange(IEnumerable<SheetLoadInfo> sheetLoadInfos);
  }
}
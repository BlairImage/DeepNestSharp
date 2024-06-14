namespace DeepNestLib
{
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using NestProject;

  public interface IMaterial
  {
    public string Name { get; set; }

    public string Description { get; set; }

    public double Cost { get; set; }

    public byte[] ImageData { get; set; }

    public ObservableCollection<ISheetLoadInfo> StockSize { get; set; }

    public string DisplayName => $"{Name}: ${Cost}";

    public void AddStockSize(int width, int height, int quantity = 0);

    public void AddStockSize(ISheetLoadInfo sheetLoadInfo);

    public void AddStockSizeRange(IEnumerable<ISheetLoadInfo> sheetLoadInfos);
  }
}
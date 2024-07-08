namespace DeepNestLib.NestProject
{
  public interface ISheetLoadInfo
  {
    string UniqueId { get; set; }

    int Height { get; set; }

    int Quantity { get; set; }

    int Width { get; set; }

    IMaterial Material { get; set; }
  }
}
namespace DeepNestLib.NestProject
{
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
  {
    [JsonConstructor]
    public SheetLoadInfo(int width, int height, int quantity)
    {
      Width = width;
      Height = height;
      Quantity = quantity;
    }

    public SheetLoadInfo(int width, int height, int quantity, IMaterial material)
    {
      Width = width;
      Height = height;
      Quantity = quantity;
      Material = material;
    }

    public virtual int Width { get; set; }

    public virtual int Height { get; set; }

    public virtual int Quantity { get; set; }

    public virtual IMaterial Material { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      JsonSerializerOptions options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
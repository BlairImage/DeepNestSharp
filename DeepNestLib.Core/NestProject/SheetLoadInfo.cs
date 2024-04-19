namespace DeepNestLib.NestProject
{
  using System;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using DeepNestLib;
  using DeepNestLib.IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo
  {
    [JsonConstructor]
    public SheetLoadInfo(int width, int height, int quantity)
    {
      this.Width = width;
      this.Height = height;
      this.Quantity = quantity;
    }

    public virtual int Width { get; set; }

    public virtual int Height { get; set; }

    public virtual int Quantity { get; set; }

    public override string ToJson(bool writeIndented = false)
    {
      var options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }
  }
}
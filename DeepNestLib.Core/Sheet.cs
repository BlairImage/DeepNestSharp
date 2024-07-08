namespace DeepNestLib
{
  using System.Text.Json;

  public class Sheet : NoFitPolygon, ISheet
  {
    public Sheet()
    {
    }

    public Sheet(ISheet sheet, WithChildren withChildren) : base(sheet, withChildren)
    {
    }

    public Sheet(INfp nfp, WithChildren withChildren) : base(nfp, withChildren)
    {
    }

    public override string ToJson()
    {
      JsonSerializerOptions options = new();
      options.Converters.Add(new NfpJsonConverter());
      options.WriteIndented = true;
      return JsonSerializer.Serialize(this, options);
    }

    public IMaterial Material { get; set; }
    public string UniqueId { get; set; }

    /// <summary>
    ///   Creates a new <see cref="Sheet" /> from the json supplied.
    /// </summary>
    /// <param name="json">Serialised representation of the Sheet to create.</param>
    /// <returns>New <see cref="Sheet" />.</returns>
    public new static Sheet FromJson(string json)
    {
      JsonSerializerOptions options = new();
      options.Converters.Add(new NfpJsonConverter());
      Sheet result = JsonSerializer.Deserialize<Sheet>(json, options);
      return result;
    }

    public static Sheet NewSheet(int nameSuffix, int w = 3000, int h = 1500)
    {
      RectangleSheet tt = new();
      tt.Name = "rectSheet" + nameSuffix;
      tt.Build(w, h);
      return tt;
    }
  }
}
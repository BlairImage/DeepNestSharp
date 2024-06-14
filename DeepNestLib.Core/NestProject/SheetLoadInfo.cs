namespace DeepNestLib.NestProject
{
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo, INotifyPropertyChanged
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

    private int m_Width;

    public virtual int Width
    {
      get => m_Width;
      set => SetField(ref m_Width, value);
    }

    private int m_Height;
    public virtual int Height
    {
      get => m_Height;
      set => SetField(ref m_Height, value);
    }

    private int m_Quantity;
    public virtual int Quantity
    {
      get => m_Quantity;
      set => SetField(ref m_Quantity, value);
    }

    private IMaterial m_Material;
    public virtual IMaterial Material
    {
      get => m_Material;
      set => SetField(ref m_Material, value);
    }

    public override string ToJson(bool writeIndented = false)
    {
      JsonSerializerOptions options = new JsonSerializerOptions();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
        return false;
      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
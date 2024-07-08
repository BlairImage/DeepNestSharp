namespace DeepNestLib.NestProject
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Text.Json;
  using System.Text.Json.Serialization;
  using IO;

  public class SheetLoadInfo : Saveable, ISheetLoadInfo, INotifyPropertyChanged
  {
    private int m_Height;

    private IMaterial m_Material;

    private int m_Quantity;

    private string m_UniqueId;

    private int m_Width;

    [JsonConstructor]
    public SheetLoadInfo(int width, int height, int quantity)
    {
      Width = width;
      Height = height;
      Quantity = quantity;
      UniqueId = Guid.NewGuid().ToString();
    }

    public SheetLoadInfo(int width, int height, int quantity, IMaterial material)
    {
      Width = width;
      Height = height;
      Quantity = quantity;
      Material = material;
      UniqueId = Guid.NewGuid().ToString();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    public virtual int Width
    {
      get => m_Width;
      set => SetField(ref m_Width, value);
    }

    public virtual string UniqueId
    {
      get => m_UniqueId;
      set => SetField(ref m_UniqueId, value);
    }

    public virtual int Height
    {
      get => m_Height;
      set => SetField(ref m_Height, value);
    }

    public virtual int Quantity
    {
      get => m_Quantity;
      set => SetField(ref m_Quantity, value);
    }

    public virtual IMaterial Material
    {
      get => m_Material;
      set => SetField(ref m_Material, value);
    }

    public override string ToJson(bool writeIndented = false)
    {
      JsonSerializerOptions options = new();
      options.WriteIndented = writeIndented;
      return JsonSerializer.Serialize(this, options);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
      if (EqualityComparer<T>.Default.Equals(field, value))
      {
        return false;
      }

      field = value;
      OnPropertyChanged(propertyName);
      return true;
    }
  }
}
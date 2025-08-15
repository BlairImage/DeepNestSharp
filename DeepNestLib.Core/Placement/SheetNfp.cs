[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DeepNestLib.CiTests")]

namespace DeepNestLib
{
  using DeepNestLib.Geometry;
  using DeepNestLib.Placement;
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Linq;
  using System.Reflection;
  using System.Text.Json;
  using System.Text.Json.Serialization;
#if NCRUNCH
  using System.Text;
#endif

  public class SheetNfp : NfpCandidateList
  {
    public new const string FileDialogFilter = "DeepNest SheetNfp (*.dnsnfp)|*.dnsnfp|All files (*.*)|*.*";
    private readonly Action<string> verboseLog;

    public SheetNfp(NfpCandidateList nfpCandidateList, ISvgNestConfig config)
      : base(nfpCandidateList.Items, nfpCandidateList.Sheet, nfpCandidateList.Part, config)
    {
    }

    [JsonConstructor]
    public SheetNfp(INfp[] items, ISheet sheet, INfp part, ISvgNestConfig config)
      : base(items, sheet, part, config)
    {
    }

    internal SheetNfp(INfpHelper nfpHelper, ISheet sheet, INfp part, ISvgNestConfig config, bool useDllImport)
      : this(nfpHelper, sheet, part, config, useDllImport, o => { })
    {
    }

    // Inner NFP aka SheetNfp
    public SheetNfp(INfpHelper nfpHelper, ISheet sheet, INfp part, ISvgNestConfig config, bool useDllImport, Action<string> verboseLog)
      : this(RemoveOuterNfps(nfpHelper.GetInnerNfp(sheet, part, MinkowskiCache.Cache, config.ClipperScale, useDllImport, verboseLog) ?? new INfp[0], sheet, verboseLog, config), sheet, part, config)
    {
    }

    private static INfp[] RemoveOuterNfps(INfp[] nfps, ISheet sheet, Action<string> verboseLog, ISvgNestConfig config)
    {
      IEnumerable<INfp> result = new List<INfp>(nfps);
      verboseLog($"RemovedOuterNfps:{result.Where(o => !SmallerThanSheet(sheet, o)).Count()}");
      result = result.Where<INfp>(o => SmallerThanSheet(sheet, o))
                     .Select(o =>
                     {
                       o.Clean(config);
                       o.EnsureIsClosed();
                       return o;
                     });
      return result.ToArray();
    }

    private static bool SmallerThanSheet(ISheet sheet, INfp o)
    {
      return o.WidthCalculated < sheet.WidthCalculated &&
             o.HeightCalculated < sheet.HeightCalculated;
    }

    internal bool CanAcceptPart
    {
      get
      {
        if (this.NumberOfNfps > 0)
        {
          if (this[0].Length == 0)
          {
            throw new ArgumentException();
          }
          else
          {
            return true;
          }
        }

        return false;
      }
    }

    public static new SheetNfp FromJson(string json, ISvgNestConfig config)
    {
      var options = new JsonSerializerOptions();
      options.Converters.Add(new SheetJsonConverter());
      options.Converters.Add(new NfpJsonConverter());
      var nfpCandidateList = JsonSerializer.Deserialize<NfpCandidateList>(json, options);
      return new SheetNfp(nfpCandidateList, config);
    }

    public static new SheetNfp LoadFromFile(string fileName, ISvgNestConfig config)
    {
      using (StreamReader inputFile = new StreamReader(fileName))
      {
        return FromJson(inputFile.ReadToEnd(), config);
      }
    }

    internal static SheetNfp LoadFromStream(string path, ISvgNestConfig config)
    {
      using (var stream = Assembly.GetExecutingAssembly().GetEmbeddedResourceStream(path))
      {
        return LoadFromStream(stream, config);
      }
    }

    internal static SheetNfp LoadFromStream(Stream stream, ISvgNestConfig config)
    {
      using (StreamReader reader = new StreamReader(stream))
      {
        return FromJson(reader.ReadToEnd(), config);
      }
    }

    internal object Shift(INfp processedPart)
    {
      throw new NotImplementedException();
    }

    public IPointXY GetCandidatePointClosestToOrigin()
    {
      SvgPoint result = null;
      for (int nfpCandidateIndex = 0; nfpCandidateIndex < this.NumberOfNfps; nfpCandidateIndex++)
      {
        var nfpCandidate = this[nfpCandidateIndex];
        for (int pointIndex = 0; pointIndex < nfpCandidate.Points.Length; pointIndex++)
        {
          var nfpPoint = nfpCandidate.Points[pointIndex];
          if (result == null ||
              nfpPoint.X < result.X ||
              (GeometryUtil.AlmostEqual(nfpPoint.X, result.X) && nfpPoint.Y < result.Y))
          {
            result = nfpPoint;
          }
        }
      }

      if (result == null)
      {
        throw new Exception("result null; shouldn't be possible; shouldn't get to method if the SheetNfp couldn't accept part.");
      }

      return result;
    }
  }
}
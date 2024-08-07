﻿namespace DeepNestLib
{
  using System;
  using System.Collections.Specialized;
  using System.ComponentModel;
  using System.Linq;
  using System.Threading;
#if NCRUNCH
  using System.Diagnostics;
#endif

  public class NestState : INestStateBackground, INestStateSvgNest, INestStateMinkowski, INestStateNestingContext, INotifyPropertyChanged
  {
    private int clipperCallCounter;
    private int dllCallCounter;
    private int generations;
    private int iterations;
    private long lastNestTime;
    private long lastPlacementTime;
    private int nestCount;
    private int population;
    private int rejected;
    private int threads;
    private long totalNestTime;
    private long totalPlacementTime;

    public NestState(ITopNestResultsConfig config, IDispatcherService dispatcherService)
    {
      TopNestResults = new TopNestResultsCollection(config, dispatcherService);
      TopNestResults.CollectionChanged += TopNestResults_CollectionChanged;
    }

    [Category("Minkowski")]
    [DisplayName("Clipper Call Counter")]
    public int ClipperCallCounter => clipperCallCounter;

    [Description("Last Nest Time (milliseconds). The total time for the nest including Pmap, DeepNest and Placement.")]
    [Category("Performance")]
    [DisplayName("Last Nest Time")]
    public long LastNestTime => lastNestTime;

    [Description("Time last top placement found.")]
    [Category("Performance")]
    [DisplayName("Last Top Found")]
    public DateTime? LastTopFoundTimestamp
    {
      get
      {
        if (TopNestResults.Count == 0)
        {
          return null;
        }
        return TopNestResults.Max(o => o.CreatedAt);
      }
    }

    [Description("Number of rejected Nests.")]
    [Category("Performance")]
    public int Rejected => rejected;

    [Description("Average time per Nest Result since start of the run.")]
    [Category("Performance")]
    [DisplayName("Average Nest Time")]
    public long AverageNestTime => nestCount == 0 ? 0 : totalNestTime / nestCount;

    [Description("Average time per Nest Result since start of the run.")]
    [Category("Performance")]
    [DisplayName("Average Placement Time")]
    public long AveragePlacementTime => nestCount == 0 ? 0 : totalPlacementTime / nestCount;

    [Description("The number of times the external C++ Minkowski library has been called. " + "This should stabilise at the number of distinct parts in the nest times the number " +
                 "of rotations. If it keeps growing then the caching mechanism may not be working as " + "intended; possibly due to complexity of the parts, possibly due to overflow " +
                 "failures in the Minkoski Sum. That said if your parts have holes then the calls to" + "hole NfpSums aren't cached?")]
    [Category("Minkowski")]
    [DisplayName("Dll Call Counter")]
    public int DllCallCounter => dllCallCounter;

    [Description("The number of generations processed.")]
    [Category("Genetic Algorithm")]
    public int Generations => generations;

    [Category("Progress")]
    public bool IsErrored { get; private set; }

    [Category("Progress")]
    public int Iterations => iterations;

    [Description("Last Placement Time (milliseconds).")]
    [Category("Performance")]
    [DisplayName("Last Placement Time")]
    public long LastPlacementTime => lastPlacementTime;

    [Category("Progress")]
    [DisplayName("Nest Count")]
    public int NestCount => nestCount;

    [Category("Performance")]
    [DisplayName("NfpPair % Cached")]
    public double NfpPairCachePercentCached { get; private set; }

    /// <inheritdoc />
    [Description("Population of the current generation.")]
    [Category("Genetic Algorithm")]
    public int Population => population;

    [Description("Number of active Nest threads.")]
    [Category("Performance")]
    public int Threads => threads;

    [Browsable(false)]
    public TopNestResultsCollection TopNestResults { get; }

    public void SetIsErrored()
    {
      IsErrored = true;
    }

    public void SetNfpPairCachePercentCached(double percentCached)
    {
      NfpPairCachePercentCached = percentCached;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NfpPairCachePercentCached)));
    }

    void INestStateMinkowski.IncrementDllCallCounter()
    {
      Interlocked.Increment(ref dllCallCounter);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DllCallCounter)));
    }

    void INestStateMinkowski.IncrementClipperCallCounter()
    {
      Interlocked.Increment(ref clipperCallCounter);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipperCallCounter)));
    }

    void INestStateNestingContext.Reset()
    {
      Interlocked.Exchange(ref nestCount, 0);
      Interlocked.Exchange(ref totalNestTime, 0);
      Interlocked.Exchange(ref totalPlacementTime, 0);
      Interlocked.Exchange(ref generations, 0);
      Interlocked.Exchange(ref population, 0);
      Interlocked.Exchange(ref lastNestTime, 0);
      Interlocked.Exchange(ref lastPlacementTime, 0);
      Interlocked.Exchange(ref iterations, 0);
      Interlocked.Exchange(ref dllCallCounter, 0);
      Interlocked.Exchange(ref clipperCallCounter, 0);
      IsErrored = false;
      TopNestResults.Clear();
      SetNfpPairCachePercentCached(0);

      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastTopFoundTimestamp)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPlacementTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastNestTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NestCount)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AveragePlacementTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageNestTime)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Generations)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Iterations)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DllCallCounter)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClipperCallCounter)));
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TopNestResults)));
    }

    void INestStateNestingContext.IncrementIterations()
    {
      Interlocked.Increment(ref iterations);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Iterations)));
    }

    void INestStateSvgNest.IncrementPopulation()
    {
      Interlocked.Increment(ref population);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
    }

    void INestStateSvgNest.SetLastPlacementTime(long placePartTime)
    {
      lastPlacementTime = placePartTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastPlacementTime)));
    }

    void INestStateSvgNest.SetLastNestTime(long backgroundTime)
    {
      lastNestTime = backgroundTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastNestTime)));
    }

    void INestStateSvgNest.IncrementNestCount()
    {
      Interlocked.Increment(ref nestCount);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NestCount)));
    }

    void INestStateSvgNest.IncrementNestTime(long backgroundTime)
    {
      totalPlacementTime += backgroundTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AverageNestTime)));
    }

    void INestStateSvgNest.IncrementPlacementTime(long placePartTime)
    {
      totalNestTime += placePartTime;
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AveragePlacementTime)));
    }

    void INestStateSvgNest.IncrementGenerations()
    {
      Interlocked.Increment(ref generations);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Generations)));
    }

    void INestStateSvgNest.ResetPopulation()
    {
      Interlocked.Exchange(ref population, 0);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Population)));
    }

    void INestStateSvgNest.IncrementThreads()
    {
      Interlocked.Increment(ref threads);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
    }

    void INestStateSvgNest.DecrementThreads()
    {
      Interlocked.Decrement(ref threads);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Threads)));
    }

    public void IncrementRejected()
    {
      Interlocked.Increment(ref rejected);
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Rejected)));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void TopNestResults_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastTopFoundTimestamp)));
      }
    }

    public static NestState CreateInstance(ISvgNestConfig config, IDispatcherService dispatcherService)
    {
      return new NestState(config, dispatcherService);
    }
  }
}
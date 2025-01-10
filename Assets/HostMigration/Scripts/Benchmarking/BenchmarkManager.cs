using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class BenchmarkManager
{
    public static Stopwatch MethodClientStopWatch = new();
    public static Stopwatch MethodServerStopWatch = new();
    public static Stopwatch MethodCStopWatch = new();

    /// <summary>
    /// Starts or restarts the benchmark timer.
    /// </summary>
    public static void StartBenchmark(Stopwatch stopwatch)
    {
        stopwatch.Reset();  // Reset to start fresh
        stopwatch.Start();
        UnityEngine.Debug.Log($"Benchmark started {stopwatch}.");
    }

    /// <summary>
    /// Stops the benchmark timer and logs the elapsed time.
    /// </summary>
    public static void StopBenchmark(Stopwatch stopwatch)
    {
        stopwatch.Stop();
        var result = stopwatch.ElapsedMilliseconds;

        if(stopwatch == MethodClientStopWatch) UnityEngine.Debug.Log($"Benchmark Client-side info finished. Elapsed time: {result} ms");
        if(stopwatch == MethodServerStopWatch) UnityEngine.Debug.Log($"Benchmark Server-side info finished. Elapsed time: {result} ms");
    }
}

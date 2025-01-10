using System.Diagnostics;

public static class BenchmarkManager
{
    public static Stopwatch MethodClientStopWatch = new();
    public static Stopwatch MethodServerRetrievalStopWatch = new();

    /// <summary>
    /// Starts or restarts the benchmark timer.
    /// </summary>
    public static void StartBenchmark(Stopwatch stopwatch)
    {
        stopwatch.Reset();  // Reset to start fresh
        stopwatch.Start();
        UnityEngine.Debug.Log($"Benchmark started.");
    }

    /// <summary>
    /// Stops the benchmark timer and logs the elapsed time in microseconds.
    /// </summary>
    public static void StopBenchmark(Stopwatch stopwatch)
    {
        stopwatch.Stop();

        // Calculate elapsed time in microseconds
        double elapsedMicroseconds = stopwatch.ElapsedTicks * (1000000.0 / Stopwatch.Frequency);

        if (stopwatch == MethodClientStopWatch) UnityEngine.Debug.Log($"Benchmark Client-side info finished. Elapsed time: {elapsedMicroseconds} µs");
        if (stopwatch == MethodServerRetrievalStopWatch) UnityEngine.Debug.Log($"Benchmark Server-side retrieval info finished. Elapsed time: {elapsedMicroseconds} µs");
    }
}

using System.Diagnostics;
using System.IO;

public class MyStopwatch : Stopwatch
{
    public bool HasAlreadyStoppedOnce { get; private set; }
    public bool NeedsTwoStops { get; set; }
    public string CSVInformation { get; set; }

    // Constructor for initializing the stopwatch with the ability to require two stops
    public MyStopwatch(bool needsTwoStops = false)
    {
        HasAlreadyStoppedOnce = false;
        NeedsTwoStops = needsTwoStops;
        //CSVInformation = extraCSVInformation; // Set through the public property, can't have two defaulting values
    }

    // Override the Reset method to reset HasAlreadyStoppedOnce as well
    public new void Reset()
    {
        base.Reset();  // Reset the stopwatch itself
        HasAlreadyStoppedOnce = false; 
    }

    // Override Stop method to handle the two-stop logic
    public new void Stop()
    {
        if (NeedsTwoStops)
        {
            if (HasAlreadyStoppedOnce)
            {
                base.Stop();  // Complete the stop if it's the second stop
            }
            else
            {
                UnityEngine.Debug.Log("Stopping stopwatch for the first time!");
                HasAlreadyStoppedOnce = true;  // Mark that the first stop has been done
            }
        }
        else
        {
            base.Stop();  // For one-stop case, just stop directly
        }
    }

    public bool IsFullyStopped()
    {
        return !NeedsTwoStops || (NeedsTwoStops && HasAlreadyStoppedOnce && !IsRunning);
    }
}

public static class BenchmarkManager
{
    // Assets folder in the Unity project
    private static readonly string CsvFilePath = Path.Combine(UnityEngine.Application.dataPath, "BenchmarkResults.csv");

    static BenchmarkManager() // For csv data saving
    {
        // Write the CSV header if the file doesn't exist
        if (!File.Exists(CsvFilePath))
        {
            File.AppendAllText(CsvFilePath, "Timestamp,Player Count,Sync Method,Extra bytes/packets,Duration in microseconds\n");
        }
    }

    public static MyStopwatch MethodClientStopWatch = new();
    public static MyStopwatch MethodServerRetrievalStopWatch = new();

    public static ulong AmountOfExtraClientBytes = 2_000_000_000;
    public static uint AmountOfExtraServerDatas = 2690;
    /// <summary>
    /// Starts or restarts the benchmark timer.
    /// </summary>
    public static void StartBenchmark(MyStopwatch stopwatch, string CSVinfo, bool needsTwoStops = false)
    {
        stopwatch.Reset();  // Reset to start fresh
        stopwatch.CSVInformation = CSVinfo;
        stopwatch.NeedsTwoStops = needsTwoStops;
        stopwatch.Start();
        UnityEngine.Debug.Log($"Benchmark started.");
    }

    /// <summary>
    /// Stops the benchmark timer and logs the elapsed time in microseconds.
    /// </summary>
    public static void StopBenchmark(MyStopwatch stopwatch)
    {
        stopwatch.Stop();
        // If it requires two stops, don't continue yet.
        if (!stopwatch.IsFullyStopped()) return;

        // Calculate elapsed time in microseconds
        double elapsedMicroseconds = stopwatch.ElapsedTicks * (1000000.0 / Stopwatch.Frequency);

        if (stopwatch == MethodClientStopWatch)
        {
            UnityEngine.Debug.Log($"Benchmark Client-side info finished. Elapsed time: {elapsedMicroseconds} µs");
            WriteResultToCsvFile(stopwatch.CSVInformation, "Client-side info", elapsedMicroseconds);
        }
        if (stopwatch == MethodServerRetrievalStopWatch)
        {
            UnityEngine.Debug.Log($"Benchmark Server-side retrieval info finished. Elapsed time: {elapsedMicroseconds} µs");
            WriteResultToCsvFile(stopwatch.CSVInformation, "Server-side info", elapsedMicroseconds);
        }
    }

    private static void WriteResultToCsvFile(string information, string nameOfMethod, double elapsedMicroseconds)
    {
        // Write result to CSV file
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string csvLine = $"{timestamp},{PlayersManager.Instance.GetClients().Count},{nameOfMethod}, {information}, {elapsedMicroseconds}\n";
        File.AppendAllText(CsvFilePath, csvLine);
    }
}

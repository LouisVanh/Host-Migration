using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BenchmarkManager : MonoBehaviour
{
    public static BenchmarkManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public List<Stopwatch> Stopwatches = new();


    public void StartMethodBenchmark()
    {

    }
}

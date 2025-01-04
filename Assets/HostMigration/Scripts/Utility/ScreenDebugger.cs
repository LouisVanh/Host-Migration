using UnityEngine;
using System.Collections;

public class ScreenDebugger : MonoBehaviour
{
    byte _maxQueueItems = 30;  // number of messages to keep
    Queue _myLogQueue = new();

    void Start()
    {
        Debug.Log("Started up on-screen logging.");
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        _myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            _myLogQueue.Enqueue(stackTrace);
        while (_myLogQueue.Count > _maxQueueItems)
            _myLogQueue.Dequeue();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(Screen.width - 400, 0, 400, Screen.height));
        GUILayout.Label("\n" + string.Join("\n", _myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
}
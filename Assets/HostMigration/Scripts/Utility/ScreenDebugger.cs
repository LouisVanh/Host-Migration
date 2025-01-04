using UnityEngine;
using System.Collections.Generic;

public class OnScreenDebugger : MonoBehaviour
{
    private class LogEntry
    {
        public string Message;
        public LogType Type;

        public LogEntry(string message, LogType type)
        {
            Message = message;
            Type = type;
        }
    }

    [Header("Log Type Filters")]
    public bool ShowErrors = true;
    public bool ShowWarnings = true;
    public bool ShowDefault = true;

    private List<LogEntry> logEntries = new List<LogEntry>();
    private Vector2 scrollPosition;
    private const byte MaxLogs = 50;

    private GUIStyle logStyle;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;

    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        logEntries.Add(new LogEntry(logString, type));

        // Limit the number of logs stored
        if (logEntries.Count > MaxLogs)
        {
            logEntries.RemoveAt(0);
        }
    }

    private void OnGUI()
    {
        logStyle = new GUIStyle(GUI.skin.label)
        {
            wordWrap = true,
            fontSize = 11, // Smaller font size
            padding = new RectOffset(0, 0, -3, -3) // Compact padding
        };
        try
        {
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (var logEntry in logEntries)
            {
                // Filter logs based on toggle settings
                if (!ShouldDisplayLog(logEntry.Type))
                    continue;

                // Set the color based on the log type
                switch (logEntry.Type)
                {
                    case LogType.Log:
                        logStyle.normal.textColor = Color.white;
                        break;
                    case LogType.Warning:
                        logStyle.normal.textColor = Color.yellow;
                        break;
                    case LogType.Error:
                    case LogType.Exception:
                        logStyle.normal.textColor = Color.red;
                        break;
                    default:
                        logStyle.normal.textColor = Color.gray;
                        break;
                }

                GUILayout.Label(logEntry.Message, logStyle);
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"OnScreenDebugger encountered an error: {ex.Message}");
        }
    }

    private bool ShouldDisplayLog(LogType type)
    {
        return (ShowErrors && (type == LogType.Error || type == LogType.Exception)) ||
               (ShowWarnings && type == LogType.Warning) ||
               (ShowDefault && type == LogType.Log);
    }
}

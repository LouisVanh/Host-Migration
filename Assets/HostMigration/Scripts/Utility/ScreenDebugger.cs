using UnityEngine;
using System.Collections.Generic;

public class OnScreenDebugger : MonoBehaviour
{
    // MIT license: Made by teledev, contact teled on discord for help
    private class LogEntry
    {
        public string Message;
        public LogType LogType;

        public LogEntry(string message, LogType type)
        {
            Message = message;
            LogType = type;
        }
    }

    [Header("Log Type Filters")]
    [SerializeField] private bool _switchLogTypesRuntime = false;

    [Space(10)]
    [SerializeField] private KeyCode _errorToggleKey = KeyCode.E;
    [SerializeField] private KeyCode _warningToggleKey = KeyCode.W;
    [SerializeField] private KeyCode _defaultToggleKey = KeyCode.D;

    [Space(10)]
    [SerializeField] private bool _showErrors = true;
    [SerializeField] private bool _showWarnings = true;
    [SerializeField] private bool _showDefault = true;

    [Header("Internal")]
    [SerializeField] private byte _maxAmountOfLogs = 50;
    private List<LogEntry> _logEntries = new();
    private Vector2 _scrollPosition;
    private GUIStyle _logStyle;


    private void OnEnable() => Application.logMessageReceived += HandleLog;
    private void OnDisable() => Application.logMessageReceived -= HandleLog;

    private void Update() // Toggle specific logs on runtime
    {
        if (!_switchLogTypesRuntime) return; // Set on design time
        if (Input.GetKeyDown(_errorToggleKey)) _showErrors = !_showErrors;
        if (Input.GetKeyDown(_defaultToggleKey)) _showDefault = !_showDefault;
        if (Input.GetKeyDown(_warningToggleKey)) _showWarnings = !_showWarnings;
    }
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logEntries.Add(new LogEntry(logString, type));

        // Limit the number of logs stored (keeping the most recent)
        if (_logEntries.Count > _maxAmountOfLogs)
        {
            _logEntries.RemoveAt(0);
        }
    }

    private void OnGUI()
    {
        if (_logStyle == null)
        {
            _logStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true,
                fontSize = 11, // Small
                padding = new RectOffset(0, 0, -3, -3) // Compact spacing
            };
        }

        try
        {
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var logEntry in _logEntries)
            {
                // Filter logs based on toggle settings in editor
                if (!ShouldDisplayLog(logEntry.LogType))
                    continue;

                switch (logEntry.LogType)
                {
                    case LogType.Log:
                        _logStyle.normal.textColor = Color.white;
                        break;

                    case LogType.Warning:
                        _logStyle.normal.textColor = Color.yellow;
                        break;

                    case LogType.Assert:
                    case LogType.Error:
                    case LogType.Exception:
                        _logStyle.normal.textColor = Color.red;
                        break;
                }

                GUILayout.Label(logEntry.Message, _logStyle);
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
        return (_showErrors && (type == LogType.Error || type == LogType.Exception)) ||
               (_showWarnings && type == LogType.Warning) ||
               (_showDefault && type == LogType.Log);
    }
}

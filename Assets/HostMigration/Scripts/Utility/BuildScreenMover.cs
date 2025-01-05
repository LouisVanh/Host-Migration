using UnityEngine;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class MultiInstanceWindowManager : MonoBehaviour
{
#if !UNITY_EDITOR
    void Start()
    {
        SetWindowToQuarterScreen();
    }

    public void SetWindowToQuarterScreen()
    {
        // Get the current screen resolution
        int screenWidth = Screen.currentResolution.width;
        int screenHeight = Screen.currentResolution.height;

        // Calculate the size for 1/4th of the screen
        int windowWidth = screenWidth / 2;
        int windowHeight = screenHeight / 2;

        // Check how many instances of the program are running
        int instanceIndex = GetInstanceIndex();

        // Determine the window position based on instance index
        int x = 0;
        int y = 0;

        switch (instanceIndex)
        {
            case 0: // Top-left
                x = 0;
                y = 0;
                break;
            case 1: // Top-right
                x = screenWidth / 2;
                y = 0;
                break;
            case 2: // Bottom-left
                x = 0;
                y = screenHeight / 2;
                break;
            case 3: // Bottom-right
                x = screenWidth / 2;
                y = screenHeight / 2;
                break;
            default: // If more than 4 instances, stack them at (0,0)
                x = 0;
                y = 0;
                break;
        }

        // Set the windowed mode and size
        Screen.SetResolution(windowWidth, windowHeight, false);

        // Move the window to the specified position
        SetWindowPosition(x, y);
    }

    private int GetInstanceIndex()
    {
        // Get the current process name
        string processName = Process.GetCurrentProcess().ProcessName;

        // Get all processes with the same name
        Process[] processes = Process.GetProcessesByName(processName);

        // Find the index of the current process
        int instanceIndex = 0;
        int currentProcessId = Process.GetCurrentProcess().Id;

        foreach (var process in processes)
        {
            if (process.Id == currentProcessId)
                break;

            instanceIndex++;
        }

        return instanceIndex;
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(System.IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern System.IntPtr GetActiveWindow();

    private void SetWindowPosition(int x, int y)
    {
        System.IntPtr hWnd = GetActiveWindow();
        MoveWindow(hWnd, x, y, Screen.width / 2, Screen.height / 2, true);
    }
#else
    private void SetWindowPosition(int x, int y)
    {
        Debug.LogWarning("SetWindowPosition is not implemented for this platform.");
    }
#endif
#endif //UNITY EDITOR
}
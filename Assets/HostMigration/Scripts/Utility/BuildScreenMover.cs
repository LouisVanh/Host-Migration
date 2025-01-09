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

    public async void SetWindowToQuarterScreen()
    {
        // Define a constant window size (quarter of a 1920x1080 screen)
        int windowWidth = 640;
        int windowHeight = 360;
        int padding = 250;
        // Set the windowed mode and size
        Screen.SetResolution(windowWidth, windowHeight, false);
        // Check how many instances of the program are running
        int instanceIndex = GetInstanceIndex();

        // Determine the window position based on instance index
        int x = 0;
        int y = 0;

        switch (instanceIndex%4)
        {
            case 0: // Top-left
                x = 0;
                y = 0;
                break;
            case 1: // Top-right
                x = windowWidth + padding + padding;
                y = 0;
                break;
            case 2: // Bottom-left
                x = 0;
                y = windowHeight + padding;
                break;
            case 3: // Bottom-right
                x = windowWidth + padding + padding;
                y = windowHeight + padding;
                break;
            default: // If more than 4 instances, stack them at (0,0)
                x = 0;
                y = 0;
                break;
        }

        await System.Threading.Tasks.Task.Delay(3000);
        // Move the window to the specified position
        SetWindowPosition(x, y, windowWidth, windowHeight);
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

    private void SetWindowPosition(int x, int y, int width, int height)
    {
        System.IntPtr hWnd = GetActiveWindow();
        MoveWindow(hWnd, x, y, width, height, true);
    }
#else
    private void SetWindowPosition(int x, int y)
    {
        Debug.LogWarning("SetWindowPosition is not implemented for this platform.");
    }
#endif
#endif //UNITY_EDITOR
}

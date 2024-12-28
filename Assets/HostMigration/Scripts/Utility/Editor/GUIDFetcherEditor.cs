using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GUIDFetcher))]
public class GUIDFetcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button to the inspector
        GUIDFetcher fetcher = (GUIDFetcher)target;
        if (GUILayout.Button("Print GUID"))
        {
            fetcher.PrintGUID();
        }
    }
}

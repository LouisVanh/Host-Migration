using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssetIdFetcher))]
public class AssetIdFetcherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button
        AssetIdFetcher fetcher = (AssetIdFetcher)target;
        if (GUILayout.Button("Print AssetId"))
        {
            fetcher.PrintAssetId();
        }
    }
}

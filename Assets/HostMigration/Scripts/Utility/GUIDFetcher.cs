using UnityEditor;
using UnityEngine;

public class GUIDFetcher : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Print GUID")]
    public void PrintGUID()
    {
        // Get the asset path of the prefab or prefab instance this script is attached to
        string assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gameObject);

        // Check if the GameObject is part of a prefab
        if (!string.IsNullOrEmpty(assetPath))
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            Debug.Log($"GUID for the prefab '{gameObject.name}': {guid}");
        }
        else
        {
            Debug.LogError($"The GameObject '{gameObject.name}' is not part of a prefab.");
        }
    }
#endif
}

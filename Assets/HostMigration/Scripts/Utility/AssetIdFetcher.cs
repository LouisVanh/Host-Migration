using System;
using UnityEngine;
using Mirror;

public class AssetIdFetcher : MonoBehaviour
{
    public void PrintAssetId()
    {
        // Get the NetworkIdentity component
        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();
        if (networkIdentity == null)
        {
            Debug.LogError("No NetworkIdentity component found on this GameObject.");
            return;
        }

        // Retrieve the assetId
        uint assetId = networkIdentity.assetId;

        // Print the assetId to the console
        Debug.Log($"AssetId (GUID) for '{gameObject.name}' is:   {assetId}");
    }
}

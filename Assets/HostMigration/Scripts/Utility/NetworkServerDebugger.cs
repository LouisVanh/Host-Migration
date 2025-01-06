using Mirror;
using UnityEngine;

public static class NetworkServerDebugger
{
    public static void PrintAllNetworkObjects()
    {
        if (!NetworkServer.active)
        {
            Debug.LogWarning("NetworkServer is not active. Unable to list registered NetworkIdentities.");
            return;
        }

        Debug.Log("Listing all registered NetworkIdentities:");

        foreach (var kvp in NetworkServer.spawned)
        {
            uint netId = kvp.Key;
            NetworkIdentity networkIdentity = kvp.Value;
            string objectName = networkIdentity.gameObject.name;

            Debug.Log($"NetID: {netId}, Object Name: {objectName}");
        }

        Debug.Log("Finished listing all registered NetworkIdentities.");
    }
}

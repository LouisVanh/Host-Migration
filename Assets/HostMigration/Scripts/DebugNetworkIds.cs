using System.Collections.Generic;
using Mirror;
using UnityEngine;

public static class DebugNetworkObjects
{
    [ClientRpc]
    public static void RpcLogNetworkObjects()
    {
        if (NetworkClient.active)
        {
            Debug.Log("---- NetworkServer Spawned Objects (Logged on Client) ----");
            foreach (KeyValuePair<uint, NetworkIdentity> kvp in NetworkServer.spawned)
            {
                uint netId = kvp.Key;
                GameObject obj = kvp.Value.gameObject;
                Debug.Log($"[CLIENT LOG] NetID: {netId}, GameObject Name: {obj.name}");
            }
        }
        else
        {
            Debug.LogError("This method is being called on a non-client!");
        }
    }

    [Command(requiresAuthority =false)]
    public static void CmdTriggerRpcLog()
    {
        Debug.Log("Triggering RpcLogNetworkObjects for all clients.");
        RpcLogNetworkObjects();
    }
}

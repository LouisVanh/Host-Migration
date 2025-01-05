using Mirror;
using Mirror.Examples.Tanks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClient : NetworkBehaviour
{
    //private string _timeOfJoining;
    private string _netId;

    private void Start()
    {
        _netId = "My netId is " + netIdentity.netId + " and it's currently " + DateTime.Now.ToString("HH:mm:ss");
        Debug.Log("Client started! New info: " + _netId);

        if (isServer)
        {
            //if you are server and disconnect you want to set this true so you dont initiate a rejoining phase
            MyNetworkManager.disconnectGracefully = true;
        }
        else
            MyNetworkManager.disconnectGracefully = false;

        if (!isOwned) return;

        RemakeGame();
    }

    [ClientRpc]
    public void StoreNewHostData(HostData hostData)
    {
        Debug.Log($"Sucess: Storing data: {hostData}");
        //storing new hostData just incase current host leaves
        MyNetworkManager.backUpHostData = hostData;


        //checking if this player is new host if not set false
        if (hostData.netID == netId && isLocalPlayer)
        {
            Debug.Log($"I'm the new host! Storing host data: {hostData}");
            MyNetworkManager.isNewHost = true;
        }
        else
            MyNetworkManager.isNewHost = false;
    }

    private void RemakeGame()
    {
        //Check if there is previous data if so reinitialize states to continue
        if (MyNetworkManager.myPlayerData.NeedsToHostMigrate == false)
        {
            Debug.LogWarning("No data found, returning: this is either the start of the game or HM's bugged");
            return;
        }

        Debug.Log("Data found, restoring");

        transform.SetPositionAndRotation(MyNetworkManager.myPlayerData.pos, MyNetworkManager.myPlayerData.rot);

        // Set anything on the server side of player data (health, amount of dice, ...) anything saved serverside
        SetNetIdOnServer(MyNetworkManager.myPlayerData.StartGameMessage);
        MyNetworkManager.myPlayerData.NeedsToHostMigrate = false; // (just did)
    }

    // experimentation
    [Command]
    void SetNetIdOnServer(string value)
    {
        Debug.Log("not sure why this is here");
        _netId = value;
    }

    private void OnDestroy()
    {
        // This will be called when the host leaves, on all clients.
        //when you are about to be destroyed save your data to be reused on new host
        if (isLocalPlayer)
        {
            var data = new PlayerData(transform.position, transform.rotation, _netId, shouldMigrate: true);
            Debug.LogWarning($"Player {this.name} being destroyed, trying to save {data}");
            MyNetworkManager.myPlayerData = data;
        }
    }
}
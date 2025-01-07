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
    [ReadOnly] public uint UniqueClientIdentifier;

    private void Start()
    {

        if (isServer)
        {
            //if you are server and disconnect you want to set this true so you dont initiate a rejoining phase
            MyNetworkManager.DisconnectGracefully = true;
        }
        else
        {
            MyNetworkManager.DisconnectGracefully = false;
        }

        if (!isOwned) return; // only simulate remaking game for the local player client

        //Check if there is previous data if so reinitialize states to continue
        if (MyNetworkManager.MyPlayerData.NeedsToHostMigrate == false)
        {
            // This is the start of the first game, set the unique identifier
            _netId = "My netId is " + netIdentity.netId + " and it's currently " + DateTime.Now.ToString("HH:mm:ss");
            Debug.Log("Client started! New startup info: " + _netId);
            UniqueClientIdProvider.Instance.CmdRequestNewClientId(this.netIdentity.netId);
            Debug.Log("Client started! New UCID: " + UniqueClientIdentifier);

            Debug.LogWarning("No data found, returning: this is either the start of the game or HM's bugged");
            return;
        }
        RemakeGame();
    }

    [ClientRpc]
    public void StoreNewHostData(HostConnectionData hostData)
    {
        Debug.Log($"Sucess: Storing data: {hostData}");
        //storing new hostData just incase current host leaves
        MyNetworkManager.BackUpHostConnectionData = hostData;


        //checking if this player is new host if not set false
        if (hostData.FutureHostNetId == netId && isLocalPlayer)
        {
            Debug.Log($"I'm the new host! Storing host data: {hostData}");
            MyNetworkManager.IsNewHost = true;
        }
        else
            MyNetworkManager.IsNewHost = false;
    }

    private void RemakeGame()
    {
        Debug.Log("Data found, restoring");

        transform.SetPositionAndRotation(MyNetworkManager.MyPlayerData.Position, MyNetworkManager.MyPlayerData.Rotation);

        // Set anything on the server side of player data (health, amount of dice, ...) anything saved serverside
        SetNetIdOnServer(MyNetworkManager.MyPlayerData.StartGameMessage);
        MyNetworkManager.MyPlayerData.NeedsToHostMigrate = false; // (just did)
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
            MyNetworkManager.MyPlayerData = data;
        }
    }
}
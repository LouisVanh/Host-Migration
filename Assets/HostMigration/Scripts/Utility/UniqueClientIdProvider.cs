using UnityEngine;
using Mirror;
using Steamworks;

public class UniqueClientIdProvider : NetworkBehaviour
{
    #region Singleton
    public static UniqueClientIdProvider Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }
    #endregion Singleton

    private uint _lastIdProvided;
    public static MyClient FindClientByUCID(uint ucid)
    {
        // Find all objects in the scene with this component (will all be players, which are registered)
        var players = PlayersManager.Instance.Players;
        // Loop through them until you find the one with this UCID
        foreach (var player in players)
        {
            if (NetworkServer.spawned.TryGetValue(player, out NetworkIdentity playerId))
            {
                var client = playerId.GetComponent<MyClient>();
                if (client.UniqueClientIdentifier == ucid)
                {
                    //Debug.Log("Found the client with ucid" + ucid);
                    return client;
                }
            }
        }
        Debug.LogWarning("No client found with ucid" + ucid);
        return null;
    }

    #region First time joining
    [Command(requiresAuthority = false)]
    public void CmdRequestNewClientId(NetworkConnectionToClient sender = null)
    {
        // Always return a new id, so it stays unique across host migrations
        _lastIdProvided++;
        Debug.Log("Provided new client id: " + _lastIdProvided);
        if (NetworkServer.spawned.TryGetValue(sender.identity.netId, out NetworkIdentity playerId))
        {
            RpcSendClientId(playerId.GetComponent<MyClient>(), _lastIdProvided, isBenchmarking: false);
        }
    }
    #endregion First time joining
    #region Post migration
    [Command(requiresAuthority = false)] // For post migration:
    public async void CmdMakeSureEveryoneKnowsMyUCID(MyClient client, uint ucid)
    {
        await System.Threading.Tasks.Task.Delay(1000);
        if (ucid == 0)
        {
            Debug.LogWarning($"{client} tried to send empty UCID. Returning.");
            return;
        }
        RpcSendClientId(client, ucid, isBenchmarking: true);
    }

    [ClientRpc] // For post migration:
    public void RpcSendClientId(MyClient client, uint ucid, bool isBenchmarking)
    {
        Debug.Log("Received UCID: '" + ucid + "' for player " + client.netId + "... will assign color soon");
        Debug.Assert(client != null);
        Debug.Assert(ucid != 0);
        client.UniqueClientIdentifier = ucid;
        AssignColorByClient(client);

        // Benchmarking: Track results of client-side data retrieval on all clients
        if (!isBenchmarking) return;
        if (NetworkClient.localPlayer.GetComponent<MyClient>() == client)
        {
            BenchmarkManager.StopBenchmark(BenchmarkManager.MethodClientStopWatch);
        }
    }
    #endregion Post Migration
    #region On player joining
    [Server]
    public void EveryoneSyncYourUCID(NetworkConnectionToClient newPlayer) // Getting our hands dirty because syncvars are reset upon migration (Called OnServerAddPlayer)
    {
        // Unfortunately, passing newPlayer through a ClientRpc gives an error. Bandage fix:
        var newPlayerNetId = newPlayer.identity.netId;
        // This needs to a) ClientRPC to all players
        Debug.LogWarning("Notifying everyone about the new player to send UCIDs");
        RpcNotifyAllClientsToSendUCID(newPlayerNetId);
    }

    [ClientRpc]
    private void RpcNotifyAllClientsToSendUCID(uint newPlayerNetId)
    {
        // This rpc should get the player's Client and UCID, and call a command
        // Don't send to yourself when joining ofcourse
        if (NetworkClient.localPlayer.netId == newPlayerNetId) return;
        var client = NetworkClient.localPlayer.GetComponent<MyClient>();
        var ucid = client.UniqueClientIdentifier;
        if (client == null || ucid == 0)
        {
            Debug.LogWarning("UCID not initialised yet. Skipping");
            return;
        }
        Debug.LogWarning("UCID succesfully retrieved, Cmding it over");
        CmdSendOwnUCIDToNewPlayer(newPlayerNetId, client, ucid);
    }

    [Command(requiresAuthority = false)]
    private void CmdSendOwnUCIDToNewPlayer(uint newPlayerNetId, MyClient client, uint ucid)
    {
        // This needs to b) TargetRPC to the new player, setting the UCID and color
        if (NetworkServer.spawned.TryGetValue(newPlayerNetId, out NetworkIdentity newPlayerId))
        {
            Debug.Log("Success! Player id found");
            RpcSendOwnUCIDToNewPlayer(newPlayer: newPlayerId.connectionToClient, client, ucid);
        }
        else Debug.LogWarning("Couldn't find that player id: " + newPlayerNetId);
    }

    [TargetRpc]
    private void RpcSendOwnUCIDToNewPlayer(NetworkConnectionToClient newPlayer, MyClient client, uint ucid)
    {
        Debug.LogWarning("Everything succesfully retrieved!");
        Debug.Assert(client != null);
        Debug.Assert(ucid != 0);
        client.UniqueClientIdentifier = ucid;
        AssignColorByClient(client);
    }
    #endregion On player joining
    #region Colour
    public static void AssignColorByUCID(uint ucid)
    {
        var client = FindClientByUCID(ucid);
        client.Color = ucid switch
        {
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.black,
            5 => Color.magenta,
            _ => throw new System.NotImplementedException(),
        };
    }

    public static void AssignColorByClient(MyClient client)
    {
        client.Color = client.UniqueClientIdentifier switch
        {
            1 => Color.red,
            2 => Color.blue,
            3 => Color.green,
            4 => Color.black,
            5 => Color.magenta,
            _ => throw new System.NotImplementedException(),
        };
        Debug.Log($"Assigned color to {client}");
    }
    #endregion colour
    #region Steam
    [TargetRpc]
    public void GetSteamIdFromPlayerAndSetAsFutureHost(NetworkConnectionToClient conn)
    {
        Debug.LogWarning("Sending over message to set future steam host");
        var steamId = SteamUser.GetSteamID();
        var nextHostNetId = NetworkClient.localPlayer.netId;
        CmdSendRpcToUpdateFutureHostSteamId(steamId, nextHostNetId);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendRpcToUpdateFutureHostSteamId(CSteamID steamId, uint nextHostNetId)
    {
        if (NetworkServer.spawned.TryGetValue(nextHostNetId, out NetworkIdentity hostNetId))
        {
            MyClient nextHost = hostNetId.GetComponent<MyClient>();
            HostConnectionData newHostData = new HostConnectionData(steamId.ToString(), nextHost.netId);

            var lobbyId = SteamLobby.SteamLobbyId;
            Debug.Log($"[lid:{lobbyId}] Trying to store new STEAM host data: " + newHostData);
            SteamMatchmaking.SetLobbyData(lobbyId, SteamLobby.HostAddressKey, steamId.ToString());
            nextHost.StoreNewHostData(newHostData);
        }
        else Debug.LogError("Tried to set steam lobby data for future host, but future host isn't spawned...");
    }
    #endregion
}
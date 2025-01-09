using UnityEngine;
using Mirror;

public class UniqueClientIdProvider : NetworkBehaviour
{
    public static UniqueClientIdProvider Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    private uint _lastIdProvided;
    private NetworkConnectionToClient _cachedNetworkConnectionToClient; // This is horrible.

    [Command(requiresAuthority = false)]
    public void CmdRequestNewClientId(NetworkConnectionToClient sender = null)
    {
        // Always return a new id, so it stays unique across host migrations
        _lastIdProvided++;
        Debug.Log("Provided new client id: " + _lastIdProvided);
        if (NetworkServer.spawned.TryGetValue(sender.identity.netId, out NetworkIdentity playerId))
        {
            RpcSendClientId(playerId.GetComponent<MyClient>(), _lastIdProvided);
        }
    }

    #region Post migration
    [Command (requiresAuthority = false)] // For post migration:
    public async void CmdMakeSureEveryoneKnowsMyUCID(MyClient client, uint ucid)
    {
        await System.Threading.Tasks.Task.Delay(1000); 
        if(ucid == 0)
        {
            Debug.LogWarning($"{client} tried to send empty UCID. Returning.");
            return;
        }
        RpcSendClientId(client, ucid);
    }

    [ClientRpc] // For post migration:
    public void RpcSendClientId(MyClient client, uint ucid)
    {
        Debug.Log("Received UCID: '" + ucid + "' for player " + client.netId + "... will assign color soon");
        Debug.Assert(client != null);
        Debug.Assert(ucid != 0);
        client.UniqueClientIdentifier = ucid;
        AssignColorByClient(client);
    }
    #endregion Post Migration

    #region On player joining
    [Command(requiresAuthority =false)]
    public void CmdEveryoneSyncYourUCID(NetworkConnectionToClient newPlayer) // Getting our hands dirty because syncvars are reset upon migration
    {
        // This command is called OnServerAddPlayer.
        // Unfortunately, passing newPlayer through a ClientRpc gives an error. Bandage fix:
        _cachedNetworkConnectionToClient = newPlayer;
        // This needs to a) ClientRPC to all players
        RpcNotifyAllClientsToSendUCID();
    }

    [ClientRpc]
    private void RpcNotifyAllClientsToSendUCID()
    {
        // That rpc should get the player's Client and UCID, and call a command
        var client = NetworkClient.localPlayer.GetComponent<MyClient>();
        var ucid = client.UniqueClientIdentifier;
        CmdSendOwnUCIDToNewPlayer(client, ucid);
    }

    [Command(requiresAuthority =false)]
    private void CmdSendOwnUCIDToNewPlayer(MyClient client, uint ucid)
    {
        // This needs to b) TargetRPC to the new player, setting the UCID and color
        RpcSendOwnUCIDToNewPlayer(newPlayer: _cachedNetworkConnectionToClient, client, ucid);
    } 

    [TargetRpc]
    private void RpcSendOwnUCIDToNewPlayer(NetworkConnectionToClient newPlayer, MyClient client, uint ucid)
    {
        Debug.Assert(client != null);
        Debug.Assert(ucid != 0);
        client.UniqueClientIdentifier = ucid;
        AssignColorByClient(client);
    }
    #endregion On player joining

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
                    Debug.Log("Found the client with ucid" + ucid);
                    return client;
                }
            }
        }
        Debug.LogWarning("No client found with ucid" + ucid);
        return null;
    }

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
}
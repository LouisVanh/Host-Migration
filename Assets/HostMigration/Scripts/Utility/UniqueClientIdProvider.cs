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

    [Command(requiresAuthority = false)]
    public void CmdRequestNewClientId(NetworkConnectionToClient sender = null)
    {
        //await System.Threading.Tasks.Task.Delay(150);
        // Always return a new id, so it stays unique across host migrations
        _lastIdProvided++;
        Debug.Log("Provided new client id: " + _lastIdProvided);
        //await System.Threading.Tasks.Task.Delay(1500);
        if (NetworkServer.spawned.TryGetValue(sender.identity.netId, out NetworkIdentity playerId))
        {
            RpcSendClientId(playerId.GetComponent<MyClient>(), _lastIdProvided);
        }
        else
        {
            Debug.Log("ATTEMPTING TO DEBUG THIS BULLSHIT");
            NetworkServerDebugger.PrintAllNetworkObjects();
        }
    }

    [ClientRpc]
    public void RpcSendClientId(MyClient client, uint ucid)
    {
        Debug.Log("Received UCID: '" + ucid + "' for player " + client.netId + "... will assign color soon");
        Debug.Assert(client != null);
        Debug.Assert(ucid != 0);
        client.UniqueClientIdentifier = ucid;
        AssignColorByClient(client);
    }

    [Command (requiresAuthority = false)]
    public async void CmdMakeSureEveryoneKnowsMyUCID(MyClient client)
    {
        await System.Threading.Tasks.Task.Delay(3000); 
        if(client.UniqueClientIdentifier == 0)
        {
            Debug.LogWarning($"{client} tried to send empty UCID. Returning.");
            return;
        }
        RpcSendClientId(client, client.UniqueClientIdentifier);
    }

    [ClientRpc] // Called from server
    public void RpcSendNewPlayerMyUCID()
    {
        var client = NetworkClient.localPlayer.GetComponent<MyClient>();
        Debug.Log("RpcSendNewPlayerMyUCID: trying to send" + client);
        if(client.UniqueClientIdentifier == 0)
        {
            Debug.LogWarning("client's UCID was 0, returning from SendNewPlayerMyUCID");
            return;
        }
        CmdMakeSureEveryoneKnowsMyUCID(client);
    }

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
        Debug.Log($"Assigned color to {client}: {client.Color}");
    }
    #endregion colour
}
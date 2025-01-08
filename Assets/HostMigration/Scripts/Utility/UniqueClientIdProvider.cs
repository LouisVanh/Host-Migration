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
        // Always return a new id, so it stays unique across host migrations
        _lastIdProvided++;
        Debug.Log("Provided new client id: " + _lastIdProvided);
        sender.identity.GetComponent<MyClient>().UniqueClientIdentifier = _lastIdProvided;
    }

    //[TargetRpc]
    //private void RpcSendNewClientId(NetworkConnectionToClient conn, uint id)
    //{
    //    Debug.Log("Received UCID:" + id);
    //    NetworkClient.localPlayer.GetComponent<MyClient>().UniqueClientIdentifier = id;
    //}

    public static MyClient FindClientByUCID(uint ucid)
    {
        // Find all objects in the scene with this component (will all be players, which are registered)
        var players = PlayersManager.Instance.Players;
        // Loop through them until you find the one with this UCID
        foreach (var player in players)
        {
            if(NetworkServer.spawned.TryGetValue(player, out NetworkIdentity playerId))
            {
                var client = playerId.GetComponent<MyClient>();
                if(client.UniqueClientIdentifier == ucid)
                {
                    Debug.Log("Found the client with ucid" + ucid);
                    return client;
                }    
            }
        }
        Debug.LogWarning("No client found with ucid" + ucid);
        return null;
    }
}
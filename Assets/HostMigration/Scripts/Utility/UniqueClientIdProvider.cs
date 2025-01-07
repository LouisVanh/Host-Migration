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
        RpcSendNewClientId(sender, _lastIdProvided);
    }

    [TargetRpc]
    public void RpcSendNewClientId(NetworkConnection conn, uint id)
    {
        Debug.Log("Received UCID:" + id);
        NetworkClient.localPlayer.GetComponent<MyClient>().UniqueClientIdentifier = id;
    }
}
using UnityEngine;
using Mirror;

public class PlayersManager : NetworkBehaviour
{
    /* README: ENSURE YOUR PLAYER HAS THIS OR IT WILL DO NOTHING
     * 
    private void OnDestroy()
    {
        if (isServer)
        {
            PlayersManager.Instance.RemovePlayer(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command]
    private void CmdRegisterPlayer() // Call this on Start, make sure to check for isLocalPlayer
    {
        PlayersManager.Instance.AddPlayer(gameObject.GetComponent<NetworkIdentity>().netId);
    }
    *
    */


    public static PlayersManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public readonly SyncList<uint> Players = new SyncList<uint>();

    [Server]
    public void AddPlayer(uint netId)
    {
        if (!Players.Contains(netId))
        {
            Players.Add(netId);
            Debug.Log($"Server: Added player with NetID {netId}");
        }
    }

    [Server]
    public void RemovePlayer(uint netId)
    {
        if (Players.Contains(netId))
        {
            Players.Remove(netId);
            Debug.Log($"Server: Removed player with NetID {netId}");
        }
    }

    public override void OnStartClient()
    {
        // Add handlers for SyncList Actions
        Players.OnAdd += OnPlayerAdded;
        Players.OnInsert += OnPlayerInserted;
        Players.OnSet += OnPlayerChanged;
        Players.OnRemove += OnPlayerRemoved;
        Players.OnClear += OnPlayerListCleared;

        // List is populated before handlers are wired up so we
        // need to manually invoke OnAdd for each element.
        for (int i = 0; i < Players.Count; i++)
            Players.OnAdd.Invoke(i);
    }

    public override void OnStopClient()
    {
        // Remove handlers when client stops
        Players.OnAdd -= OnPlayerAdded;
        Players.OnInsert -= OnPlayerInserted;
        Players.OnSet -= OnPlayerChanged;
        Players.OnRemove -= OnPlayerRemoved;
        Players.OnClear -= OnPlayerListCleared;
    }

    void OnPlayerAdded(int index)
    {
        Debug.Log($"Element added at index {index} {Players[index]}");
        if (NetworkServer.spawned.TryGetValue(Players[index], out NetworkIdentity playerId))
        {
            playerId.GetComponent<Player>().PlayerScreenPosition = (PlayerPosition)index;
        }
    }

    void OnPlayerInserted(int index)
    {
        Debug.Log($"Player inserted at index {index} {Players[index]}");
    }

    void OnPlayerChanged(int index, uint oldValue)
    {
        Debug.Log($"Player changed at index {index} from {oldValue} to {Players[index]}");
    }

    void OnPlayerRemoved(int index, uint oldValue)
    {
        Debug.Log($"Player removed at index {index} {oldValue}");
    }

    void OnPlayerListCleared()
    {
        // OnListCleared is called before the list is actually cleared
        // so we can iterate the list to get the elements if needed.
        foreach (uint id in Players)
        {
            if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
                Debug.Log($"Player cleared {objNetIdentity.transform.name}");
        }
    }
}
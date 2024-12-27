using UnityEngine;
using Mirror;

public class PlayersManager : NetworkBehaviour
{

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
        Players.OnAdd += OnItemAdded;
        Players.OnInsert += OnItemInserted;
        Players.OnSet += OnItemChanged;
        Players.OnRemove += OnItemRemoved;
        Players.OnClear += OnListCleared;

        // List is populated before handlers are wired up so we
        // need to manually invoke OnAdd for each element.
        for (int i = 0; i < Players.Count; i++)
            Players.OnAdd.Invoke(i);
    }

    public override void OnStopClient()
    {
        // Remove handlers when client stops
        Players.OnAdd -= OnItemAdded;
        Players.OnInsert -= OnItemInserted;
        Players.OnSet -= OnItemChanged;
        Players.OnRemove -= OnItemRemoved;
        Players.OnClear -= OnListCleared;
    }

    void OnItemAdded(int index)
    {
        Debug.Log($"Element added at index {index} {Players[index]}");
    }

    void OnItemInserted(int index)
    {
        Debug.Log($"Element inserted at index {index} {Players[index]}");
    }

    void OnItemChanged(int index, uint oldValue)
    {
        Debug.Log($"Element changed at index {index} from {oldValue} to {Players[index]}");
    }

    void OnItemRemoved(int index, uint oldValue)
    {
        Debug.Log($"Element removed at index {index} {oldValue}");
    }

    void OnListCleared()
    {
        // OnListCleared is called before the list is actually cleared
        // so we can iterate the list to get the elements if needed.
        foreach (uint id in Players)
        {
        if (NetworkClient.spawned.TryGetValue(id, out NetworkIdentity objNetIdentity))
            Debug.Log($"Element cleared {objNetIdentity.transform.name}");
        }
    }
}
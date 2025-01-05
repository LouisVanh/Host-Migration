using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class PlayersManager : NetworkBehaviour
{
    // Make sure the player has a "PlayerRegistering" component
    public static PlayersManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public List<Player> GetPlayers()
    {
        List<Player> playerList = new();
        foreach (uint playerId in Players)
        {
            if (NetworkClient.spawned.TryGetValue(playerId, out NetworkIdentity playerObj))
                playerList.Add(playerObj.GetComponent<Player>());
        }
        return playerList;
    }

    public List<Player> GetAlivePlayers()
    {
        List<Player> playerList = new();
        foreach (uint playerId in Players)
        {
            if (NetworkClient.spawned.TryGetValue(playerId, out NetworkIdentity playerObj))
            {
                if (playerObj.GetComponent<Player>().IsAlive)
                    playerList.Add(playerObj.GetComponent<Player>());
            }
        }
        return playerList;
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
        Players.OnAdd += SetBackupHost;
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
        Players.OnAdd -= SetBackupHost;
        Players.OnInsert -= OnPlayerInserted;
        Players.OnSet -= OnPlayerChanged;
        Players.OnRemove -= OnPlayerRemoved;
        Players.OnClear -= OnPlayerListCleared;
    }

    void SetBackupHost(int index)
    {
        if (isServer)
        {
            Debug.Log("I'm the server, setting backuphost for everyone now");
            HostMigrationData.Instance.TrySetBackUpHost("localhost", HostMigrationData.GetNextHost());
        }
    }

    void OnPlayerAdded(int index)
    {
        Debug.Log($"Element added at index {index} of players list:{Players[index]}");
        if (MyNetworkManager.singleton.IsDebugging) return;
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
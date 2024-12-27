using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayersManager : NetworkBehaviour
{
    public static PlayersManager Instance;

    // A synchronized list of players
    private readonly SyncList<GameObject> _players = new SyncList<GameObject>();

    // Publicly accessible list of players
    public IReadOnlyList<GameObject> Players => _players;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Called when a player joins the game
    [ServerCallback]
    public void AddPlayer(GameObject player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
            RpcPlayerListUpdated();
        }
    }

    // Called when a player leaves the game
    [ServerCallback]
    public void RemovePlayer(GameObject player)
    {
        if (_players.Contains(player))
        {
            _players.Remove(player);
            RpcPlayerListUpdated();
        }
    }

    // Notify clients that the player list has updated
    [ClientRpc]
    private void RpcPlayerListUpdated()
    {
        Debug.Log($"Player list updated: {_players.Count} players connected.");
        foreach (var player in _players)
        {
            Debug.Log($"Player: {player.name}");
        }
    }
}
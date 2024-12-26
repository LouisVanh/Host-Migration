using UnityEngine;
using Mirror;
using System.Collections.Generic;

public enum GameState
{
    WaitingLobby,
    PreDiceReceived,
    EveryoneRollingTime,
    EveryoneJustRolled,
    AfterRollDamageEnemy,
    AfterRollEnemyAttack,
    EveryonePickBooster
}

// **TurnManager.cs**
public class TurnManager : NetworkBehaviour // SERVER ONLY CLASS (ONLY RUN EVERYTHING ONCE)
{
    public GameState CurrentGameState;
    private Dictionary<NetworkConnectionToClient, Player> _players = new Dictionary<NetworkConnectionToClient, Player>();
    private int _currentDiceRoll;
    private Enemy _currentEnemy;

    public int TurnCount;
    public static TurnManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    [Server]
    public void GiveAllPlayersDice()
    {
        if (CurrentGameState != GameState.PreDiceReceived) return;

        foreach (var player in _players)
        {
            GivePlayerDice(player.Key);
        }
        TurnCount++;
    }

    [TargetRpc]
    private void GivePlayerDice(NetworkConnectionToClient playerConnection)
    {
        // Get how many dice the player should have from the Player class attached to the gameobject with that netId
        if (_players.TryGetValue(playerConnection, out Player player))
        {
            var diceCount = player.DiceCount;
            player.ReceiveDice(diceCount); // Assuming a method to handle dice assignment
        }
    }

    public Player GetRandomDeadPlayer()
    {
        // return a random dead player (through player.IsDead prop)
        List<Player> deadPlayers = new List<Player>();

        foreach (var player in _players.Values)
        {
            if (player.IsDead)
            {
                deadPlayers.Add(player);
            }
        }

        // If there are no dead players, return null
        if (deadPlayers.Count == 0)
        {
            return null;
        }

        // Return a random dead player from the list
        int randomIndex = Random.Range(0, deadPlayers.Count);
        return deadPlayers[randomIndex];
    }

    [Server]
    public void UpdateGameState(GameState newState)
    {
        CurrentGameState = newState;
        // Handle state transitions
        switch (newState)
        {
            case GameState.WaitingLobby:

                break;

            case GameState.PreDiceReceived:
                // Show the right screen
                // Explain anything if it's the first time
                WaveManager.Instance.CreateNewWave();
                //When done waiting for a few seconds, switch to the next
                break;

            case GameState.EveryoneRollingTime:
                // Give people their dice
                GiveAllPlayersDice();

                // When all players are done rolling, switch to the next
                break;

            case GameState.EveryoneJustRolled:
                // Count up the dice
                _currentDiceRoll = CountTotalRollAmount();
                _currentEnemy.TakeDamage(_currentDiceRoll);
                break;
            case GameState.AfterRollDamageEnemy:
                _currentDiceRoll = 0;

                break;
            case GameState.AfterRollEnemyAttack:
                break;
            case GameState.EveryonePickBooster:
                break;
            default:
                break;
        }
    }
    [Server]
    private int CountTotalRollAmount()
    {
        var total = 0;
        foreach (var player in _players)
        {
            total += player.Value.DiceCount;
        }
        return total;
    }
}

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

    private List<Player> GetPlayers()
    {
        List<Player> playerList = new();
        foreach (uint playerId in PlayersManager.Instance.Players)
        {
            if (NetworkClient.spawned.TryGetValue(playerId, out NetworkIdentity playerObj))
                playerList.Add(playerObj.GetComponent<Player>());
        }
        return playerList;
    }

    [Server]
    public void GiveAllPlayersDice()
    {
        if (CurrentGameState != GameState.PreDiceReceived) return;

        foreach (var player in GetPlayers())
        {
            GivePlayerDice(player);
        }
        TurnCount++;
    }

    [TargetRpc]
    private void GivePlayerDice(Player player)
    {
            var diceCount = player.DiceCount;
            player.ReceiveDice(diceCount);
    }

    public Player GetRandomDeadPlayer()
    {
        // return a random dead player (through player.IsDead prop)
        List<Player> deadPlayers = new List<Player>();

        foreach (var player in GetPlayers())
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
                // Healthbar shit
                RpcGivePlayersHealthBars();
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
        foreach (var player in GetPlayers())
        {
            total += player.DiceCount;
        }
        return total;
    }

    [ClientRpc]
    private void RpcGivePlayersHealthBars()
    {
        foreach (var player in GetPlayers())
        {
            Debug.Log(player.name + " is the player Turn Manager is giving a healthbar to now!");
            player.CreatePlayerHealthBar();
        }
    }
}

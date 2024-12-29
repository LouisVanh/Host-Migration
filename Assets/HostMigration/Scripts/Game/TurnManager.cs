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

    public int _turnCount;
    public bool FirstRoundPlaying => _turnCount < 1;
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
        Debug.Log("GiveAAllPlayerDice called but not yet inside");
        //if (CurrentGameState != GameState.PreDiceReceived) return;
        Debug.Log("GiveAAllPlayerDice called inside");

        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            GivePlayerDice(player);
        }
        _turnCount++;
    }

    [ClientRpc]
    private void GivePlayerDice(Player player)
    {
        Debug.Log("GivePlayerDice called");
        var diceCount = player.DiceCount;
        player.ReceiveDice(diceCount);
    }

    public Player GetRandomDeadPlayer()
    {
        // return a random dead player (through player.IsDead prop)
        List<Player> deadPlayers = new List<Player>();

        foreach (var player in PlayersManager.Instance.GetPlayers())
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
                if (FirstRoundPlaying)
                {
                    RpcGivePlayersHealthBars();
                    WaveManager.Instance.CreateNewWave();
                }
                //When done waiting for a few seconds, switch to the next
                break;

            case GameState.EveryoneRollingTime:
                // Give people their dice
                GiveAllPlayersDice();
                UIManager.Instance.UpdateUIState(ScreenState.EveryoneRollingTime);
                // When all players are done rolling, switch to the next
                break;

            case GameState.EveryoneJustRolled:
                // Count up the dice
                _currentDiceRoll = CountTotalRollAmount();
                UIManager.Instance.UpdateUIState(ScreenState.EveryoneJustRolled);
                // UI ANIMATION will call to next
                break;
            case GameState.AfterRollDamageEnemy:
                _currentEnemy.TakeDamage(_currentDiceRoll);
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
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            total += player.DiceCount;
        }
        return total;
    }

    [ClientRpc]
    private void RpcGivePlayersHealthBars()
    {
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            Debug.Log(player.name + " is the player Turn Manager is giving a healthbar to now!");
            player.CreatePlayerHealthBar();
        }
    }
}

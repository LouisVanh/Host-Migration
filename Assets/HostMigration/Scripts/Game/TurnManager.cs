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
    LeftOverDamageToEnemy,
    EveryonePickBooster
}

// **TurnManager.cs**
public class TurnManager : NetworkBehaviour // SERVER ONLY CLASS (ONLY RUN EVERYTHING ONCE)
{
    public GameState CurrentGameState;
    public int CurrentDiceRoll;

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
    [ClientRpc]
    private void SetSyncedUIState(ScreenState newState)
    {
        UIManager.Instance.UpdateUIState(newState);
    }
    [Server]
    public async void UpdateGameState(GameState newState)
    {
        CurrentGameState = newState;
        // Handle state transitions
        switch (newState)
        {
            case GameState.WaitingLobby:

                break;

            case GameState.PreDiceReceived:
                // Show the right screen
                SetSyncedUIState(ScreenState.PreDiceReceived);

                // Explain anything if it's the first time
                if (FirstRoundPlaying)
                {
                    RpcGivePlayersHealthBars();
                    WaveManager.Instance.CreateNewWave();
                }
                //When done waiting for a few seconds, switch to the next
                break;

            case GameState.EveryoneRollingTime:                 // UI ANIMATION will call to this
                // Give people their dice
                GiveAllPlayersDice();
                SetSyncedUIState(ScreenState.EveryoneRollingTime);

                // When all players are done rolling, switch to the next
                break;

            case GameState.EveryoneJustRolled:
                // Count up the dice
                CurrentDiceRoll = DiceManager.Instance.CountTotalRollAmount();
                SetSyncedUIState(ScreenState.EveryoneJustRolled);

                break;

            case GameState.AfterRollDamageEnemy:                 // UI ANIMATION will call to this
                await HandleEnemyDamage(CurrentDiceRoll);
                break;

            case GameState.LeftOverDamageToEnemy:
                await System.Threading.Tasks.Task.Delay(1500); // Waiting for enemy to spawn again
                await HandleEnemyDamage(DiceManager.Instance.LeftOverEyesFromLastRoll);
                break;

            case GameState.AfterRollEnemyAttack:
                SetSyncedUIState(ScreenState.AfterRollEnemyAttack);

                // This could be randomised later...
                WaveManager.Instance.CurrentEnemy.EnemyAttackDealDamage(2);
                await System.Threading.Tasks.Task.Delay(500);
                DiceManager.Instance.RemoveAllDice();

                UpdateGameState(GameState.PreDiceReceived);
                break;

            case GameState.EveryonePickBooster:
                SetSyncedUIState(ScreenState.EveryonePickBooster);
                break;

            default:
                break;
        }
    }
    private async System.Threading.Tasks.Task HandleEnemyDamage(int damage)
    {
        Debug.Log($"TURNMANAGER / Current Damage: {damage}");
        var enemyToDamage = WaveManager.Instance.CurrentEnemy;
        bool wasEnemyBoss = WaveManager.Instance.CurrentWave.CurrentEnemyIndex == WaveManager.Instance.CurrentWave.TotalEnemiesCount;
        // This could kill the enemy, giving leftovers. This could also kill the boss, which would send you to the booster picking state.
        enemyToDamage.TakeDamage(damage, out int leftOverEyes, out bool enemyDied);
        DiceManager.Instance.LeftOverEyesFromLastRoll = leftOverEyes;
        await System.Threading.Tasks.Task.Delay(500); // Don't instantly switch after damage gets removed, let players look at it.
        if (enemyDied && wasEnemyBoss)
        {
            return; // WavesManager handles this.
        }
        if (enemyDied && !wasEnemyBoss)
        {
            if (leftOverEyes != 0)
            {
                UpdateGameState(GameState.LeftOverDamageToEnemy);
            }
            else
            {
                UpdateGameState(GameState.PreDiceReceived);
            }
        }
        else // enemy survived, he will attack now.
        {
            UpdateGameState(GameState.AfterRollEnemyAttack);
        }
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

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
    EveryonePickBooster,
    NewWaveEveryonePickedBooster,
    EndGame
}

// **TurnManager.cs**
public class TurnManager : NetworkBehaviour // SERVER ONLY CLASS (ONLY RUN EVERYTHING ONCE)
{
    public GameState CurrentGameState;
    public int CurrentDiceRoll;

    public int _turnCount;
    public bool FirstTurnPlaying => _turnCount < 1;
    public bool FirstWavePlaying => WaveManager.Instance.CurrentWaveIndex == 1;
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
        foreach (var player in PlayersManager.Instance.GetAlivePlayers())
        {
            GivePlayerDice(player);
        }
        _turnCount++;
    }

    [ClientRpc]
    private void GivePlayerDice(Player player)
    {
        //Debug.Log("GivePlayerDice called");
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
        Debug.LogWarning($"TURN / NEW GAME STATE / " + newState);
        // Handle state transitions
        switch (newState)
        {
            case GameState.WaitingLobby:

                break;

            case GameState.PreDiceReceived:
                // Show the right screen
                SetSyncedUIState(ScreenState.PreDiceReceived);

                //Remove any leftover dice from last turns
                DiceManager.Instance.RemoveAllDice();

                // Explain anything if it's the first time
                if (FirstTurnPlaying)
                {
                    RpcGivePlayersHealthBars();
                    WaveManager.Instance.CreateNewWave(3);
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
                await HandleEnemyDamage(CurrentDiceRoll, false);
                break;

            case GameState.LeftOverDamageToEnemy:
                await System.Threading.Tasks.Task.Delay(1500); // Waiting for enemy to spawn again
                await HandleEnemyDamage(DiceManager.Instance.LeftOverEyesFromLastRoll, true);
                break;

            case GameState.AfterRollEnemyAttack:
                SetSyncedUIState(ScreenState.AfterRollEnemyAttack);

                // This could be randomised later...
                WaveManager.Instance.CurrentEnemy.EnemyAttackDealDamage(1);
                await System.Threading.Tasks.Task.Delay(1500);

                if (PlayersManager.Instance.GetAlivePlayers().Count == 0)
                {
                    // All players are dead.
                    UpdateGameState(GameState.EndGame);
                    return;
                }

                UpdateGameState(GameState.PreDiceReceived);
                break;

            case GameState.EveryonePickBooster:
                foreach(var player in PlayersManager.Instance.GetPlayers())
                {
                    player.BoosterManager.ShowPotentialBoosters();
                }
                SetSyncedUIState(ScreenState.EveryonePickBooster);
                break;

            case GameState.NewWaveEveryonePickedBooster:
                // space for UI "new wave!"
                UpdateGameState(GameState.PreDiceReceived);
                break;

            case GameState.EndGame:
                // END THE GAME
                SetSyncedUIState(ScreenState.EndOfGame);
                break;

            default:
                break;
        }
    }
    private async System.Threading.Tasks.Task HandleEnemyDamage(int damage, bool isLeftOvers)
    {
        Debug.Log($"TURNMANAGER / Current Damage: {damage} - Is this a leftover? {isLeftOvers}");
        var enemyToDamage = WaveManager.Instance.CurrentEnemy;
        bool wasEnemyBoss = WaveManager.Instance.CurrentWave.CurrentEnemyIndex == WaveManager.Instance.CurrentWave.TotalEnemiesCount;

        // This could kill the enemy, giving leftovers. This could also kill the boss, which would send you to the booster picking state.
        enemyToDamage.TakeDamage(damage, out int leftOverEyes, out bool enemyDied);
        DiceManager.Instance.LeftOverEyesFromLastRoll = leftOverEyes;
        await System.Threading.Tasks.Task.Delay(1500); // Don't instantly switch after damage gets removed, let players look at it.
        if (enemyDied && wasEnemyBoss)
        {
            return; // WavesManager handles this.
        }

        if (enemyDied)
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
            if (isLeftOvers)
            {
                Debug.Log("Leftover damage done, not attacking player and going back to preDice");
                UpdateGameState(GameState.PreDiceReceived);
                return; // if it's leftovers, don't attack twice for no reason.}
            }

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

using UnityEngine;
using Mirror;
using System.Collections.Generic;

public enum GameState
{
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

    [Server]
    public void GiveAllPlayersDice()
    {
        if (CurrentGameState != GameState.PreDiceReceived) return;

        foreach (var player in _players)
        {
            GivePlayerDice(player.Key);
        }
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

    [Server]
    private void UpdateGameState(GameState newState)
    {
        CurrentGameState = newState;
        // Handle state transitions
        switch (newState)
        {
            case GameState.PreDiceReceived:
                // ADD STUFF HERE
                break;
            case GameState.EveryoneRollingTime:
                break;
            case GameState.EveryoneJustRolled:
                break;
            case GameState.AfterRollDamageEnemy:
                break;
            case GameState.AfterRollEnemyAttack:
                break;
            case GameState.EveryonePickBooster:
                break;
            default:
                break;
        }
    }

    private void CountTotalRollAmount()
    {

    }
}

using UnityEngine;

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
public class TurnManager : MonoBehaviour
{
    private GameState _currentGameState;

    public void GiveAllPlayersDice()
    {
        _currentGameState = GameState.PreDiceReceived;
        // Distribute dice to all players
    }

    private void UpdateGameState(GameState newState)
    {
        _currentGameState = newState;
        // Handle state transitions
        switch (newState)
        {
            case GameState.PreDiceReceived:
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
}

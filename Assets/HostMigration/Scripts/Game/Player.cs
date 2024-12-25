using UnityEngine;

public class Player : MonoBehaviour
{
    private int _diceCount;
    private bool _hasAlreadyRolled;
    private bool _canRoll;
    private BoostersManager _boostersManager;
    private int _health;

    public void RollDice()
    {
        if (_canRoll && !_hasAlreadyRolled)
        {
            // Perform dice roll logic
            _hasAlreadyRolled = true;
        }
    }

    public void ShakeDiceJar()
    {
        // Play dice shake animation
    }
}
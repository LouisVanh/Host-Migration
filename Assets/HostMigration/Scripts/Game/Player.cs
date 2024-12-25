using UnityEngine;
using Mirror;
using System;

public class Player : NetworkBehaviour
{
    public int DiceCount;
    private bool _hasAlreadyRolled;
    private bool _canRoll;
    private BoostersManager _boostersManager;
    private int _health;

    [SerializeField] private GameObject _dicePrefab;
    [SerializeField] private Transform _cup;

    private void Awake()
    {
        if (!isLocalPlayer) return;

        _boostersManager = this.gameObject.GetComponent<BoostersManager>();
    }
    private void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rolling dice from player");
            RollDice(this);
        }
    }
    [Command(requiresAuthority =false)]
    public void RollDice(Player player)
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

    internal void ReceiveDice(int diceCount)
    {
        Debug.Log(this.ToString() + " received " + diceCount + "dice");
        _canRoll = true;
        _hasAlreadyRolled = false;
        throw new NotImplementedException();
        // enable cup, allow for rolling
    }
}
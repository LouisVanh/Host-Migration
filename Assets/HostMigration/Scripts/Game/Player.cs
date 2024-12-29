using UnityEngine;
using Mirror;
using System;

public enum PlayerPosition { BottomLeft, BottomRight, TopLeft, TopRight, None }
public class Player : NetworkBehaviour
{
    [SyncVar]
    public bool ReadyToPlay;
    [SyncVar]
    public bool HasAlreadyRolled;
    public int DiceCount = 1;
    private bool _canRoll;
    public BoostersManager BoosterManager;
    public HealthBar HealthBar { get; private set; }
    [SerializeField] GameObject PlayerHealthBarVisual;

    public bool IsDead => HealthBar.CurrentHealth == 0;
    public int StartingHealth = 20;
    public int CurrentDiceRollAmount;

    // NETWORK TRANSFORMS
    [SerializeField] private GameObject _dicePrefab;
    [SerializeField] private GameObject _cupPrefab;
    [SerializeField] private Transform _cupPosition1;
    [SerializeField] private Transform _cupPosition2;
    [SerializeField] private Transform _cupPosition3;
    [SerializeField] private Transform _cupPosition4;

    public readonly SyncList<uint> DiceOwned = new SyncList<uint>();

    [SyncVar]
    public PlayerPosition PlayerScreenPosition;

    private void OnDestroy()
    {
        if (isServer)
        {
            PlayersManager.Instance.RemovePlayer(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdRegisterPlayer() // Call this on Start, make sure to check for isLocalPlayer
    {
        PlayersManager.Instance.AddPlayer(gameObject.GetComponent<NetworkIdentity>().netId);
    }

    public void CreatePlayerHealthBar()
    {
        HealthBar = this.GetComponent<HealthBar>();
        Debug.Log("Start setting up health bar for player " + this.gameObject.name + "!");
        HealthBar.SetupOwnHealthBar(PlayerHealthBarVisual, StartingHealth, this);
        Debug.Log("End setup health bar for player " + this.gameObject.name + "!");
    }

    private void Start()
    {
        if (isLocalPlayer) // never use localplayer in awake
        {
            CmdRegisterPlayer();

            BoosterManager = GetComponent<BoostersManager>();

            UIManager.Instance.UpdateUIState(ScreenState.WaitingLobby);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        //if (Input.GetKeyDown(KeyCode.T))
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Rolling dice from player");
                CmdRollDice();
            }

        if (Input.GetKeyDown(KeyCode.L)) CmdTestRemovePlayerHealth();
    }

    [Command(requiresAuthority = false)]
    private void CmdTestRemovePlayerHealth()
    {
        HealthBar.CurrentHealth--; // DEBUG
    }
    [Command(requiresAuthority = false)]
    public void CmdRollDice()
    {
        //WaveManager.Instance.CurrentEnemy.TakeDamage(3); // obviously for testing only
        if (_canRoll && !HasAlreadyRolled)
        {
            Debug.Log("ROLLING DICE!");
            // Perform dice roll logic
            HasAlreadyRolled = true;
            int totalRoll = 0;
            for (int i = 0; i < DiceCount; i++)
            {
                int eyes = UnityEngine.Random.Range(1, 7);
                totalRoll += eyes;
                CmdSpawnDiceWithEyes(eyes);
            }
        }
        else Debug.LogWarning($"CANT ROLL DICE! canRoll = {_canRoll}, hasAlreadyRolled = {HasAlreadyRolled}");
    }

    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(int health)
    {
        HealthBar.CurrentHealth -= health;
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnDiceWithEyes(int eyesRolled)
    {
        DiceManager.Instance.AddDice(new Dice(this.gameObject.GetComponent<NetworkIdentity>().netId, eyesRolled));
    }

    public void SpawnAndShakeDiceJar()
    {
        Vector3 pos = GetPlayerCupPosition();
        var upsideDownRotation = new Vector3(0, 180, 0);
        var quaternion = Quaternion.Euler(upsideDownRotation);
        var cup = Instantiate(_cupPrefab, pos, quaternion);
        // Play dice shake animation
    }

    public Vector3 GetPlayerCupPosition()
    {
        return (PlayerScreenPosition) switch
        {
            PlayerPosition.BottomLeft => _cupPosition1.position,
            PlayerPosition.BottomRight => _cupPosition2.position,
            PlayerPosition.TopLeft => _cupPosition3.position,
            PlayerPosition.TopRight => _cupPosition4.position,
            PlayerPosition.None => Vector3.zero,
            _ => throw new NotImplementedException()
        };
    }

    public void ReceiveDice(int diceCount)
    {
        Debug.Log(this.ToString() + " received " + diceCount + "dice");
        _canRoll = true;
        HasAlreadyRolled = false;
        // enable cup, allow for rolling
    }
}
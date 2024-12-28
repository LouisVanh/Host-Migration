using UnityEngine;
using Mirror;
using System;

public enum PlayerPosition { BottomLeft, BottomRight, TopLeft, TopRight, None }
public class Player : NetworkBehaviour
{
    [SyncVar]
    public bool ReadyToPlay;
    public int DiceCount = 1;
    private bool _hasAlreadyRolled;
    private bool _canRoll;
    private BoostersManager _boostersManager;
    public HealthBar HealthBar { get; private set; }
    [SerializeField] GameObject PlayerHealthBarVisual;
    [SerializeField] GameObject PlayerHealthBarScriptPrefab;

    public bool IsDead => HealthBar.CurrentHealth == 0;
    public int StartingHealth = 20;
    public int CurrentDiceRollAmount;

    [SerializeField] private GameObject _dicePrefab;
    [SerializeField] private Transform _cup;

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

            _boostersManager = GetComponent<BoostersManager>();

            //HandleHealthBarSetup();

            UIManager.Instance.UpdateUIState(ScreenState.WaitingLobby);
        }
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

    [Command(requiresAuthority = false)]
    public void RollDice(Player player)
    {
        WaveManager.Instance.CurrentEnemy.TakeDamage(3); // obviously for testing only
        if (_canRoll && !_hasAlreadyRolled)
        {
            // Perform dice roll logic
            _hasAlreadyRolled = true;
            int totalRoll = 0;
            for (int i = 0; i < DiceCount; i++)
            {
                int eyes = UnityEngine.Random.Range(1, 7);
                totalRoll += eyes;
                SpawnDiceWithEyes(eyes);
            }
        }
    }

    public void SpawnDiceWithEyes(int eyesRolled)
    {
        // Spawn the dice object with the amount of eyes rolled showing upwards. I'd make like 6 prefab variations tbh, too lazy.
        Debug.Log("spawning dice (unimplemented)");
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
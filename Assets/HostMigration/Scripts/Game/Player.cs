using UnityEngine;
using Mirror;
using System;

public enum PlayerPosition { BottomLeft, BottomRight, TopLeft, TopRight }
public class Player : NetworkBehaviour
{
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

    private void OnDestroy()
    {
        if (isServer)
        {
            PlayersManager.Instance.RemovePlayer(gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command]
    private void CmdRegisterPlayer()
    {
        PlayersManager.Instance.AddPlayer(gameObject.GetComponent<NetworkIdentity>().netId);
    }

    private void Start()
    {
        Debug.Log("i woke up lol");

        if (isLocalPlayer)
        {
            CmdRegisterPlayer();

            // Ensure BoostersManager is properly assigned
            _boostersManager = GetComponent<BoostersManager>();
            if (_boostersManager == null)
            {
                Debug.LogError("BoostersManager component is missing!");
                return;
            }

            if (PlayerHealthBarVisual != null)
            {
                // Add HealthBar component but delay initialization
                var scriptObj = Instantiate(PlayerHealthBarScriptPrefab, Vector3.zero, Quaternion.identity);
                HealthBar = scriptObj.GetComponent<HealthBar>();

                if (isServer)
                {
                    Debug.Log("Server initializing HealthBar!");
                    NetworkServer.Spawn(scriptObj); // Ensure object is network-spawned
                    HealthBar.Visual = PlayerHealthBarVisual;
                    HealthBar.TotalHealth = StartingHealth; // Initialize SyncVars after spawn
                    HealthBar.CurrentHealth = StartingHealth;
                    HealthBar.CmdCreateBar();
                }
            }
            else
            {
                Debug.LogError("PlayerHealthBarVisual is not assigned in the Inspector!");
            }

            if (isServer)
            {
                Debug.Log("Hiding canvas stuff!");
                UIManager.Instance.ChangeScreenState(ScreenState.WaitingLobby);
            }
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
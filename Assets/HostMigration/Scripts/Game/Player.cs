using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

public enum PlayerPosition { BottomLeft, BottomRight, TopLeft, TopRight, None }
public class Player : NetworkBehaviour
{
    [SyncVar]
    public bool IsAlive = true;
    [SyncVar]
    public bool ReadyToPlay;
    [SyncVar(hook = nameof(OnPlayerRolledAllDice))]
    public bool HasAlreadyRolled;
    [SyncVar]
    public bool ReadyForNextWave;
    [SyncVar]
    public bool HasAlreadyPickedCard;

    [SyncVar(hook = nameof(OnLifeStealAdded))]
    public float LifeStealDue;

    private void OnLifeStealAdded(float oldValue, float newValue)
    {
        if (this.GetComponent<NetworkIdentity>().netId != NetworkClient.localPlayer.netId) return;
        Debug.Log($"Lifesteal value: {newValue}");
        if (newValue > 1)
        {
            Debug.LogWarning("restored health with lifesteal perk!");
            CmdRestoreHealth(Mathf.FloorToInt(newValue));
            CmdUpdateLifeStealDue(this, newValue - Mathf.FloorToInt(newValue));
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdUpdateLifeStealDue(Player player, float updatedValue)
    {
        player.LifeStealDue = updatedValue;
    }
    public int DiceCount = 1;
    private bool _canRoll;
    public BoostersManager BoosterManager;
    public HealthBar HealthBar { get; private set; }
    [SerializeField] GameObject PlayerHealthBarVisual;

    public bool IsDead => HealthBar.CurrentHealth == 0;
    public int StartingHealth = 20;

    // NETWORK TRANSFORMS
    [SerializeField] private GameObject _cupPrefab;

    private Vector3 _cupPosition1;
    private Vector3 _cupPosition2;
    private Vector3 _cupPosition3;
    private Vector3 _cupPosition4;

    public uint PlayerNetId => this.gameObject.GetComponent<NetworkIdentity>().netId;
    public readonly SyncList<uint> DiceOwned = new SyncList<uint>();

    [SyncVar]
    public PlayerPosition PlayerScreenPosition;

    private void OnPlayerRolledAllDice(bool oldValue, bool newValue)
    {
        if (newValue == true && this.GetComponent<NetworkIdentity>().netId == NetworkClient.localPlayer.netId) // Don't check if it's resetting to false or on all clients
            CmdCheckIfAllPlayersHaveRolled();
    }

    [Command(requiresAuthority = false)]
    private void CmdCheckIfAllPlayersHaveRolled()
    {
        DiceManager.Instance.CheckIfEverybodyRolledDice();
    }

    public override void OnStopClient()
    {
        if (isServer)
        {
            PlayersManager.Instance.RemovePlayer(PlayerNetId);
        }
        base.OnStopClient();
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
        if (MyNetworkManager.singleton.IsDebugging) return;
        // This should probably be refactored, but I don't have time for it 
        _cupPosition1 = GameObject.FindWithTag("CupPosition1").transform.position;
        _cupPosition2 = GameObject.FindWithTag("CupPosition2").transform.position;
        _cupPosition3 = GameObject.FindWithTag("CupPosition3").transform.position;
        _cupPosition4 = GameObject.FindWithTag("CupPosition4").transform.position;
        BoosterManager = GetComponent<BoostersManager>();

        if (isLocalPlayer) // never use localplayer in awake
        {
            Debug.Log("IsLocalPlayer is true! starting player");
            CmdRegisterPlayer();

                                                                                                                                                                                                                                   
            UIManager.Instance.StartOwnPlayerUI();                                                                                                                                                                                 
                                                                                                                                                                                                                                   
        }                                                                                                                                                                                                                          
    }                                                                                                                                                                                                                              
                                                                                                                                                                                                                                   
    private void Update()                                                                                                                                                                                                          
    {                                                                                                                                                                                                                              
        if (!isLocalPlayer) return;                                                                                                                                                                                                

        //if (Input.GetKeyDown(KeyCode.T))
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Rolling dice from player");
            CmdRollDice(this);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Restoring health from player");
            CmdRestoreHealth(5);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRollDice(Player player)
    {
        if (!IsAlive)
        {
            Debug.LogWarning("Player is dead, can't roll");
            return;
        }

        if (_canRoll && !HasAlreadyRolled)
        {
            //Debug.Log("ROLLING DICE!");
            // Perform dice roll logic
            int totalRoll = 0;
            for (int i = 0; i < DiceCount; i++)
            {
                int eyes = UnityEngine.Random.Range(1, 7);
                totalRoll += eyes;

                // Lifesteal perk
                if (player.BoosterManager.LifestealPercentage > 0)
                {
                    Debug.Log("LIFESTEALIN");
                    LifeStealDue += (totalRoll * player.BoosterManager.LifestealPercentage);
                }
                CmdSpawnDiceWithEyes(PlayerNetId, eyes);
            }
            // Set at the end, so dice don't get rolled after everyone is ready with MoreDice perk
            HasAlreadyRolled = true;
        }
        else Debug.LogWarning($"CANT ROLL DICE! canRoll = {_canRoll}, hasAlreadyRolled = {HasAlreadyRolled}");
    }

    [Command(requiresAuthority = false)]
    public async void CmdTakeDamage(int health, float waitForSeconds)
    {
        if (!IsAlive)
        {
            Debug.LogWarning("Player is dead, can't take damage");
            return;
        }

        // Sync damage taken when enemy hits player in animation
        await System.Threading.Tasks.Task.Delay((int)(waitForSeconds * 1000));

        // Deal damage
        HealthBar.CurrentHealth -= health;
        if (HealthBar.CurrentHealth <= 0)
        {
            Debug.Log("Player died.");
            HealthBar.CurrentHealth = 0;
            IsAlive = false;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdRestoreHealth(int health)
    {
        if (!IsAlive)
        {
            Debug.LogWarning("Player is dead, can't restore health. Revive first!");
            return;
        }

        HealthBar.CurrentHealth += health;
        if (HealthBar.CurrentHealth > HealthBar.TotalHealth)
        {
            Debug.Log("Player health clamped. Health was above max");
            HealthBar.CurrentHealth = HealthBar.TotalHealth;
        }
    }

    internal void RevivePlayer()
    {
        if (!IsAlive)
        {
            Debug.Log("Player reviving!.");
            HealthBar.CurrentHealth = HealthBar.TotalHealth/2;
            IsAlive = true;
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnDiceWithEyes(uint playerId, int eyesRolled)
    {
        var newDice = new Dice(playerId, eyesRolled);
        //Debug.Log("New dice made: " + newDice);
        DiceManager.Instance.AddDice(newDice);
    }

    public void SpawnAndShakeDiceJar()
    {
        Vector3 pos = GetPlayerCupPosition();
        var upsideDownRotation = new Vector3(0, 180, 0);
        var quaternion = Quaternion.Euler(upsideDownRotation);
        CmdSpawnCup(quaternion, pos);
    }

    [Command(requiresAuthority = false)]
    public void CmdSpawnCup(Quaternion rotation, Vector3 position)
    {
        //Debug.Log($"PLAYER / CMDSPAWNCUP / Spawning cup at pos: {position}, rot: {rotation.eulerAngles}");

        // Spawning the cup
        var cup = Instantiate(_cupPrefab);
        NetworkServer.Spawn(cup);
        DiceManager.Instance.CupIdList.Add(cup.GetComponent<NetworkIdentity>().netId);

        // Synced by Network Transform
        cup.transform.SetPositionAndRotation(position, rotation);
        ShakeThatCup();
    }

    public void ShakeThatCup()
    {
        // Play dice shake animation
        Debug.LogWarning("No cup animation implemented yet");
    }

    public Vector3 GetPlayerCupPosition()
    {
        //Debug.Log($"Player {this.name} is getting it's position: {PlayerScreenPosition} to spawn dice at");
        //Debug.Log($"Possible values are {_cupPosition1}, {_cupPosition2}, {_cupPosition3}, {_cupPosition4}");
        return (PlayerScreenPosition) switch
        {
            PlayerPosition.BottomLeft => _cupPosition1,
            PlayerPosition.BottomRight => _cupPosition2,
            PlayerPosition.TopLeft => _cupPosition3,
            PlayerPosition.TopRight => _cupPosition4,

            // Will never happen
            PlayerPosition.None => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    public void ReceiveDice(int diceCount)
    {
        if (!IsAlive)
        {
            Debug.LogWarning("Player is dead, can't receive dice");
            return;
        }
        Debug.Log(this.ToString() + " received " + diceCount + "dice");
        // Enable cup, allow for rolling

        _canRoll = true;
        HasAlreadyRolled = false;
    }
}
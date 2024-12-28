using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{

    /// <summary>
    ///  DEBUG
    /// </summary>
    private void Update()
    {
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.L))
        {
            DebugNetworkObjects.CmdTriggerRpcLog();
        }
    }

    [SyncVar(hook = nameof(OnTotalHealthChanged))]
    public int TotalHealth;

    [SyncVar(hook = nameof(OnCurrentHealthChanged))]
    public int CurrentHealth;

    private Image _greenHealth;
    public Vector3 Position;
    public GameObject VisualPreset { get; private set; }

    private PlayerPosition _cachedPlayerPosition;
    private GameObject _actualVisualOfHealthBar;
    public GameObject ActualVisualOfHealthBar
    {
        get
        {
            if (_actualVisualOfHealthBar != null) return _actualVisualOfHealthBar;
            if (NetworkServer.spawned.TryGetValue(BarVisualNetId, out NetworkIdentity bar))
            {
                Debug.Log("Found the bar from property ActualVisual" + bar.netId);
                _actualVisualOfHealthBar = bar.gameObject;
                return bar.gameObject;
            }
            Debug.LogError("Couldnt find the damn visual for the health bar man");
            return null;
        }
        set { _actualVisualOfHealthBar = value; }
    }
    [SyncVar(hook = nameof(OnBarVisualNetAssigned))]
    public uint BarVisualNetId;

    public void SetupHealthBar(GameObject playerHealthBarVisual, int startingHealth, PlayerPosition position = PlayerPosition.None)
    {
        this.VisualPreset = playerHealthBarVisual;
        this.TotalHealth = startingHealth; // Initialize SyncVars after spawn
        this.CurrentHealth = startingHealth;
        CmdCreateBar(position);
    }
    [Command(requiresAuthority = false)]
    public void CmdCreateBar(PlayerPosition playerPosition)
    {
        ActualVisualOfHealthBar = Instantiate(VisualPreset, Position, Quaternion.identity);
        NetworkServer.Spawn(ActualVisualOfHealthBar);
        _cachedPlayerPosition = playerPosition; // cache it so we can use it for syncvar hook

        DebugNetworkObjects.RpcLogNetworkObjects();
        // This will automatically call OnBarVisualNetAssigned, which will call RpcCreateBar
        BarVisualNetId = ActualVisualOfHealthBar.GetComponent<NetworkIdentity>().netId;
    }

    private void OnBarVisualNetAssigned(uint oldValue, uint newValue)
    {
        // We don't have a reference to playerPosition here, which is required by the rpc
        Debug.LogWarning("JUST CHANGED THE FUCKING VALUE OF THE FUCKING BAR VISUAL: " + newValue + "(was " + oldValue + ")");
        if(isServer)
        RpcCreateBar(newValue, _cachedPlayerPosition);
    }

    [ClientRpc]
    public void RpcCreateBar(uint netId, PlayerPosition playerPosition)
    {
        Debug.Log("RpcCreateBar running here");

        if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity objNetIdentity))
        {
            Debug.Log("PARENTING THE HEALTH BAR HERE");
            objNetIdentity.transform.SetParent(UIManager.Instance.HealthBarsCanvas.transform);
            _greenHealth = objNetIdentity.transform.GetChild(2).GetComponent<Image>();
            if (playerPosition == PlayerPosition.None) // if not a player
            {
                Debug.Log("Trying to set pos of enemy health bar now!");
                SetPositionOfHealthBarEnemy(netId);
            }
            else
            {
                Debug.Log("Trying to set pos of player health bar now!");
                SetPositionOfHealthBarPlayer(netId, playerPosition);
            }
        }
        else Debug.LogWarning("NO HEALTH BAR FOUND!");
    }

    private void OnTotalHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"TotalHealth changed from {oldValue} to {newValue}");
        UpdateBar();
    }

    private void OnCurrentHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"CurrentHealth changed from {oldValue} to {newValue}");
        UpdateBar();
    }

    private void UpdateBar()
    {
        if (_greenHealth == null)
        {
            if(CurrentHealth != 0 && TotalHealth != 0)
            Debug.LogWarning("Green health bar is not assigned!");

            return;
        }

        if (TotalHealth > 0)
        {
            Debug.Log("Changing health!");
            _greenHealth.fillAmount = (float)CurrentHealth / TotalHealth; // Fixed division
        }
        else
        {
            Debug.LogError("TotalHealth is zero or less, cannot update health bar!");
        }
    }

    public void SetPositionOfHealthBarPlayer(uint netId, PlayerPosition screenPos)
    {
        Debug.Log(BarVisualNetId + "(PLAYER) is the bar id I'm trying to change position of -BUT! This is passed through the syncvar newvalue:" + netId);
        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity barId))
        {
            Debug.Log("Changing healthbar pos player");
            RectTransform bar = barId.GetComponent<RectTransform>();
            Debug.Log(bar.localScale);
            bar.localScale = new Vector3(1f, 1f, 1f);
            switch (screenPos)
            {
                case PlayerPosition.BottomLeft:
                    bar.localPosition = new Vector3(-600, -450);
                    break;
                case PlayerPosition.BottomRight:
                    bar.localPosition = new Vector3(+600, -450);
                    break;
                case PlayerPosition.TopLeft:
                    bar.localPosition = new Vector3(-600, 300);
                    break;
                case PlayerPosition.TopRight:
                    bar.localPosition = new Vector3(+600, 300);
                    break;
                default:
                    break;
            }
            UpdateBar();
        }
        else Debug.LogError($"NO BAR VISUAL NET ID {netId} FOUND! HEALTHBAR WONT GET POSITIONED");
    }

    public void SetPositionOfHealthBarEnemy(uint netId)
    {
        // Set rect transform of health bar to X: 0, Y: -350, and local scale to 1.25f
        Debug.Log(BarVisualNetId + "(ENEMY) is the bar id I'm trying to change position of - BUT! This is passed through the syncvar newvalue:" + netId);
        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity barId))
        {
            Debug.Log("Changing healthbar pos enemy");
            RectTransform bar = barId.GetComponent<RectTransform>();
            bar.localScale = new Vector3(1.25f, 1.25f, 1.25f);
            bar.localPosition = new Vector3(0, -350);
            UpdateBar();
        }
        else Debug.LogError($"NO BAR VISUAL NET ID {netId} FOUND! HEALTHBAR WONT GET POSITIONED");
    }
}

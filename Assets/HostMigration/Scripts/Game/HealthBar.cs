using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnTotalHealthChanged))]
    public int TotalHealth;

    [SyncVar(hook = nameof(OnCurrentHealthChanged))]
    public int CurrentHealth;

    private Image _greenHealth;
    public Vector3 Position;
    public GameObject VisualPreset { get; private set; }
    private GameObject _actualVisualOfHealthBar;

    public GameObject ActualVisualOfHealthBar
    {
        get
        {
            if (_actualVisualOfHealthBar != null) return _actualVisualOfHealthBar;
            if (NetworkServer.spawned.TryGetValue(BarNetId, out NetworkIdentity bar))
            {
                _actualVisualOfHealthBar = bar.gameObject;
                return bar.gameObject;
            }
            Debug.LogError("Couldnt find the damn visual for the health bar man");
            return null;
                }
        set { _actualVisualOfHealthBar = value; }
    }
    [SyncVar]
    public uint BarNetId;

    public void SetupHealthBar(GameObject playerHealthBarVisual, int startingHealth, PlayerPosition position = PlayerPosition.None)
    {
        this.VisualPreset = playerHealthBarVisual;
        this.TotalHealth = startingHealth; // Initialize SyncVars after spawn
        this.CurrentHealth = startingHealth;
        this.BarNetId = GetComponent<NetworkIdentity>().netId;
        CmdCreateBar(position);
        // TODO RPC MOVE HEALTHBAR TO POSITION --------------------------------------------------------------------
    }
    [Command(requiresAuthority = false)]
    public void CmdCreateBar(PlayerPosition playerPosition)
    {
        ActualVisualOfHealthBar = Instantiate(VisualPreset, Position, Quaternion.identity);
        NetworkServer.Spawn(ActualVisualOfHealthBar);
        RpcCreateBar(BarNetId, playerPosition);
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
                RpcSetPositionOfHealthBarEnemy();
            }
            else
            {
                RpcSetPositionOfHealthBarPlayer(playerPosition);
            }
            UpdateBar();
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

    public void RpcSetPositionOfHealthBarPlayer(PlayerPosition screenPos)
    {
        Debug.Log("Changing healthbar pos player");
        RectTransform bar = ActualVisualOfHealthBar.GetComponent<RectTransform>();
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
    }

    [ClientRpc]
    public void RpcSetPositionOfHealthBarEnemy()
    {
        // Set rect transform of health bar to X: 0, Y: -350, and local scale to 1.25f
        Debug.Log("Changing healthbar pos enemy");
        RectTransform bar = ActualVisualOfHealthBar.GetComponent<RectTransform>();
        bar.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        bar.localPosition = new Vector3(0, -350);
    }
}

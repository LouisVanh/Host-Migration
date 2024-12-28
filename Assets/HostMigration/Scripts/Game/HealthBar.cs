using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L) && isLocalPlayer) CurrentHealth--; // DEBUG
    }
    [Header("Sync Vars")]

    [SyncVar(hook = nameof(OnTotalHealthChanged))]
    public int TotalHealth;

    [SyncVar(hook = nameof(OnCurrentHealthChanged))]
    public int CurrentHealth;

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

    [Header("Fields")]
    public GameObject VisualPreset { get; private set; }
    public readonly uint VisualPresetGUID = 1420681367;
    private Image _greenHealth;
    private GameObject _actualVisualOfHealthBar;
    private uint _ownerNetId; // dont think i'll need this tbh

    public void SetupOwnHealthBar(GameObject HealthBarVisual, int startingHealth, Player player = null)
    {
        this.VisualPreset = HealthBarVisual;

        if (player)
        {
            // everything happens locally
            CreateBar();
            //_ownerNetId = player.GetComponent<NetworkIdentity>().netId;
            SetPositionOfHealthBarPlayer(_actualVisualOfHealthBar.GetComponent<RectTransform>(), player.PlayerScreenPosition);
        }
        else // if enemy
        {
            // everything happens synced
            CmdRpcCreateBar();
            Debug.Log("_actualVisual AFTER THE COMMAND= " + _actualVisualOfHealthBar);
            Debug.Log("Done: position of enemy health bar");
        }

        this.TotalHealth = startingHealth;
        this.CurrentHealth = startingHealth;
    }

    private void CreateBar()
    {
        _actualVisualOfHealthBar = Instantiate(VisualPreset);
        _actualVisualOfHealthBar.transform.SetParent(UIManager.Instance.HealthBarsCanvas.transform);
        _greenHealth = _actualVisualOfHealthBar.transform.GetChild(2).GetComponent<Image>();
    }
    [Command(requiresAuthority = false)]
    private void CmdRpcCreateBar()
    { // for enemy: we want this to be spawned on all clients
        RpcCreateBar();
    }
    [ClientRpc]
    private void RpcCreateBar()
    {
        Debug.Log("something going wrong past this point");
        Debug.Log("VISUALPRESET = " + VisualPreset);
        if(NetworkClient.GetPrefab(VisualPresetGUID, out GameObject healthBarPrefab))
        _actualVisualOfHealthBar = Instantiate(healthBarPrefab);

        Debug.Log("_actualVisual = " + _actualVisualOfHealthBar);
        _actualVisualOfHealthBar.transform.SetParent(UIManager.Instance.HealthBarsCanvas.transform);
        _greenHealth = _actualVisualOfHealthBar.transform.GetChild(2).GetComponent<Image>();

        SetPositionOfHealthBarEnemy(_actualVisualOfHealthBar.GetComponent<RectTransform>());
    }


    private void UpdateBar()
    {
        if (_greenHealth == null)
        {
            if (CurrentHealth != 0 && TotalHealth != 0)
                Debug.LogWarning("Something is wrong, green health bar is not assigned when it should be!");

            return;
        }

        if (TotalHealth > 0)
        {
            Debug.Log("Changing health!");
            _greenHealth.fillAmount = (float)CurrentHealth / TotalHealth; // Fixed division
        }
        else
        {
            if (TurnManager.Instance.TurnCount > 1)
                Debug.LogError("TotalHealth is zero or less, cannot update health bar!");
        }
    }

    public void SetPositionOfHealthBarPlayer(RectTransform bar, PlayerPosition screenPos)
    {
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

    public void SetPositionOfHealthBarEnemy(RectTransform bar)
    {
        // Set rect transform of health bar to X: 0, Y: -350, and local scale to 1.25f
        Debug.Log("Setting position of enemy health bar");
        bar.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        bar.localPosition = new Vector3(0, -350);
        UpdateBar();
    }
}

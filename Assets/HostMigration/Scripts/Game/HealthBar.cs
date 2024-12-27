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
    public GameObject Visual { get; set; }


    public void SetupHealthBar(GameObject playerHealthBarVisual, int startingHealth)
    {
        this.Visual = playerHealthBarVisual;
        this.TotalHealth = startingHealth; // Initialize SyncVars after spawn
        this.CurrentHealth = startingHealth;
        CmdCreateBar();
        // TODO RPC MOVE HEALTHBAR TO POSITION --------------------------------------------------------------------
    }
    [Command(requiresAuthority = false)]
    public void CmdCreateBar()
    {
        var obj = Instantiate(Visual, Position, Quaternion.identity);
        NetworkServer.Spawn(obj);
        RpcCreateBar(obj.GetComponent<NetworkIdentity>().netId);
    }

    [ClientRpc]
    public void RpcCreateBar(uint netId)
    {
        Debug.Log("RpcCreateBar running here");
        if (Visual == null)
        {
            Debug.LogError("HealthBar Visual prefab is not assigned!");
            return;
        }
        if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity objNetIdentity))
        {
            Debug.Log("PARENTING THE HEALTH BAR HERE");
            objNetIdentity.transform.SetParent(UIManager.Instance.HealthBarsCanvas.transform);
            _greenHealth = objNetIdentity.transform.GetChild(2).GetComponent<Image>();
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
            _greenHealth.fillAmount = (float)CurrentHealth / TotalHealth; // Fixed division
        }
        else
        {
            Debug.LogError("TotalHealth is zero or less, cannot update health bar!");
        }
    }


    public void SetPositionOfHealthBarPlayer(PlayerPosition screenPos)
    {
        RectTransform bar = Visual.GetComponent<RectTransform>();
        bar.localScale = new Vector3(1f, 1f, 1f);
        switch (screenPos)
        {
            case PlayerPosition.BottomLeft:
                bar.position = new Vector3(-600, -450);
                break;
            case PlayerPosition.BottomRight:
                bar.position = new Vector3(+600, -450);
                break;
            case PlayerPosition.TopLeft:
                bar.position = new Vector3(-600, 300);
                break;
            case PlayerPosition.TopRight:
                bar.position = new Vector3(+600, 300);
                break;
            default:
                break;
        }
    }
    public void SetPositionOfHealthBarEnemy()
    {
        // Set rect transform of health bar to X: 0, Y: -350, and local scale to 1.25f
        RectTransform bar = Visual.GetComponent<RectTransform>();
        bar.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        bar.position = new Vector3(0, -350);
    }
}

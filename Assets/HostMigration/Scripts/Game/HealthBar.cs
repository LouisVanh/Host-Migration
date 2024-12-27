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
        if (Visual == null)
        {
            Debug.LogError("HealthBar Visual prefab is not assigned!");
            return;
        }
        if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity objNetIdentity))
        {
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


    private void SetPositionOfHealthBar()
    {

    }
}

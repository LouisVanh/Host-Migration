using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    [SyncVar(hook = nameof(AdjustTotalHealth))]
    public int TotalHealth;

    [SyncVar(hook = nameof(UpdateHealth))]
    public int CurrentHealth;

    private Image _greenHealth;
    public Vector3 Position;
    public GameObject Visual;

    //public HealthBar(int totalHealth, GameObject visual, Vector3 position)
    //{
    //    TotalHealth = totalHealth;
    //    CurrentHealth = totalHealth;
    //    Visual = visual;
    //    Instantiate(Visual, position, Quaternion.identity);
    //}

    public void CreateBar()
    {
        Instantiate(Visual, Position, Quaternion.identity);
        UpdateBar();
    }

    [ClientRpc]
    public void UpdateHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, TotalHealth);
        UpdateBar();
    }

    [ClientRpc]
    public void AdjustTotalHealth(int amount)
    {
        TotalHealth += amount;
        UpdateBar();
    }

   void UpdateBar()
    {
        _greenHealth.fillAmount = CurrentHealth / TotalHealth;
    }
}
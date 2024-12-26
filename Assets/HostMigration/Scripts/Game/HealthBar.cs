using Mirror;
using UnityEngine;

public class HealthBar : NetworkBehaviour
{
    public int TotalHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public GameObject Visual { get; private set; }

    public HealthBar(int totalHealth, GameObject visual)
    {
        TotalHealth = totalHealth;
        CurrentHealth = totalHealth;
        Visual = visual;
    }

    [ClientRpc]
    public void UpdateHealth(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, TotalHealth);
        // Update UI visual progress bar
    }

    [ClientRpc]
    public void AdjustTotalHealth(int amount)
    {
        TotalHealth += amount;
    }
}
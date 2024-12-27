using UnityEngine;

public class HealthyBooster : MonoBehaviour, IBoosterPermanent
{
    public string Name => "Healthy";
    public string Description => $"Upgrade your total player health by {HealthToAdd}";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public int HealthToAdd = 20;

    public void AddPermanentEffect(Player player)
    {
        var healthBar = player.GetComponent<HealthBar>();
        healthBar.TotalHealth += HealthToAdd;
    }

    public void RemovePermanentEffect(Player player)
    {
        var healthBar = player.GetComponent<HealthBar>();
        healthBar.CurrentHealth -= HealthToAdd;
    }
}
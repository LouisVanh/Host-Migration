using UnityEngine;

public class RegenerateHealthBooster : MonoBehaviour, IBoosterConsumable
{
    public string Name => "Regenerate";
    public string Description => "Regenerate your player health (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public void ConsumeEffect(Player player)
    {
        var healthBar = player.GetComponent<HealthBar>();
        var maxHealthToRestore = healthBar.TotalHealth - healthBar.CurrentHealth;
        healthBar.CurrentHealth += maxHealthToRestore;

        player.BoosterManager.RemoveSpecificOwnedBooster(this as IBooster);
    }
}
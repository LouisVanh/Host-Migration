using UnityEngine;

public class RegenerateHealthBooster : MonoBehaviour, IBoosterConsumable
{
    public string Name => "Regenerate";
    public string Description => "Regenerate your player health (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }


    public int HealthToRestore;

    public void ConsumeEffect(Player player)
    {
        if (player != null)
        {
            var healthBar = player.GetComponent<HealthBar>();
            healthBar.UpdateHealth(healthBar.CurrentHealth, HealthToRestore);
        }
    }
}
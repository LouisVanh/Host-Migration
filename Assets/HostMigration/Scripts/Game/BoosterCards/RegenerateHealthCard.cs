using UnityEngine;

public class RegenerateHealthBooster : IBoosterConsumable
{
    public string Name => "Regenerate";
    public string Description => "Regenerate your player health (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
    Player PlayerShownTo { get; }


    private int _healthToRestore;

    public RegenerateHealthBooster(int healthToRestore)
    {
        _healthToRestore = healthToRestore;
    }

    public void ConsumeEffect(Player player)
    {
        if (player != null)
        {
            player.GetComponent<HealthBar>().UpdateHealth(_healthToRestore);
        }
    }
}
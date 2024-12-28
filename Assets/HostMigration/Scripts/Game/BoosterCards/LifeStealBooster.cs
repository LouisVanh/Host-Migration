using UnityEngine;

public class LifeStealBooster : MonoBehaviour, IBoosterPermanent
{
    public string Name => "Lifesteal";
    public string Description => $"Restore {LifeStealPercentage}% health of your attack damage";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public int LifeStealPercentage = 20;

    public void AddPermanentEffect(Player player)
    {
        player.BoosterManager.LifestealPercentage += 20;
    }

    public void RemovePermanentEffect(Player player)
    {
        player.BoosterManager.LifestealPercentage -= 20;
    }
}
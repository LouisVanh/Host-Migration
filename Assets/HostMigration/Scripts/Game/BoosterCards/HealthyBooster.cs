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
        player.GetComponent<HealthBar>().AdjustTotalHealth(HealthToAdd);
    }

    public void RemovePermanentEffect(Player player)
    {
        player.GetComponent<HealthBar>().AdjustTotalHealth(- HealthToAdd);
    }
}
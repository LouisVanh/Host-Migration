using UnityEngine;

public class SaviourBooster : MonoBehaviour, IBoosterConsumable
{
    public string Name => "Saviour";
    public string Description => "Revive a random teammate (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public void ConsumeEffect(Player player)
    {
        if (player == null) return;
        if (player.IsDead) return;

        var deadGuy = TurnManager.Instance.GetRandomDeadPlayer();
        deadGuy.HealthBar.UpdateHealth(deadGuy.HealthBar.CurrentHealth, +deadGuy.HealthBar.TotalHealth / 2);
    }
}
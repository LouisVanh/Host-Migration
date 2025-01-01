using UnityEngine;

public class SaviourBooster : MonoBehaviour, IBoosterConsumable
{
    public string Name => "Saviour";
    public string Description => "Revive a random teammate (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Legendary;
    public Player PlayerShownTo { get; }

    public void ConsumeEffect(Player player)
    {
        //if (player == null) return;
        //if (player.IsDead) return;

        var deadGuy = TurnManager.Instance.GetRandomDeadPlayer();
        if (deadGuy == null)
        {
            player.BoosterManager.RemoveSpecificOwnedBooster(this);
            return;
        }
        deadGuy.HealthBar.CurrentHealth += (deadGuy.HealthBar.TotalHealth / 2);

        player.BoosterManager.RemoveSpecificOwnedBooster(this as IBooster);
    }
}
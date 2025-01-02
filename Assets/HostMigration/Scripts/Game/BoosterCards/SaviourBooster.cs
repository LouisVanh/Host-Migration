using UnityEngine;
using Mirror;

public class SaviourBooster : NetworkBehaviour, IBoosterConsumable
{
    public string Name => "Saviour";
    public string Description => "Revive a random teammate (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Legendary;
    public Player PlayerShownTo { get; }

    [Command(requiresAuthority = false)]
    public void CmdConsumeEffect(Player player)
    {
        //if (player == null) return;
        //if (player.IsDead) return;

        var deadGuy = TurnManager.Instance.GetRandomDeadPlayer();
        if (deadGuy == null)
        {
            player.BoosterManager.RemoveSpecificOwnedBooster(Name);
            return;
        }
        deadGuy.HealthBar.CurrentHealth += (deadGuy.HealthBar.TotalHealth / 2);

        player.BoosterManager.RemoveSpecificOwnedBooster(Name);
    }
}
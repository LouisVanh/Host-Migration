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
        if (player == null) Debug.LogError("No player found to add effect for");
        Debug.Log($"Inside consume effect: {player.name}");

        var deadGuy = TurnManager.Instance.GetRandomDeadPlayer();
        if (deadGuy == null)
        {
            player.BoosterManager.RemoveSpecificOwnedBooster(player, Name);
            return;
        }
        deadGuy.RevivePlayer();
        player.BoosterManager.RemoveSpecificOwnedBooster(player, Name);
    }
}
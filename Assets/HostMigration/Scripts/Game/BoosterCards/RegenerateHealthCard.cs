using UnityEngine;
using Mirror;

public class RegenerateHealthBooster : NetworkBehaviour, IBoosterConsumable
{
    public string Name => "Regenerate";
    public string Description => "Regenerate your player health (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    [Command(requiresAuthority = false)]
    public void CmdConsumeEffect(Player player)
    {
        if (player == null) Debug.LogError("No player found to add effect for");
        if (player.IsDead) return;
        Debug.Log($"Inside consume effect: {player.name}");

        var healthBar = player.GetComponent<HealthBar>();
        var maxHealthToRestore = healthBar.TotalHealth - healthBar.CurrentHealth;
        healthBar.CurrentHealth += maxHealthToRestore;

        player.BoosterManager.RemoveSpecificOwnedBooster(player, Name);
    }
}
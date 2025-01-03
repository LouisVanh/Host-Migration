using UnityEngine;
using Mirror;

public class GlassCannonBooster : NetworkBehaviour, IBoosterConsumable
{
    public string Name => "Glass Cannon";
    public string Description => "Double all of your future rolls, in exchange for half of your health";
    public BoosterRarity Rarity => BoosterRarity.Chaos;
    public Player PlayerShownTo { get; }

    [Command(requiresAuthority = false)]
    public void CmdConsumeEffect(Player player)
    {
        if (player == null) Debug.LogError("No player found to add effect for");
        Debug.Log($"Inside consume effect: {player.name}");
        player.BoosterManager.DamageMultiplier *= 2;
        player.CmdTakeDamage(Mathf.RoundToInt(player.HealthBar.CurrentHealth / 2), waitForSeconds: 0);

        player.BoosterManager.RemoveSpecificOwnedBooster(player, Name);
    }
}
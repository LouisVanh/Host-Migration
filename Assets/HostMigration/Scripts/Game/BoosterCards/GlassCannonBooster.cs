using UnityEngine;

public class GlassCannonBooster : MonoBehaviour, IBoosterConsumable
{
    public string Name => "Glass Cannon";
    public string Description => "Double all of your future rolls, in exchange for half of your health";
    public BoosterRarity Rarity => BoosterRarity.Chaos;
    public Player PlayerShownTo { get; }

    public void ConsumeEffect(Player player)
    {
        player.BoosterManager.DamageMultiplier *= 2;
        player.CmdTakeDamage(Mathf.RoundToInt(player.HealthBar.CurrentHealth / 2), waitForSeconds: 0);

        player.BoosterManager.RemoveSpecificOwnedBooster(this as IBooster);
    }
}
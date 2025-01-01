using UnityEngine;
using Mirror;

public class LifeStealBooster : NetworkBehaviour, IBoosterPermanent
{
    public string Name => "Lifesteal";
    public string Description => $"Restore {LifeStealPercentage}% health of your attack damage";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public int LifeStealPercentage = 20;

    [Command(requiresAuthority = false)]
    public void CmdAddPermanentEffect(Player player)
    {
        player.BoosterManager.LifestealPercentage += LifeStealPercentage/100;
    }

    [Command(requiresAuthority = false)]
    public void CmdRemovePermanentEffect(Player player)
    {
        player.BoosterManager.LifestealPercentage -= LifeStealPercentage/100;
    }
}
using Mirror;
using UnityEngine;

public class HealthyBooster : NetworkBehaviour, IBoosterPermanent
{
    public string Name => "Healthy";
    public string Description => $"Upgrade your total player health by {HealthToAdd}";
    public BoosterRarity Rarity => BoosterRarity.Common;
    public Player PlayerShownTo { get; }

    public int HealthToAdd = 20;


    [Command(requiresAuthority = false)]
    public void CmdAddPermanentEffect(Player player)
    {
        var healthBar = player.GetComponent<HealthBar>();
        healthBar.TotalHealth += HealthToAdd;
    }

    [Command(requiresAuthority = false)]
    public void CmdRemovePermanentEffect(Player player)
    {
        var healthBar = player.GetComponent<HealthBar>();
        healthBar.CurrentHealth -= HealthToAdd;
    }
}
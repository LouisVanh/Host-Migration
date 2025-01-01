using UnityEngine;
using Mirror;

public class AceBooster : NetworkBehaviour, IBoosterPermanent
{
    public string Name => "Ace";
    public string Description => "Rolling one eye doubles your teammate's roll";
    public BoosterRarity Rarity => BoosterRarity.Legendary;
    public Player PlayerShownTo { get; }

    [Command(requiresAuthority = false)]
    public void CmdAddPermanentEffect(Player player)
    {
        Debug.LogWarning("Implement ace becoming double points for teammates here");
        throw new System.NotImplementedException();
    }

    [Command(requiresAuthority = false)]
    public void CmdRemovePermanentEffect(Player player)
    {
        Debug.LogWarning("Implement removing the ace becoming double points for teammates here");
        throw new System.NotImplementedException();
    }
}
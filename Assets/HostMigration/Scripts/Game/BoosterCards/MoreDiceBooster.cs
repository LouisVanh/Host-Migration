using UnityEngine;
using Mirror;

public class MoreDiceBooster : NetworkBehaviour, IBoosterPermanent
{
    public string Name => "More Dice!";
    public string Description => $"Permanently unlock an extra dice";
    public BoosterRarity Rarity => BoosterRarity.Legendary;
    public Player PlayerShownTo { get; }

    [Command(requiresAuthority = false)]
    public void CmdAddPermanentEffect(Player player)
    {
        if (player == null) Debug.LogError("No player found to add effect for");
        Debug.Log($"Inside permanent effect: {player.name}");

        player.DiceCount += 1;
    }

    [Command(requiresAuthority = false)]
    public void CmdRemovePermanentEffect(Player player)
    {
        player.DiceCount -= 1;
    }
}
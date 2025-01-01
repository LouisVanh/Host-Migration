using UnityEngine;

public class AceBooster : MonoBehaviour, IBoosterPermanent
{
    public string Name => "Ace";
    public string Description => "Rolling one eye doubles your teammate's roll";
    public BoosterRarity Rarity => BoosterRarity.Legendary;
    public Player PlayerShownTo { get; }

    public void AddPermanentEffect(Player player)
    {
        Debug.LogWarning("Implement ace becoming double points for teammates here");
        throw new System.NotImplementedException();
    }

    public void RemovePermanentEffect(Player player)
    {
        Debug.LogWarning("Implement removing the ace becoming double points for teammates here");
        throw new System.NotImplementedException();
    }
}
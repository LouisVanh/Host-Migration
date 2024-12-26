using UnityEngine;

public class AceBooster : MonoBehaviour, IBoosterPermanent
{
    public string Name => "Regenerate";
    public string Description => "Regenerate your player health (one-time-use)";
    public BoosterRarity Rarity => BoosterRarity.Common;
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
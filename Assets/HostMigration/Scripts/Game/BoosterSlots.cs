using Mirror;
using System;
using UnityEngine;

public class BoosterSlot
{
    public IBooster CurrentBooster;
    public bool IsEmpty => CurrentBooster == null;
    public bool IsAlreadyOwned;
    [Range(1, 7)]
    [SerializeField] private int _slotIndex;

    public BoosterSlot(IBooster currentBooster, bool isAlreadyOwned, int slotIndex)
    {
        CurrentBooster = currentBooster;
        IsAlreadyOwned = isAlreadyOwned;
        _slotIndex = slotIndex;
    }

    public void AssignBooster(IBooster booster)
    {
        CurrentBooster = booster;
    }

    public void RemoveBooster()
    {
        if (CurrentBooster is IBoosterPermanent permanentBooster)
        {
            permanentBooster.RemovePermanentEffect(NetworkClient.localPlayer.GetComponent<Player>());
        }
        CurrentBooster = null;
    }

    public override string ToString()
    {
        if (CurrentBooster == null)
        {
            return $"This is a slot with no booster, which is on slot {_slotIndex}. Owned = {IsAlreadyOwned}";
        }
        return $"This is a slot with {CurrentBooster.Name}, which is on slot {_slotIndex}. Owned = {IsAlreadyOwned}";
    }
}

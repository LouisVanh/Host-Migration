using Mirror;
using System;
using UnityEngine;

//[Serializable] adding or removing this doesn't seem to do anything
public class BoosterSlot
{
    public string CurrentBoosterName;
    public bool IsEmpty => CurrentBoosterName==null;
    public bool IsAlreadyOwned;
    public int SlotIndex;

    public BoosterSlot(string currentBoosterName, bool isAlreadyOwned, int slotIndex)
    {
        CurrentBoosterName = currentBoosterName;
        IsAlreadyOwned = isAlreadyOwned;
        SlotIndex = slotIndex;
    }
    public BoosterSlot() { } // needed for the weaver alien

    public void AssignBoosterByName(string name)
    {
        Debug.Log("Assigned booster with name" + name);
        CurrentBoosterName = name;
    }

    public void RemoveBooster()
    {
        if (BoosterContainer.GetFirstBoosterByName(CurrentBoosterName) is IBoosterPermanent permanentBooster)
        {
            permanentBooster.CmdRemovePermanentEffect(NetworkClient.localPlayer.GetComponent<Player>());
        }
        CurrentBoosterName = null;
    }

    public override string ToString()
    {
        if (CurrentBoosterName == null)
        {
            return $"This is a slot with no booster, which is on slot {SlotIndex}. Owned = {IsAlreadyOwned}";
        }
        return $"This is a slot with {CurrentBoosterName}, which is on slot {SlotIndex}. Owned = {IsAlreadyOwned}";
    }
}

using System;
using UnityEngine;

[Serializable]
public class BoosterSlot : MonoBehaviour
{
    public IBooster CurrentBooster;
    public bool IsEmpty => CurrentBooster == null;
    public bool IsAlreadyOwned;
    [Range(1, 7)]
    [SerializeField] private int _slotIndex;

    public void AssignBooster(IBooster booster)
    {
        CurrentBooster = booster;
    }
    public void RemoveBooster()
    {
        CurrentBooster = null;
    }
}

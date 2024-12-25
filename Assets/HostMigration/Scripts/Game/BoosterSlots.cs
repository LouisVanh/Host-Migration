using UnityEngine;
public class BoosterSlot
{
    public IBooster CurrentBooster;
    public bool IsEmpty => CurrentBooster == null;
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

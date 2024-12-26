using UnityEngine;

public class BoostersManager : MonoBehaviour
{
    private const int MAX_SLOTS = 7;
    public BoosterSlot[] Slots = new BoosterSlot[MAX_SLOTS];
    private RectTransform _layoutCanvas;
    private float _smallScale;
    private float _largeScale;

    private void Awake()
    {
        _layoutCanvas = GameObject.FindWithTag("CardLayout").GetComponent<RectTransform>();
    }

    public bool AddBooster(IBooster booster)
    {
        foreach (var slot in Slots)
        {
            if (slot.IsEmpty)
            {
                slot.AssignBooster(booster);
                return true;
            }
        }
        return false; // No available slots
    }

    public void RemoveSpecificBooster(IBooster booster)
    {
        foreach (var slot in Slots)
        {
            if (slot.CurrentBooster != booster) return;
            slot.RemoveBooster();
        }
    }

    public void RemoveRandomCommonBooster()
    {
        int amountOfCommonBoosters = GetAmountOfCommonBoosters();
        if (amountOfCommonBoosters == 0) { Debug.LogWarning("No common boosters found!"); return; }
        BoosterSlot[] commonBoosters = new BoosterSlot[amountOfCommonBoosters];
        int counter = 0;
        foreach (var slot in Slots)
        {
            if (slot.CurrentBooster.Rarity == BoosterRarity.Common)
            {
                commonBoosters[counter] = slot;
                counter++;
            }
        }
        var random = Random.Range(0, amountOfCommonBoosters);
        commonBoosters[random].RemoveBooster();
    }

    private int GetAmountOfCommonBoosters()
    {
        int counter = 0;
        foreach (var slot in Slots)
        {
            if (slot.CurrentBooster.Rarity == BoosterRarity.Common) counter++;
        }
        return counter;
    }

    private void EnlargeBoosterLayout()
    {
        _layoutCanvas.localScale = new Vector3(_largeScale, _largeScale, _largeScale);

    }
}
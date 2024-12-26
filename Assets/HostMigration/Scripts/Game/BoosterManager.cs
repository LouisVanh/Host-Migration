using UnityEngine;
using Mirror;

public class BoostersManager : NetworkBehaviour
{
    private const int MAX_SLOTS = 7;
    public BoosterSlot[] Slots = new BoosterSlot[MAX_SLOTS];
    private RectTransform _layoutCanvas;
    private RectTransform _newCardsLayoutCanvas;
    private float _smallScaleMultiplier;
    private float _largeScaleMultiplier;

    private void Awake()
    {
        _layoutCanvas = GameObject.FindWithTag("CardLayout").GetComponent<RectTransform>();
        _newCardsLayoutCanvas = GameObject.FindWithTag("NewCardLayout").GetComponent<RectTransform>();

        HideOwnedBoosterLayout();
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

    #region UI (all should be synced)

    [ClientRpc]
    private void EnlargeOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutCanvas.localScale = smallScale;
        LeanTweenUtility.ScaleTo(_layoutCanvas, bigScale, 1);
    }

    [ClientRpc]
    private void ShrinkOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        var smallScale = new Vector3(_smallScaleMultiplier, _smallScaleMultiplier, _smallScaleMultiplier);
        _layoutCanvas.localScale = bigScale;
        LeanTweenUtility.ScaleTo(_layoutCanvas, smallScale, 1);
    }

    [ClientRpc]
    private void HideOwnedBoosterLayout()
    {
        LeanTweenUtility.ScaleOut(_layoutCanvas, 0.75f, true);
    }

    [ClientRpc]
    private void ShowOwnedBoosterLayout()
    {
        var bigScale = new Vector3(_largeScaleMultiplier, _largeScaleMultiplier, _largeScaleMultiplier);
        LeanTweenUtility.ScaleIn(_layoutCanvas, bigScale, 0.75f);
    }
    #endregion
}
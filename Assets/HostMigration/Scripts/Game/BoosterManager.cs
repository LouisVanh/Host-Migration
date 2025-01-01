using UnityEngine;
using Mirror;

public class BoostersManager : NetworkBehaviour
{
    private const int MAX_SLOTS = 7;
    private const int MAX_POTENTIAL_SLOTS = 3;
    private BoosterContainer _boosterContainer;
    public BoosterSlot[] Slots = new BoosterSlot[MAX_SLOTS];
    public BoosterSlot[] PotentialBoosterSlots; // Create and assign on runtime

    [Header("Player stats")]
    public float LifestealPercentage;
    public int AmountOfAceBoosters;

    private void Start()
    {
        _boosterContainer = (BoosterContainer)FindFirstObjectByType(typeof(BoosterContainer));
        Debug.Log(_boosterContainer);
    }
    public bool AddOwnedBooster(IBooster booster)
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

    public void RemoveSpecificOwnedBooster(IBooster booster)
    {
        foreach (var slot in Slots)
        {
            if (slot.CurrentBooster != booster) return;
            slot.RemoveBooster();
        }
    }

    public void RemoveRandomOwnedCommonBooster()
    {
        int amountOfCommonBoosters = GetAmountOfOwnedCommonBoosters();
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

    private int GetAmountOfOwnedCommonBoosters()
    {
        int counter = 0;
        foreach (var slot in Slots)
        {
            if (slot.CurrentBooster.Rarity == BoosterRarity.Common) counter++;
        }
        return counter;
    }

    #region Potential boosters to unlock

    public void ShowPotentialBoosters()
    {
        Debug.LogWarning(" BOOSTERMANAGER / Showing potential boosters");
        // All client side, as each client will have different boosters.
        PopulatePotentialBoosters();

        // Update visual client-side
        UIManager.Instance.UpdatePotentialCardsVisualAndShow(
            PotentialBoosterSlots[0].CurrentBooster, 
            PotentialBoosterSlots[1].CurrentBooster, 
            PotentialBoosterSlots[2].CurrentBooster);
    }

    private void PopulatePotentialBoosters()
    {
        PotentialBoosterSlots = new BoosterSlot[MAX_POTENTIAL_SLOTS];
        for (int i = 0; i < MAX_POTENTIAL_SLOTS; i++)
        {
            PotentialBoosterSlots[i] = new BoosterSlot(_boosterContainer.GetRandomBoosterEntry().GetBoosterAsInterface(), false, i);
            Debug.Log(PotentialBoosterSlots[i]);
        }
    }
    #endregion
}
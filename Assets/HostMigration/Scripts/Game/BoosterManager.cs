using UnityEngine;
using Mirror;
using System.Linq;
using System.Collections.Generic;

public class BoostersManager : NetworkBehaviour
{
    private const int MAX_OWNED_SLOTS = 7;
    private const int MAX_POTENTIAL_SLOTS = 3;
    private BoosterContainer _boosterContainer;
    public BoosterSlot[] OwnedSlots = new BoosterSlot[MAX_OWNED_SLOTS];

    public readonly SyncList<BoosterSlot> PotentialBoosterSlots = new SyncList<BoosterSlot>();

    private void OnPotentialBoostersAdded(int index)
    {
        if(index == 2)
        {
            Debug.Log($"index 2 found, sending visuals");
            UIManager.Instance.UpdatePotentialCardsVisualAndShow();
        }
    }

    public override void OnStartClient()
    {
        PotentialBoosterSlots.OnAdd += OnPotentialBoostersAdded;
        base.OnStartClient();
    }

    public override void OnStopClient()
    {
        PotentialBoosterSlots.OnAdd -= OnPotentialBoostersAdded;
        base.OnStopClient();
    }

    public BoosterCardVisualData[] PotentialBoosterSlotsVisuals
    {
        get
        {
            var data = new BoosterCardVisualData[MAX_POTENTIAL_SLOTS];
            Debug.Log($"{NetworkClient.localPlayer.name} is getting Visuals.");
            Debug.Log($"{NetworkClient.localPlayer.name} is getting slots count {PotentialBoosterSlots.Count}.");
            Debug.Log($"{NetworkClient.localPlayer.name} is getting max slots {MAX_POTENTIAL_SLOTS}.");
            for (int cardIndex = 0; cardIndex < MAX_POTENTIAL_SLOTS;  cardIndex++)
            {
                Debug.Log($"Trying to get visual for card {cardIndex}");
                data[cardIndex] = new BoosterCardVisualData(
                    PotentialBoosterSlots[cardIndex].CurrentBoosterName,
                    BoosterContainer.GetFirstBoosterByName(PotentialBoosterSlots[cardIndex].CurrentBoosterName).Description,
                    UIManager.Instance.RarityColors[BoosterContainer.GetFirstBoosterByName(PotentialBoosterSlots[cardIndex].CurrentBoosterName).Rarity]);
            }
            return data;
        }
    }

    [Header("Player stats")]
    public float LifestealPercentage;
    public int AmountOfAceBoosters;
    public float DamageMultiplier /*= 1*/;

    private void Start()
    {
        _boosterContainer = (BoosterContainer)FindFirstObjectByType(typeof(BoosterContainer));
        Debug.Log(_boosterContainer);

        //Ensure that the owned boosters are all initialised as null before starting
        // This is now ran in UIManager
        //PopulateOwnedBoosters();

        // ensure the sync direction is correct
        syncDirection = SyncDirection.ServerToClient;
        //syncMode = SyncMode.Owner;
    }

    [Server]
    public bool AddOwnedBooster(Player player, string boosterName)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.IsEmpty)
            {
                IBooster booster = BoosterContainer.GetFirstBoosterByName(boosterName);
                slot.AssignBooster(booster);
                if (booster is IBoosterConsumable consumableBooster)
                {
                    consumableBooster.CmdConsumeEffect(player);
                }
                else if (booster is IBoosterPermanent permanentBooster)
                {
                    permanentBooster.CmdAddPermanentEffect(player);
                }
                return true; // everything happened successfully
            }
        }
        return false; // No available slots
    }

    //public void RemoveSpecificOwnedBooster(int slotId)
    //{
    //    foreach (var slot in OwnedSlots)
    //    {
    //        if (slot.SlotIndex != slotId)
    //        {
    //            return;
    //        }

    //        slot.RemoveBooster();
    //    }
    //}
    [Server]
    public void RemoveSpecificOwnedBooster(string name)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.CurrentBoosterName == null) { Debug.LogWarning($"This slot {slot} is null!!!!"); return; }
            if (slot.CurrentBoosterName != name)
            {
                return;
            }

            slot.RemoveBooster();
        }
    }

    #region Potential boosters to unlock

    [Server]
    public void ShowPotentialBoosters()
    {
        Debug.LogWarning(" BOOSTERMANAGER / Showing potential boosters");
        // All client side, as each client will have different boosters.
        PopulatePotentialBoosters();

        // Now called from the OnPotentialBoosters list added (so the values are initialised).
        //UIManager.Instance.UpdatePotentialCardsVisualAndShow();
    }

    [Server]
    private void PopulatePotentialBoosters()
    {
        Debug.Log($"Populating the potential boosters from {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            player.BoosterManager.PotentialBoosterSlots.Clear();
            for (int i = 0; i < MAX_POTENTIAL_SLOTS; i++)
            {
                player.BoosterManager.PotentialBoosterSlots.Add(new BoosterSlot(
                    BoosterContainer.GetRandomBoosterEntry().GetBoosterAsInterface().Name, false, i));
                Debug.Log(player.name + " got potential slot " + i + " = " + player.BoosterManager.PotentialBoosterSlots[i]);
            }
            Debug.Log(player.name + "  slots = " + player.BoosterManager.PotentialBoosterSlots.Count);
        }
    }

    [Server]
    public void PopulateOwnedBoosters()
    {
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            player.BoosterManager.OwnedSlots = new BoosterSlot[MAX_OWNED_SLOTS];
            for (int i = 0; i < MAX_OWNED_SLOTS; i++)
            {
                player.BoosterManager.OwnedSlots[i] = new BoosterSlot(null, false, i);
                Debug.Log(player.name + " got owned slot " + i + " = " + player.BoosterManager.OwnedSlots[i]);
            }
        }
    }
    #endregion
    #region currently unused
    [Server]
    public void RemoveRandomOwnedCommonBooster()
    {
        int amountOfCommonBoosters = GetAmountOfOwnedCommonBoosters();
        if (amountOfCommonBoosters == 0) { Debug.LogWarning("No common boosters found!"); return; }
        BoosterSlot[] commonBoosters = new BoosterSlot[amountOfCommonBoosters];
        int counter = 0;
        foreach (var slot in OwnedSlots)
        {
            if (BoosterContainer.GetFirstBoosterByName(slot.CurrentBoosterName).Rarity == BoosterRarity.Common)
            {
                commonBoosters[counter] = slot;
                counter++;
            }
        }
        var random = Random.Range(0, amountOfCommonBoosters);
        commonBoosters[random].RemoveBooster();
    }

    [Server]
    private int GetAmountOfOwnedCommonBoosters()
    {
        int counter = 0;
        foreach (var slot in OwnedSlots)
        {
            if (BoosterContainer.GetFirstBoosterByName(slot.CurrentBoosterName).Rarity == BoosterRarity.Common) counter++;
        }
        return counter;
    }
    #endregion currently unused
}
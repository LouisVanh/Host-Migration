using UnityEngine;
using Mirror;

public class BoostersManager : NetworkBehaviour
{
    private const int MAX_OWNED_SLOTS = 7;
    private const int MAX_POTENTIAL_SLOTS = 3;
    private BoosterContainer _boosterContainer;
    public BoosterSlot[] OwnedSlots = new BoosterSlot[MAX_OWNED_SLOTS];
    public BoosterSlot[] PotentialBoosterSlots; // Create and assign on runtime

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
    }
    public bool AddOwnedBooster(IBooster booster)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.IsEmpty)
            {
                slot.AssignBooster(booster);
                if (booster is IBoosterConsumable consumableBooster)
                {
                    consumableBooster.CmdConsumeEffect(NetworkClient.localPlayer.GetComponent<Player>());
                }
                else if (booster is IBoosterPermanent permanentBooster)
                {
                    permanentBooster.CmdAddPermanentEffect(NetworkClient.localPlayer.GetComponent<Player>());
                }
                return true; // everything happened successfully
            }
        }
        return false; // No available slots
    }

    [Server]
    public bool AddOwnedBooster(string boosterName)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.IsEmpty)
            {
                IBooster booster = _boosterContainer.GetFirstBoosterByName(boosterName);
                slot.AssignBooster(booster);
                if (booster is IBoosterConsumable consumableBooster)
                {
                    consumableBooster.CmdConsumeEffect(NetworkClient.localPlayer.GetComponent<Player>());
                }
                else if (booster is IBoosterPermanent permanentBooster)
                {
                    permanentBooster.CmdAddPermanentEffect(NetworkClient.localPlayer.GetComponent<Player>());
                }
                return true; // everything happened successfully
            }
        }
        return false; // No available slots
    }

    [Server]
    public void RemoveSpecificOwnedBooster(IBooster booster)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.CurrentBooster != booster)
            {
                return;
            }

            slot.RemoveBooster();
        }
    }
    [Server]
    public void RemoveSpecificOwnedBooster(int slotId)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.SlotIndex != slotId)
            {
                return;
            }

            slot.RemoveBooster();
        }
    }
    [Server]
    public void RemoveSpecificOwnedBooster(string name)
    {
        foreach (var slot in OwnedSlots)
        {
            if (slot.CurrentBooster == null) { Debug.LogWarning($"This slot {slot} is null!!!!"); return; }
            if (slot.CurrentBooster.Name != name)
            {
                return;
            }

            slot.RemoveBooster();
        }
    }
    [Server]
    public void RemoveRandomOwnedCommonBooster()
    {
        int amountOfCommonBoosters = GetAmountOfOwnedCommonBoosters();
        if (amountOfCommonBoosters == 0) { Debug.LogWarning("No common boosters found!"); return; }
        BoosterSlot[] commonBoosters = new BoosterSlot[amountOfCommonBoosters];
        int counter = 0;
        foreach (var slot in OwnedSlots)
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

    [Server]
    private int GetAmountOfOwnedCommonBoosters()
    {
        int counter = 0;
        foreach (var slot in OwnedSlots)
        {
            if (slot.CurrentBooster.Rarity == BoosterRarity.Common) counter++;
        }
        return counter;
    }

    #region Potential boosters to unlock

    [Server]
    public void ShowPotentialBoosters()
    {
        Debug.LogWarning(" BOOSTERMANAGER / Showing potential boosters");
        // All client side, as each client will have different boosters.
        PopulatePotentialBoosters();

        // Update visual client-side (RPC)
        UIManager.Instance.RpcUpdatePotentialCardsVisualAndShow(
            PotentialBoosterSlots[0].CurrentBooster.Name,
            PotentialBoosterSlots[0].CurrentBooster.Description,
            PotentialBoosterSlots[0].CurrentBooster.Rarity,
            PotentialBoosterSlots[1].CurrentBooster.Name,
            PotentialBoosterSlots[1].CurrentBooster.Description,
            PotentialBoosterSlots[1].CurrentBooster.Rarity,
            PotentialBoosterSlots[2].CurrentBooster.Name,
            PotentialBoosterSlots[2].CurrentBooster.Description,
            PotentialBoosterSlots[2].CurrentBooster.Rarity);
    }

    [Server]
    private void PopulatePotentialBoosters()
    {
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {

            player.BoosterManager.PotentialBoosterSlots = new BoosterSlot[MAX_POTENTIAL_SLOTS];
            for (int i = 0; i < MAX_POTENTIAL_SLOTS; i++)
            {
                player.BoosterManager.PotentialBoosterSlots[i] = new BoosterSlot(_boosterContainer.GetRandomBoosterEntry().GetBoosterAsInterface(), false, i);
                Debug.Log(player.name + " got potential slot " + i + " = " + player.BoosterManager.PotentialBoosterSlots[i]);
            }
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
}
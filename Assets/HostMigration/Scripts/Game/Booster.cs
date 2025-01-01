using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class BoosterContainer : MonoBehaviour
{
    public List<BoosterEntry> Boosters = new(); // List of all boosters


    public IBooster GetBoosterByName(string name)
    {
        foreach (var boosterMono in Boosters)
        {
            if(boosterMono.BoosterScript is IBooster booster)
            {
                if (booster.Name == name) return booster;
            }
        }
        Debug.LogError("No such booster found with name {name}");
        return null;
    }

    public BoosterEntry GetRandomBoosterEntry()
    {
        // Calculate the total weight
        float totalWeight = 0f;
        foreach (var booster in Boosters)
        {
            totalWeight += booster.Chance;
        }

        float randomValue = Random.Range(0, totalWeight);

        // Find the corresponding booster
        float currentWeight = 0f;
        foreach (var booster in Boosters)
        {
            currentWeight += booster.Chance;
            if (randomValue <= currentWeight)
            {
                return booster;
            }
        }

        // Fallback (this should not happen if weights are set correctly)
        Debug.LogWarning("GetRandomBooster failed to select a booster. Check the weights!");
        return null;
    }

    public override string ToString()
    {
        return $"{name} has {Boosters.Count} boosters assigned";
    }
}

public interface IBooster
{
    string Name { get; }
    string Description { get; }
    BoosterRarity Rarity { get; }
}

public enum BoosterRarity
{
    Common, Legendary, Chaos
}

public interface IBoosterPermanent : IBooster
{
    [Command(requiresAuthority =false)]
    void CmdAddPermanentEffect(Player player);
    [Command(requiresAuthority = false)]
    void CmdRemovePermanentEffect(Player player);
}

public interface IBoosterConsumable : IBooster
{
    void CmdConsumeEffect(Player player);
}

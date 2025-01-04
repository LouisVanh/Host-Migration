using UnityEngine;
using System.Collections.Generic;
using Mirror;

public class BoosterContainer : NetworkBehaviour
{
    public static string BoosterContainerTag = "BoosterContainer";
    public static List<BoosterEntry> Boosters = new();
    [ReadOnly, SerializeField] BoosterEntry Booster1, Booster2, Booster3, Booster4, Booster5, Booster6; // Debug

    protected override void OnValidate()
    {
        if (Application.isPlaying) return;
        base.OnValidate();
        // Get references
        Booster1 = transform.GetChild(0).GetComponent<BoosterEntry>();
        Booster2 = transform.GetChild(1).GetComponent<BoosterEntry>();
        Booster3 = transform.GetChild(2).GetComponent<BoosterEntry>();
        Booster4 = transform.GetChild(3).GetComponent<BoosterEntry>();
        Booster5 = transform.GetChild(4).GetComponent<BoosterEntry>();
        Booster6 = transform.GetChild(5).GetComponent<BoosterEntry>();
    }

    // Used to have this, but this would sometimes not work?
    //public override void OnStartClient()
    //{
    //    base.OnStartClient();
    //    var parent = GameObject.FindWithTag(BoosterContainerTag);
    //    for (int i = 0; i < parent.transform.childCount; i++)
    //    {
    //        var child = parent.transform.GetChild(i);
    //        if (child.gameObject.activeSelf)
    //            Boosters.Add(child.GetComponent<BoosterEntry>());
    //    }
    //}

    public override void OnStartClient()
    {
        base.OnStartClient();
        Boosters.Clear();
        Boosters.Add(Booster1);
        Boosters.Add(Booster2);
        Boosters.Add(Booster3);
        Boosters.Add(Booster4);
        Boosters.Add(Booster5);
        Boosters.Add(Booster6);
        Debug.Log(Boosters);
    }


    public static IBooster GetFirstBoosterByName(string name)
    {
        foreach (var boosterMono in Boosters)
        {
            if (boosterMono.BoosterScript is IBooster booster)
            {
                if (booster.Name == name) return booster;
            }
        }
        Debug.LogError("No such booster found with name {name}");
        return null;
    }

    public static BoosterEntry GetRandomBoosterEntry()
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
    [Command(requiresAuthority = false)]
    void CmdAddPermanentEffect(Player player);
    [Command(requiresAuthority = false)]
    void CmdRemovePermanentEffect(Player player);
}

public interface IBoosterConsumable : IBooster
{
    void CmdConsumeEffect(Player player);
}

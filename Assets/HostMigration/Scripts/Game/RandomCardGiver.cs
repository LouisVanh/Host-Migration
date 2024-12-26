//using UnityEngine;
//using System.Collections.Generic;

//public class RandomCardGiver : MonoBehaviour
//{
//    [SerializeField] private List<IBooster> _allBoosters = new();

//    /// <summary>
//    /// Returns a random booster card based on their assigned chances.
//    /// </summary>
//    /// <returns>The selected booster card.</returns>
//    public IBooster RandomBoosterCard()
//    {
//        if (_allBoosters.Count == 0) return default;

//        // Calculate the total weight (sum of all chances)
//        float totalWeight = 0f;
//        foreach (var booster in _allBoosters)
//        {
//            totalWeight += booster.Chance;
//        }

//        // Generate a random number between 0 and the total weight
//        float randomValue = Random.Range(0f, totalWeight);

//        // Iterate through the boosters to find which one corresponds to the random value
//        float cumulativeWeight = 0f;
//        foreach (var booster in _allBoosters)
//        {
//            cumulativeWeight += booster.Chance;
//            if (randomValue <= cumulativeWeight)
//            {
//                return booster.BoosterScript;
//            }
//        }

//        // Fallback (should not occur if weights are set correctly)
//        return _allBoosters[0].BoosterScript;
//    }
//}

using UnityEngine;

public class BoosterEntry : MonoBehaviour
{
    public MonoBehaviour BoosterScript; // The booster script, which has to use the interface IBooster
    public float Chance; // The chance (weight) of this booster being selected

    public IBooster GetBoosterAsInterface()
    {
        if (BoosterScript is IBooster booster)
        {
            return booster;
        }
        Debug.LogError($"The BoosterScript {BoosterScript.name} does not implement IBooster!");
        return null;
    }
}

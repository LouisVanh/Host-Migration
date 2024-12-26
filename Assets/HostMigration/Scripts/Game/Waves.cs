using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Wave CurrentWave;
    public int CurrentWaveIndex;
    public int StandardEnemyHealth;
    public Enemy CurrentEnemy;

    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    public void CreateNewWave()
    {
        CurrentWave = new Wave(5);
    }

    public void AdvanceToNextEnemy()
    {
        if (CurrentWaveIndex == CurrentWave.TotalEnemiesCount - 1) Debug.Log("BOSS COMING UP NEXT!");// Right before the boss 
        if(CurrentWaveIndex == CurrentWave.TotalEnemiesCount) // If this was the boss
        {
            SuccesfullyBeatWave();
            return;
        }

        CurrentEnemy = new Enemy(StandardEnemyHealth, GetRandomEnemyType());
        CurrentWave.CurrentEnemyIndex++;
    }

    public void SuccesfullyBeatWave()
    {
        Debug.LogWarning("Insert reward here (unimplemented)");
        TurnManager.Instance.UpdateGameState(GameState.EveryonePickBooster);
    }

    public EnemyType GetRandomEnemyType()
    {
        var random = UnityEngine.Random.Range(1, 4);
        switch (random)
        {
            case 1:
                return EnemyType.DefaultEnemyCapsule;
            case 2:
                return EnemyType.DefaultEnemyCube;
            case 3:
                return EnemyType.DefaultEnemySphere;
            default:
                return EnemyType.DefaultEnemyCapsule;
        }
    }
}

public class Wave
{
    public int TotalEnemiesCount;
    public int CurrentEnemyIndex;

    public Wave(int totalEnemiesCount)
    {
        TotalEnemiesCount = totalEnemiesCount;
    }
}
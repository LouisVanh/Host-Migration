using UnityEngine;
using Mirror;
using System.Collections;

public class WaveManager : NetworkBehaviour
{
    public Wave CurrentWave;
    public int CurrentWaveIndex = 1;
    public int StandardEnemyHealth;
    public Enemy CurrentEnemy;
    public GameObject EnemyScriptPrefab;

    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    [Server]
    public void CreateNewWave()
    {
        Debug.Log("CREATING NEW WAVE! Enemy will spawn after this");
        CurrentWave = new Wave(5);
        SpawnEnemy();
    }

    [Server]
    public async void AdvanceToNextEnemy()
    {
        Debug.LogWarning($"Current wave: {CurrentWaveIndex}, Current Enemy: {CurrentWave.CurrentEnemyIndex}, Total Enemies: {CurrentWave.TotalEnemiesCount}");
        if (CurrentWave.CurrentEnemyIndex == CurrentWave.TotalEnemiesCount - 1) Debug.Log("BOSS COMING UP NEXT!");// Right before the boss 
        await System.Threading.Tasks.Task.Delay(1000);
        if (CurrentWave.CurrentEnemyIndex == CurrentWave.TotalEnemiesCount) // If this was the boss
        {
            SuccesfullyBeatWave(); // Go to pick booster
            return;
        }

        SpawnEnemy();
    }

    [Server]
    private void SpawnEnemy()
    {
        var scriptObj = Instantiate(EnemyScriptPrefab, Vector3.zero, Quaternion.identity);
        CurrentEnemy = scriptObj.GetComponent<Enemy>();
        NetworkServer.Spawn(scriptObj); // Ensure object is network-spawned
        CurrentEnemy.CmdSetupEnemyVisual(GetRandomEnemyType());
        CurrentEnemy.CmdSyncHealthBarValue(StandardEnemyHealth);
        CurrentWave.CurrentEnemyIndex++;
    }

    [Server]
    public void SuccesfullyBeatWave()
    {
        Debug.LogWarning("Succesfully beat wave: Insert reward here (unimplemented)");
        CurrentWaveIndex++;
        DiceManager.Instance.ResetAfterWaveComplete();
        TurnManager.Instance.UpdateGameState(GameState.EveryonePickBooster);
    }

    [Server]
    public EnemyType GetRandomEnemyType()
    {
        var random = UnityEngine.Random.Range(1, 4);
        switch (random)
        {
            case 1:
                Debug.Log("Setting random enemy type: Capsule");
                return EnemyType.DefaultEnemyCapsule;
            case 2:
                Debug.Log("Setting random enemy type: Cube");
                return EnemyType.DefaultEnemyCube;
            case 3:
                Debug.Log("Setting random enemy type: Sphere");
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
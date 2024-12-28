using UnityEngine;
using Mirror;
using System.Collections;

public class WaveManager : NetworkBehaviour
{
    public Wave CurrentWave;
    public int CurrentWaveIndex;
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
        CurrentWave = new Wave(5);
        StartCoroutine(SpawnEnemy());
    }

    [Server]
    public async void AdvanceToNextEnemy()
    {
        await System.Threading.Tasks.Task.Delay(1000);
        if (CurrentWaveIndex == CurrentWave.TotalEnemiesCount - 1) Debug.Log("BOSS COMING UP NEXT!");// Right before the boss 
        if (CurrentWaveIndex == CurrentWave.TotalEnemiesCount) // If this was the boss
        {
            SuccesfullyBeatWave();
            return;
        }

        StartCoroutine(
                SpawnEnemy());
    }

    [Server]
    private IEnumerator SpawnEnemy()
    {
        var scriptObj = Instantiate(EnemyScriptPrefab, Vector3.zero, Quaternion.identity);
        CurrentEnemy = scriptObj.GetComponent<Enemy>();
        NetworkServer.Spawn(scriptObj); // Ensure object is network-spawned
        CurrentEnemy.CmdSetupEnemy(StandardEnemyHealth, GetRandomEnemyType());
        CurrentWave.CurrentEnemyIndex++;

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        RpcCreateEnemyHealthBar(CurrentEnemy.gameObject.GetComponent<NetworkIdentity>().netId);
        yield return null;
    }

    [ClientRpc]
    private void RpcCreateEnemyHealthBar(uint enemyNetId)
    {
        if (NetworkServer.spawned.TryGetValue(enemyNetId, out NetworkIdentity enemyObj))
        {
            enemyObj.GetComponent<Enemy>().CreateEnemyHealthBar(StandardEnemyHealth);
        }
        else Debug.LogWarning("No enemy found with that netid"); // Add frame delay?
    }
    [Server]
    public void SuccesfullyBeatWave()
    {
        Debug.LogWarning("Insert reward here (unimplemented)");
        TurnManager.Instance.UpdateGameState(GameState.EveryonePickBooster);
    }

    [Server]
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
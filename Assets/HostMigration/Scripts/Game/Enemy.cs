using Mirror;
using UnityEngine;

public enum EnemyType
{
    DefaultEnemyCube, DefaultEnemyCapsule, DefaultEnemySphere
}
public class Enemy : NetworkBehaviour
{
    public int Health => HealthBar.CurrentHealth;
    public EnemyType EnemyType { get; private set; }
    [SerializeField] GameObject EnemyHealthBarVisual;
    [SerializeField] GameObject EnemyHealthBarScriptPrefab;
    public HealthBar HealthBar { get; set; }
    public GameObject CurrentEnemyVisual { get; set; }

    private GameObject ChosenVisual;
    [SerializeField] GameObject VisualCubePrefab;
    [SerializeField] GameObject VisualCapsulePrefab;
    [SerializeField] GameObject VisualSpherePrefab;

    public virtual void SetupEnemy(int health, EnemyType enemyType)
    {
        if (isLocalPlayer)
        {
            var scriptObj = Instantiate(EnemyHealthBarScriptPrefab, Vector3.zero, Quaternion.identity);
            HealthBar = scriptObj.GetComponent<HealthBar>();
            NetworkServer.Spawn(scriptObj); // Ensure object is network-spawned
            HealthBar.SetupHealthBar(EnemyHealthBarVisual, health);
            HealthBar.SetPositionOfHealthBarEnemy();
            Debug.Log("Setting up health bar for enemy " + this.gameObject.name + "!");
        }
        EnemyType = enemyType;
        ChosenVisual = enemyType switch
        {
            EnemyType.DefaultEnemyCube => VisualCubePrefab,
            EnemyType.DefaultEnemyCapsule => VisualCubePrefab,
            EnemyType.DefaultEnemySphere => VisualCubePrefab,
            _ => VisualCapsulePrefab,
        };
        var spawnPos = GameObject.FindWithTag("EnemySpawnLocation").transform.position;
        CurrentEnemyVisual = Instantiate(ChosenVisual, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(CurrentEnemyVisual);
        CurrentEnemyVisual.transform.localScale *= 5;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        HealthBar.CurrentHealth -= damage;

        if (Health <= 0)
        {
            Die();
        }
        else
        {
            PlayHitAnimation();
        }
    }

    [ClientRpc]
    private void PlayHitAnimation()
    {
        // Trigger hit animation
        Debug.LogWarning("Play hit animation, but unimplemented");
    }

    [ClientRpc]
    public virtual void Die()
    {
        // Handle enemy death logic (go to next enemy)
        Debug.LogWarning("Enemy died, but unimplemented");
        WaveManager.Instance.AdvanceToNextEnemy();
        Destroy(this.gameObject);
    }
}

public class Boss : Enemy
{
    public override void SetupEnemy(int health, EnemyType bossType)
    {
        base.SetupEnemy(health, bossType);
        // optional: other health bar visual here
    }

    [ClientRpc]
    public override void Die() // FUN FACT THIS MIGHT NEVER RUN, GOT NO CLUE LOL
    {
        base.Die();
        Debug.LogError("Boss died, but unimplemented");
        // ADD EFFECTS HERE
    }
}
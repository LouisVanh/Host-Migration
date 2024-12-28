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

    [Header("Animation")]
    private float _normalScale = 5;
    private float _hitScale = 5.5f;
    private float _hitShakeDuration = 0.3f;

    [Command(requiresAuthority = false)]
    public virtual void CmdSetupEnemy(int health, EnemyType enemyType)
    {
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
        CurrentEnemyVisual.transform.localScale *= 5; // automatically networked thanks to NT

        CreateEnemyHealthBar(health);
    }

    public void CreateEnemyHealthBar(int health)
    {
        HealthBar = GetComponent<HealthBar>();
        Debug.Log("START OF setting up health bar for enemy " + this.gameObject.name + "!");
        HealthBar.SetupOwnHealthBar(EnemyHealthBarVisual, health);
        Debug.Log("END OF setting up health bar for enemy " + this.gameObject.name + "!");
        // if this doesn't work, consider looping through all players, and 
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

    private void PlayHitAnimation()
    {
        // Trigger hit animation
        var randomX = Random.Range(-15, 15);
        var randomY = Random.Range(-15, 15);
        var randomZ = Random.Range(-15, 15);
        Vector3 rotation = new(randomX, randomY, randomZ);

        Vector3 oldScale = new Vector3(_normalScale, _normalScale, _normalScale);
        Vector3 hitScale = new Vector3(_hitScale, _hitScale, _hitScale);
        LeanTween.scale(CurrentEnemyVisual, hitScale, _hitShakeDuration / 2).setOnComplete(() =>
          {
              LeanTween.scale(CurrentEnemyVisual, oldScale, _hitShakeDuration / 2);
          });
        LeanTween.rotate(CurrentEnemyVisual, rotation, _hitShakeDuration / 2).setOnComplete(() =>
        {
            LeanTween.rotate(CurrentEnemyVisual, Vector3.zero, _hitShakeDuration / 2);
        });
    }

    [ClientRpc]
    private void PlayHitAnimationFlash()
    {
        // Trigger hit animation (flash)
        Debug.LogWarning("Play hit animation flash, but unimplemented");
    }

    public virtual void Die()
    {
        Debug.LogWarning("Enemy died, but unimplemented");
        WaveManager.Instance.AdvanceToNextEnemy();
        NetworkServer.UnSpawn(CurrentEnemyVisual);
        NetworkServer.UnSpawn(HealthBar.HealthBarVisualInScene);
        NetworkServer.UnSpawn(this.gameObject);
    }
}

public class Boss : Enemy
{
    [Command(requiresAuthority = false)]
    public override void CmdSetupEnemy(int health, EnemyType bossType)
    {
        base.CmdSetupEnemy(health, bossType);
        // optional: other health bar visual here
    }

    public override void Die() // FUN FACT THIS MIGHT NEVER RUN, GOT NO CLUE LOL
    {
        base.Die();
        Debug.LogError("Boss died, but unimplemented");
        // ADD EFFECTS HERE
    }
}
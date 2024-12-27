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
    public HealthBar HealthBar;
    public GameObject CurrentEnemyVisual;

    [SerializeField] Transform SpawnPosition;
    [SerializeField] GameObject ChosenVisual;
    [SerializeField] GameObject VisualCubePrefab;
    [SerializeField] GameObject VisualCapsulePrefab;
    [SerializeField] GameObject VisualSpherePrefab;

    public Enemy(int health, EnemyType enemyType)
    {
        var obj = Instantiate(new GameObject());
        HealthBar = (HealthBar) obj.AddComponent(typeof(HealthBar));
        HealthBar.TotalHealth = health;
        HealthBar.CurrentHealth = health;
        HealthBar.Visual = EnemyHealthBarVisual;
        EnemyType = enemyType;
        ChosenVisual = enemyType switch
        {
            EnemyType.DefaultEnemyCube => VisualCubePrefab,
            EnemyType.DefaultEnemyCapsule => VisualCubePrefab,
            EnemyType.DefaultEnemySphere => VisualCubePrefab,
            _ => VisualCapsulePrefab,
        };

        CurrentEnemyVisual = Instantiate(ChosenVisual, SpawnPosition.transform.position, Quaternion.identity);
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
    public Boss(int health, EnemyType bossType)
        : base(health, bossType)
    {
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
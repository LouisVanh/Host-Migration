using UnityEngine;

public enum EnemyType
{
    DefaultEnemyCube, DefaultEnemyCapsule, DefaultEnemySphere
}
public class Enemy
{
    public int Health { get; private set; }
    public EnemyType EnemyType { get; private set; }
    public GameObject Visual { get; private set; }

    public Enemy(int health, EnemyType enemyType, GameObject visual)
    {
        Health = health;
        EnemyType = enemyType;
        Visual = visual;
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
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
    }

    private void Die()
    {
        // Handle enemy death logic
    }
}

public class Boss : Enemy
{
    public Boss(int health, EnemyType bossType, GameObject visual)
        : base(health, bossType, visual)
    {
    }

    private void Die()
    {

    }
}
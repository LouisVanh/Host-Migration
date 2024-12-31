using Mirror;
using UnityEngine;

public enum EnemyType
{
    DefaultEnemyCube, DefaultEnemyCapsule, DefaultEnemySphere
}
public class Enemy : NetworkBehaviour
{
    [SyncVar]
    public bool IsBoss;
    [SyncVar(hook = nameof(OnDefaultHealthChanged))]
    private int _defaultHealth = 99999999; // Should be overridden
    private void OnDefaultHealthChanged(int oldValue, int newValue)
    {
        //Debug.Log("ENEMY/ ONDEFAULTHEALTHCHANGED / " + oldValue + " ---) " + newValue);
        InitHealthBar(oldValue, newValue);
    }
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
    private float _hitScale => _normalScale * 1.1f;
    private float _hitShakeDuration = 0.3f;
    private float _attackRotateBackDuration = 0.3f;
    private float _attackRotateHitDuration = 0.2f;
    private float _attackRotateCorrectionDuration = 0.1f;

    [Command(requiresAuthority = false)]
    public void CmdSetupEnemyVisual(EnemyType enemyType, bool isBoss)
    {
        IsBoss = isBoss;
        EnemyType = enemyType;
        ChosenVisual = enemyType switch
        {
            EnemyType.DefaultEnemyCube => VisualCubePrefab,
            EnemyType.DefaultEnemyCapsule => VisualCapsulePrefab,
            EnemyType.DefaultEnemySphere => VisualSpherePrefab,
            _ => VisualCapsulePrefab,
        };
        var spawnPos = GameObject.FindWithTag("EnemySpawnLocation").transform.position;
        CurrentEnemyVisual = Instantiate(ChosenVisual, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(CurrentEnemyVisual);
        _normalScale = (isBoss ? 7.5f : 5.1f);
        CurrentEnemyVisual.transform.localScale *= (_normalScale); // automatically networked thanks to NT
        Debug.Log(CurrentEnemyVisual.transform.localScale);
    }

    [Command(requiresAuthority = false)]
    public void CmdSyncHealthBarValue(int health)
    {
        _defaultHealth = health; // Synced server to client
    }

    public override void OnStopClient()
    {
        //Debug.LogWarning("ENEMY / ONSTOPCLIENT / Destroying health bar visual in scene!");
        if (this.HealthBar.HealthBarVisualInScene)
        {
            Destroy(this.HealthBar.HealthBarVisualInScene);
        }
        base.OnStopClient();
    }

    private void InitHealthBar(int oldValue, int newValue)
    {
        // DO NOT make this cmd/rpc.
        if (oldValue == newValue) return; // If defaulthealth doesn't change, something is really wrong.
        Debug.Log("Hey! the health changed to start off, I can make the healthbar now!");
        HealthBar = GetComponent<HealthBar>();
        HealthBar.SetupOwnHealthBar(EnemyHealthBarVisual, newValue);
    }

    [Server]
    public void TakeDamage(int damage, out int leftOverEyes, out bool enemyDied)
    {
        if (Health <= 0)// IF ALREADY DEAD, STOP KICKIN EM
        {
            Debug.LogWarning("Take Damage called when enemy was already dead");
            leftOverEyes = -1;
            enemyDied = false;
            return;
        }
        HealthBar.CurrentHealth -= damage;

        if (Health <= 0)
        {
            leftOverEyes = Mathf.Abs(Health);
            enemyDied = true;
            Die();
        }
        else // SURVIVED!
        {
            leftOverEyes = 0;
            enemyDied = false;
            RpcPlayHitAnimationFlash();
            PlayHitAnimation();
        }
    }

    [Server]
    public void EnemyAttackDealDamage(int damage)
    {
        PlayAttackAnimation();
        foreach (var player in PlayersManager.Instance.GetPlayers())
        {
            player.CmdTakeDamage(damage, _attackRotateHitDuration + _attackRotateBackDuration);
        }
    }

    [Server]
    private void PlayHitAnimation() // synced through NT
    {
        // Scale and rotate like a small punch
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

    [Server]
    private void PlayAttackAnimation() // synced through NT
    {
        // Scale and rotate like a small punch
        Vector3 beginRotation = Vector3.zero;
        Vector3 backRotation = new(15, 0, 0);
        Vector3 frontRotation = new(-45, 0, 0);

        LeanTween.rotate(CurrentEnemyVisual, backRotation, _attackRotateBackDuration).setOnComplete(() =>
        {
            LeanTween.rotate(CurrentEnemyVisual, frontRotation, _attackRotateHitDuration).setOnComplete(() =>
            {
                LeanTween.rotate(CurrentEnemyVisual, beginRotation, _attackRotateCorrectionDuration);
            });
        });
    }

    [ClientRpc]
    private void RpcPlayHitAnimationFlash()
    {
        // Trigger hit animation (flash)
        Debug.LogWarning("Play hit animation flash, but unimplemented");
    }

    [Server]
    public virtual void Die()
    {
        Debug.LogWarning("I'm dead as hell bruh");
        WaveManager.Instance.AdvanceToNextEnemy();
        NetworkServer.UnSpawn(CurrentEnemyVisual);
        NetworkServer.UnSpawn(this.gameObject);
        // Health bar is despawned OnStopClient
    }
}
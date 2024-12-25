public class Wave
{
    public int TotalEnemiesCount { get; private set; }
    public int CurrentEnemyIndex { get; private set; }
    public int CurrentWaveIndex { get; private set; }

    public Wave(int totalEnemiesCount, int currentWaveIndex)
    {
        TotalEnemiesCount = totalEnemiesCount;
        CurrentWaveIndex = currentWaveIndex;
        CurrentEnemyIndex = 0;
    }

    public void AdvanceToNextEnemy()
    {
        CurrentEnemyIndex++;
    }
}
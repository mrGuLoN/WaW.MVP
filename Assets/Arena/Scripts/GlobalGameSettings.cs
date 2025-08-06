// GlobalGameSettings.cs
using UnityEngine;
using System.Collections.Generic;
using Arena.Scripts.Controllers;

[CreateAssetMenu(fileName = "GlobalGameSettings", menuName = "Game/Global Settings", order = 0)]
public class GlobalGameSettings : ScriptableObject
{
    [Header("Настройки сложности")]
    [Tooltip("Через сколько этапов увеличивать сложность")]
    public int stagesForDifficultyJump = 5;

    public float upEnemyInWave = 0.1f;
    public float upEnemyInMap = 0.1f;

    [Header("Экономика")]
    [Tooltip("Базовая стоимость врага")]
    public float baseCostCoff = 1f;
    public float baseHealthCoff = 1f;
    public float baseDamageCoff = 1f;
    
    [Tooltip("Базовое увеличение")]
    public float baseCostUp = 0.1f;
    public float baseHealthUp = 0.1f;
    public float baseDamageUp = 0.1f;

    [Header("Ограничения")]
    [Tooltip("Абсолютный максимум врагов на экране")]
    public int absoluteMaxEnemiesOnScreen = 100;

    [Header("Появление врагов")]
    [Tooltip("Настройки появления типов врагов")]
    public List<EnemyAppearanceSettings> enemyAppearanceSettings = new List<EnemyAppearanceSettings>();

    [Header("Боссы")]
    [Tooltip("Настройки появления боссов")]
    public List<BossAppearanceSettings> bossAppearanceSettings = new List<BossAppearanceSettings>();
}

[System.Serializable]
public class EnemyAppearanceSettings
{
    public EnumEnemyType enemyType;
    public int appearFromStage = 0;
    public int appearFromWave = 0;
}

[System.Serializable]
public class BossAppearanceSettings
{
    public EnumEnemyType bossType;
    public int appearEveryStages = 5;
    public int appearEveryWaves = 10;
}
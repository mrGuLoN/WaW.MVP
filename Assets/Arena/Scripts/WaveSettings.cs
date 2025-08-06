// WaveSettings.cs
using System;
using System.Collections.Generic;
using Arena.Scripts.Controllers;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveSettings", menuName = "Game/Wave Settings", order = 2)]
public class WaveSettings : ScriptableObject
{
    [Header("Основные параметры")]
    [Tooltip("Общее количество врагов в волне")]
    public int totalEnemies = 20;
    [Tooltip("Максимальное количество врагов на экране одновременно")]
    public int maxEnemiesOnScreen = 10;
    [Tooltip("Минимальное количество врагов на экране, при котором начинается респавн")]
    public int minEnemiesToRespawn = 3;

    [Header("Время")]
    [Tooltip("Задержка между спавнами врагов (минимальная)")]
    public float spawnDelayMin = 0.5f;
    [Tooltip("Задержка между спавнами врагов (максимальная)")]
    public float spawnDelayMax = 1.5f;
    [Tooltip("Время между волнами (в секундах)")]
    public float timeBetweenWaves = 10f;

    [Header("Враги")]
    [Tooltip("Настройки типов врагов, появляющихся в этой волне")]
    public List<EnemySettings> enemySettings = new List<EnemySettings>();
}
[Serializable]
public class EnemySettings
{
    [Tooltip("Тип врага")]
    public EnumEnemyType type;
}
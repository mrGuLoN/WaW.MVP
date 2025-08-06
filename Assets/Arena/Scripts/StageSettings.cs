// StageSettings.cs
using System.Collections.Generic;
using Arena.Scripts.Controllers;
using UnityEngine;

[CreateAssetMenu(fileName = "StageSettings", menuName = "Game/Stage Settings", order = 1)]
public class StageSettings : ScriptableObject
{
    [SerializeField] private int _waveCount;
    public int waveCount => _waveCount;
    [Header("Волны")]
    [Tooltip("Список волн на этом этапе")]
    public List<WaveSettings> waves = new List<WaveSettings>();
  
}
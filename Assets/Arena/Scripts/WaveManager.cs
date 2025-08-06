using System.Collections.Generic;
using System.Linq;
using Arena.Scripts.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Arena.Scripts
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GlobalGameSettings _globalSettings;
        public GlobalGameSettings GlobalSettings => _globalSettings;
        [SerializeField] private List<StageSettings> _stageConfigurations;
        [SerializeField] private ArenaEnemyController _enemyController;
        [SerializeField] private RectTransform _nextStageGO;
        [SerializeField] private RectTransform _fightCanvasGO;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private TMP_Text _waveInfoText;
        [SerializeField] private TMP_Text _timeBetweenWavesText;
        [SerializeField] private Camera _cameraFight;

        [Header("Events")]
        public UnityEvent OnStageStarted;
        public UnityEvent OnStageCompleted;
        public UnityEvent OnWaveStarted;
        public UnityEvent OnWaveCompleted;

        private StageSettings _currentStage;
        private WaveSettings _currentWaveSettings;
        private int _currentWaveNumber = 0;
        private int _currentStageIndex = 0;
        private int _enemiesSpawnedInWave = 0;
        private int _enemiesKilledInWave = 0;
        private float _timeBetweenWavesRemaining = 0f;
        private bool _isBetweenWaves = false;
        private int _currentLvl;
        public int CurrentLvl => _currentLvl;
        private float _nextSpawnTime;
        private bool _spawningActive;

        private void Awake()
        {
            _nextStageButton.onClick.AddListener(StartNextStage);
            _nextStageButton.gameObject.SetActive(false);
            _nextStageGO.gameObject.SetActive(false);
            _fightCanvasGO.gameObject.SetActive(true);
        
            _enemyController.OnEnemyDied += HandleEnemyDeath;
        }

        private void Start()
        {
            StartRandomStage();
        }

        private void Update()
        {
            if (_isBetweenWaves)
            {
                UpdateBetweenWavesTimer();
            }
            else if (_spawningActive)
            {
                TrySpawnEnemies();
            }
        }

        private void UpdateBetweenWavesTimer()
        {
            _timeBetweenWavesRemaining -= Time.deltaTime;
            _timeBetweenWavesText.text = $"До следующей волны: {Mathf.Ceil(_timeBetweenWavesRemaining)}с";
        
            if (_timeBetweenWavesRemaining <= 0)
            {
                _isBetweenWaves = false;
                _timeBetweenWavesText.text = "";
                StartNextWave();
            }
        }

        private void TrySpawnEnemies()
        {
            // Если уже заспавнили всех врагов или не пришло время спавна
            if (_enemiesSpawnedInWave >= _currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave || Time.time < _nextSpawnTime)
                return;

            int activeEnemies = _enemyController.GetActiveEnemiesCount();
            int remainingEnemies =  (int)(_currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave) - _enemiesSpawnedInWave;

            // Если врагов меньше минимального порога или можно добавить до максимума
            if (activeEnemies < _currentWaveSettings.minEnemiesToRespawn || 
                (activeEnemies < _currentWaveSettings.maxEnemiesOnScreen && activeEnemies < remainingEnemies))
            {
                SpawnEnemy();
                _nextSpawnTime = Time.time + Random.Range(_currentWaveSettings.spawnDelayMin, _currentWaveSettings.spawnDelayMax);
            }
        }

        public void StartRandomStage()
        {
            _cameraFight.gameObject.SetActive(true);
            if (_stageConfigurations.Count == 0)
            {
                Debug.LogError("No stage configurations available!");
                return;
            }

            _currentStageIndex = Random.Range(0, _stageConfigurations.Count);
            _currentStage = _stageConfigurations[_currentStageIndex];
            _currentWaveNumber = 0;
            _currentLvl++;
        
            _nextStageButton.gameObject.SetActive(false);
            _nextStageGO.gameObject.SetActive(false);
            _fightCanvasGO.gameObject.SetActive(true);
        
            OnStageStarted?.Invoke();
            StartNextWave();
        }

        public void StartNextStage()
        {
            StartRandomStage();
        }

        private void StartNextWave()
        {
            _currentWaveNumber++;
        
            if (_currentWaveNumber > _currentStage.waveCount)
            {
                CompleteStage();
                return;
            }

            // Выбираем случайную волну из доступных
            int randomIndex = Random.Range(0, _currentStage.waves.Count);
            _currentWaveSettings = _currentStage.waves[randomIndex];
           

            // Для последней волны проверяем босса
            if (_currentWaveNumber == _currentStage.waveCount)
            {
                TryAddBossToWave();
            }

            _enemiesSpawnedInWave = 0;
            _enemiesKilledInWave = 0;
            _spawningActive = true;
        
            UpdateUI();
            OnWaveStarted?.Invoke();
        
            // Начальный спавн
            SpawnInitialEnemies();
        }

        private void TryAddBossToWave()
        {
            foreach (var bossSetting in _globalSettings.bossAppearanceSettings)
            {
                if (_currentLvl % bossSetting.appearEveryWaves == 0)
                {
                    // Добавляем босса как отдельного врага
                    _currentWaveSettings.totalEnemies++; // Увеличиваем общее количество врагов
                    break;
                }
            }
        }

        private void SpawnInitialEnemies()
        {
            int enemiesToSpawn = Mathf.Min(
                _currentWaveSettings.maxEnemiesOnScreen + (int)(_currentWaveSettings.maxEnemiesOnScreen*(_currentLvl-1)*_globalSettings.upEnemyInMap),
                _currentWaveSettings.totalEnemies
            );

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnEnemy();
            }
        
            _nextSpawnTime = Time.time + Random.Range(_currentWaveSettings.spawnDelayMin, _currentWaveSettings.spawnDelayMax);
        }

        private void HandleEnemyDeath(EnumEnemyType enemyType)
        {
            _enemiesKilledInWave++;
        
            // Проверка на босса
            foreach (var bossSetting in _globalSettings.bossAppearanceSettings)
            {
                if (bossSetting.bossType == enemyType)
                {
                    _enemyController.KillAllEnemies();
                    _enemiesKilledInWave = (int)(_currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave);
                    break;
                }
            }
        
            UpdateUI();
        
            // Проверяем условия завершения волны
            if (_enemiesKilledInWave >= (int)(_currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave))
            {
                CompleteWave();
            }
        }

        private void SpawnEnemy()
        {
            if (_enemiesSpawnedInWave >= (int)(_currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave)) 
                return;

            var enemyType = GetEnemyTypeForSpawn();
            _enemyController.SpawnEnemy(enemyType);
            _enemiesSpawnedInWave++;
        }

        private EnumEnemyType GetEnemyTypeForSpawn()
        {
            // Если это последний враг в последней волне и нужно спавнить босса
            if (_enemiesSpawnedInWave == _currentWaveSettings.totalEnemies - 1 && 
                _currentWaveNumber == _currentStage.waveCount)
            {
                foreach (var bossSetting in _globalSettings.bossAppearanceSettings)
                {
                    if (_currentLvl % bossSetting.appearEveryWaves == 0)
                    {
                        return bossSetting.bossType;
                    }
                }
            }

            // Фильтрация доступных врагов
            var availableEnemies = _currentWaveSettings.enemySettings
                .Where(e => IsEnemyTypeAvailable(e.type))
                .ToList();

            return availableEnemies.Count > 0 
                ? availableEnemies[Random.Range(0, availableEnemies.Count)].type 
                : EnumEnemyType.SimpleRunner;
        }

        private bool IsEnemyTypeAvailable(EnumEnemyType type)
        {
            var setting = _globalSettings.enemyAppearanceSettings
                .FirstOrDefault(e => e.enemyType == type);
        
            return setting != null && _currentLvl >= setting.appearFromWave;
        }

        private void CompleteWave()
        {
            _spawningActive = false;
            OnWaveCompleted?.Invoke();
        
            if (_currentWaveNumber < _currentStage.waveCount)
            {
                StartBetweenWavesTimer();
            }
            else
            {
                CompleteStage();
            }
        }

        private void StartBetweenWavesTimer()
        {
            _isBetweenWaves = true;
            _timeBetweenWavesRemaining = _currentWaveSettings.timeBetweenWaves;
        }

        private void CompleteStage()
        {
            OnStageCompleted?.Invoke();
            _nextStageButton.gameObject.SetActive(true);
            _nextStageGO.gameObject.SetActive(true);
            _cameraFight.gameObject.SetActive(false); 
            _fightCanvasGO.gameObject.SetActive(false);
        }

        private void UpdateUI()
        {
            _waveInfoText.text = $"Wave: {_currentWaveNumber}/{_currentStage.waveCount}\n" +
                                 $"Enemies: {_enemiesKilledInWave}/{(int)(_currentWaveSettings.totalEnemies +_currentWaveSettings.totalEnemies*(_currentLvl-1)*_globalSettings.upEnemyInWave)}";
        }
    }
}
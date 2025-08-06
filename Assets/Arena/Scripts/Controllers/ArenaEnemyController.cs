using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arena.Scripts.Player;
using FSG.MeshAnimator.Snapshot;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

namespace Arena.Scripts.Controllers
{
    public class ArenaEnemyController : MonoBehaviour
    {
        [Header("Enemy Settings")]
        [SerializeField] private EnemyData[] _enemyDatas;
        [SerializeField] private int _initialPoolSizePerType = 30;
        [SerializeField] private int _poolExpansionSize = 10;
        [SerializeField] private float _spawnDelayMin = 1f;
        [SerializeField] private float _spawnDelayMax = 2f;
        [SerializeField] private int _initialSpawnCount = 10;
        [SerializeField] private ArenaPlayerController _target;

        [Header("Spawn Lines")]
        [SerializeField] private List<SpawnLine> _spawnLines = new List<SpawnLine>();
        [SerializeField] private List<int> _activeLineIndices = new List<int>() { 0 };
        [SerializeField]   private WaveManager _waveManager;

        private Dictionary<EnumEnemyType, Queue<EnemyWrapper>> _enemyPools = new Dictionary<EnumEnemyType, Queue<EnemyWrapper>>();
        private Dictionary<EnumEnemyType, List<EnemyWrapper>> _activeEnemies = new Dictionary<EnumEnemyType, List<EnemyWrapper>>();
        private System.Random _random = new System.Random();

        // Job system
        private TransformAccessArray _transformAccessArray;
        private NativeArray<EnemyJobData> _enemyJobDataArray;
        private NativeArray<Vector3> _targetPositions;
        private JobHandle _jobHandle;
        public  Action<EnumEnemyType> OnEnemyDied;

        [System.Serializable]
        public class SpawnLine
        {
            public Vector2 pointA;
            public Vector2 pointB;
        }

        public struct EnemyWrapper
        {
            public GameObject gameObject;
            public Rigidbody2D rigidbody;
            public Transform transform;
            public EnemyInstanceData data;
            public EnumEnemyType type;
            public ArenaEnemyState enemyState;
        }

        public struct EnemyInstanceData
        {
            public float speed;
            public float attackDistanceSqr;
            public bool isAttacking;
            public int spawnLineIndex;
        }

        private struct EnemyJobData
        {
            public float speed;
            public float attackDistanceSqr;
            public Vector2 velocity;
            public bool shouldAttack;
        }

        [BurstCompile]
        private struct EnemyMovementJob : IJobParallelForTransform
        {
            [ReadOnly] public NativeArray<Vector3> targetPositions;
            public NativeArray<EnemyJobData> enemyData;

            public void Execute(int index, TransformAccess transform)
            {
                Vector3 direction = targetPositions[0] - transform.position;
                float distanceSqr = direction.x * direction.x + direction.y * direction.y;

                var data = enemyData[index];
                data.shouldAttack = distanceSqr <= data.attackDistanceSqr;
            
                if (!data.shouldAttack)
                {
                    Vector2 normalizedDir = new Vector2(direction.x, direction.y).normalized;
                    data.velocity = normalizedDir * data.speed;
                }
                else
                {
                    data.velocity = Vector2.zero;
                }

                enemyData[index] = data;
            }
        }

        void Start()
        {
            InitializePools();
            StartCoroutine(SpawnInitialEnemies());
            _transformAccessArray = new TransformAccessArray(0);
            _enemyJobDataArray = new NativeArray<EnemyJobData>(0, Allocator.Persistent);
            _targetPositions = new NativeArray<Vector3>(1, Allocator.Persistent);
        }

        void OnDestroy()
        {
            _transformAccessArray.Dispose();
            _enemyJobDataArray.Dispose();
            _targetPositions.Dispose();
            foreach (var data in _enemyDatas)
            {
                foreach (var prefabData in data._enemiesPrefab)
                {
                    if (prefabData._clonedAnimations != null)
                    {
                        foreach (var anim in prefabData._clonedAnimations)
                        {
                            if (anim != null)
                                Destroy(anim);
                        }
                        prefabData._clonedAnimations = null;
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (_target == null || _transformAccessArray.length == 0) return;
    
            _targetPositions[0] = _target.transform.position;
    
            var job = new EnemyMovementJob
            {
                targetPositions = _targetPositions,
                enemyData = _enemyJobDataArray
            };
    
            _jobHandle = job.Schedule(_transformAccessArray);
            _jobHandle.Complete();
    
            ApplyJobResults();
        }

        public void SetDamageToTarget(float damage, Vector3 direction)
        {
            _target.Damage(damage, direction);
        }

        private void ApplyJobResults()
        {
            for (int i = 0; i < _transformAccessArray.length; i++)
            {
                var transform = _transformAccessArray[i];
                var data = _enemyJobDataArray[i];
            
                Vector3 direction = _targetPositions[0] - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
          
                foreach (var kvp in _activeEnemies)
                {
                    for (int j = 0; j < kvp.Value.Count; j++)
                    {
                        if (kvp.Value[j].transform == transform)
                        {
                            var wrapper = kvp.Value[j]; 
                            if (wrapper.rigidbody != null)
                            {
                                wrapper.rigidbody.linearVelocity = data.velocity;
                            }

                            if (data.shouldAttack)
                            {
                                wrapper.enemyState.SetState(ArenaEnemyStateEnum.Hit);
                            }
                            else
                            {
                                wrapper.enemyState.SetPreviuseAnimations();
                            }
                       
                            EnemyInstanceData newData = wrapper.data; 
                            newData.isAttacking = data.shouldAttack; 

                            wrapper.data = newData;
                            kvp.Value[j] = wrapper; 
                            break;  
                        }
                    }
                }
            }
        }

        private void InitializePools()
        {
            foreach (var data in _enemyDatas)
            {
                _enemyPools[data._type] = new Queue<EnemyWrapper>();
                _activeEnemies[data._type] = new List<EnemyWrapper>();
        
                if (data._enemiesPrefab == null || data._enemiesPrefab.Length == 0)
                {
                    Debug.LogError($"No prefabs assigned for enemy type: {data._type}");
                    continue;
                }
        
                ExpandPool(data._type, _initialPoolSizePerType);
            }
        }

        private void ExpandPool(EnumEnemyType enemyType, int count)
        {
            var data = GetEnemyData(enemyType);
            if (data == null || data._enemiesPrefab.Length == 0)
            {
                Debug.LogError($"Invalid enemy data for type: {enemyType}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                int prefabIndex = i % data._enemiesPrefab.Length;
                var prefabData = data._enemiesPrefab[prefabIndex];
        
                if (prefabData._enemiesPrefab == null)
                {
                    Debug.LogError($"Enemy prefab is null at index {prefabIndex} for type {enemyType}");
                    continue;
                }

                if (prefabData._clonedAnimations == null)
                {
                    CloneAnimationsForPrefab(prefabData);
                }

                var enemyObj = Instantiate(prefabData._enemiesPrefab, transform);
                enemyObj.SetActive(false);

                var meshAnimator = enemyObj.GetComponent<ArenaEnemyState>();
                var rigidbody2D = enemyObj.GetComponent<Rigidbody2D>();
        
                if (meshAnimator == null || rigidbody2D == null)
                {
                    Debug.LogError($"Enemy prefab is missing required components: {prefabData._enemiesPrefab.name}");
                    Destroy(enemyObj);
                    continue;
                }

                if (meshAnimator != null)
                {
                    meshAnimator.SetAnimations(prefabData._clonedAnimations);
                }

                var wrapper = new EnemyWrapper
                {
                    gameObject = enemyObj,
                    rigidbody = rigidbody2D,
                    transform = enemyObj.transform,
                    type = enemyType,
                    enemyState = meshAnimator,
                    data = new EnemyInstanceData
                    {
                        speed = Random.Range(data._min2dVelocitySpeed, data._max2dVelocitySpeed),
                        attackDistanceSqr = data._distanceAttack * data._distanceAttack,
                        isAttacking = false,
                        spawnLineIndex = 0
                    }
                };
        
                _enemyPools[enemyType].Enqueue(wrapper);
            }
        }

        private void CloneAnimationsForPrefab(EnemyPrefabData prefabData)
        {
            if (prefabData._snapshotMeshAnimation == null || prefabData._snapshotMeshAnimation.Length == 0)
                return;

            prefabData._clonedAnimations = new SnapshotMeshAnimation[prefabData._snapshotMeshAnimation.Length];

            for (int i = 0; i < prefabData._snapshotMeshAnimation.Length; i++)
            {
                prefabData._clonedAnimations[i] = Instantiate(prefabData._snapshotMeshAnimation[i]);
            }
        }

        public void AddMoney(int money)
        {
            float baseCost = money*_waveManager.GlobalSettings.baseCostCoff*(1+Mathf.Pow(_waveManager.GlobalSettings.baseCostUp+1, _waveManager.CurrentLvl - 1)); 
            CurrencySystem.Instance.AddMoney((int)(baseCost * Random.Range(0.8f, 1.2f)));
        }
        
        public void RemoveEnemy(ArenaEnemyState enemy)
        {
            foreach (var kvp in _activeEnemies)
            {
                for (int i = kvp.Value.Count - 1; i >= 0; i--)
                {
                    if (kvp.Value[i].enemyState == enemy)
                    {
                        kvp.Value.RemoveAt(i);
                        UpdateJobSystemArrays();
                        return;
                    }
                }
            }
        }
        public void KillAllEnemies()
        {
            foreach (var kvp in _activeEnemies)
            {
                foreach (var enemy in kvp.Value.ToList()) // ToList для копии
                {
                    enemy.enemyState.DieImmediately(); // Нужно реализовать этот метод
                    ReturnEnemyToPool(enemy.enemyState);
                }
            }
        }

        public void ReturnEnemyToPool(ArenaEnemyState enemy)
        {
            foreach (var kvp in _activeEnemies)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (kvp.Value[i].enemyState == enemy)
                    {
                        var wrapper = kvp.Value[i];
                        wrapper.gameObject.SetActive(false);
                        _enemyPools[wrapper.type].Enqueue(wrapper);
                        kvp.Value.RemoveAt(i);
                        UpdateJobSystemArrays();
                        return;
                    }
                }
            }
    
            var tempWrapper = new EnemyWrapper
            {
                gameObject = enemy.gameObject,
                rigidbody = enemy.GetComponent<Rigidbody2D>(),
                transform = enemy.transform,
                enemyState = enemy,
                type = EnumEnemyType.SimpleRunner
            };
            _enemyPools[tempWrapper.type].Enqueue(tempWrapper);
            enemy.gameObject.SetActive(false);
            OnEnemyDied?.Invoke(tempWrapper.type);
        }

        private IEnumerator SpawnInitialEnemies()
        {
            for (int i = 0; i < _initialSpawnCount; i++)
            {
                SpawnRandomEnemy();
                float delay = Random.Range(_spawnDelayMin, _spawnDelayMax);
                yield return new WaitForSeconds(delay);
            }
        }

        private void SpawnRandomEnemy()
        {
            if (_enemyDatas.Length == 0 || _activeLineIndices.Count == 0) return;
            var enemyType = _enemyDatas[Random.Range(0, _enemyDatas.Length)]._type;
            SpawnEnemy(enemyType);
        }

        public void SpawnEnemy(EnumEnemyType enemyType)
        {
            if (!_enemyPools.TryGetValue(enemyType, out var pool)) return;
            if (pool.Count == 0) ExpandPool(enemyType, _poolExpansionSize);
            if (pool.Count == 0) return;

            var wrapper = pool.Dequeue();
            var data = GetEnemyData(enemyType);

            if (_activeLineIndices.Count == 0 || _spawnLines.Count == 0)
            {
                Debug.LogError("No active spawn lines available!");
                return;
            }

            int lineIndex = _activeLineIndices[Random.Range(0, _activeLineIndices.Count)];
            var spawnLine = _spawnLines[lineIndex];
            float t = Random.Range(0f, 1f);
    
            Vector3 spawnPosition = Vector2.Lerp(spawnLine.pointA, spawnLine.pointB, t);
            spawnPosition.z = 0;
    
            wrapper.transform.position = spawnPosition;
    
            if (wrapper.rigidbody == null || wrapper.enemyState == null)
            {
                Debug.LogError("Enemy wrapper is missing critical components!");
                ReturnToPool(wrapper);
                return;
            }

            var realspeed = Random.Range(data._min2dVelocitySpeed, data._max2dVelocitySpeed);
            wrapper.data = new EnemyInstanceData
            {
                speed = realspeed,
                attackDistanceSqr = data._distanceAttack * data._distanceAttack,
                isAttacking = false,
                spawnLineIndex = lineIndex
            };

            _activeEnemies[enemyType].Add(wrapper);
            wrapper.gameObject.SetActive(true);
    
            wrapper.rigidbody.isKinematic = false;
            wrapper.rigidbody.WakeUp();

            float speedAverage = (data._min2dVelocitySpeed + data._max2dVelocitySpeed) / 2;
    
            if (realspeed < speedAverage)
            {
                wrapper.enemyState.SetAnimationSpeed(2 * realspeed / (speedAverage + data._min2dVelocitySpeed));
                wrapper.enemyState.SetmovementAnimation(ArenaEnemyStateEnum.Walk);
            }
            else
            {
                wrapper.enemyState.SetAnimationSpeed(2 * realspeed / (speedAverage + data._max2dVelocitySpeed));
                wrapper.enemyState.SetmovementAnimation(ArenaEnemyStateEnum.Run);
            }

            wrapper.enemyState.currentHealth = wrapper.enemyState.Health * _waveManager.GlobalSettings.baseHealthCoff *
                                               (1 + Mathf.Pow(_waveManager.GlobalSettings.baseHealthUp+1,
                                                   _waveManager.CurrentLvl - 1));
            wrapper.enemyState.currentDamage = wrapper.enemyState.damage * _waveManager.GlobalSettings.baseDamageCoff *
                                               (1 + Mathf.Pow(_waveManager.GlobalSettings.baseDamageUp+1,
                                                   _waveManager.CurrentLvl - 1));
            UpdateJobSystemArrays();
        }

        private void UpdateJobSystemArrays()
        {
            if (_transformAccessArray.isCreated) _transformAccessArray.Dispose();
            if (_enemyJobDataArray.IsCreated) _enemyJobDataArray.Dispose();

            var activeWrappers = _activeEnemies.Values.SelectMany(list => list).ToList();
            _transformAccessArray = new TransformAccessArray(activeWrappers.Select(w => w.transform).ToArray());
        
            _enemyJobDataArray = new NativeArray<EnemyJobData>(activeWrappers.Count, Allocator.Persistent);
            for (int i = 0; i < activeWrappers.Count; i++)
            {
                _enemyJobDataArray[i] = new EnemyJobData
                {
                    speed = activeWrappers[i].data.speed,
                    attackDistanceSqr = activeWrappers[i].data.attackDistanceSqr
                };
            }
        }

        public void ReturnToPool(EnemyWrapper wrapper)
        {
            if (wrapper.gameObject == null) return;
    
            wrapper.gameObject.SetActive(false);
    
            if (_activeEnemies.ContainsKey(wrapper.type))
            {
                _activeEnemies[wrapper.type].Remove(wrapper);
            }
    
            if (_enemyPools.ContainsKey(wrapper.type))
            {
                _enemyPools[wrapper.type].Enqueue(wrapper);
            }
    
            UpdateJobSystemArrays();
        }
        
        public void RemoveFromActive(EnemyWrapper wrapper)
        {
            if (_activeEnemies.ContainsKey(wrapper.type))
            {
                _activeEnemies[wrapper.type].Remove(wrapper);
                UpdateJobSystemArrays();
            }
        }

        public int GetActiveEnemiesCount()
        {
            return _activeEnemies.Sum(kvp => kvp.Value.Count);
        }

        public void NotifyEnemyKilled(EnumEnemyType enemyType)
        {
            if (TryGetComponent<WaveManager>(out var waveManager))
            {
                OnEnemyDied?.Invoke(enemyType);
            }
        }

        private EnemyData GetEnemyData(EnumEnemyType type)
        {
            foreach (var data in _enemyDatas)
            {
                if (data._type == type) return data;
            }
            return null;
        }
    }

    [System.Serializable]
    public class EnemyData
    {
        public EnumEnemyType _type;
        public float _distanceAttack;
        public float _min2dVelocitySpeed, _max2dVelocitySpeed;
        public EnemyPrefabData[] _enemiesPrefab;
    }

    [System.Serializable]
    public class EnemyPrefabData
    {
        public GameObject _enemiesPrefab;
        public SnapshotMeshAnimation[] _snapshotMeshAnimation;
    
        [System.NonSerialized]
        public SnapshotMeshAnimation[] _clonedAnimations;
    }

    public enum EnumEnemyType
    {
        SimpleRunner,
        NormalRunner,
        HardRunner,
        SimpleGun,
        NormalGun,
        HardGun,
        Boss
    }
}
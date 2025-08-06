using System;
using System.Collections;
using Arena.Scripts.Controllers;
using Arena.Scripts.MeshEventSystem;

using FSG.MeshAnimator.Snapshot;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Arena.Scripts
{
    public class ArenaEnemyState : AbstractDamagable
    {
        [SerializeField] private SnapshotMeshAnimator _meshAnimator;
        [SerializeField] private int _cost;
        [SerializeField] private EnemyEvent[] _events;
        [SerializeField] private Transform _deadMesh;
        [SerializeField] private Rigidbody[] _deadMeshRigidbodies;
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private ArenaEnemyStateEnum _pAnimation;
        private ArenaEnemyStateEnum _currentState;
        private Collider2D _collider2d;
        private ArenaEnemyController _arenaEnemyController;
        private Vector3[] _deadMeshPositions;
        private Quaternion[] _deadMeshRotations;

        private float _previouseFrame;

        private void Start()
        {
            _collider2d = GetComponent<Collider2D>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            GetRigidBodies();
        }
        
        public void DieImmediately()
        {
            // Реализация мгновенной смерти
            Dead(Vector3.down,5);
            // Или другая логика уничтожения
        }

        private void GetRigidBodies()
        {
            _deadMeshRigidbodies = _deadMesh.GetComponentsInChildren<Rigidbody>(true);
            _deadMeshPositions = new Vector3[_deadMeshRigidbodies.Length];
            _deadMeshRotations = new Quaternion[_deadMeshRigidbodies.Length];
            
            for (int i = 0; i < _deadMeshRigidbodies.Length; i++)
            {
                _deadMeshPositions[i] = _deadMeshRigidbodies[i].transform.localPosition;
                _deadMeshRotations[i] = _deadMeshRigidbodies[i].transform.localRotation;
            }
        }

        

        public override void Damage(float damage, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (currentHealth <= 0)
            {
                Dead(hitNormal,damage);
                return;
            }
            ParticleController.instance.ParticlePlay(hitPoint,hitNormal, EParticleType.Blood);
            currentHealth -= damage;
            if (currentHealth<=0) Dead(hitNormal,damage);
        }

        protected override void Dead(Vector3 directions, float damage)
        {
            // Отключаем управление через контроллер
            if (!_arenaEnemyController)
                _arenaEnemyController = FindAnyObjectByType<ArenaEnemyController>();
            _arenaEnemyController?.RemoveEnemy(this);
            _arenaEnemyController.AddMoney(_cost);

            // Отключаем основной меш и включаем ragdoll
            _meshAnimator.gameObject.SetActive(false);
            _deadMesh.gameObject.SetActive(true);
    
            // Применяем силу к ragdoll
            var randomPart = Random.Range(0, _deadMeshRigidbodies.Length);
            _deadMeshRigidbodies[randomPart].AddForce(damage * directions * Random.Range(0.9f, 1.1f), ForceMode.Impulse);
    
            // Отключаем коллайдер
            _rigidbody2D.linearVelocity = Vector3.zero;
            _rigidbody2D.angularVelocity = 0;
            _collider2d.enabled = false;
            

            // Запускаем возврат в пул
            StartCoroutine(ReturnToPoolAfterDelay(2f));
        }

        private IEnumerator ReturnToPoolAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
    
            // Сбрасываем физику ragdoll
            foreach (var rb in _deadMeshRigidbodies)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.transform.localPosition = _deadMeshPositions[Array.IndexOf(_deadMeshRigidbodies, rb)];
                rb.transform.localRotation = _deadMeshRotations[Array.IndexOf(_deadMeshRigidbodies, rb)];
            }
    
            // Включаем обратно основные компоненты
            _collider2d.enabled = true;
            _deadMesh.gameObject.SetActive(false);
            _meshAnimator.gameObject.SetActive(true);

            // Возвращаем в пул
            _arenaEnemyController?.ReturnEnemyToPool(this);
        }
        public void SetState(ArenaEnemyStateEnum stateEnum)
        {
            if (_currentState == stateEnum) return;
        
            _currentState = stateEnum;
            RestartAllEvents();
            switch (stateEnum)
            {
                case ArenaEnemyStateEnum.Walk: 
                    _meshAnimator.Crossfade("Z_Walk_InPlace");
                    break;
                case ArenaEnemyStateEnum.Run: 
                    _meshAnimator.Crossfade("Z_Run_InPlace");
                    break;
                case ArenaEnemyStateEnum.Hit: 
                    _meshAnimator.Crossfade("Z_Attack");
                    break;
            }
        }

        public void SetmovementAnimation(ArenaEnemyStateEnum stateEnum)
        {
            _pAnimation = stateEnum;
        }
        

        public void SetAnimations(SnapshotMeshAnimation[] clonedAnimations)
        {
            _meshAnimator.meshAnimations = clonedAnimations;
            _meshAnimator.defaultMeshAnimation = clonedAnimations[0];
        }
        public void SetAnimationSpeed(float speed)
        {
            _meshAnimator.speed = speed;
        }

        public void SetPreviuseAnimations()
        {
            SetState(_pAnimation);
        }

        private void FixedUpdate()
        {
            if (_meshAnimator == null || !_meshAnimator.IsPlaying()) return;

            foreach (var e in _events)
            {
                if (e?.Event == null || e.isPlayed || _currentState!=e.state) 
                    continue;

                float currentFrame = _meshAnimator.currentFrame;
                if (currentFrame > e.MinMaxFrame.x && currentFrame < e.MinMaxFrame.y)
                {
                   
                        var type = e.Event.DoEvent();
                        if (type != EParticleType.Null)
                        {
                            ParticleController.instance.ParticlePlay(transform.TransformPoint(e.Position),Vector3.zero,type);
                        }
                        if (!_arenaEnemyController)
                            _arenaEnemyController = FindAnyObjectByType<ArenaEnemyController>();
                        _arenaEnemyController.SetDamageToTarget(currentDamage, transform.up);
                        e.isPlayed = true;
                }
            }
           
            if (_meshAnimator.currentFrame < _previouseFrame) RestartAllEvents();
            _previouseFrame = _meshAnimator.currentFrame;
        }
        
        private void RestartAllEvents()
        {
            foreach (var e in _events)
            {
                e.isPlayed = false;
            }
        }
    }
  
    [Serializable]
    public class EnemyEvent
    {
        public Vector2 MinMaxFrame;
        public BaseMeshEventSo Event;
        public Vector3 Position;
        public bool isPlayed;
        public ArenaEnemyStateEnum state;
    }
    [Serializable]
    public enum ArenaEnemyStateEnum
    {
        Walk = 0,Run = 1,Hit = 2
    }
}
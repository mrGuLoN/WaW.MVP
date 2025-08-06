using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Arena.Scripts.Controllers;
using UnityEngine;

namespace Arena.Scripts
{
    public class AmmoPool : MonoBehaviour
    {
        public static AmmoPool instance;
        
        [Header("Settings")]
        [SerializeField] private BulletPrefabPair[] _bulletConfig;
        [SerializeField] private int _initialPoolSize = 50;
        [SerializeField] private float _maxLifeTimeBullet = 5f;
        
        private Dictionary<BulletType, Queue<BulletData>> _bulletPools;
        private List<BulletData> _activeBullets = new List<BulletData>();
        private List<BulletData> _bulletsToAdd = new List<BulletData>();
        private int _bulletCounter = 0;

        private void Awake()
        {
            if (instance == null) instance = this;
            
            _bulletPools = new Dictionary<BulletType, Queue<BulletData>>();
            
            foreach (var config in _bulletConfig)
            {
                var queue = new Queue<BulletData>();
                for (int i = 0; i < _initialPoolSize; i++)
                {
                    CreateNewBullet(config, queue);
                }
                _bulletPools.Add(config.type, queue);
            }
        }

        private void CreateNewBullet(BulletPrefabPair config, Queue<BulletData> queue)
        {
            var bulletObj = Instantiate(config.prefab, transform);
            var bulletData = new BulletData(_bulletCounter++, bulletObj, config.type)
            {
                collisonMask = config.collisionMask,
                liveTime = _maxLifeTimeBullet
            };
            
            bulletObj.SetActive(false);
            queue.Enqueue(bulletData);
        }

        public void SpawnBullet(BulletType bulletType, Vector3 position, Vector3 direction, float speed, float damage)
        {
            if (!_bulletPools.TryGetValue(bulletType, out var pool))
                return;

            BulletData bullet;
            if (pool.Count == 0)
            {
                var config = Array.Find(_bulletConfig, x => x.type == bulletType);
                if (config == null) return;
                
                CreateNewBullet(config, pool);
                bullet = pool.Dequeue();
            }
            else
            {
                bullet = pool.Dequeue();
            }
            
            bullet.transform.position = position;
            bullet.transform.forward = direction;
            bullet.speed = speed;
            bullet.damage = damage;
            bullet.currentTime = 0;
            bullet.prevPosition = position;
            bullet.gameObject.SetActive(true);
            
            _bulletsToAdd.Add(bullet);
        }

        private void Update()
        {
            // Добавляем новые пули в активный список
            if (_bulletsToAdd.Count > 0)
            {
                _activeBullets.AddRange(_bulletsToAdd);
                _bulletsToAdd.Clear();
            }

            // Обновляем пули
            for (int i = _activeBullets.Count - 1; i >= 0; i--)
            {
                var bullet = _activeBullets[i];
                
                // Обновляем позицию
                bullet.transform.position += bullet.transform.forward * bullet.speed * Time.deltaTime;
                bullet.currentTime += Time.deltaTime;

                // Проверка столкновений
                var distance = Vector3.Distance(bullet.prevPosition, bullet.transform.position);
                var hit = Physics2D.Raycast(
                    bullet.prevPosition,
                    bullet.transform.forward,
                    bullet.speed * Time.deltaTime*1.05f,
                    bullet.collisonMask);

                if (hit.collider != null)
                {
                    HandleCollision(bullet, hit.collider, new Vector3(hit.point.x,hit.point.y,bullet.transform.position.z));
                    ReturnToPool(bullet);
                    _activeBullets.RemoveAt(i);
                    continue;
                }

                // Проверка времени жизни
                if (bullet.currentTime > bullet.liveTime)
                {
                    ReturnToPool(bullet);
                    _activeBullets.RemoveAt(i);
                }
                else
                {
                    bullet.prevPosition = bullet.transform.position;
                }
            }
        }

        private void HandleCollision(BulletData bullet, Collider2D hitCollider, Vector3 hitPoint)
        {
            if (hitCollider.TryGetComponent<AbstractDamagable>(out var damagable))
                damagable.Damage(bullet.damage,hitPoint,bullet.transform.forward);
            else
            {
                ParticleController.instance.ParticlePlay(hitPoint,bullet.transform.forward, EParticleType.WallHit);
            }
        }

        private void ReturnToPool(BulletData bulletData)
        {
            if (bulletData == null) return;

            bulletData.gameObject.SetActive(false);
            
            if (_bulletPools.TryGetValue(bulletData.type, out var pool))
            {
                pool.Enqueue(bulletData);
            }
        }
    }

    [Serializable]
    public class BulletPrefabPair
    {
        public BulletType type;
        public GameObject prefab;
        public LayerMask collisionMask;
    }

    [Serializable]
    public class BulletData
    {
        public int id;
        public Transform transform;
        public GameObject gameObject;
        public BulletType type;
        public Vector3 prevPosition;
        public float speed;
        public LayerMask collisonMask;
        public float damage;
        public float liveTime;
        public float currentTime;

        public BulletData(int id, GameObject gameObject, BulletType type)
        {
            this.id = id;
            this.gameObject = gameObject;
            this.transform = gameObject.transform;
            this.type = type;
        }
    }

    [Serializable]
    public enum BulletType
    {
        Standard,
        Rocket,
    }
}
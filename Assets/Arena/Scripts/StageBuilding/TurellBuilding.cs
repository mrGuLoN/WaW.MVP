using System;
using System.Collections.Generic;
using Arena.Scripts.Controllers;
using Arena.Scripts.Player;
using Arena.Scripts.UI;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arena.Scripts.StageBuilding
{
    public class TurellBuilding : BaseBuilding
    {
        [SerializeField] private float tangle = 90f;
        [Header("Combat Settings")]
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private int maxAmmo = 10;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float reloadDistance = 5f;
        [SerializeField] private int damage;
        [SerializeField] private float reloadSpeed = 1f;
        [SerializeField] private Transform firePoint, system;
        [SerializeField] private BulletType bulletType;

        [Header("Visual Settings")] [SerializeField]
        private GameObject _visualFireRate;
        [SerializeField] private GameObject _visualReloadRate;
       
        private int currentAmmo;
        private float nextFireTime;
        private float reloadTimer;
        private bool isReloading;
        private Transform currentTarget;
        private Collider2D buildingCollider;
        private Transform playerTransform;
        private float reloadDistanceSqr;
        
        private HashSet<Transform> _activeEnemies = new HashSet<Transform>();
        
       
        
        private void Start()
        {
            currentAmmo = maxAmmo;
            playerTransform = FindObjectOfType<ArenaPlayerController>().transform;
            reloadDistanceSqr = reloadDistance * reloadDistance;
        }
        
        private void Update()
        {
            if (isReloading)
            {
                HandleReloading();
                return;
            }
            
            if (currentAmmo <= 0)
            {
                StartReloading();
                return;
            }
            
            UpdateTarget();
            
            if (currentTarget != null)
            {
                RotateTowardsTarget();
                TryShoot();
            }
        }
        
        private void UpdateTarget()
        {
            _activeEnemies.RemoveWhere(t => t == null || !t.gameObject.activeInHierarchy);
            
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || !_activeEnemies.Contains(currentTarget))
            {
                currentTarget = GetClosestTarget();
            }
        }
        
        private Transform GetClosestTarget()
        {
            if (_activeEnemies.Count == 0) return null;
            
            Transform closest = null;
            float closestAngle = float.MaxValue;
            Vector3 turretPosition = transform.position;
            
            foreach (var enemy in _activeEnemies)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                
                Vector2 direction = enemy.position - turretPosition;
                float angle = Vector2.Angle(transform.up, direction);
                
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closest = enemy;
                }
            }
            
            return closest;
        }
        
        private void RotateTowardsTarget()
        {
            if (currentTarget == null) return;

            // Получаем направление к цели в мировых координатах
            Vector3 directionToTarget = currentTarget.position - system.position;
            // Преобразуем направление в локальное пространство системы
            Vector3 localDirection = system.parent.InverseTransformDirection(directionToTarget);
    
            // Вычисляем угол поворота вокруг локальной оси Y (в локальном пространстве)
            float angle = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
    
            // Создаем целевой поворот (сохраняем начальные 90 градусов по X)
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0f);
    
            // Плавное вращение
            system.localRotation = Quaternion.Slerp(
                system.localRotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
        
        private void TryShoot()
        {
            if (Time.time >= nextFireTime && currentTarget != null && currentTarget.gameObject.activeInHierarchy)
            {
                Shoot();
                nextFireTime = Time.time + 1f / fireRate;
            }
        }
        
        private void Shoot()
        {
            AmmoPool.instance.SpawnBullet(
                bulletType, 
                firePoint.position, 
                firePoint.right, // Используем forward для 3D
                bulletSpeed,
                damage
            );
            
            // Анимация отдачи
            firePoint.localPosition = Vector3.zero;
            firePoint.DOLocalMove(firePoint.localPosition -0.2f* firePoint.right, (1/fireRate)*0.2f)
                     .SetLoops(2, LoopType.Yoyo);
            
            currentAmmo--;
        }
        
        private void StartReloading()
        {
            _visualFireRate.SetActive(false);
            _visualReloadRate.SetActive(true);
            float distanceToPlayerSqr = (playerTransform.position - transform.position).sqrMagnitude;
            
            if (distanceToPlayerSqr <= reloadDistanceSqr)
            {
                isReloading = true;
                reloadTimer = 0f;
            }
        }
        
        private void HandleReloading()
        {
            float distanceToPlayerSqr = (playerTransform.position - transform.position).sqrMagnitude;
            
            if (distanceToPlayerSqr > reloadDistanceSqr)
            {
                isReloading = false;
                return;
            }
            
            reloadTimer += Time.deltaTime;
            
            if (reloadTimer >= reloadSpeed)
            {
                currentAmmo = maxAmmo;
                isReloading = false;
                _visualFireRate.SetActive(true);
                _visualReloadRate.SetActive(false);
            }
        }
        
        public override void Upgrade()
        {
            currentlvl++;
            fireRate *= 1.1f;
            maxAmmo += 2;
            rotationSpeed *= 1.1f;
            damage++;
            upgradeCost *=(int)((UpdateUpCost)*(1+CurrentLevel*0.2f));
        }
        
        public override void Demolish(float refundPercent)
        {
            CurrencySystem.Instance.AddMoney((int)(cost * refundPercent));
            Destroy(gameObject);
        }
        
       
        
     
        
        private void OnTriggerEnter2D(Collider2D other)
        {
                _activeEnemies.Add(other.transform);
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
                _activeEnemies.Remove(other.transform);
        }
    }
}
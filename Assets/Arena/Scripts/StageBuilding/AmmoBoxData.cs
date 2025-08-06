using System;
using DG.Tweening;
using Arena.Scripts.Player;
using UnityEngine;

namespace Arena.Scripts.StageBuilding
{
    public class AmmoBoxData : MonoBehaviour
    {
        [Header("DOTween Animation")]
        [SerializeField] private float _floatHeight = 0.2f;
        [SerializeField] private float _floatSpeed = 1f;
        [SerializeField] private float _rotationSpeed = 30f;
        [SerializeField] private float _pickupScaleTime = 0.3f;
        [SerializeField] private Ease _pickupEase = Ease.InBack;

        [Header("Ammo Settings")]
        [SerializeField] private BulletType[] _allowedBulletTypes; // Массив типов патронов
        [SerializeField] private int _ammoCount = 30;

        private Vector3 _startPosition;
        private bool _isPickedUp;
        private ArenaPlayerController _player; // Запоминаем игрока для пополнения после анимации

        private void Start()
        {
            _startPosition = transform.position;
            StartFloatingAnimation();
        }

        public void Initialize(BulletType[] dataBulletTypes, int dataCount)
        {
            _allowedBulletTypes = dataBulletTypes;
            _ammoCount = dataCount;
        }

        private void StartFloatingAnimation()
        {
            transform.DOMoveZ(_startPosition.z - _floatHeight, _floatSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);

            transform.DORotate(new Vector3(0, 0, 360), _rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isPickedUp || !other.TryGetComponent(out _player)) 
                return;

            _isPickedUp = true;
            PlayPickupAnimation();
        }

        private void PlayPickupAnimation()
        {
            DOTween.Kill(transform);

            // 1. Анимация подбора (уменьшение + движение к игроку)
            Sequence pickupSequence = DOTween.Sequence();
            
            pickupSequence.Append(
                transform.DOScale(Vector3.zero, _pickupScaleTime)
                    .SetEase(_pickupEase)
            );

            // Опционально: движение к игроку (если нужно)
            pickupSequence.Join(
                transform.DOMove(_player.transform.position, _pickupScaleTime)
                    .SetEase(Ease.InQuad)
            );

            // 2. После анимации — пополняем патроны и уничтожаем объект
            pickupSequence.OnComplete(() => 
            {
                AddAmmoToPlayer(_player);
                Destroy(gameObject);
            });
        }

        private void AddAmmoToPlayer(ArenaPlayerController player)
        {
            if (player.weapons == null || _allowedBulletTypes == null)
                return;

            foreach (var weapon in player.weapons)
            {
                foreach (var bulletType in _allowedBulletTypes)
                {
                    if (weapon.BulletType == bulletType)
                    {
                        weapon.AddAmmo(_ammoCount);
                        break; // Прерываем, если патроны уже пополнены
                    }
                }
            }
        }
    }
}
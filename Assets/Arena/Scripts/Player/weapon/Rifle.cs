using System;
using UnityEngine;

namespace Arena.Scripts.Player.weapon
{
    public class Rifle : ArenaWeapon
    {
        [SerializeField] private SecondaryFirePont[] _secondaryFirePoint;
        private bool _isNewFireSystem;

        protected override void Start()
        {
            base.Start();
            foreach (var f in _secondaryFirePoint)
            {
                f.firePoint.gameObject.SetActive(false);
            }
        }

        public override void Fire()
        {
            if (isReloading) return;
            if (Time.time < nextFireTime) return;
            if (_isNewFireSystem)
            {
                foreach (var f in _secondaryFirePoint)
                {
                    if (currentAmmo<=0) break;
                    AmmoPool.instance.SpawnBullet(bulletType, f.firePoint.position, f.firePoint.up,bulletSpeed,damage);
                    f.secondaryMuzzlePoint.Play();
                    f.secondarySleeveParticleSystem.Play();
                    currentAmmo--;
                }
            }
            base.Fire();
        }

        #region Upgrade Methods

        public override void Upgrade()
        {
            damage += 2;
            _currentLvl++;
            if (_currentLvl % 5 == 0)
            {
                magazineSize += 10;
            }
            
            if (_currentLvl % 3 == 0)
            {
                fireRate *= 0.99f;
            }

            if (_currentLvl == 15)
            {
                _isNewFireSystem = true;
                magazineSize *= 10;
                foreach (var f in _secondaryFirePoint)
                {
                    f.firePoint.gameObject.SetActive(true);
                }
            }
        }
       
        #endregion
    }
    
    
    [Serializable]
    public class SecondaryFirePont
    {
        public Transform firePoint;
        public ParticleSystem secondaryMuzzlePoint;
        public ParticleSystem secondarySleeveParticleSystem;
    }
}

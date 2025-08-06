using DG.Tweening;
using UnityEngine;

namespace Arena.Scripts.Player.weapon
{
    public class ShotGun : ArenaWeapon
    {
        [SerializeField] private int _pillet;
        [SerializeField] private float _angle;
        [SerializeField] private GameObject _visualSecondary;
        protected override void Start()
        {
            base.Start();
            _visualSecondary.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Upgrade();
            }
        }
        public override void Fire()
        {
            if (isReloading) return;
        
            if (Time.time < nextFireTime) return;
        
            if (currentAmmo <= 0)
            {
                if (totalAmmo>0)
                    data.text = "Reload";
                else
                {
                    data.text = "NO AMMO";
                }
                StartCoroutine(Reload());
                return;
            }

            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            
            if(_muzzleFlashParticleSystem)
                _muzzleFlashParticleSystem.Play();
            if(_sleeveParticleSystem)
                _sleeveParticleSystem.Play();
            transform.localPosition=Vector3.zero;
            transform.DOLocalMove(_localWeaponRecoil,fireRate*0.3f).SetLoops(2,LoopType.Yoyo);
            for (int i = 0; i < _pillet; i++)
            {
                Vector3 currentForward = _muzzleFlashParticleSystem.transform.forward;
                float randomAngleZ = Random.Range(-_angle, _angle);
                Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomAngleZ);
                currentForward = randomRotation * currentForward;
                AmmoPool.instance.SpawnBullet(bulletType, _muzzleFlashParticleSystem.transform.position, currentForward,bulletSpeed,damage);
            }
            data.text = currentAmmoTextInUI.text = currentAmmo + "/" + totalAmmo;
        }
        #region Upgrade Methods

        public override void Upgrade()
        {
            damage += 1;
            _currentLvl++;
            if (_currentLvl % 5 == 0)
            {
                magazineSize += 2;
            }
            
            if (_currentLvl % 3 == 0)
            {
                fireRate *= 0.9f;
                _pillet++;
            }
            if (_currentLvl == 15)
            {
                fireRate *= 0.9f;
                _visualSecondary.SetActive(true);
                magazineSize += 15;
                _pillet++;
            }

         
        }
       
        #endregion
    }
}

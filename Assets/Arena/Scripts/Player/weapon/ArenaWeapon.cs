using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Arena.Scripts.Player
{
    [System.Serializable]
    public class ArenaWeapon : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] protected string _weaponName = "Rifle";

        [SerializeField] protected string _description;
        [SerializeField] protected Sprite _icon;

        public string weaponName => _weaponName;
        public string description => _description;
        public Sprite icon => _icon;
        [SerializeField] protected int damage = 10;
        [SerializeField] protected float bulletSpeed = 10f;
        [SerializeField] protected int maxAmmo = 30;
        [SerializeField] protected int magazineSize = 15;
        [SerializeField] protected float fireRate = 0.2f;
        [SerializeField] protected float reloadTime = 1.5f;
        [SerializeField] protected BulletType bulletType;
        public BulletType BulletType => bulletType;

        public int bulletInMagazine => currentAmmo;
        public int allAmmo => totalAmmo;
        public TMP_Text currentAmmoTextInUI;
        
        [Header("Visual Data")]
        [SerializeField] protected ParticleSystem _muzzleFlashParticleSystem;
        [SerializeField] protected ParticleSystem _sleeveParticleSystem;
        [SerializeField] protected Vector3 _localWeaponRecoil;

        [Header("Upgrade Data")] [SerializeField]
        protected int _baseCostWeapon;

        public int cost => _baseCostWeapon;

        [SerializeField] protected int _baseCostUpdate;
        [SerializeField] protected float _baseUpUpdateCost;
        public float baseUpdateCost=>_baseCostUpdate;
        public float baseUpUpdateCost=>_baseUpUpdateCost;
        protected int _currentLvl =1;
        public int currentLvl => _currentLvl;
        

        [Header("Rig Point")] 
        [SerializeField] protected Transform _righHandPoint;
        public Transform righHandPoint => _righHandPoint;
        [SerializeField] protected Transform _leftHandPoint;
        public Transform leftHandPoint => _leftHandPoint;

        public TMP_Text data;

       

        protected int currentAmmo;
        public int CurrentAmmo => currentAmmo;
        protected int totalAmmo;
        public int TotalAmmo => totalAmmo;
        protected float nextFireTime;
        protected bool isReloading = false;

        protected virtual void Start()
        {
            totalAmmo = maxAmmo;
            currentAmmo = magazineSize;
        }

        public virtual void AddAmmo(int ammo)
        {
            totalAmmo += ammo;
            data.text = currentAmmo + "/" + totalAmmo;
        }

        public virtual void Fire()
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
            AmmoPool.instance.SpawnBullet(bulletType, _muzzleFlashParticleSystem.transform.position, _muzzleFlashParticleSystem.transform.forward,bulletSpeed,damage);
            data.text = currentAmmoTextInUI.text = currentAmmo + "/" + totalAmmo;
        }

        protected virtual System.Collections.IEnumerator Reload()
        {
            if (totalAmmo <= 0 || currentAmmo == magazineSize) yield break;
        
            isReloading = true;
            yield return new WaitForSeconds(reloadTime);
        
            int ammoToAdd = Mathf.Min(magazineSize - currentAmmo, totalAmmo);
            currentAmmo += ammoToAdd;
            totalAmmo -= ammoToAdd;
        
            isReloading = false;
            data.text = currentAmmo + "/" + totalAmmo;
        }

        #region Upgrade Methods

        public virtual void Upgrade()
        {
           
        }
       
        #endregion
    }
}
using System;
using Arena.Scripts.Controllers;
using Arena.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace Arena.Scripts.StageBuilding
{
    public class AmmoCreator : BaseBuilding
    {
        [SerializeField] private AmmoCreatorData[] bulletTypes;
        [SerializeField] private AmmoBoxData _ammoBoxData;
        [SerializeField] private float _radiusMin, _radiusMax;
        [SerializeField] private float _timeToCreateAmmo;
        [SerializeField] private float _multiply = 1;
        [SerializeField] private int _currentTypeCount;

        [SerializeField] private int _baseCostUpdate;
        [SerializeField] private float _multiplierCostUpdate;
        
        [Header("Base Settings")]
       
       
        
        private int currentLevel = 1;
        private Collider2D buildingCollider;
        public GameObject Prefab { get; }
        public Collider2D Collider => buildingCollider ??= GetComponent<Collider2D>();
        
        public int Cost => cost;
        public Sprite Sprite => sprite;
        public string Name => name;
        public string Description => description;
        public int CurrentLevel => currentLevel;
        public int UpgradeCost =>  (int)(upgradeCost * (currentLevel + currentLevel*UpdateUpCost));
        public float UpdateUpCost => updateUpCost;
    

        private bool _healthCreator;
        private int _lvl;
        private AmmoBoxData _currentAmmoBox;
        private float _currentTime;
        private float _cost1;
        private Sprite _sprite1;
        private string _name1;
        private string _description1;


        private void FixedUpdate()
        {
            if (_currentAmmoBox != null) return;
            _currentTime += Time.fixedDeltaTime;
            if (_currentTime >= _timeToCreateAmmo)
            {
                _currentAmmoBox = Instantiate(_ammoBoxData, transform);
                _currentAmmoBox.transform.position = transform.position + (Vector3)Random.insideUnitCircle.normalized * Random.Range(_radiusMin, _radiusMax);
                AmmoCreatorData data = null;
                if (_currentTypeCount < bulletTypes.Length - 1)
                {
                    data = bulletTypes[Random.Range(0, _currentTypeCount)];
                }
                else
                {
                    data = bulletTypes[Random.Range(0, bulletTypes.Length)];
                }
                _currentAmmoBox.Initialize(data.bulletTypes, (int)(data.count*_multiply));
                if (_healthCreator){}
            }
        }
        public void UpdateBuilding()
        {
            _lvl++;
            if (_lvl % 3 == 0)
            {
                _currentTypeCount++;
            }
            _multiply *= 1.02f;
            if (_lvl == 5)
            {
                _healthCreator = true;
            }
        }
        
      

        public override void Upgrade()
        {
            UpdateBuilding();
        }

        public override void Demolish(float refundPercent)
        {
          
        }
    }
    [Serializable]
    public class AmmoCreatorData
    {
        public BulletType[] bulletTypes;
        public int count;
    }
}
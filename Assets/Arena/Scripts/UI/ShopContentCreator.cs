using System;
using System.Collections.Generic;
using System.Linq;
using Arena.Scripts.Controllers;
using Arena.Scripts.Player;
using Arena.Scripts.ScriptableObjects;
using Arena.Scripts.StageBuilding;
using UnityEngine;

namespace Arena.Scripts.UI
{
    public class ShopContentCreator : MonoBehaviour
    {
        [SerializeField] private WeaponSO _weapons;
        [SerializeField] private BuildingsSO _buildings;
        [SerializeField] private RectTransform _buildinfShopContent, _weaponsShopContent;
        [SerializeField] private ItemCellView _itemCellViewPrefab;
        [SerializeField] private ArenaPlayerController _playerController;
        [SerializeField] private Camera _creatorCamera;
        
        private BuildingPlacer _buildingPlacer;

        private List<ItemCellView> _itemCellViews = new List<ItemCellView>();

        private void OnEnable()
        {
            CreateShopItems();
            _creatorCamera.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            ClearShopItems();
            _creatorCamera.gameObject.SetActive(false);
        }

        private void CreateShopItems()
        {
            // Оружие
            foreach (var weapon in _weapons.Weapons)
            {
                var cell = Instantiate(_itemCellViewPrefab, _weaponsShopContent);
                bool isOwned = _playerController.weapons.Exists(w => w.weaponName == weapon.weaponName);
                
                if (isOwned)
                {
                    var sWeapon  = _playerController.weapons.FirstOrDefault(x => x.weaponName == weapon.weaponName);
                    // Если оружие уже есть, показываем кнопку апгрейда
                    int upgradeCost =(int)(sWeapon.baseUpdateCost*((1+sWeapon.baseUpUpdateCost* sWeapon.currentLvl)));
                    Debug.Log(sWeapon.baseUpdateCost + " /// " + sWeapon.baseUpUpdateCost + " /// " + sWeapon.currentLvl + " /// " + (1+sWeapon.baseUpUpdateCost* sWeapon.currentLvl));
                    Debug.Log(upgradeCost + " upgrade cost");
                    cell.SetData(sWeapon.icon, $"{sWeapon.weaponName} (Lvl {sWeapon.currentLvl})", sWeapon.description, upgradeCost);
                    cell.Button.onClick.AddListener(() => TryUpgradeWeapon(sWeapon));
                }
                else
                {
                    // Если оружия нет, показываем кнопку покупки
                    cell.SetData(weapon.icon, weapon.weaponName, weapon.description, weapon.cost);
                    cell.Button.onClick.AddListener(() => TryBuyWeapon(weapon));
                }
                _itemCellViews.Add(cell);
            }
           
            foreach (var building in _buildings.Buildings)
            {
                var cell = Instantiate(_itemCellViewPrefab, _buildinfShopContent);
                cell.SetData(building.Sprite, building.Name, building.Description, (int)building.Cost);
                if (!_buildingPlacer) _buildingPlacer = FindObjectOfType<BuildingPlacer>();
                cell.Button.onClick.AddListener(() => {
                    _buildingPlacer.StartBuildingPlacement(building);
                });
    
                _itemCellViews.Add(cell);
            }
        }

        private void TryBuyWeapon(ArenaWeapon weaponPrefab)
        {
            if (CurrencySystem.Instance.TrySpendMoney(weaponPrefab.cost))
            {
                var newWeapon = Instantiate(weaponPrefab);
                _playerController.AddWeapon(newWeapon);
                RefreshShop();
            }
            else
            {
                Debug.Log("Не хватает денег!");
            }
        }

        private void TryUpgradeWeapon(ArenaWeapon weaponPrefab)
        {
            int upgradeCost =(int)(weaponPrefab.baseUpdateCost*((1+weaponPrefab.baseUpUpdateCost* weaponPrefab.currentLvl)));
            if (CurrencySystem.Instance.TrySpendMoney(upgradeCost))
            {
                var weapon = _playerController.weapons.Find(w => w.weaponName == weaponPrefab.weaponName);
                weapon.Upgrade();
                RefreshShop();
            }
            else
            {
                Debug.Log("Не хватает денег на апгрейд!");
            }
        }

        private void RefreshShop()
        {
            ClearShopItems();
            CreateShopItems();
        }

        private void ClearShopItems()
        {
            foreach (var cell in _itemCellViews)
            {
                Destroy(cell.gameObject);
            }
            _itemCellViews.Clear();
        }
    }
}
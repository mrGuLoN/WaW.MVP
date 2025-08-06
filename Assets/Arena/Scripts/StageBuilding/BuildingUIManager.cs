using Arena.Scripts.Controllers;
using Arena.Scripts.StageBuilding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena.Scripts.UI
{
    public class BuildingUIManager : MonoBehaviour
    {
        public static BuildingUIManager Instance;
        
        [Header("UI References")]
        [SerializeField] private GameObject buildingMenu;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text upgradeCostText;
        [SerializeField] private Button upgradeButton;
        [SerializeField] private Button demolishButton;
        [SerializeField] private Button closeButton;
        
        private IBuilding currentBuilding;
        
        private void Awake() => Instance = this;
        
        public void ShowBuildingMenu(IBuilding building)
        {
            currentBuilding = building;
            Debug.Log("show building menu");
            // Обновляем UI
            nameText.text = building.Name;
            levelText.text = $"Level: {building.CurrentLevel}";
            upgradeCostText.text = $"Upgrade: {building.UpgradeCost}$";
            
            // Настраиваем кнопки
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
            
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
            
            demolishButton.onClick.RemoveAllListeners();
            demolishButton.onClick.AddListener(OnDemolishClicked);
            
            buildingMenu.SetActive(true);
        }

        private void Close()
        {
            HideBuildingMenu();
        }

        public void HideBuildingMenu()
        {
            buildingMenu.SetActive(false);
        }
        
        private void OnUpgradeClicked()
        {
            Debug.Log("ClickUpgrade");
            if (CurrencySystem.Instance.TrySpendMoney(currentBuilding.UpgradeCost))
            {
                currentBuilding.Upgrade();
                ShowBuildingMenu(currentBuilding);
            }
        }
        
        private void OnDemolishClicked()
        {
            currentBuilding.Demolish(0.8f); // Возвращаем 80% стоимости
            HideBuildingMenu();
        }
    }
}
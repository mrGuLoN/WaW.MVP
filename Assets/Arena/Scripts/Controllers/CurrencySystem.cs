using TMPro;
using UnityEngine;

namespace Arena.Scripts.Controllers
{
    public class CurrencySystem : MonoBehaviour
    {
        public static CurrencySystem Instance;
        public int Money { get; private set; }
        [SerializeField] private TMP_Text _moneyText;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            _moneyText.text = "0$";
        }

        public bool TrySpendMoney(int amount)
        {
            if (Money >= amount)
            {
                Money -= amount;
                _moneyText.text = Money + " $";
                return true;
            }
            return false;
        }

        public void AddMoney(int amount)
        {
            Money += amount;
            _moneyText.text = Money + " $";
        }

        public bool CanAfford(int buildingCost)
        {
            return buildingCost<=Money;
        }
    }
}
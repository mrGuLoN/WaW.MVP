using Arena.Scripts.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Arena.Scripts.StageBuilding
{
    public class BaseBuilding : MonoBehaviour, IBuilding
    {
        [Header("Base Settings")] [SerializeField]
        protected int cost;

        public int Cost => cost;
        [SerializeField] protected Sprite sprite;
        [SerializeField]
        protected string name;
        [SerializeField]
        protected string description;

        [SerializeField] protected int currentlvl;
        [SerializeField] protected int upgradeCost;
        
        [SerializeField] protected float updateUpCost;
        [SerializeField] protected Collider2D collider;

        public Sprite Sprite => sprite;
        public string Name => name;
        public string Description => description;
        public int CurrentLevel => currentlvl;
        public int UpgradeCost => upgradeCost;
        public float UpdateUpCost => updateUpCost;
        public Collider2D Collider => collider;
        [SerializeField] private BuildingPlacer _buildingPlacer;
        
        
        public virtual void Place(Vector2 position)
        {
          
        }

        public virtual void Upgrade()
        {
           
        }

        public virtual void Demolish(float refundPercent)
        {
           
        }
        
        protected virtual void OnMouseDown()
        {
            Debug.Log("OnMouseDown called  " +IsPointerOverUI());
            if (!_buildingPlacer)_buildingPlacer = FindAnyObjectByType<BuildingPlacer>(FindObjectsInactive.Include);
            
            if (IsPointerOverUI() || !_buildingPlacer.gameObject.activeSelf) return;
            
            BuildingUIManager.Instance.ShowBuildingMenu(this);
            
        }
        
        protected virtual bool IsPointerOverUI()
        {
            // Для мыши
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            // Для тачей
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;
            }

            return false;
        }
    }
}

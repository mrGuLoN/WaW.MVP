using System;
using Arena.Scripts.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arena.Scripts.StageBuilding
{
    public class BuildingPlacer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private GameObject confirmUI;
        [SerializeField] private Transform buildingParent;
        [SerializeField] private float buildingYOffset = 0.5f;
        [SerializeField] private Button _ok, _cancel;

        private BaseBuilding currentBuilding;
        private BaseBuilding buildingPreview;
        private Collider2D previewCollider;
        private bool isDragging;
        private bool isValidPosition;
        private bool isWaitingForConfirmation;
        private Renderer _curMeshRender;// Новый флаг

        private void Start()
        {
            HideConfirmUI();
        }

        public void StartBuildingPlacement(BaseBuilding building)
        {
            if (buildingPreview) Destroy(buildingPreview.gameObject);
            if (building == null)
            {
                Debug.LogError("Building or its prefab is null!");
                return;
            }

            if (!CurrencySystem.Instance.CanAfford(building.Cost))
            {
                Debug.Log("Not enough money!");
                return;
            }

          
            currentBuilding = building;
            buildingPreview = Instantiate(building, buildingParent);
            _curMeshRender =  buildingPreview.GetComponentInChildren<Renderer>();
            previewCollider = buildingPreview.GetComponent<Collider2D>();

            if (previewCollider == null)
            {
                Debug.LogError("No Collider2D found on building prefab!");
                Destroy(buildingPreview);
                return;
            }

            previewCollider.enabled = false;
            SetPreviewTransparency(0.7f);

            HideConfirmUI();
            isDragging = true;
            isWaitingForConfirmation = false;
        }

        private void OnEnable()
        {
            _ok.onClick.AddListener(ConfirmPlacement);
            _cancel.onClick.AddListener(CancelPlacement);
        }

        private void OnDisable()
        {
            _ok.onClick.AddListener(ConfirmPlacement);
            _cancel.onClick.AddListener(CancelPlacement);
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void Update()
        {
            if (!isDragging) return;

            // Если касаемся экрана И не над UI
            if (IsPointerDown() && !IsPointerOverUI())
            {
                // Получаем позицию касания
                Vector2 touchPosition = GetTouchPosition();

                // Перемещаем превью под палец
                buildingPreview.transform.position = new Vector3(
                    touchPosition.x, 
                    touchPosition.y + buildingYOffset, 
                    0f);

                // Проверяем коллизии
                isValidPosition = CheckPlacement(touchPosition);
                SetPreviewColor(isValidPosition ? Color.green : Color.red);
        
                // Сбрасываем флаг ожидания подтверждения, если снова начали двигать
                isWaitingForConfirmation = false;
            }

            // Показываем кнопки при отпускании экрана, только если не над UI
            if (IsTouchEnded() && !isWaitingForConfirmation && !IsPointerOverUI())
            {
                isWaitingForConfirmation = true;
                ShowConfirmUI();
            }
        }

        private bool IsPointerOverUI()
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

        private bool IsPointerDown()
        {
            return Input.GetMouseButton(0) || 
                  (Input.touchCount > 0 && 
                   (Input.GetTouch(0).phase == TouchPhase.Moved || 
                    Input.GetTouch(0).phase == TouchPhase.Stationary));
        }

        private bool CheckPlacement(Vector2 position)
        {
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(obstacleLayer);
            contactFilter.useTriggers = false;

            Collider2D[] results = new Collider2D[1];
            int numColliders = previewCollider.Overlap(contactFilter, results);

            return numColliders == 0;
        }

        public void ConfirmPlacement()
        {
            if (!isValidPosition) return;
            if (_curMeshRender)_curMeshRender.material.color=Color.white;
            CurrencySystem.Instance.TrySpendMoney(currentBuilding.Cost);
            HideConfirmUI();
            FinishPlacement();
        }

        public void CancelPlacement()
        {
            HideConfirmUI();
            isDragging = true; // Возвращаем в режим перемещения
            if (buildingPreview)
            {
                Destroy(buildingPreview.gameObject);
                buildingPreview = null;
            }

            isWaitingForConfirmation = false;
        }

        private void FinishPlacement()
        {
            var col = buildingPreview.GetComponent<Collider2D>();
            col.enabled = true;
            col.isTrigger = false;
            buildingPreview = null;
            currentBuilding = null;
            previewCollider = null;
            isDragging = false;
            isWaitingForConfirmation = false;
        }

        private Vector2 GetTouchPosition()
        {
            return mainCamera.ScreenToWorldPoint(
                Input.touchCount > 0 ?
                    Input.GetTouch(0).position :
                    (Vector2)Input.mousePosition);
        }

        private bool IsTouchEnded()
        {
            return Input.touchCount > 0 ?
                Input.GetTouch(0).phase == TouchPhase.Ended :
                Input.GetMouseButtonUp(0);
        }

        private int GetTouchPointerID()
        {
            return Input.touchCount > 0 ? Input.GetTouch(0).fingerId : -1;
        }

        private void SetPreviewTransparency(float alpha)
        {
            foreach (var renderer in buildingPreview.GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = renderer.color;
                c.a = alpha;
                renderer.color = c;
            }
        }

        private void SetPreviewColor(Color color)
        {
            if (_curMeshRender)  _curMeshRender.material.color = color;
        }

        private void ShowConfirmUI()
        {
            confirmUI.SetActive(true);
        }

        private void HideConfirmUI()
        {
            confirmUI.SetActive(false);
        }
    }
}
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using Slider = UnityEngine.UI.Slider;

namespace Arena.Scripts.Player
{
    public class ArenaPlayerController : MonoBehaviour
    {
        private static readonly int FireState = Animator.StringToHash("FireState");
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");

        [Header("Controller")]
        [SerializeField] private Joystick _movementJoystick;
        [SerializeField] private Joystick _fireJoystick;
        [Header("Movement")]
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float shootSpeed = 4f;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator _animator;

        [Header("Stats")]
        [SerializeField] private float maxHealth = 100;
        private float _currentHealth;
        private bool _isShooting = false;

        [Header("Weapons")]
        [SerializeField] private Transform weaponHolder;
        [SerializeField] private ArenaWeapon startingWeaponPrefab;
        [SerializeField] private int maxWeaponSlots = 4;
        private List<ArenaWeapon> _weapons = new List<ArenaWeapon>();
        public List<ArenaWeapon> weapons => _weapons;
        private int activeWeaponIndex = 0;

        [Header("RigSetup")] 
        [SerializeField] private Transform _rightHandrig;
        [SerializeField] private Transform _leftHandrig;
        [SerializeField] private Transform _rightHand;
        private TwoBoneIKConstraint _rightHandSplitView;

        [Header("Canvas Setup")] 
        [SerializeField] private Slider _healthslider;

        [SerializeField] private TMP_Text _ammoText;

        [Header("Upgrades")]
        [SerializeField] private int healthUpgradeAmount = 25;
        [SerializeField] private int damageUpgradeAmount = 5;
        [SerializeField] private int magazineUpgradeAmount = 5;
        [Header("UI Setup")]
        [SerializeField] private RectTransform _weaponsContainer;

        [SerializeField] private GameObject _weaponUIprefab;

        private Vector2 movementInput;
        private Vector2 _loocDir;
        private bool _previuseStateIsFire;

        private void Start()
        {
            _currentHealth = maxHealth;
            _rightHandSplitView=_rightHandrig.gameObject.GetComponent<TwoBoneIKConstraint>();
            InitializeStartingWeapon();
        }

        private void InitializeStartingWeapon()
        {
            // Создаём стартовое оружие
            if (startingWeaponPrefab != null)
            {
                var weapon = Instantiate(startingWeaponPrefab, weaponHolder);
                AddWeapon(weapon);
                _weapons[activeWeaponIndex].transform.SetParent(_rightHand);
                _rightHandSplitView.weight = 0;
                _weapons[activeWeaponIndex].transform.DOLocalMove(Vector3.zero, 0.1f);
                _weapons[activeWeaponIndex].transform.DOLocalRotate(Vector3.zero, 0.1f);
            }
        }

        public void Damage(float damage, Vector3 direction)
        {
            _currentHealth -= damage;
            _healthslider.value = _currentHealth/maxHealth;
        }

        private void Update()
        {
            HandleInput();
            HandleWeaponSwitch();
            HandleShooting();
        }

        private void FixedUpdate()
        {
            MovePlayer();
            RotatePlayer();
            SetAnimation();
            RigMoved();
        }

        private void RigMoved()
        {
            if (_weapons[activeWeaponIndex])
            {
                _leftHandrig.transform.position = _weapons[activeWeaponIndex].leftHandPoint.position;
                _leftHandrig.transform.rotation = _weapons[activeWeaponIndex].leftHandPoint.rotation;
                _rightHandrig.transform.position = _weapons[activeWeaponIndex].righHandPoint.position;
                _rightHandrig.transform.rotation = _weapons[activeWeaponIndex].righHandPoint.rotation;
            }
        }

        private void HandleInput()
        {
            movementInput = _movementJoystick.Direction.normalized;
            if (movementInput == Vector2.zero)
            {
                movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }

            _isShooting = _fireJoystick.Direction!=Vector2.zero;
        }

        private void HandleWeaponSwitch()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToWeapon(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToWeapon(3);
        }

        private void HandleShooting()
        {
            if (_isShooting && _weapons.Count > 0)
            {
                _weapons[activeWeaponIndex].Fire();
            }
        }

        private void MovePlayer()
        {
            float currentSpeed = _isShooting ? shootSpeed : runSpeed;
            rb.linearVelocity = movementInput * currentSpeed;
        }

        private void RotatePlayer()
        {
            if (_fireJoystick.Direction != Vector2.zero) _loocDir =_fireJoystick.Direction.normalized;
            else if (movementInput != Vector2.zero) _loocDir = movementInput;
            float angle = Mathf.Atan2(_loocDir.y, _loocDir.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = angle;
        }

        private void SetAnimation()
        {
            if (_isShooting)
            {
                _animator.SetBool(FireState, true);
                var direction = transform.InverseTransformVector(movementInput);
                _animator.SetFloat(InputX, direction.x);
                _animator.SetFloat(InputY, direction.y);
                if (_isShooting != _previuseStateIsFire)
                {
                    _weapons[activeWeaponIndex].transform.SetParent(weaponHolder);
                    _rightHandSplitView.weight = 1;
                    _weapons[activeWeaponIndex].transform.DOLocalMove(Vector3.zero, 0.1f);
                    _weapons[activeWeaponIndex].transform.DOLocalRotate(Vector3.zero, 0.1f);
                }
            }
            else
            {
                _animator.SetBool(FireState, false);
                if (movementInput != Vector2.zero)  _animator.SetFloat(InputX, 1);
                else  _animator.SetFloat(InputX, 0);
                if (_isShooting != _previuseStateIsFire)
                {
                    _weapons[activeWeaponIndex].transform.SetParent(_rightHand);
                    _rightHandSplitView.weight = 0;
                    _weapons[activeWeaponIndex].transform.DOLocalMove(Vector3.zero, 0.1f);
                    _weapons[activeWeaponIndex].transform.DOLocalRotate(Vector3.zero, 0.1f);
                }
            }

            _previuseStateIsFire = _isShooting;
        }

        public bool AddWeapon(ArenaWeapon newWeapon)
        {
            if (_weapons.Count >= maxWeaponSlots)
            {
                Debug.Log("No free weapon slots!");
                return false;
            }

            newWeapon.transform.SetParent(weaponHolder);
            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
            newWeapon.data = _ammoText;
            _weapons.Add(newWeapon);
            newWeapon.gameObject.SetActive(_weapons.Count == 1); // Активируем только если это первое оружие

            // Создаем UI элемент для оружия
            var weaponUI = Instantiate(_weaponUIprefab, _weaponsContainer);
            var button = weaponUI.GetComponent<UnityEngine.UI.Button>();
            var image = weaponUI.GetComponentInChildren<UnityEngine.UI.Image>();
            var ammoText = weaponUI.GetComponentInChildren<TMP_Text>();

            // Устанавливаем иконку оружия
            if (newWeapon.icon != null)
            {
                image.sprite = newWeapon.icon;
            }

            // Обновляем текст с патронами
            ammoText.text = $"{newWeapon.CurrentAmmo}/{newWeapon.TotalAmmo}";

            // Привязываем обработчик нажатия кнопки
            int weaponIndex = _weapons.Count - 1;
            button.onClick.AddListener(() => SwitchToWeapon(weaponIndex));
            newWeapon.currentAmmoTextInUI=ammoText;
            return true;
        }

        private void SwitchToWeapon(int index)
        {
            if (index < 0 || index >= _weapons.Count) return;
        
            _weapons[activeWeaponIndex].gameObject.SetActive(false);
            activeWeaponIndex = index;
            _weapons[activeWeaponIndex].gameObject.SetActive(true);
            _ammoText.text = _weapons[activeWeaponIndex].bulletInMagazine + "/" + _weapons[activeWeaponIndex].allAmmo;
        }

        public void DropCurrentWeapon()
        {
            if (_weapons.Count == 0) return;
        
            ArenaWeapon weaponToDrop = _weapons[activeWeaponIndex];
            _weapons.RemoveAt(activeWeaponIndex);
        
            weaponToDrop.transform.SetParent(null);
            weaponToDrop.gameObject.SetActive(true);
        
            // Переключаем на предыдущее оружие
            if (_weapons.Count > 0)
            {
                activeWeaponIndex = Mathf.Clamp(activeWeaponIndex - 1, 0, _weapons.Count - 1);
                _weapons[activeWeaponIndex].gameObject.SetActive(true);
            }
        }

        #region Upgrade Methods
        public void UpgradeHealth()
        {
            maxHealth += healthUpgradeAmount;
            _currentHealth = maxHealth;
        }
       

        public void AddWeaponSlot()
        {
            if (maxWeaponSlots < 4)
            {
                maxWeaponSlots++;
            }
        }
        #endregion
    }
}
using System;
using ArtNotes.UndergroundLaboratoryGenerator;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NetWorking;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GameEngine
{
    public sealed class PlayerStateController : MonoBehaviour
    {
        [Header("Movement setting")]
        [SerializeField] private float _speedRun,_speedWalk;
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private Animator _animator;
        [SerializeField] private Renderer[] _lightMaterial;
      
        private NetworkPlayerController _networkPlayerController;
        private Vector2 _direction, _fireDirection;
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        private static readonly int FireState = Animator.StringToHash("FireState");
        private Vector2 _prevRigi;
        
        private static readonly int VectorBuffer = Shader.PropertyToID("_LightPositions");
        private static readonly int RangeIntesivityBuffer = Shader.PropertyToID("_RangeIntesivityBuffer");
        private static readonly int LightColors = Shader.PropertyToID("_LightColors");
        private static readonly int MaxLights = Shader.PropertyToID("_MaxLights");
        //private Transform cameraTarget => GetComponent<NetworkPlayerController>().cameraTarget;

        private EPlayerState _currentState;

        private async void Awake()
        {
            _networkPlayerController = GetComponent<NetworkPlayerController>();
            if (!_collider2D) _collider2D = GetComponent<Collider2D>();
          
            _animator = GetComponentInChildren<Animator>();
            var stageCreator = FindAnyObjectByType<Laboratory2DGenerator>();
            await UniTask.WaitWhile(() => stageCreator.stageCreated);
            _networkPlayerController.SetAimVisual(false);
            await UniTask.DelayFrame(220);
            _collider2D.enabled = true;
        }
        public void Move(Vector2 direction, Vector2 fireDirection)
        {
            _direction = direction.normalized;
            _fireDirection = fireDirection.normalized;
            if (fireDirection == Vector2.zero)
            {
                _animator.SetBool(FireState, false);
            }
            else
            {
                _animator.SetBool(FireState, true);
            }
        }
      

        private void FixedUpdate()
        {
            if (_direction == Vector2.zero) _direction = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (_fireDirection==Vector2.zero &&_direction != Vector2.zero)
            {
                transform.up = _direction;
                if (_currentState!=EPlayerState.NonFire)
                    _networkPlayerController.SetAimVisual(false);
                _rigidbody2D.linearVelocity = _direction * _speedRun;
                _animator.SetFloat(InputX,1);
                _currentState = EPlayerState.NonFire;
            }
            else if (_fireDirection != Vector2.zero)
            {
	            transform.up = _fireDirection;
                _rigidbody2D.linearVelocity = _direction * _speedWalk;
                if (_currentState != EPlayerState.Fire)
                {
                    if (_direction != Vector2.zero) _networkPlayerController.SetAimVisual(true);
                    else
                    {
                        _networkPlayerController.SetAimVisual(true);
                    }
                }

                var realDirection = transform.InverseTransformDirection(_direction);
                _animator.SetFloat(InputX,realDirection.x);
                _animator.SetFloat(InputY,realDirection.y);
                _currentState = EPlayerState.Fire;
                _networkPlayerController.NetworkFire();
            }
            else
            {
                _rigidbody2D.linearVelocity = Vector2.zero;
                _animator.SetFloat(InputX,0);
                if (_currentState!=EPlayerState.NonFire)
                    _networkPlayerController.SetAimVisual(false);
                _currentState = EPlayerState.NonFire;
            }
            var r = transform.eulerAngles;
            transform.eulerAngles = new Vector3(0, 0, r.z);
        }

        public void SetMaterialLightBuffer(ComputeBuffer vectorBuffer, ComputeBuffer rangeIntesivityBuffer, ComputeBuffer colorsBuffer, int lightsLength)
        {
            foreach (var mat in _lightMaterial)
            {
                mat.material.SetBuffer(VectorBuffer, vectorBuffer);
                mat.material.SetBuffer(RangeIntesivityBuffer, rangeIntesivityBuffer);
                mat.material.SetBuffer(LightColors, colorsBuffer);
                mat.material.SetFloat(MaxLights, lightsLength);
            }
        }
    }
}

public enum EPlayerState
{
    NonFire, Fire
}
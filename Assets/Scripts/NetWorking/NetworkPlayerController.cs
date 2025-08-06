using System;
using DG.Tweening;
using Fusion;
using GameEngine;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace NetWorking
{
    public sealed class NetworkPlayerController : NetworkBehaviour
    {
        
        [SerializeField] private PlayerStateController playerStateController;
        [SerializeField] private NetWorkInputReceiver _inputReceiver;
        [SerializeField] private Animator _animator;
      
        [SerializeField] private float _maxHealth;
        private static readonly int damageX = Animator.StringToHash("DamageX");
        private static readonly int damageY = Animator.StringToHash("DamageY");
        private static readonly int damage1 = Animator.StringToHash("Damage");

        [Header("Animation setting")] [SerializeField]
        private Transform _lefthand;
        [SerializeField] private Transform _rightHand;
        [SerializeField] private TwoBoneIKConstraint _leftRig, _rightRig;
        
        [Header("Gun setup")] 
        [SerializeField] private Transform _leftGunRig;
        [SerializeField] private Transform _rightgunRig, _gunPosition, _gun;
        [SerializeField] private BaseGun _gunController;
        
     
        public float currentHealth { get; private set; }
        public override void Spawned()
        {
	        currentHealth = _maxHealth;
            transform.position = Vector3.zero;
            if (HasInputAuthority)
            {
                var cnm = FindAnyObjectByType<SimpleCameraController>();
                cnm.SetTarget(transform);
            }
            SetAimVisual(false);
        }

        public void TakeDamage(Vector2 direction, float damage)
        {
	        if (!Runner.IsServer) return;
	        currentHealth -= damage;
	        _animator.SetFloat(damageX,direction.x);
	        _animator.SetFloat(damageY,direction.y);
	        _animator.SetTrigger(damage1);
	        RpcDamage(direction);
        }
        public void SetAimVisual(bool isAiming)
        {
	        if (isAiming)
	        {
		        _leftRig.weight = 1;
		        _rightRig.weight = 1;
				_gun.SetParent(_gunPosition);
	        }
	        else
	        {
		        _rightRig.weight = 0;
		        _gun.SetParent(_rightHand);
	        }

	        if (_gun.localPosition != Vector3.zero || _gun.localEulerAngles != Vector3.zero)
	        {
		        _gun.DOLocalRotate(Vector3.zero, 0.2f);
		        _gun.DOLocalMove(Vector3.zero, 0.2f); 
	        }

	        RpcSetAimVisual(isAiming);
        }

        [Rpc]
        private void RpcSetAimVisual(bool isAiming)
        {
	        if (isAiming)
	        {
		        _leftRig.weight = 1;
		        _rightRig.weight = 1;
		        _gun.SetParent(_gunPosition);
		        _gunController.IsTrailOn(true);
	        }
	        else
	        {
		        _rightRig.weight = 0;
		        _gun.SetParent(_rightHand);
		        _gunController.IsTrailOn(false);
	        }

	        if (_gun.localPosition != Vector3.zero || _gun.localEulerAngles != Vector3.zero)
	        {
		        _gun.DOLocalRotate(Vector3.zero, 0.2f);
		        _gun.DOLocalMove(Vector3.zero, 0.2f); 
	        }
        }
      

        [Rpc]
        public void RpcDamage(Vector2 direction)
        {
	        _animator.SetFloat(damageX,direction.x);
	        _animator.SetFloat(damageY,direction.y);
	        _animator.SetTrigger(damage1); 
        }

        public override void FixedUpdateNetwork()
        {
	        _leftRig.transform.position = _leftGunRig.position;
	        _rightRig.transform.position = _rightgunRig.position;
			playerStateController.Move(_inputReceiver.MovementInput, _inputReceiver.FireInput);
        }

        public void NetworkFire()
        {
	        if (Runner.IsServer)
		        _gunController.ServerFire(Runner.DeltaTime);
	        RpcNetworkFire();
        }

        [Rpc]
        public void RpcNetworkFire()
        {
	        _gunController.Fire(Runner.DeltaTime);
        }
        
    }
}

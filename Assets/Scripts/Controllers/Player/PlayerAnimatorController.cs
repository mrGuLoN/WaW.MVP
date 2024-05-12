using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerAnimatorController : NetworkBehaviour,IPlayerControllers
{
   private Animator _animator;
   private Transform _thisTR;
   private PlayerController _playerController;
   
   private static readonly int IsGunState = Animator.StringToHash("IsGunState");
   private static readonly int Reload1 = Animator.StringToHash("Reload");
   private static readonly int X = Animator.StringToHash("X");
   private static readonly int Y = Animator.StringToHash("Y");
   private Vector2 _direction;

   public void Initialise(IPlayerControllers[] playerControllersArray,bool thisIsOwner)
   {
      _playerController = playerControllersArray.FirstOrDefault(x=>x is PlayerController)as PlayerController;
   }

   public void MakeSubscriptions()
   {
      _playerController.onMovedAndRotation += SetMoveServerRpc;
      _playerController.onGunState += SetGunStateServerRpc;
      _playerController.onReload += Reload;
      Starting();
   }

   private void Starting()
   {
      _animator = GetComponentInChildren<Animator>();
      _thisTR = GetComponent<Transform>();
      if (_playerController.animatorController)
         _animator.runtimeAnimatorController = _playerController.animatorController;
   }
   [ServerRpc]
   private void SetGunStateServerRpc(int isGunState)
   {
      Debug.Log(isGunState);
      _animator.SetInteger(IsGunState,isGunState);
   }
 
   private void Reload()
   {
      _animator.SetTrigger(Reload1);
   }
   [ServerRpc]
   private void SetMoveServerRpc(Vector2 move, Vector2 rotate)
   {
      _direction = _thisTR.InverseTransformVector(move).normalized;
      _animator.SetFloat(X, _direction.x);
      _animator.SetFloat(Y, _direction.y);
   }

   private void OnDestroy()
   {
      _playerController.onMovedAndRotation -= SetMoveServerRpc;
      _playerController.onGunState -= SetGunStateServerRpc;
      _playerController.onReload -= Reload;
   }
}

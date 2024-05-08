using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour,IPlayerControllers
{
   private Animator _animator;
   private Transform _thisTR;
   private PlayerController _playerController;
   
   private static readonly int IsGunState = Animator.StringToHash("IsGunState");
   private static readonly int IsFire = Animator.StringToHash("isFire");
   private static readonly int Reload1 = Animator.StringToHash("Reload");
   private static readonly int X = Animator.StringToHash("X");
   private static readonly int Y = Animator.StringToHash("Y");

   public void Initialise(IPlayerControllers[] playerControllersArray)
   {
      _playerController = playerControllersArray.FirstOrDefault(x=>x is PlayerController)as PlayerController;
   }

   public void MakeSubscriptions()
   {
      _playerController.onMoved += SetMove;
      _playerController.onFire += SetFireState;
      _playerController.onGunState += SetGunState;
      _playerController.onReload += Reload;
   }

   private void Start()
   {
      _animator = GetComponentInChildren<Animator>();
      _thisTR = GetComponent<Transform>();
   }

   private void SetGunState(bool isGunState)
   {
      _animator.SetBool(IsGunState,isGunState);
   }
   private void SetFireState(bool isFireState)
   {
      _animator.SetBool(IsFire,isFireState);
   }
   private void Reload()
   {
      _animator.SetTrigger(Reload1);
   }

   private void SetMove(Vector3 direction)
   {
      direction = _thisTR.InverseTransformVector(direction).normalized;
      _animator.SetFloat(X, direction.x);
      _animator.SetFloat(Y, direction.y);
   }

   private void OnDestroy()
   {
      _playerController.onMoved -= SetMove;
      _playerController.onFire -= SetFireState;
      _playerController.onGunState -= SetGunState;
      _playerController.onReload -= Reload;
   }
}

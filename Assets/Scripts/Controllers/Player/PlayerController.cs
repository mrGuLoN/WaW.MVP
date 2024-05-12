using System;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour, IDamagable, IMovable,IPlayerControllers
{
    public Action<Vector2,Vector2,bool> onMovedAndRotation;
    public Action<bool> onGunState;
    public Action onReload;

    public AnimatorOverrideController animatorController=>_animatorController;
    
    [SerializeField] private float _speed;
    [SerializeField] private AnimatorOverrideController _animatorController;
    [SerializeField] private GunPosition[] _gunPositions;
    [SerializeField] private BaseGunController _firstGun, _secondGun;

   
    private BaseGunController _activeGun, _passiveGun;
    private Rigidbody2D _rb;
    private Vector2 _inputDirection;
    private Transform _thisTR;

    #region StateMachine

    

    #endregion

    #region MonoBehaviour

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _thisTR = GetComponent<Transform>();
        var controllers = GetComponents<IPlayerControllers>();
        foreach (var controller in controllers)
        {
            controller.Initialise(controllers, IsOwner);
        }
        foreach (var controller in controllers)
        {
            controller.MakeSubscriptions();
        }
        
        Debug.Log(IsOwner);
        if (!IsOwner) return;
        Camera.main.GetComponent<SimpleCameraController>().SetTarget(transform);
        RespawnGunsOnStartServerRpc();
    }
    [ServerRpc]
    private void RespawnGunsOnStartServerRpc()
    {
        _activeGun = Instantiate(_firstGun,_gunPositions.FirstOrDefault(x=>x.namePosition==_firstGun.activePosition)?.transformPosition );
        _activeGun.GetComponent<NetworkObject>().Spawn();
        _activeGun.GetComponent<NetworkTransform>().OnNetworkObjectParentChanged(GetComponent<NetworkObject>());
        _activeGun.InitializeServerRpc();

        _passiveGun = Instantiate(_secondGun,_gunPositions.FirstOrDefault(x=>x.namePosition==_secondGun.notActivePosition)?.transformPosition );
        _passiveGun.GetComponent<NetworkObject>().Spawn();
        _passiveGun.GetComponent<NetworkTransform>().OnNetworkObjectParentChanged(GetComponent<NetworkObject>());
        _passiveGun.InitializeServerRpc();
    }

    private void OnDestroy()
    {
        onMovedAndRotation -= MoveAndRotation;
    }
    #endregion

    #region IPlayerControllers
    
    public void Initialise(IPlayerControllers[] playerControllersArray, bool thisIsOwner)
    {
       
    }

    public void MakeSubscriptions()
    {
        onMovedAndRotation += MoveAndRotation;
    }
    #endregion

    #region Health
    public void TakeDamage(Vector2 direction, float damage)
    {
    }
    #endregion

    #region Movement

    private void MoveAndRotation(Vector2 move, Vector2 rotate, bool isFire)
    {
        _rb.velocity = move.normalized * _speed;
        if (rotate != Vector2.zero)
        {
            _thisTR.up = rotate;
            Vector3 NullRotate = _thisTR.eulerAngles;
            NullRotate = new Vector3(0,0, NullRotate.z);
            _thisTR.rotation = Quaternion.Euler(NullRotate);
        }

        MoveAndRotationServerRpc(move, rotate, isFire);
    }

    [ServerRpc]
    public void MoveAndRotationServerRpc(Vector2 move, Vector2 rotate, bool isFire)
    {
        _rb.velocity = move.normalized * _speed;
        if (rotate != Vector2.zero)
        {
            _thisTR.up = rotate;
            Vector3 NullRotate = _thisTR.eulerAngles;
            NullRotate = new Vector3(0,0, NullRotate.z);
            _thisTR.rotation = Quaternion.Euler(NullRotate);
        }
    }
   

    #endregion
}
[Serializable]
public class GunPosition
{
    public NamegunPosition namePosition;
    public Transform transformPosition;
}

public enum NamegunPosition
{
    LeftHand,
    RightHand,
    LeftLegs,
    RightLegs,
    Spine
}

public enum AnimationTriggerType
{
    
}

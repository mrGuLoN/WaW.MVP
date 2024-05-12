using System;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerController : NetworkBehaviour, IDamagable, IMovable,IPlayerControllers
{
    public Action<Vector2,Vector2> onMovedAndRotation;
    public Action<bool> onGunState;
    public Action onReload;

    public AnimatorOverrideController animatorController=>_animatorController;
    
    [SerializeField] private float _speed;
    [SerializeField] private AnimatorOverrideController _animatorController;
    [SerializeField] private GunPosition[] _gunPositions;
    [SerializeField] private BaseGunController _firstGun, _secondGun;

    public BaseGunController currentGun => _activeGun;
   
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
        RespawnGunsOnStart();
        if (!IsOwner) return;
        Camera.main.GetComponent<SimpleCameraController>().SetTarget(transform);
    }

  
    
    private void RespawnGunsOnStart()
    {
        _activeGun = Instantiate(_firstGun,_gunPositions.FirstOrDefault(x=>x.namePosition==_firstGun.activePosition)?.transformPosition );  
        _activeGun.transform.localRotation = Quaternion.Euler(0,0,0);
        _activeGun.transform.localPosition = Vector3.zero;

        _passiveGun = Instantiate(_secondGun,_gunPositions.FirstOrDefault(x=>x.namePosition==_secondGun.notActivePosition)?.transformPosition );
        _passiveGun.transform.localRotation = Quaternion.Euler(0,0,0);
        _passiveGun.transform.localPosition = Vector3.zero;
        if (IsOwner)
        {
            _activeGun.Initialize();
            _passiveGun.Initialize();
        }
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

    private void MoveAndRotation(Vector2 move, Vector2 rotate)
    {
        _rb.velocity = move.normalized * _speed;
        if (rotate != Vector2.zero)
        {
            _thisTR.up = rotate;
            Vector3 NullRotate = _thisTR.eulerAngles;
            NullRotate = new Vector3(0,0, NullRotate.z);
            _thisTR.rotation = Quaternion.Euler(NullRotate);
        }
        MoveAndRotationServerRpc(move, rotate);
    }

    [ServerRpc]
    public void MoveAndRotationServerRpc(Vector2 move, Vector2 rotate)
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

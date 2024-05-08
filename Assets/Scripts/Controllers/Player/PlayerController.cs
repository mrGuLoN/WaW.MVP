using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamagable, IMovable,IPlayerControllers
{
    public Action<Vector3> onMoved;
    public Action<Vector3> onRotated;
    public Action<bool> onGunState;
    public Action<bool> onFire;
    public Action onReload;
    
    [SerializeField] private float _speed;
    
    
    private Rigidbody2D _rb;
    private Vector2 _inputDirection;
    private Transform _thisTR;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _thisTR = GetComponent<Transform>();
        var controllers = GetComponents<IPlayerControllers>();
        foreach (var controller in controllers)
        {
            controller.Initialise(controllers);
        }
        foreach (var controller in controllers)
        {
            controller.MakeSubscriptions();
        }
    }

    public void Initialise(IPlayerControllers[] playerControllersArray)
    {
       
    }

    public void MakeSubscriptions()
    {
        onMoved += Move;
        onRotated += Rotation;
    }

    private void Update()
    {
        _inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        onMoved?.Invoke(_inputDirection);
        onRotated?.Invoke(_inputDirection);
    }

    public void TakeDamage(Vector2 direction, float damage)
    {
    }

    public void Move(Vector3 direction)
    {
        direction = direction.normalized;
        _rb.velocity = (Vector2)direction * _speed;
    }

    public void Rotation(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            _thisTR.up = direction;
            Vector3 NullRotate = _thisTR.eulerAngles;
            NullRotate = new Vector3(0,0, NullRotate.z);
            _thisTR.rotation = Quaternion.Euler(NullRotate);
        }
    }

    private void OnDestroy()
    {
        onMoved -= Move;
        onRotated -= Rotation;
    }
}

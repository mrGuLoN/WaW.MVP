using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamagable, IMovable
{
    [SerializeField] private float _speed;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;

    private Vector2 _inputDirection;
    private Transform _thisTR;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _thisTR = GetComponent<Transform>();
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        _inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Move(_inputDirection,_speed);
    }

    public void TakeDamage(Vector2 direction, float damage)
    {
    }

    public void Move(Vector2 direction, float speed)
    {
        direction = direction.normalized;
        _rb.velocity = direction * speed;
        if (direction != Vector2.zero)
        {
            _thisTR.up = direction;
            Vector3 NullRotate = _thisTR.eulerAngles;
            NullRotate = new Vector3(0,0, NullRotate.z);
            _thisTR.rotation = Quaternion.Euler(NullRotate);
            _animator.SetFloat("X",1);
        }
        else
        {
            _animator.SetFloat("X",0);
        }
    }
}

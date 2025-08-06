using System;
using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    private Transform _target;
    private Transform _thisTR;

    private void Start()
    {
        _thisTR = GetComponent<Transform>();
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _thisTR.position = _target.position + _offset;
        _thisTR.LookAt(_target.position);
    }

    private void LateUpdate()
    {
        if (_target != null)
        {
            _thisTR.position = _target.position + _offset;
        }
    }
}

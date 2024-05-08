using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [SerializeField] private Transform _player;
    private Transform _thisTR;
    private Vector3 _direction;
    void Start()
    {
        _thisTR = GetComponent<Transform>();
        _direction = _player.position - _thisTR.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        _thisTR.position = _player.position - _direction;
    }
}

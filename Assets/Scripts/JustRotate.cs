using System;
using UnityEngine;

public class JustRotate : MonoBehaviour
{
    [SerializeField] private Transform[] _transforms;
    [SerializeField] private Vector3 _speedRotate;

    private void Update()
    {
        foreach (var t in _transforms)
        {
            t.eulerAngles += _speedRotate * Time.deltaTime;
        }
    }
}

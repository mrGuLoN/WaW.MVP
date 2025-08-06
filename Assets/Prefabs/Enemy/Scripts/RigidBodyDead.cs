using System;
using UnityEngine;
using Random = UnityEngine.Random;


public class RigidBodyDead : MonoBehaviour
{
    [SerializeField] private float _deadForce;
    public void DeadAnimation(Vector2 direction, float damage)
    {
        Debug.Log(direction + " : " + damage + " damage");
        var rbs = GetComponentsInChildren<Rigidbody>();
        rbs[Random.Range(0, rbs.Length)].AddForce(direction*_deadForce*damage,ForceMode.Impulse);
    }
}

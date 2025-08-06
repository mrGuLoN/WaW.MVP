using System;
using Fusion;
using UnityEngine;

public class NetworkParticleController : NetworkBehaviour
{
    [SerializeField] private PartycleType[] _partycleTypes;
   
    public void PlayPartycle(ETypePart type, Vector3 position, Vector3 direction)
    {
        for (int i =0; i < _partycleTypes.Length; i++)
        {
            if (_partycleTypes[i].type == type)
            {
                _partycleTypes[i].ps.transform.position = position;
                _partycleTypes[i].ps.transform.forward = direction;
                _partycleTypes[i].ps.Play();
                RpcPlayPartycle(i, position, direction);
                return;
            }
        }
    }

    [Rpc]
    public void RpcPlayPartycle(int pos, Vector3 position, Vector3 direction)
    {
        _partycleTypes[pos].ps.transform.position = position;
        _partycleTypes[pos].ps.transform.forward = direction;
        _partycleTypes[pos].ps.Play();
    }
}
[Serializable]
public enum ETypePart
{
    EnemyBlood, WallHit, EnemyDead
}

[Serializable]
public class PartycleType
{
    public ETypePart type;
    public ParticleSystem ps;
}
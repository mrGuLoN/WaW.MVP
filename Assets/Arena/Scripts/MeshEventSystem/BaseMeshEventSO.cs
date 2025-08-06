using Arena.Scripts.Controllers;
using UnityEngine;

namespace Arena.Scripts.MeshEventSystem
{
    public class BaseMeshEventSo : ScriptableObject
    {
        public virtual EParticleType DoEvent()
        {
            return EParticleType.Null;
        }
    }
}

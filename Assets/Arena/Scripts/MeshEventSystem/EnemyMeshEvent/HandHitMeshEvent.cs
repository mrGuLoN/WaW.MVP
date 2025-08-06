using Arena.Scripts.Controllers;
using UnityEngine;

namespace Arena.Scripts.MeshEventSystem.EnemyMeshEvent
{
   [CreateAssetMenu(fileName = "HandHitMeshEvent", menuName = "MeshEvents/HandHitMeshEvent", order = 1)]
   public class HandHitMeshEvent : BaseMeshEventSo
   {
      [SerializeField] private EParticleType _particleType;
      public override EParticleType DoEvent()
      {
            return _particleType;
      }
   }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class LightsSetup : MonoBehaviour
{
   [SerializeField] private Renderer[] _materials;
   [SerializeField] private EnemyController _enemyController;
   [SerializeField] private Light[] pointLights;
   
   private ComputeBuffer vectorBuffer, rangeIntesivityBuffer, colorsBuffer;

   private static readonly int VectorBuffer = Shader.PropertyToID("_LightPositions");
   private static readonly int RangeIntesivityBuffer = Shader.PropertyToID("_RangeIntesivityBuffer");
   private static readonly int LightColors = Shader.PropertyToID("_LightColors");
   private static readonly int MaxLights = Shader.PropertyToID("_MaxLights");
   private void Start()
   {
      List<Light> lightsTemp = new List<Light>();
      lightsTemp.AddRange(pointLights);
      
      List<Vector3> pos = new List<Vector3>();
      List<Vector2> rng = new List<Vector2>();
      List<Color> col = new List<Color>();

      foreach (var l in lightsTemp)
      {
         pos.Add(l.transform.position);
         rng.Add(new Vector2(l.range, l.intensity));
         col.Add(l.color);
      }

      //if (vectorBuffer != null) vectorBuffer.Release();
      vectorBuffer = new ComputeBuffer(pos.Count, sizeof(float) * 3);
      vectorBuffer.SetData(pos);

      //if (rangeIntesivityBuffer != null) rangeIntesivityBuffer.Release();
      rangeIntesivityBuffer = new ComputeBuffer(rng.Count, sizeof(float) * 2);
      rangeIntesivityBuffer.SetData(rng);
        
      //if (colorsBuffer != null) rangeIntesivityBuffer.Release();
      colorsBuffer = new ComputeBuffer(col.Count, sizeof(float) * 4);
      colorsBuffer.SetData(col);

      foreach (var r in _enemyController.allMaterials)
      {
         r.material.SetBuffer(VectorBuffer, vectorBuffer);
         r.material.SetBuffer(RangeIntesivityBuffer, rangeIntesivityBuffer);
         r.material.SetBuffer(LightColors, colorsBuffer);
         r.material.SetFloat(MaxLights, pointLights.Length);
      }
   }

   private void FixedUpdate()
   {
      if (Input.GetKeyDown(KeyCode.Space))
         _enemyController.StateWork(EEnemyState.Dead, null);
   }
}

using UnityEngine;

public class CreateLights : MonoBehaviour
{
   [SerializeField] private Transform _lbTR;
   [SerializeField] private float _multiple =0;
#if UNITY_EDITOR
   [EditorButton("Create Lights")]
   #endif
   public void CreateLight()
   {
      var lights = GameObject.FindObjectsOfType<Light>();
      foreach (var l in lights)
      {
         var light = new GameObject();
         switch (l.type)
         {
            case LightType.Point:
               var bl = light.AddComponent<BakeryPointLight>();
               light.name = "PointLight";
               bl.projMode = BakeryPointLight.ftLightProjectionMode.Omni;
               bl.transform.position = l.transform.position;
               bl.transform.rotation = l.transform.rotation;
               bl.intensity = l.intensity*_multiple;
               bl.color = l.color;
               bl.cutoff = l.range;
               bl.transform.SetParent(_lbTR);
               break;
            case LightType.Directional:
               var dl = light.AddComponent<BakeryDirectLight>();
               light.name = "DirectionalLight";
               dl.transform.position = l.transform.position;
               dl.transform.rotation = l.transform.rotation;
               dl.intensity = l.intensity*_multiple;
               dl.color = l.color;
               dl.transform.SetParent(_lbTR);
               break;
            case LightType.Rectangle:
               var ar = light.AddComponent<BakeryLightMesh>();
               light.name = "AreaLight";
               ar.transform.localScale = l.transform.localScale;
               ar.transform.position = l.transform.position;
               ar.transform.rotation = l.transform.rotation;
               ar.intensity = l.intensity*_multiple;
               ar.color = l.color;
               ar.transform.SetParent(_lbTR);
               break;
            case LightType.Spot:
               var pl = light.AddComponent<BakeryPointLight>();
               light.name = "SpotLight";
               pl.projMode = BakeryPointLight.ftLightProjectionMode.Cookie;
               pl.transform.position = l.transform.position;
               pl.transform.rotation = l.transform.rotation;
               pl.intensity = l.intensity*_multiple;
               pl.color = l.color;
               pl.cutoff = l.range;
               pl.transform.SetParent(_lbTR);
               break;
         }
      }
   }
}

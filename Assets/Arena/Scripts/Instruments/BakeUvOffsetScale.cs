// GetLightmapUVDataEditor.cs
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
public class GetLightmapUVDataEditor : EditorWindow
{
    public GameObject targetObject;
    public Material targetMaterial;

    [MenuItem("Tools/Get Lightmap UV Data")]
    public static void ShowWindow()
    {
        GetWindow<GetLightmapUVDataEditor>("Get Lightmap UV Data");
    }

    private void OnGUI()
    {
        GUILayout.Label("Get Lightmap UV Data", EditorStyles.boldLabel);

        targetObject = (GameObject)EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
        targetMaterial = (Material)EditorGUILayout.ObjectField("Target Material", targetMaterial, typeof(Material), false);

        if (GUILayout.Button("Get Lightmap Data and Apply"))
        {
            if (targetObject == null || targetMaterial == null)
            {
                Debug.LogError("Please assign both a target object and a material.");
                return;
            }

            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("Target object does not have a renderer.");
                return;
            }

            if (renderer.lightmapIndex == -1)
            {
                Debug.LogError("Target object is not using lightmapping.");
                return;
            }

            int lightmapIndex = renderer.lightmapIndex;

            if (lightmapIndex >= LightmapSettings.lightmaps.Length)
            {
                Debug.LogError("Lightmap index is out of range.");
                return;
            }

            LightmapData lightmapData = LightmapSettings.lightmaps[lightmapIndex];
            Vector4 lightmapScaleOffset = renderer.lightmapScaleOffset;

            Undo.RecordObject(targetMaterial, "Set Lightmap Data"); // Enable undo

            // Set the scale and offset values in the material
            targetMaterial.SetVector("_BakedLightingTex_ST", lightmapScaleOffset);
            targetMaterial.SetTexture("_BakedLightingTex", LightmapSettings.lightmaps[lightmapIndex].lightmapColor);

            EditorUtility.SetDirty(targetMaterial);
        }
    }
}
#endif
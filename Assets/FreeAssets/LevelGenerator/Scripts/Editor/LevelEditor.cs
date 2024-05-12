using UnityEditor;
using UnityEngine;

namespace LevelGenerator.Scripts.Editor
{
    [CustomEditor(typeof(LevelGenerator))]
    public class GeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelGenerator myScript = (LevelGenerator)target;
            if (GUILayout.Button("Create Labirint"))
            {
                myScript.Start();
            }
            if (GUILayout.Button("Retransform light"))
            {
                myScript.RetransformLight();
            }
            if (GUILayout.Button("Retransform walls"))
            {
                myScript.RetransformWall2D();
            }
            if (GUILayout.Button("Add Section Template"))
            {
                myScript.AddSectionTemplate();
            }

            if (GUILayout.Button("Add Dead End Template"))
            {
                myScript.AddDeadEndTemplate();
            }
        }
    }
}

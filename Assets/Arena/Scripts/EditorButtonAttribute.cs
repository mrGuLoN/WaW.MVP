#if UNITY_EDITOR
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class EditorButtonAttribute : PropertyAttribute
{
    public string ButtonName { get; }

    public EditorButtonAttribute(string buttonName = null)
    {
        ButtonName = buttonName;
    }
}
#endif
#if UNITY_EDITOR
[CustomEditor(typeof(MonoBehaviour), true)]
public class EditorButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var mono = target as MonoBehaviour;
        if (mono == null) return;

        // Получаем все методы с атрибутом EditorButton
        var methods = mono.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(EditorButtonAttribute), false).Length > 0);

        foreach (var method in methods)
        {
            var attr = (EditorButtonAttribute)method.GetCustomAttributes(typeof(EditorButtonAttribute), false)[0];
            var buttonName = string.IsNullOrEmpty(attr.ButtonName) ? method.Name : attr.ButtonName;

            if (GUILayout.Button(buttonName))
            {
                // Вызываем метод при нажатии кнопки
                method.Invoke(mono, null);
            }
        }
    }
}
#endif
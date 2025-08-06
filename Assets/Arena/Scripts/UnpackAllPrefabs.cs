using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_EDITOR
public class UnpackAllPrefabs : EditorWindow
{
    
    [MenuItem("Tools/Unpack Root Prefabs Only")]
    public static void UnpackRootPrefabsOnly()
    {
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        List<GameObject> rootPrefabs = new List<GameObject>();

        // Собираем только корневые префабы
        foreach (GameObject obj in rootObjects)
        {
            FindRootPrefabs(obj, rootPrefabs);
        }

        // Распаковываем только корневые префабы
        foreach (GameObject prefab in rootPrefabs)
        {
            PrefabUtility.UnpackPrefabInstance(prefab, PrefabUnpackMode.OutermostRoot, InteractionMode.UserAction);
            Debug.Log($"Unpacked root prefab: {prefab.name}", prefab);
        }

        Debug.Log($"Unpacked {rootPrefabs.Count} root prefabs");
    }

    private static void FindRootPrefabs(GameObject parent, List<GameObject> rootPrefabs)
    {
        // Если это префаб и НЕ является частью другого префаба
        if (PrefabUtility.IsAnyPrefabInstanceRoot(parent) && !IsPartOfLargerPrefab(parent))
        {
            if (!rootPrefabs.Contains(parent))
            {
                rootPrefabs.Add(parent);
            }
            return; // Не проверяем детей, так как они часть этого префаба
        }

        // Рекурсивно проверяем детей (только для НЕ-префабов)
        foreach (Transform child in parent.transform)
        {
            FindRootPrefabs(child.gameObject, rootPrefabs);
        }
    }

    private static bool IsPartOfLargerPrefab(GameObject obj)
    {
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(parent.gameObject))
            {
                return true; // Найден родитель-префаб
            }
            parent = parent.parent;
        }
        return false; // Не является частью другого префаба
    }
 
}
#endif
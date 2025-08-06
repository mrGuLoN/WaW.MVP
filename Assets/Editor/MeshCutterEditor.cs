using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MeshCutterEditor : EditorWindow
{
    public Camera camera;
    public Vector3 startPoint; // Начальная точка прямоугольника
    public Vector3 endPoint;   // Конечная точка прямоугольника
    public float movementDistance = 10f; // Расстояние перемещения камеры
    public float stepSize = 0.5f; // Шаг перемещения камеры

    [MenuItem("Tools/Mesh Cutter")]
    public static void ShowWindow()
    {
        GetWindow<MeshCutterEditor>("Mesh Cutter");
    }

    void OnGUI()
    {
        camera = (Camera)EditorGUILayout.ObjectField("Camera", camera, typeof(Camera), true);
        startPoint = EditorGUILayout.Vector3Field("Start Point", startPoint);
        endPoint = EditorGUILayout.Vector3Field("End Point", endPoint);
        movementDistance = EditorGUILayout.FloatField("Movement Distance", movementDistance);
        stepSize = EditorGUILayout.FloatField("Step Size", stepSize);

        if (GUILayout.Button("Cut Invisible Meshes"))
        {
            CutInvisibleMeshes();
        }
    }

    void CutInvisibleMeshes()
    {
        if (camera == null)
        {
            Debug.LogError("Camera is not assigned!");
            return;
        }

        MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            Mesh originalMesh = meshFilter.sharedMesh;
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();

            Vector3[] vertices = originalMesh.vertices;
            int[] triangles = originalMesh.triangles;

            bool isVisible = false;

            // Перемещаем камеру по заданной плоскости
            for (float xOffset = startPoint.x; xOffset <= endPoint.x; xOffset += stepSize)
            {
                for (float zOffset = startPoint.z; zOffset <= endPoint.z; zOffset += stepSize)
                {
                    Vector3 cameraPosition = new Vector3(xOffset, camera.transform.position.y, zOffset);
                    camera.transform.position = cameraPosition;

                    Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

                    for (int i = 0; i < triangles.Length; i += 3)
                    {
                        Vector3 v1 = meshFilter.transform.TransformPoint(vertices[triangles[i]]);
                        Vector3 v2 = meshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
                        Vector3 v3 = meshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);

                        if (IsTriangleVisible(v1, v2, v3, frustumPlanes))
                        {
                            isVisible = true; // Если хотя бы один треугольник виден, ставим флаг
                            break;
                        }
                    }

                    if (isVisible) break; // Если нашли видимый треугольник, выходим из цикла
                }

                if (isVisible) break; // Если нашли видимый треугольник, выходим из внешнего цикла
            }

            if (!isVisible)
            {
                DestroyImmediate(meshFilter.gameObject); // Удаляем объект, если он полностью невидим
            }
            else
            {
                // Если объект виден, создаем новый меш
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 v1 = meshFilter.transform.TransformPoint(vertices[triangles[i]]);
                    Vector3 v2 = meshFilter.transform.TransformPoint(vertices[triangles[i + 1]]);
                    Vector3 v3 = meshFilter.transform.TransformPoint(vertices[triangles[i + 2]]);

                    if (IsTriangleVisible(v1, v2, v3, GeometryUtility.CalculateFrustumPlanes(camera)))
                    {
                        int index1 = AddVertex(v1, newVertices);
                        int index2 = AddVertex(v2, newVertices);
                        int index3 = AddVertex(v3, newVertices);

                        newTriangles.Add(index1);
                        newTriangles.Add(index2);
                        newTriangles.Add(index3);
                    }
                }

                if (newVertices.Count > 0)
                {
                    Mesh newMesh = new Mesh();
                    newMesh.vertices = newVertices.ToArray();
                    newMesh.triangles = newTriangles.ToArray();
                    newMesh.RecalculateNormals();

                    // Создаем новый объект с новым мешем
                    GameObject newObject = new GameObject(meshFilter.gameObject.name + "_Cut");
                    MeshFilter newMeshFilter = newObject.AddComponent<MeshFilter>();
                    newMeshFilter.mesh = newMesh;
                    newObject.AddComponent<MeshRenderer>().material = meshFilter.GetComponent<MeshRenderer>().sharedMaterial;

                    // Устанавливаем позицию нового объекта
                    newObject.transform.position = meshFilter.transform.position;

                    // Сохраняем новый объект как префаб
                    string prefabPath = "Assets/Prefabs/" + newObject.name + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(newObject, prefabPath);
                    Debug.Log($"Сохранен новый префаб: {prefabPath}");

                    // Удаляем временный объект из сцены
                    DestroyImmediate(newObject);
                }
            }
        }

        // Сохраняем изменения в проекте
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(this);
        Debug.Log("Процесс завершен: невидимые меши удалены, новые меши созданы и сохранены как префабы!");
    }

    bool IsTriangleVisible(Vector3 v1, Vector3 v2, Vector3 v3, Plane[] planes)
    {
        foreach (Plane plane in planes)
        {
            if (plane.GetSide(v1) && plane.GetSide(v2) && plane.GetSide(v3))
            {
                return false; // Все три вершины находятся с одной стороны плоскости
            }
        }
        return true; // Треугольник видим
    }

    int AddVertex(Vector3 vertex, List<Vector3> newVertices)
    {
        int index = newVertices.IndexOf(vertex);
        if (index < 0)
        {
            newVertices.Add(vertex);
            return newVertices.Count - 1; // Возвращаем индекс нового вертекса
        }
        return index; // Возвращаем индекс существующего вертекса
    }
}
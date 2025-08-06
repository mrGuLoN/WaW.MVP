using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class LightDataTextureGenerator : MonoBehaviour
{
    public int maxLights = 10; // Максимальное количество источников света
    public float lightDistance = 5.0f; // Максимальное расстояние света

    // Текстуры для хранения данных о свете
    public Texture2D lightPositionsTex;
    public Texture2D lightColorsTex;
    public Texture2D rangeIntensityTex;

    public Transform lightsTR, player;
    // Данные о свете
    public Vector3[] lightPositions;
    public Color[] lightColors;
    public Vector2[] rangeIntensities;
    [ProButton]
    void ReadLight()
    {
        var lights = lightsTR.GetComponentsInChildren<Light>();
        maxLights = lights.Length;
        // Инициализируем массивы данных о свете
        lightPositions = new Vector3[maxLights];
        lightColors = new Color[maxLights];
        rangeIntensities = new Vector2[maxLights];

        // Заполняем массивы данными о свете
        for (int i = 0; i < Mathf.Min(lights.Length, maxLights); i++)
        {
            lightPositions[i] = lights[i].transform.position;
            lightColors[i] = lights[i].color;
            rangeIntensities[i] = new Vector2(lights[i].range, lights[i].intensity);
        }
        
        // Проверяем, что данные о свете инициализированы
        if (lightPositions == null || lightColors == null || rangeIntensities == null)
        {
            Debug.LogError("Light data not initialized!");
            return;
        }

        // Убедимся, что длина массивов совпадает
        if (lightPositions.Length != maxLights || lightColors.Length != maxLights || rangeIntensities.Length != maxLights)
        {
            Debug.LogError("Light data arrays have different lengths!");
            return;
        }

        // Создаем текстуры
        CreateTextures();

        // Заполняем текстуры данными о свете
        FillTextures();
    }

    void CreateTextures()
    {
        // Создаем текстуры размером maxLights x 1
        lightPositionsTex = new Texture2D(maxLights, 1, TextureFormat.RGBAFloat, false);
        lightColorsTex = new Texture2D(maxLights, 1, TextureFormat.RGBA32, false);
        rangeIntensityTex = new Texture2D(maxLights, 1, TextureFormat.RGBA32, false);
    }
    [ProButton]
    void SetTexture()
    {
    // Применяем текстуры к материалу шейдера
        Material material =player.GetComponent<Renderer>().material;
        material.SetTexture("_LightPositionsTex", lightPositionsTex);
        material.SetTexture("_LightColorsTex", lightColorsTex);
        material.SetTexture("_RangeIntensityTex", rangeIntensityTex);
    }

    void FillTextures()
    {
        for (int i = 0; i < maxLights; i++)
        {
            // Заполняем текстуру позиций света
            lightPositionsTex.SetPixel(i, 0, new Color(lightPositions[i].x, lightPositions[i].y, lightPositions[i].z, 0));

            // Заполняем текстуру цветов света
            lightColorsTex.SetPixel(i, 0, lightColors[i]);

            // Заполняем текстуру диапазонов и интенсивностей
            rangeIntensityTex.SetPixel(i, 0, new Color(rangeIntensities[i].x, rangeIntensities[i].y, 0, 0));
        }

        // Применяем изменения к текстурам
        lightPositionsTex.Apply();
        lightColorsTex.Apply();
        rangeIntensityTex.Apply();
    }
}
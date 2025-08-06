using UnityEngine;

public class CustomProbeGenerated : MonoBehaviour
{
    [SerializeField] private Texture2D _lightProbePositions;
    [SerializeField] private Texture2D _lightProbeColors;
    [SerializeField] private Texture2D[] _positionsTex;
    [SerializeField] private Texture2D[] _colorsTex;
    [SerializeField] private Vector3[] _positions;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    private void SetAllTextures()
    {
        _lightProbePositions = CombinePositionTexturesVertically(_positionsTex, _positions);
        _lightProbeColors = CombineColorsTexturesVertically(_colorsTex);
    }
    
    private Texture2D CombinePositionTexturesVertically(Texture2D[] textures, Vector3[] position)
    {
        // Вычисляем общую высоту и максимальную ширину
        int totalHeight = 0;
        int maxWidth = 0;

        foreach (var texture in textures)
        {
            totalHeight += texture.height;
            maxWidth = Mathf.Max(maxWidth, texture.width);
        }

        // Создаем новую текстуру для объединения
        Texture2D combinedTexture = new Texture2D(maxWidth, totalHeight);

        // Копируем пиксели из каждой текстуры в новую текстуру
        int currentY = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            var texture = textures[i];

            // Получаем пиксели текущей текстуры
            Color[] pixels = texture.GetPixels();

            // Применяем цвет из Vector3 к пикселям
            Color colorAdjustment = new Color(position[i].x, position[i].y, position[i].z);
            for (int j = 0; j < pixels.Length; j++)
            {
                pixels[j] += colorAdjustment; // Применяем цветовой эффект
            }

            // Устанавливаем пиксели в объединенной текстуре
            combinedTexture.SetPixels(0, currentY, texture.width, texture.height, pixels);
            currentY += texture.height; // Сдвигаем текущую позицию по Y
        }

        // Применяем изменения к объединенной текстуре
        combinedTexture.Apply();

        return combinedTexture;
    }
    private Texture2D CombineColorsTexturesVertically(Texture2D[] textures)
    {
        // Вычисляем общую высоту и максимальную ширину
        int totalHeight = 0;
        int maxWidth = 0;

        foreach (var texture in textures)
        {
            totalHeight += texture.height;
            maxWidth = Mathf.Max(maxWidth, texture.width);
        }

        // Создаем новую текстуру для объединения
        Texture2D combinedTexture = new Texture2D(maxWidth, totalHeight);

        // Копируем пиксели из каждой текстуры в новую текстуру
        int currentY = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            var texture = textures[i];

            // Получаем пиксели текущей текстуры
            Color[] pixels = texture.GetPixels();

            // Устанавливаем пиксели в объединенной текстуре
            combinedTexture.SetPixels(0, currentY, texture.width, texture.height, pixels);
            currentY += texture.height; // Сдвигаем текущую позицию по Y
        }

        // Применяем изменения к объединенной текстуре
        combinedTexture.Apply();

        return combinedTexture;
    }
    
}


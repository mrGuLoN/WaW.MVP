using System.Collections.Generic;
using com.cyborgAssets.inspectorButtonPro;
using UnityEngine;

public class LightReader : MonoBehaviour
{
    [SerializeField] private Vector3[] _positions;
    [SerializeField] private Vector2[] _rangeIten;
    [SerializeField] private Color[] _colors;
    [SerializeField] private float _maxDistance;
    [SerializeField] private Transform _stage;
    public MeshRenderer _player;
    private ComputeBuffer vectorBuffer, rangeIntesivityBuffer, colorsBuffer;

    private static readonly int VectorBuffer = Shader.PropertyToID("_LightPositions");
    private static readonly int RangeIntesivityBuffer = Shader.PropertyToID("_RangeIntesivityBuffer");
    private static readonly int LightColors = Shader.PropertyToID("_LightColors");
    private static readonly int MaxLights = Shader.PropertyToID("_MaxLights");
 

    [ProButton]
    public void ReadLights()
    {
        var lights = _stage.GetComponentsInChildren<Light>();
        if (lights.Length == 0)
        {
            Debug.LogWarning("No lights found in children.");
            return;
        }

        List<Vector3> pos = new List<Vector3>();
        List<Vector2> rng = new List<Vector2>();
        List<Color> col = new List<Color>();

        foreach (var l in lights)
        {
            pos.Add(l.transform.position);
            rng.Add(new Vector2(l.range, l.intensity));
            col.Add(l.color);
        }

        _positions = pos.ToArray();
        _rangeIten = rng.ToArray();
        _colors = col.ToArray();

        //if (vectorBuffer != null) vectorBuffer.Release();
        vectorBuffer = new ComputeBuffer(_positions.Length, sizeof(float) * 3);
        vectorBuffer.SetData(_positions);

        //if (rangeIntesivityBuffer != null) rangeIntesivityBuffer.Release();
        rangeIntesivityBuffer = new ComputeBuffer(_rangeIten.Length, sizeof(float) * 2);
        rangeIntesivityBuffer.SetData(_rangeIten);
        
        //if (colorsBuffer != null) rangeIntesivityBuffer.Release();
        colorsBuffer = new ComputeBuffer(_colors.Length, sizeof(float) * 4);
        colorsBuffer.SetData(_colors);
    }

    [ProButton]
    void SetMaterial()
    {
        Material material = _player.material;
        material.SetBuffer(VectorBuffer, vectorBuffer);
        material.SetBuffer(RangeIntesivityBuffer, rangeIntesivityBuffer);
        material.SetBuffer(LightColors, colorsBuffer);
        material.SetFloat(MaxLights, _positions.Length);
    }

    private void OnDestroy()
    {
        if (vectorBuffer != null)
        {
            vectorBuffer.Release();
            vectorBuffer = null; // Обнуляем ссылку для безопасности
        }

        if (rangeIntesivityBuffer != null)
        {
            rangeIntesivityBuffer.Release();
            rangeIntesivityBuffer = null; // Обнуляем ссылку для безопасности
        }
     
    }
}
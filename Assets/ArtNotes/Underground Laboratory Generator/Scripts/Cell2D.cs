using System;
using System.Collections.Generic;
using GameEngine;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Cell2D : MonoBehaviour
{
    [SerializeField] private EnemyRespawnData _enemyRespawnData;
    public EnemyRespawnData EnemyRespawnData => _enemyRespawnData;
    [SerializeField] private LayerMask _stagemask;
    [SerializeField] private int _id;
    public int Id => _id;
    public Light[] lights => _lights;
    [SerializeField] private Light[] _lights;
    [SerializeField] private Light[] _lightsBuff;
    [HideInInspector]
    public BoxCollider2D TriggerBox2D;
    public GameObject[] Exits;
    [HideInInspector]public bool isStartRoom;

    // Список соседних комнат
    public List<Cell2D> ConnectedCells = new List<Cell2D>();
    private ComputeBuffer vectorBuffer, rangeIntesivityBuffer, colorsBuffer;
    public bool isNotServerCell2D;
    
    private static readonly int VectorBuffer = Shader.PropertyToID("_LightPositions");
    private static readonly int RangeIntesivityBuffer = Shader.PropertyToID("_RangeIntesivityBuffer");
    private static readonly int LightColors = Shader.PropertyToID("_LightColors");
    private static readonly int MaxLights = Shader.PropertyToID("_MaxLights");
    // Метод для добавления соседней комнаты
    public void AddConnectedCell(Cell2D cell)
    {
        if (!ConnectedCells.Contains(cell))
        {
            ConnectedCells.Add(cell);
        }
    }
    public void FinalGenerate()
    {
        List<Light> conLight = new();
        if (isNotServerCell2D)
        {
            conLight.AddRange(lights);
            foreach (var exit in Exits)
            {
                RaycastHit2D hit = Physics2D.Raycast(exit.transform.position, exit.transform.up,3f,_stagemask);
                if (hit.collider)
                {
                    if (hit.transform.TryGetComponent<Cell2D>(out var C2D))
                    {
                        conLight.AddRange(C2D.lights);
                    }
                }
            }
        }
        foreach (var exit in Exits)
        {
            if (exit) Destroy(exit);
        }

        Exits = Array.Empty<GameObject>();
      
        List<Light> lightsTemp = new List<Light>();
        lightsTemp.AddRange(_lights);
        foreach (var c in ConnectedCells)
        {
            lightsTemp.AddRange(c.lights);
        }
        lightsTemp.AddRange(conLight);
        _lightsBuff = lightsTemp.ToArray();
        List<Vector3> pos = new List<Vector3>();
        List<Vector2> rng = new List<Vector2>();
        List<Color> col = new List<Color>();

        foreach (var l in lightsTemp)
        {
            pos.Add(l.transform.position);
            rng.Add(new Vector2(l.range, l.intensity));
            col.Add(l.color);
        }
       
        vectorBuffer = new ComputeBuffer(pos.Count, sizeof(float) * 3);
        vectorBuffer.SetData(pos);

        //if (rangeIntesivityBuffer != null) rangeIntesivityBuffer.Release();
        rangeIntesivityBuffer = new ComputeBuffer(rng.Count, sizeof(float) * 2);
        rangeIntesivityBuffer.SetData(rng);
        
        //if (colorsBuffer != null) rangeIntesivityBuffer.Release();
        colorsBuffer = new ComputeBuffer(col.Count, sizeof(float) * 4);
        colorsBuffer.SetData(col);
        TriggerBox2D.isTrigger = true;

        if (isStartRoom)
        {
            var players = FindObjectsByType<PlayerStateController>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                p.SetMaterialLightBuffer(vectorBuffer,rangeIntesivityBuffer,colorsBuffer,_lights.Length);
            }
        }
    }

    public void DestroyAllLights()
    {
        foreach (var l in _lights)
        {
            if (l)
            {
                Destroy(l.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ConnectedCells.Count <= 1)
        {
            
            Debug.Log("ENTER and exit");
            //return;
        }
        
        Debug.Log("ENTER");
        var mat = other.GetComponent<PlayerStateController>();
        if (mat)
        {
            mat.SetMaterialLightBuffer(vectorBuffer,rangeIntesivityBuffer,colorsBuffer,_lights.Length);
        }
        else
        {
            var emats = other.GetComponent<EnemyController>().allMaterials;
            foreach (var emat in emats)
            {
                emat.material.SetBuffer(VectorBuffer, vectorBuffer);
                emat.material.SetBuffer(RangeIntesivityBuffer, rangeIntesivityBuffer);
                emat.material.SetBuffer(LightColors, colorsBuffer);
                emat.material.SetFloat(MaxLights, _lights.Length);
                
            }
           
        }
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
        if (colorsBuffer != null)
        {
            colorsBuffer.Release();
            colorsBuffer = null; // Обнуляем ссылку для безопасности
        }
    }

    protected void Awake()
    {
        _lights = GetComponentsInChildren<Light>();
        TriggerBox2D = GetComponent<BoxCollider2D>();
    }
}
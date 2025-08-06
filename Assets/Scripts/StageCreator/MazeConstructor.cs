using UnityEngine;

public class MazeConstructor : MonoBehaviour
{
    //1
    public bool showDebug;
    public Vector2 sizeStep => _sizeStep;
    public Mesh mesh => _mesh;
    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;
    [SerializeField] private Vector2 _sizeStep;
    
    private MazeDataGenerator _dataGenerator;
    private MazeMeshGenerator _meshGenerator;
    private Mesh _mesh;

    //2
    public int[,] data
    {
        get; private set;
    }

    //3
    void Awake()
    {
        mazeMat1.color = Color.red;
        _dataGenerator = new MazeDataGenerator();
        _meshGenerator = new MazeMeshGenerator(_sizeStep);
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }
    
    public void GenerateNewMaze(int sizeRows, int sizeCols)
    {
        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
        {
            Debug.LogError("Odd numbers work better for dungeon size.");
        }

        data = _dataGenerator.FromDimensions(sizeRows, sizeCols);
        DisplayMaze();
    }
    
    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        _mesh = _meshGenerator.FromData(data);
        mf.mesh =_mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] {mazeMat1, mazeMat2};
    }
}
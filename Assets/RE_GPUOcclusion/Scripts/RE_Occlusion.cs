using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RE_Occlusion : MonoBehaviour
{
    ComputeBuffer[] counterBuffer = new ComputeBuffer[3];
    uint[] initialData;
    uint[] currentData;
    int currentBufferIndex = 0;
    class ObjectEntry
    {
        public GameObject gameObject;
        public MeshRenderer meshRenderer;
        public Mesh mesh;
        public Matrix4x4 matrix;
        public Bounds bounds;
        public MaterialPropertyBlock block;

        public GameObject boundingBox;
        public MeshRenderer boundingBoxRenderer;
        public Matrix4x4 boundingBoxMatrix;
        public bool isDynamic = false;
        //position in objectMatrixLists
        public int ListIndex;
        public int ElementIndex;
    };
    List<ObjectEntry> objectList = new List<ObjectEntry>();
    List<ObjectEntry> dynamicObjectList = new List<ObjectEntry>();

    Matrix4x4[] objectMatrices;
    List<Matrix4x4[]> objectMatrixLists = new List<Matrix4x4[]>();

    public Material occlusionMat;
    //public Material occlusionMatGLSL;
    //Material occlusionMat;

    public GameObject UnitCube;
    Mesh cube;
    public int visibleObjects = 0;
    public int totalObjects = 0;
    public List<GameObject> OccludeesStatic = new List<GameObject>();
    public List<GameObject> OccludeesDynamic = new List<GameObject>();
    public bool DebugBoxes = false;
    public bool HideObjects = false;
    public bool DisableVisibility = false;
    public float BoxScale = 1.01f;//Slightly bigger than 1 so it doesn't create flickers for flat objects (like walls)

    ComputeBuffer CreateComputeBuffer( int slots )
    {
        ComputeBuffer buffer = new ComputeBuffer( slots, 4, ComputeBufferType.Default );
        buffer.SetData(initialData);
        return buffer;
    }    

    Matrix4x4[] SplitArray( Matrix4x4[] Initial, int maxElements, out Matrix4x4[] Remaining )
    {
        if( Initial.Length > maxElements )
        {
            Matrix4x4[] Ret = new Matrix4x4[maxElements];
            int remainingCount = Initial.Length - maxElements;
            Remaining = null;
            if ( remainingCount > 0 )
                Remaining = new Matrix4x4[Initial.Length - maxElements];
            for( int i = 0; i < maxElements; i++ )
            {
                Ret[i] = Initial[i];
            }
            for( int i = maxElements; i < Initial.Length; i++ )
            {
                Remaining[i - maxElements] = Initial[i];
            }

            return Ret;
        }
        else
        {
            Remaining = null;
            return Initial;
        }
    }
    void SplitMatrixArray( int maxUnits )
    {
        Matrix4x4[] Remaining = objectMatrices;
        while( Remaining != null )
        {
            Matrix4x4[] array = SplitArray( Remaining, maxUnits, out Remaining );
            objectMatrixLists.Add( array );
        }
    }

    Vector3 GetAdequateScale( Vector3 scale )
    {
        float smallestdDepth = 0.001f;
        if( scale.x < smallestdDepth )
            scale.x = smallestdDepth;
        if( scale.y < smallestdDepth )
            scale.y = smallestdDepth;
        if( scale.z < smallestdDepth )
            scale.z = smallestdDepth;

        return scale;
    }
    // Use this for initialization
    void Start()
    {
        #if UNITY_2018_1_OR_NEWER
        if( !SystemInfo.supportsAsyncGPUReadback )
            useAsyncReadback = false;
        #else
            useAsyncReadback = false;
        #endif

        if ( !SystemInfo.supportsComputeShaders )
        {
            Debug.Log( "SystemInfo.supportsComputeShaders = null; GPUOcclusion needs SystemInfo.supportsComputeShaders !" );
            return;
        }

        //occlusionMat = occlusionMatDX;

        if ( UnitCube == null )
        {
            Debug.LogError( "Unit cube is not assign, please create a unit cube in scene and assign it as UnitCube" );
            return;
        }
        if( occlusionMat == null )
        {
            Debug.LogError( "occlusionMat is not assigned, please assign Asset OcclusionMat.mat" );
            return;
        }

        MeshRenderer cubeMeshRenderer = UnitCube.GetComponent<MeshRenderer>();
        cube = UnitCube.GetComponent<MeshFilter>().sharedMesh;
        UnitCube.SetActive(false);

        if( OccludeesStatic.Count == 0 && OccludeesDynamic.Count == 0 )
        {
            MeshRenderer[] localMeshRenderers = Object.FindObjectsOfType<MeshRenderer>();
            for( int u = 0; u < localMeshRenderers.Length; u++ )
            {
                ObjectEntry entry = new ObjectEntry();
                entry.meshRenderer = localMeshRenderers[u];

                if ( HideObjects )
                {
                    entry.meshRenderer.gameObject.SetActive( false );
                }

                Animation anim = localMeshRenderers[u].GetComponent<Animation>();
                Rigidbody body = localMeshRenderers[u].GetComponent<Rigidbody>();
                if ( ( anim!=null ) || (body != null && !body.isKinematic) )
                {
                    entry.isDynamic = true;//It's a dynamic physical object
                    dynamicObjectList.Add( entry );
                }

                objectList.Add( entry );
            }
        }
        else
        {
            for( int i = 0; i < OccludeesStatic.Count; i++ )
            {
                MeshRenderer[] localMeshRenderers = OccludeesStatic[i].GetComponentsInChildren<MeshRenderer>();
                for(int u = 0; u< localMeshRenderers.Length; u++)
                {
                    ObjectEntry entry = new ObjectEntry();
                    entry.meshRenderer = localMeshRenderers[u];

                    if( HideObjects )
                    {
                        entry.meshRenderer.gameObject.SetActive( false );
                    }

                    objectList.Add( entry );
                }
            }

            for( int i = 0; i < OccludeesDynamic.Count; i++ )
            {
                MeshRenderer[] localMeshRenderers = OccludeesDynamic[i].GetComponentsInChildren<MeshRenderer>();
                for( int u = 0; u < localMeshRenderers.Length; u++ )
                {
                    ObjectEntry entry = new ObjectEntry();
                    entry.meshRenderer = localMeshRenderers[u];
                    objectList.Add( entry );
                    dynamicObjectList.Add( entry );
                }
            }
        }

        GameObject BoundingBoxes = new GameObject( "RE_Occlusion_BoundingBoxes" );
        

        for( int i = 0; i < objectList.Count; i++ )
        {
            var meshRenderer = objectList[i].meshRenderer;
            ObjectEntry entry = objectList[i];

            if( !meshRenderer.enabled || ( meshRenderer.gameObject.isStatic && meshRenderer.isPartOfStaticBatch ) )
                continue;

            entry.gameObject = meshRenderer.gameObject;
            var mf = entry.gameObject.GetComponent<MeshFilter>();
            if( mf == null || mf.sharedMesh == null )
                continue;

            entry.mesh = mf.sharedMesh;
            entry.meshRenderer = meshRenderer;
            entry.bounds = meshRenderer.bounds;
            entry.matrix = Matrix4x4.TRS( entry.bounds.center, Quaternion.identity, entry.bounds.extents * 2 );
            entry.block = new MaterialPropertyBlock();
            entry.block.SetFloat( "_ObjectIndex", i );

            entry.boundingBox = new GameObject( "RE_Occlusion_BoundingBox" + i );
            entry.boundingBox.transform.SetParent( entry.gameObject.transform, false );
            entry.boundingBox.transform.localPosition = entry.mesh.bounds.center;
            
            entry.boundingBox.transform.localScale = GetAdequateScale( entry.mesh.bounds.size ) * BoxScale;

            //if( DebugBoxes )
                //entry.boundingBox.transform.SetParent( BoundingBoxes.transform, true );

            entry.boundingBoxRenderer = entry.boundingBox.AddComponent<MeshRenderer>();
            entry.boundingBoxRenderer.material = cubeMeshRenderer.material;
            MeshFilter bbMeshFilter = entry.boundingBox.AddComponent<MeshFilter>();
            bbMeshFilter.sharedMesh = cube;

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat( "_ObjectIndex", objectList.Count );
            entry.boundingBoxRenderer.SetPropertyBlock( block );
            entry.boundingBoxRenderer.shadowCastingMode = ShadowCastingMode.Off;
            entry.boundingBoxRenderer.receiveShadows = false;
            entry.boundingBoxRenderer.lightProbeUsage = LightProbeUsage.Off;
            entry.boundingBoxRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            entry.boundingBoxRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            entry.boundingBoxRenderer.allowOcclusionWhenDynamic = false;

            entry.boundingBoxMatrix = entry.boundingBoxRenderer.transform.localToWorldMatrix;

            //if( !DebugBoxes )
                entry.boundingBoxRenderer.enabled = false;            

            if( !entry.isDynamic )
            {
                Destroy( entry.boundingBox );
            }
        }

        initialData = new uint[objectList.Count];
        currentData = new uint[objectList.Count];
        objectMatrices = new Matrix4x4[objectList.Count];

        int MaxMatricesInList = 511;
        int ListIndex = 0;
        int ElementIndex = 0;
        for(int i=0; i < objectList.Count; i++)
        {
            ObjectEntry entry = objectList[i];
            objectMatrices[ i ] = entry.boundingBoxMatrix;

            entry.ListIndex = ListIndex;
            entry.ElementIndex = ElementIndex;

            ElementIndex++;
            if ( ElementIndex >= MaxMatricesInList )
            {
                ElementIndex = 0;
                ListIndex++;
            }
        }

        SplitMatrixArray( MaxMatricesInList );

        for (int i = 0; i < initialData.Length; i++)
            initialData[i] = 0;        

        for (int i = 0; i < counterBuffer.Length; i++)
        {
            counterBuffer[i] = CreateComputeBuffer( objectList.Count );
        }

        totalObjects = objectList.Count;        
    }
    int GetPreviousBufferIndex()
    {
        int offset = counterBuffer.Length - 1;
        int bufferIndex = currentBufferIndex - offset;
        if (bufferIndex < 0)
            bufferIndex = counterBuffer.Length + bufferIndex;
        return bufferIndex;
    }
    bool useAsyncReadback = true;
    // Update is called once per frame
    void UpdateVisibility( uint[] data )
    {
        if( DisableVisibility )
            return;

        visibleObjects = 0;
        
        UnityEngine.Profiling.Profiler.BeginSample( "RE_Occlusion_VisibilityUpdates" );
        for( int i = 0; i < objectList.Count; i++ )
        {
            var entry = objectList[i];
            if( !entry.meshRenderer )
                continue;

            uint val = 1;
            if ( data != null )
                val = data[i];
            if( val > 0 )
            {
                if( !entry.meshRenderer.enabled )
                    entry.meshRenderer.enabled = true;
                visibleObjects++;
            }
            else
            {
                if( entry.meshRenderer.enabled )
                    entry.meshRenderer.enabled = false;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
    private void OnDisable()
    {
        UpdateVisibility( null );
    }
    void Update()
    {
        ComputeBuffer prevBuffer = counterBuffer[GetPreviousBufferIndex()];

        if( useAsyncReadback )
        {
            #if UNITY_2018_1_OR_NEWER
            AsyncGPUReadback.Request( prevBuffer, ( AsyncGPUReadbackRequest req) =>
            {
                if( counterBuffer == null )
                    return;

                if( req.done )
                {
                    var nat = req.GetData<uint>();
                    nat.CopyTo( currentData );
                    if ( this.enabled)
                        UpdateVisibility( currentData );
                }
            } );
            #endif
        }
        else
            prevBuffer.GetData( currentData );

        if (!useAsyncReadback )
            UpdateVisibility( currentData );

        UnityEngine.Profiling.Profiler.BeginSample( "DynamicObjectUpdates" );

        for( int i = 0; i < dynamicObjectList.Count; i++ )
        {
            var entry = dynamicObjectList[i];
            objectMatrixLists[entry.ListIndex][entry.ElementIndex] = entry.boundingBoxRenderer.transform.localToWorldMatrix;
        }

        UnityEngine.Profiling.Profiler.EndSample();

        counterBuffer[currentBufferIndex].SetData( initialData );
        Graphics.SetRandomWriteTarget( 1, counterBuffer[ currentBufferIndex ] );

        int InstanceOffset = 0;
        for(int i=0; i< objectMatrixLists.Count; i++ )
        {
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetFloat( "_InstanceOffset", (float)InstanceOffset );
            block.SetInt( "_DebugBoxes", DebugBoxes ? 1 : 0 );

            //Graphics.DrawMeshInstanced( cube, 0, occlusionMat, objectMatrixLists[i], objectMatrixLists[i].Length, block, ShadowCastingMode.Off, false, 0, null, LightProbeUsage.Off );
            Graphics.DrawMeshInstanced( cube, 0, occlusionMat, objectMatrixLists[i], objectMatrixLists[i].Length, block, ShadowCastingMode.Off, false, 0, null );
            InstanceOffset += objectMatrixLists[i].Length;
        }

      

        currentBufferIndex = (currentBufferIndex + 1) % counterBuffer.Length;
    }
    
    private void OnDestroy()
    {
        for (int i = 0; i < counterBuffer.Length; i++)
        {
            if (counterBuffer[i] != null)
                counterBuffer[i].Release();
        }

        counterBuffer = null;
    }
}

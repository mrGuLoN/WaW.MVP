using System.Diagnostics.Contracts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public class AnimationToTexture : EditorWindow
{
    [MenuItem("CONTEXT/SkinnedMeshRenderer/Bake animation")]
    private static void Open(MenuCommand command)
    {
        var window = GetWindow<AnimationToTexture>();
        window._context = (SkinnedMeshRenderer)command.context;
    }

    private SkinnedMeshRenderer _context;
    private AnimationClip _clip;
    private static int _frameRate = 30;

    private void CreateGUI()
    {
        var frameCountField = new IntegerField("Clip")
        {
            value = _frameRate
        };
        frameCountField.RegisterValueChangedCallback(OnFrameRateChanged);

        var clipField = new UnityEditor.UIElements.ObjectField("Clip")
        {
            objectType = typeof(AnimationClip),
            allowSceneObjects = false
        };
        clipField.RegisterValueChangedCallback(OnClipChanged);
        
        rootVisualElement.Add(frameCountField);
        rootVisualElement.Add(clipField);
        rootVisualElement.Add(new Button(CreateAnimationTextures){text = "Bake Animation"});
    }

    private void CreateAnimationTextures()
    {
       Close();
       Contract.Assert(_clip != null, "Animation clip not defined");

       var duration = _clip.length;
       var frameCount = Mathf.Max((int)(duration * _frameRate),1);
       var vertexCount = _context.sharedMesh.vertexCount;

       var texture = new Texture2D(
           frameCount, 
           vertexCount*2, 
           TextureFormat.RGBAHalf,
           false,
           false);
       texture.wrapMode = TextureWrapMode.Clamp;

       var targetGameObject = _context.GetComponentInParent<Animator>();
       targetGameObject.applyRootMotion = false;
       BakeAnimation(targetGameObject.gameObject, frameCount, duration, texture);
       CreateTextureAsset(texture);
    }

    private void BakeAnimation(GameObject targetGameObject, int frameCount, float duration, Texture2D texture)
    {
        var mesh = new Mesh();

        var lastFrameIndex = frameCount - 1;
        for (int frameIndex = 0; frameIndex < frameCount; frameIndex++)
        {
            _clip.SampleAnimation(targetGameObject, (float)frameIndex / lastFrameIndex*duration);
            _context.BakeMesh(mesh);

            var vertices = mesh.vertices;
            var normals = mesh.normals;

            for (int i = 0; i < vertices.Length; i++)
            {
                var position = vertices[i];
                var normal = normals[i];
                var positionColor = new Color(position.x, position.y, position.z);
                var normalColor = new Color(normal.x, normal.y, normal.z);
                
                texture.SetPixel(frameIndex, i*2, positionColor);
                texture.SetPixel(frameIndex, i*2+1, normalColor);
            }
        }
        DestroyImmediate(mesh);
    }

    private void CreateTextureAsset(Texture2D texture)
    {
        var path = EditorUtility.SaveFilePanelInProject("Save Animation Texture", "Animation", "asset",
            "Select animation asset path");
        if (string.IsNullOrEmpty(path))
        {
            DestroyImmediate(texture);
            return;
        }
        
        AssetDatabase.CreateAsset(texture, path);
        AssetDatabase.Refresh();
    }

    private void OnFrameRateChanged(ChangeEvent<int> evt)
    {
        _frameRate = Mathf.Max(evt.newValue, 1, 60);
    }

    private void OnClipChanged(ChangeEvent<Object> evt)
    {
        _clip = evt.newValue as AnimationClip;
        
    }
}
#endif

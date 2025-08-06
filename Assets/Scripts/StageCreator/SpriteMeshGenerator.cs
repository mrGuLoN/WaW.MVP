using UnityEngine;

public class SpriteMeshGenerator : MonoBehaviour
{
   [SerializeField] private RenderTexture _renderTexture;

   public void SetCamera(Vector2 size, Vector2 scale)
   {
      var go = new GameObject();
      var cam2D = go.AddComponent<Camera>();
      cam2D.orthographic = true;
      var xpos = (size.x-1) * scale.x * 0.5f;
      var zpos = (size.y-1) * scale.y * 0.5f;
      go.transform.position = new Vector3(xpos, 5, zpos);
      go.transform.eulerAngles = new Vector3(90, 0, 0); 
      cam2D.orthographicSize = (size.x -1)/2-0.5f*scale.x;
      cam2D.targetTexture = _renderTexture;
      cam2D.clearFlags = CameraClearFlags.Color;
      
   }
   public Sprite GetSpriteWalls()
   {
      // Создаем Texture2D из RenderTexture
      var tex = new Texture2D(_renderTexture.width, _renderTexture.height, TextureFormat.RGBA32, false);
    
      // Устанавливаем RenderTexture как активный
      RenderTexture.active = _renderTexture;
    
      // Читаем пиксели из RenderTexture
      tex.ReadPixels(new Rect(0, 0, _renderTexture.width, _renderTexture.height), 0, 0);
      tex.Apply();
    
      // Возвращаем активный RenderTexture к null
      RenderTexture.active = null;

      // Создаем Sprite из Texture2D
      Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

      return sprite; // Возвращаем созданный Sprite
   }
}

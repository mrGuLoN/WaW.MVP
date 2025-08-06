using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(MazeConstructor))]   
public class StageController : MonoBehaviour
{
   private MazeConstructor _mazeConstructor;
   private SpriteMeshGenerator _spriteMeshGenerator;
   private SpriteRenderer _spriteRenderer;
   [SerializeField] private int _sizeLabirint;
   [SerializeField] private Sprite _sprite;

   private async void Start()
   {
      Vector2 sLaba = new Vector2(_sizeLabirint, _sizeLabirint);
      _mazeConstructor = GetComponent<MazeConstructor>();
      _spriteMeshGenerator = GetComponent<SpriteMeshGenerator>();
      _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
      _mazeConstructor.GenerateNewMaze((int)sLaba.x, (int)sLaba.y);
      _spriteMeshGenerator.SetCamera(sLaba, _mazeConstructor.sizeStep);
      await UniTask.Delay(5) ;
      _sprite = _spriteMeshGenerator.GetSpriteWalls();
      _spriteRenderer.sprite = _sprite;
      _spriteRenderer.gameObject.AddComponent<PolygonCollider2D>();
   }
}

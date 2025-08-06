using System;
using UnityEngine;

public class JustUpKey : MonoBehaviour
{
   [SerializeField] private BossCell2D bossCell;

   private void OnTriggerEnter2D(Collider2D other)
   {
      bossCell.laboratory2DGenerator.OpenNextBossRoom();
      Destroy(gameObject);
   }
}

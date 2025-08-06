using ArtNotes.UndergroundLaboratoryGenerator;
using UnityEngine;

public class BossCell2D : Cell2D
{
  public bool isClear
  {
    get => _isClear;
    set
    {
      _isClear = value;
     
      _doors.SetActive(!value); 
    }
  }
  private bool _isClear; 
  [SerializeField] private GameObject _doors;
  
  public Laboratory2DGenerator laboratory2DGenerator;

  public void QuestDone()
  {
    laboratory2DGenerator.OpenNextBossRoom();
  }
  
}

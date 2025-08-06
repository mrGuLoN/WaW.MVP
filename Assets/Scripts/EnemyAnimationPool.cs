using System;
using System.Collections.Generic;
using System.Linq;
using FSG.MeshAnimator.Snapshot;
using UnityEngine;

public class EnemyAnimationPool : MonoBehaviour
{
    public static EnemyAnimationPool instance = null;
    [SerializeField] private EnemyAnimationData[] _enemyAnimationDatas;
    
   
  
    void Start()
    {
        if (instance == null)
        {
            instance = this; 
        }
        else if (instance == this)
        {
            Destroy(gameObject); 
        }
       
        foreach (var en in _enemyAnimationDatas)
        {
            var animData = en;
            animData.idleAnimation = Instantiate(en.idleAnimation);
            for (int i = 0; i < animData.snapshotMeshAnimations.Length; i++)
            {
                animData.snapshotMeshAnimations[i] = Instantiate(en.snapshotMeshAnimations[i]);
            }
        }
    }
    public EnemyAnimationData GetAnimationData(int id)
    {
        return _enemyAnimationDatas.FirstOrDefault(x=>x.id == id);
    }
}



[Serializable]
public class EnemyAnimationData
{
    public int id;
    public SnapshotMeshAnimation idleAnimation;
    public SnapshotMeshAnimation[] snapshotMeshAnimations;
}

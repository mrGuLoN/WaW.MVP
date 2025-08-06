using System;
using DG.Tweening;
using UnityEngine;

public class KeyRandomRotation : MonoBehaviour
{
    private void Awake()
    {
        DG.Tweening.DOTween.Init();
    }

    void Start()
    {
        var seq = DG.Tweening.DOTween.Sequence();
        seq.Append(transform.DORotate(new Vector3(0, 0, -360), 1f))
            .SetLoops(-1, LoopType.Yoyo);
    }

   
}

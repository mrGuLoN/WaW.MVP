using System;
using ArtNotes.UndergroundLaboratoryGenerator;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Awater : MonoBehaviour
{
    [SerializeField] private Laboratory2DGenerator _laboratory2DGenerator;
    [SerializeField] private GameObject[] testedEnemy;
    private async void Start()
    {
        await UniTask.WaitUntil(() => _laboratory2DGenerator.stageCreatedonRPC == true);
        await UniTask.WaitForSeconds(5f);
        foreach (var e in testedEnemy)
        {
            e.SetActive(true);
        }
    }
}

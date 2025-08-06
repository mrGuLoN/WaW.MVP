using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Arena.Scripts.UI
{
    public class ItemCellView : MonoBehaviour
    {
       [SerializeField] private Image _image;
       [SerializeField] private TMP_Text _name,_description, _cost;
       [SerializeField] private Button _button;
       public Button Button => _button;

       public void SetData(Sprite image, string name, string description, int cost)
       {
           _image.sprite = image;
           _name.text = name;
           _description.text = description;
           _cost.text = cost + "$";
       }

       private void OnDestroy()
       {
           _button.onClick.RemoveAllListeners();
       }
    }
}

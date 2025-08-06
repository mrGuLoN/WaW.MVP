using TMPro;
using UnityEngine;

public class FPSChecker : MonoBehaviour
{
    private TMP_Text _text;
    private int _frame;
    private float _curtime;
    void Start()
    {
        _text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
       
            _text.text = (1f/Time.deltaTime).ToString("0") + "FPS " + Application.targetFrameRate + " target";
      
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class FPS : MonoBehaviour {

    Text TextC;
    RE_Occlusion occlusion;
	// Use this for initialization
	void Start ()
    {
        TextC = this.gameObject.GetComponent<Text>();
        Application.targetFrameRate = 0;

        occlusion = FindObjectOfType<RE_Occlusion>();

    }
    int Frames = 0;
    float TimePassed = 0;
	// Update is called once per frame
	void Update () {
        TimePassed += Time.deltaTime;
        if (TimePassed > 1.0f)
        {
            TextC.text = "FPS " + Frames + " at " + Screen.width + " x " + Screen.height + " " +
                "Visible " + occlusion.visibleObjects + " / " + occlusion.totalObjects;

            TimePassed = 0;
            Frames = 0;            
        }
        Frames++;	
	}
}

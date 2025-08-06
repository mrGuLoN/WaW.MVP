using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilerSave : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Application.targetFrameRate = -1;

        UnityEngine.Profiling.Profiler.logFile = "Profiler";
        UnityEngine.Profiling.Profiler.enableBinaryLog = true;
        UnityEngine.Profiling.Profiler.enabled = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SetSortMode : MonoBehaviour {

    Camera cam;
    public OpaqueSortMode opaueSortMode;
    public TransparencySortMode transparencySortMode;
    // Use this for initialization
    void Start () {
        cam = GetComponent<Camera>();
       
	}
	
	// Update is called once per frame
	void Update ()
    {
        if( cam == null )
            return;

        if( cam.opaqueSortMode != opaueSortMode )
            cam.opaqueSortMode = opaueSortMode;
        if( cam.transparencySortMode != transparencySortMode )
            cam.transparencySortMode = transparencySortMode;
    }
}

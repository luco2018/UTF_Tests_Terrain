using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableComponents2 : MonoBehaviour {

    FlyCam2 flyCamScript;

    // Use this for initialization
    void Start ()
    {
        flyCamScript = GetComponent<FlyCam2>();
    }
	
	// Update is called once per frame
	void Update ()
    {

        if (Input.GetMouseButtonDown(1))
        {
            flyCamScript.enabled = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            flyCamScript.enabled = false;
        }
        
    }

}


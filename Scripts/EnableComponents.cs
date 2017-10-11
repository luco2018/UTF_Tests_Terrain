using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableComponents : MonoBehaviour {

    FlyCam flyCamScript;

    // Use this for initialization
    void Start ()
    {
        flyCamScript = GetComponent<FlyCam>();
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


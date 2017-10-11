using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleLights2 : MonoBehaviour {

	public GameObject addLights;
    public GameObject initLight;
	public bool isActive = false;

	// Use this for initialization
	void Start () {
		
	}

	public void ToggleLights () 

	{
		
		if (isActive == false) {
			addLights.SetActive (true);
            initLight.SetActive(false);
			isActive = !isActive;

		} 
		else if  (isActive == true) 
		{
			addLights.SetActive (false);
            initLight.SetActive(true);
            isActive = !isActive;
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}

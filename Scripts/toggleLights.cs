using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleLights : MonoBehaviour {

	public GameObject addLights;
	public bool isActive = false;

	// Use this for initialization
	void Start () {
		
	}

	public void ToggleLights () 

	{
		
		if (isActive == false) {
			addLights.SetActive (true);
			isActive = !isActive;

		} 
		else if  (isActive == true) 
		{
			addLights.SetActive (false);
			isActive = !isActive;
		}
	}


	// Update is called once per frame
	void Update () {
		
	}
}

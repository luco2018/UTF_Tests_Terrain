using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class enableRotation : MonoBehaviour {
	 
	public Camera myCam;

	// Use this for initialization
	void Start () {


	}

	public void EnableRotation()
	{
		 
		if (myCam.GetComponent <rotateCamera> ().enabled == false)
		{
			myCam.GetComponent <rotateCamera> ().enabled = true;
		}

		else 
			
		{
			myCam.GetComponent <rotateCamera> ().enabled = false;
		}

	}


	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleInstancing : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public void ToggleInstancing()
	{
        var renderers = Object.FindObjectsOfType<Renderer>();
        bool instancingEnabled = renderers[0].sharedMaterial.enableInstancing;

        foreach (var obj in renderers)
        {
            obj.sharedMaterial.enableInstancing = !instancingEnabled;
        }


        /*foreach (var obj in Object.FindObjectsOfType<Renderer>())
		{
			//if (obj.material.enableInstancing == true) 
			//{
				//obj.material.enableInstancing = false;
			//} 
			//else 
			//{
				//obj.material.enableInstancing = true;
			//}
			obj.material.enableInstancing = !obj.material.enableInstancing;
		}*/
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

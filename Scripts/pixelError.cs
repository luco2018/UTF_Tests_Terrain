using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pixelError : MonoBehaviour {

    Terrain ter;
    public float pixelErrorTest;

	// Use this for initialization
	void Start () {
        ter = GetComponent<Terrain>();
	}
	
	// Update is called once per frame
	void Update () {

        pixelErrorTest = ter.heightmapPixelError;
	}
}

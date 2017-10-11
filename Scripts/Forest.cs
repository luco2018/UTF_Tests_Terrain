using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forest : MonoBehaviour {

    public GameObject tree;
    public float xDist;
    public float yDist;

    public float xSize;
    public float ySize;
	// Use this for initialization
	void Start () {
        for (float x=0;x<xSize;x+=xDist)
        {
            for (float z=0;z<ySize;z+=yDist)
            {
                float xp = x - (xSize / 2);
                float zp = z - (ySize / 2);
                GameObject t = Instantiate(tree);
                t.transform.position = new Vector3(xp, 0, zp);
            }
        }

	}
	
    	// Update is called once per frame
	void Update () {
		
	}

}

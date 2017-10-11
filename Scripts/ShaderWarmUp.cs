using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderWarmUp : MonoBehaviour {

    public ShaderVariantCollection shaderVariantCollection;

	// Use this for initialization
	void Start () {
        shaderVariantCollection.WarmUp();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tInst : MonoBehaviour {

	public GameObject prefab;
	public float gridX = 5f;
	public float gridY = 5f;
	public float spacing = 2f;
	//public Color color;
	public MaterialPropertyBlock props;
	//public Texture2D[] instTex;
	//public Texture instTex2;



	// Use this for initialization
	void Start () {
		MaterialPropertyBlock props = new MaterialPropertyBlock();
		for (int y = 0; y < gridY; y++) {
				for (int x = 0; x < gridX; x++) {
					Vector3 pos = new Vector3 (x, 0, y) * spacing;
					GameObject inst = Instantiate (prefab, pos, Quaternion.identity) as GameObject;
					float colorVarianceR = Random.Range(0.5f, 1.0f);
					float colorVarianceG = Random.Range(0.5f, 1.0f);
					float colorVarianceB = Random.Range(0.5f, 1.0f);
					props.SetColor ("_Color", new Color(colorVarianceR, colorVarianceG, colorVarianceB));
					MeshRenderer renderer = inst.GetComponent<MeshRenderer> ();
				    //Texture instTexture = instTex[Random.Range(0, instTex.Length)];	
				    //props.SetTexture ("_MainTex", instTexture); 
				    renderer.SetPropertyBlock (props);
					props.Clear();
				    
				}

			}
		}	



// Update is called once per frame 

	void Update () {
		
	}
}


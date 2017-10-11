using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testInstantiatorTree2 : MonoBehaviour {

	public GameObject prefab;
   // public GameObject prefab1;
   // public GameObject prefab2;

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
					Vector3 pos = new Vector3 (x-6, y, 10) * spacing;
                    //Vector3 pos2 = new Vector3(x - 7, y, 10) * spacing;
                  //  Vector3 pos3 = new Vector3(x, y, 10) * spacing;
                    GameObject inst = Instantiate (prefab, pos, Quaternion.identity) as GameObject;
                   // GameObject inst2 = Instantiate(prefab, pos2, Quaternion.identity) as GameObject;
                   // GameObject inst3 = Instantiate(prefab, pos3, Quaternion.identity) as GameObject;
                   // float colorVarianceR = Random.Range(0.5f, 1.0f);
					//float colorVarianceG = Random.Range(0.5f, 1.0f);
					//float colorVarianceB = Random.Range(0.5f, 1.0f);
					//props.SetColor ("_Color", new Color(colorVarianceR, colorVarianceG, colorVarianceB));
					//MeshRenderer renderer = inst.GetComponent<MeshRenderer> ();
                    //MeshRenderer renderer2 = inst2.GetComponent<MeshRenderer>();
                   // MeshRenderer renderer3 = inst3.GetComponent<MeshRenderer>();

                    //Texture instTexture = instTex[Random.Range(0, instTex.Length)];	
                    // props.SetTexture ("_MainTex", instTexture); 
                   // renderer.SetPropertyBlock (props);
                   // renderer2.SetPropertyBlock(props);
                    //renderer3.SetPropertyBlock(props);
                   // props.Clear();
				    
				}

			}
		}	



// Update is called once per frame 

	void Update () {
		
	}
}


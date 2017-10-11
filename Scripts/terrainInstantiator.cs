using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainInstantiator : MonoBehaviour
{
	public GameObject prefab;
	public float posX;
	//public float posY;
	public float posZ;
	public float distance = 1;

		
	void Start()
	{
		for (int i = 0; i < 10; i++) {
			Vector3 position = new Vector3(posX, 0, posZ);
			Instantiate (prefab, position, Quaternion.identity);
			posX = (posX + distance);
			posZ = (posZ + distance);
		}
	}

	void Update() 
	{
		
	}
}

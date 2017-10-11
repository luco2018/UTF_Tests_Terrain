using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class terrainCreator : MonoBehaviour {

	public TerrainData terrainData;
	public float worldSizeX = 100;
	public float worldSizeY = 100;
	public float worldSizeZ = 100;
	//public int heightRes;
	//public TreeInstance[] treeInstances;

	// Use this for initialization
	void Start () {

		terrainData.size = new Vector3(worldSizeX, worldSizeY, worldSizeZ);
		//terrainData.heightmapResolution = heightRes;
		GameObject ter = Terrain.CreateTerrainGameObject(terrainData);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

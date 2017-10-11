﻿using UnityEngine;
using System.Collections;

public class randomizeHeights : MonoBehaviour {

	Terrain  terrain; 
	TerrainData tData;

	public int xRes = 500;
	public int yRes = 500;

	public float strength;
	public float coef;

	float[,] heights;

	void Start () { 
		terrain = transform.GetComponent<Terrain>(); 
		tData = terrain.terrainData;

		xRes = tData.heightmapWidth;
		yRes = tData.heightmapHeight;
	}

	void OnGUI() {
		if(GUI.Button (new Rect (10, 10, 100, 25), "Wrinkle")) {
			randomizePoints(0.1f);
		}

		if(GUI.Button (new Rect (10, 40, 100, 25), "Reset")) {
			resetPoints();
		} 
	}

	void randomizePoints(float strength) { 
		heights = tData.GetHeights(0, 0, xRes, yRes);

		for (int y = 0; y < yRes; y++) {
			for (int x = 0; x < xRes; x++) {
				heights[x,y] = Random.Range(0.0f, strength) * coef;
			}
		}

		tData.SetHeights(0, 0, heights);
	}

	void resetPoints() { var heights = tData.GetHeights(0, 0, xRes, yRes);
		for (int y = 0; y < yRes; y++) {
			for (int x = 0; x < xRes; x++) {
				heights[x,y] = 0;
			}
		}

		tData.SetHeights(0, 0, heights);
	} 

	// Update is called once per frame
	void Update () {

	}
}
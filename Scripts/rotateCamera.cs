using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateCamera : MonoBehaviour {

	public GameObject target;
	Vector3 point;
	public float rotateSpeed;

	// Use this for initialization
	void Start () {
		point = target.transform.position;
		transform.LookAt(point);
	}

	public void RotateCamera() 
	{
		transform.LookAt(point);
		transform.RotateAround (point, Vector3.up, rotateSpeed * Time.deltaTime);;
	}
	// Update is called once per frame
	void Update () {
		
		RotateCamera();
	}
}

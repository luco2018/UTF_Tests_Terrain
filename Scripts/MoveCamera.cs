using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

    float moveSpeed = -40;
    int bounceCount = 0;
    float timeBeforeStart = 2;      // wait time before moving.. Allows for screen to display on PS4
    // Update is called once per frame
    void Update() {

        if (timeBeforeStart>0)
        {
            timeBeforeStart -= Time.deltaTime;
            return;
        }
        if (bounceCount < 2)
        {
            Vector3 pos = transform.position;
            pos.x += Time.deltaTime * moveSpeed;
            if (pos.x < -160 || pos.x > 0)
            {
                moveSpeed = -moveSpeed;
                bounceCount++;
            }
            transform.position = pos;
        }
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour {

    public static SceneController Instance; 

    int curSceneIndex = 0;

	// Use this for initialization
	void Start () {

       // SceneManager.LoadScene(curSceneIndex);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
        {
            Destroy(gameObject);
        }

	}

    public void LoadPrevLevel()
    {
        curSceneIndex--;
        SceneManager.LoadScene(curSceneIndex);
        Debug.Log("index = " + curSceneIndex);
    }

    public void LoadNextLevel()
    {
        curSceneIndex++;
        SceneManager.LoadScene(curSceneIndex);
        Debug.Log("index = " + curSceneIndex);
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp(KeyCode.RightArrow) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            LoadPrevLevel();
        }
    }
}

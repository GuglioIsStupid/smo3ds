﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class scr_loadScene : MonoBehaviour {
	
	[HideInInspector] public static scr_loadScene s;
	[HideInInspector] public string nextScene = "scn_menuTitle";
	[HideInInspector] public bool isDone = false;
	AsyncOperation loadOP;

    //List<GameObject> rootObjects = new List<GameObject>();

    void Start(){ s = this; }
	public void StartScene(string sceneName, int transition = 0){
		isDone = false;
		string currentscn = SceneManager.GetActiveScene ().name;

		//Scene scene = SceneManager.GetActiveScene();

		//GetComponent<AudioListener>().enabled = true;
		scr_main.s.hasLevelLoaded = false;
		nextScene = sceneName;
		scr_main.s.dbg_enemyCount = 0;

        scr_main.DPrint ("nSCN: " + nextScene);
		switch (transition) {
		case 0: //direct, no transition
			SceneManager.LoadScene (sceneName);
			break;
		case 1: //flying ship line
			scr_main.s.SetFocus(false);
			SceneManager.LoadScene ("scn_loadShip");
			nextScene = sceneName;
				//wip
			break;
		case 2://cap fly transition
			scr_main.s.transform.GetChild (1).GetChild (2).gameObject.SetActive (true);
            break;
		case 3: //async
            StartCoroutine (loadAsync ());
			break;
		}
	}
	IEnumerator loadAsync(){
		loadOP = SceneManager.LoadSceneAsync (nextScene, LoadSceneMode.Additive);
		loadOP.allowSceneActivation = false;
		while (!loadOP.isDone) {
			scr_main.DPrint ("loading: " + (loadOP.progress*100) + "%", false);
			yield return null;
		}
		scr_main.DPrint ("loading: 100%");

        // Makes sure that the scene is completely empty just in case the scene does not get unloaded at all
        /*for (int i = 0; i < rootObjects.Count; ++i)
        {
            print(rootObjects[i]);
            if (rootObjects[i].name != "objFader")
                Destroy(rootObjects[i]);
        }*/
        isDone = true;
	}
	public void SetSceneActive(){
		loadOP.allowSceneActivation = true;
	}
}

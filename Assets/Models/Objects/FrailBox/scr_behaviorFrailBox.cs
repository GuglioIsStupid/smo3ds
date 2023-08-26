﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scr_behaviorFrailBox : MonoBehaviour {

	Animator anim;
	int currentState;
	int hitCount;
	int hitMax = 14;
	void Start () {
		anim = GetComponent<Animator> ();
	}
	public void OnTouch(int num){
		if (num == 1) {
			hitCount++;
			if (hitCount == hitMax) toBreak ();
		}
	}
	void toBreak(){
		hitCount = 0;
		anim.CrossFade ("damage", 0.1f);
		switch (currentState) {
		case 1://1
			transform.GetChild (0).gameObject.SetActive (false);
			transform.GetChild (1).gameObject.SetActive (true);
			transform.GetChild (2).gameObject.SetActive (true);
			break;
		case 2://2
			transform.GetChild (1).gameObject.SetActive (false);
			hitCount = hitMax-2;
			break;
		case 3:
			GameObject.Instantiate (Resources.Load<GameObject> ("Objects/objFrailBoxTrace"), transform.position, transform.rotation);
			Destroy (gameObject);
			break;
		}
		currentState++;
	}
}

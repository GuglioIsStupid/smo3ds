﻿#define isRelease // UNCOMMENT FOR RELEASING TO THE PUBLIC!

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scr_main : MonoBehaviour {

	// Initialization script for Super Mario Odyssey for 3ds, made by Team Alpha.
	public string version = "0.4";

	//constants
	[HideInInspector] public static scr_main s;
	[HideInInspector] public static GUIStyle stl_debug; //style for debug menu
	[HideInInspector] public static LayerMask lyr_def;
	[HideInInspector] public static LayerMask lyr_player;
	//modifiable
	[HideInInspector] public int dbg_enemyCount = 0; 
	[HideInInspector] public int coinsCount = 0;
	[HideInInspector] public int moonsCount = 0;
	[HideInInspector] public int nextSpawn = -1; // next index of spawn point
	[HideInInspector] public string capMountPoint = "missingno"; //used by cap
	[HideInInspector] public bool hasLevelLoaded = false; //used by level loading and data.
	
	public void SetFocus(bool boolean){
		Time.timeScale=boolean?1:0;
	}
	public void SetLOD(GameObject coll, bool state)
	{
		if (coll.GetComponent<paramObj>() != null && coll.GetComponent<paramObj>().isLOD)
		{
			if (coll.transform.GetChild(1).gameObject.name == "Mesh") coll.transform.GetChild(1).gameObject.SetActive(state);
			else { Debug.Log("E: INVALID MESH TREE AT " + coll.name); return; }
			if (coll.GetComponent<Animator>() != null) coll.GetComponent<Animator>().enabled = state;
			if (coll.GetComponent<AudioSource>() != null)
			{
				coll.GetComponent<AudioSource>().enabled = state;
			}
			if (coll.GetComponent<Rigidbody>() != null)
			{
				if (state) coll.GetComponent<Rigidbody>().WakeUp();
				else coll.GetComponent<Rigidbody>().Sleep();
			}
		}
	}

	void Awake(){
		if(s == null)
		{
			Debug.ClearDeveloperConsole ();
			scr_main.DPrint("INITIALIZE SUPER MARIO ODYSSEY - 3DS DEMAKE");
			DontDestroyOnLoad(gameObject);
			s = this;
			Init ();

			return;
		}
		Destroy(gameObject);
	}
	
	void Init () {
		Application.targetFrameRate = 30;
		QualitySettings.vSyncCount = 0;
		//UnityEngine.N3DS.HomeButton.Enable ();

		stl_debug = new GUIStyle();
		stl_debug.normal = new GUIStyleState();
		stl_debug.normal.textColor = Color.white;

		lyr_def= LayerMask.NameToLayer ("Default");
		lyr_player = LayerMask.NameToLayer ("Player");

		string authorVersion;
		#if isRelease
			authorVersion = "SMO3DS a" + version; //TODO: maybe make this some compiling #if, to save resources at runtime?
		#else
			authorVersion = "SMO3DS pre-a" + version;
		#endif
		transform.GetChild (1).GetChild (0).GetComponent<Text> ().text = authorVersion;
	}

	#if isRelease
	public static void DPrint(string text, bool isEditorOut = true){ }
	#else
	public static void DPrint(string text, bool isEditorOut = true){
		if(scr_devMenu.txt_cmdOut != null) scr_devMenu.txt_cmdOut = text;
		if(isEditorOut && text != "") Debug.Log (text);
	}
	#endif
}

﻿using UnityEngine;
using System.Collections;
using System;

public enum MarioState
{
	Ground,
	Jumping,
	Landing,
	CappyCatch,
	Squat,
	GroundPound,
	WallJump
}

public class MarioController : MonoBehaviour
{
	public static MarioState myState;
	public int mySubState = 0;
	public int maxJump = 6;

	public float jumpForce = 2.5f;
	public float moveSpeed = 5.0f;

	[HideInInspector] public bool isGrounded = true;
	[HideInInspector] public bool isMoving = false;
	[HideInInspector] public bool wasMoving = false;
	[HideInInspector] public bool lockJump = false;
	[HideInInspector] public bool isFixWalk = false; // fix run animation

	[HideInInspector] public Animator anim;
	[HideInInspector] public Rigidbody rb;

	[HideInInspector] private bool key_jump = false;
	[HideInInspector] bool key_squat = false;
    
	private bool bvar0 = false;
	private float hackFlyLength = 0.5f;
	private float hackFlyStartTime;

	private float h = 0;
	private float v = 0;
	private float currentMoveSpeed = 0;
	private float jumpVelocity = 0;

	[HideInInspector] public scr_behaviorMarioCap cappy;
	[HideInInspector] public static MarioController marioObject;

	[HideInInspector] public Vector3 moveAdditional = Vector3.zero;
	[HideInInspector] public float groundedPosition = 0; // latest floor position
	private float walkRotation = 0f; // stores the walk rotation offset or something
	[HideInInspector] public bool hasCaptured = false;
	private bool isCapturing = false;
	private RaycastHit hit;
	//private float fvar0 = 0; // temporary var
	public bool isBlocked = false;
	[HideInInspector] public bool isHacking = false; // hack = modify/take control of object
	public bool isBlockBlocked = false; // to prevent it from setting block to false, if it handles multiple blocks...
	[HideInInspector] public bool plsUnhack = false;
	[HideInInspector] public string animLast = "idle";
	bool hasJumped= false;
	int jumpAfterTimer = 0; //timer till it refuses to execute double jump
	int jumpType = 0;
	bool hasTouchedCeiling = false;
	float lastGroundedPosition = 0;

    float slidingForce = 375f;

    CapsuleCollider capsColl1;
    public CapsuleCollider capsColl2;

    void Awake()
	{
		// Disable AudioListener for the global values
		scr_gameInit.globalValues.GetComponent<AudioListener>().enabled = false;

		// Enable AudioListener for Mario's object
		GetComponent<AudioListener>().enabled = true;

		// Store references to Animator and Rigidbody components
		anim = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();
        capsColl1 = GetComponent<CapsuleCollider>();
        marioObject = this;
	}

	void Update()
	{
		if (scr_gameInit.globalValues.isFocused)
		{

			HandleInput ();

			if(!isBlocked) HandleMove ();

			HandleHack ();

			// Update Mario's animation and movement based on states
			switch (myState) {
			case MarioState.Ground: // Standing still, wait
                    capsColl1.center = new Vector3(0, 0.8f, 0);
                    capsColl1.height = 1.6f;

                    capsColl2.center = new Vector3(0, 0.8f, 0);
                    capsColl2.height = 1.6f; 

                    moveSpeed = 8f;
                if (jumpAfterTimer > 0) {//maximal
					if(jumpType > 2)/*triple jump WIP*/ jumpAfterTimer = 11;
					if (jumpAfterTimer > 8) {
						jumpAfterTimer = 0;
						jumpType = 0;
					}
					jumpAfterTimer++;
				}
				break;

			case MarioState.Jumping: // Jumping from land normal
				float jumpedHeight = transform.position.y - lastGroundedPosition;
				switch(mySubState){
				case 0:
					rb.AddForce (Vector3.up * (jumpForce + (jumpType/3)) * 2000 * Time.deltaTime, ForceMode.Impulse); //start accelerating up
					mySubState++;
					jumpAfterTimer = 1;
					break;
				case 1:
					if ((key_jump && jumpedHeight > maxJump + jumpType && jumpType != 3 || hasTouchedCeiling) //if condition(reached top), stop accelerating and start falling
						|| (!key_jump && jumpedHeight > jumpType-1 && jumpType != 3) 
						|| (jumpedHeight > maxJump + jumpType && jumpType == 3)) { //TODO: more efficient...
						rb.AddForce (Vector3.down * jumpForce * 200 * Time.deltaTime, ForceMode.Impulse);
						if (hasTouchedCeiling)
							jumpAfterTimer = 0;
					}
					if (transform.position.y - groundedPosition > 0.1f || hasTouchedCeiling)
						hasJumped = true;//he has jumped, so isGrounded wont get in the way when starting to jump.
					else if (isGrounded && hasJumped) {
						hasJumped = false;
						SetState (MarioState.Landing);
					}
					break;
				}
			    break;


			case MarioState.Landing: //land
				SetState (MarioState.Ground, 1);
				break;

			case MarioState.CappyCatch: //flying to capture enemy
				// Calculate the current percentage of the journey completed
				float distanceCovered = (Time.time - hackFlyStartTime) * hackFlyLength / 1;
				float journeyFraction = distanceCovered / hackFlyLength;

				// Calculate the current position along the Bezier curve
				Vector3 targetPosition = Bezier (transform.position, cappy.capturedObject.transform.position + Vector3.up * 3 + (transform.position - cappy.capturedObject.transform.position).normalized * 1, cappy.capturedObject.transform.position, journeyFraction);

				// Calculate the additional movement vector based on the target position
				moveAdditional = targetPosition - transform.position;

				// Check if the movement is completed
				if (Time.time - hackFlyStartTime >= 1)
				{
					// Ensure the object ends up at the final position
					transform.position = cappy.capturedObject.transform.position;
					SetState(MarioState.Ground);
					if (!isBlockBlocked)
						isBlocked = false;
					isCapturing = false;
					hasCaptured = true;
					cappy.capturedObject.SendMessage("setState", 6);
					for (int i = 0; i <= 8; i++)
						transform.GetChild(i).gameObject.SetActive(false);
					rb.useGravity = true;
					moveAdditional = Vector3.zero;
				}
				break;
                case MarioState.Squat:
                    capsColl1.center = new Vector3(0, 0.47f, 0);
                    capsColl1.height = 0.94f;

                    capsColl2.center = new Vector3(0, 0.47f, 0);
                    capsColl2.height = 0.94f;

                    moveSpeed = 1.5f;
                    if (slidingForce > 0f && currentMoveSpeed > 0.0001f)
                    {
                        rb.AddForce(transform.rotation * Vector3.forward * slidingForce, ForceMode.Impulse);
                        slidingForce -= Time.deltaTime * 250f;
                    }
                    if (!key_squat)
                    {
                        SetState(MarioState.Ground);
                    }
                    break;
			}
		}
		hasTouchedCeiling = false;
		isGrounded = false;

		wasMoving = isMoving;
		// Calculate the movement vector based on the input and current speed
		Vector3 movementVector = Vector3.forward * currentMoveSpeed * Time.deltaTime;
        
        // Move the character using the Rigidbody
		rb.MovePosition ((rb.position + (transform.rotation * Vector3.forward) * movementVector.magnitude) + Vector3.up * jumpVelocity + moveAdditional);
    }

    void HandleInput(){
        
		#if UNITY_EDITOR
		h = Input.GetAxisRaw("Horizontal");
		v = Input.GetAxisRaw("Vertical");

		key_jump = Input.GetKey(KeyCode.Space);
        key_squat = Input.GetKey(KeyCode.LeftControl);
#else
		h = UnityEngine.N3DS.GamePad.CirclePad.x;
		v = UnityEngine.N3DS.GamePad.CirclePad.y;

		key_jump = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.A) || UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.B);
        key_squat = UnityEngine.N3DS.GamePad.GetButtonHold(N3dsButton.L);
#endif

        // Check if Mario is blocked
        if (isBlocked) {
			h = 0;
			v = 0;
			isMoving = false;
		} else {
			isMoving = h != 0 || v != 0;
		}

		switch (myState) {
		    case MarioState.Ground:
			    if (key_jump) {
			    	if (!lockJump) {
			    		lockJump = true;
			    		SetState(MarioState.Jumping);
			    	}
			    } else if (lockJump)
				    lockJump = false;

                if (key_squat)
                {
                    SetState(MarioState.Squat);
                }
			    break;
		}
	}

	public void SetState(MarioState state, int subState = 0)
	{
		myState = state;
		mySubState = subState;
		switch (state)
		{
		    case MarioState.Ground:
			    switch (subState) {
			    case 0:
			    	SetAnim ("idle");
			    	break;
			    }
			    if (isMoving)
			    	isFixWalk = true;
			    break;
                
		case MarioState.Jumping:
			lastGroundedPosition = groundedPosition;
			jumpType++;
			switch (jumpType) {
			case 1:
				SetAnim ("jump");
				break;
			case 2:
				SetAnim ("jump2");
				break;
			case 3:
				SetAnim ("jump3");
				break;
			default:
				SetAnim ("jump");
				jumpType = 1;
				break;
			}
			break;

		    case MarioState.Landing:
			    SetAnim ("land", 0.1f, 1, false);
			    break;

		    case MarioState.CappyCatch: 
			    hackFlyStartTime = Time.time;
			    SetAnim ("captureFly");
			    rb.useGravity = false;
			    break;
            case MarioState.Squat:
                SetAnim("squatStart");
                break;
		}

	}


	void HandleMove(){
		GetComponent<Animator>().SetFloat("Speed", currentMoveSpeed);
		if (isMoving) {

			if ((!wasMoving || isFixWalk) && isGrounded && !key_squat) {
				isFixWalk = false;
			} else if ((!wasMoving || isFixWalk) && isGrounded && key_squat)
            {
                isFixWalk = false;
                SetAnim("squatWalk");
            }

                float tmp_walkRotation = 0;
			if (transform.rotation.y < 179 && transform.rotation.y > -179) {

				walkRotation += tmp_walkRotation / 76;
				tmp_walkRotation = Mathf.Atan2 (h, v) * Mathf.Rad2Deg;
			} else {
				tmp_walkRotation = 0;
			}
			transform.rotation = Quaternion.Euler (transform.eulerAngles.x, tmp_walkRotation + walkRotation + MarioCam.marioCamera.gameObject.transform.eulerAngles.y, transform.eulerAngles.z);

			if (currentMoveSpeed < moveSpeed) {
				currentMoveSpeed += 0.5f;
			}

            if (currentMoveSpeed > moveSpeed)
                currentMoveSpeed = moveSpeed;

		} else {
			if (currentMoveSpeed > 0) currentMoveSpeed = 0;
            if (wasMoving && isGrounded && !key_squat)
                SetAnim("idle");
            else if (wasMoving && isGrounded && key_squat)
                SetAnim("squatWait");
		}

	}

	void HandleHack(){
		if (cappy != null) {
			if (plsUnhack) {
				plsUnhack = false;
			} else {
				if (cappy.currentState == 3)
				if (UnityEngine.N3DS.GamePad.GetButtonHold (N3dsButton.Y) || UnityEngine.N3DS.GamePad.GetButtonHold (N3dsButton.X) || Input.GetKey (KeyCode.LeftShift)) {
					cappy.SetState (0); //cappy handles everything.
				}
			}
		}
		if (isHacking) {
			if (hasCaptured) {
				if (UnityEngine.N3DS.GamePad.GetButtonHold (N3dsButton.L) || UnityEngine.N3DS.GamePad.GetButtonHold (N3dsButton.R)
					|| Input.GetKey (KeyCode.Y) || plsUnhack) {
					plsUnhack = false;
					transform.GetChild (2).gameObject.SetActive (false);//hair
					transform.GetChild (1).gameObject.SetActive (true);//cap
					SetState (MarioState.Ground);
					cappy.capturedObject.SendMessage ("setState", 7);
					cappy.capturedObject.tag = "Untagged";
					if (cappy.capturedObject.GetComponent<Collider> () != null)
						cappy.capturedObject.GetComponent<Collider> ().enabled = true;
					transform.Translate (0, 0, -2);
					ResetSpeed ();
					isBlocked = false;
					isBlockBlocked = false;
					isHacking = false;
					scr_gameInit.globalValues.capMountPoint = "missingno";
					var Mustache = cappy.capturedObject.transform.GetChild (0);
					if (Mustache.name == "Mustache" || Mustache.name == "Mustache__HairMT")
						Mustache.gameObject.SetActive (false); //if mustache, place it at index 0
				}
			} else {
				if (!isCapturing) {
					isCapturing = true;
					isBlocked = true;
					SetState (MarioState.CappyCatch);
				}
			}
		} else {
			if (hasCaptured == true) {
				hasCaptured = false;
				cappy.SetState (2);
				for (int i = 0; i <= 8; i++) {
					transform.GetChild (i).gameObject.SetActive (true);
				}
				transform.GetChild (2).gameObject.SetActive (true); // hair
				transform.GetChild (1).gameObject.SetActive (false); // cap
			}
		}
	}

	void OnTriggerEnter(Collider collis)
	{
		try
		{
			if (collis.gameObject.layer != scr_gameInit.lyr_def)
			if (collis.gameObject.layer == scr_gameInit.lyr_enemy || collis.gameObject.layer == scr_gameInit.lyr_obj)
			{
				if(collis.GetComponent<paramObj>() != null) if(!collis.GetComponent<paramObj>().isTouch) return;

				if(hasTouchedCeiling){
					collis.gameObject.SendMessage("OnTouch", 4);
					return;
				}
				if (transform.position.y < collis.GetComponent<paramObj>().bCenterY())
					collis.gameObject.SendMessage("OnTouch", 2);
				else if(!hasTouchedCeiling)
					collis.gameObject.SendMessage("OnTouch", 3);
			}
		}
		catch (Exception e)
		{
			Debug.Log(e.Message);
		}
	}
	void OnSensorTopEnter(Collider col){
		hasTouchedCeiling = true;
	}
	void OnSensorBottomStay(Collider col){
		if(col.gameObject.layer == 0){
			isGrounded = true;
			groundedPosition = transform.position.y;
		}
	}
	public void ResetSpeed()
	{
		maxJump = 3;
		moveSpeed = 8f;
	}

	public void SetSpeed(int _maxJump, float _moveSpeed, float scaleCap = 1)
	{
		maxJump = _maxJump;
		moveSpeed = _moveSpeed;
		cappy.transform.localScale = new Vector3 (scaleCap, scaleCap, scaleCap);
	}

	public void SetAnim(string animName, float transitionTime = 0, float animSpeed = 1, bool isInstant = true)
	{
		if (isAnim (animName)) {
			if(isInstant) anim.Play(animName);
			else anim.CrossFade (animName, transitionTime);
			anim.speed = animSpeed;
			animLast = animName;
		}
	}

	public bool isAnim(string anmName)
	{
		try {
			return !anim.GetCurrentAnimatorStateInfo (0).IsName (anmName) && !anim.GetCurrentAnimatorStateInfo (1).IsName (anmName);
		} catch (Exception e) {
			return !anim.GetCurrentAnimatorStateInfo (0).IsName (anmName);
		}
	}
	public void SetHand(int side, bool state){
		transform.GetChild (4 + side).gameObject.SetActive (state);
	}

	// Bezier function (same as before)
	Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
	{
		return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
	}
}

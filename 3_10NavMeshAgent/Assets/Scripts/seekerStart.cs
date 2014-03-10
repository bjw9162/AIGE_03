
using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;


[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Dimensions))]


public class seekerStart : MonoBehaviour
{
	public GameObject target  = null;
	// These weights will be exposed in the Inspector window

//	public float avoidDist = 30.0f;
	public float seekWt = 10.0f;
//	public float avoidWt = 30.0f;
	
	// Each vehicle contains a CharacterController which
	// makes it easier to deal with the relationship between
	// movement initiated by the character and the forces
	// generated by contact with the terrain & other game objects.
	private CharacterController characterController = null;
	
	// the steering component implements the basic steering functions
	private Steering steering = null;
	
	// movement variables
	private float gravity = 20.0f;
	private Vector3 moveDirection;

	//steering variable
	private Vector3 steeringForce;
	
	//reference to an array of obstacles
	//private  GameObject[] obstacles; 
	private Vector3 startPos;

	
	public void Start ()
	{
		//get component reference
		characterController = gameObject.GetComponent<CharacterController> ();
		steering = gameObject.GetComponent<Steering> ();
		moveDirection = transform.forward;
		//obstacles = GameObject.FindGameObjectsWithTag ("Obstacle");
		startPos = transform.position;
	}
	private void OnGUI ()
	{
		if (GUI.Button (new Rect (20, 20, 100, 30), "restart")) {
			transform.position = startPos;
		}
	}

	//Put a clamp on the possible steering force
	private void ClampSteering ()
	{
		if (steeringForce.magnitude > steering.maxForce) 
		{
			steeringForce.Normalize ();
            steeringForce *= steering.maxForce;
		}
	}
	
	// Update is called once per frame
	public void Update ()
	{
		CalcSteeringForce ();
		ClampSteering ();
		
		moveDirection = transform.forward * steering.Speed;
		// movedirection equals velocity
		//add acceleration
		moveDirection += steeringForce * Time.deltaTime;
		//modified for dt
		//update speed
		steering.Speed = moveDirection.magnitude;
		if (steering.Speed != moveDirection.magnitude) {
			moveDirection = moveDirection.normalized * steering.Speed;
		}
		//orient transform
		if (moveDirection != Vector3.zero)
			transform.forward = moveDirection;
		
		// Apply gravity
		moveDirection.y -= gravity;
		
		// the CharacterController moves us subject to physical constraints
		characterController.Move (moveDirection * Time.deltaTime);
	}


	private void CalcSteeringForce ()
	{
		steeringForce = Vector3.zero;
		steeringForce += seekWt * steering.Seek (target.transform.position);
	}
	
}




 using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;
using System.IO;

//directives to enforce that our parent Game Object required components
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Steering))]


 // Made the class abstract since we are only going to have
 // People that are specialized villagers. 

public abstract class Villager : MonoBehaviour
{
    // NO MORE CHARACTER CONTROLLERS THANKS - We're going to make these NavMeshAgents
	protected FSMmatcher stMatch;
	protected int currSt;
	private Steering steering; // for low-level stuff
	private GameManager gameManager;
    

	private GameObject currentTarget;
	private bool nearWere;
	private bool wereInCity;

	//File IO variables
	int nStates;		// Number of states
	int nInputs;		// Number of input classes
	string [] states;	// Array of state names
	string [] inputs;	// Array of input class names
	int [ , ] trans;	// Transition table derived from a transition diagram
	private string FSMPath = null; // Data file name expected in bin folder

	//Wander variables for Steering. 
	public int _wanAngle;
	public int radiusOfCircle;
	public int _wanChange;
	
	//Follower reference, mostly for deletion
	private Follow follower;

	public Follow Follower
    {
        get{return follower;} 
        set{follower = value;}
    }
	
	//Unique identification index assigned by the Game Manager 
	private int index = -1; // auto assigned to an error variable
	public int Index
    {
		get { return index; }
		set { index = value; }
	}

	// Returns a reference to the manager's GameManager component (script)
	public void setGameManager (GameObject gManager)
	{
		gameManager = gManager.GetComponent<GameManager> ();
	}
	
	// We won't need movement variables because we are going to make this nice and 
    // NavMesh-y

	//list of nearby flockers
	private List<GameObject> nearVillagers = new List<GameObject> ();
	private List<float> nearVillagersDistances = new List<float> ();
	
	//Constructor for generic typeVillager
	public void Start ()
	{
		// Retrieve component references from settings in Unity
		this.characterController = gameObject.GetComponent<CharacterController> ();

		steering = gameObject.GetComponent<Steering> ();

		gameManager = GameManager.Instance; // Only one GameManager


        // Reading in from text files.... apparently
		FSMPath = "Assets/Resources/VillagerFSM.txt";
		LoadFSM();
		currSt = 0; // Initialize state in constructor???? TODO: Fix state machine

		leaderFollowBool = false; // following mayor TODO: What the hell is this boolean and change it to a dynamic thing

		nearWere = false; // if near werewolf for decision tree: TODO: make this a method so we don't need a member boolean

		wereInCity = false; //if werewolves have infiltrated the city (This should be part of the game state maybe?) TODO: fix name of this stupid variable
	}
	


	// Look up the next state from the current state and the input class
	public int MakeTrans (int currState, int inClass)
	{
		return trans [currState, inClass];
	}
	
	// Read the data file to define and fill the tables
	void LoadFSM ()
	{

		StreamReader inStream = new StreamReader (FSMPath);
		
		// State table
		nStates = int.Parse(inStream.ReadLine());
		states = new string [nStates];
		for (int i = 0; i < nStates; i++)
			states[i] = inStream.ReadLine ();
		
		// Input table
		nInputs = int.Parse(inStream.ReadLine());
		inputs = new string [nInputs];
		for (int i = 0; i < nInputs; i++)
			inputs[i] = inStream.ReadLine ();
		
		// Transition table
		trans = new int[nStates, nInputs];
		for (int i = 0; i < nStates; i++)
		{
			string[] nums = inStream.ReadLine ().Split (' ');
			for (int j = 0; j < nInputs; j++)
				trans [i, j] = int.Parse(nums[j]);
		}
		//EchoFSM ();	// See it verything got into the tables correctly
	}

	public int NInputs	// Main needs to know this
	{
		get {
			return nInputs;
		}
	}
	
	public string [] Inputs	// Ghost classes need to see this
	{
		get {
			return inputs;
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

	

	
	//Movement AI Behaviors -----------------------------------------------------------------------

	/*
	private Vector3 Separation ()
	{
		//empty our lists
		nearVillagers.Clear ();
		nearVillagersDistances.Clear ();
		
		//method variables
		Vector3 dv = new Vector3(); // the desired velocity
		Vector3 sum = new Vector3();
		
		for(int i = 0; i < gameManager.Villagers.Count; i++)
		{
			//retireves distance between two flockers of reference numbers
			// findFlocker and i
			
			GameObject villager = gameManager.Villagers[i];
			
			float dist = Vector3.Distance(this.transform.position, gameManager.Villagers[i].transform.position);
			
			if(dist < 10.0 && dist != 0)
			{
				dv =  this.transform.position - villager.transform.position;
				
				dv.Normalize();
				
				dv = dv * ((1.0f/dist));
				sum += dv;
			}
		}
		
		float dist2 = Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position);
		
		if(dist2 <= 10.0 && dist2 != 0)
		{
			dv = this.transform.position - gameManager.Mayor.transform.position;
			
			dv.Normalize();
			
			dv = dv * ((1.0f/dist2));
			
			sum += dv;
		}
		
		
		//sum.Normalize();
		//sum = sum * (steering.maxSpeed);
		sum = sum - this.steering.Velocity;

		return steering.AlignTo(sum);
	}
	
	private Vector3 runAway()
	{
		
		steeringForce = Vector3.zero;
		
		for(int i = 0; i < gameManager.Werewolves.Count; i++)
		{
			if(Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 80)
			{
				steeringForce += steering.Evasion(gameManager.Werewolves[i].transform.position);	
			}
			else
			{
				steeringForce += Vector3.zero;	
			}
		}
		
		for(int i = 0; i < gameManager.Werewolves.Count; i++)
		{
			if(Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 20)
			{
				steeringForce += steering.Flee(gameManager.Werewolves[i]);	
			}
			else
			{
				steeringForce += Vector3.zero;
			}
		}
		
		return steeringForce;
	}*/
	
	
	//---------------------------------------------------------------------------------------------

    //Handles Collision with Cart for Scoring and Clean Up Purposes - and UI
    public void OnCollisionEnter(Collision wCollision)
    {
        if (wCollision.gameObject.tag == "Cart")
        {
            GameObject savedVillager = this.gameObject;
            Villager safe = this;
            gameManager.Villagers.Remove(savedVillager);
            gameManager.vFollowers.Remove(follower.gameObject);
            gameManager.Followers.Remove(safe);
            Destroy(follower.gameObject);
            Destroy(follower);
            Destroy(savedVillager);
            Destroy(this);
            gameManager.createNewVillager();
            gameManager.Saved.SavedVillagers = gameManager.Saved.SavedVillagers + 1;

            Destroy(savedVillager);

        }
    }

	
	
}
using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;
using System.IO;

//directives to enforce that our parent Game Object required components
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Steering))]

/// <summary>
/// Summary description for Class1
/// </summary>
public class Child : Villager
{
    //Re-declare private variables
    private CharacterController characterController;
    private Steering steering;
    private GameManager gameManager;

    // List of nodes for pathfinding
    // Note: when I used this I just kept a singleton
    // and passed this around. mve9302
    private List<GameObject> pathNodes;
    public List<GameObject> PathNodes
    {
        get { return pathNodes; }
        set { pathNodes = value; }
    }

    private GameObject currentTarget;
    private bool nearWere;
    private bool wereInCity;
    //private GameObject endOfPath;

    //distance until we move to the next path node
    const int FoundTargetDistance = 25;
    const bool PathFollowingEnabled = false;
    //uncomment this and the below 
    //in the pathfolliwing method to allow 
    //paths that dont delete
    //private int pathLocation;

    //File IO variables
    int nStates;		// Number of states
    int nInputs;		// Number of input classes
    string[] states;	// Array of state names
    string[] inputs;	// Array of input class names
    int[,] trans;	// Transition table derived from a transition diagram
    private string FSMPath = null; // Data file name expected in bin folder

    //wander varables
    public int _wanAngle;
    public int radiusOfCircle;
    public int _wanChange;

    //follower reference for deletion
    private Follow follower;
    public Follow Follower
    {
        get
        {
            return
                follower;
        }
        set { follower = value; }
    }

    // a unique identification number assigned by the flock manager 
    private int index = -1;
    public int Index
    {
        get { return index; }
        set { index = value; }
    }

    // get a reference to the manager's FlockManager component (script)
    public void setGameManager(GameObject gManager)
    {
        gameManager = gManager.GetComponent<GameManager>();
    }

    //movement variables
    private float gravity = 200.0f;
    private Vector3 moveDirection;

    //steering variable
    private Vector3 steeringForce;
    private bool leaderFollowBool;

    //list of nearby flockers
    private List<GameObject> nearVillagers = new List<GameObject>();
    private List<float> nearVillagersDistances = new List<float>();

    //Creation of Villager
    public void Start()
    {
        //get component references
        characterController = gameObject.GetComponent<CharacterController>();
        steering = gameObject.GetComponent<Steering>();

        gameManager = GameManager.Instance;

        FSMPath = "Assets/Resources/VillagerFSM.txt";
        LoadFSM();
        currSt = 0;

        leaderFollowBool = false; // following mayor
        nearWere = false; // if near werewolf for decision tree
        wereInCity = false; //if were wolves have infiltrated the city

        pathNodes = new List<GameObject>();
    }

    public void Update()
    {
        CalcSteeringForce();
        ClampSteering();

        moveDirection = transform.forward * steering.Speed;
        // movedirection equals velocity
        //add acceleration
        moveDirection += steeringForce * Time.deltaTime;
        //update speed
        steering.Speed = moveDirection.magnitude;
        if (steering.Speed != moveDirection.magnitude)
        {
            moveDirection = moveDirection.normalized * steering.Speed;
        }
        //orient transform
        if (moveDirection != Vector3.zero)
            transform.forward = moveDirection;

        // Apply gravity
        moveDirection.y -= gravity;

        // the CharacterController moves us subject to physical constraints
        characterController.Move(moveDirection * Time.deltaTime);
    }

    //Uses the various Movement Behaviors to calculate the vector to
    // determine the next position of the agent
    private void CalcSteeringForce()
    {
        steeringForce = Vector3.zero;

        switch (currSt)
        {
            //WanderArea
            case 0:

                nearWere = false;
                leaderFollowBool = false;

                for (int i = 0; i < gameManager.Werewolves.Count; i++)
                {

                    if (Vector3.Distance(this.transform.position, gameManager.Werewolves[i].transform.position) < 20)
                    {
                        MakeTrans(currSt, 0);
                        nearWere = true;
                    }

                }

                if (nearWere == false)
                {

                    if (wereInCity)
                    {
                        //stMatch.MakeTrans(currSt, 3);		
                    }
                    else
                    {

                        if (Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position) < 50)
                        {
                            MakeTrans(currSt, 1);
                        }
                        else
                        {
                            Debug.Log(Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position));
                            Debug.Log("Wander");
                            steeringForce += 2 * wander();
                        }
                    }

                }

                break;
            //FollowMayor
            case 1:
                Debug.Log("Mayor");

                leaderFollowBool = true;
                /*for(int i = 0; i < gameManager.Werewolves.Count; i++) {
							
                    if(Vector3.Distance(this.transform.position, gameManager.Werewolves[i].transform.position) < 5) {
                        MakeTrans(currSt, 0);
                        nearWere = true;
                    }
							
                }*/

                if (nearWere == false)
                {

                    if (Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position) > 30 && leaderFollowBool == true)
                    {

                        MakeTrans(currSt, 4);
                        leaderFollowBool = false;

                    }
                    else
                    {
                        if (leaderFollowBool == false)
                        {
                            gameManager.Followers.Add(this);
                            steeringForce += 10 * leaderFollow();
                            steeringForce += gameManager.separationWt * Separation();
                            steeringForce += gameManager.cohesionWt * Cohesion();
                            leaderFollowBool = true;
                        }
                        else
                        {
                            steeringForce += 15 * leaderFollow();
                            steeringForce += gameManager.separationWt * Separation();
                            steeringForce += gameManager.cohesionWt * Cohesion();
                        }

                    }

                }
                break;
            //FleeWerewolf
            case 2:
                Debug.Log("Werewolf");
                nearWere = false;

                if (wereInCity)
                {
                    MakeTrans(currSt, 2);
                }
                else
                {

                    if (Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position) < 20 && leaderFollowBool == false)
                    {
                        MakeTrans(currSt, 1);
                    }
                    else
                    {

                        for (int i = 0; i < gameManager.Werewolves.Count; i++)
                        {
                            float wDist = Vector3.Distance(this.transform.position, gameManager.Werewolves[i].transform.position);

                            if (wDist < 20)
                            {
                                steeringForce += steering.Flee(gameManager.Werewolves[i].transform.position);
                                nearWere = true;
                            }

                            if (leaderFollowBool == true)
                            {
                                gameManager.Followers.Remove(this);

                            }
                        }
                    }

                    if (nearWere == false)
                        MakeTrans(currSt, 4);
                }


                leaderFollowBool = false;
                break;
            //EscapeCity
            case 3:
                //stub to be implemented later
                break;
            //TravelToNewCity
            case 4:
                //stub to be implemented later
                break;
            case 5:

                //stub - if made to new city, go to state 0

                CheckPath();
                if (currentTarget)
                {
                    steering.Seek(currentTarget);
                }
                break;
            default:

                break;
        }

        steeringForce += gameManager.inBoundsWt * StayInBounds(200, new Vector3(469, 0, 454));
    }

    //Movement AI Behaviors -----------------------------------------------------------------------

    // tether type containment - not very good!
    private Vector3 StayInBounds(float radius, Vector3 center)
    {

        steeringForce = Vector3.zero;

        if (transform.position.x > 750)
        {
            steeringForce += steering.Flee(new Vector3(800, 0, transform.position.z));
        }

        if (transform.position.x < 200)
        {
            steeringForce += steering.Flee(new Vector3(150, 0, transform.position.z));
        }

        if (transform.position.z > 715)
        {
            steeringForce += steering.Flee(new Vector3(transform.position.x, 0, 765));
        }

        if (transform.position.z < 205)
        {
            steeringForce += steering.Flee(new Vector3(transform.position.x, 0, 155));
        }

        if (transform.position.x > 750 || transform.position.x < 200 ||
            transform.position.z > 715 || transform.position.z < 205)
        {
            steeringForce += steering.Seek(gameManager.gameObject);
        }

        return steeringForce;
    }

    private Vector3 Cohesion()
    {

        return steering.Arrival(gameManager.Mayor.transform.position);
    }

    private Vector3 Separation()
    {
        //empty our lists
        nearVillagers.Clear();
        nearVillagersDistances.Clear();

        //method variables
        Vector3 dv = new Vector3(); // the desired velocity
        Vector3 sum = new Vector3();

        for (int i = 0; i < gameManager.Villagers.Count; i++)
        {
            //retireves distance between two flockers of reference numbers
            // findFlocker and i

            GameObject villager = gameManager.Villagers[i];

            float dist = Vector3.Distance(this.transform.position, gameManager.Villagers[i].transform.position);

            if (dist < 10.0 && dist != 0)
            {
                dv = this.transform.position - villager.transform.position;

                dv.Normalize();

                dv = dv * ((1.0f / dist));
                sum += dv;
            }
        }

        float dist2 = Vector3.Distance(this.transform.position, gameManager.Mayor.transform.position);

        if (dist2 <= 10.0 && dist2 != 0)
        {
            dv = this.transform.position - gameManager.Mayor.transform.position;

            dv.Normalize();

            dv = dv * ((1.0f / dist2));

            sum += dv;
        }


        //sum.Normalize();
        //sum = sum * (steering.maxSpeed);
        sum = sum - this.steering.Velocity;

        return steering.AlignTo(sum);
    }

    private Vector3 wander()
    {

        steeringForce = Vector3.zero;

        Vector3 distance = transform.forward * 2;// distance
        Vector3 refer = new Vector3(this.transform.position.x + distance.x, 0, this.transform.position.z + distance.z);
        Vector3 wanderForce = Vector3.forward * radiusOfCircle;
        wanderForce = Quaternion.AngleAxis(_wanAngle, Vector3.up) * wanderForce;
        _wanAngle += Random.Range(0, 2 * _wanChange) - _wanChange;
        refer = refer + wanderForce;

        steeringForce += steering.Seek(refer);

        return steeringForce;

    }

    private Vector3 leaderFollow()
    {
        steeringForce = Vector3.zero;


        if (gameManager.Followers[0] == this || gameManager.Followers[0] == null)
        {

            steeringForce += steering.Arrival(gameManager.Mayor.transform.position);
        }
        else
        {

            steeringForce += steering.Arrival(gameManager.Followers[gameManager.Followers.Count - 1].transform.position);
        }

        return steeringForce;
    }

    private Vector3 runAway()
    {

        steeringForce = Vector3.zero;

        for (int i = 0; i < gameManager.Werewolves.Count; i++)
        {
            if (Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 80)
            {
                steeringForce += steering.Evasion(gameManager.Werewolves[i].transform.position);
            }
            else
            {
                steeringForce += Vector3.zero;
            }
        }

        for (int i = 0; i < gameManager.Werewolves.Count; i++)
        {
            if (Vector3.Distance(gameManager.Werewolves[i].transform.position, this.transform.position) < 20)
            {
                steeringForce += steering.Flee(gameManager.Werewolves[i]);
            }
            else
            {
                steeringForce += Vector3.zero;
            }
        }

        return steeringForce;
    }

    private void ClampSteering()
    {
        if (steeringForce.magnitude > steering.maxForce)
        {
            steeringForce.Normalize();
            steeringForce *= steering.maxForce;
        }
    }

    private void CheckPath()
    {
        if (pathNodes.Count > 0)
        {
            currentTarget = pathNodes[0];
        }

        if (currentTarget)
        {
            if (Vector3.Distance(this.transform.position, currentTarget.transform.position) < FoundTargetDistance)
            {
                //this is for a not infinite path
                /*if( currentTarget == endOfPath )
                {
                    pathNodes.RemoveAt(0);
                    steering.Arrival( currentTarget.gameObject.transform.position );
                }
                else
                {
                    //move below code here
                }*/



                //remove the noade from the list
                pathNodes.RemoveAt(0);

                // If we want to keept he path for some reason visible for some reason

                //pathLocation++;
                //currentTarget = pathNodes[pathLocation];
                //this makes the target invisible
                //pathNodes[pathLocation].renderer.enabled = false;
            }
        }

    }

    //---------------------------------------------------------------------------------------------
	


}

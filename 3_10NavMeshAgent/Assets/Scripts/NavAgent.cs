using UnityEngine;
using System.Collections;

public class NavAgent : MonoBehaviour {

	NavMeshAgent agent;

	// Use this for initialization
	void Start () {

		agent = GetComponent<NavMeshAgent>();
	
	}
	
	// Update is called once per frame
	void Update () {
		// Basically just set this to a Vec3
		//agent.SetDestination(targetPoint);
	}
}

using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {
	
	
	private GameObject toFollow;
	public GameObject ToFollow{get{return toFollow;} set{toFollow = value;}}
	
	
	// Use this for initialization
	void Start () {
		
		if(toFollow == null)
		toFollow = GameObject.FindGameObjectWithTag("Mayor");
		
	}
	
	// Update is called once per frame
	void Update () {
		
		this.transform.position = new Vector3(toFollow.transform.position.x,150,toFollow.transform.position.z);
		
	
	}
}

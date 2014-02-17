using UnityEngine;
using System.Collections;

public class Dead : MonoBehaviour {
	
	private int deadVillagers;
	public int DeadVillagers {get{return deadVillagers;} set{deadVillagers = value;}}
	
	private GUIText dead;
	
	// Use this for initialization
	void Start () {
		
		deadVillagers = 0;
		dead = GameObject.FindGameObjectWithTag("Dead").GetComponent<GUIText>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
		dead.text = "Villagers Killed: " + deadVillagers;
	
	}
}

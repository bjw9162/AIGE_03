using UnityEngine;
using System.Collections;

public class Saved : MonoBehaviour {
	
	private int savedVillagers;
	public int SavedVillagers {get{return savedVillagers;} set{savedVillagers = value;}}
	
	private GUIText saved;

	// Use this for initialization
	void Start () {
		
		savedVillagers = 0;
		saved = GameObject.FindGameObjectWithTag("Saved").GetComponent<GUIText>();
	
	}
	
	// Update is called once per frame
	void Update () {
	
		saved.text = "Villagers Saved: " + savedVillagers;
	}
}

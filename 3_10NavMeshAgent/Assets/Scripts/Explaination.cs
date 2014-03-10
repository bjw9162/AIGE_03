using UnityEngine;
using System.Collections;

public class Explaination : MonoBehaviour {
	
	private GUIText explain;
	private GUIText villagers;
	private GUIText werewolves;
	private GUIText removal;
	
	private bool written;
	
	// Use this for initialization
	void Start () {
		
		written = true;
		
		explain = GameObject.FindGameObjectWithTag("Explain").GetComponent<GUIText>();
		villagers = GameObject.FindGameObjectWithTag("EVillager").GetComponent<GUIText>();
		villagers.guiText.material.SetColor("_Color",Color.green);
		werewolves = GameObject.FindGameObjectWithTag("EWerewolf").GetComponent<GUIText>();
		werewolves.guiText.material.SetColor("_Color", Color.red);
		removal = GameObject.FindGameObjectWithTag("Remove").GetComponent<GUIText>();
		
	}
	
	public void OnGUI()
	{
		if (Event.current.type == EventType.KeyDown) 
		{
        	written = false;
    	}	
	}
	
	// Update is called once per frame
	void Update () {
		
		if(written)
		{
			explain.text = "You are Mayor VanHelsing! The town is under attack from Werewolves" + "\n"
							+ "You must lead the villagers to the cart next you!" + "\n" +
							"The werewolves are trying to eat the villagers! So try to chase off the"+ "\n" +
							"werewolves when you can!";
			villagers.text = "Villagers are green!";
			werewolves.text = "Werewolves are red!";
			removal.text = "Press ENTER to remove these messages!";	
		}
		else
		{
			explain.text = "";
			villagers.text = "";
			werewolves.text = "";
			removal.text = "";
		}
	}
}
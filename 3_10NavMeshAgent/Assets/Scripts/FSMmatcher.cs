using UnityEngine;
using System.Collections;
using System.IO;

public class FSMmatcher : MonoBehaviour {

	int nStates;		// Number of states
	int nInputs;		// Number of input classes
	string [] states;	// Array of state names
	string [] inputs;	// Array of input class names
	int [ , ] trans;	// Transition table derived from a transition diagram
	private string FSMPath = null; // Data file name expected in bin folder
	
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
	
	// path name for data file passed into constructor
	public FSMmatcher (string filePath)
	{
		FSMPath = filePath;
		LoadFSM ();
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
	
	// Echoes the tables read from the data file, used for debugging
	void EchoFSM()
	{
		Debug.Log (nStates);
		for (int i = 0; i < nStates; i++)
			Debug.Log (states[i]);
		Debug.Log (nInputs);
		for (int i = 0; i < nInputs; i++)
			Debug.Log (inputs[i]);
		for (int i = 0; i < nStates; i++)
		{
			for (int j = 0; j < nInputs; j++)
				Debug.Log (trans [i, j] + " ");
			Debug.Log (" ");
		}
	}
	
	// Look up the next state from the current state and the input class
	public int MakeTrans (int currState, int inClass)
	{
		return trans [currState, inClass];
	}
}

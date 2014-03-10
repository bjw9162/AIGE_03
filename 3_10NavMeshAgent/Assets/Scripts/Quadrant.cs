using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;

[System.Serializable]

/* This class will be used alongside Grid 
to make the map into a searchable datastructure */

public class Quadrant
{
	public float leftUpper; // the starting point of the quadrant
	public float width, height;
	
	// Use this for initialization
	void Start (float lu_, float w_, float h_) {
		
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

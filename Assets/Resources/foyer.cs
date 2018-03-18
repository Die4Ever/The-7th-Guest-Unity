using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foyer : baseRoom {

	// Use this for initialization
	void Start () {
        BaseInit();
        myvidpath = "FH/f";
        nodeNames = new string[]{ "null", "front door", "dining door", "kitchen door", "music door", "library door", "upstairs" };
        CreateNodeConnection(1, 2, new Rect(0, 0.2f, 0.2f, 0.5f));
	}
	
	// Update is called once per frame
	/*void Update () {
		
	}*/
}

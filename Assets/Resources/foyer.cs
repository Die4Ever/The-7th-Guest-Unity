using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foyer : baseRoom {

	// Use this for initialization
	void Start () {
        BaseInit();
        myvidpath = "FH/f";
        nodeNames = new string[]{ "null", "front door", "dining door", "kitchen door", "music door", "library door", "upstairs" };
        CreateNodeConnection(1, 2, new Rect(0, 0.2f, 0.2f, 0.3f), 'c', 'b');
        CreateNodeConnection(1, 6, new Rect(0.4f, 0.7f, 0.2f, 0.5f), 'c', 'c');
        CreateNodeConnection(2, 3, new Rect(0.3f, 0.3f, 0.4f, 0.5f), 'c', 'c');
        CreateNodeConnection(3, 2, new Rect(0.1f, 0.1f, 0.4f, 0.5f), 'b', 'a', 'a', 'b');
        CreateNodeConnection(3, 2, new Rect(0.5f, 0.1f, 0.4f, 0.5f), 'a', 'a');
        CreateNodeConnection(2, 1, new Rect(0.7f, 0.4f, 0.2f, 0.4f), 'd', 'a');

        CreateNodeConnection(1, 5, new Rect(0.7f, 0.2f, 0.2f, 0.3f), 'c', 'c');
        CreateNodeConnection(1, 5, new Rect(0.2f, 0.2f, 0.2f, 0.5f), 'd', 'c', 'c', 'd');
        CreateNodeConnection(5, 4, new Rect(0.2f, 0.2f, 0.4f, 0.7f), 'c', 'c');

        CreateNodeConnection(4, 5, new Rect(0.7f, 0.1f, 0.2f, 0.7f), 'd', 'a', 'a', 'd');
        CreateNodeConnection(4, 5, new Rect(0.2f, 0.1f, 0.2f, 0.8f), 'a', 'b');
        CreateNodeConnection(5, 1, new Rect(0.4f, 0.3f, 0.4f, 0.5f), 'b', 'a');

        CreateNodeConnectionRotations(1, 'a', 'd');
        CreateNodeConnectionRotations(2, 'a', 'd');
        CreateNodeConnectionRotations(3, 'a', 'd');
        CreateNodeConnectionRotations(4, 'a', 'd');
        CreateNodeConnectionRotations(5, 'a', 'd');
    }
	
	// Update is called once per frame
	/*void Update () {
		
	}*/
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foyer : baseRoom {

    public int front_door=1, dining_door=2, kitchen_door=3, music_door=4, library_door=5, upstairs=6, spiders=90;//use the puzzle node to disable the clickboxes?
    // Use this for initialization
    void Start () {
        BaseInit();
        myvidpath = "FH/f";
        //nodeNames = new string[]{ "null", "front door", "dining door", "kitchen door", "music door", "library door", "upstairs" };
        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(dining_door, 'b'), new Rect(0, 0.2f, 0.2f, 0.3f));
        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(upstairs, 'c'), new Rect(0.4f, 0.7f, 0.2f, 0.5f));
        CreateNodeConnection(new RoomPosition(dining_door, 'c'), new RoomPosition(kitchen_door, 'c'), new Rect(0.3f, 0.3f, 0.4f, 0.5f));
        CreateNodeConnection(new RoomPosition(kitchen_door, 'b'), null, new Rect(0.1f, 0.1f, 0.4f, 0.5f), new RoomPosition[] { new RoomPosition(kitchen_door, 'a'), new RoomPosition(dining_door, 'a'), new RoomPosition(dining_door, 'b') } );
        CreateNodeConnection(new RoomPosition(kitchen_door, 'a'), new RoomPosition(dining_door, 'a'), new Rect(0.5f, 0.1f, 0.4f, 0.5f));
        CreateNodeConnection(new RoomPosition(dining_door, 'd'), new RoomPosition(front_door, 'a'), new Rect(0.7f, 0.4f, 0.2f, 0.4f));

        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(library_door, 'c'), new Rect(0.7f, 0.2f, 0.2f, 0.3f));
        CreateNodeConnection(new RoomPosition(front_door, 'd'), null, new Rect(0.2f, 0.2f, 0.2f, 0.5f), new RoomPosition[] { new RoomPosition(front_door, 'c'), new RoomPosition(library_door, 'c'), new RoomPosition(library_door, 'd') });
        CreateNodeConnection(new RoomPosition(library_door, 'c'), new RoomPosition(music_door, 'c'), new Rect(0.2f, 0.2f, 0.4f, 0.7f));

        CreateNodeConnection(new RoomPosition(music_door, 'd'), null, new Rect(0.7f, 0.1f, 0.2f, 0.7f), new RoomPosition[] { new RoomPosition(music_door, 'a'), new RoomPosition(library_door, 'a'), new RoomPosition(library_door, 'd') });
        CreateNodeConnection(new RoomPosition(music_door, 'a'), new RoomPosition(library_door, 'b'), new Rect(0.2f, 0.1f, 0.2f, 0.8f));
        CreateNodeConnection(new RoomPosition(library_door, 'b'), new RoomPosition(front_door, 'a'), new Rect(0.4f, 0.3f, 0.4f, 0.5f));
        CreateNodeConnection(new RoomPosition(music_door, 'a'), null, new Rect(0.4f, 0.4f, 0.3f, 0.3f), new RoomPosition[] { new RoomPosition(library_door, 'b'), new RoomPosition(front_door, 'a') } );

        CreateNodeConnectionRotations(1, 'a', 'd');
        CreateNodeConnectionRotations(2, 'a', 'd');
        CreateNodeConnectionRotations(3, 'a', 'd');
        CreateNodeConnectionRotations(4, 'a', 'd');
        CreateNodeConnectionRotations(5, 'a', 'd');

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(front_door, 'a'), type=ClickboxType.DRAMAMASK, clickbox=new Rect(0.2f, 0.1f, 0.5f, 0.8f), callback = f2 });
    }
	
	// Update is called once per frame
	/*void Update () {
		
	}*/

    void f2(NodeConnection nc)
    {
        fmvman.QueueVideo(myvidpath+"2_.avi");
        nc.type = ClickboxType.PUZZLE;
        nc.callback = startspiders;
    }

    void startspiders(NodeConnection nc)
    {
        Debug.Log("startspiders()");
        currPos.node = spiders;
        fmvman.QueueSong("GAMWAV/2_s_2.avi");
        fmvman.QueueVideo(myvidpath + "1_pf.avi");
        //fmvman.QueueSong("GAMWAV/2_s_2.avi", true);
        GameObject go = Instantiate(Resources.Load("spiders", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = endspiders;
    }

    void endspiders(string s)
    {
        currPos.node = front_door;
        fmvman.QueueVideo(myvidpath + "3_0.avi");
        fmvman.QueueVideo(myvidpath + "1_pb.avi");
    }
}

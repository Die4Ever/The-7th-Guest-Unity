using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class foyer : baseRoom {

    const int front_door=1, dining_door=2, kitchen_door=3, music_door=4, library_door=5, upstairs=6, spiders=90;//use the puzzle node to disable the clickboxes?
    // Use this for initialization
    void Start () {
        BaseInit();
        myvidpath = "FH/f";
        PlaySong("GU56");
        //PlaySong("../music/GU61.ogg");//when to use which song? maybe I need an EnterRoom function to determine where we're coming from and set the song?
        //nodeNames = new string[]{ "null", "front door", "dining door", "kitchen door", "music door", "library door", "upstairs" };
        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(dining_door, 'b'), new Rect(0, 0.2f, 0.2f, 0.3f));
        CreateNodeConnection(new RoomPosition(front_door, 'b'), null, new Rect(0.8f, 0.2f, 0.17f, 0.35f), new RoomPosition[] { new RoomPosition(front_door, 'c'), new RoomPosition(dining_door, 'b') });
        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(upstairs, 'c'), new Rect(0.4f, 0.7f, 0.2f, 0.5f));
        CreateNodeConnection(new RoomPosition(dining_door, 'c'), new RoomPosition(kitchen_door, 'c'), new Rect(0.3f, 0.3f, 0.4f, 0.7f));
        CreateNodeConnection(new RoomPosition(kitchen_door, 'b'), null, new Rect(0.1f, 0.1f, 0.4f, 0.9f), new RoomPosition[] { new RoomPosition(kitchen_door, 'a'), new RoomPosition(dining_door, 'a'), new RoomPosition(dining_door, 'b') } );
        CreateNodeConnection(new RoomPosition(kitchen_door, 'a'), new RoomPosition(dining_door, 'a'), new Rect(0.5f, 0.1f, 0.4f, 0.9f));
        CreateNodeConnection(new RoomPosition(dining_door, 'd'), new RoomPosition(front_door, 'a'), new Rect(0.7f, 0.4f, 0.2f, 0.4f));

        CreateNodeConnection(new RoomPosition(front_door, 'c'), new RoomPosition(library_door, 'c'), new Rect(0.7f, 0.2f, 0.2f, 0.3f));
        CreateNodeConnection(new RoomPosition(front_door, 'd'), null, new Rect(0.3f, 0.2f, 0.2f, 0.5f), new RoomPosition[] { new RoomPosition(front_door, 'c'), new RoomPosition(library_door, 'c'), new RoomPosition(library_door, 'd') });
        CreateNodeConnection(new RoomPosition(front_door, 'd'), null, new Rect(0.05f, 0.2f, 0.2f, 0.5f), new RoomPosition[] { new RoomPosition(front_door, 'c'), new RoomPosition(library_door, 'c') });
        CreateNodeConnection(new RoomPosition(library_door, 'c'), new RoomPosition(music_door, 'c'), new Rect(0.2f, 0.2f, 0.4f, 0.8f));

        CreateNodeConnection(new RoomPosition(music_door, 'd'), null, new Rect(0.7f, 0.1f, 0.2f, 0.9f), new RoomPosition[] { new RoomPosition(music_door, 'a'), new RoomPosition(library_door, 'a'), new RoomPosition(library_door, 'd') });
        CreateNodeConnection(new RoomPosition(music_door, 'a'), new RoomPosition(library_door, 'a'), new Rect(0.1f, 0.1f, 0.2f, 0.8f));
        CreateNodeConnection(new RoomPosition(music_door, 'a'), null, new Rect(0.4f, 0.4f, 0.3f, 0.3f), new RoomPosition[] { new RoomPosition(library_door, 'a'), new RoomPosition(front_door, 'a') });
        CreateNodeConnection(new RoomPosition(library_door, 'a'), new RoomPosition(front_door, 'a'), new Rect(0.4f, 0.3f, 0.4f, 0.5f));

        CreateNodeConnectionRotations(front_door, 'a', 'd');
        CreateNodeConnectionRotations(dining_door, 'a', 'd');
        CreateNodeConnectionRotations(kitchen_door, 'a', 'd');
        CreateNodeConnectionRotations(music_door, 'a', 'd');
        CreateNodeConnectionRotations(library_door, 'a', 'd');

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(front_door, 'a'), type=ClickboxType.DRAMAMASK, clickbox=new Rect(0.2f, 0.1f, 0.5f, 0.9f), callback = f2 });

        //doors to rooms - dining_door b, music_door c, library door d, kitchen_door c
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(dining_door, 'b'), type = ClickboxType.EXITROOM, clickbox = new Rect(0.2f, 0.2f, 0.6f, 0.6f), callback = enterDining });
    }
	
	// Update is called once per frame
	/*void Update () {
		
	}*/

    void enterDining(NodeConnection nc)
    {
        fmvman.QueueVideo(new FMVManager.Command { file=myvidpath+"2_d.avi", callback=enterDining2, type = FMVManager.CommandType.VIDEO });
    }

    void enterDining2(FMVManager.Command c)
    {
        Debug.Log("enterDining2");
        fmvman.QueueVideo(new FMVManager.Command { file = "DR/dr_mi.avi", type = FMVManager.CommandType.VIDEO, freezeFrame = true, fadeInTime = 1 });
        fmvman.SwitchRoom("diningRoom", 1, 'a');
    }

    void f2(NodeConnection nc)
    {
        PlaySong("GU8");
        QueueVideo("2_.avi");
        PlaySong("GU16");
        nc.type = ClickboxType.PUZZLE;
        nc.callback = startspiders;
    }

    void startspiders(NodeConnection nc)
    {
        Debug.Log("startspiders()");
        currPos.node = spiders;
        QueueMovement("1_pf.avi", wait:false);
        //fmvman.QueueSong("GAMWAV/2_s_2.avi", true);
        GameObject go = Instantiate(Resources.Load("spiders", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = endspiders;
    }

    void endspiders(string s)
    {
        currPos.node = front_door;
        PlaySong("GU18", true);
        QueueVideo("3_0.avi");
        PlaySong("GU16");
        QueueMovement("1_pb.avi");
    }
}

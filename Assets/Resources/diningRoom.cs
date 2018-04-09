using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diningRoom : baseRoom
{
    private int dr_door = 1, dr_table = 2, cake=90;
    // Use this for initialization
    void Start () {
        BaseInit();
        myvidpath = "DR/dr";
        //PlaySong("../music/GU56.ogg");//dining room doesn't change music until the cutscene?

        //CreateNodeConnection(new RoomPosition(dr_door, 'a'), new RoomPosition(dr_table, 'a', "_mi.avi"), new Rect(0.3f, 0.2f, 0.6f, 0.5f));
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(dr_door, 'a'), toPos = new RoomPosition[] { new RoomPosition(dr_table, 'a', "_mi.avi") }, clickbox = new Rect(0.3f, 0.2f, 0.6f, 0.5f), callback = approachTable });
    }

    void approachTable(NodeConnection nc)
    {
        PlaySong("GU19", false);
        QueueVideo("1_0.avi", completed1_0);
        //completed1_0(null);
    }

    void completed1_0(FMVManager.Command c)
    {
        PlaySong("GU16");

        //need to make a clickbox for this here...
        currPos.node = cake;
        QueueMovement("_v.avi", false);
        GameObject go = Instantiate(Resources.Load("cake", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = endcake;
    }

    void endcake(string s)
    {
        QueueMovement("_vb.avi");
        PlaySong("GU9");
        QueueVideo("2_.avi", completed2_);
    }

    void completed2_(FMVManager.Command c)
    {
        //GU19 for after dr2_.avi??? does the dining room have a song or not?
        PlaySong("GU19");
    }
}

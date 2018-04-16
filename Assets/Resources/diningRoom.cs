using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class diningRoom : baseRoom
{
    const int dr_door = 1, dr_table = 2, cake=90;
    // Use this for initialization
    void Start () {
        BaseInit();
        myvidpath = "DR/dr";
        //PlaySong("../music/GU56.ogg");//dining room doesn't change music until the cutscene?

        //CreateNodeConnection(new RoomPosition(dr_door, 'a'), new RoomPosition(dr_table, 'a', "_mi.avi"), new Rect(0.3f, 0.2f, 0.6f, 0.5f));
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(dr_door, 'a'), toPos = new RoomPosition[] { new RoomPosition(dr_table, 'a', "_mi.avi") }, clickbox = new Rect(0.3f, 0.2f, 0.6f, 0.5f), callback = approachTable });
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(dr_table, 'a'), toPos = new RoomPosition[] { new RoomPosition(dr_table, 'b', "_mtf.avi"), new RoomPosition(dr_door, 'b', "_mo.avi") }, clickbox = right });

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(dr_door, 'b'), type = ClickboxType.EXITROOM, clickbox = new Rect(0.2f, 0.2f, 0.6f, 0.6f), callback = exitDining });
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
        fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORVIDEO });
        fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO });
        QueueMovement("_vb.avi");
        currPos.node = dr_table;
        PlaySong("GU9");
        QueueVideo("2_.avi", completed2_);
    }

    void completed2_(FMVManager.Command c)
    {
        //GU19 for after dr2_.avi??? does the dining room have a song or not?
        PlaySong("GU19");
    }

    void exitDining(NodeConnection nc)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + "_d.avi", callback = exitDining2, type = FMVManager.CommandType.VIDEO });
    }

    void exitDining2(FMVManager.Command c)
    {
        Debug.Log("exitDining2");
        fmvman.QueueVideo(new FMVManager.Command { file = "FH/f_2bc.avi", type = FMVManager.CommandType.VIDEO, freezeFrame = true, fadeInTime = 1 });
        fmvman.SwitchRoom("foyer", 2, 'd');
    }
}

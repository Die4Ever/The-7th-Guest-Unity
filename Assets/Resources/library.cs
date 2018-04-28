using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class library : baseRoom
{
    public const int li_door = 1, li_mid = 2, li_bookshelf = 4, telescope = 90, book = 91;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "LI/l";
        //PlaySong("../music/GU56.ogg");//dining room doesn't change music until the cutscene?
        CreateNodeConnection(new RoomPosition(li_door, 'a'), new RoomPosition(li_mid, 'a'), new Rect(0.5f, 0.3f, 0.4f, 0.5f));
        CreateNodeConnection(new RoomPosition(li_door, 'a'), new RoomPosition(li_bookshelf, 'a'), new Rect(0.1f, 0.4f, 0.4f, 0.5f));
        CreateNodeConnection(new RoomPosition(li_mid, 'c'), new RoomPosition(li_door, 'c'), new Rect(0.3f, 0.1f, 0.4f, 0.7f));

        CreateNodeConnectionRotations(li_door, new char[] { 'a', 'c' });
        CreateNodeConnectionRotations(li_mid, 'a', 'd');

        MakeRoomTransition(new RoomPosition(li_door, 'c'), "foyer", foyer.library_door, 'b', new Rect(0.1f, 0.1f, 0.4f, 0.8f), "1_x.avi", "FH/f_5ba.avi");

        //telescope puzzle starts with 7_s_1
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(li_mid, 'a'), type = ClickboxType.PUZZLE, clickbox = CenteredRect(0.6f, 0.7f, 0.3f, 0.3f), callback = StartTelescope });
    }

    protected override void AfterTravel()
    {
        if (currPos.node == li_mid && currPos.facing == 'a')
        {
            PlaySong("GU43");
            QueueVideo(file: "i_suck.avi", fps: 9, callback: AfterSuck);//lol this filename
        }
    }

    void AfterSuck(FMVManager.Command c)
    {
        PlaySong("GU63");
    }

    void StartTelescope(NodeConnection nc)
    {
        currPos.node = telescope;
        QueueMovement("2_3f.avi", wait: true);
        GameObject go = Instantiate(Resources.Load("telescope", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = EndTelescope;
    }

    void EndTelescope(string s)
    {
        QueueMovement("2_3b.avi");
        QueueMovement("_2bd");
        currPos.node = li_mid;
        currPos.facing = 'd';
        QueueVideo("ipoem");
    }
}

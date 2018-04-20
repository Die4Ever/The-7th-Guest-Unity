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

        CreateNodeConnectionRotations(li_door, new char[]{ 'a', 'c'});
        CreateNodeConnectionRotations(li_mid, 'a', 'd');

        MakeRoomTransition(new RoomPosition(li_door, 'c'), "foyer", foyer.library_door, 'b', new Rect(0.1f, 0.1f, 0.4f, 0.8f), "1_x.avi", "FH/f_5ba.avi");
    }
}

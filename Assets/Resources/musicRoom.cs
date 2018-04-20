using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicRoom : baseRoom
{
    public const int m_door = 1;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "MU/mu";

        CreateNodeConnection(new RoomPosition(m_door, 'b'), new RoomPosition(m_door, 'a', "abb.avi"), left);
        CreateNodeConnection(new RoomPosition(m_door, 'b'), new RoomPosition(m_door, 'a', "cd.avi"), right);
        CreateNodeConnection(new RoomPosition(m_door, 'a'), new RoomPosition(m_door, 'b', "ab.avi"), right);
        CreateNodeConnection(new RoomPosition(m_door, 'a'), new RoomPosition(m_door, 'b', "cdb.avi"), left);

        MakeRoomTransition(new RoomPosition(m_door, 'b'), "foyer", foyer.music_door, 'a', new Rect(0.3f, 0.1f, 0.4f, 0.8f), "ex.avi", "FH/f_4fa.avi");
    }
}

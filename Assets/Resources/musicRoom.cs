using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class musicRoom : baseRoom
{
    public const int m_door = 1, m_piano = 90;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "MU/mu";

        CreateNodeConnection(new RoomPosition(m_door, 'b'), new RoomPosition(m_door, 'a', "abb"), left);
        CreateNodeConnection(new RoomPosition(m_door, 'b'), new RoomPosition(m_door, 'a', "cd"), right);
        CreateNodeConnection(new RoomPosition(m_door, 'a'), new RoomPosition(m_door, 'b', "ab"), right);
        CreateNodeConnection(new RoomPosition(m_door, 'a'), new RoomPosition(m_door, 'b', "cdb"), left);

        MakeRoomTransition(new RoomPosition(m_door, 'b'), "foyer", foyer.music_door, 'a', new Rect(0.3f, 0.1f, 0.4f, 0.8f), "ex", "FH/f_4fa");

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(m_door, 'a'), type = ClickboxType.PUZZLE, clickbox = CenteredRect(0.5f, 0.5f, 0.7f, 0.7f), callback = StartPiano });
    }

    void StartPiano(NodeConnection nc)
    {
        QueueMovement("pi");
        currPos.node = m_piano;
        GameObject go = Instantiate(Resources.Load("piano", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = EndPiano;
        //fmvman.QueueVideo(new FMVManager.Command { file="K/shelf.avi", tags="shelf", fadeInTime=1.0f });//play this in the puzzle, and then k6_.avi outside of the puzzle object? that means the kitchen is responsible for killing the shelf video even though the puzzle started it
    }

    void EndPiano(string s)
    {
        QueueMovement("pib");
        currPos.node = m_door;
    }
}

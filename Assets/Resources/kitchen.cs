using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kitchen : baseRoom
{
    public const int k_door = 1, k_mid = 2, k_cans = 90;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "K/k";

        //CreateNodeConnectionRotations(k_door, 'a', 'b');
        CreateNodeConnection(new RoomPosition(k_mid, 'b'), new RoomPosition(k_door, 'a'), new Rect(0.3f, 0.1f, 0.3f, 0.7f));

        CreateNodeConnection(new RoomPosition(k_door, 'b'), new RoomPosition(k_door, 'a'), left);
        CreateNodeConnection(new RoomPosition(k_door, 'a'), new RoomPosition(k_door, 'b'), right);

        CreateNodeConnectionRotations(k_mid, 'a', 'd');

        MakeRoomTransition(new RoomPosition(k_door, 'a'), "foyer", foyer.kitchen_door, 'a', new Rect(0.1f, 0.01f, 0.5f, 0.9f), "1_6.avi", "FH/f_3fa.avi");

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(k_mid, 'd'), type = ClickboxType.PUZZLE, clickbox = CenteredRect(0.5f, 0.5f, 0.7f, 0.7f), callback = StartCans });
        nodeConnections.Add(new NodeConnection { fromPos= new RoomPosition(k_door, 'b'), type = ClickboxType.DRAMAMASK, clickbox = CenteredRect(0.5f, 0.5f, 0.3f, 0.8f), callback = k5_ });
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(k_door, 'b'), type = ClickboxType.DRAMAMASK, clickbox = CenteredRect(0.8f, 0.5f, 0.3f, 0.3f), callback = k3_ });

        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(k_mid, 'b'), type = ClickboxType.DRAMAMASK, clickbox = CenteredRect(0.3f, 0.5f, 0.3f, 0.3f), callback = k7_ });
    }

    void k3_(NodeConnection nc)
    {
        QueueVideo("3_");
        //nodeConnections.Remove(nc);//we can allow this one to play again if the user wants, maybe instead of getting rid of the clickboxes they should be moved offscreen so a replay option can find them?
    }

    void k5_(NodeConnection nc)
    {
        QueueVideo("5_");
        nodeConnections.Remove(nc);
        CreateNodeConnection(new RoomPosition(k_door, 'b'), new RoomPosition(k_mid, 'd'), new Rect(0.3f, 0.1f, 0.3f, 0.7f));
    }

    void k7_(NodeConnection nc)
    {
        QueueMovement("2_7f");
        QueueVideo("7_");
        QueueMovement("2_7b");
    }

    void StartCans(NodeConnection nc)
    {
        QueueMovement("2_4.avi");
        currPos.node = k_cans;
        GameObject go = Instantiate(Resources.Load("cans", typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = EndCans;
        //fmvman.QueueVideo(new FMVManager.Command { file="K/shelf.avi", tags="shelf", fadeInTime=1.0f });//play this in the puzzle, and then k6_.avi outside of the puzzle object? that means the kitchen is responsible for killing the shelf video even though the puzzle started it
    }

    void EndCans(string s)
    {
        QueueVideo("6_.avi");
        fmvman.QueueVideo(new FMVManager.Command { file="K/k2_4.avi", freezeFrame=true, fadeInTime=1 });
        currPos.node = k_mid;
    }
}

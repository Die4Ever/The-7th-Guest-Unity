using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kitchen : baseRoom
{
    public const int k_door = 1, k_mid = 2;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "K/k";

        //CreateNodeConnectionRotations(k_door, 'a', 'b');
        CreateNodeConnection(new RoomPosition(k_door, 'b'), new RoomPosition(k_mid, 'd'), new Rect(0.3f, 0.1f, 0.3f, 0.7f));
        CreateNodeConnection(new RoomPosition(k_mid, 'b'), new RoomPosition(k_door, 'a'), new Rect(0.3f, 0.1f, 0.3f, 0.7f));

        CreateNodeConnection(new RoomPosition(k_door, 'b'), new RoomPosition(k_door, 'a'), left);
        CreateNodeConnection(new RoomPosition(k_door, 'a'), new RoomPosition(k_door, 'b'), right);

        CreateNodeConnectionRotations(k_mid, 'a', 'd');

        MakeRoomTransition(new RoomPosition(k_door, 'a'), "foyer", foyer.kitchen_door, 'a', new Rect(0.1f, 0.01f, 0.5f, 0.9f), "1_6.avi", "FH/f_3fa.avi");
    }
}

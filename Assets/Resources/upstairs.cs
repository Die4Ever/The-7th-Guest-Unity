﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class upstairs : baseRoom
{
    public const int u_stairs = 1, u_mb_ek = 2, u_bathroom = 3, u_dutton = 4, u_dollroom = 5, u_attic = 7, u_east = 8;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "FH/h";

        //east to west
        CreateNodeConnection(new RoomPosition(u_east, 'b'), new RoomPosition(u_stairs, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_stairs, 'b'), new RoomPosition(u_mb_ek, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_mb_ek, 'b'), new RoomPosition(u_bathroom, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_bathroom, 'b'), new RoomPosition(u_dutton, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_dutton, 'b'), new RoomPosition(u_dollroom, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));

        //west to east
        CreateNodeConnection(new RoomPosition(u_dollroom, 'd'), new RoomPosition(u_dutton, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_dutton, 'd'), new RoomPosition(u_bathroom, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_bathroom, 'd'), new RoomPosition(u_mb_ek, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_mb_ek, 'd'), new RoomPosition(u_stairs, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_stairs, 'd'), new RoomPosition(u_east, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));

        CreateNodeConnectionRotations(u_stairs, 'a', 'd');
        CreateNodeConnectionRotations(u_mb_ek, 'a', 'd');
        CreateNodeConnectionRotations(u_bathroom, 'a', 'd');
        CreateNodeConnectionRotations(u_dutton, 'a', 'd');
        //CreateNodeConnectionRotations(u_dollroom, 'a', 'b');//need to fix these rotations, they will probably need to be custom
        //CreateNodeConnectionRotations(u_attic, 'a', 'b');//same as dollroom door
        CreateNodeConnectionRotations(u_east, 'a', 'd');

        //we can't use the normal way of MakeRoomTransition because the transition video actually has the f prefix on the filename
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(u_stairs, 'a'), type = ClickboxType.EXITROOM, clickbox = new Rect(0.3f, 0.0f, 0.4f, 0.5f), callback = downStairs });
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(u_stairs, 'c'), type = ClickboxType.CHATTERINGTEETH, clickbox = CenteredRect(0.6f, 0.5f, 0.4f, 0.7f), callback = h_morph });
    }

    void h_morph(NodeConnection nc)
    {
        PlaySong("GU18", loop:true);//I might need to be able to set it to loop twice?
        //QueueVideo("_morph", fps: 10);
        QueueVideo("_morph", callback: AfterMorph, fps: 8);
        fmvman.IncrementVariable("h_morph");
    }

    void AfterMorph(FMVManager.Command c)
    {
        PlaySong("GU56");
    }

    void downStairs(NodeConnection nc)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = "FH/f6_1.avi", callback = downStairs2, type = FMVManager.CommandType.VIDEO, tags = "movement" });
    }

    void downStairs2(FMVManager.Command c)
    {
        fmvman.SwitchRoom("foyer", foyer.front_door, 'a');
    }

    protected override void AfterTravel()
    {
        if (currPos.node == u_stairs && currPos.facing == 'b')
        {
            var r = Random.value;
            if (r < 0.3) QueueVideo("_ghost3.avi");
            else if(r < 0.6) QueueVideo("_ghost4");
            else QueueVideo("_ghost1.avi");
            QueueVideo("c_");
            QueueVideo("2_");
        }
        if(currPos.node == u_dutton && currPos.facing == 'd')
        {
            QueueVideo("_ghost2");
        }
        if(currPos.node==u_stairs && currPos.facing == 'd')
        {
            QueueVideo("b_");
        }
    }
}

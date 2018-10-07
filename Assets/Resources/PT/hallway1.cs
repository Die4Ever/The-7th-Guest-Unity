using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hallway1 : baseRoom
{
    public const int u_stairs = 1, u_mb_ek = 2, u_bathroom = 3, u_dutton = 4, u_dollroom = 5, u_attic = 7, u_east = 8;
    int stairs_b_times = 0;
    int stage = 0;
    int loops = 0;
    int advance_stage = 0;
    int ghost_sights = 0;
    NodeConnection hands_painting, knoxdoor_con;
    Color multColor = new Color(1, 1, 1, 1);
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
        //CreateNodeConnection(new RoomPosition(u_dutton, 'b'), new RoomPosition(u_dollroom, 'b'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));

        //west to east
        CreateNodeConnection(new RoomPosition(u_dollroom, 'd'), new RoomPosition(u_dutton, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_dutton, 'd'), new RoomPosition(u_bathroom, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_bathroom, 'd'), new RoomPosition(u_mb_ek, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_mb_ek, 'd'), new RoomPosition(u_stairs, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));
        CreateNodeConnection(new RoomPosition(u_stairs, 'd'), new RoomPosition(u_east, 'd'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));

        CreateNodeConnectionRotations(u_stairs, 'a', 'd');
        CreateNodeConnectionRotations(u_mb_ek, 'a', 'd'/*, only180: true*/);
        CreateNodeConnectionRotations(u_bathroom, 'a', 'd'/*, only180: true*/);
        CreateNodeConnectionRotations(u_dutton, 'a', 'd');
        //CreateNodeConnectionRotations(u_dollroom, 'a', 'b');//need to fix these rotations, they will probably need to be custom
        //CreateNodeConnectionRotations(u_attic, 'a', 'b');//same as dollroom door
        CreateNodeConnectionRotations(u_east, 'a', 'd', only180: true);

        //we can't use the normal way of MakeRoomTransition because the transition video actually has the f prefix on the filename
        //nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(u_stairs, 'a'), type = ClickboxType.EXITROOM, clickbox = new Rect(0.3f, 0.0f, 0.4f, 0.5f), callback = downStairs });
        hands_painting = new NodeConnection { fromPos = new RoomPosition(u_stairs, 'c'), type = ClickboxType.CHATTERINGTEETH, clickbox = CenteredRect(0.6f, 0.5f, 0.4f, 0.7f), callback = h_morph };
        knoxdoor_con = new NodeConnection { fromPos = new RoomPosition(u_mb_ek, 'c'), type = ClickboxType.CHATTERINGTEETH, clickbox = CenteredRect(0.5f, 0.5f, 0.7f, 0.8f), callback = knoxdoor };
        //CreateNodeConnection(new RoomPosition(u_attic, 'c'), new RoomPosition(1, 'a', "7_t.avi"), new Rect(0.1f, 0.1f, 0.8f, 0.8f) );
        nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(u_attic, 'c'), type = ClickboxType.EXITROOM, clickbox = CenteredRect(0.5f, 0.5f, 0.7f, 0.7f), callback = h_attic_gameroom });
        CreateNodeConnection(new RoomPosition(u_dutton, 'c'), new RoomPosition(u_attic, 'c'), new Rect(0.1f, 0.1f, 0.8f, 0.8f));

        CreateNodeConnection(new RoomPosition(u_attic, 'c'), new RoomPosition(u_attic, 'a', "_7b.avi"), left);
        CreateNodeConnection(new RoomPosition(u_attic, 'c'), new RoomPosition(u_attic, 'a', "_7b.avi"), right);

        CreateNodeConnection(new RoomPosition(u_attic, 'a'), new RoomPosition(u_attic, 'c', "_7f.avi"), left);
        CreateNodeConnection(new RoomPosition(u_attic, 'a'), new RoomPosition(u_attic, 'c', "_7f.avi"), right);

        CreateNodeConnection(new RoomPosition(u_attic, 'a'), new RoomPosition(u_dutton, 'a'), CenteredRect(0.5f, 0.5f, 0.7f, 0.7f));

        PlaySong("GU15");
        //GU21 for grate song, GU25 for end of crypt cutscene, GU24 for leaving crypt? GU27 for piano banging in the maze, GU34 for we dare you Tad! GU67 for going down the sink, 

        if (fmvman.variables.ContainsKey("ptloops"))
        {
            stage = fmvman.variables["ptstage"];
            loops = fmvman.variables["ptloops"];
            NewLoop(new FMVManager.Command());
        }
        fmvman.variables["ptloops"] = loops;
        fmvman.variables["ptstage"] = stage;

        if (loops == 0 && stage == 0)
        {
            FreezeMovement("8_1.avi", 1);
            PlaySound("GAMWAV/1_e_1.avi");
            fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO, countdown = 1 });
            PlaySound("GAMWAV/1_e_2.avi");
            fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO, countdown = 1 });
        }
    }

    void h_morph(NodeConnection nc)
    {
        PlaySong("GU18", loop: true);//I might need to be able to set it to loop twice?
        //QueueVideo("_morph", fps: 10);
        QueueVideo("_morph", callback: PlayMusic, fps: 8);
        fmvman.IncrementVariable("h_morph");
        advance_stage = 1;
        nodeConnections.Remove(hands_painting);
    }

    void knoxdoor(NodeConnection nc)
    {
        if(stage==4)
        {
            advance_stage = 1;
            PlaySound("../wavs/thud1.wav", true, countdown: 5);
            PlaySound("../wavs/thud1.wav", true);
        }
        if(stage==5)
        {
            advance_stage = 1;
            //PlaySound("../wavs/thud1.wav");
            QueueMovement("2_g.avi");
            QueueVideo("", fullfilename: "B/b1_s.avi", fadeIn: 1);
            QueueVideo("", fullfilename: "B/b_drain.avi");
            fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFOROVERLAY, callback = NewLoop }, true);
        }
        nodeConnections.Remove(knoxdoor_con);
    }

        void h_attic_gameroom(NodeConnection nc)
    {
        QueueMovement("7_t.avi");
        fmvman.QueueVideo(new FMVManager.Command{ type = FMVManager.CommandType.WAITFOROVERLAY, callback = NewLoop }, true);
    }

    void NewLoop(FMVManager.Command c)
    {
        if(stage%4 == 3)
            multColor = new Color(0.3f, 0.3f, 0.3f, 1);
        else if(stage % 4 == 2)
            multColor = new Color(0.5f, 0.3f, 0.3f, 1);
        else if(stage % 4 == 1)
            multColor = new Color(0.4f, 0.4f, 0.7f, 1);
        else
            multColor = new Color(0.8f, 1.0f, 0.8f, 1);
        loops++;
        stage += advance_stage;
        advance_stage = 0;
        fmvman.variables["ptloops"] = loops;
        fmvman.variables["ptstage"] = stage;
        ghost_sights = 0;

        if (stage == 1) PlaySound("GAMWAV/8_s_13.avi");
        if (stage == 6) PlaySound("GAMWAV/8_s_14.avi");

        if(stage>12)
        {
            multColor = new Color(1, 1, 1, 1);
        }
        if (stage == 12)
        {
            stage++;
            fmvman.variables["ptstage"] = stage;
            fmvman.SwitchRoom("PT/maze", 1, 'a');
        }
        else
        {
            FreezeMovement("8_1.avi", 1);
            currPos = new RoomPosition(u_east, 'b');
        }
    }

    /*void AfterMorph(FMVManager.Command c)
    {
        PlaySong("GU15");
    }*/

    void downStairs(NodeConnection nc)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = "FH/f6_1.avi", callback = downStairs2, type = FMVManager.CommandType.VIDEO, tags = "movement" });
    }

    void downStairs2(FMVManager.Command c)
    {
        fmvman.SwitchRoom("foyer", foyer.front_door, 'a');
    }

    void PlayMusic(FMVManager.Command c)
    {
        PlaySong("GU15");
    }

    private void Update()
    {
        base.Update();
        videoScript[] vids = GameObject.FindObjectsOfType<videoScript>();
        foreach (videoScript v in vids)
        {
            v.multColor = multColor;
        }
    }

    protected override void AfterTravel()
    {
        //5_e_2 "this goes some place" is unused
        //10_ is all unused?
        //N/ryme.avi is the kid's rhyme
        //N/n1po.avi is the jack in the box winding up, I could probably loop that a bunch of times for suspense
        if (stage == 0 && currPos.node == u_bathroom && currPos.facing == 'b')
        {
            //PlaySound("GAMWAV/8_s_12.avi");//"right would've been a safer choice", save this for when the player actually has a choice
        }
        if (currPos.node == u_stairs && currPos.facing == 'b')
        {
            if (stage == 10 && ghost_sights == 0)
            {
                ghost_sights++;
                PlaySong("");//maybe don't play song for this one? or even turn off music?
                //fmvman.QueueOverlay(new FMVManager.Command { file = myvidpath + "_ghost4", playbackSpeed = 1, threshold = 5, transparentColor = new Color(0.5f, 0.5f, 0.5f, 0.5f), type = FMVManager.CommandType.OVERLAY, slope = 0.5f, z = 0 }, false);
                PlaySound("FH/h_ghost4.avi");
                fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO });
                PlaySound("GAMWAV/8_s_9.avi");//FEELING LONELY, or do I play this when they go up to the now-locked attic door?
                fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO, callback = PlayMusic });

                //nodeConnections.Add(new NodeConnection { fromPos = new RoomPosition(u_attic, 'c'), type = ClickboxType.EXITROOM, clickbox = CenteredRect(0.5f, 0.5f, 0.7f, 0.7f), callback = h_attic_gameroom });
                foreach (var c in nodeConnections)
                {
                    if (
                        c.fromPos.node == u_attic && c.fromPos.facing == 'c'
                        &&
                        c.type == ClickboxType.EXITROOM)
                    {
                        //nodeConnections.Remove(c);
                        c.fromPos.node += 999;
                        break;
                    }
                }
            }
            else if (stage == 4 && ghost_sights == 0)
            {
                ghost_sights++;
                //advance_stage = 1;//need to require player clicking on elinor's door to trigger a sound
                //PlaySong("GU27", loop: false);
                QueueVideo("_ghost4", fps: 30);
                //fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFORSONG, callback = PlayMusic });
                nodeConnections.Remove(knoxdoor_con);
                nodeConnections.Add(knoxdoor_con);
                //chattering teeth on elinor's door, plays sound and sets advance_stage=1
                //stage 5, _ghost4 again, this time you can go into elinor's door, but it's actually the bathroom?
            }
            else if(stage==5 && ghost_sights == 0)
            {
                ghost_sights++;
                //advance_stage = 1;//need to require player to go through elinor's door to the bathroom
                //PlaySong("GU27", loop: false);
                QueueVideo("_ghost4", fps: 30);
                //fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFORSONG, callback = PlayMusic });
                nodeConnections.Remove(knoxdoor_con);
                knoxdoor_con.type = ClickboxType.EXITROOM;
                nodeConnections.Add(knoxdoor_con);
            }
            else if (stage == 7 && ghost_sights == 0)
            {
                ghost_sights++;
                //advance_stage = 1;
                PlaySong("GU27", loop: false);
                QueueVideo("_ghost3", fps: 60);
                fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITFORSONG, callback = PlayMusic });
                nodeConnections.Remove(hands_painting);
                nodeConnections.Add(hands_painting);
            }
            else if(stage>12)
            {
            }
            else if (stage > 0 && ghost_sights == 0)
            {
                ghost_sights++;
                advance_stage = 1;
                QueueVideo("_ghost1.avi");
            }
            else if (stage == 0)
                advance_stage = 1;
            //else
                //PlaySound("GAMWAV/8_s_12.avi");
            stairs_b_times++;
        }
        if (stage == 10 && currPos.node == u_dutton && currPos.facing == 'd' && ghost_sights == 1)
        {
            ghost_sights++;
            advance_stage = 1;
            QueueVideo("_ghost2");

            foreach (var c in nodeConnections)
            {
                if (
                    c.fromPos.node == u_attic+999 && c.fromPos.facing == 'c'
                    &&
                    c.type == ClickboxType.EXITROOM)
                {
                    //nodeConnections.Remove(c);
                    c.fromPos.node -= 999;
                    break;
                }
            }
        }
        if (currPos.node == u_stairs && currPos.facing == 'd')
        {
            //QueueVideo("b_");
        }
    }

    /*protected override void QueueVideo(string file, System.Action<FMVManager.Command> callback = null, float fadeIn = 0, bool wait = true, int fps = 15)
    {
        float speed = ((float)fps) / 15.0f;
        //fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + file, tags = "overlay other video", callback = callback, fadeInTime = fadeIn, playbackSpeed = speed, type = FMVManager.CommandType.OVERLAY }, wait);
        fmvman.QueueOverlay(new FMVManager.Command { file = myvidpath + file, tags = "overlay other video", callback = callback, fadeInTime = fadeIn, playbackSpeed = speed, type = FMVManager.CommandType.OVERLAY }, wait);
        //fmvman.ClearPlayingVideos("overlay");
    }

    protected override void QueueMovement(string file, bool wait = true, float speed = 1, string tags = "")
    {
        fmvman.QueueOverlay(new FMVManager.Command { file = myvidpath + file, tags = tags + " overlay movement", playbackSpeed = speed, type = FMVManager.CommandType.OVERLAY }, wait);
        //fmvman.ClearPlayingVideos("overlay");
    }

    protected override void FreezeMovement(string file, float fadeIn = 0)
    {
        fmvman.QueueOverlay(new FMVManager.Command { file = myvidpath + file, tags = "overlay movement", playbackSpeed = 1, freezeFrame = true, fadeInTime = fadeIn, type = FMVManager.CommandType.OVERLAY }, true);
        //fmvman.ClearPlayingVideos("overlay");
    }*/
}

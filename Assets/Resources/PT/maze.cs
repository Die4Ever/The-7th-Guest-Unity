using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class maze : baseRoom
{
    Color multColor = new Color(0.6f, 0.3f, 0.3f, 1);
    // Use this for initialization
    void Start () {
        BaseInit();
        myvidpath = "MC/m";

        QueueVideo("g_in.avi", fadeIn: 1);
        QueueVideo("g_thru.avi");
        PlaySound("GAMWAV/8_s_11.avi");
        QueueMovement("s.avi");
        QueueMovement("s.avi");
        QueueMovement("s.avi");
        QueueVideo("_ghostb.avi");
        QueueMovement("s.avi");
        //PlaySound("GAMWAV/8_s_9.avi");
        QueueVideo("_ghostl.avi");
        //PlaySound("GAMWAV/8_e_3.avi");
        QueueMovement("s.avi");
        PlaySound("GAMWAV/8_s_8.avi");
        QueueVideo("_ghostr.avi");
        QueueMovement("s.avi");
        PlaySound("GAMWAV/8_s_7.avi");//you're getting... warmer
        QueueMovement("s.avi");
        //PlaySound("GAMWAV/8_s_10.avi");//I was afraid of the dark when I was your age
        QueueMovement("s.avi");
        PlaySound("GAMWAV/8_s_15.avi");
        QueueMovement("y.avi");
        fmvman.QueueVideo(new FMVManager.Command { type= FMVManager.CommandType.WAITFORVIDEO, callback= ExitMaze });
    }
	
    void ExitMaze(FMVManager.Command c)
    {
        hallway1 h = fmvman.SwitchRoom("PT/hallway1", 8, 'b') as hallway1;
    }

	// Update is called once per frame
	void Update () {
        base.Update();
        videoScript[] vids = GameObject.FindObjectsOfType<videoScript>();
        foreach (videoScript v in vids)
        {
            v.multColor = multColor;
        }
    }
}

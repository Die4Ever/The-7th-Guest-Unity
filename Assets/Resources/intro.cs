using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intro : baseRoom {

	// Use this for initialization
	void Start () {
        BaseInit();
        /*fmvman.QueueSong("../oggs/track1.ogg");
        fmvman.QueueVideoFade("HDISK/vlogo.avi", 1, 5, 1);
        fmvman.QueueVideoFade("HDISK/tripro.avi");
        fmvman.playlist.Enqueue(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 5 });
        fmvman.QueueVideoFade("HDISK/title.avi", 1.0f, 0.0f, 0.5f);

        QueueVideo("INTRO/o1pa.avi");
        QueueVideo("INTRO/o1tu.avi");
        QueueVideo("INTRO/o3pa.avi");
        QueueVideo("INTRO/o3tu.avi");
        QueueVideo("INTRO/o4pa.avi");
        QueueVideo("INTRO/o4tu.avi");
        QueueVideo("INTRO/o5pa.avi");
        QueueVideo("INTRO/o5tu.avi");
        QueueVideo("INTRO/o6pa.avi");
        QueueVideo("INTRO/o6tu.avi");
        QueueVideo("INTRO/o7pa.avi");
        QueueVideo("INTRO/o7tu.avi");
        QueueVideo("INTRO/o8pa.avi");
        QueueVideo("INTRO/o8tu.avi");
        QueueVideo("INTRO/o9pa.avi");
        QueueVideo("INTRO/o9tu.avi");
        QueueVideo("INTRO/o10pa.avi");
        QueueVideo("INTRO/o10tu.avi");
        QueueVideo("INTRO/o12pa.avi");
        QueueVideo("LI/l_in.avi");
        QueueVideo("FH/f_5ba.avi");
        QueueVideo("FH/f5_1.avi");
        QueueVideo("FH/f1_.avi");
        QueueVideo("FH/f_1fa.avi");*/
        QueueVideo("FH/f_1fb.avi");
        fmvman.QueueSong("GAMWAV/1_e_1.avi");
        fmvman.playlist.Enqueue(new FMVManager.Command { type = FMVManager.CommandType.WAITFORSONG });
        fmvman.playlist.Enqueue(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 1 });
        fmvman.QueueSong("GAMWAV/1_e_2.avi");
        fmvman.playlist.Enqueue(new FMVManager.Command { type = FMVManager.CommandType.WAITFORSONG, callback = SwitchToFoyer });
        //fmvman.playlist.Enqueue(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 1 });
    }
	
	// Update is called once per frame
	/*void Update () {
		
	}*/

    void SwitchToFoyer(FMVManager.Command c)
    {
        Debug.Log("SwitchToFoyer");
        GameObject go = Instantiate(Resources.Load("foyer", typeof(GameObject))) as GameObject;
        foyer f = go.GetComponent<foyer>();
        f.currNode = 1;
        Destroy(this);
    }
}

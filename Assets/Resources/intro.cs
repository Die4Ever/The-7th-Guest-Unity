using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intro : baseRoom {

	// Use this for initialization
	void Start () {
        BaseInit();
        bool playIntro = false;
        if (playIntro)
        {
            PlaySound("../oggs/track1.ogg");
            fmvman.QueueVideo(new FMVManager.Command { file = "HDISK/vlogo.avi", fadeInTime=1, fadeOutTime=4, playbackSpeed=1 });
            fmvman.QueueVideo(new FMVManager.Command { file = "HDISK/tripro.avi", fadeInTime = 4, fadeOutTime = 4 });
            fmvman.QueueVideo(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 2 });
            fmvman.QueueVideo(new FMVManager.Command { file = "HDISK/title.avi", fadeInTime = 1.0f, fadeOutTime = 0.0f, playbackSpeed = 0.5f });

            PlaySong("GU71", false);
            QueueVideo("INTRO/o1pa.avi");
            QueueVideo("INTRO/o1tu.avi");
            QueueVideo("INTRO/o3pa.avi");
            QueueVideo("INTRO/o3tu.avi");
            QueueVideo("INTRO/o4pa.avi");
            QueueVideo("INTRO/o4tu.avi");
            QueueVideo("INTRO/o5pa.avi");

            PlaySong("GU72", false);
            QueueVideo("INTRO/o5tu.avi");
            QueueVideo("INTRO/o6pa.avi");
            QueueVideo("INTRO/o6tu.avi");
            QueueVideo("INTRO/o7pa.avi");
            QueueVideo("INTRO/o7tu.avi");

            PlaySong("GU73", false);
            QueueVideo("INTRO/o8pa.avi");
            QueueVideo("INTRO/o8tu.avi");
            QueueVideo("INTRO/o9pa.avi");
            QueueVideo("INTRO/o9tu.avi");
            QueueVideo("INTRO/o10pa.avi");
            QueueVideo("INTRO/o10tu.avi");
            QueueVideo("INTRO/o12pa.avi");

            QueueVideo("LI/l_in.avi");
            QueueVideo("FH/f_5ba.avi", null, 1);
            QueueVideo("FH/f5_1.avi");
            PlaySong("AGU32", false);
            QueueVideo("FH/f1_.avi");
            PlaySong("GU56");
            QueueVideo("FH/f_1fa.avi");
            QueueVideo("FH/f_1fb.avi");
            PlaySound("GAMWAV/1_e_1.avi");
            fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO, countdown = 1 });
            PlaySound("GAMWAV/1_e_2.avi");
            fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORAUDIO, callback = SwitchToFoyer, countdown = 1 });
        }
        else
        {
            QueueMovement("FH/f_1fb.avi");
            SwitchToFoyer(null);
        }
    }

    void SwitchToFoyer(FMVManager.Command c)
    {
        fmvman.SwitchRoom("foyer", 1, 'c');
    }
}

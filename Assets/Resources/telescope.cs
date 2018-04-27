using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class telescope : basePuzzle {

	// Use this for initialization
	void Start () {
        BaseInit();
        myvidpath = "LI/";
        PlaySound("GAMWAV/7_s_1.avi", wait:true, callback: EndPuzzle);
        PlaySong("GU17");
    }

    void EndPuzzle(FMVManager.Command c)
    {
        fmvman.ClearPlayingVideos("puzzle");
        if (endPuzzle != null) endPuzzle("cake");
        Destroy(this.gameObject);
    }
}

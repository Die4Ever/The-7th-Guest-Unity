using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cake : basePuzzle
{

	// Use this for initialization
	void Start () {
        BaseInit();
        myvidpath = "DR/dr";
        PlaySong("GU12");
        PlaySound("GAMWAV/3_s_1.avi", GraveDigger);
    }

    void GraveDigger(FMVManager.Command c)
    {
        PlaySound(there, ExitPuzzle);
    }

    void ExitPuzzle(FMVManager.Command c)
    {
        if (endPuzzle != null) endPuzzle("spiders");
        Destroy(this);
    }

}

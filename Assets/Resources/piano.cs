using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class piano : basePuzzle
{
	// Use this for initialization
	void Start () {
        BaseInit("piano");
        myvidpath = "MU/";
        PlaySong("GU17");//which song?
        EndPiano(null);
    }

    void EndPiano(FMVManager.Command c)
    {
        WinPuzzle();
    }
}

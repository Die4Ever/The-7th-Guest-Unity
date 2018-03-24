using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiders : basePuzzle {

    //spider moving files are f_a_d.avi to f_h_e.avi, with a being the top position and b being the position to the left, going counter-clockwise
    //foy_spa is the video file for a spider appearing at point a
    //audio
    //2_s_2 (intro, play from foyer?), 2_s_3, 2_e_4, 2_e_3, gen_s_2("curses!"), gen_e_8 ("which way should I go now?")
    // Use this for initialization
    int currSpider = -1;
    bool[] spiderSpots;

    void Start () {
        BaseInit();
        myvidpath = "FH/f";
        //fmvman.QueueSong(whichway);

        AddPuzzlePoint("a", new Rect(0.46f, 0.82f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("b", new Rect(0.33f, 0.72f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("c", new Rect(0.28f, 0.49f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("d", new Rect(0.32f, 0.26f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("e", new Rect(0.46f, 0.15f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("f", new Rect(0.6f, 0.25f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("g", new Rect(0.66f, 0.49f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePoint("h", new Rect(0.6f, 0.72f, 0.05f, 0.05f), ClickPoint);
        spiderSpots = new bool[8];
        Debug.Log(spiderSpots[0]);
    }
	
	// Update is called once per frame
	/*void Update () {
        fmvman.QueueSong("GAMWAV/gen_s_2", true);
        endPuzzle("end");
        Destroy(this);
    }*/

    bool SpotHasSpider(char spot)
    {
        int i = (int)spot - (int)'a';
        return spiderSpots[i];
    }

    void SpotAddSpider(char spot)
    {
        int i = (int)spot - (int)'a';
        spiderSpots[i] = true;
    }

    int SpotNameToInt(string spot)
    {
        return (int)spot[0] - (int)'a';
    }

    void ClickPoint(PuzzlePoint pp)
    {
        int spot = SpotNameToInt(pp.name);
        if(spiderSpots[spot])
        {
            Debug.Log("spot already filled");
            return;
        }
        if (currSpider == -1 || true)
        {
            var f = Instantiate(fmvman);
            f.baseVideoPlayer=fmvman.baseVideoPlayer;
            f.QueueOverlayCallback(myvidpath + "oy_sp" + pp.name + ".avi", SpiderAppeared, new Color(0,0,0,1));
            currSpider = spot;
        } else
        {
            currSpider = -1;
        }
    }

    void SpiderAppeared(FMVManager.Command c)
    {
        /*int spot = SpotNameToInt(c.file.Substring(c.file.Length-4, 1));
        Debug.Log(c.file+" spot: "+spot.ToString());*/
        fmvman.QueueSong(whichway);
    }
}

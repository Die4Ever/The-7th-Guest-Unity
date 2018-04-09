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

        AddPuzzlePointCentered("00", new Rect(0.26f, 0.84f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("01", new Rect(0.25f, 0.76f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("02", new Rect(0.22f, 0.65f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("03", new Rect(0.19f, 0.53f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("04", new Rect(0.15f, 0.34f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("05", new Rect(0.34f, 0.85f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("06", new Rect(0.32f, 0.76f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("07", new Rect(0.31f, 0.66f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("08", new Rect(0.29f, 0.53f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("09", new Rect(0.27f, 0.37f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("10", new Rect(0.41f, 0.85f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("11", new Rect(0.40f, 0.78f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("12", new Rect(0.40f, 0.67f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("13", new Rect(0.39f, 0.55f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("14", new Rect(0.38f, 0.38f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("15", new Rect(0.48f, 0.86f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("16", new Rect(0.48f, 0.79f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("17", new Rect(0.48f, 0.68f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("18", new Rect(0.48f, 0.56f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("19", new Rect(0.49f, 0.40f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("20", new Rect(0.54f, 0.87f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("21", new Rect(0.55f, 0.79f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("22", new Rect(0.57f, 0.70f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("23", new Rect(0.58f, 0.57f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("24", new Rect(0.60f, 0.41f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("25", new Rect(0.61f, 0.88f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("26", new Rect(0.62f, 0.80f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("27", new Rect(0.64f, 0.71f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("28", new Rect(0.68f, 0.59f, 0.05f, 0.05f), ClickPoint);
        AddPuzzlePointCentered("29", new Rect(0.71f, 0.43f, 0.05f, 0.05f), ClickPoint);

        fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITFORVIDEO });
        for (int i=0;i<30;i++)
        {
            QueueOverlay(myvidpath+i.ToString("00")+"df.avi", null, new Color(0, 0, 251.0f/255.0f, 1), "cake-"+i.ToString(), false, true, GetZ(i), 0.5f, 0.5f);
            //fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 0.01f });
        }
        fmvman.playlist.Add(new FMVManager.Command { type= FMVManager.CommandType.WAITTIME, countdown=1 });
        QueueVideo("_tray.avi");
    }

    float GetZ(int slot)
    {
        float z = -(float)slot / 1000.0f;
        if (slot > 19) z += 0.1f;
        if (slot > 24) z += 0.1f;
        return z;
    }

    void GraveDigger(FMVManager.Command c)
    {
        //PlaySound(there, ExitPuzzle);
    }

    void ClickPoint(PuzzlePoint pp)
    {
        if (pp == null) return;
    }

        void ExitPuzzle(FMVManager.Command c)
    {
        fmvman.ClearPlayingVideos("puzzle");
        if (endPuzzle != null) endPuzzle("spiders");
        Destroy(this);
    }

}

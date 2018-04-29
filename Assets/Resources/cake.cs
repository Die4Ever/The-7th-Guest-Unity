using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cake : basePuzzle
{
    int[,] slices = new int[6, 5];
    int[,] slice_types;
    const int initial = 0;
    const int selected = 1;
    const int done = 2;
    const int skull = 1;
    const int stone = 2;
    const int icing = 3;

    int num_selected = 0;
    int[] types_selected = new int[4] { 0, 0, 0, 0 };
	// Use this for initialization
	void Start () {
        BaseInit("cake");
        myvidpath = "DR/dr";
        PlaySong("GU12");
        PlaySound("GAMWAV/3_s_1.avi", GraveDigger);

        slice_types = new int[6, 5] {//this is actually sideways, top to bottom
            { skull, icing, icing, skull, stone },//left column
            { stone, stone, stone, skull, stone },//2nd column
            { stone, skull, stone, stone, skull },//3rd column
            { skull, skull, icing, icing, icing },//4th column
            { stone, stone, skull, skull, skull },//5th column
            { skull, skull, icing, stone, stone } };//right column

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
            QueueOverlay(myvidpath+i.ToString("00")+"df.avi", null, new Color(0, 0, 251.0f/255.0f, 1), "start-cake-"+i.ToString("00"), false, true, GetZ(i), 0.5f, 0.5f);
            //QueueOverlay(myvidpath + i.ToString("00") + "db.avi", null, new Color(0, 0, 251.0f / 255.0f, 1), "cake-" + i.ToString("00"), false, false, GetZ(i), 0.5f, 0.5f);
            SetSlice(i, initial);
            //fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 0.01f });
        }
        fmvman.playlist.Add(new FMVManager.Command { type= FMVManager.CommandType.WAITTIME, countdown=1 });
        QueueVideo("_tray.avi");
    }

    int GetSlice(int slot)
    {
        return GetSlice(GetCoords(slot));
    }

    int GetSlice(int x, int y)
    {
        if (x < 0 || x > 5 || y < 0 || y > 4) return done;//a done piece is unavailable for selecting and not helpful for building a group
        return slices[x, y];
    }

    int GetSliceType(int x, int y)
    {
        if (x < 0 || x > 5 || y < 0 || y > 4) return 0;
        return slice_types[x, y];
    }

    int GetSlice(Vector2Int coords)
    {
        return GetSlice(coords.x, coords.y);
    }

    int GetSlot(int x, int y)
    {
        return x * 5 + y;
    }

    void SetSlice(int slot, int val)
    {
        slices[slot / 5, slot % 5] = val;
    }

    Vector2Int GetCoords(int slot)
    {
        return new Vector2Int(slot / 5, slot % 5);
    }

    bool IsLegal(Vector2Int coords)
    {
        if (num_selected == 0) return true;
        if (GetSlice(coords) == selected) return false;//we'll add unselecting later...
        int x = coords.x;
        int y = coords.y;
        if (GetSlice(x - 1, y) != selected && GetSlice(x + 1, y) != selected && GetSlice(x, y - 1) != selected && GetSlice(x, y + 1) != selected) return false;
        int type = GetSliceType(x, y);
        if (type == icing && types_selected[icing] > 0) return false;
        if (types_selected[type] > 1) return false;
        return true;
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
        int slot = System.Int32.Parse(pp.name);
        Vector2Int coords = GetCoords(slot);

        if(IsLegal(coords))
        {
            int type = GetSliceType(coords.x, coords.y);
            types_selected[type]++;
            num_selected++;
            SetSlice(slot, selected);
            //fmvman.ClearPlayingVideos("cake-"+slot.ToString("00"));
            QueueOverlay(myvidpath + slot.ToString("00") + "df.avi", CakeUp, new Color(0, 0, 251.0f / 255.0f, 1), "select-cake-" + slot.ToString("00"), false, false, GetZ(slot), 0.5f, 0.5f);
            fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 0.1f });
            fmvman.playlist.Add(new FMVManager.Command { type= FMVManager.CommandType.CLEARVIDEOS, tags= "start-cake-" + slot.ToString("00") });
            //1st selection, Ego says "2 skulls, and 2 stones, the rest is just icing"
            //2nd selection, Ego says "puzzling"
            //3rd, Ego says "some cannot be created more equally than others"
        }
        //Debug.Log("clicked " + slot.ToString("00")+", ("+coords.ToString()+")");
    }

    void CakeUp(FMVManager.Command c)
    {
        if (types_selected[skull] < 2 || types_selected[stone] < 2 || types_selected[icing] < 1)
            return;
        for(int x=0;x<6;x++)
        {
            for(int y=0;y<5;y++)
            {
                if(GetSlice(x, y)==selected)
                {
                    int slot = GetSlot(x, y);
                    //fmvman.ClearPlayingVideos("cake-" + slot.ToString("00"));
                    QueueOverlay(myvidpath + slot.ToString("00") + "cf.avi", CakeAway, new Color(0, 0, 251.0f / 255.0f, 1), "away-cake-" + slot.ToString("00"), false, false, GetZ(slot), 0.5f, 0.5f);
                    fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.WAITTIME, countdown = 0.1f });
                    fmvman.playlist.Add(new FMVManager.Command { type = FMVManager.CommandType.CLEARVIDEOS, tags = "select-cake-" + slot.ToString("00") });
                    SetSlice(slot, done);
                }
            }
        }
        num_selected = 0;
        types_selected = new int[4] { 0, 0, 0, 0 };
    }

    void CakeAway(FMVManager.Command c)
    {
        fmvman.ClearPlayingVideos(c.tags);

        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                if (GetSlice(x, y) != done) return;
            }
        }
        //Ego says "there"
        WinPuzzle();
    }
}

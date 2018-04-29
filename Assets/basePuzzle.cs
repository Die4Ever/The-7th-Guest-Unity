using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basePuzzle : MonoBehaviour
{
    protected static Texture2D currcursor;
    protected FMVManager fmvman;
    protected string myvidpath;
    string PuzzleName;
    protected string whichway = "GAMWAV/gen_e_8.avi";
    protected string there = "GAMWAV/gen_e_2.avi";
    protected class PuzzlePoint
    {
        public Rect clickbox;
        public string name;
        public System.Action<PuzzlePoint> callback = null;
    };
    protected List<PuzzlePoint> puzzlePoints;
    public System.Action<string> endPuzzle = null;

    protected void BaseInit(string puzzleName)
    {
        PuzzleName = puzzleName;
        if (puzzlePoints != null)
        {
            Debug.Log("double init?");
            return;
        }
        puzzlePoints = new List<PuzzlePoint>();

        fmvman = GameObject.FindObjectOfType<FMVManager>();
        SetCursor(fmvman.handwag);
    }

    protected void SetCursor(Texture2D c)
    {
        if (currcursor == c) return;
        //Debug.Log("changing cursor");
        Cursor.SetCursor(c, Vector2.zero, CursorMode.Auto);
        currcursor = c;
    }

    // Use this for initialization
    void Start()
    {

    }

    protected void WinPuzzle()
    {
        fmvman.IncrementVariable("beatpuzzle-" + PuzzleName);
        fmvman.ClearPlayingVideos("puzzle");
        if (endPuzzle != null) endPuzzle(PuzzleName);
        Destroy(this.gameObject);
    }

    protected void RestartPuzzle()
    {
        fmvman.IncrementVariable("restartpuzzle-" + PuzzleName);
    }

    protected void LeavePuzzle()
    {
        fmvman.IncrementVariable("leavepuzzle-" + PuzzleName);
        fmvman.ClearPlayingVideos("puzzle");
        if (endPuzzle != null) endPuzzle(PuzzleName);
        Destroy(this.gameObject);
    }

    protected PuzzlePoint GetPuzzlePoint(Vector2 pos)
    {
        foreach (var pp in puzzlePoints)
        {
            if (pp.clickbox.Contains(pos))
            {
                return pp;
            }
        }
        return null;
    }

    protected void AddPuzzlePoint(string name, Rect clickbox, System.Action<PuzzlePoint> callback)
    {
        puzzlePoints.Add(new PuzzlePoint { clickbox = clickbox, name = name, callback = callback });
    }

    protected void AddPuzzlePointCentered(string name, Rect clickbox, System.Action<PuzzlePoint> callback)
    {
        clickbox.position = new Vector2(clickbox.position.x - clickbox.width / 2, clickbox.position.y - clickbox.height / 2);
        Debug.Log(clickbox.ToString("0.00"));
        AddPuzzlePoint(name, clickbox, callback);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = fmvman.ScreenToVideo(Input.mousePosition);//Camera.main.ScreenToViewportPoint(Input.mousePosition);
        PuzzlePoint pp = GetPuzzlePoint(pos);
        int puzzle_videos = fmvman.CountPlayingVideos("puzzle");
        //if (puzzle_videos > 0) Debug.Log("puzzle_videos == "+puzzle_videos.ToString());
        if (puzzle_videos > 0) pp = null;
        if (pp != null) SetCursor(fmvman.blueeye);
        else SetCursor(fmvman.handwag);
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("clicked at "+pos.ToString("0.00"));
            if (pp != null) OnClick(pos, pp);
        }
    }

    protected void OnClick(Vector2 pos, PuzzlePoint pp)
    {
        Debug.Log("clicked " + pp.name + ", at " + pos.ToString("0.00"));
        if (pp.callback != null) pp.callback(pp);
    }

    protected void OnDestroy()
    {
        fmvman.ClearQueue("puzzle");
    }

    protected void QueueVideo(string file)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + file, tags = "puzzle" });
    }

    protected void QueueOverlay(string file, System.Action<FMVManager.Command> callback, Color transparentColor, string tags = "", bool wait = false, bool freezeFrame = false, float z = 0, float threshold = 0.1f, float slope = 0.5f)
    {
        fmvman.QueueOverlay(new FMVManager.Command { file = file, callback = callback, transparentColor = transparentColor, type = FMVManager.CommandType.OVERLAY, tags = tags + " puzzle", freezeFrame = freezeFrame, z = z, threshold = threshold, slope = slope }, wait);
    }

    protected void QueueMovement(string file)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + file, tags = "puzzle" });
    }

    protected void PlaySong(string file, bool loop = true)
    {
        fmvman.PlaySong(new FMVManager.Command { file = "../music/" + file + ".ogg", type = FMVManager.CommandType.SONG, tags = "", loop = loop });
    }

    protected void PlaySound(string file, System.Action<FMVManager.Command> callback = null, bool wait = false, bool clear = true)
    {
        if (clear)
        {
            fmvman.ClearPlayingAudio("puzzle");
        }
        fmvman.PlayAudio(new FMVManager.Command { file = file, type = FMVManager.CommandType.AUDIO, tags = "puzzle", callback = callback }, wait);
    }
}

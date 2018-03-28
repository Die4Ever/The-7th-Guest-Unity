using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basePuzzle : MonoBehaviour
{
    protected static Texture2D currcursor;
    protected FMVManager fmvman;
    protected string myvidpath;
    protected string whichway = "GAMWAV/gen_e_8.avi";
    protected class PuzzlePoint
    {
        public Rect clickbox;
        public string name;
        public System.Action<PuzzlePoint> callback = null;
    };
    protected List<PuzzlePoint> puzzlePoints;
    public System.Action<string> endPuzzle = null;

    protected void BaseInit()
    {
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
        puzzlePoints.Add(new PuzzlePoint { clickbox=clickbox, name=name, callback=callback });
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        PuzzlePoint pp = GetPuzzlePoint(pos);
        if (pp != null) SetCursor(fmvman.blueeye);
        else SetCursor(fmvman.handwag);
        if (Input.GetMouseButtonDown(0))
        {
            //Debug.Log("clicked at "+pos.ToString("0.00"));
            if(pp!=null) OnClick(pos, pp);
        }
    }

    protected void OnClick(Vector2 pos, PuzzlePoint pp)
    {
        Debug.Log("clicked " + pp.name+", at "+pos.ToString());
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

    protected void QueueMovement(string file)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + file, tags = "puzzle" });
    }

    protected void PlaySong(string file)
    {
        fmvman.PlaySong(new FMVManager.Command { file = file, type = FMVManager.CommandType.SONG, tags = "puzzle" });
    }

    protected void PlaySound(string file)
    {
        fmvman.PlaySong(new FMVManager.Command { file = file, type = FMVManager.CommandType.AUDIO, tags = "puzzle" });
    }
}

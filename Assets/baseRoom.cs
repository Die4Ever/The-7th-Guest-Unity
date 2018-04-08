using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseRoom : MonoBehaviour {
    protected static Texture2D currcursor;
    protected FMVManager fmvman;
    protected string myvidpath;
    //filenames for rotation seem to be f_4fc where the f=foyer, 4=the node number, f=forwards (b=backwards) or clockwise, the last letter is a/b/c/d and it means the orientation
    //for movement it's f1_2 where f=foyer, 1=starting node number, 2=destination node number
    //include the filename prefix in myvidpath, some are 1 letter (like f for foyer) and some are 2 letters (like bd for brian dutton)
    //not of all these seem to follow those rules, I think I'll have to use nodeNames to lookup filenames
    protected string[] nodeNames;//for convenience? I can just leave them as comments, but then I can't show them in debug output
    //string[] facingNames;
    public class RoomPosition
    {
        public int node;
        public char facing;//0=a, 1=b, 2=c, 3=d...?
        public string filename;

        public RoomPosition(int n, char f, string Filename="")
        {
            node = n;
            facing = f;
            filename = Filename;
        }
    };
    public RoomPosition currPos = new RoomPosition(1, 'a');
    //public int currNode=1;
    //public char facing;//0=a, 1=b, 2=c, 3=d...
    public enum ClickboxType { MOVE, TURN, EXITROOM, PUZZLE, DRAMAMASK, CHATTERINGTEETH };
    protected class NodeConnection
    {
        public RoomPosition fromPos;
        public Rect clickbox;
        public ClickboxType type;
        public RoomPosition[] toPos;
        public int timesClicked = 0;
        public System.Action<NodeConnection> callback = null;
        public float speed = 1;
    };
    protected List<NodeConnection> nodeConnections;

    protected void BaseInit()
    {
        if (nodeConnections != null)
        {
            Debug.Log("double init?");
            return;
        }

        fmvman = GameObject.FindObjectOfType<FMVManager>();
        //Debug.Log(fmvman.ToString());
        nodeConnections = new List<NodeConnection>();
        currPos.node = 1;

        SetCursor(fmvman.handwag);
    }

	// Use this for initialization
	void Start () {
        BaseInit();
    }

    protected void SetCursor(Texture2D c)
    {
        if (currcursor == c) return;
        //Debug.Log("changing cursor");
        Cursor.SetCursor(c, Vector2.zero, CursorMode.Auto);
        currcursor = c;
    }

    protected void Update()
    {
        if(fmvman.playlist.Count>0 || fmvman.CountPlayingVideos("") > 0)
        {
            SetCursor(fmvman.handwag);
            return;
        }
        Vector2 pos = fmvman.ScreenToVideo(Input.mousePosition);// Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var nc = GetNodeConnection(pos);
        if (nc == null)
        {
            SetCursor(fmvman.handwag);
        }
        else if (nc.type == ClickboxType.DRAMAMASK)
        {
            SetCursor(fmvman.dramamask);
        }
        else if (nc.type == ClickboxType.CHATTERINGTEETH)
        {
            SetCursor(fmvman.chatteringteeth);
        }
        else if (nc.type == ClickboxType.PUZZLE)
        {
            SetCursor(fmvman.throbbingbrain);
        }
        else
        {
            SetCursor(fmvman.handbeckon);
        }

        if (Input.GetAxis("Horizontal") < -0.2f)
        {
            Vector2 fakeClickPos = new Vector2(-6, 0.5f);
            nc = GetNodeConnection(fakeClickPos);
            OnClick(fakeClickPos, nc);
        } else if (Input.GetAxis("Horizontal") > 0.2f)
        {
            Vector2 fakeClickPos = new Vector2(6, 0.5f);
            nc = GetNodeConnection(fakeClickPos);
            OnClick(fakeClickPos, nc);
        }
        else if (Input.GetAxis("Vertical") < -0.2f)
        {
            Vector2 fakeClickPos = new Vector2(0.5f, -6);//need to create the way bottom clickbox for turning around
            nc = GetNodeConnection(fakeClickPos);
            OnClick(fakeClickPos, nc);
        }
        else if (Input.GetAxis("Vertical") > 0.2f)
        {
            Vector2 fakeClickPos = new Vector2(0.5f, 0.5f);//center of the screen
            nc = GetClosestNodeConnection(fakeClickPos);
            OnClick(fakeClickPos, nc);
        }

        else if (Input.GetMouseButtonDown(0))
        {
            OnClick(pos, nc);
        }
        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("right clicky! " + pos.ToString("0.00") + " from " + currPos.node.ToString() + " " + currPos.facing);
        }
    }

    protected void CreateNodeConnection(RoomPosition fromPos, RoomPosition toPos, Rect clickbox, RoomPosition[] toPosArray=null, float speed=1)
    {
        var nc = new NodeConnection { fromPos=fromPos, clickbox = clickbox, type=ClickboxType.MOVE, speed = speed };
        if(toPosArray != null)
        {
            nc.toPos = toPosArray;
        } else
        {
            nc.toPos = new RoomPosition[1];
            nc.toPos[0] = toPos;
        }
        if (fromPos.node == nc.toPos[nc.toPos.Length-1].node) nc.type = ClickboxType.TURN;
        nodeConnections.Add( nc );
        MakeClickboxes();
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, char fromFacing, char toFacing, char before, char after)
    {
        RoomPosition fromPos = new RoomPosition(from, fromFacing);
        RoomPosition[] toPosArray = new RoomPosition[3] { new RoomPosition(from, before), new RoomPosition(to, toFacing), new RoomPosition(to, after) };
        CreateNodeConnection(fromPos, toPosArray[0], clickbox, toPosArray);
    }

    protected void CreateNodeConnectionRotations(int from, char fromFacing, char toFacing)
    {
        Rect left = new Rect(-10, 0, 10.02f, 1.0f);
        Rect right = new Rect(0.98f, 0, 10.02f, 1.0f);
        Rect turnaround = new Rect(0, -10, 1, 5);
        if(false)//if disable click to turn, and only use arrow keys or WASD...will need a config setting for this later
        {
            left = new Rect(-5, 0, 10.02f, 1.0f);
            right = new Rect(5, 0, 10.02f, 1.0f);
        }

        for (char f=fromFacing; f<toFacing;f++)
        {
            RoomPosition posLeft = new RoomPosition(from, f);
            RoomPosition posRight = new RoomPosition(from, (char)((int)f + 1));
            RoomPosition posLeftLeft = new RoomPosition(from, (char)((int)f - 1));
            if ((int)posLeftLeft.facing < (int)fromFacing) posLeftLeft.facing = toFacing;
            CreateNodeConnection(posRight, posLeft, left);
            CreateNodeConnection(posLeft, posRight, right);
            //CreateNodeConnection(posRight, posLeftLeft, turnaround);
            if(toFacing-fromFacing>2) CreateNodeConnection(posRight, null, turnaround, new RoomPosition[] { posLeft, posLeftLeft }, 2);
        }
        RoomPosition fromPos = new RoomPosition(from, fromFacing);
        RoomPosition toPos = new RoomPosition(from, toFacing);
        CreateNodeConnection(fromPos, toPos, left);
        CreateNodeConnection(toPos, fromPos, right);

        if (toFacing - fromFacing > 2)
        {
            RoomPosition posLeft = new RoomPosition(from, (char)((int)fromFacing - 1));
            if ((int)posLeft.facing < (int)fromFacing) posLeft.facing = toFacing;
            RoomPosition posLeftLeft = new RoomPosition(from, (char)((int)posLeft.facing - 1));
            if ((int)posLeft.facing < (int)fromFacing) posLeftLeft.facing = toFacing;
            CreateNodeConnection(fromPos, null, turnaround, new RoomPosition[] { posLeft, posLeftLeft }, 2);
        }
    }

    void MakeClickboxes()
    {
        return;
    }

    protected NodeConnection GetNodeConnection(Vector2 pos)
    {
        foreach (var nc in nodeConnections)
        {
            //Debug.Log(nc.ToString());
            if (nc.fromPos.node == currPos.node && nc.fromPos.facing==currPos.facing && nc.clickbox.Contains(pos))
            {
                return nc;
            }
        }
        return null;
    }

    protected NodeConnection GetClosestNodeConnection(Vector2 pos)
    {
        float closestDist = 9999;
        NodeConnection closest = null;

        foreach (var nc in nodeConnections)
        {
            //Debug.Log(nc.ToString());
            if (nc.fromPos.node == currPos.node && nc.fromPos.facing == currPos.facing)
            {
                if(nc.clickbox.Contains(pos)) return nc;
                float dx = Mathf.Min( Mathf.Abs(pos.x-nc.clickbox.xMin), Mathf.Abs(pos.x - nc.clickbox.xMax));
                float dy = Mathf.Min(Mathf.Abs(pos.y - nc.clickbox.yMin), Mathf.Abs(pos.y - nc.clickbox.yMax));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if(dist<closestDist)
                {
                    closestDist = dist;
                    closest = nc;
                }
            }
        }
        return closest;
    }

    //public void Travel(int to, char toFacing)
    public void Travel(RoomPosition to)
    {
        Debug.Log("going from node " + currPos.node.ToString() + "-"+currPos.facing+" to " + to.node.ToString()+"-"+to.facing);
        if(to.filename.Length>0)
        {
            Debug.Log("using manual filename "+ myvidpath + to.filename);
            QueueMovement(to.filename, true, to.speed);
        }
        else if (currPos.node != to.node)
        {
            //Debug.Log("going from node " + currNode.ToString() + " to " + to.ToString());
            QueueMovement(currPos.node.ToString() + "_" + to.node.ToString() + ".avi", true, to.speed);
        }
        else //rotation
        {
            if (currPos.facing + 1 == to.facing || currPos.facing > to.facing + 1)//f
            {
                QueueMovement("_" + currPos.node.ToString() + "f" + currPos.facing + ".avi", true, to.speed);//the underscore won't always be there?
            }
            else //b
            {
                QueueMovement("_" + currPos.node.ToString() + "b" + to.facing + ".avi", true, to.speed);//the underscore won't always be there?
            }
        }
        currPos = to;
    }

    protected void OnClick(Vector2 pos, NodeConnection nc)
    {
        Debug.Log("clicky! "+pos.ToString("0.00")+" from "+ currPos.node.ToString()+" "+ currPos.facing);
        if (fmvman.playlist.Count > 0)
        {
            //Debug.Log("speeeed boost! queue full");
            /*var p = fmvman.playing_videos;
            foreach(var c in p)
            {
                c.playbackSpeed = 4;
                if(c.player && c.player.GetComponent<UnityEngine.Video.VideoPlayer>())
                {
                    c.player.GetComponent<UnityEngine.Video.VideoPlayer>().playbackSpeed = 4;
                }
            }*/
            //return;
        }
        /*if(fmvman.playlist.Count > 3)
        {
            Debug.Log("queue full!");
            return;
        }*/

        //var nc = GetNodeConnection(pos);
        if(nc!=null)
        {
            nc.timesClicked++;
            if(nc.toPos!=null) foreach(var f in nc.toPos)
                {
                    if(currPos!=f) Travel(f);
                }
            if(nc.callback!=null)
            {
                nc.callback(nc);
            }
        }
    }

    protected void SetPosition(int node)
    {
        currPos.node = node;
        MakeClickboxes();
    }

    protected void QueueVideo(string file, System.Action<FMVManager.Command> callback=null, float fadeIn=0)
    {
        fmvman.QueueVideo(new FMVManager.Command { file=myvidpath + file, tags="movement", callback = callback, fadeInTime=fadeIn });
    }

    protected void QueueMovement(string file, bool wait=true, float speed=1)
    {
        fmvman.QueueVideo(new FMVManager.Command { file = myvidpath + file, tags = "movement", playbackSpeed = speed }, wait);
    }

    protected void PlaySong(string file, bool loop=true)
    {
        fmvman.PlaySong(new FMVManager.Command { file = file, type=FMVManager.CommandType.SONG, tags = "room", loop = loop });
    }

    protected void PlaySound(string file)
    {
        fmvman.PlayAudio(new FMVManager.Command { file = file, type=FMVManager.CommandType.AUDIO, tags = "room" }, false);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseRoom : MonoBehaviour {
    public static Texture2D handbeckon;
    public static Texture2D handwag;
    protected static Texture2D currcursor;
    protected FMVManager fmvman;
    protected string myvidpath;
    //filenames for rotation seem to be f_4fc where the f=foyer, 4=the node number, f=forwards (b=backwards) or clockwise, the last letter is a/b/c/d and it means the orientation
    //for movement it's f1_2 where f=foyer, 1=starting node number, 2=destination node number
    //include the filename prefix in myvidpath, some are 1 letter (like f for foyer) and some are 2 letters (like bd for brian dutton)
    //not of all these seem to follow those rules, I think I'll have to use nodeNames to lookup filenames
    protected string[] nodeNames;//for convenience? I can just leave them as comments, but then I can't show them in debug output
    string[] facingNames;
    public int currNode=1;
    public char facing;//0=a, 1=b, 2=c, 3=d...
    protected class NodeConnection
    {
        public int from;
        public int to;
        public char fromFacing;
        public char toFacing;
        public Rect clickbox;
        //public int[] through;//optional
        public char[] before;
        public char[] after;
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
        currNode = 1;

        handwag = Instantiate(Resources.Load("cursors/wagging-hand", typeof(Texture2D))) as Texture2D;
        handbeckon = Instantiate(Resources.Load("cursors/beckon", typeof(Texture2D))) as Texture2D;
        SetCursor(handwag);
    }

	// Use this for initialization
	void Start () {
        BaseInit();
    }

    protected void SetCursor(Texture2D c)
    {
        if (currcursor == c) return;
        Cursor.SetCursor(c, Vector2.zero, CursorMode.Auto);
        currcursor = c;
    }

    protected void Update()
    {
        Vector2 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        var nc = GetNodeConnection(pos);
        if(nc==null)
        {
            SetCursor(handwag);
        } else
        {
            SetCursor(handbeckon);
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnClick(pos);
        }
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, char fromFacing, char toFacing, char[] before=null, char[] after=null)
    {
        nodeConnections.Add( new NodeConnection { from=from, to=to, clickbox=clickbox, fromFacing=fromFacing, toFacing=toFacing, before=before, after=after } );
        MakeClickboxes();
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, char fromFacing, char toFacing, char before, char after)
    {
        CreateNodeConnection(from, to, clickbox, fromFacing, toFacing, new char[] { before }, new char[] { after });
    }

    protected void CreateNodeConnectionRotations(int from, char fromFacing, char toFacing)
    {
        Rect left = new Rect(0, 0, 0.02f, 1.0f);
        Rect right = new Rect(0.98f, 0, 0.02f, 1.0f);

        for(char f=fromFacing; f<toFacing;f++)
        {
            CreateNodeConnection(from, from, left, (char)((int)f + 1), f);
            CreateNodeConnection(from, from, right, f, (char)((int)f + 1));
        }
        CreateNodeConnection(from, from, left, fromFacing, toFacing);
        CreateNodeConnection(from, from, right, toFacing, fromFacing);
    }

    void MakeClickboxes()
    {
        return;
        foreach (var nc in nodeConnections)
        {
            if (nc.from == currNode)
            {
                //debug option to draw them on the screen
                Vector3 topleft = new Vector3(nc.clickbox.xMin, nc.clickbox.yMin, 0);
                Vector3 bottomright = new Vector3(nc.clickbox.xMax, nc.clickbox.yMax, 0);
                Debug.Log(topleft.ToString());
                Debug.Log(bottomright.ToString());
                topleft = Camera.main.ViewportToScreenPoint(topleft);
                bottomright = Camera.main.ViewportToScreenPoint(bottomright);
                topleft = Camera.main.ScreenToWorldPoint(topleft);
                bottomright = Camera.main.ScreenToWorldPoint(bottomright);
                topleft.z = 0;
                bottomright.z = 0;
                Debug.Log(topleft.ToString());
                Debug.Log(bottomright.ToString());
                GameObject q = Instantiate(Resources.Load("Quad")) as GameObject;
                //q.transform.localScale = (bottomright - topleft);
                q.transform.position = (topleft + bottomright) / 2;
                Debug.Log(q.transform.position.ToString());
            }
        }
    }

    protected NodeConnection GetNodeConnection(Vector2 pos)
    {
        foreach (var nc in nodeConnections)
        {
            //Debug.Log(nc.ToString());
            if (nc.from == currNode && nc.fromFacing==facing && nc.clickbox.Contains(pos))
            {
                return nc;
            }
        }
        return null;
    }

    public void Travel(int to, char toFacing)
    {
        Debug.Log("going from node " + currNode.ToString() + "-"+facing+" to " + to.ToString()+"-"+toFacing);
        if (currNode != to)
        {
            //Debug.Log("going from node " + currNode.ToString() + " to " + to.ToString());
            fmvman.QueueVideo(myvidpath + currNode.ToString() + "_" + to.ToString() + ".avi");
            currNode = to;
            facing = toFacing;
        }
        else //rotation
        {
            if (facing + 1 == toFacing || facing > toFacing + 1)//f
            {
                fmvman.QueueVideo(myvidpath + "_" + currNode.ToString() + "f" + facing + ".avi");//the underscore won't always be there?
            }
            else //b
            {
                fmvman.QueueVideo(myvidpath + "_" + currNode.ToString() + "b" + toFacing + ".avi");//the underscore won't always be there?
            }
            facing = toFacing;
        }
    }

    public void OnClick(Vector2 pos)
    {
        Debug.Log("clicky! "+pos.ToString()+" from "+currNode.ToString()+" "+facing);
        if (fmvman.playlist.Count > 1)
        {
            Debug.Log("speeeed boost!");
            var p = fmvman.playlist.ToArray();
            fmvman.playlist.Clear();
            foreach(var c in p)
            {
                c.playbackSpeed = 4;
                fmvman.playlist.Enqueue(c);
            }
            //return;
        }
        if(fmvman.playlist.Count > 3)
        {
            Debug.Log("queue full!");
            return;
        }

        var nc = GetNodeConnection(pos);
        if(nc!=null)
        {
            if(nc.before!=null) foreach(var f in nc.before)
            {
                if(f!=facing) Travel(currNode, f);
            }
            Travel(nc.to, nc.toFacing);
            if(nc.after!=null) foreach(var f in nc.after)
            {
                if(f!=facing) Travel(currNode, f);
            }
            /*if (nc.from != nc.to)
            {
                Debug.Log("going from node " + currNode.ToString() + " to " + nc.to.ToString());
                currNode = nc.to;
                facing = nc.toFacing;
                fmvman.QueueVideo(myvidpath + nc.from.ToString() + "_" + nc.to.ToString() + ".avi");
            } else //rotation
            {
                if(nc.fromFacing+1 == nc.toFacing || nc.fromFacing > nc.toFacing+1)//f
                {
                    facing = nc.toFacing;
                    fmvman.QueueVideo(myvidpath + "_" + nc.from.ToString() + "f" + nc.fromFacing + ".avi");//the underscore won't always be there?
                } else //b
                {
                    facing = nc.toFacing;
                    fmvman.QueueVideo(myvidpath + "_" + nc.from.ToString() + "b" + nc.toFacing + ".avi");//the underscore won't always be there?
                }
                facing = nc.toFacing;
            }*/
        }
        /*foreach (var nc in nodeConnections)
        {
            Debug.Log(nc.ToString());
            if (nc.from == currNode && nc.clickbox.Contains(pos))
            {
                Debug.Log("going from node "+currNode.ToString()+" to "+nc.to.ToString());
                currNode = nc.to;
                fmvman.QueueVideo(myvidpath+nc.from.ToString()+"_"+nc.to.ToString()+".avi");
            }
        }*/
    }

    protected void SetPosition(int node)
    {
        currNode = node;
        MakeClickboxes();
    }

    protected void MoveToPosition(int node)
    {
    }

    protected void QueueVideo(string file)
    {
        fmvman.QueueVideo(myvidpath+file);
    }
}

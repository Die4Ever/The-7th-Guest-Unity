using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class baseRoom : MonoBehaviour {

    protected FMVManager fmvman;
    protected string myvidpath;
    //filenames for rotation seem to be f_4fc where the f=foyer, 4=the node number, f=forwards (b=backwards) or clockwise, the last letter is a/b/c/d and it means the orientation
    //for movement it's f1_2 where f=foyer, 1=starting node number, 2=destination node number
    //include the filename prefix in myvidpath, some are 1 letter (like f for foyer) and some are 2 letters (like bd for brian dutton)
    //not of all these seem to follow those rules, I think I'll have to use nodeNames to lookup filenames
    protected string[] nodeNames;//for convenience? I can just leave them as comments, but then I can't show them in debug output
    string[] facingNames;
    public int currNode=1;
    int facing;//0=a, 1=b, 2=c, 3=d...
    protected class NodeConnection
    {
        public int from;
        public int to;
        public int fromFacing;
        public int toFacing;
        public Rect clickbox;
        public int[] through;//optional
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
        Debug.Log(fmvman.ToString());
        nodeConnections = new List<NodeConnection>();
        currNode = 1;
    }

	// Use this for initialization
	void Start () {
        BaseInit();
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, int[] through=null)
    {
        nodeConnections.Add( new NodeConnection { from=from, to=to, clickbox=clickbox, through=through } );
        MakeClickboxes();
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

    public void OnClick(Vector2 pos)
    {
        Debug.Log("clicky! "+pos.ToString());

        foreach (var nc in nodeConnections)
        {
            Debug.Log(nc.ToString());
            if (nc.from == currNode && nc.clickbox.Contains(pos))
            {
                Debug.Log("going from node "+currNode.ToString()+" to "+nc.to.ToString());
                currNode = nc.to;
                fmvman.QueueVideo(myvidpath+nc.from.ToString()+"_"+nc.to.ToString()+".avi");
            }
        }
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

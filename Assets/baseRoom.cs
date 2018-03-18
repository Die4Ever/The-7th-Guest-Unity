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
    //string[] facingNames;
    protected int currNode;
    //int facing;//0=a, 1=b, 2=c, 3=d...
    protected class NodeConnection
    {
        public int from;
        public int to;
        //public int fromFacing;
        //public int toFacing;
        public Rect clickbox;
        public int[] through;//optional
    };
    protected List<NodeConnection> nodeConnections;

    protected void BaseInit()
    {
        fmvman = GameObject.FindObjectOfType<FMVManager>();
        Debug.Log(fmvman.ToString());
        nodeConnections = new List<NodeConnection>();
    }

	// Use this for initialization
	void Start () {
        BaseInit();
    }

    protected void CreateNodeConnection(int from, int to, Rect clickbox, int[] through=null)
    {
        nodeConnections.Add( new NodeConnection { from=from, to=to, clickbox=clickbox, through=through } );
    }

    void MakeClickboxes()
    {
        foreach (var nc in nodeConnections)
        {
            if (nc.from == currNode)
            {

            }
        }
    }

    public void OnClick(Vector3 pos)
    {
        Debug.Log("clicky! "+pos.ToString());
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

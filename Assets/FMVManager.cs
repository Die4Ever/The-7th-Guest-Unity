using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FMVManager : MonoBehaviour
{
    public GameObject baseVideoPlayer;
    public Texture2D handbeckon, handwag, dramamask, chatteringteeth, throbbingbrain, blueeye, browneye;
    //GameObject currentVideo = null;
    GameObject currentSong = null;
    string path = "";
    //GameObject movementPlayer = null;
    baseRoom currRoom = null;

    public enum CommandType { VIDEO, SONG, AUDIO, WAITFORVIDEO, WAITFORSONG, WAITFORAUDIO, WAITFOROVERLAY, WAITTIME, SWITCHROOM, OVERLAY };
    public class Command
    {
        public CommandType type=CommandType.VIDEO;
        public GameObject player = null;
        public string file;
        public string tags = "";
        public float countdown=0;
        public float fadeInTime=0;
        public float fadeOutTime=0;
        public float playbackSpeed = 1;
        public Color transparentColor = new Color(0, 0, 0, 0);
        public System.Action<Command> callback = null;
        public float threshold = 0.24f;
        public float slope = 0.6f;
    };

    public List<Command> playlist = new List<Command>();
 
    // Use this for initialization
    void Start()
    {
        path = "file://" + Application.dataPath + "/../upscaled/";
        Application.runInBackground = true;
        Debug.Log("FMVManager::Start()");

        handwag = Instantiate(Resources.Load("cursors/wagging-hand", typeof(Texture2D))) as Texture2D;
        handbeckon = Instantiate(Resources.Load("cursors/beckon", typeof(Texture2D))) as Texture2D;
        dramamask = Instantiate(Resources.Load("cursors/drama-mask", typeof(Texture2D))) as Texture2D;
        chatteringteeth = Instantiate(Resources.Load("cursors/chattering-teeth", typeof(Texture2D))) as Texture2D;
        throbbingbrain = Instantiate(Resources.Load("cursors/throbbing-skull", typeof(Texture2D))) as Texture2D;
        blueeye = Instantiate(Resources.Load("cursors/blue-eyeball", typeof(Texture2D))) as Texture2D;
        browneye = Instantiate(Resources.Load("cursors/brown-eyeball", typeof(Texture2D))) as Texture2D;
    }

    public baseRoom SwitchRoom(string roomName, int node, char facing)
    {
        Debug.Log("SwitchRoom("+roomName+")");
        GameObject go = Instantiate(Resources.Load(roomName, typeof(GameObject))) as GameObject;
        baseRoom r = go.GetComponent<baseRoom>();
        r.currPos.node = node;
        r.currPos.facing = facing;
        Destroy(currRoom);
        currRoom = r;
        return r;
    }

    public void PlaySong(Command c, bool wait=false)
    {
        //Debug.Log(c.file);
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type=CommandType.WAITFORSONG });
    }

    public void PlayAudio(Command c, bool wait=true)
    {
        //Debug.Log(c.file);
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFORAUDIO });
    }

    public void QueueVideo(Command c, bool wait=true)
    {
        //Debug.Log(c.file);
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueOverlay(Command c, bool wait=false)
    {
        //Debug.Log(c.file);
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFOROVERLAY });
    }

    public void ClearQueue(string tags)
    {
        for(int i=0;i<playlist.Count;i++)
        {
            if(playlist[i].tags.Contains(tags))
            {
                if (playlist[i].player)
                    Destroy(playlist[i].player);
                playlist.RemoveAt(i);
                i--;
            }
        }
    }

    IEnumerator PlaySong(Command c)
    {
        Debug.Log("test PlaySong");
        AudioSource source = Instantiate(baseVideoPlayer).GetComponent<AudioSource>();
        c.player = source.gameObject;
        source.playOnAwake = true;
        using (var www = new WWW(path + c.file))
        {
            yield return www;
            source.clip = www.GetAudioClip();
            while(source.clip.loadState == AudioDataLoadState.Loading)
            {
                System.Threading.Thread.Sleep(1);
            }
            source.Play();
            currentSong = source.gameObject;
        }
    }

    GameObject LoadVideo(Command c)
    {
        Debug.Log("LoadVideo("+c.file+") playlist.Count=="+playlist.Count.ToString());
        GameObject go = null;
        /*if(c.tags.Contains("movement"))
        {
            if (movementPlayer == null)
            {
                movementPlayer = Instantiate(baseVideoPlayer);
            }
            else if (movementPlayer.GetComponent<videoScript>().done == false) return null;
            go = movementPlayer;
        }
        else
        {*/
            go = Instantiate(baseVideoPlayer);
        //}
        VideoPlayer vp = go.GetComponent<VideoPlayer>();
        videoScript vs = go.GetComponent<videoScript>();
        vs.transparentColor = c.transparentColor;
        vs.fadeInTime = c.fadeInTime;
        vs.fadeOutTime = c.fadeOutTime;
        vs.threshold = c.threshold;
        vs.slope = c.slope;
        vp.url = path + c.file;
        vp.playbackSpeed = c.playbackSpeed;
        vp.prepareCompleted += PrepareCompleted;
        //vp.loopPointReached += EndReached;
        vs.fadeOutFinished += EndReached;
        vp.Prepare();
        return go;
    }

    // Update is called once per frame
    void Update()
    {
        PlaylistProcess();
    }

    bool CheckWait(CommandType type, int slot)
    {
        //Debug.Log("CheckWait("+type.ToString()+", "+slot.ToString()+")");
        for(int i=0;i<slot;i++)
        {
            if (playlist[i].type != type) continue;
            if(playlist[i].player==null || playlist[i].player.GetComponent<videoScript>().done==false)
            {
                //Debug.Log("waiting on " + playlist[i].file+", "+playlist[i].player.ToString()+", done=="+ playlist[i].player.GetComponent<videoScript>().done.ToString());
                return false;
            }
        }
        Debug.Log("CheckWait going forwards!");
        /*for (int i = 0; i < slot; i++)
        {
            if (playlist[i].type != type) continue;
            if (playlist[i].player) Destroy(playlist[i].player, 0.25f);
            playlist.RemoveAt(i);
            i--;
            slot--;
        }*/
        playlist.RemoveAt(slot);
        return true;
    }

    void PlaylistProcess()
    {
        if (playlist.Count==0) return;

        for (int i = 0; i < playlist.Count; i++)
        {
            Command c = playlist[i];
            if (c.type == CommandType.WAITFORVIDEO)
            {
                CheckWait(CommandType.VIDEO, i);
                break;
            }
            if (c.type == CommandType.WAITFORSONG)
            {
                CheckWait(CommandType.SONG, i);
                break;
            }
            if (c.type == CommandType.WAITFORAUDIO)
            {
                CheckWait(CommandType.AUDIO, i);
                break;
            }
            if (c.type == CommandType.WAITFOROVERLAY)
            {
                CheckWait(CommandType.OVERLAY, i);
                break;
            }

            if (c.type == CommandType.SONG || c.type == CommandType.AUDIO)
            {
                if(c.player!=null)
                {
                    AudioSource audioSource = c.player.GetComponent<AudioSource>();
                    if (!audioSource.clip) continue;
                    if (audioSource.clip.loadState == AudioDataLoadState.Loading || audioSource.clip.loadState == AudioDataLoadState.Unloaded) continue;
                    if (audioSource.isPlaying) continue;
                    Debug.Log("detected song end");
                    SongEndReached(c.player);
                    //playlist.RemoveAt(i);
                    continue;
                }
                if (c.file.EndsWith(".avi"))
                {
                    GameObject go = Instantiate(baseVideoPlayer);
                    VideoPlayer vp = go.GetComponent<VideoPlayer>();
                    videoScript vs = go.GetComponent<videoScript>();
                    vp.url = path + c.file;
                    vp.targetCamera = null;
                    vp.renderMode = VideoRenderMode.APIOnly;
                    vs.fadeOutFinished += SongEndReached;
                    vp.prepareCompleted += SongPrepared;
                    vp.Prepare();
                    c.player = go;
                }
                else StartCoroutine(PlaySong(c));
                //playlist.RemoveAt(0);
            }
            if (c.type == CommandType.VIDEO)
            {
                if (c.player != null)
                {
                    var vs = c.player.GetComponent<videoScript>();
                    /*if (vs && vs.done)
                    {
                        //if (c.callback!=null) c.callback(c);
                        //Destroy(vs, 0.25f);
                        //playlist.RemoveAt(i);
                        break;
                    }*/
                    continue;
                }
                c.player = LoadVideo(c);
                //break;
                //playlist.RemoveAt(0);
            }
        }
    }

    void EndReached(VideoPlayer vp)
    {
        //Debug.Log("playlist count == "+playlist.Count.ToString());
        /*if(playlist.Count>0 && playlist[0].type==CommandType.WAITFORVIDEO)
        {
            Command c = playlist[0];
            playlist.RemoveAt(0);
            if (c.callback!=null) c.callback(c);
        }*/
        //if(vp!=movementPlayer) Destroy(vp.gameObject);

        if (vp == null) return;
        for (int i = 0; i < playlist.Count; i++)
        {
            var c = playlist[i];
            if (c.type == CommandType.VIDEO)
            {
                if (c.player == vp)
                {
                    if (c.callback != null) c.callback(c);
                    //playlist.RemoveAt(i);
                    break;
                }
            }
        }
    }

    
    void SongEndReached(VideoPlayer vp)
    {
        SongEndReached(vp.gameObject);
    }

    void SongEndReached(GameObject s)
    {
        //Debug.Log("playlist count == " + playlist.Count.ToString());
        if (s == null) return;
        for(int i=0;i<playlist.Count;i++)
        {
            var c = playlist[i];
            if(c.type == CommandType.SONG || c.type==CommandType.AUDIO)
            {
                if(c.player==s)
                {
                    if (c.callback != null) c.callback(c);
                    playlist.RemoveAt(i);
                    i--;
                }
            }/* else if(c.type==CommandType.WAITFORSONG || c.type==CommandType.WAITFORAUDIO)
            {
                if (c.callback != null) c.callback(c);
                playlist.RemoveAt(i);
                i--;
            }*/
        }
        if(s!=null) Destroy(s);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //vp.targetCameraAlpha = 0;
        vp.Play();
        /*for(int i=0;i<playlist.Count;i++)
        {

        }*/
        //for()
    }

    void SongPrepared(VideoPlayer vp)
    {
        currentSong = vp.gameObject;
        vp.Play();
    }

    private void OnDestroy()
    {
        if (currentSong) Destroy(currentSong);
        foreach(var v in playlist)
        {
            if (v.player) Destroy(v.player);
        }
    }
}

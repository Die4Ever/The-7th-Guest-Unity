using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FMVManager : MonoBehaviour
{
    public GameObject baseVideoPlayer;
    GameObject currentVideo = null;
    GameObject currentSong = null;
    public Texture2D handbeckon, handwag, dramamask, chatteringteeth, throbbingbrain, blueeye, browneye;

    public enum CommandType { VIDEO, SONG, WAITFORVIDEO, WAITFORSONG, WAITTIME, SWITCHROOM, OVERLAY };
    public class Command
    {
        public CommandType type=CommandType.VIDEO;
        public string file;
        public float countdown=0;
        public float fadeInTime=0;
        public float fadeOutTime=0;
        public float playbackSpeed = 1;
        public Color transparentColor = new Color(0, 0, 0, 0);
        public System.Action<Command> callback =null;
    };

    public Queue<Command> playlist = new Queue<Command>();
    Command currentCommand = null;
    string path = "";

    /*void DialogWindow(int windowID) { }
    private void OnGUI()
    {
        var w = GUI.Window(0, new Rect(0, 0, 1000, 200), DialogWindow, Application.dataPath);
    }*/
 
    // Use this for initialization
    void Start()
    {
        path = "file://" + Application.dataPath + "/../upscaled/";
        Debug.Log(Application.dataPath);
        Debug.Log(Application.streamingAssetsPath);
        Application.runInBackground = true;
        Debug.Log("test");

        handwag = Instantiate(Resources.Load("cursors/wagging-hand", typeof(Texture2D))) as Texture2D;
        handbeckon = Instantiate(Resources.Load("cursors/beckon", typeof(Texture2D))) as Texture2D;
        dramamask = Instantiate(Resources.Load("cursors/drama-mask", typeof(Texture2D))) as Texture2D;
        chatteringteeth = Instantiate(Resources.Load("cursors/chattering-teeth", typeof(Texture2D))) as Texture2D;
        throbbingbrain = Instantiate(Resources.Load("cursors/throbbing-skull", typeof(Texture2D))) as Texture2D;
        blueeye = Instantiate(Resources.Load("cursors/blue-eyeball", typeof(Texture2D))) as Texture2D;
        browneye = Instantiate(Resources.Load("cursors/brown-eyeball", typeof(Texture2D))) as Texture2D;
    }

    public void QueueSong(string file, bool wait=false)
    {
        playlist.Enqueue(new Command { file = file, type = CommandType.SONG });
        if (wait)
            playlist.Enqueue(new Command { type=CommandType.WAITFORSONG });
    }

    public void QueueVideo(string file)
    {
        Debug.Log(file);
        playlist.Enqueue(new Command { file = file, fadeInTime = 0.0f, fadeOutTime = 0.0f });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueVideoCallback(string file, System.Action<Command> callback)
    {
        Debug.Log(file);
        playlist.Enqueue(new Command { file = file, fadeInTime = 0.0f, fadeOutTime = 0.0f });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO, callback=callback });
    }

    public void QueueOverlayCallback(string file, System.Action<Command> callback, Color transparentColor)
    {
        Debug.Log(file);
        playlist.Enqueue(new Command { file = file, fadeInTime = 0.0f, fadeOutTime = 0.0f, transparentColor=transparentColor });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO, callback = callback });
    }

    public void QueueVideoFade(string file)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = 5, fadeOutTime = 5 });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueVideoFade(string file, float fadeInTime, float fadeOutTime, float playbackSpeed, System.Action<Command> callback=null)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = fadeInTime, fadeOutTime = fadeOutTime, playbackSpeed = playbackSpeed });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO, file = file, callback = callback });
    }

    IEnumerator PlaySong(Command c)
    {
        //var ac = Instantiate(new AudioClip());
        /*if(c.file.EndsWith(".avi"))
        {
            Debug.Log(c.file);
            GameObject go = Instantiate(baseVideoPlayer);
            VideoPlayer vp = go.GetComponent<VideoPlayer>();
            videoScript vs = go.GetComponent<videoScript>();
            vp.url = path + c.file;
            vp.targetCamera = null;
            //vp.renderMode = VideoRenderMode.APIOnly;
            vp.renderMode = VideoRenderMode.RenderTexture;
            vs.fadeOutFinished += SongEndReached;
            vp.prepareCompleted += SongPrepared;
            vp.Prepare();
            yield break;
        }*/
        Debug.Log("test PlaySong");
        AudioSource source = Instantiate(baseVideoPlayer).GetComponent<AudioSource>();
        source.playOnAwake = true;
        Debug.Log("test 1");
        using (var www = new WWW(path + c.file))
        {
            Debug.Log("test 2");
            yield return www;
            source.clip = www.GetAudioClip();
            Debug.Log("test 3");
            while(source.clip.loadState == AudioDataLoadState.Loading)
            {
                System.Threading.Thread.Sleep(1);
            }
            source.Play();
            currentSong = source.gameObject;
        }
    }

    void LoadVideo(Command c)
    {
        GameObject go = Instantiate(baseVideoPlayer);
        VideoPlayer vp = go.GetComponent<VideoPlayer>();
        videoScript vs = go.GetComponent<videoScript>();
        vs.transparentColor = c.transparentColor;
        vs.fadeInTime = c.fadeInTime;
        vs.fadeOutTime = c.fadeOutTime;
        vp.url = path + c.file;
        vp.playbackSpeed = c.playbackSpeed;
        vp.prepareCompleted += PrepareCompleted;
        //vp.loopPointReached += EndReached;
        vs.fadeOutFinished += EndReached;
        vp.Prepare();
    }

    // Update is called once per frame
    void Update()
    {
        PlaylistProcess();
    }

    void PlaylistProcess()
    {
        if (playlist.Count==0) return;

        if(currentCommand != null && currentCommand.countdown>0)
        {
            currentCommand.countdown -= Time.deltaTime;
            return;
        }
        Command c = playlist.Peek();
        if (c.type == CommandType.WAITFORVIDEO) return;
        if(c.type == CommandType.WAITFORSONG)
        {
            if (!currentSong) return;
            AudioSource audioSource = currentSong.GetComponent<AudioSource>();
            if (!audioSource.clip) return;
            if (audioSource.clip.loadState == AudioDataLoadState.Loading || audioSource.clip.loadState == AudioDataLoadState.Unloaded) return;
            if(audioSource.isPlaying) return;
            /*Destroy(currentSong);
            currentSong = null;
            playlist.Dequeue();*/
            Debug.Log("detected song end");
            SongEndReached(currentSong);
        }
        if(c.type == CommandType.WAITTIME)
        {
            playlist.Dequeue();
        }
        currentCommand = c;
        if (c.type == CommandType.SONG)
        {
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
            }
            else StartCoroutine(PlaySong(c));
            playlist.Dequeue();
        }
        if(c.type==CommandType.VIDEO)
        {
            LoadVideo(c);
            playlist.Dequeue();
        }
    }

    void EndReached(VideoPlayer vp)
    {
        //Debug.Log("playlist count == "+playlist.Count.ToString());
        if(playlist.Count>0 && playlist.Peek().type==CommandType.WAITFORVIDEO)
        {
            Command c = playlist.Dequeue();
            if(c.callback!=null) c.callback(c);
        }
    }

    
    void SongEndReached(VideoPlayer vp)
    {
        SongEndReached(vp.gameObject);
    }

    void SongEndReached(GameObject s)
    {
        //Debug.Log("playlist count == " + playlist.Count.ToString());
        if (s!=null && s == currentSong)
        {
            if (playlist.Count>0 && playlist.Peek().type == CommandType.WAITFORSONG)
            {
                Command c = playlist.Dequeue();
                if (c.callback != null) c.callback(c);
            }
            currentSong = null;
        }
        if(s!=null) Destroy(s);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //vp.targetCameraAlpha = 0;
        vp.Play();
        if (currentVideo)
        {
            float fadeOut = 0.5f;
            if(vp.GetComponent<videoScript>()) fadeOut += vp.GetComponent<videoScript>().fadeOutTime;
            Destroy(currentVideo, fadeOut);//make sure we leave enough time for crossfading and overlays... how will this work with puzzle sprites? might need to do those very differently
            //StartCoroutine(DestroyVideoPlayer(currentVideo));
        }
        currentVideo = vp.gameObject;
    }

    void SongPrepared(VideoPlayer vp)
    {
        currentSong = vp.gameObject;
        //System.Threading.Thread.Sleep(10);
        vp.Play();
    }
}

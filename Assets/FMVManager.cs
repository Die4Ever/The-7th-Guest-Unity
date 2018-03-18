using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FMVManager : MonoBehaviour
{
    public GameObject baseVideoPlayer;
    GameObject currentVideo = null;
    GameObject currentSong = null;

    public enum CommandType { VIDEO, SONG, WAITFORVIDEO, WAITFORSONG, WAITTIME };
    public class Command
    {
        public CommandType type=CommandType.VIDEO;
        public string file;
        public float countdown=0;
        public float fadeInTime=0;
        public float fadeOutTime=0;
        public float playbackSpeed = 1;
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
    }

    public void QueueSong(string file)
    {
        playlist.Enqueue(new Command { file = file, type = CommandType.SONG });
    }

    public void QueueVideo(string file)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = 0.0f, fadeOutTime = 0.0f });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueVideoFade(string file)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = 5, fadeOutTime = 5 });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueVideoFade(string file, float fadeInTime, float fadeOutTime, float playbackSpeed)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = fadeInTime, fadeOutTime = fadeOutTime, playbackSpeed = playbackSpeed });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    IEnumerator PlaySong(Command c)
    {
        //var ac = Instantiate(new AudioClip());
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

        if(Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            GameObject.FindObjectOfType<baseRoom>().OnClick(pos);
        }
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
            Destroy(currentSong);
            currentSong = null;
            playlist.Dequeue();
        }
        if(c.type == CommandType.WAITTIME)
        {
            playlist.Dequeue();
        }
        currentCommand = c;
        if (c.type == CommandType.SONG)
        {
            StartCoroutine(PlaySong(c));
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
        Debug.Log("playlist count == "+playlist.Count.ToString());
        if(playlist.Peek().type==CommandType.WAITFORVIDEO)
        {
            playlist.Dequeue();
        }
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //vp.targetCameraAlpha = 0;
        vp.Play();
        if (currentVideo)
        {
            //Destroy(currentVideo, 0.01f);
            //StartCoroutine(DestroyVideoPlayer(currentVideo));
        }
        currentVideo = vp.gameObject;
    }
}

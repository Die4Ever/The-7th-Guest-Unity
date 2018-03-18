using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class FMVManager : MonoBehaviour
{
    public GameObject baseVideoPlayer;
    GameObject currentVideo = null;
    GameObject currentSong = null;

    enum CommandType { VIDEO, SONG, WAITFORVIDEO, WAITFORSONG, WAITTIME };
    class Command
    {
        public CommandType type=CommandType.VIDEO;
        public string file;
        public float countdown=0;
        public float fadeInTime=0;
        public float fadeOutTime=0;
        public float playbackSpeed = 1;
    };

    Queue<Command> playlist = new Queue<Command>();
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
        //vp.url = "file://D:/t7g/upscaled/deband-f1_.avi";
        QueueSong("../oggs/track1.ogg");
        QueueVideoFade("HDISK/vlogo.avi", 1, 5, 1);
        QueueVideoFade("HDISK/tripro.avi");
        playlist.Enqueue(new Command { type = CommandType.WAITTIME, countdown = 5 });
        QueueVideoFade("HDISK/title.avi", 1.0f, 0.0f, 0.5f);

        QueueVideo("INTRO/o1pa.avi");
        QueueVideo("INTRO/o1tu.avi");
        QueueVideo("INTRO/o3pa.avi");
        QueueVideo("INTRO/o3tu.avi");
        QueueVideo("INTRO/o4pa.avi");
        QueueVideo("INTRO/o4tu.avi");
        QueueVideo("INTRO/o5pa.avi");
        QueueVideo("INTRO/o5tu.avi");
        QueueVideo("INTRO/o6pa.avi");
        QueueVideo("INTRO/o6tu.avi");
        QueueVideo("INTRO/o7pa.avi");
        QueueVideo("INTRO/o7tu.avi");
        QueueVideo("INTRO/o8pa.avi");
        QueueVideo("INTRO/o8tu.avi");
        QueueVideo("INTRO/o9pa.avi");
        QueueVideo("INTRO/o9tu.avi");
        QueueVideo("INTRO/o10pa.avi");
        QueueVideo("INTRO/o10tu.avi");
        QueueVideo("INTRO/o12pa.avi");
        QueueVideo("LI/l_in.avi");
        QueueVideo("FH/f_5ba.avi");
        QueueVideo("FH/f5_1.avi");
        QueueVideo("FH/f1_.avi");
        QueueVideo("FH/f_1fa.avi");
        QueueVideo("FH/f_1fb.avi");
        QueueSong("GAMWAV/1_e_1.avi");
        playlist.Enqueue(new Command { type = CommandType.WAITFORSONG });
        playlist.Enqueue(new Command { type = CommandType.WAITTIME, countdown = 3 });
        QueueSong("GAMWAV/1_e_2.avi");
        playlist.Enqueue(new Command { type = CommandType.WAITFORSONG });
        playlist.Enqueue(new Command { type = CommandType.WAITTIME, countdown = 1 });
        QueueVideo("FH/f1_6.avi");

        //LoadVideo("f6_1.avi");
        //playlist.Enqueue("f1_.avi");
    }

    void QueueSong(string file)
    {
        playlist.Enqueue(new Command { file = file, type = CommandType.SONG });
    }

    void QueueVideo(string file)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = 0.0f, fadeOutTime = 0.0f });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    void QueueVideoFade(string file)
    {
        playlist.Enqueue(new Command { file = file, fadeInTime = 5, fadeOutTime = 5 });
        playlist.Enqueue(new Command { type = CommandType.WAITFORVIDEO });
    }

    void QueueVideoFade(string file, float fadeInTime, float fadeOutTime, float playbackSpeed)
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

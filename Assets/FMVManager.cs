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

    float puzzleSpeed = 30.0f / 15.0f;//the game is originally 15fps
    float movementSpeed = 30.0f / 15.0f;
    float turningSpeed = 30.0f / 15.0f;
    float musicVolume = 0.25f;
    float videoVolume = 1.0f;
    float otherVolume = 1.0f;

    public enum CommandType { VIDEO, SONG, AUDIO, WAITFORVIDEO, WAITFORSONG, WAITFORAUDIO, WAITFOROVERLAY, WAITTIME, SWITCHROOM, OVERLAY, CLEARVIDEOS };
    public class Command
    {
        public CommandType type=CommandType.VIDEO;
        //public GameObject player = null;
        public string file;
        public string tags = "";
        public float countdown=0;
        public float fadeInTime=0;
        public float fadeOutTime=0;
        public float playbackSpeed = 1;
        public Color transparentColor = new Color(0, 0, 0, 0);
        public System.Action<Command> callback = null;
        public float threshold = 0.1f;
        public float slope = 0.5f;
        public bool loop = false;
        public bool freezeFrame = false;
        public float z = 0;
    };

    public List<Command> playlist = new List<Command>();
    public List<GameObject> playing_videos = new List<GameObject>();
    public List<GameObject> playing_audio = new List<GameObject>();
    public Dictionary<string, int> variables = new Dictionary<string, int>();//the original game uses an array of bytes, I should be able to make due with this

    // Use this for initialization
    void Start()
    {
        path = Application.dataPath + "/../upscaled15fps/";
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        Debug.Log("FMVManager::Start()");
        Debug.Log("puzzleSpeed == " + puzzleSpeed.ToString("0.000"));
        Debug.Log("turningSpeed == " + turningSpeed.ToString("0.000"));
        Debug.Log("movementSpeed == " + movementSpeed.ToString("0.000"));

        handwag = Instantiate(Resources.Load("cursors/wagging-hand", typeof(Texture2D))) as Texture2D;
        handbeckon = Instantiate(Resources.Load("cursors/beckon", typeof(Texture2D))) as Texture2D;
        dramamask = Instantiate(Resources.Load("cursors/drama-mask", typeof(Texture2D))) as Texture2D;
        chatteringteeth = Instantiate(Resources.Load("cursors/chattering-teeth", typeof(Texture2D))) as Texture2D;
        throbbingbrain = Instantiate(Resources.Load("cursors/throbbing-skull", typeof(Texture2D))) as Texture2D;
        blueeye = Instantiate(Resources.Load("cursors/blue-eyeball", typeof(Texture2D))) as Texture2D;
        browneye = Instantiate(Resources.Load("cursors/brown-eyeball", typeof(Texture2D))) as Texture2D;

        if (currRoom == null)
        {
            currRoom = GameObject.FindObjectOfType<baseRoom>();
            if(currRoom!=null) Debug.Log("found room " + currRoom.name);
        }
        UpdateDebugText();
    }

    void UpdateDebugText()
    {
        float t = movementSpeed * 15.0f / 9.0f;
        var dt = GameObject.Find("DebugText").GetComponent<UnityEngine.UI.Text>();
        dt.text = "Movement Speed: " + (t * 100.0f).ToString("0") + "%\n" + ((int)(movementSpeed*15.0f)).ToString()+" fps";
    }

    public baseRoom SwitchRoom(string roomName, int node, char facing)
    {
        IncrementVariable("enter-" + roomName);
        Debug.Log("SwitchRoom("+roomName+")");
        if(currRoom==null)
        {
            currRoom = GameObject.FindObjectOfType<baseRoom>();
            //Debug.Log("found room " + currRoom.name);
        }
        GameObject go = Instantiate(Resources.Load(roomName, typeof(GameObject))) as GameObject;
        baseRoom r = go.GetComponent<baseRoom>();
        r.currPos.node = node;
        r.currPos.facing = facing;
        if(currRoom!=null) Destroy(currRoom.gameObject);
        currRoom = r;
        return r;
    }

    public GameObject StartPuzzle(string name, System.Action<string> endPuzzle)
    {
        IncrementVariable("startpuzzle-" + name);
        GameObject go = Instantiate(Resources.Load(name, typeof(GameObject))) as GameObject;
        go.GetComponent<basePuzzle>().endPuzzle = endPuzzle;
        return go;
    }

    public void PlaySong(Command c, bool wait=false)
    {
        //Debug.Log(c.file);
        //if (currRoom.name == "intro") { c.playbackSpeed *= 4.0f; c.fadeInTime = 0; c.fadeOutTime = 0; c.countdown = 0; }
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type=CommandType.WAITFORSONG });
    }

    public void PlayAudio(Command c, bool wait=true)
    {
        //Debug.Log(c.file);
        //if (currRoom.name == "intro") { c.playbackSpeed *= 4.0f; c.fadeInTime = 0; c.fadeOutTime = 0; c.countdown = 0; }
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFORAUDIO });
    }

    public void QueueVideo(Command c, bool wait=true)
    {
        //Debug.Log(c.file);
        //if (currRoom.name == "intro") { c.playbackSpeed *= 4.0f; c.fadeInTime = 0; c.fadeOutTime = 0; c.countdown = 0; }
        //c.playbackSpeed *= 4.0f;
        if (HasTags(c.tags, "puzzle") && c.freezeFrame == false) c.playbackSpeed *= puzzleSpeed;
        else if (HasTags(c.tags, "turning") && c.freezeFrame == false) c.playbackSpeed *= turningSpeed;
        else if (HasTags(c.tags, "movement") && c.freezeFrame == false) c.playbackSpeed *= movementSpeed;
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFORVIDEO });
    }

    public void QueueOverlay(Command c, bool wait=false)
    {
        //Debug.Log(c.file);
        //if (currRoom.name == "intro") { c.playbackSpeed *= 4.0f; c.fadeInTime = 0; c.fadeOutTime = 0; c.countdown = 0; }
        if (HasTags(c.tags, "puzzle") && c.freezeFrame == false) c.playbackSpeed *= puzzleSpeed;
        else if (HasTags(c.tags, "turning") && c.freezeFrame == false) c.playbackSpeed *= turningSpeed;
        else if (HasTags(c.tags, "movement") && c.freezeFrame == false) c.playbackSpeed *= movementSpeed;
        playlist.Add(c);
        if (wait) playlist.Add(new Command { type = CommandType.WAITFOROVERLAY });
    }

    public bool HasTags(string haystack, string needle)
    {
        if (haystack.Contains(needle)) return true;//later make this smarter, to accept space separated classes
        else return false;
    }

    public Vector2 ScreenToVideo(Vector2 pos)
    {
        Vector2 v = Camera.main.ScreenToViewportPoint(pos);
        float ratio = ((float)Screen.width) / ((float)Screen.height);
        float videoRatio = 640.0f / 320.0f;//the game is 2:1 aka 18:9
        Vector2 ret = v;
        if(ratio < videoRatio)//letterbox, trim the height
        {
            ret.y *= videoRatio / ratio;
            ret.y -= (videoRatio / ratio - 1.0f) / 2.0f;
            //Debug.Log("letterbox");
        } else if(ratio > videoRatio)//pillarbox, trim the width
        {
            ret.x *= ratio / videoRatio;
            ret.x -= (ratio / videoRatio - 1.0f) / 2.0f;
            //Debug.Log("pillarbox");
        } else //perfect fit? return v?
        {
            //Debug.Log("perfect fit?");
        }
        return ret;
    }

    public void ClearPlayingAudio(string tags)
    {
        for (int i = 0; i < playing_audio.Count; i++)
        {
            var vs = playing_audio[i].GetComponent<videoScript>();
            var c = vs.command;
            if (HasTags(c.tags, tags))
            {
                Destroy(vs.gameObject, 0.1f);
                playing_audio.RemoveAt(i);
                i--;
            }
        }

    }
    public void ClearPlayingVideos(string tags)
    {
        for (int i = 0; i < playing_videos.Count; i++)
        {
            var vs = playing_videos[i].GetComponent<videoScript>();
            var c = vs.command;
            if (HasTags(c.tags, tags))
            {
                Destroy(vs.gameObject, 0.1f);
                playing_videos.RemoveAt(i);
                i--;
            }
        }
    }

    public int CountPlayingVideos(string tags)
    {
        int videos = 0;
        foreach (var v in playing_videos)
        {
            var vs = v.GetComponent<videoScript>();
            if (vs.done == false && HasTags(vs.command.tags, tags)) videos++;
        }
        return videos;
    }

    public void ClearQueue(string tags)
    {
        for(int i=0;i<playlist.Count;i++)
        {
            if(HasTags(playlist[i].tags, tags))
            {
                //if (playlist[i].player)
                    //Destroy(playlist[i].player);
                playlist.RemoveAt(i);
                i--;
            }
        }
    }

    string AddDefaultExtension(string filename)
    {
        bool m = System.Text.RegularExpressions.Regex.IsMatch(filename, "\\..+$");
        /*
         * return filename.replace(".avi", ".mkv");//if I ever need to change file extensions
         */
        if (m) return filename;
        return filename + ".avi";
    }

    public void IncrementVariable(string name)
    {
        int v = 0;
        variables.TryGetValue(name, out v);
        variables[name] = v + 1;

        foreach (var k in variables.Keys) Debug.Log(k + ": " + variables[k].ToString());
    }

    IEnumerator PlaySong(Command c)
    {
        if(c.file=="")
        {
            if (currentSong != null) Destroy(currentSong);
            currentSong = null;
            yield break;
        }
        c.file = AddDefaultExtension(c.file);
        //IncrementPlayCount(c.file);
        if(c.type == CommandType.SONG && currentSong!=null)
        {
            if(c.file == currentSong.GetComponent<videoScript>().command.file)
            {
                Debug.Log("already playing song " + c.file);
                yield break;
            }
        }
        Debug.Log("playing " + c.file);
        videoScript vs;
        if (c.file.EndsWith(".avi"))
        {
            GameObject go = Instantiate(baseVideoPlayer);
            VideoPlayer vp = go.GetComponent<VideoPlayer>();
            vs = go.GetComponent<videoScript>();
            vs.command = c;
            vp.isLooping = c.loop;
            vp.url = path + c.file;
            vp.targetCamera = null;
            vp.renderMode = VideoRenderMode.APIOnly;
            if (c.type == CommandType.AUDIO)
            {
                go.GetComponent<AudioSource>().volume *= otherVolume;
                playing_audio.Add(go);
                vs.fadeOutFinished += AudioEndReached;
                vp.prepareCompleted += AudioPrepared;
            }
            if (c.type == CommandType.SONG)
            {
                go.GetComponent<AudioSource>().volume *= musicVolume;
                if (currentSong != null)
                {
                    Destroy(currentSong);
                }
                currentSong = go;
                vs.fadeOutFinished += SongEndReached;
                vp.prepareCompleted += SongPrepared;
            }
            //vs.Init();
            vp.Prepare();
            yield break;
        }

        Debug.Log("test PlaySong");
        AudioSource source = Instantiate(baseVideoPlayer).GetComponent<AudioSource>();
        vs = source.GetComponent<videoScript>();
        source.loop = c.loop;
        vs.command = c;
        GameObject player = source.gameObject;
        source.playOnAwake = true;
        using (var www = new WWW(path + c.file))
        {
            yield return www;
            source.clip = www.GetAudioClip();
            while(source.clip.loadState == AudioDataLoadState.Loading)
            {
                System.Threading.Thread.Sleep(1);
            }
            if (c.type == CommandType.SONG)
            {
                source.volume *= musicVolume;
                if (currentSong != null)
                {
                    Destroy(currentSong);
                }
                currentSong = source.gameObject;
                vs.fadeOutFinished += SongEndReached;
            }
            else if (c.type == CommandType.AUDIO)
            {
                source.volume *= otherVolume;
                playing_audio.Add(source.gameObject);
                vs.fadeOutFinished += AudioEndReached;
            }
            //vs.Init();
            source.Play();
        }
    }

    GameObject LoadVideo(Command c)
    {
        c.file = AddDefaultExtension(c.file);
        //IncrementPlayCount(c.file);
        //Debug.Log("LoadVideo("+c.file+") playlist.Count=="+playlist.Count.ToString());
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
        go.GetComponent<AudioSource>().volume *= videoVolume;
        vs.command = c;
        vs.transparentColor = c.transparentColor;
        vs.fadeInTime = c.fadeInTime;
        vs.fadeOutTime = c.fadeOutTime;
        vs.freezeFrame = c.freezeFrame;
        vs.threshold = c.threshold;
        vs.slope = c.slope;
        vp.isLooping = c.loop;
        vp.url = path + c.file;
        vp.playbackSpeed = c.playbackSpeed;
        vp.prepareCompleted += PrepareCompleted;
        //vp.loopPointReached += EndReached;
        vs.fadeOutFinished += EndReached;
        //vs.Init();
        vp.Prepare();
        playing_videos.Add(go);
        return go;
    }

    // Update is called once per frame
    void Update()
    {
        PlaylistProcess();

        if(Input.GetKeyDown(KeyCode.KeypadPlus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            int fps = Mathf.RoundToInt(movementSpeed * 15.0f);
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) fps++;
            else if (Input.GetKeyDown(KeyCode.KeypadMinus)) fps--;
            if (fps < 5) fps = 5;
            if (fps > 45) fps = 45;
            movementSpeed = (float)fps / 15.0f;
            turningSpeed = movementSpeed;
            UpdateDebugText();
        }
    }

    bool CheckWait(CommandType type, int slot, Command c)
    {
        //Debug.Log("CheckWait("+type.ToString()+", "+slot.ToString()+")");
        /*for(int i=0;i<slot;i++)
        {
            if (playlist[i].type != type) continue;
            if(playlist[i].player==null || playlist[i].player.GetComponent<videoScript>().done==false)
            {
                //Debug.Log("waiting on " + playlist[i].file+", "+playlist[i].player.ToString()+", done=="+ playlist[i].player.GetComponent<videoScript>().done.ToString());
                return false;
            }
        }*/
        if (type == CommandType.VIDEO || type==CommandType.OVERLAY)
        {
            foreach(var v in playing_videos)
            {
                var vs = v.GetComponent<videoScript>();
                if ( vs.done==false && vs.command.type==type && HasTags(vs.command.tags, c.tags)) return false;
            }
        }
        if (type == CommandType.AUDIO)
        {
            foreach (var a in playing_audio)
            {
                var vs = a.GetComponent<videoScript>();
                if (HasTags(vs.command.tags, c.tags)) return false;
            }
        }
        if (type == CommandType.SONG && currentSong != null && HasTags(currentSong.GetComponent<videoScript>().command.tags, c.tags) ) return false;//I don't see why I would use tags on the current song
        //Debug.Log("CheckWait going forwards!");
        /*for (int i = 0; i < slot; i++)
        {
            if (playlist[i].type != type) continue;
            if (playlist[i].player) Destroy(playlist[i].player, 0.25f);
            playlist.RemoveAt(i);
            i--;
            slot--;
        }*/
        if (c.callback != null) c.callback(c);
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
                CheckWait(CommandType.VIDEO, i, c);
                break;
            }
            if (c.type == CommandType.WAITFORSONG)
            {
                CheckWait(CommandType.SONG, i, c);
                break;
            }
            if (c.type == CommandType.WAITFORAUDIO)
            {
                CheckWait(CommandType.AUDIO, i, c);
                break;
            }
            if (c.type == CommandType.WAITFOROVERLAY)
            {
                CheckWait(CommandType.OVERLAY, i, c);
                break;
            }

            if (c.type == CommandType.SONG || c.type == CommandType.AUDIO)
            {
                /*if (c.file.EndsWith(".avi"))
                {
                    GameObject go = Instantiate(baseVideoPlayer);
                    VideoPlayer vp = go.GetComponent<VideoPlayer>();
                    videoScript vs = go.GetComponent<videoScript>();
                    vs.command = c;
                    vp.isLooping = c.loop;
                    vp.url = path + c.file;
                    vp.targetCamera = null;
                    vp.renderMode = VideoRenderMode.APIOnly;
                    if (c.type == CommandType.AUDIO)
                    {
                        playing_audio.Add(go);
                        vs.fadeOutFinished += AudioEndReached;
                        vp.prepareCompleted += AudioPrepared;
                    }
                    if (c.type == CommandType.SONG)
                    {
                        go.GetComponent<AudioSource>().volume *= 0.25f;
                        if(currentSong!=null)
                        {
                            Destroy(currentSong);
                        }
                        currentSong = go;
                        vs.fadeOutFinished += SongEndReached;
                        vp.prepareCompleted += SongPrepared;
                    }
                    vp.Prepare();
                }
                else*/ StartCoroutine(PlaySong(c));
                playlist.RemoveAt(i);
                i--;
            }
            if (c.type == CommandType.VIDEO || c.type == CommandType.OVERLAY)
            {
                LoadVideo(c);
                playlist.RemoveAt(i);
                if (c.callback != null) c.callback(c);
                i--;
            }
            if(c.type == CommandType.WAITTIME)
            {
                c.countdown -= Time.deltaTime;
                if (c.countdown <= 0)
                {
                    playlist.RemoveAt(i);
                    if (c.callback != null) c.callback(c);
                }
                break;
            }
            if(c.type==CommandType.CLEARVIDEOS)
            {
                ClearPlayingVideos(c.tags);
                playlist.RemoveAt(i);
                if (c.callback != null) c.callback(c);
                i--;
            }
        }
    }

    void EndReached(VideoPlayer vp)
    {
        if (vp == null) return;
        for (int i = 0; i < playing_videos.Count; i++)
        {
            var vs = playing_videos[i].GetComponent<videoScript>();
            var c = vs.command;
            if (playing_videos[i] == vp.gameObject)
            {
                if (c.callback != null) c.callback(c);
                break;
            }
        }
    }

    void AudioEndReached(VideoPlayer vp)
    {
        SongEndReached(vp.gameObject);
    }

    void SongEndReached(VideoPlayer vp)
    {
        SongEndReached(vp.gameObject);
    }

    void SongEndReached(GameObject s)
    {
        //Debug.Log("playlist count == " + playlist.Count.ToString());
        if (s == null) return;
        for(int i=0;i<playing_audio.Count;i++)
        {
            var go = playing_audio[i];
            var vs = go.GetComponent<videoScript>();
            var c = vs.command;
            if(c.type == CommandType.SONG || c.type==CommandType.AUDIO)
            {
                if(go==s)
                {
                    if (c.callback != null) c.callback(c);
                    playing_audio.RemoveAt(i);
                    i--;
                }
            }
        }
        if(currentSong==s)
        {
            currentSong = null;
            var vs = s.GetComponent<videoScript>();
            var c = vs.command;
            if (c.callback != null) c.callback(c);
        }
        if(s!=null) Destroy(s);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //vp.targetCameraAlpha = 0;
        vp.Play();
        if (vp.GetComponent<videoScript>().command.type != CommandType.OVERLAY) for (int i = 0; i < playing_videos.Count; i++)
            {
                var vs = playing_videos[i].GetComponent<videoScript>();
                if (vs.done && vs.command.type == CommandType.VIDEO)
                {
                    Destroy(vs.gameObject, 0.1f + vs.command.fadeOutTime);
                    playing_videos.RemoveAt(i);
                    i--;
                }
            }
        /*for(int i=0;i<playlist.Count;i++)
        {

        }*/
        //for()
    }

    void SongPrepared(VideoPlayer vp)
    {
        //if (currentSong != null) Destroy(currentSong);
        currentSong = vp.gameObject;
        vp.Play();
    }

    void AudioPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnDestroy()
    {
        if (currentSong) Destroy(currentSong);
        currentSong = null;
        foreach(var v in playing_videos)
        {
            Destroy(v);
        }
        foreach (var a in playing_audio)
        {
            Destroy(a);
        }
        playing_videos.Clear();
        playing_audio.Clear();
        playlist.Clear();
    }
}

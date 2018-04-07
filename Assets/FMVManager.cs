﻿using System.Collections;
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
        //public GameObject player = null;
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
        public bool loop = false;
    };

    public List<Command> playlist = new List<Command>();
    public List<GameObject> playing_videos = new List<GameObject>();
    public List<GameObject> playing_audio = new List<GameObject>();
 
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

    public bool HasTags(string haystack, string needle)
    {
        if (haystack.Contains(needle)) return true;//later make this smarter, to accept space separated classes
        else return false;
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

    IEnumerator PlaySong(Command c)
    {
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
                playing_audio.Add(go);
                vs.fadeOutFinished += AudioEndReached;
                vp.prepareCompleted += AudioPrepared;
            }
            if (c.type == CommandType.SONG)
            {
                go.GetComponent<AudioSource>().volume *= 0.25f;
                if (currentSong != null)
                {
                    Destroy(currentSong);
                }
                currentSong = go;
                vs.fadeOutFinished += SongEndReached;
                vp.prepareCompleted += SongPrepared;
            }
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
                source.volume *= 0.25f;
                if (currentSong != null)
                {
                    Destroy(currentSong);
                }
                currentSong = source.gameObject;
                vs.fadeOutFinished += SongEndReached;
            }
            else if (c.type == CommandType.AUDIO)
            {
                playing_audio.Add(source.gameObject);
                vs.fadeOutFinished += AudioEndReached;
            }
            source.Play();
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
        vs.command = c;
        vs.transparentColor = c.transparentColor;
        vs.fadeInTime = c.fadeInTime;
        vs.fadeOutTime = c.fadeOutTime;
        vs.threshold = c.threshold;
        vs.slope = c.slope;
        vp.isLooping = c.loop;
        vp.url = path + c.file;
        vp.playbackSpeed = c.playbackSpeed;
        vp.prepareCompleted += PrepareCompleted;
        //vp.loopPointReached += EndReached;
        vs.fadeOutFinished += EndReached;
        vp.Prepare();
        playing_videos.Add(go);
        return go;
    }

    // Update is called once per frame
    void Update()
    {
        PlaylistProcess();
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
        Debug.Log("CheckWait going forwards!");
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
                i--;
            }
            if(c.type == CommandType.WAITTIME)
            {
                c.countdown -= Time.deltaTime;
                if(c.countdown<=0)
                    playlist.RemoveAt(i);
                break;
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
                if (vs.done)
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

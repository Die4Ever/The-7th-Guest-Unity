using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class menu : MonoBehaviour {
    FMVManager fmvman;
    videoScript myvid=null;
    MeshRenderer rp = null;
    public Texture2D sphinx_background;
    float YSpeed = 0.0f;
	// Use this for initialization
	void Start () {
        fmvman = GameObject.FindObjectOfType<FMVManager>();
        PlaySong("GU61");
        QueueOverlay("INTRO/sphinx", MenuReady, Color.black, z:500);
        PlaySound("GAMWAV/gen_s_19");
        rp = GetComponentInChildren<MeshRenderer>();
        var m = rp.material;
        //Destroy(m.mainTexture);
        m.mainTexture = sphinx_background;
        rp.transform.position = new Vector3(0, 0, 501);
        m.SetColor("_keyingColor", new Color(0, 0, 1));
        m.SetFloat("_thresh", 1);
        m.SetFloat("_slope", 1);
        fmvman.playlist.Add(new FMVManager.Command { type= FMVManager.CommandType.WAITTIME, countdown=10, callback= NewGame });
    }
	
	void MenuReady(FMVManager.Command c)
    {
        foreach(var v in fmvman.playing_videos)
        {
            var vs = v.GetComponent<videoScript>();
            var com = vs.command;
            if (fmvman.HasTags(com.tags, "menu"))
            {
                myvid = vs;
                if(myvid.rp!=null) myvid.rp.transform.SetParent(transform);
            }
        }
    }

    void NewGame(FMVManager.Command c)
    {
        if (myvid == null) MenuReady(c);

        myvid.rp.transform.SetParent(transform);
        fmvman.PlaySong(new FMVManager.Command { file = "", type = FMVManager.CommandType.SONG });
        fmvman.SwitchRoom("intro", 1, 'a');
        YSpeed = 10.0f;
    }

    private void Update()
    {
        float scale = Camera.main.aspect / 2.0f;
        if (scale > 1.0f) scale = 1.0f;
        rp.transform.localScale = new Vector3(scale * 2.0f, 1, scale);
        if (YSpeed!=0)
        {
            transform.position = new Vector3(0, transform.position.y + YSpeed * Time.deltaTime, 0);
            if (transform.position.y > 100.0f) YSpeed = 0;
        }
    }

    protected void QueueOverlay(string file, System.Action<FMVManager.Command> callback, Color transparentColor, string tags = "", bool wait = false, bool freezeFrame = false, float z = 0, float threshold = 0.1f, float slope = 0.5f)
    {
        fmvman.QueueOverlay(new FMVManager.Command { file = file, callback = callback, transparentColor = transparentColor, type = FMVManager.CommandType.OVERLAY, tags = tags + " menu", freezeFrame = freezeFrame, z = z, threshold = threshold, slope = slope }, wait);
    }

    protected void PlaySong(string file, bool loop = true)
    {
        fmvman.PlaySong(new FMVManager.Command { file = "../music/" + file + ".ogg", type = FMVManager.CommandType.SONG, tags = "menu", loop = loop });
    }

    protected void PlaySound(string file)
    {
        fmvman.PlayAudio(new FMVManager.Command { file = file, type = FMVManager.CommandType.AUDIO, tags = "menu" }, false);
    }
}


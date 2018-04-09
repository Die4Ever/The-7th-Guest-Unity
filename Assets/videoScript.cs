using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class videoScript : MonoBehaviour
{
    public float fadeInTime = 0;
    public float fadeOutTime = 0;
    float fadeInSpeed = 0;
    float fadeOutSpeed = 0;
    VideoPlayer vp = null;
    bool fadingOut = false;
    public bool done = false;
    public bool prepared = false;
    public VideoPlayer.EventHandler fadeOutFinished;
    public Color transparentColor = new Color(0, 0, 0, 0);
    public GameObject baseRenderPlane;
    public float threshold = 0.24f;
    public float slope = 0.6f;
    public bool freezeFrame = false;
    GameObject rp;
    public FMVManager.Command command;

    // Use this for initialization
    void Start()
    {
        vp = this.gameObject.GetComponent<VideoPlayer>();
        vp.started += VideoStarted;//I need to make a separate event for finishing the fade out
        vp.targetCameraAlpha = 0.0f;
        vp.prepareCompleted += PrepareCompleted;
        vp.loopPointReached += EndReached;
        vp.targetCamera = Camera.main;
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if(rp!=null)
        {
            if(rp.transform.position.z>9000 && prepared) rp.transform.position = new Vector3(0, 0, command.z);
            float scale = Camera.main.aspect / 2.0f;
            if (scale > 1.0f) scale = 1.0f;
            rp.transform.localScale = new Vector3(scale*2.0f, 1, scale);
        }
        if(done==false && (command.type == FMVManager.CommandType.AUDIO || command.type== FMVManager.CommandType.SONG) )
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource && audioSource.clip) if (audioSource.clip.loadState == AudioDataLoadState.Loaded || audioSource.clip.loadState == AudioDataLoadState.Failed) if (audioSource.isPlaying == false)
                    {
                        Debug.Log("detected song end "+command.file);
                        EndReached(vp);
                        prepared = true;
                    }
        }
        if (prepared == false) return;

        if (fadeInTime > 0)
        {
            vp.targetCameraAlpha += fadeInSpeed * Time.deltaTime;
            fadeInTime -= Time.deltaTime;

            if(fadeInTime <= 0)
            {
                vp.targetCameraAlpha = 1;
                if (freezeFrame == false) vp.Play();
                else EndReached(vp);
            }
        }
        else if(fadingOut)
        {
            if (fadeOutTime > 0)
            {
                vp.targetCameraAlpha -= fadeOutSpeed * Time.deltaTime;
                fadeOutTime -= Time.deltaTime;
            }
            else if(fadeOutSpeed > 0)
            {
                vp.targetCameraAlpha = 0;
                done = true;
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
                Destroy(this.gameObject, 15);
            }
            else if(done==false)
            {
                done = true;
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
                //Destroy(this.gameObject);//only destroy when there is a video to replace it?
            }
        }
        /*else if (vp.targetCameraAlpha != 1)
        {
            vp.targetCameraAlpha = 1;
            vp.Play();
        }*/
    }

    void VideoStarted(VideoPlayer vp)
    {
        done = false;

        if (fadeInTime != 0) fadeInSpeed = 1 / fadeInTime;
        if (fadeOutTime != 0) fadeOutSpeed = 1 / fadeOutTime;
        if (fadeInTime > 0) vp.targetCameraAlpha = 0;
        else vp.targetCameraAlpha = 1;

        //if (transparentColor.a > 0)
        //if (command.type== FMVManager.CommandType.OVERLAY) rp.GetComponent<MeshRenderer>().material.SetColor("_keyingColor", transparentColor);
        /*{
            vp.renderMode = VideoRenderMode.RenderTexture;
            rp = Instantiate(baseRenderPlane, new Vector3(0, 0, command.z), Quaternion.Euler(90, -90, 90));
            var r = rp.GetComponent<MeshRenderer>();
            var m = r.material;
            m.mainTexture = Instantiate(m.mainTexture);
            Debug.Log(m.name);
            Debug.Log(m.ToString());
            vp.targetTexture = m.mainTexture as RenderTexture;
            m.SetFloat("_thresh", threshold);
            m.SetFloat("_slope", slope);
            m.SetColor("_keyingColor", transparentColor);
            //vp.targetTexture.width = Screen.width;
            //vp.targetTexture.height = Screen.height;
        }*/

        if (fadeInTime > 0)
        {
            vp.targetCameraAlpha = 0;
            vp.Pause();
            //vp.targetCamera = Camera.main;
        }
        else
        {
            vp.targetCameraAlpha = 1;
            if (freezeFrame)
            {
                Debug.Log("freeze frame!");
                vp.Pause();
                EndReached(vp);
            }
            //vp.targetCamera = Camera.main;
        }

        //if (command.type == FMVManager.CommandType.OVERLAY) rp.transform.position = new Vector3(0, 0, command.z);
    }

    void EndReached(VideoPlayer vp)
    {
        //Debug.Log("EndReached");
        fadingOut = true;
        /*done = true;
        if(fadeOutFinished!=null)
            fadeOutFinished.Invoke(vp);
        fadeOutFinished = null;*/
        //Destroy(this.gameObject, fadeOutTime);
    }

    public void Init()
    {
        done = false;
        //if (transparentColor.a > 0)
        if (command.type == FMVManager.CommandType.OVERLAY)
        {
            vp.renderMode = VideoRenderMode.RenderTexture;
            rp = Instantiate(baseRenderPlane, new Vector3(0, 0, 9001.0f), Quaternion.Euler(90, -90, 90));//init it out of camera
            var r = rp.GetComponent<MeshRenderer>();
            var m = r.material;
            m.mainTexture = Instantiate(m.mainTexture);
            Debug.Log(m.name);
            Debug.Log(m.ToString());
            m.SetFloat("_thresh", threshold);
            m.SetFloat("_slope", slope);
            m.SetColor("_keyingColor", transparentColor);
            vp.targetTexture = m.mainTexture as RenderTexture;
            //vp.targetTexture.width = Screen.width;
            //vp.targetTexture.height = Screen.height;
        }
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //Debug.Log("PrepareCompleted");
        //fadeInTime = 0;
        //fadeOutTime = 0;
        done = false;
        prepared = true;
        /*if (fadeInTime != 0) fadeInSpeed = 1 / fadeInTime;
        if(fadeOutTime != 0) fadeOutSpeed = 1 / fadeOutTime;
        if(fadeInTime>0) vp.targetCameraAlpha = 0;
        else vp.targetCameraAlpha = 1;*/
    }

    private void OnDestroy()
    {
        vp.renderMode = VideoRenderMode.APIOnly;
        if(vp.targetTexture!=null)
        {
            Destroy(vp.targetTexture);
            vp.targetTexture = null;
        }
        if(rp!=null)
        {
            Destroy(rp);
            rp = null;
        }
    }
}

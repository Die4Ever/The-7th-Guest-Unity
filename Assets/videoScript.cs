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
    public VideoPlayer.EventHandler fadeOutFinished;
    public Color transparentColor = new Color(0, 0, 0, 0);
    public GameObject baseRenderPlane;
    public float threshold = 0.24f;
    public float slope = 0.6f;
    public bool freezeFrame = false;
    GameObject rp;

    // Use this for initialization
    void Start()
    {
        vp = this.GetComponent<VideoPlayer>();
        vp.started += VideoStarted;//I need to make a separate event for finishing the fade out
        vp.targetCameraAlpha = 0.0f;
        vp.prepareCompleted += PrepareCompleted;
        vp.loopPointReached += EndReached;
        vp.targetCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (fadeInTime > 0)
        {
            vp.targetCameraAlpha += fadeInSpeed * Time.deltaTime;
            fadeInTime -= Time.deltaTime;

            if(fadeInTime <= 0)
            {
                vp.targetCameraAlpha = 1;
                if(freezeFrame==false) vp.Play();
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
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
                Destroy(this.gameObject, 15);
            }
            else
            {
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
                //Destroy(this.gameObject);//only destroy when there is a video to replace it?
            }
        }
        else if (vp.targetCameraAlpha != 1)
        {
            vp.targetCameraAlpha = 1;
            vp.Play();
        }
    }

    void VideoStarted(VideoPlayer vp)
    {
        if (fadeInTime > 0)
        {
            vp.targetCameraAlpha = 0;
            vp.Pause();
            //vp.targetCamera = Camera.main;
        }
        else
        {
            vp.targetCameraAlpha = 1;
            if(freezeFrame) vp.Pause();
            //vp.targetCamera = Camera.main;
        }
    }

    void EndReached(VideoPlayer vp)
    {
        //Debug.Log("EndReached");
        fadingOut = true;
        if(fadeOutFinished!=null)
            fadeOutFinished.Invoke(vp);
        fadeOutFinished = null;
        //Destroy(this.gameObject, fadeOutTime);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        //Debug.Log("PrepareCompleted");
        //fadeInTime = 0;
        //fadeOutTime = 0;
        if(fadeInTime != 0) fadeInSpeed = 1 / fadeInTime;
        if(fadeOutTime != 0) fadeOutSpeed = 1 / fadeOutTime;
        if(fadeInTime>0) vp.targetCameraAlpha = 0;
        else vp.targetCameraAlpha = 1;

        if(transparentColor.a>0)
        {
            vp.renderMode = VideoRenderMode.RenderTexture;
            rp = Instantiate(baseRenderPlane, new Vector3(0, 0, 0), Quaternion.Euler(90, -90, 90));
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

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

    // Use this for initialization
    void Start()
    {
        vp = this.GetComponent<VideoPlayer>();
        vp.started += VideoStarted;//I need to make a separate event for finishing the fade out
        vp.targetCameraAlpha = 0.0f;
        vp.prepareCompleted += PrepareCompleted;
        vp.loopPointReached += EndReached;
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
                vp.Play();
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
                Destroy(this.gameObject, 15.1f);
                vp.targetCameraAlpha = 0;
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
            }
            else
            {
                Destroy(this.gameObject, 15.1f);
                if (fadeOutFinished != null)
                {
                    fadeOutFinished.Invoke(vp);
                    fadeOutFinished = null;
                }
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
            vp.targetCamera = Camera.main;
        }
        else
        {
            vp.targetCameraAlpha = 1;
            vp.targetCamera = Camera.main;
        }
    }

    void EndReached(VideoPlayer vp)
    {
        Debug.Log("EndReached");
        fadingOut = true;
        fadeOutFinished.Invoke(vp);
        fadeOutFinished = null;
        //Destroy(this.gameObject, fadeOutTime);
    }

    void PrepareCompleted(VideoPlayer vp)
    {
        Debug.Log("PrepareCompleted");
        //fadeInTime = 0;
        //fadeOutTime = 0;
        if(fadeInTime != 0) fadeInSpeed = 1 / fadeInTime;
        if(fadeOutTime != 0) fadeOutSpeed = 1 / fadeOutTime;
        if(fadeInTime>0) vp.targetCameraAlpha = 0;
        else vp.targetCameraAlpha = 1;
    }
}

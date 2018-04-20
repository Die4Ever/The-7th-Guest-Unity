using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class library : baseRoom
{
    const int li_door = 1, li_mid = 2, telescope = 90, book = 91;
    // Use this for initialization
    void Start()
    {
        BaseInit();
        myvidpath = "LI/l";
        //PlaySong("../music/GU56.ogg");//dining room doesn't change music until the cutscene?
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicBootScript : MonoBehaviour 
{
    //Script used to boot music and audio setting handling
	void Start () 
    {

        if (PlayerPrefs.GetInt("MM_SoundEnabled", -1) != -1)
            this.GetComponent<PanelMenuButtonsNoNetwork>().SetAudio("Sound", PlayerPrefs.GetInt("MM_SoundEnabled", -1));

        if (PlayerPrefs.GetInt("MM_MusicEnabled", -1) != -1)
            this.GetComponent<PanelMenuButtonsNoNetwork>().SetAudio("Music", PlayerPrefs.GetInt("MM_MusicEnabled", -1));
    }
}

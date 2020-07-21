using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicToggler : MonoBehaviour {

    public float MaxVolume = 0.2f;
    public float MinVolume = 0f;
    public float MusicFadeTime = 0.5f;

    private AudioSource myMusic;
    private WaitingPanelManager myWaitingManager;
    private float timer = 0f;

    //Script is used to toggle music audio elegantly
	void Start () 
    {
        myMusic = this.GetComponent<AudioSource>();
        myWaitingManager = transform.parent.GetComponentInChildren<WaitingPanelManager>();
    }
	
	void Update () 
    {
		if (myWaitingManager.InGame)//Should play music
        {
            if (!myMusic.isPlaying)
            {
                myMusic.Play();
                myMusic.volume = MaxVolume;
            }
        }
        else//Shouldn't play music
        {
            if (myMusic.isPlaying && myMusic.volume > MinVolume)
            {
                if (myMusic.volume == MaxVolume)
                    timer = 0f;
                timer += Time.deltaTime;
                myMusic.volume = Mathf.Lerp(MaxVolume, MinVolume, timer / MusicFadeTime);
            }
            else if (myMusic.isPlaying)
                myMusic.Stop();
        }
	}
}

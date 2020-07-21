using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using GoogleMobileAds.Api;
using TMPro;

public class InitialScript : MonoBehaviour {

    public static bool Initialized = false;
    public float ShowTime = 2f;//In seconds
    public float FadeAwayTime = 1f;//In seconds
    public Image BlackPanel;
    public GameObject musicManager;
    public TextMeshProUGUI loadingtext;
    public bool DoneLoading = false;
    public string UnityAdsInitializeCode;
    public string AdMobInitializeCode;
    private float timer = 0f;


    //Initialize ads on awake
    void Awake()
    {
        musicManager.SetActive(true);
        Advertisement.Initialize(UnityAdsInitializeCode);
        MobileAds.Initialize(AdMobInitializeCode);
    }

	//Make sure this object is shown once per boot of game
	void Start () {
        if (Initialized)
            Destroy(gameObject);
	}
	
	//Update will show the initial boot panel for a set amount of time
	void Update () {
		if (Initialized == false && DoneLoading)
        {
            if (timer> ShowTime + FadeAwayTime)
            {
                Initialized = true;
                gameObject.SetActive(false);
            }
            timer += Time.deltaTime;
            BlackPanel.color = Color.Lerp(Color.clear, Color.black, (timer - ShowTime)/FadeAwayTime);
            if (loadingtext.text.Contains("ERROR - "))//If there was an error as the game boots, don't let the players play the game
                timer = 0f;
        }
	}
}

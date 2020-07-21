using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using UnityEngine.SceneManagement;
using TMPro;
using GoogleMobileAds.Api;
using System;

public class BannerScript : MonoBehaviour
{
    public bool IsOnGooglePlay = true;
    public Sprite[] AdImages;
    public float DifferentImageTimer = 12.5f;
    public string Ad_Unit_ID_Android;
    public string Ad_Unit_ID_iOS;

    private float timer = 0f;
    private int adImageIndex = 0;
    private BannerView bannerView;

    static private bool enabledAds = false;


    //In the case no real banner ad is shown, this will show and use a placeholder banner to my game You're Not Supposed To Be Here.
    private void Update()
    {
        if (!IsOnGooglePlay)
        if (timer >= DifferentImageTimer)
        {
            timer = 0;
            adImageIndex++;
            if (adImageIndex >= AdImages.Length)
                adImageIndex = 0;
            this.GetComponent<Image>().sprite = AdImages[adImageIndex];
        }
        timer += Time.deltaTime;
    }

    public void OnAdPress()
    {
        if (!IsOnGooglePlay) //In the case no real banner ad is shown, this will show and use a placeholder banner to my game You're Not Supposed To Be Here.
            Application.OpenURL("https://play.google.com/store/apps/details?id=emmetgames.game.youre_not_supposed_to_be_here");
    }

    //When object enabled, begin banner creation
    public void OnEnable()
    {
        this.RequestBanner();
    }


    //When object disables, begin banner deletion
    void OnDisable()
    {
        this.bannerView.Hide();
        this.bannerView.Destroy();
    }

    private void Awake()
    {
        if (!enabledAds) //Ad initialization happens in Initial script. Ads must be initialized for ad to work.
            enabledAds = true;
    }

    //Request AdMob banner ad, and then display it
    private void RequestBanner()
    {
#if UNITY_ANDROID
        string adUnitId = Ad_Unit_ID_Android; //For testing code, use the following val: ca-app-pub-3940256099942544/6300978111
#elif UNITY_IPHONE
            string adUnitId = Ad_Unit_ID_iOS;
#else
            string adUnitId = "unexpected_platform";
#endif
        Debug.Log("About to create an AdMob banner ad");
        // Create a 320x50 banner at the top of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.SmartBanner, AdPosition.Top);

        // Called when an ad request has successfully loaded.
        this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        // Called when an ad request failed to load.
        this.bannerView.OnAdFailedToLoad += this.HandleOnAdFailedToLoad;
        // Called when an ad is clicked.
        this.bannerView.OnAdOpening += this.HandleOnAdOpened;
        // Called when the user returned from the app after an ad click.
        this.bannerView.OnAdClosed += this.HandleOnAdClosed;
        // Called when the ad click caused the user to leave the application.
        this.bannerView.OnAdLeavingApplication += this.HandleOnAdLeavingApplication;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();

        // Load the banner with the request.
        this.bannerView.LoadAd(request);
        Debug.Log("AdMob banner ad created");
    }


    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLoaded event received");
        MonoBehaviour.print("HandleAdLoaded event received");
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        Debug.Log("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
        MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                            + args.Message);
    }

    public void HandleOnAdOpened(object sender, EventArgs args)
    {
        Debug.Log("HandleAdOpened event received");
        MonoBehaviour.print("HandleAdOpened event received");
    }

    public void HandleOnAdClosed(object sender, EventArgs args)
    {
        Debug.Log("HandleAdClosed event received");
        MonoBehaviour.print("HandleAdClosed event received");
    }

    public void HandleOnAdLeavingApplication(object sender, EventArgs args)
    {
        Debug.Log("HandleAdLeavingApplication event received");
        MonoBehaviour.print("HandleAdLeavingApplication event received");
    }
}

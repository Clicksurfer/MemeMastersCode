using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.Networking;
using TMPro;
using System.IO;
using UnityEngine.UI;

public class MemeLoaderScript : MonoBehaviour
{
    public List<Sprite> memes;
    public List<string> memePackage;

    public TextMeshProUGUI loadingText;
    public InitialScript InitialLoadingPanel;
    public Slider InitialLoadSlider;
    public float TimeToDownloadText = 5f;

    private static bool created = false;
    bool shouldDisplayStuff = false;
    private LoadAssetBundles mine;
    private string MemesFileName;
    private TextAsset textXml;
    private XmlDocument xmlDoc;
    float timer = 0f;
    private bool showDownloadText = false;
    private string downloadTextLong = "";

    // Script is used to load and contains memes from asset bundles
    // Script contains functions that help retreive relevant meme packages from it
    // Script also updates UI accordingly
    void Start()
    {
        mine = this.GetComponent<LoadAssetBundles>();
        memes = new List<Sprite>();
        memePackage = new List<string>();
        DontDestroyOnLoad(gameObject);
        LoadFromAssetBundles();
    }

    private void LoadFromAssetBundles()
    {
        mine.GetAllAssetBundles();
        shouldDisplayStuff = true;
    }

    //Make sure only one MemeLoaderScript object exists
    private void Awake()
    {
        if (created)
            Destroy(gameObject);
        else
            created = true;
    }

    // Update should make sure we retreive the relevant UI objects
    void Update()
    {
        if (shouldDisplayStuff)
        {
            if (loadingText == null)
                loadingText = GameObject.FindGameObjectWithTag("InitialLoadingText").GetComponent<TextMeshProUGUI>();
            if (InitialLoadingPanel == null)
                InitialLoadingPanel = GameObject.FindGameObjectWithTag("InitialLoadingPanel").GetComponent<InitialScript>();
            if (InitialLoadSlider == null)
                InitialLoadSlider = GameObject.FindGameObjectWithTag("InitialLoadingSlider").GetComponent<Slider>();
        }
    }

    //Function is used to update UI as handling asset bundles
    public void UpdateLoadingText()
    {
        if (shouldDisplayStuff)
        {
            float newVal = mine.GetDownloadProgress();//Show download progress
            InitialLoadSlider.value = newVal;

            if (mine.IsDownloadingPackages())//Display text saying it will take a while as downloading
                loadingText.text = getDownloadText();
            else
                loadingText.text = "Loading...";

            if (mine.AllPackagesLoaded)//Escaping for when everything's ready
            {
                Debug.Log("All pacakges loaded.");
                shouldDisplayStuff = false;
                loadingText.text = "Loading...";
                InitialLoadingPanel.DoneLoading = !shouldDisplayStuff;
            }
        }
    }

    //Returns a funny random text to show when downloading for first-time boot
    private string getDownloadText()
    {
        if (showDownloadText)
            return downloadTextLong;
        showDownloadText = true;
        string[] possibleArr = new string[]
        {
            "Why don't you ask your friends how their weekend was?",
            "Why don't you ask your friends what their plans for the weekend are?",
            "Why not take the time to regret your life choices?",
            "Why not take a moment to make some tea?",
            "Why don't you take a moment to share a fact with your friends?",
            "Why not talk to your friends about the good ol' days?",
            "Why not go to the bathroom while you wait?",
            "Why not make sure you're hydrated while you wait?",
            "Why not go grab some snacks while you wait?",
            "Why don't you take a moment to think about your favorite memes?"
        };
        downloadTextLong = "Downloading first-time assets - this may take a while. " + possibleArr[Random.Range(0, possibleArr.Length)];
        return (downloadTextLong);
    }
}
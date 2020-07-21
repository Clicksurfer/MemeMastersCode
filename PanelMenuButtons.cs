using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PanelMenuButtons : NetworkBehaviour {

    public GameObject ObjectToToggle;
    public string roomType = "";
    public RoomHandlerCode RoomManager;
    public AudioClip WindowOpenClip;
    public AudioClip WindowCloseClip;
    public static bool SfxEnabled = true;
    public static bool MusicEnabled = true;

    private int disableObjectForTimeCounter = 0;

    //Script contains lots of generic logics for various in-game buttons

    //Awake handles buttons in case they are sound toggle buttons
    private void Awake()
    {
        if (this.gameObject.name.Contains("SoundToggleButton"))
        {
            if (PlayerPrefs.GetInt("MM_SoundEnabled", -1) != -1)
                SetAudio("Sound", PlayerPrefs.GetInt("MM_SoundEnabled", -1));
        }
        if (this.gameObject.name.Contains("MusicToggleButton"))
        { 

            if (PlayerPrefs.GetInt("MM_MusicEnabled", -1) != -1)
                SetAudio("Music", PlayerPrefs.GetInt("MM_MusicEnabled", -1));
        }
    }

    //One of the most commonly used button functions, this makes a UI window gameobject appear with a nice animation
    public void ToggleUIObject()
    {
        if (!ObjectToToggle.activeSelf)
        {
            ObjectToToggle.SetActive(!ObjectToToggle.activeSelf);
            foreach(Transform child in transform.parent)
            {
                if (child.name!= "BlurPanel" && child.GetComponent<Animator>() != null)
                {
                    child.GetComponent<Animator>().SetBool("WindowOpen", true);
                }
            }
        }
        else
        {
            bool foundObjectToToggle = false;

            foreach (Transform child in ObjectToToggle.transform)
            {
                //Debug.Log("Child - " + child.name);
                if (child.name != "BlurPanel" && child.GetComponent<Animator>() != null)
                {
                    foundObjectToToggle = true;
                    //Debug.Log("Found the animator object!");
                    StartCoroutine(ClosePanelTransition(child.gameObject));
                }
            }
            if (!foundObjectToToggle && ObjectToToggle.GetComponent<Animator>() != null)
            {
                StartCoroutine(ClosePanelTransition(ObjectToToggle));
            }
        }
    }

    public void TogglePlayerLeaderboardObject()
    {
        if (ObjectToToggle.activeSelf)
        {
            this.GetComponent<AudioSource>().PlayOneShot(WindowCloseClip);
            StartCoroutine(DisableObjectForTime(1f / 2.3f));
        }
        else
        {
            this.GetComponent<AudioSource>().PlayOneShot(WindowOpenClip);
            StartCoroutine(DisableObjectForTime(1f / 1.8f));
        }
        ToggleUIObject();
    }

    private IEnumerator DisableObjectForTime (float timeVal)
    {
        float timer = 0f;
        disableObjectForTimeCounter++;
        int myCounter = disableObjectForTimeCounter;
        while (timeVal == 0 && timer < 1f)
        {
            timeVal = ObjectToToggle.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.length;
            timer += Time.deltaTime;
        }
        this.GetComponent<Button>().interactable = false;
        timer = 0f;
        while (timer < timeVal && myCounter == disableObjectForTimeCounter)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            timer += Time.deltaTime;
        }
        this.GetComponent<Button>().interactable = true;
    }

    IEnumerator ClosePanelTransition(GameObject animated)
    {
        animated.GetComponent<Animator>().SetBool("WindowOpen", false);
        yield return new WaitForSeconds(Time.deltaTime);
        while (animated.GetComponent<RectTransform>().localScale.x != 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            //Debug.Log("Still waiting for animation to end!");
        }
        ObjectToToggle.SetActive(false);
    }

    public void ToggleMatchPanel()
    {
        RoomManager.PopulateRoomPanel(roomType);
        ToggleUIObject();
    }


    public void ToggleAudio(string audioType)
    {
        bool newState = false;
        if (audioType == "Music")
            newState = !MusicEnabled;
        else if (audioType == "Sound")
            newState = !SfxEnabled;

        if (newState)
            SetAudio(audioType, 1);
        else
            SetAudio(audioType, 0);
    }

    //Function is used to set the volumes of all certain type of audio
    //By default it simply toggles them
    public void SetAudio(string audioType, int forceSet = 1)
    {
        bool newState = false;
        if (audioType == "Music")
            newState = !MusicEnabled;
        else if (audioType == "Sound")
            newState = !SfxEnabled;

        if (forceSet == 0)
            newState = false;
        else if (forceSet == 1)
            newState = true;

        if (this.gameObject.name.Contains("ToggleButton"))
        {
            if (newState)
                this.GetComponent<Image>().color = Color.green;
            else
                this.GetComponent<Image>().color = Color.red;
        }

        AudioSource[] audioSources = Resources.FindObjectsOfTypeAll(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource src in audioSources)
        {
            if (audioType == "Sound" && src.tag != "Music")
            {
                src.mute = !newState;
            }
            else if (audioType == "Music" && src.tag == "Music")
            {
                src.mute = !newState;
            }
        }

        if (audioType == "Sound")
        {
            SfxEnabled = newState;
            int soundEnabled = 0;
            if (SfxEnabled)
                soundEnabled = 1;
            PlayerPrefs.SetInt("MM_SoundEnabled", soundEnabled);
        }
        else if (audioType == "Music")
        {
            MusicEnabled = newState;
            int musicEnabled = 0;
            if (MusicEnabled)
                musicEnabled = 1;
            PlayerPrefs.SetInt("MM_MusicEnabled", musicEnabled);
        }
    }

    //Function quits online match safely
    public void QuitMatchButton()
    {
        PlayerPrefs.SetString("MM_CrashRoomcode", "");
        PlayerPrefs.SetString("MM_HostDecendantData", "");
        {
            NetworkManager.singleton.StopHost();

            NetworkManager.singleton.StopClient();
        }
        {
            Debug.Log("Disconnecting Server.");
            NetworkManager.singleton.StopHost();
        }
    }

    public void ShowPrivacyPolicy()
    {
        Application.OpenURL("https://meme-masters.flycricket.io/privacy.html");
    }

    public void QuitGameButton()
    {
        Application.Quit();
    }
}

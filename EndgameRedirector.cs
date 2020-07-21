using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndgameRedirector : MonoBehaviour {

    //Script is used to handle player redirection in regards to matches ending
	void Awake () {
        if (PlayerPrefs.GetString("MM_Temp", "") != "")
        {
            //Player was redirected here from the the match for the end of the game
            //It's time to redirect him to the end of the match
            PlayerPrefs.SetString("MM_CrashRoomcode", "");
            PlayerPrefs.SetString("MM_HostDecendantData", "");
            SceneManager.LoadScene("EndScene");
        }
    }

    private void Start()
    {
        if (PlayerPrefs.GetString("MM_CrashRoomcode", "") != "")
        {
            //Player crashed from a room, so we will try to reconnect
            StartCoroutine(DoCrashRoomCodeLogin());
        }
    }

    IEnumerator DoCrashRoomCodeLogin()
    {
        yield return new WaitForSeconds(Time.deltaTime * 2);
        if (PlayerPrefs.GetString("MM_HostDecendantData", "") != "")
        {
            Debug.Log("Player crashed from room " + PlayerPrefs.GetString("MM_HostDecendantData", "") + ", and he's going to be the new host!");
            GameObject.FindGameObjectWithTag("MatchManager").GetComponent<RoomHandlerCode>().ButtonStartServer(PlayerPrefs.GetString("MM_CrashRoomcode", ""));
        }
        else
        {
            Debug.Log("Player crashed from room " + PlayerPrefs.GetString("MM_CrashRoomcode", "") + "!");
            yield return new WaitForSeconds(1f);
            GameObject.FindGameObjectWithTag("MatchManager").GetComponent<RoomHandlerCode>().ReconnectToMatch(PlayerPrefs.GetString("MM_CrashRoomcode", ""));
        }
    }
}

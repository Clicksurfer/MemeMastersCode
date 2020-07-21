using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class PlayerCode : NetworkBehaviour
{
    public string PlayerUID;
    public string PlayerName;
    public string PlayerTitle;
    public string MyMemeText;
    public string VotedMemeMadeBy = "";
    public Text MyMainMemeText;
    public float ServerUpdateFreq = 3f;
    public bool ReadyToTerminate = false;
    public AudioClip roundWin;
    public AudioClip roundLost;
    public bool HostDecendant = false;

    private TMP_InputField MyInputField;
    private GameObject gameManager;
    private TurnManager myTurnManager;
    private float serverUpdateOffset;
    private bool updatedTextYet = false;
    private bool checkLocalPlayer = false;

    // This important script is attached to all Player Gameobjects. This is the unique object that identifies them in matches.
    // It contains logic for local player behavior, and stores player data
    
    //Upon boot, update the server and all other players of your presence and your data
    void Start()
    {
        if (isServer)
            Debug.Log("I am the server!");

        if (isLocalPlayer)
        {
            //This is code that only works if this is the local player!

            /*
            PlayerUID = PlayerPrefs.GetInt("MM_UID", Random.Range(0, 10000000)).ToString();// GenerateRandomUID();
            PlayerName = PlayerPrefs.GetString("MM_Username", GenerateRandomName());
            PlayerTitle = PlayerPrefs.GetString("MM_PlayerTitle", "");
            */

            ///*
            //TESTING CODE
            PlayerUID = GenerateRandomUID();// GenerateRandomUID();
            PlayerName = GenerateRandomName();
            PlayerTitle = GenerateRandomTitle();
            //END OF TESTING CODE
            //*/

            serverUpdateOffset = Random.Range(0f, 1 / ServerUpdateFreq);
            UpdatePlayerObjectName();
            CmdUpdateNewPlayerDetails(PlayerName, PlayerTitle, PlayerUID, ReadyToTerminate);
        }
        gameManager = GameObject.Find("ManagerMatch");
        myTurnManager = gameManager.GetComponentInChildren<TurnManager>();

        if (isLocalPlayer)
        {
            PlayerPrefs.SetString("MM_CrashRoomcode", GameObject.FindGameObjectWithTag("MatchManager").GetComponent<RoomHandlerCode>().RoomCodeConnectedTo);
        }
    }

    //When the object is enabled, prepare match objects accordingly
    void OnEnable()
    {
        Start();
        myTurnManager.CmdSetupMainMemeForMe(PlayerUID);
        myTurnManager.CmdGetMyTurnStage(PlayerUID);
        checkLocalPlayer = true;
    }

    
    //Using update, we know when to enable or disable certain functionality depending on where we are in the match
    void Update()
    {
        if (isLocalPlayer)
        {
            if (checkLocalPlayer)
            {
                checkLocalPlayer = false;
                if (myTurnManager.TurnStage == 5 || myTurnManager.TurnStage == 7 || myTurnManager.TurnStage == 9)
                {
                    Debug.Log("Local player is turning waiting for next round panel on");
                    myTurnManager.IsStageEndLoadingPanelEnabled(true);
                }

                myTurnManager.GetComponent<MatchManager>().CmdUpdatePlayerListUI("");
            }

            //This is code that only works if this is the local player!
            if (MyMainMemeText == null)
            {
                try
                {
                    MyMainMemeText = GameObject.Find("MemeTextNormalMain").GetComponent<Text>();
                }
                catch
                {

                }
            }
            else if (myTurnManager.TurnStage == 3)
                MyMainMemeText.text = MyMemeText;

            if (MyInputField == null)
            {
                try
                {
                    MyInputField = GameObject.Find("InputFieldMeme").GetComponent<TMP_InputField>();
                    MyInputField.interactable = true;
                }
                catch
                {

                }
            }
            else if (myTurnManager.TurnStage == 3)
            {
                UpdateMemeText(MyInputField.text);
            }
        }
    }

    //When player disconnects, notify the UI manager
    void OnDisable()
    {
        myTurnManager.gameObject.GetComponent<MatchManager>().CmdUpdatePlayerListUI(PlayerUID);
    }

    /// <summary>
    /// These are functions for testing, and should not be used in the final version.
    /// Instead, they are here for testing purposes only.
    /// Use them as intended!
    /// </summary>
    /// 
    private string GenerateRandomName()
    {
        string[] Names = new string[10] { "Nimrod", "Inbar", "Gal", "Shachar", "Naama", "Robin", "Ilay", "Omri", "Amit", "Batman" };
        return (Names[Random.Range(0, Names.Length)]);
    }

    private string GenerateRandomUID()
    {
        return (Random.Range(1, 9999999).ToString());
    }

    private string GenerateRandomTitle()
    {
        string[] Titles = new string[10] { "The Great", "The Terrible", "The Wicked", "The Good", "The Bad", "The Ugly", "The Master", "The Unknown", "The Unready", "The Hilarious" };
        return (Titles[Random.Range(0, Titles.Length)]);
    }

    /// <summary>
    /// End of testing funcs
    /// </summary>

    [Command]
    void CmdUpdateNewPlayerDetails(string plName, string plTitle, string plUID, bool plTerminate)
    {
        PlayerName = plName;
        PlayerTitle = plTitle;
        PlayerUID = plUID;
        ReadyToTerminate = plTerminate;
        UpdatePlayerObjectName();
        RpcUpdateNewPlayerDetails(plName, plTitle, plUID ,plTerminate);
    }

    [ClientRpc]
    void RpcUpdateNewPlayerDetails(string plName, string plTitle, string plUID, bool plTerminate)
    {
        if (!hasAuthority)
        {
            PlayerName = plName;
            PlayerTitle = plTitle;
            PlayerUID = plUID;
            ReadyToTerminate = plTerminate;
            UpdatePlayerObjectName();
        }
    }

    void UpdatePlayerObjectName()
    {
        gameObject.name = "PlayerObject [" + PlayerName + " " + PlayerTitle + "]";
    }

    public void SetAsHostDecendant()
    {
        RpcSetAsHostDecendant();
    }

    [ClientRpc]
    private void RpcSetAsHostDecendant()
    {
        if (IsLocalPlayer())
        {
            HostDecendant = true;
            GameObject.Find("ManagerMatch").GetComponent<MatchManager>().HostDecendant = HostDecendant;
        }
    }

    public void UpdateMemeText(string memeText)
    {
        if (!isLocalPlayer)
            return;
        MyMemeText = memeText;
        if (((myTurnManager.GetTime() + serverUpdateOffset) % (1f / ServerUpdateFreq) < (1f / ServerUpdateFreq) / 2f && updatedTextYet == false) || myTurnManager.GetTime() == myTurnManager.GetWaitingForInputTime())
        {
            updatedTextYet = true;
            CmdUpdateMemeText(MyMemeText);
        }
        else if ((myTurnManager.GetTime() + serverUpdateOffset) % (1f / ServerUpdateFreq) > (1f / ServerUpdateFreq) / 2f && updatedTextYet == true)
        {
            updatedTextYet = false;
        }
    }

    public void LockSubmitButtonPress()
    {
        if (IsLocalPlayer())
        {
            CmdLockSubmitbuttonPress();
            MyInputField.interactable = false;
        }
    }

    [Command]
    public void CmdLockSubmitbuttonPress()
    {
        CmdUpdateMemeText(MyMemeText);
        myTurnManager.LockPlayer(PlayerUID);
    }

    [Command]
    public void CmdLockVoteButtonPress()
    {
        myTurnManager.LockPlayer(PlayerUID);
    }

    [Command]
    public void CmdUnlockSubmitButtonPress()
    {
            myTurnManager.UnlockPlayer(PlayerUID);
    }

    public void UnlockSubmitButtonPress()
    {
        if (IsLocalPlayer())
        {
            CmdUnlockSubmitButtonPress();
            if (MyInputField != null)
                MyInputField.interactable = true;
        }
    }

    public void PayPlayer(string compressedPlayerList)
    {
        if (!isLocalPlayer)
            return;
        List<string> playerRankList = TurnManager.StringDeseparator(compressedPlayerList);
        for (int i = 0; i < playerRankList.Count; i++)
        {
            if (playerRankList[i] == (PlayerName + " " + PlayerTitle))
            {
                PlayerPrefs.SetInt("MM_MoneyEarned", Mathf.Clamp((playerRankList.Count - i), 0, 5));
                PlayerPrefs.SetInt("MM_Money", PlayerPrefs.GetInt("MM_Money", 0) + Mathf.Clamp((playerRankList.Count - i), 0, 5));
                break;
            }
        }
    }

    public void PlayRoundEndMusic(string winnerUID)
    {
        if (!isLocalPlayer)
        {
            Debug.Log("Not local player. Not playing round end music");
            return;
        }
        if (winnerUID == PlayerUID)
        {
            this.GetComponent<AudioSource>().PlayOneShot(roundWin);
            Debug.Log("Won round, so playing winner music");
        }
        else
        {
            this.GetComponent<AudioSource>().PlayOneShot(roundLost);
            Debug.Log("Lost round, so playing losing music");
        }
    }

    [Command]
    void CmdUpdateMemeText(string memeText)
    {
        MyMemeText = memeText;
        //RpcUpdateMemeText(memeText);//I don't want to update meme text on all clients, because this adds heaps of traffic to the server
    }

    [ClientRpc]
    void RpcUpdateMemeText(string memeText)
    {
        //This happens on all clients
        if (!hasAuthority)
        {
            //But this happens only on the clients that didn't cause the update
            MyMemeText = memeText;
        }
    }

    public void ChooseMemeToVoteFor(string memeWriter)
    {
        if (isLocalPlayer)
        {
            VotedMemeMadeBy = memeWriter;
            CmdChooseMemeToVoteFor(memeWriter);
        }
    }

    [Command]
    void CmdChooseMemeToVoteFor(string memeWriter)
    {
        VotedMemeMadeBy = memeWriter;
    }

    public bool IsLocalPlayer()
    {
        return isLocalPlayer;
    }
}

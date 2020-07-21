using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;

public class TurnManager : NetworkBehaviour
{
   
    //This script is responsible for the progression of a match, through individual turns.
    //It updates the match manager every time a round has begun or when somebody gets points.

    public int TurnStage = 0;
    /*These are the stages of the match:
     * 0 is inactive. This is in case we want the thing off
     * 1 is the first stage (Round display)
     * 2 is filler
     * 3 is the second stage (Creating a meme)
     * 4 is filler
     * 5 is the third stage (Voting for a meme)
     * 6 is filler
     * 7 is the fourth stage (Seeing the winner)
     * 8 is filler
     * 9 is the fitfh stage (Voting for a new meme)
     * 10 is the transition stage back to 1
     */
    public int RoundNumber = 1;
    public float FirstStageTime = 5f;
    public float SecondStageTime = 60f;
    public float ThirdStageTime = 30f;
    public float FourthStageTime = 15f;
    public float FifthStageTime = 15f;
    public GameObject PrimaryRoundTitle;
    public GameObject PrimaryMainMeme;
    public GameObject PrimaryMemeOptions;
    public GameObject SecondaryInput;
    public GameObject SecondaryNextStageButtons;
    public GameObject SecondaryCircles;
    public GameObject SecondaryWhoVotedForWho;
    public Sprite[] sprites;
    public GameObject ContentButtonObject;
    public GameObject MemeButtonObject;
    public GameObject MainMemeObject;
    public GameObject MainMemeText;
    public GameObject[] NextMemeButtons;
    public GameObject WhoVotedForWhoRowPrefab;
    public Text TimerObject;
    public TextMeshProUGUI AlertMessage;
    public Slider TimerSliderLeft;
    public Slider TimerSliderRight;
    public bool TurnPaused = false;
    public AudioClip Tick;
    public AudioClip Tock;
    public GameObject StageEndWaitingPanel;
    public Text RoundText;
    public TextMeshProUGUI RoundSubText;
    public Image WinnerColorPanel;
    public GameObject PeopleWhoVotedForWinnerObject;

    private List<string> usersWhoLocked = new List<string>();
    private bool finalWhoVotedForWho = false;
    private string availablePackagesString;
    private float timer = 0f;
    private int currentMemeIndex;
    private AudioClip myClock;
    private bool memesCreatedThisRound = false;
    private string displayWinnerText = "";
    private List<string> playerUIDs;
    private List<string> playerMemes;
    private Dictionary<string, int> playerVoteCount = new Dictionary<string, int>();
    private List<int> availableSpriteIndexes = new List<int>();
    private int stageMaxTime = 65;
    private string sentShowMessage = "";
    private bool sentPauseForNoMeme = false;

    //When object awakens, load sprites from MemeLoader object
    private void Awake()
    {
        LoadSpritesFromGameobject();
    }

    [ClientRpc]
    private void RpcSetupMemePool(string hostAvailablePackagesString, int nextMemeID)
    {
        Sprite[] returnArr;
        //hostAvailablePackagesString = CmdGetAvailablePackages();
        if (string.IsNullOrEmpty(hostAvailablePackagesString) && string.IsNullOrWhiteSpace(hostAvailablePackagesString))
        {
            Debug.Log("Available package string is empty so I'm calling the base one");
            returnArr = GameObject.Find("MemeLoader").GetComponent<MemeLoaderScript>().GetAvailableMemes("base");
        }
        else
        {
            Debug.Log("Available package string is not empty (it is " + hostAvailablePackagesString + ") so I'm calling the normal one");
            returnArr = GameObject.Find("MemeLoader").GetComponent<MemeLoaderScript>().GetAvailableMemes(hostAvailablePackagesString);
        }
        sprites = returnArr;
        //System.Array.Copy(returnArr, sprites, returnArr.Length);
        availableSpriteIndexes.Clear();
        RefillAllIndexes();

        SetupMainMeme(nextMemeID);
    }

    //Until client players have time to get and use the specified meme packages, load all memes available to player
    private void LoadSpritesFromGameobject()
    {
        Sprite[] returnArr;
        availablePackagesString = GameObject.Find("MemeLoader").GetComponent<MemeLoaderScript>().GetAvailablePackages();
        returnArr = GameObject.Find("MemeLoader").GetComponent<MemeLoaderScript>().GetAvailableMemes(availablePackagesString);
        sprites = returnArr;
        Debug.Log("Copied array");
    }

    //In Start, setup for the first stage and prepare local vars
    void Start ()
    {
        myClock = Tick;
        RefillAllIndexes();
        if (!isServer)
            return;
        int nextMeme = availableSpriteIndexes[Random.Range(0, availableSpriteIndexes.Count)];
        availableSpriteIndexes.Remove(nextMeme);
        usersWhoLocked.Clear();
        //Send them to all clients (And also server) to be created as main meme for stage 1
        SetupMainMeme(nextMeme);//Make sure to update currentMeme in all of them!
        RpcSetupMainMeme(nextMeme);
    }

    // Update is used to track the time, the stage we're in and stage progression
    void Update ()
    {
        NetworkServer.SpawnObjects();
        if (!TurnPaused)
        {
            StageProgresser();
            StageHandler();
            ServerStageHandler();

            //Only handle display timer for stages 3, 5, and 9. Stages 1 (Round number) and 7 (Round winner) don't need a timer.
            if (TurnStage != 1 && TurnStage != 2 && TurnStage != 7 && TurnStage != 8)
            {
                TimerObject.transform.parent.gameObject.SetActive(true);

                if (TurnStage == 1 || TurnStage == 2)
                    stageMaxTime = (int)FirstStageTime;
                else if (TurnStage == 3 || TurnStage == 4)
                    stageMaxTime = (int)SecondStageTime;
                else if (TurnStage == 5 || TurnStage == 6)
                    stageMaxTime = (int)ThirdStageTime;
                else if (TurnStage == 7 || TurnStage == 8)
                    stageMaxTime = (int)FourthStageTime;
                else if (TurnStage == 9 || TurnStage == 10)
                    stageMaxTime = (int)FifthStageTime;

                //Calculate display time correctly
                string newTimerVal = stageMaxTime.ToString();
                float relTimer = (timer % (FirstStageTime + SecondStageTime + ThirdStageTime + FourthStageTime + FifthStageTime)); //timer relative to a turn length
                if (TurnStage == 1)
                    relTimer = (stageMaxTime - relTimer % stageMaxTime);
                else if (TurnStage == 3)
                    relTimer = (stageMaxTime - (relTimer - (int)FirstStageTime) % stageMaxTime);
                else if (TurnStage == 5)
                    relTimer = (stageMaxTime - (relTimer - (int)FirstStageTime - (int)SecondStageTime) % stageMaxTime);
                else if (TurnStage == 7)
                    relTimer = (stageMaxTime - (relTimer - (int)FirstStageTime - (int)SecondStageTime - (int)ThirdStageTime) % stageMaxTime);
                else if (TurnStage == 9)
                    relTimer = (stageMaxTime - (relTimer - (int)FirstStageTime - (int)SecondStageTime - (int)ThirdStageTime - (int)FourthStageTime) % stageMaxTime);

                TimerSliderLeft.maxValue = stageMaxTime;
                TimerSliderLeft.value = relTimer;
                TimerSliderRight.maxValue = stageMaxTime;
                TimerSliderRight.value = relTimer;
                newTimerVal = ((int)relTimer).ToString();
                if (TimerObject.text != newTimerVal)
                {
                    TimerObject.text = newTimerVal;
                    if (StageEndWaitingPanel.activeSelf == false)
                    {
                        TimerObject.GetComponent<AudioSource>().PlayOneShot(myClock, (float)Mathf.Lerp(0f, 1f, ((stageMaxTime - relTimer) - (stageMaxTime - 6)) / 6f)); 
                        if (myClock == Tick)
                            myClock = Tock;
                        else
                            myClock = Tick;
                        StartCoroutine(TimeTextAnimation());
                    }
                }
            }
            else
            {
                TimerObject.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    //Stage progresser is used to progress through the stages when the time is right, and also trigger the relevant prompt during transitions
    private void StageProgresser()
    {
        if (!isServer)
        {
            if (!sentPauseForNoMeme)
            {
                timer += Time.deltaTime;
                UpdateTimer(timer, "", false);
            }
            return;
        }

        if (timer >= FirstStageTime && TurnStage == 1)
            CmdUpdateTurnstage(2);
        else if (TurnStage == 2)
            CmdUpdateTurnstage(3);
        else if (timer >= SecondStageTime + FirstStageTime && TurnStage == 3)
            CmdUpdateTurnstage(4);
        else if (TurnStage == 4)
            CmdUpdateTurnstage(5);
        else if (timer >= FirstStageTime + SecondStageTime + ThirdStageTime && TurnStage == 5)
            CmdUpdateTurnstage(6);
        else if (TurnStage == 6)
            CmdUpdateTurnstage(7);
        else if (timer >= FirstStageTime + SecondStageTime + ThirdStageTime + FourthStageTime && TurnStage == 7)
            CmdUpdateTurnstage(8);
        else if (TurnStage == 8)
            CmdUpdateTurnstage(9);
        else if (timer >= FirstStageTime + SecondStageTime + ThirdStageTime + FourthStageTime + FifthStageTime && TurnStage == 9)
            CmdUpdateTurnstage(10);
        else if (TurnStage == 10)
        {
            timer = -Time.deltaTime;
            CmdUpdateTurnstage(1);
        }

        float prevTime = timer;
        timer += Time.deltaTime;
        bool timeValueChanged = false;
        if ((int)timer != (int)prevTime)
        {
            timeValueChanged = true;
        }
        string showMessage = "";
        if (timer < 0f)
            timer = 0f;

        //Pause in the middle of the first stage if no player wrote any meme
        if (CheckForExistingMemes() == false && memesCreatedThisRound == false)
        {
            timer = Mathf.Clamp(timer, 0f, FirstStageTime + SecondStageTime / 2f - 1f);
            if (timer == FirstStageTime + SecondStageTime / 2f - 1 && sentPauseForNoMeme == false)
            {
                sentPauseForNoMeme = true;
                RpcSetPauseForNoMeme(sentPauseForNoMeme);
            }
        }
        else if (TurnStage == 3 && sentPauseForNoMeme == true)
        {
            sentPauseForNoMeme = false;
            RpcSetPauseForNoMeme(sentPauseForNoMeme);
        }
        

        //Show relevant prompt
        if (TurnStage == 1)
            showMessage = "Get ready!";
        else if (TurnStage == 3)
            showMessage = "Write a meme caption!";
        else if (TurnStage == 5)
            showMessage = "Vote for the best meme!";
        else if (TurnStage == 7)
            if (displayWinnerText == "")
                showMessage = "D R A W";
            else
                showMessage = "Winner is " + displayWinnerText + "!";
        else if (TurnStage == 9)
            showMessage = "Vote for the next meme!";
        else if (CheckForExistingMemes() == true && memesCreatedThisRound == false && timer > 0.5f)
            memesCreatedThisRound = true;

        if (timeValueChanged || sentShowMessage != showMessage)
        {
            if (sentShowMessage != showMessage)
            {
                sentShowMessage = showMessage;
                RpcUpdateTimer(timer, showMessage, true);
            }
            else
                RpcUpdateTimer(timer, "", false);
        }
    }

        //Stage hanlder loads/unloads objects in scene based on the current stage
    private void StageHandler()
    {
        switch (TurnStage)
        {
            case 1:
                PrimaryMainMeme.SetActive(false);
                PrimaryMemeOptions.SetActive(false);
                SecondaryInput.SetActive(false);
                SecondaryNextStageButtons.SetActive(false);
                SecondaryCircles.SetActive(false);
                PrimaryRoundTitle.SetActive(true);
                SecondaryWhoVotedForWho.SetActive(false);
                break;
            case 3:
                PrimaryMainMeme.SetActive(true);
                PrimaryMemeOptions.SetActive(false);
                SecondaryInput.SetActive(true);
                SecondaryNextStageButtons.SetActive(false);
                SecondaryCircles.SetActive(false);
                PrimaryRoundTitle.SetActive(false);
                SecondaryWhoVotedForWho.SetActive(false);
                break;
            case 5:
                PrimaryMainMeme.SetActive(false);
                PrimaryMemeOptions.SetActive(true);
                SecondaryInput.SetActive(false);
                SecondaryNextStageButtons.SetActive(false);
                SecondaryCircles.SetActive(true);
                PrimaryRoundTitle.SetActive(false);
                SecondaryWhoVotedForWho.SetActive(false);
                break;
            case 7:
                PrimaryMainMeme.SetActive(true);
                PrimaryMemeOptions.SetActive(false);
                SecondaryInput.SetActive(false);
                SecondaryNextStageButtons.SetActive(false);
                SecondaryCircles.SetActive(false);
                PrimaryRoundTitle.SetActive(false);
                SecondaryWhoVotedForWho.SetActive(true);
                break;
            case 9:
                PrimaryMainMeme.SetActive(true);
                PrimaryMemeOptions.SetActive(false);
                SecondaryInput.SetActive(false);
                SecondaryNextStageButtons.SetActive(true);
                SecondaryCircles.SetActive(false);
                PrimaryRoundTitle.SetActive(false);
                SecondaryWhoVotedForWho.SetActive(false);
                break;
        }
    }

    //This function is similar to StageHandler, but only runs on the server. It does all the calculations between transitions of stages
    void ServerStageHandler()
    {
        if (!isServer)
            return;

        switch (TurnStage)
        {
            case 4:
                //Debug.Log("About to try and run stage 2 on the server.");
                //Get all player UIDs and meme texts
                playerUIDs = new List<string>();
                playerMemes = new List<string>();
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                //Debug.Log("Created players.");
                for (int i = 0; i < players.Length; i++)
                {
                    playerUIDs.Add(players[i].GetComponent<PlayerCode>().PlayerUID);
                    playerMemes.Add(players[i].GetComponent<PlayerCode>().MyMemeText);
                }
                //Debug.Log("Server: Got all player UIDs and memes! Turning them into buttons now.");
                CreateButtonsForVote(playerUIDs, playerMemes, currentMemeIndex);
                RpcCreateButtonsForVote(StringSeparator(playerUIDs), StringSeparator(playerMemes),currentMemeIndex);
                //Send them to all clients (And also server) to be created as buttons for stage 3
                RpcSetStageEndLoadingPanelEnabled(false);
                usersWhoLocked.Clear();
                break;
            case 6:
                //Debug.Log("About to try and run stage 4 on the server.");
                //Get all player UIDs and votes
                playerUIDs = new List<string>();
                playerVoteCount.Clear();
                players = GameObject.FindGameObjectsWithTag("Player");
                //Debug.Log("Created players.");
                for (int i = 0; i < players.Length; i++)
                {
                    playerUIDs.Add(players[i].GetComponent<PlayerCode>().PlayerUID);

                    //Register the vote made by this player
                    string votedMemeUID = players[i].GetComponent<PlayerCode>().VotedMemeMadeBy;
                    if (votedMemeUID != "")
                        Debug.Log("Player" + playerUIDs[i] + " voted for player " + votedMemeUID + "'s meme. Registring...");
                    if (votedMemeUID == "")
                        Debug.Log("Player " + players[i].GetComponent<PlayerCode>().PlayerUID + " didn't actually vote for anyone. Shame...");
                    else if (playerVoteCount.ContainsKey(votedMemeUID))
                        playerVoteCount[votedMemeUID] = playerVoteCount[votedMemeUID] + 1;
                    else
                        playerVoteCount.Add(votedMemeUID, 1);
                    //Debug.Log("Player vote registered");
                }
                //Debug.Log("Server: Got all player UIDs and votes. Setting up victory screen on stage 5...");
                string roundWinner = CalculateRoundWinner();
                string nextMemeChoices = GenerateNextRoundMemeOptions();
                //Send them to all clients (And also server) to be created as buttons for stage 5
                CmdSetupWinner(roundWinner, nextMemeChoices, currentMemeIndex);
                RpcResetNextMemeButtonColors();
                this.GetComponent<MatchManager>().AddPointToUID(roundWinner);
                this.GetComponent<MatchManager>().CmdTurnEndPlayerListUI();
                RpcSetStageEndLoadingPanelEnabled(false);
                break;
            case 8:
                if (finalWhoVotedForWho)
                    this.GetComponent<MatchManager>().BeginEndEnactment();
                else
                {
                    EmptyVoteString();
                    RpcEmptyVoteString();
                    EmptyWinnerMeme();
                    RpcEmptyWinnerMeme();
                }
                break;
            case 10:
                //Debug.Log("About to try and run stage 6 on the server.");
                //Get all player UIDs and votes
                playerUIDs = new List<string>();
                playerVoteCount.Clear();
                players = GameObject.FindGameObjectsWithTag("Player");
                //Debug.Log("Created players.");
                for (int i = 0; i < players.Length; i++)
                {
                    playerUIDs.Add(players[i].GetComponent<PlayerCode>().PlayerUID);

                    //Register the vote made by this player
                    string votedMemeUID = players[i].GetComponent<PlayerCode>().VotedMemeMadeBy;
                    //Debug.Log("Player" + playerUIDs[i] + " voted for meme number " + votedMemeUID + ". Registring...");
                    if (playerVoteCount.ContainsKey(votedMemeUID))
                        playerVoteCount[votedMemeUID] = playerVoteCount[votedMemeUID] + 1;
                    else if (votedMemeUID != "")
                        playerVoteCount.Add(votedMemeUID, 1);
                    //Debug.Log("Player vote registered");
                }
                //Debug.Log("Server: Got all player UIDs and votes. Setting up meme for stage 1...");
                int nextMeme = CalculateNextMemeWinner();
                if (nextMeme == -1)
                {
                    nextMeme = int.Parse(NextMemeButtons[Random.Range(0, 4)].GetComponent<MemeButtonScript>().CreatorUID);
                    //Debug.Log("Chose next meme at random - " + nextMeme);
                }
                //Send them to all clients (And also server) to be created as main meme for stage 1
                EmptyVoteString();
                RpcEmptyVoteString();
                memesCreatedThisRound = false;
                SetupMainMeme(nextMeme);//Make sure to update currentMeme in all of them!
                RpcSetupMainMeme(nextMeme);
                RoundNumber++;
                string listOfPlayersWhoAreClose = this.GetComponent<MatchManager>().GetAlmostWinners();
                int maxPoints = this.GetComponent<MatchManager>().MaxPointsForMatch;
                SetupRoundText(RoundNumber, listOfPlayersWhoAreClose,maxPoints);
                RpcSetupRoundText(RoundNumber, listOfPlayersWhoAreClose,maxPoints);
                RpcSetStageEndLoadingPanelEnabled(false);
                usersWhoLocked.Clear();
                break;
        }
    }

    IEnumerator TimeTextAnimation(float largeSize = 150f, float animTime = 0.15f)
    {
        float originalSize = 60f;
        float maxTime = animTime;
        TimerObject.fontSize = (int)largeSize;
        while (animTime >0f)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            float newSize = Mathf.Lerp(originalSize, largeSize, animTime / maxTime);
            TimerObject.fontSize = (int)newSize;
            animTime -= Time.deltaTime;
        }
        TimerObject.fontSize = (int)originalSize;
    }

    [Command]
    public void CmdGetMyTurnStage(string myUID)
    {
        RpcGetMyTurnStage(myUID, TurnStage);
    }

    [ClientRpc]
    public void RpcGetMyTurnStage(string myUID, int turnStage)
    {
        string localUID = "";

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerCode>().IsLocalPlayer())
            {
                localUID = players[i].GetComponent<PlayerCode>().PlayerUID;
                break;
            }
        }

        if (myUID == localUID)
        {
            TurnStage = turnStage;
        }
    }


    [ClientRpc]
    private void RpcSetPauseForNoMeme(bool newVal)
    {
        SetPauseForNoMeme(newVal);
    }

    private void SetPauseForNoMeme(bool newVal)
    {
        sentPauseForNoMeme = newVal;
        Debug.Log("Paused turn!");
    }

    public float GetWaitingForInputTime()
    {
        return (FirstStageTime + SecondStageTime / 2f - 1f);
    }

    private float alertAnimationTime = 5f / 3f;
    private float timerForAlertAnimation = 0f;
    [ClientRpc]
    void RpcUpdateTimer(float time, string showMessage, bool newMessage)
    {
        UpdateTimer(time, showMessage, newMessage);
    }

    private void UpdateTimer(float time, string showMessage, bool newMessage)
    {
        timer = time;
        if (StageEndWaitingPanel.activeSelf == false)
        {
            timerForAlertAnimation = Mathf.Clamp(timerForAlertAnimation - Time.deltaTime, 0f, alertAnimationTime);

            if (AlertMessage.transform.parent.parent.gameObject.GetComponent<Animator>().GetBool("Trigger"))
                AlertMessage.transform.parent.parent.gameObject.GetComponent<Animator>().SetBool("Trigger", false);

            if (showMessage != AlertMessage.text || newMessage)
            {
                //Text has changed, time to play animation
                if (showMessage != "")
                {
                    AlertMessage.transform.parent.parent.gameObject.SetActive(true);
                    AlertMessage.transform.parent.parent.gameObject.GetComponent<Animator>().SetBool("Trigger", true);
                }
                if (newMessage)
                {
                    AlertMessage.text = showMessage;
                }
            }
            else
            {
                AlertMessage.transform.parent.parent.gameObject.GetComponent<Animator>().SetBool("Trigger", false);
            }
        }
    }

    [Command]
    void CmdUpdateTurnstage(int tS)
    {
        //Debug.Log("Updating turn stage on server - " + tS);
        TurnStage = tS;
        RpcUpdateTurnstage(tS);
    }

    [ClientRpc]
    void RpcUpdateTurnstage(int tS)
    {
        //Debug.Log("Updating turn stage on client - " + tS);
        TurnStage = tS;
    }

    [ClientRpc]
    private void RpcEmptyWinnerMeme()
    {
        EmptyWinnerMeme();
    }

    private void EmptyWinnerMeme()
    {
        foreach(Transform child in PeopleWhoVotedForWinnerObject.transform)
        {
            Destroy(child.gameObject);
        }
        WinnerColorPanel.color = Color.clear;
    }


    [ClientRpc]
    private void RpcSetupRoundText(int num, string almostList, int maxPoints)
    {
        SetupRoundText(num, almostList, maxPoints);
    }

    private void SetupRoundText(int num, string almostListCompressed, int maxPoints)
    {
        List<string> almostList = StringDeseparator(almostListCompressed);
        RoundNumber = num;
        RoundText.text = "Round " + RoundNumber;
        if (almostList != null && almostList.Count > 0)
        {
            if (almostList.Count == 1)
            {
                RoundSubText.text = almostList[0] + " has almost won!";
            }
            else if (almostList.Count == 2)
            {
                RoundSubText.text = almostList[0] + " & " + almostList[1] + " have almost won!";
            }
            else
            {
                string txt = "";
                for (int i = 0; i < almostList.Count; i++)
                {
                    if (i == almostList.Count - 1)
                        txt += " & ";
                    else if (i > 0)
                        txt += ", ";
                    txt += almostList[i];
                }
                RoundSubText.text = txt + " have almost won!";
            }
        }
        else
        {
            RoundSubText.text = "First to " + maxPoints + " points wins!";
        }
        PrimaryRoundTitle.GetComponent<FadeInScript>().ResetFade();
    }

    [ClientRpc]
    void RpcEmptyVoteString()
    {
        EmptyVoteString();
    }

    void EmptyVoteString()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerCode>().VotedMemeMadeBy = "";
        }
    }

    [ClientRpc]
    void RpcCreateButtonsForVote(string uids, string memeText, int memeImageIndex)
    {
        CreateButtonsForVote(StringDeseparator(uids), StringDeseparator(memeText), memeImageIndex);
    }

    private void CreateButtonsForVote(List<string> uids, List<string> memeText, int memeImageIndex)
    {
        //Delete all children of option parent
        //Debug.Log("Destroying all button children!");
        foreach (Transform child in ContentButtonObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        //For each submission, create a new button using the prefab
        //Debug.Log("Creating a new button for each meme. Number of memes: " + uids.Count);
        for (int i = 0; i < uids.Count; i++)
        {
            if (memeText[i] != "")
            {
                GameObject newButton = (Instantiate(MemeButtonObject, ContentButtonObject.transform)) as GameObject;
                newButton.GetComponent<MemeButtonScript>().CreatorUID = uids[i];
                Text[] memeTextCandidates = newButton.GetComponentsInChildren<Text>();//.text
                for (int j = 0; j < memeTextCandidates.Length; j++)//= memeText[i];
                {
                    if (memeTextCandidates[j].name == "MemeTextNormalOption")
                    {
                        memeTextCandidates[j].text = memeText[i];
                    }
                }
                newButton.GetComponent<Image>().sprite = sprites[memeImageIndex];
                //Check if it's the creator's button, and disable it if so.
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                if (isClient)
                {
                    foreach (GameObject player in players)
                    {
                        if (player.GetComponent<PlayerCode>().PlayerUID == uids[i] && (player.GetComponent<PlayerCode>().IsLocalPlayer()) == true)
                            Destroy(newButton);
                    }
                }
            }
        }
        ContentButtonObject.GetComponent<ChangeContentSize>().SetPosXToTop();
    }

    private IEnumerator repositionContentButtonObject()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        ContentButtonObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(ContentButtonObject.GetComponent<RectTransform>().anchoredPosition.x, -999999f);
        yield return new WaitForSeconds(Time.deltaTime);
        ContentButtonObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(ContentButtonObject.GetComponent<RectTransform>().anchoredPosition.x, -999999f);
    }

    public static string StringSeparator(List<string> myList)
    {
        //Debug.Log("Separating string...");
        string returnString = "";
        for (int i = 0; i < myList.Count; i++)
        {
            if (myList[i] != "" && myList[i] != " ")
            {
                if (i != 0)
                    returnString += "_,_";
                returnString += myList[i].Replace(",", ",,");
            }
            else
            {
                if (i != 0)
                    returnString += "_,_";
                returnString += "!empty$";
                Debug.Log("Found an empty player name in index " + i + " of the players list!");
            }
        }
        //Debug.Log("String separated. String is: " + returnString);
        return returnString;
    }

    public static List<string> StringDeseparator(string str)
    {
        //Debug.Log("Separating list...");

        string a = "abc";
        string b = a.Substring(2);

        List<string> returnList = new List<string>();
        int prevPoint = 0;
        for (int i = 0; i < str.Length; i++)
        {
            if (i == 0)
            {
                Debug.Log("Currently in i=0. str length is: " + str.Length + ", str is: " + str + ".");
            }
            else if (i != (str.Length - 1))
            {
                if (str[i] == ',' && str[i - 1] == '_' && str[i + 1] == '_')
                {
                    string newPart = str.Substring(prevPoint, i - 1 - prevPoint).Replace(",,", ",");
                    if (newPart == "!empty$")
                        returnList.Add("");
                    else
                        returnList.Add(newPart);
                    prevPoint = i + 2;
                }

            }
            else
            {
                string newPart = str.Substring(prevPoint).Replace(",,", ",");
                if (newPart == "!empty$")
                    returnList.Add("");
                else
                    returnList.Add(newPart);
            }
        }
        return returnList;
    }

    private static int[] IntSeparator(string str)
    {
        string[] parts = str.Split(',');
        int[] intParts = new int[parts.Length];
        for (int i=0;i<parts.Length;i++)
        {
            intParts[i] = int.Parse(parts[i]);
        }
        return intParts;
    }

    private string CalculateRoundWinner()
    {
        string winnerUID = "";
        int maxVotes = -1;
        bool draw = false;
        foreach(KeyValuePair<string,int> contestant in playerVoteCount)
        {
            if (contestant.Value > maxVotes)
            {
                maxVotes = contestant.Value;
                winnerUID = contestant.Key;
                draw = false;
            }
            else if (contestant.Value == maxVotes)
                draw = true;
        }
        if (draw)
            winnerUID = "";
        Debug.Log("Player with UID of " + winnerUID + " won with " + maxVotes + " votes.");
        return (winnerUID);
    }

    private int CalculateNextMemeWinner()
    {
        int winnerID = -1;
        int maxVotes = -1;
        bool draw = false;
        foreach (KeyValuePair<string, int> memeID in playerVoteCount)
        {
            int n;
            if (memeID.Value > maxVotes && int.TryParse(memeID.Key, out n))
            {
                maxVotes = memeID.Value;
                winnerID = int.Parse(memeID.Key);
                draw = false;
            }
            else if (memeID.Value == maxVotes && int.TryParse(memeID.Key, out n))
            {
                draw = true;
            }
        }
        if (draw)
            winnerID = -1;
        return (winnerID);
    }

    private string GenerateNextRoundMemeOptions()
    {
        string returnString = "";
        for (int i = 0; i < 4; i++)
        {
            if (availableSpriteIndexes.Count != 0)
            {
                int newChoice = availableSpriteIndexes[Random.Range(0, availableSpriteIndexes.Count)];
                availableSpriteIndexes.Remove(newChoice);
                if (i != 0)
                    returnString += ",";
                returnString += newChoice.ToString();
            }
            else
            {
                RefillAllIndexes();
                i--;
            }
        }
        return returnString;
    }

    private void RefillAllIndexes()
    {
        Debug.Log("Refilling all indexes!");
        for (int i = 0; i < sprites.Length; i++)
        {
            availableSpriteIndexes.Add(i);
        }
    }

    [Command]
    void CmdSetupWinner(string winnerUID, string nextOptions, int memeImageIndex)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        string winnerName = "";
        string winnerTitle = "";
        string winnerMeme = "";
        foreach (GameObject player in players)
        {
            if (winnerUID == "")
            {
                winnerName = "";
                winnerTitle = "";
                winnerMeme = "When you can't decide which meme is best";
                Debug.Log("SERVER: No best meme was chosen. Sending him to client now...");
            }
            else if (player.GetComponent<PlayerCode>().PlayerUID == winnerUID)
            {
                winnerName = player.GetComponent<PlayerCode>().PlayerName;
                winnerTitle = player.GetComponent<PlayerCode>().PlayerTitle;
                winnerMeme = player.GetComponent<PlayerCode>().MyMemeText;
                //Debug.Log("SERVER: Found the winner player. The winner is " + winnerName + ". Sending him to client now...");
            }
        }
        SetupWinner(winnerName,winnerTitle,winnerMeme, nextOptions,memeImageIndex, winnerUID);
        RpcSetupWinner(winnerName, winnerTitle, winnerMeme, nextOptions, memeImageIndex, winnerUID);
    }

    [ClientRpc]
    void RpcSetupWinner(string winnerName, string winnerTitle, string winnerMeme, string nextOptions, int memeImageIndex, string winnerUID)
    {
        //Debug.Log("CLIENT: Got the winner player. The winner is " + winnerName + ". Updating now...");
        SetupWinner(winnerName, winnerTitle, winnerMeme, nextOptions, memeImageIndex, winnerUID);
    }

    private void SetupWinner(string winnerName, string winnerTitle, string winnerMeme, string nextOptions, int memeImageIndex, string winnerUID)
    {
        //Set the main meme to have the winner's meme text, meme image, and get his name so we can display it in a pretty way!
        MainMemeObject.GetComponent<Image>().sprite = sprites[memeImageIndex];
        MainMemeText.GetComponent<Text>().text = winnerMeme;
        //Set the underbuttons to have the images of the four new potential memes.
        int[] nextOptionsArray = IntSeparator(nextOptions);
        for (int i = 0; i < nextOptionsArray.Length; i++)
        {
            NextMemeButtons[i].GetComponent<Image>().sprite = sprites[nextOptionsArray[i]];
            NextMemeButtons[i].GetComponent<MemeButtonScript>().CreatorUID = nextOptionsArray[i].ToString();
        }
        if (winnerName == "")
            displayWinnerText = "";
        else
            displayWinnerText = winnerName + " " + winnerTitle;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<PlayerCode>().PlayRoundEndMusic(winnerUID);
        }
    }

    [ClientRpc]
    public void RpcSetupMainMeme(int nextMeme)
    {
        if (nextMeme == -1)
            CmdSetupMainMeme();
        else
            SetupMainMeme(nextMeme);
    }

    [Command]
    public void CmdSetupMainMeme()
    {
        RpcSetupMemePool(availablePackagesString, currentMemeIndex);
        Debug.Log("Current meme index - " + currentMemeIndex);
    }

    [Command]
    public void CmdSetupMainMemeForMe(string myUID)
    {
        RpcSetupMemePool(availablePackagesString, currentMemeIndex);
        Debug.Log("Current meme index - " + currentMemeIndex);
        RpcSetupMainMemeForMe(myUID, currentMemeIndex);
    }

    //Deprecated function
    [ClientRpc]
    public void RpcSetupMainMemeForMe(string myUID, int nextMeme)
    {
        string localUID = "";
    }

    private void SetupMainMeme(int nextmeme)
    {
        if (nextmeme == -1)
            nextmeme = int.Parse(NextMemeButtons[0].GetComponent<MemeButtonScript>().CreatorUID);
        currentMemeIndex = nextmeme;
        MainMemeObject.GetComponent<Image>().sprite = sprites[nextmeme];
        SecondaryInput.GetComponent<TMP_InputField>().text = "";
        foreach (Transform child in PeopleWhoVotedForWinnerObject.transform)
        {
            Destroy(child.gameObject);
        }
        WinnerColorPanel.color = Color.clear;
    }

    private bool CheckForExistingMemes()
    {
        if (!isServer)
            return true;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerCode>().MyMemeText != "")
                return true;
        }
        return false;
    }

    private bool CheckForPlayerVotes()
    {
        if (!isServer)
            return true;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerCode>().VotedMemeMadeBy != "")
                return true;
        }
        return false;
    }

    public void UpdateNextMemeButtonColors(string selectedButton)
    {
        foreach (GameObject singleButton in NextMemeButtons)
        {
            Color coloringColor = Color.red;
            if (singleButton.GetComponentsInChildren<Image>()[1].sprite == sprites[int.Parse(selectedButton)])
                coloringColor = Color.green;
            singleButton.GetComponentsInChildren<Image>()[1].color = coloringColor;
        }
    }

    public void UpdateMemeVoteButtonColors(string selectedButton)
    {
        foreach (Transform child in ContentButtonObject.transform)
        {

            Color coloringColor = Color.red;
            if (child.gameObject.GetComponent<MemeButtonScript>().CreatorUID == selectedButton)
                coloringColor = Color.green;

            Image[] myPossibleImages = child.gameObject.GetComponentsInChildren<Image>();
            foreach (Image possibility in myPossibleImages)
            {
                if (possibility.name == "MemeImage")
                {
                    possibility.color = coloringColor;
                }
            }
        }
    }

    [ClientRpc]
    public void RpcResetNextMemeButtonColors()
    {
        foreach (GameObject singleButton in NextMemeButtons)
        {
            singleButton.GetComponentsInChildren<Image>()[1].color = Color.white;
        }
    }

    public float GetTime()
    {
        return timer;
    }

    [ClientRpc]
    public void RpcResetTurn()
    {
        ResetTurn();
    }

    public void ResetTurn()
    {
        timer = 0f;
        TurnStage = 1;
        SecondaryInput.GetComponent<TMP_InputField>().text = "";
        SetupRoundText(RoundNumber,"", 3);
        memesCreatedThisRound = false;
        usersWhoLocked.Clear();
    }

    [ClientRpc]
    public void RpcSetStageEndLoadingPanelEnabled(bool input)
    {
        IsStageEndLoadingPanelEnabled(input);
    }

    public void IsStageEndLoadingPanelEnabled(bool input)
    {
        StageEndWaitingPanel.SetActive(input);
    }

    public void SetupWhoVotedForWhoPanel(string playersData)
    {
        Debug.Log("PlayersData: " + playersData);
        playersData = SortPlayerDataByWhoVotedForWho(playersData);
        Dictionary<string, GameObject> circleParentByUID = new Dictionary<string, GameObject>();
        Dictionary<string, string> memeTextByUID = new Dictionary<string, string>();
        List<string> rpcPlayers = StringDeseparator(playersData);
        Debug.Log("PlayersData after reorder: " + playersData);

        //Before we begin this, let's get all the meme texts and their creators and put them in a dictionary
        foreach (Transform child in ContentButtonObject.transform)
        {
            string creator = child.GetComponent<MemeButtonScript>().CreatorUID;
            string meme = "";
            Text[] memeTextCandidates = child.GetComponentsInChildren<Text>();//.text
            for (int j = 0; j < memeTextCandidates.Length; j++)//= memeText[i];
            {
                if (memeTextCandidates[j].name == "MemeTextNormalOption")
                {
                    meme = memeTextCandidates[j].text;
                    break;
                }
            }
            memeTextByUID.Add(creator, meme);
        }
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetComponent<PlayerCode>().IsLocalPlayer())
                memeTextByUID.Add(players[i].GetComponent<PlayerCode>().PlayerUID, players[i].GetComponent<PlayerCode>().MyMemeText);
        }

        foreach (Transform child in SecondaryWhoVotedForWho.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < rpcPlayers.Count; i++)
        {
            List<string> playerDetails = StringDeseparator(rpcPlayers[i]);
            if (playerDetails[0] != "DRAW")
            {
                if (i == 0)//Specifically for the first player, we want to alter the main panel meme
                {
                    //Load player 1 his background color and spawn playercircles under him. Make sure to hide score & rank
                    Color playerColor;
                    if (ColorUtility.TryParseHtmlString("#" + playerDetails[3], out playerColor))
                        WinnerColorPanel.color = playerColor;
                    WinnerColorPanel.color = new Color(WinnerColorPanel.color.r, WinnerColorPanel.color.g, WinnerColorPanel.color.b, 100f / 255f);
                    Debug.Log("Player color : " + WinnerColorPanel.color.r + "," + WinnerColorPanel.color.g + "," + WinnerColorPanel.color.b);
                    circleParentByUID.Add(playerDetails[0], PeopleWhoVotedForWinnerObject);
                }
                else if (i < 4)// For all other players, we want to show them in the entries
                {
                    /* 
                     * For each, instantiate a WhoVotedForWhoRow
                     * Then, set the meme text to match the text of the person who created the meme
                     * Afterwards, config the playercircle on the left to match the meme creator. Make circle half-transparent, and remove rank & score
                     * Then, instantiate on the right all the circles of the players who voted for this meme. Remove rank & score, and make the button unselectable.
                     */
                    GameObject row = (Instantiate(WhoVotedForWhoRowPrefab, SecondaryWhoVotedForWho.transform)) as GameObject;
                    row.GetComponent<Image>().color = new Color(row.GetComponent<Image>().color.r, row.GetComponent<Image>().color.g, row.GetComponent<Image>().color.b, 200f / 255f);
                    row.GetComponent<WhoVotedForRowScript>().MyMemeText.text = "\"" + memeTextByUID[playerDetails[0]] + "\"";
                    row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<LeaderboardCircleUIScript>().SetFullName(playerDetails[1]);
                    row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<LeaderboardCircleUIScript>().TextName.text = playerDetails[1][0].ToString();
                    Color playerColor;
                    if (ColorUtility.TryParseHtmlString("#" + playerDetails[3], out playerColor))
                    {
                        row.GetComponent<Image>().color = playerColor;
                        row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<LeaderboardCircleUIScript>().SetBackgroundColor(playerColor);
                    }
                    row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<LeaderboardCircleUIScript>().TextScore.text = "";
                    row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<LeaderboardCircleUIScript>().SetPositionText(false);
                    row.GetComponent<WhoVotedForRowScript>().MyMemePlayerCircle.GetComponent<FadeInScript>().SetFade(0.5f);

                    circleParentByUID.Add(playerDetails[0], row.GetComponent<WhoVotedForRowScript>().MyPeopleWhoVotedFor);
                }
            }
        }

        for (int i = 0; i < rpcPlayers.Count; i++)
        {
            List<string> playerDetails = StringDeseparator(rpcPlayers[i]);
            Debug.Log("About to process player - " + playerDetails[0]);
            if (playerDetails[0] != "DRAW")
            {
                if (playerDetails[4] != null && playerDetails[4] != "")
                {
                    Debug.Log("Player " + playerDetails[1] + " voted for " + playerDetails[4]);
                    if (circleParentByUID.ContainsKey(playerDetails[4]))
                    {
                        GameObject parentToInstantiateUnder = circleParentByUID[playerDetails[4]];
                        GameObject theyWhoVoted = (Instantiate(this.GetComponent<MatchManager>().PlayerCirclePrefab, this.GetComponent<MatchManager>().PlayerCirclePrefab.transform.position, Quaternion.Euler(0, 0, 0), parentToInstantiateUnder.transform)) as GameObject;
                        LeaderboardCircleUIScript newObjScript = theyWhoVoted.GetComponent<LeaderboardCircleUIScript>();
                        Color playerColor;
                        if (ColorUtility.TryParseHtmlString("#" + playerDetails[3], out playerColor))
                            newObjScript.SetBackgroundColor(playerColor);
                        newObjScript.SetFullName(playerDetails[1]);
                        newObjScript.SetUUID(playerDetails[0]);
                        newObjScript.TextScore.text = "";
                        newObjScript.TextPosition.text = "";
                        newObjScript.TextName.text = playerDetails[1][0].ToString();
                        theyWhoVoted.GetComponentInChildren<Button>().interactable = false;
                    }
                }
            }
        }
    }

    public string SortPlayerDataByWhoVotedForWho(string playerDataOriginal)
    {
        Dictionary<string, int> VotesPerUIDDictionary = new Dictionary<string, int>();
        Dictionary<string, string> PlayerDataPerUIDDictionary = new Dictionary<string, string>();
        List<string> rpcPlayers = StringDeseparator(playerDataOriginal);
        List<string> playerDataNew = new List<string>();
        int maxVotes = 0;
        foreach (string rpcPlayer in rpcPlayers)
        {
            List<string> playerDetails = StringDeseparator(rpcPlayer);
            PlayerDataPerUIDDictionary.Add(playerDetails[0], rpcPlayer);
            if (playerDetails[4] != null && playerDetails[4] != "")
            {
                if (VotesPerUIDDictionary.ContainsKey(playerDetails[4]))
                    VotesPerUIDDictionary[playerDetails[4]] = VotesPerUIDDictionary[playerDetails[4]] + 1;
                else
                    VotesPerUIDDictionary[playerDetails[4]] = 1;

                if (VotesPerUIDDictionary[playerDetails[4]] > maxVotes)
                    maxVotes = VotesPerUIDDictionary[playerDetails[4]];
            }

            if (!VotesPerUIDDictionary.ContainsKey(playerDetails[0]))
                VotesPerUIDDictionary[playerDetails[0]] = 0;
        }

        for (int i = maxVotes; i >= 0; i--)
        {
            int sameVoteAmount = 0;
            foreach (string key in VotesPerUIDDictionary.Keys)
            {
                if (VotesPerUIDDictionary[key] == i)
                {
                    sameVoteAmount++;
                    playerDataNew.Add(PlayerDataPerUIDDictionary[key]);
                    if (sameVoteAmount > 1 && VotesPerUIDDictionary[key] == maxVotes)//In the case multiple users have the max votes, it must mean there's a draw.
                        playerDataNew.Insert(0, "DRAW");
                }
            }
        }

        return (StringSeparator(playerDataNew));
    }

    public void LockPlayer(string playerUID)
    {
        if (!usersWhoLocked.Contains(playerUID))
            usersWhoLocked.Add(playerUID);
        if (this.GetComponent<MatchManager>().AllPlayersLocked(usersWhoLocked))
        {
            Debug.Log("Locked " + playerUID + ". This was enough to go to next scene.");
            if (TurnStage == 3)
                timer = SecondStageTime + FirstStageTime + Time.deltaTime;
            if (TurnStage == 5)
                timer = FirstStageTime + SecondStageTime + ThirdStageTime + Time.deltaTime;
        }
        else
            Debug.Log("Locked " + playerUID + ". This was NOT enough to go to next scene.");
    }

    public void UnlockPlayer(string playerUID)
    {
        usersWhoLocked.Remove(playerUID);
    }

    public void SetFinalWhoVotedForWho(bool val)
    {
        finalWhoVotedForWho = val;
    }
}

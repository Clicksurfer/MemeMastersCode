using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MatchManager : NetworkBehaviour {

    //This script is responsible for managing the match.
    //It has the match ending condition, and when it is met it goes on to the winner page.
    //It also handles displaying the UI of the match progress as it holds the relevant data (players, points, etc)

    public int MaxPointsForMatch = 10;
    public GameObject PlayerCirclePrefab;
    public GameObject PlayerCircleParentObject;
    public bool HostDecendant = false;
    public GameObject LoadScreen;

    private Dictionary<string, int> playerPoints = new Dictionary<string, int>();
    private Dictionary<string, string> playerNames = new Dictionary<string, string>();
    private Dictionary<string, Color> playerColors = new Dictionary<string, Color>();
    private GameObject[] playersCurrentlyInMatch;
    private string decendantUID = "";
    private bool waitingForPlayerResponse = false;
    private int playerAmountAtEnd = 0;
    private int responsesGot = 0;
    private List<Color> circleColorList = new List<Color>();
    private string compPlayerString = "";

    //When the game begins, prepare all the players and colors
    //Also check if this is a reboot from a crashed match or not
	void Start () 
    {
        UpdatePlayerDictionary();
        RefillColorList();
        ReadHostDecendantString();
    }

    //Read data from playerPrefs if crashed, and prepare server players/points accordingly
    private void ReadHostDecendantString()
    {
        string playerPrefData = PlayerPrefs.GetString("MM_HostDecendantData", "");
        if (playerPrefData != "")
        {
            playerNames.Clear();
            playerPoints.Clear();
            playerColors.Clear();
            RefillColorList();

            List<string> playersList = TurnManager.StringDeseparator(playerPrefData);
            foreach(string playerInList in playersList)
            {
                List<string> playerData = TurnManager.StringDeseparator(playerInList);
                playerPoints.Add(playerData[0], int.Parse(playerData[2]));
                playerNames.Add(playerData[0], playerData[1]);
                Color playerColor;
                if (ColorUtility.TryParseHtmlString("#" + playerData[3], out playerColor))
                {
                    playerColors.Add(playerData[0], playerColor);
                    circleColorList.Remove(playerColor);
                }
            }
            PlayerPrefs.SetString("MM_HostDecendantData", "");
        }
    }

    private void RefillColorList()
    {
        circleColorList.Clear();
        circleColorList.Add(new Color(1, 0.5f, 0.5f));
        circleColorList.Add(new Color(0.5f, 1, 0.5f));
        circleColorList.Add(new Color(0.5f, 0.5f, 1f));
        circleColorList.Add(new Color(0.75f, 0.75f, 0.5f));
        circleColorList.Add(new Color(0.75f, 0.5f, 0.75f));
        circleColorList.Add(new Color(0.5f, 0.75f, 0.75f));
        circleColorList.Add(new Color(0.75f, 0.75f, 0.75f));
        circleColorList.Add(new Color(1, 1, 0.5f));
        circleColorList.Add(new Color(1, 0.5f, 1));
        circleColorList.Add(new Color(0.5f, 1, 1));
    }

    //Update is used primarily to update the player list/UI
    //It is also used to determine when the game's end is reached and when the players can be redirected to end
    void Update ()
    {
        if (!isServer)
            return;
        UpdatePlayerDictionary();
        //Check for winner
        if (waitingForPlayerResponse && responsesGot == playerAmountAtEnd)
        {
            //Got response from all the players. It's now safe to close the server.
            CmdGoToEnd(compPlayerString);
        }
        else if (waitingForPlayerResponse)
        {
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObj in playerObjects)
            {
                if (playerObj.GetComponent<PlayerCode>().ReadyToTerminate)
                {
                    responsesGot++;
                    RpcTerminatePlayerConnection(playerObj.GetComponent<PlayerCode>().PlayerUID);
                }
            }
        }
        else
        {
            foreach (KeyValuePair<string, int> entry in playerPoints)
            {
                if (entry.Value >= MaxPointsForMatch && waitingForPlayerResponse == false)
                {
                    this.GetComponent<TurnManager>().SetFinalWhoVotedForWho(true);
                }
            }
        }
	}

        //Function goes over all players in scene, and in case it is not registered in the lists, update it accordingly
    //Update UI for players only if the list changed somehow
    private void UpdatePlayerDictionary()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        bool foundDecendant = false;
        foreach (GameObject playerObj in playerObjects)
        {
            //For each player in the match:
            string playerUID = playerObj.GetComponent<PlayerCode>().PlayerUID;
            //Check if the player already exists in the dictionary
            if (!playerPoints.ContainsKey(playerUID))
            {
                playerPoints.Add(playerUID, 0);
                playerNames.Add(playerUID, playerObj.GetComponent<PlayerCode>().PlayerName + " " + playerObj.GetComponent<PlayerCode>().PlayerTitle);
                playerColors.Add(playerUID, AssignColor());
                Debug.Log("Server: Calling CmdUpdatePlayerListUI because a new playerUID was found");
                CmdUpdatePlayerListUI("");
            }

            if (!foundDecendant)
            {
                if (!playerObj.GetComponent<PlayerCode>().IsLocalPlayer())
                {
                    if (playerUID != null && playerUID!= "" && playerUID != " ")
                    {
                        if (decendantUID == "")
                        {
                            decendantUID = playerUID;
                            playerObj.GetComponent<PlayerCode>().SetAsHostDecendant();
                            foundDecendant = true;
                            CmdUpdatePlayerListUI("");
                        }
                        else if (decendantUID == playerUID)
                            foundDecendant = true;
                    }
                }
            }

            if (playerPoints[playerUID] > MaxPointsForMatch)
                playerPoints[playerUID] = MaxPointsForMatch;
        }

        if (!foundDecendant)
            decendantUID = "";

        if (!AreArraysEqual(playersCurrentlyInMatch, playerObjects)) //This means a player joined or left
        {
            //Debug.Log("Server: Calling CmdUpdatePlayerListUI because playersCurrentlyInMatch didn't equal playerObjects");
            CmdUpdatePlayerListUI("");
        }
        playersCurrentlyInMatch = playerObjects;
    }

    //Assigns new, unused color when called
    private Color AssignColor()
    {
        if (circleColorList.Count == 0)
            RefillColorList();
        Color retColor = circleColorList[Random.Range(0, circleColorList.Count)];
        circleColorList.Remove(retColor);
        return (retColor);
    }

    public void AddPointToUID(string UID)
    {
        if (UID != "" && playerPoints.ContainsKey(UID))
        {
            playerPoints[UID] = playerPoints[UID] + 1;
            Debug.Log("Awarded a point to the UID '" + UID +"'");
        }
        else if (UID != "")
        {
            playerPoints.Add(UID, 1);
            Debug.Log("Awarded a point to the UID '" + UID + "'");
        }
        else
        {
            Debug.Log("Awarded nobody points");
        }
    }

    //Get a list of all players who almost won the game
    public string GetAlmostWinners()
    {
        List<string> lst = new List<string>();
        foreach (string key in playerPoints.Keys)
        {
            if (key != "" && key != " ")
            {
                if (playerPoints[key] +1 >= MaxPointsForMatch)
                {
                    lst.Add(playerNames[key]);
                }
            }
        }
        return (TurnManager.StringSeparator(lst));
    }

    //Begin preparing for end by sending data to them for their endscreen
    public void BeginEndEnactment()
    {
        LoadScreen.SetActive(true);
        RpcLoadScreen();
        waitingForPlayerResponse = true;
        this.GetComponent<TurnManager>().TurnPaused = true;
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        playerAmountAtEnd = 0;
        foreach (GameObject playerObj in playerObjects)
        {
            playerAmountAtEnd++;
        }
        string CompressedPlayerList = GetPlayerListByPoints();
        compPlayerString = CompressedPlayerList;
        RpcPayEachPlayer(CompressedPlayerList);
        RpcSetMM_Temp(CompressedPlayerList);
    }

    [ClientRpc]
    private void RpcLoadScreen()
    {
        LoadScreen.SetActive(true);
    }

    [ClientRpc]
    private void RpcSetMM_Temp(string playerList)
    {
        SetMM_Temp(playerList);
    }

    //Used by clients to finish preparing for end and tell server that they're ready for endscreen
    private void SetMM_Temp(string playerList)
    {
        Debug.Log("Set MM_Temp!");
        PlayerPrefs.SetString("MM_Temp", playerList);
        //We sent a response, but now we have to get a confirmation that the response was gotten, two-generals style.
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj.GetComponent<PlayerCode>().IsLocalPlayer())
            {
                Debug.Log("Found local player");
                playerObj.GetComponent<PlayerCode>().ReadyToTerminate = true;
            }
        }
    }

    [ClientRpc]
    private void RpcTerminatePlayerConnection(string playerUid)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj.GetComponent<PlayerCode>().IsLocalPlayer())
            {
                if (playerObj.GetComponent<PlayerCode>().PlayerUID == playerUid)
                {
                    Debug.Log("Client got terminate command from the server");
                    if (!isServer)
                    {
                        GoToEnd(); //Player got the data he needed, he can now exit the server
                    }
                }
                break;
            }
        }
    }

    [ClientRpc]
    private void RpcPayEachPlayer(string compressedPlayerList)
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject playerObj in playerObjects)
        {
            playerObj.GetComponent<PlayerCode>().PayPlayer(compressedPlayerList);
        }
    }

    [ClientRpc]
    private void RpcGoToEnd()
    {
        GoToEnd();
    }

    private void GoToEnd()
    {
        Debug.Log("Disconnecting...");
        {
            Debug.Log("Disconnecting client.");
            NetworkManager.singleton.StopHost();

            NetworkManager.singleton.StopClient();
        }
        if (isServer)
        {
            Debug.Log("Disconnecting Server.");
            NetworkManager.singleton.StopHost();
        }
    }

    [Command]
    private void CmdGoToEnd(string playerList)
    {
        GoToEnd();
    }


    //Static function to compare if two arrays equal one another
    public static bool AreArraysEqual (GameObject[] arrayA, GameObject[] arrayB)
    {
        if (arrayA == null && arrayB == null)
            return true;
        else if ((arrayA == null && arrayB != null) || (arrayA != null && arrayB == null))
            return false;

        if (arrayA.Length != arrayB.Length)// This obviously means the lists can't be identical
            return false;
        for (int i = 0; i < arrayA.Length; i++)
        {
            if (arrayA[i] != arrayB[i]) // This means one of the values isn't equal
                return false;
        }
        return true;
    }

    //Get a sorted list of players by their point value
    public string GetPlayerListByPoints()
    {
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        List<string> cmdPlayerTexts = new List<string>();
        for (int i = MaxPointsForMatch; i >= 0; i--)
        {
            //Debug.Log("Max points for match - " + MaxPointsForMatch);
            if (playerPoints.ContainsValue(i))
            {
                foreach (GameObject playerObj in playerObjects)
                {
                    string playerUID = playerObj.GetComponent<PlayerCode>().PlayerUID;
                    if (playerPoints[playerUID] == i)
                    {
                        cmdPlayerTexts.Add(playerNames[playerUID]);
                    }
                }
            }
        }
        return(TurnManager.StringSeparator(cmdPlayerTexts));
    }

    [Command]
    public void CmdUpdatePlayerListUI(string removeUID)
    {
        //Sort all players based on their point value, so the player with the most points is at top
        //Instantiate them as such
        //In order to sort, we need to do the following things:
        //1. Get the highest value in the entire player list (We already do that thankfully in UpdatePlayerDictionary()
        //2. Create 2 new lists (player UIDS and score) for storing this sorted list
        //3. Do a for loop from maxScore to 0, and in each iteration check if the value exists in the player score dictionary. If it does, go over all the values in the player score dictionary, and add when found.
        RpcUpdatePlayerListUI(GetLocalPlayerDataForUI(removeUID));
    }

    [Command]
    public void CmdTurnEndPlayerListUI()
    {
        RpcTurnEndPlayerListUI(GetLocalPlayerDataForUI("",true));
    }

    private string GetLocalPlayerDataForUI(string removeUID, bool sendPlayerVotes = false)
    {
       
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        List<string> cmdPlayerDetals = new List<string>();
        for (int i = MaxPointsForMatch; i >= 0; i--)
        {
            if (playerPoints.ContainsValue(i))
            {
                foreach (GameObject playerObj in playerObjects)
                {
                    string playerUID = playerObj.GetComponent<PlayerCode>().PlayerUID;
                    if (playerPoints[playerUID] == i && removeUID != playerUID)
                    {
                        List<string> playerData = new List<string>();
                        playerData.Add(playerUID);//0 - UID
                        playerData.Add(playerNames[playerUID].ToString());//1 - Player Name
                        playerData.Add(playerPoints[playerUID].ToString());//2 - Player Points
                        playerData.Add(ColorUtility.ToHtmlStringRGB(playerColors[playerUID]));//3 - Color
                        if (sendPlayerVotes)
                            playerData.Add(playerObj.GetComponent<PlayerCode>().VotedMemeMadeBy);//4 - UID of vote - Add the UID of the user this player voted for only if neccessary
                        cmdPlayerDetals.Add(TurnManager.StringSeparator(playerData));
                    }
                }
            }
        }
        return (TurnManager.StringSeparator(cmdPlayerDetals));
    }

    private void OverrideHostDecendantString(string inputVal)
    {
        PlayerPrefs.SetString("MM_HostDecendantData", inputVal);
    }

    [ClientRpc]
    public void RpcTurnEndPlayerListUI(string players)
    {
        if (HostDecendant)
            OverrideHostDecendantString(players);

        List<string> UIplayerUIDs = new List<string>();
        List<int> UIplayerScores = new List<int>();
        List<int> UIplayerPositionDiffs = new List<int>();

        List<string> CMDplayerUIDs = new List<string>();
        List<int> CMDplayerScores = new List<int>();
        List<string> CMDplayerPositions = new List<string>();
        List<Color> CmdplayerColors = new List<Color>();
        List<string> CMDplayerNames = new List<string>();
        List<string> rpcPlayers = TurnManager.StringDeseparator(players);
        List<int> playerRankings = new List<int>();

        Dictionary<GameObject, int> animationNewPos = new Dictionary<GameObject, int>();
        Dictionary<string, int> animationNewScores = new Dictionary<string, int>();

        //Get local player
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        PlayerCode localPlayer = playerObjects[0].GetComponent<PlayerCode>();
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj.GetComponent<PlayerCode>().IsLocalPlayer())
            {
                localPlayer = playerObj.GetComponent<PlayerCode>();
                break;
            }
        }

        //Get data from UI icons
        foreach (Transform child in PlayerCircleParentObject.transform)
        {
            UIplayerUIDs.Add(child.GetComponent<LeaderboardCircleUIScript>().GetUUID());
            UIplayerScores.Add(int.Parse(child.GetComponent<LeaderboardCircleUIScript>().TextScore.text));
        }

        //Get data from Cmd function
        for (int i = 1; i <= rpcPlayers.Count; i++)
        {
            int rank = i;
            List<string> playerDetails = TurnManager.StringDeseparator(rpcPlayers[i - 1]);
            string playerScore = playerDetails[2];
            animationNewScores.Add(playerDetails[0], int.Parse(playerScore));
            for (int j = i + 1; j <= rpcPlayers.Count; j++)
            {
                if (TurnManager.StringDeseparator(rpcPlayers[j - 1])[2] == playerScore)
                    rank = j;
                else
                    j = rpcPlayers.Count + 1;
            }
            playerRankings.Add(rank);
        }
        int nonPlayerFound = 0;
        int playerFound = 0;
        for (int i = rpcPlayers.Count - 1; i >= 0; i--)
        {
            List<string> playerDetails = TurnManager.StringDeseparator(rpcPlayers[i]);
            string playerUID = playerDetails[0];
            if (playerUID == localPlayer.PlayerUID || ((playerFound == 0 && i <= 3) || (i <= 2)))
            {
                string playerLetter = playerDetails[1][0].ToString();
                string playerScore = playerDetails[2];
                int playerRank = playerRankings[i];

                Color playerColor;
                if (ColorUtility.TryParseHtmlString("#" + playerDetails[3], out playerColor))
                    CmdplayerColors.Add(playerColor);

                CMDplayerNames.Add(playerDetails[1]);
                CMDplayerUIDs.Add(playerUID);


                if (playerUID == localPlayer.PlayerUID)
                    playerFound++;
                else
                    nonPlayerFound++;

                CMDplayerScores.Add(int.Parse(playerScore));
                if (playerRank == 1)
                    CMDplayerPositions.Add("1st");
                else if (playerRank == 2)
                    CMDplayerPositions.Add("2nd");
                else if (playerRank == 3)
                    CMDplayerPositions.Add("3rd");
                else
                    CMDplayerPositions.Add(playerRank.ToString() + "th");
            }
        }

        //Check  existing UI positions diffs
        for (int i = 0; i < UIplayerUIDs.Count; i++)
        {
            int diff = -4;
            //Try to see if UI object exists in upcoming leaderboard
            for (int j = 0; j < CMDplayerUIDs.Count; j++)
            {
                if (UIplayerUIDs[i] ==  CMDplayerUIDs[j])
                {
                    //Player exists in new list, time to register his difference in position
                    diff = -1 * (i - j);//Diff is given minus because object is placed upside down
                    break;
                }
            }
            UIplayerPositionDiffs.Add(diff);
        }

        //Add all UI objects to animator dictionary
        int index = 0;
        foreach (Transform child in PlayerCircleParentObject.transform)
        {
            animationNewPos.Add(child.gameObject, UIplayerPositionDiffs[index]);
            index++;
        }

        //Check if any of the CMD objects didn't appear in the UI list
        for (int i = 0; i < CMDplayerUIDs.Count; i++)
        {
            bool found = false;
            //Try to see if UI object exists in upcoming leaderboard
            for (int j = 0; j < UIplayerUIDs.Count; j++)
            {
                if (UIplayerUIDs[j] == CMDplayerUIDs[i])
                {
                    found = true;
                    break;
                }
            }
            if (!found)//Cmd object doesn't exist in UI, time to make GameObject for it
            {
                GameObject createdPlayerUI = (Instantiate(PlayerCirclePrefab, PlayerCirclePrefab.transform.position, Quaternion.Euler(0, 0, 0), PlayerCircleParentObject.transform)) as GameObject;
                createdPlayerUI.transform.SetSiblingIndex(0);//Set as first child because order is reversed and we want it coming from bottom of leaderboard
                LeaderboardCircleUIScript newObjScript = createdPlayerUI.GetComponent<LeaderboardCircleUIScript>();
                newObjScript.SetBackgroundColor(CmdplayerColors[i]);
                newObjScript.SetFullName(CMDplayerNames[i]);
                newObjScript.SetUUID(CMDplayerUIDs[i]);
                newObjScript.TextName.text = CMDplayerNames[i][0].ToString();
                newObjScript.TextScore.text = CMDplayerScores[i].ToString();
                newObjScript.TextPosition.text = CMDplayerPositions[i];
                newObjScript.ChangeColorAccordingToPosition();

                //Add new player to animation dictionary
                animationNewPos.Add(createdPlayerUI, int.Parse(CMDplayerPositions[i].Replace("st", "").Replace("nd", "").Replace("rd", "").Replace("th", "")));
            }
        }

        //Update position text for all UI objects and hide it
        foreach (Transform child in PlayerCircleParentObject.transform)
        {
            LeaderboardCircleUIScript newObjScript = child.GetComponent<LeaderboardCircleUIScript>();
            for (int i = 0; i < CMDplayerUIDs.Count; i++)
            {
                if (newObjScript.GetUUID() == CMDplayerUIDs[i])
                {
                    newObjScript.TextPosition.text = CMDplayerPositions[i];
                    newObjScript.ChangeColorAccordingToPosition();
                }
            }
            newObjScript.SetPositionText(false);
        }

        //Start animation co-routine
        this.GetComponent<TurnManager>().SetupWhoVotedForWhoPanel(players);
        StartCoroutine(AnimateLeaderboardsUI(animationNewPos, animationNewScores, players));
    }

    private IEnumerator AnimateLeaderboardsUI(Dictionary<GameObject, int> animationNewPos, Dictionary<string, int> animationNewScores, string players, float timerNewScore = 4f, float timerChangePos = 1f)
    {
        PlayerCircleParentObject.GetComponent<RectTransform>().offsetMax = new Vector2((PlayerCircleParentObject.GetComponent<RectTransform>().offsetMax.x + (Screen.width - 200) / 4 * (animationNewPos.Count - 4)), PlayerCircleParentObject.GetComponent<RectTransform>().offsetMax.y);
        bool doFade = animationNewPos.Count > 4;
        yield return new WaitForSeconds(Time.deltaTime);//Wait a second so UI could update and ann new objects

        //Prepare variables
        List<float> CircleUIxByPosition = new List<float>();
        foreach (GameObject obj in animationNewPos.Keys)
        {
            CircleUIxByPosition.Add(obj.transform.position.x);
            Debug.Log("Circle by position x value: " + obj.transform.position.x);
        }
        CircleUIxByPosition.Sort();
        //CircleUIxByPosition.Reverse();
        float posDiff = Mathf.Abs(CircleUIxByPosition[0] - CircleUIxByPosition[1]);

        List<Vector3> originalPositons = new List<Vector3>();
        List<Vector3> targetPositions = new List<Vector3>();
        int index = 0;
        foreach (GameObject obj in animationNewPos.Keys)
        {
            originalPositons.Add(obj.transform.position);
            if (index < 4)
                targetPositions.Add(new Vector3(obj.transform.position.x - animationNewPos[obj] * posDiff, obj.transform.position.y, obj.transform.position.z));
            else
            {
                targetPositions.Add(new Vector3(CircleUIxByPosition[animationNewPos[obj] - 1], obj.transform.position.y, obj.transform.position.z));
                Debug.Log("First place X: " + CircleUIxByPosition[0] + "; Target X: " + targetPositions[index].x + "; Diff: " + posDiff + "; animationNewPos[obj] - 1: " + (animationNewPos[obj] - 1) + ";");
            }
            index++;
        }
        float timer = 0f;

        PlayerCircleParentObject.GetComponent<HorizontalLayoutGroup>().enabled = false;

        //Do score up animation
        while (timer < timerNewScore)
        {
            timer += Time.deltaTime;
            if (doFade)
            {
                foreach (GameObject obj in animationNewPos.Keys)
                {
                    obj.GetComponent<FadeInScript>().SetFade((CircleUIxByPosition[4] - obj.transform.position.x) / posDiff);
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        foreach (GameObject obj in animationNewPos.Keys)
        {
            LeaderboardCircleUIScript objScript =  obj.GetComponent<LeaderboardCircleUIScript>();
            objScript.TextScore.text = animationNewScores[objScript.GetUUID()].ToString();
        }

        //Do position change animation
        timer = 0f;
        while (timer < timerChangePos)
        {
            index = 0;
            foreach (GameObject obj in animationNewPos.Keys)
            {
                obj.transform.position = Vector3.Lerp(originalPositons[index], targetPositions[index], timer / timerChangePos);
                if (doFade)
                    obj.GetComponent<FadeInScript>().SetFade((CircleUIxByPosition[4] - obj.transform.position.x)/posDiff);
                index++;
            }
            timer += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }

        //Set children correctly
        PlayerCircleParentObject.GetComponent<RectTransform>().offsetMax = new Vector2(-200, PlayerCircleParentObject.GetComponent<RectTransform>().offsetMax.y);
        UpdatePlayerListUI(players);

        //Lock grid
        PlayerCircleParentObject.GetComponent<HorizontalLayoutGroup>().enabled = true;

    }

    [ClientRpc]
    public void RpcUpdatePlayerListUI(string players)
    {
        UpdatePlayerListUI(players);
    }

    //Function is used to update the UI for players
    private void UpdatePlayerListUI(string players)
    {
        //Start by getting player data
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");

        if (HostDecendant)
            OverrideHostDecendantString(players);

        PlayerCode localPlayer = playerObjects[0].GetComponent<PlayerCode>();
        foreach (GameObject playerObj in playerObjects)
        {
            if (playerObj.GetComponent<PlayerCode>().IsLocalPlayer())
            {
                localPlayer = playerObj.GetComponent<PlayerCode>();
                break;
            }
        }

        List<string> rpcPlayers = TurnManager.StringDeseparator(players);
        //Remove all children
        foreach (Transform child in PlayerCircleParentObject.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Get the rankings of players
        List<int> playerRankings = new List<int>();
        for (int i = 1; i <= rpcPlayers.Count; i++)
        {
            int rank = i;
            List<string> playerDetails = TurnManager.StringDeseparator(rpcPlayers[i - 1]);
            string playerScore = playerDetails[2];
            for (int j = i + 1; j <= rpcPlayers.Count; j++)
            {
                if (TurnManager.StringDeseparator(rpcPlayers[j - 1])[2] == playerScore)
                    rank = j;
                else
                    j = rpcPlayers.Count + 1;
            }
            playerRankings.Add(rank);
        }


        int nonPlayerFound = 0;
        int playerFound = 0;
        for (int i = rpcPlayers.Count - 1; i >= 0; i--)
        {
            List<string> playerDetails = TurnManager.StringDeseparator(rpcPlayers[i]);
            string playerUID = playerDetails[0];
            if (playerUID == localPlayer.PlayerUID || ((playerFound == 0 && i <= 3) || (i <= 2))) //Only show the top 3 players + the local player
            {
                string playerLetter = playerDetails[1][0].ToString();
                string playerScore = playerDetails[2];
                int playerRank = playerRankings[i];
                GameObject createdPlayerUI = (Instantiate(PlayerCirclePrefab, PlayerCirclePrefab.transform.position, Quaternion.Euler(0, 0, 0), PlayerCircleParentObject.transform)) as GameObject;
                LeaderboardCircleUIScript newObjScript = createdPlayerUI.GetComponent<LeaderboardCircleUIScript>();

                Color playerColor;
                if (ColorUtility.TryParseHtmlString("#" + playerDetails[3], out playerColor))
                    newObjScript.SetBackgroundColor(playerColor);

                newObjScript.SetFullName(playerDetails[1]);
                newObjScript.SetUUID(playerUID);


                if (playerUID == localPlayer.PlayerUID)
                {
                    newObjScript.SetOutlineColor(Color.green);
                    playerFound++;
                }
                else
                    nonPlayerFound++;


                newObjScript.TextName.text = playerLetter;
                newObjScript.TextScore.text = playerScore;
                if (playerRank == 1)
                    newObjScript.TextPosition.text = "1st";
                else if (playerRank == 2)
                    newObjScript.TextPosition.text = "2nd";
                else if (playerRank == 3)
                    newObjScript.TextPosition.text = "3rd";
                else
                    newObjScript.TextPosition.text = playerRank.ToString() + "th";
                newObjScript.ChangeColorAccordingToPosition();
            }
        }
    }

    public bool AllPlayersLocked (List<string> lockedPlayers)
    {
        foreach (string uid in playerNames.Keys)
        {
            if (uid != "" && uid != " ")
            {
                if (!lockedPlayers.Contains(uid))
                {
                    Debug.Log("Player " + uid + " is not locked yet.");
                    return false;
                }
            }
        }
        return true;
    }

    public int GetCurrentPlayerCount()
    {
        return (playersCurrentlyInMatch.Length);
    }
}

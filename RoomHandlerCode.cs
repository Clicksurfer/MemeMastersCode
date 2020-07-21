using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class RoomHandlerCode : NetworkMatch 
{
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescText;
    public TextMeshProUGUI DoButton;
    public TMP_InputField CodeTextbox;
    public bool isAtStartup = true;
    public string RoomCodeConnectedTo = "";
    public string roomPassword = "";
    public TextMeshProUGUI ErrorText;
    public GameObject LoadingPanel;

    private char[] alphabet = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private GameObject roomPanel;
    private NetworkManager networkManager;
    string roomCodeGenerated = "";
    NetworkClient myClient;

    //This script is responsible for all the roomcode logic
    //In turn, this means it is responsible for all player match creation and joining

    //Function is used when QuickPlay button is pressed
    //It tries to connect to existing public match and creates one if none are found
    public void ButtonQuickPlay()
    {
        if (LoadingPanel == null)
            LoadingPanel = FindInActiveObjectByName("EndLoadingPanel");
        if (LoadingPanel != null)
            LoadingPanel.SetActive(true);
        foreach (TextMeshProUGUI textObj in LoadingPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (textObj.name == "EditableText")
            {
                textObj.text = "Looking for public matches to join...";
                break;
            }
        }

        StartCoroutine(TryToConnectToPublicServer());
    }

    IEnumerator TryToConnectToPublicServer()
    {
        NetworkManager_Custom manager = this.GetComponent<NetworkManager_Custom>();
        manager.matchName = ""; // Making this string empty will be a joker for us to get all public matches
        manager.StartMatchMaker();

        //Try to find all public roomcodes and join the public one with the least amount of players
        float timer = 0f;
        float intervals = 0.25f;

        manager.matchMaker.ListMatches(0, 20, manager.matchName, true, 0, 0, manager.OnMatchList);

        Debug.Log("Sent list request, about to wait now");
        while (timer < 5f && manager.matches == null)
        {
            timer += intervals;
            yield return new WaitForSeconds(intervals);
        }

        if (manager.matches != null)
        {
            Debug.Log("Matches don't equal null");
            if (manager.matches.Count == 0)
            {
                Debug.Log("No matches fit description");
                StartPublicServer();
            }
            else
            {
                Debug.Log("Found a match matching the details! Going to join the first one.");
                int smallestPlayerCount = 999;
                string smallestPlayerRoomcode = "";
                NetworkID smallestNetworkID = NetworkID.Invalid;
                for (int i = 0; i < manager.matches.Count; i++)
                {
                    if (manager.matches[i].currentSize < smallestPlayerCount && manager.matches[i].currentSize < manager.matches[i].maxSize)
                    {
                        smallestPlayerCount = manager.matches[i].currentSize;
                        smallestPlayerRoomcode = manager.matches[i].name;
                        smallestNetworkID = manager.matches[i].networkId;
                    }
                }
                if (smallestPlayerCount == 999)
                {
                    StartPublicServer();
                }
                manager.matchName = smallestPlayerRoomcode;
                string password = "";
                manager.matchMaker.JoinMatch(smallestNetworkID, password, "", "", 0, 0, manager.OnMatchJoined);
            }
        }
        else
        {
            StartPublicServer();
        }
    }

    public void StartPublicServer()
    {
        foreach (TextMeshProUGUI textObj in LoadingPanel.GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (textObj.name == "EditableText")
            {
                textObj.text = "Creating public match...";
                break;
            }
        }
        Debug.Log("Starting public server since no public room is available");
        NetworkManager_Custom manager = this.GetComponent<NetworkManager_Custom>();
        manager.matchName = GenerateMatchName();
        manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, "", "", "", 0, 0, manager.OnMatchCreate);
    }

    //Function is called when a player tries to start a new private match
    public void ButtonStartServer(string roomCodeKnown = "")
    {
        NetworkManager_Custom manager = this.GetComponent<NetworkManager_Custom>();
        if (LoadingPanel == null)
            LoadingPanel = FindInActiveObjectByName("EndLoadingPanel");
        if (LoadingPanel!=null)
            LoadingPanel.SetActive(true);
        manager.StartMatchMaker();
        if (roomCodeKnown != "")
            manager.matchName = roomCodeKnown;
        else
            manager.matchName = GenerateMatchName();
        Debug.Log("Manager match name - " + manager.matchName);
        StartCoroutine(startServerAfterStuff(manager, roomCodeKnown));
    }

    //This is the Coroutine that effectively creates a new private server on player's machine
    IEnumerator startServerAfterStuff(NetworkManager_Custom manager, string roomCodeKnown = "")
    {
        Debug.Log("Start server after stuff is being called!");
        roomPassword = manager.matchName;
        manager.matchMaker.CreateMatch(manager.matchName, manager.matchSize, true, roomPassword, "", "", 0, 0, manager.OnMatchCreate);//Setting the room password as the room name, so random players won't be able to join

    }

    //Button logic for private server creation/joining
    public void ButtonDoScript()
    {
        if (DoButton.text == "Create")
        {
            this.GetComponent<NetworkManager_Custom>().StartHost();
            DoButton.transform.parent.GetComponent<Button>().interactable = false;
        }
        else if (DoButton.text == "Join")
        {
            LoadingPanel.SetActive(true);
            ErrorText.text = "";
            string inputMatchName = this.GetComponent<RoomHandlerCode>().CodeTextbox.text;
            inputMatchName = inputMatchName.ToUpper().Replace(" ", "").Replace("\n", "");
            if (IsMatchNameValid(inputMatchName))
            {
                DoButton.transform.parent.GetComponent<Button>().interactable = false;
                this.GetComponent<NetworkManager_Custom>().matchName = inputMatchName;
                ConnectPlayerToRoomcode();
            }
            else
            {
                LoadingPanel.SetActive(false);
                DoButton.transform.parent.GetComponent<Button>().interactable = true;
                TimedOutConnecting("<b>Error</b> - Room code is invalid.");
            }
        }
    }

    //Function is called when a player crashed from a server
    //It automatically tries to reconnect him to the crashed server
    public void ReconnectToMatch(string roomCode)
    {
        if (LoadingPanel == null)
            LoadingPanel = FindInActiveObjectByName("EndLoadingPanel");
        if (LoadingPanel != null)
            LoadingPanel.SetActive(true);
        string inputMatchName = roomCode;
        inputMatchName = inputMatchName.ToUpper().Replace(" ", "").Replace("\n", "");
        if (IsMatchNameValid(inputMatchName))
        {
            this.GetComponent<NetworkManager_Custom>().matchName = inputMatchName;
            ConnectPlayerToRoomcode(true);
        }
        else
        {
            if (LoadingPanel != null)
                LoadingPanel.SetActive(false);
        }
    }

    public void ConnectPlayerToRoomcode(bool retryMultipleTimes = false)
    {
        NetworkManager_Custom manager = this.GetComponent<NetworkManager_Custom>();
        manager.StartMatchMaker();

        int retries = 0;
        if (retryMultipleTimes)
            retries = 10;
        StartCoroutine(ConnectToRoomWhenLoaded(manager, false, retries));
    }

    //This is the coroutine that searches for a specific room in the list of existing rooms
    //If it finds the room we're looking for, it will try to join it
    IEnumerator ConnectToRoomWhenLoaded(NetworkManager_Custom manager, bool hidePrivateMatches = true, int retryCounter = 0)
    {
        if (retryCounter > 0)
            Debug.Log("Retrying! " + retryCounter + " tries left before final.");
        else
            Debug.Log("No more retries left");
        float timer = 0f;
        float intervals = 0.25f;

        manager.matchMaker.ListMatches(0, 20, manager.matchName, hidePrivateMatches, 0, 0, manager.OnMatchList);

        Debug.Log("Sent list request, about to wait now");
        while (timer < 5f && manager.matches == null)
        {
            timer += intervals;
            yield return new WaitForSeconds(intervals);
        }
        Debug.Log("Got to end of waiting period");

        if (manager.matches != null)
        {
            Debug.Log("Matches don't equal null");
            if (manager.matches.Count == 0)
            {
                Debug.Log("No matches fit description");
                if (retryCounter > 0)
                {
                    Debug.Log("Going to retry now because there are no matches fitting desc");
                    StartCoroutine(ConnectToRoomWhenLoaded(manager, hidePrivateMatches, retryCounter - 1));
                }
                else
                {
                    Debug.Log("Done retrying - no match fitting desc.");
                    DoButton.transform.parent.GetComponent<Button>().interactable = true;
                    TimedOutConnecting("<b>Error</b> - Room not found.");
                }
            }
            else
            {
                Debug.Log("Found a match matching the details! Going to join the first one.");
                manager.matchName = manager.matches[0].name;

                manager.matchSize = (uint)manager.matches[0].maxSize;

                string password = "";
                if (!hidePrivateMatches)//If we're connecting to a private match, set the password as the room name.
                    password = manager.matchName;
                manager.matchMaker.JoinMatch(manager.matches[0].networkId, password, "", "", 0, 0, manager.OnMatchJoined);
            }
        }
        else
        {
            if (retryCounter > 0)
                StartCoroutine(ConnectToRoomWhenLoaded(manager, hidePrivateMatches, retryCounter - 1));
            else
            {
                DoButton.transform.parent.GetComponent<Button>().interactable = true;
                TimedOutConnecting("<b>Error</b> - Room not found.");
            }
        }
    }

    public void TimedOutConnecting(string defaultError = "<b>Error</b> - Timed out connecting to room.")
    {
        ErrorText.text = defaultError;
        if (LoadingPanel != null)
            LoadingPanel.SetActive(false);
        DoButton.transform.parent.GetComponent<Button>().interactable = true;
    }

    GameObject FindInActiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None)
            {
                if (objs[i].name == name)
                {
                    return objs[i].gameObject;
                }
            }
        }
        return null;
    }


    // Update is used here just to find relevant GameObjects during runtime
    // It is neccessary to do in update since this script goes between scenes
    void Update () {
		if (roomPanel == null)
        {
            //Debug.Log("Looking for room panel");
            roomPanel = GameObject.FindGameObjectWithTag("RoomPanel");
        }
        else if (TitleText == null)
        {
            TextMeshProUGUI[] texts = roomPanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI aText in texts)
            {
                if (aText.gameObject.name == "TextTitle")
                    TitleText = aText;
                else if (aText.gameObject.name == "TextDesc")
                    DescText = aText;
            }
            DoButton = roomPanel.GetComponentInChildren<Button>().gameObject.GetComponentsInChildren<TextMeshProUGUI>()[0];
            CodeTextbox = roomPanel.GetComponentInChildren<TMP_InputField>();
        }
        RoomCodeConnectedTo = this.GetComponent<NetworkManager_Custom>().matchName;
    }

    public void PopulateRoomPanel(string panelType)
    {
        if (panelType == "creator")
        {
            ////This is not used anymore because there's no longer a private match creation 'window' - it just creates a private match
            // TitleText.text = "Create Room";
            // DescText.text = "Use the following room code:";
            // DoButton.text = "Create";
            // CodeTextbox.text = GenerateRoomCode();
            // CodeTextbox.interactable = false;
        }
        else if (panelType == "joiner")
        {
            TitleText.text = "Join Room";
            DescText.text = "Enter the room code:";
            DoButton.text = "Join";
            CodeTextbox.text = "";
            CodeTextbox.interactable = true;
        }
    }

    public string GenerateMatchName()
    {
        string retString = "";
        for (int i = 0; i < 4; i++)
        {
            retString += alphabet[Random.Range(0, 26)];
        }
        return retString;
    }

    public bool IsMatchNameValid(string name)
    {
        foreach (char letter in name)
        {
            bool foundMatchingLetter = false;
            for (int i=0;i<alphabet.Length;i++)
            {
                if (letter == alphabet[i])
                    foundMatchingLetter = true;
            }
            if (foundMatchingLetter == false)
                return false;
        }
        return true;
    }

    public string GenerateRoomCode()
    {
        return (IpToRoomcode(IPManager.GetIP(ADDRESSFAM.IPv4)));
    }

    //Unused, deprecated function
    public bool CheckIfRoomExists(string roomCode)
    {
        //TODO - put code here
        return false;
    }
    
    //Functio turns input IP address string into a roomcode. Currently unused.
    public string IpToRoomcode(string ipAddress)
    {
        //Turn it into number without dots (Adding 0 of course), and split it in two
        //Convert from base 10 to base 26
        //Convert to string

        string RetString = "";
        string[] ipSplit = ipAddress.Split('.');//Break IP into subnets
        //Debug.Log("Ip split.length - " + ipSplit.Length);
        for (int i = 0; 0 < ipSplit.Length; i++)//Make sure IP is displayed with zeroes so each part is three chars long
        {
            if (i >= ipSplit.Length)
                break;
            while (ipSplit[i].Length < 3)
            {
                ipSplit[i] = "0" + ipSplit[i];
            }
            //Debug.Log("Ip part - " + ipSplit[i]);
        }
        string roomCodeFormer = IntToString(int.Parse(ipSplit[0] + ipSplit[1]), alphabet);
        string roomCodeLatter = IntToString(int.Parse(ipSplit[2] + ipSplit[3]), alphabet);
        RetString = roomCodeFormer + "-" + roomCodeLatter;
        //Debug.Log("Room code - " + RetString);
        return (RetString);
    }

    //Function is used to convert input roomCode into an IP address. Currently unused.
    public string RoomcodeToIp(string roomCode)
    {
        //Turn it into number without '-' (Adding 0 of course), and split it in two
        //Convert from base 26 to base 10
        //Convert to string
        roomCode = roomCode.ToUpper().Replace(" ","").Replace("\n","");
        string RetString = "";
        string[] roomCodeSplit = roomCode.Split('-');//Break IP into subnets
        for (int i = 0; 0 < roomCodeSplit.Length; i++)//Make sure IP is displayed with zeroes so each part is three chars long
        {
            if (i >= roomCodeSplit.Length)
                break;
            string decValueOfRoomcode = StringToInt(roomCodeSplit[i], alphabet).ToString();
            while (decValueOfRoomcode.Length < 6)
            {
                decValueOfRoomcode = "0" + decValueOfRoomcode;
            }
            if (RetString != "")
                RetString += ".";
            RetString += decValueOfRoomcode.Substring(0, 3) + "." + decValueOfRoomcode.Substring(3, 3);
        }
        string newRetString = "";
        string[] ipSplitByDot = RetString.Split('.');
        for (int i = 0; i < ipSplitByDot.Length; i++)
        {
            if (newRetString != "")
                newRetString += ".";

            bool foundSomeDigit = false;
            for (int j=0;j<ipSplitByDot[i].Length;j++)
            {
                if (ipSplitByDot[i][j] != '0' || j == ipSplitByDot[i].Length - 1 || foundSomeDigit)
                {
                    foundSomeDigit = true;
                    newRetString += ipSplitByDot[i][j];
                }
            }
        }
        RetString = newRetString;
        return (RetString);
    }

    public static string IntToString(int value, char[] baseChars)
    {
        string result = string.Empty;
        int targetBase = baseChars.Length;

        do
        {
            result = baseChars[value % targetBase] + result;
            value = value / targetBase;
        }
        while (value > 0);

        return result;
    }

    public static int StringToInt(string value, char[] baseChars)
    {
        int result = 0;
        for (int i = 0; i < value.Length; i++)
        {
            result = result * baseChars.Length;
            for (int j = 0; j < baseChars.Length; j++)
            {
                if (value[i] == baseChars[j])
                {
                    //Found the char, now we know the value
                    result += j;
                    break;
                }
            }
        }
        return result;
    }
}

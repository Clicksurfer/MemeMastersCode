using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerWaitingManager : NetworkBehaviour
{
    public float MaxRotation = 10f;
    public GameObject[] spawnPoints;
    
    public GameObject PrefabPlayerWaiting;


    GameObject[] players;
    private List<string> playerNamesForDisplay = new List<string>();
    private List<string> tempPlayerNamesForDisplay = new List<string>();

    //Script is used to manage the displayed player names in the waiting for match start panel

    //Update is used to track connected players & make sure UI elements are placed correctly
    void Update()
    {
        Centralizer();
        if (isServer)
        {
            CheckPlayers();
        }
        RemoveNonExistantChildren();
        AddNewChildren();
    }

    //This function checks the connected players and updates names for display
    private void CheckPlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        tempPlayerNamesForDisplay.Clear();
        foreach (GameObject player in players)
        {
            string fullPlayerName = player.GetComponent<PlayerCode>().PlayerName + " " + player.GetComponent<PlayerCode>().PlayerTitle;
            tempPlayerNamesForDisplay.Add(fullPlayerName);
        }
        if (!AreListsEqual(tempPlayerNamesForDisplay, playerNamesForDisplay))
        {
            string tempString = "";
            string playerString = "";
            foreach (string name in tempPlayerNamesForDisplay)
                tempString += name + ",";
            foreach (string name in playerNamesForDisplay)
                playerString += name + ",";
            playerNamesForDisplay = tempPlayerNamesForDisplay;
            RpcCheckPlayers(StringSeparator(playerNamesForDisplay));
        }
    }

    //This function removes any players that are displayed but not connected
    private void RemoveNonExistantChildren()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            foreach (Transform child in spawnPoints[i].transform)
            {
                if (!playerNamesForDisplay.Contains(child.GetComponent<PlayerNameCode>().PlayerName))
                {
                    child.GetComponent<PlayerNameCode>().Destroy();
                }
            }
        }
    }

    //Adds to display any player that isn't displayed yet
    private void AddNewChildren()
    {
        foreach (string name in playerNamesForDisplay)
        {
            bool found = false;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                foreach (Transform child in spawnPoints[i].transform)
                {
                    if (child.GetComponent<PlayerNameCode>().PlayerName == name)
                        found = true;
                }
            }
            if (!found && name != " ")
                CreateNewChild(name);
        }
    }

    //Instantiates a playername in one of the available spaces in a wacky rotation
    private void CreateNewChild(string newName)
    {
        List<int> availableSpaces = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++)
            if (spawnPoints[i].transform.childCount == 0)
                availableSpaces.Add(i);
        int newIndex = Random.Range(0, availableSpaces.Count);
        GameObject spawnParent = spawnPoints[availableSpaces[newIndex]];

        GameObject newChild = (Instantiate(PrefabPlayerWaiting, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), spawnParent.transform)) as GameObject;

        newChild.GetComponent<RectTransform>().anchorMin = new Vector2(0, 0);
        newChild.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        newChild.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        newChild.GetComponent<RectTransform>().offsetMin = new Vector2(50, 50);
        newChild.GetComponent<RectTransform>().offsetMax = new Vector2(-50, -50);
        float randomRotation = Random.Range(-MaxRotation, MaxRotation);
        if (randomRotation < 0f)
            randomRotation += 360f;
        newChild.transform.Rotate(0, 0, randomRotation);
        newChild.GetComponent<PlayerNameCode>().PlayerName = newName;
    }

    public static bool AreListsEqual(List<string> listA, List<string> listB)
    {
        if (listA.Count != listB.Count)// This obviously means the lists can't be identical
            return false;
        for (int i = 0; i < listA.Count; i++)
        {
            if (listA[i] != listB[i]) // This means one of the values aren't equal
                return false;
        }
        return true;
    }

    [ClientRpc]
    private void RpcCheckPlayers(string playerNamesSeparated)
    {
        playerNamesForDisplay = StringDeseparator(playerNamesSeparated);
    }

    private void Centralizer()
    {
        this.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
        this.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
    }

    //Defined custom function here to merge list into single string
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
        }
        return returnString;
    }

    //Defined custom function here to separate string into list
    public static List<string> StringDeseparator(string str)
    {
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
                    returnList.Add(newPart);
                    prevPoint = i + 2;
                }
            }
            else
            {
                string newPart = str.Substring(prevPoint).Replace(",,", ",");
                returnList.Add(newPart);
            }
        }
        return returnList;
    }
}

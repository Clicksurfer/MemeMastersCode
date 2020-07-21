using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerLister : MonoBehaviour
{
    public GameObject PlayerEndEntryPrefab;
    public float startingXOffset = -20f;
    public float XMax = 0.95f;
    public float XDiff = 10f;
    public float TimePerName = 1f;

    //Script is used in endscreen to display all the players and their final ranking in the UI

    void OnEnable()
    {
        EnactEnd();
    }

    public void EnactEnd()
    {
        string playerList = PlayerPrefs.GetString("MM_Temp", "");
        PlayerPrefs.SetString("MM_Temp", "");
        if (playerList != "")
        {
            List<string> UncompressedPlayerList = TurnManager.StringDeseparator(playerList);
            string WinnerName = UncompressedPlayerList[0];
            GameObject myCanvas = GameObject.Find("Canvas");
            EndManager myManager = myCanvas.GetComponentInChildren<EndManager>();
            myManager.LoadEnd(WinnerName, UncompressedPlayerList);
        }
    }

    public void GeneratePlayerEntries(List<string> players)
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        for (int i = 0; i < players.Count; i++)
        {
            GameObject newPlayerObj = (Instantiate(PlayerEndEntryPrefab, this.transform)) as GameObject;
            newPlayerObj.GetComponentsInChildren<TextMeshProUGUI>()[0].text = players[i];
            newPlayerObj.GetComponentsInChildren<TextMeshProUGUI>()[1].text = (i + 1).ToString() + "#";
            newPlayerObj.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 1f - 0.1f * (i + 1));
            newPlayerObj.GetComponent<RectTransform>().anchorMax = new Vector2(XMax, 1f - 0.1f * (i));
            newPlayerObj.GetComponent<FadeInScript>().FadeInDelay = 2 + i * TimePerName;
            newPlayerObj.GetComponent<AnimatorDelayer>().AnimationDelay = 2 + i * TimePerName;
            newPlayerObj = SetRectTransformDirs(newPlayerObj, 5f, 5f, -500f, startingXOffset + XDiff * i);
        }
    }

    private GameObject SetRectTransformDirs(GameObject obj, float top, float bottom, float left, float right)
    {
        obj.GetComponent<RectTransform>().offsetMin = new Vector2(left, bottom);
        obj.GetComponent<RectTransform>().offsetMax = new Vector2(-right, -top);
        return obj;
    }
}
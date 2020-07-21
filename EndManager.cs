using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Advertisements;

public class EndManager : MonoBehaviour
{
    public TextMeshProUGUI WinnerName;
    public PlayerLister MyLister;
    public GameObject BottomObj;

    //Script is responsible for managing the end scene of the match
    private void Awake()
    {
        StartCoroutine(ShowAd());
    }

    public void LoadEnd(string winnerName, List<string> playerList)
    {
        WinnerName.text = winnerName;
        StartCoroutine(ShowAd());//Ads were initialized in initial script, this is essential for ads to work.
        MyLister.GeneratePlayerEntries(playerList);
        BottomObj.GetComponent<FadeInScript>().FadeInDelay = (2.5f + MyLister.TimePerName * playerList.Count);
    }

    public static IEnumerator ShowAd()
    {
        Debug.Log("Told to show ad!");
        while (!Advertisement.IsReady("video"))
        {
            Debug.Log("Waiting to show video");
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Debug.Log("Ad loaded, about to show!");
        Advertisement.Show("video");
        Debug.Log("Ad shown");
    }
}

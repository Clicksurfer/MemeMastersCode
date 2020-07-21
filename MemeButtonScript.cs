using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemeButtonScript : MonoBehaviour {

    public string CreatorUID = "";
    public bool IsNextMemeButton = false;

    private TurnManager myTurnManager;
    GameObject[] players;

	// Script is used by MemeButtons in match, to be pressed by player
	void Start () {
        myTurnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponentInChildren<TurnManager>();
        if (!IsNextMemeButton)
            this.GetComponent<RectTransform>().sizeDelta = new Vector2(transform.parent.GetComponent<ChangeContentSize>().GetImageSize(), this.GetComponent<RectTransform>().sizeDelta.y);
    }

    
    void Update () {
        myTurnManager = GameObject.FindGameObjectWithTag("GameManager").GetComponentInChildren<TurnManager>();
    }

    //When meme button is pressed, save the player's choice in the player's Gameobject
    //Also trigger other function to do something on the screen
    public void OnButtonPress()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        string localUid = "";
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerCode>().ChooseMemeToVoteFor(CreatorUID);
            if (player.GetComponent<PlayerCode>().IsLocalPlayer())
            {
                localUid = player.GetComponent<PlayerCode>().PlayerUID;
                player.GetComponent<PlayerCode>().CmdLockVoteButtonPress();
            }
        }
        if (IsNextMemeButton)
            myTurnManager.UpdateNextMemeButtonColors(CreatorUID);
        else
        {
            myTurnManager.UpdateMemeVoteButtonColors(CreatorUID);
        }
    }
}

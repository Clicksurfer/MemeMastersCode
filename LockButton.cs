using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LockButton : MonoBehaviour {

    public bool locked = false;
    public TextMeshProUGUI MyTextObject;
    public Color UnlockedColor = new Color(48f / 255f, 217f / 255f, 30f/255f);
    public Color LockedColor = new Color(217f / 255f, 33f / 255f, 30f / 255f);
    public string UnlockedText = "Lock-In";
    public string LockedText = "Un-Lock";

    //Script is responsible for locking and unlocking player's textbox
    //More importantly, it notifies the server when he's ready to advance to next round
    void Start () {
        locked = false;
	}

    //Resets progress on lock button whenever a new round starts
    void OnEnable()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        locked = false;
        this.GetComponent<Image>().color = UnlockedColor;
        MyTextObject.text = UnlockedText;
        for (int i = 0; i < players.Length; i++)
        {
            players[i].GetComponent<PlayerCode>().UnlockSubmitButtonPress();
        }
    }

    //When the button is pressed, lock/unlock input accordingly
    public void pressed()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (!locked)
        {
            this.GetComponent<Image>().color = LockedColor;
            MyTextObject.text = LockedText;
            for (int i = 0; i < players.Length; i++)
            {
                    players[i].GetComponent<PlayerCode>().LockSubmitButtonPress();
            }
        }
        else
        {
            this.GetComponent<Image>().color = UnlockedColor;
            MyTextObject.text = UnlockedText;
            for (int i = 0; i < players.Length; i++)
            {
                    players[i].GetComponent<PlayerCode>().UnlockSubmitButtonPress();
            }
        }
        locked = !locked;
    }
}

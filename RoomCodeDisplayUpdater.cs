using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomCodeDisplayUpdater : MonoBehaviour 
{

    public RoomHandlerCode targetRoomHandlerCode;
    private Text myText;
    private TextMeshProUGUI myProText;

    //Update the text object this is attached to with current roomcode the client is connected to from RoomHandlerCode

	void Start () 
    {
        targetRoomHandlerCode = GameObject.FindGameObjectWithTag("MatchManager").GetComponent<RoomHandlerCode>();
        myText = this.GetComponent<Text>();
        myProText = this.GetComponent<TextMeshProUGUI>();
    }

    void Update () 
    {
        if (myText != null)
            myText.text = targetRoomHandlerCode.RoomCodeConnectedTo;
        else if (myProText != null)
            myProText.text = "Room Code: <b>" + targetRoomHandlerCode.RoomCodeConnectedTo + "</b>";
	}
}

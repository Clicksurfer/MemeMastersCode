using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MatchManagerFinder : MonoBehaviour
{
    public GameObject MyMatchManager;
    public GameObject LoadingPanel;
    public bool isErrorText = false;

    	
	// Script is used to find MatchManager object in the scene
    // It also contains some common functions used by objects looking for MatchManager
	void Update ()
    {
		if (MyMatchManager == null)
        {
            MyMatchManager = GameObject.FindGameObjectWithTag("MatchManager");
            if (isErrorText)
                MyMatchManager.GetComponent<RoomHandlerCode>().ErrorText = this.gameObject.GetComponent<TextMeshProUGUI>();
        }
	}

    public void RemoteButtonDoScript()
    {
        MyMatchManager.GetComponent<RoomHandlerCode>().LoadingPanel = LoadingPanel;
        MyMatchManager.GetComponent<RoomHandlerCode>().ButtonDoScript();
    }

    public void RemoteButtonQuickPlay()
    {
        MyMatchManager.GetComponent<RoomHandlerCode>().LoadingPanel = LoadingPanel;
        MyMatchManager.GetComponent<RoomHandlerCode>().ButtonQuickPlay();
    }

    public void RemoteButtonStartServer()
    {
        MyMatchManager.GetComponent<RoomHandlerCode>().LoadingPanel = LoadingPanel;
        MyMatchManager.GetComponent<RoomHandlerCode>().ButtonStartServer();
    }
}

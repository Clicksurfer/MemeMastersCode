using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    public GameObject PanelNewPlayer;
    public GameObject InitialPanel;

    // Script simply loads data and enabled the starting panels
    
    private void Awake()
    {
        if (! InitialScript.Initialized)
        {
            PlayerPrefs.SetString("MM_CrashRoomcode", "");
            PlayerPrefs.SetString("MM_HostDecendantData", "");
        }
    }

    void Start () {
        PanelNewPlayer.SetActive(true);
        InitialPanel.SetActive(true);

    }
}

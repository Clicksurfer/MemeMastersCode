using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameShopScript : MonoBehaviour {

    public GameObject NewPlayerPanel;
    private TMP_InputField playerNameTextbox;

    //This script is used to allow the player to edit his nickname inside the shop

    void Awake()
    {
        playerNameTextbox = this.GetComponent<TMP_InputField>();
    }

    void Start () {
        OnEnable();
	}

    void OnEnable()
    {
        UpdateTextbox();
    }

    public void OnTextboxChange()
    {
        if (CheckInputValidity())
        {
            SaveData();
        }
        UpdateTextbox();
    }

    private bool CheckInputValidity()
    {
        var regexItem = new System.Text.RegularExpressions.Regex("^[a-zA-Z0-9 ]*$");

        return (playerNameTextbox.text != "" && regexItem.IsMatch(playerNameTextbox.text));
    }

    private void SaveData()
    {
        PlayerPrefs.SetString("MM_Username", playerNameTextbox.text);
    }

    private void UpdateTextbox()
    {
        playerNameTextbox.text = PlayerPrefs.GetString("MM_Username", "Default_Name");
        NewPlayerPanel.GetComponent<NewPlayerCode>().UpdateStartPanel();
    }
}

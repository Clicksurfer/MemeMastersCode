using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyUpdater : MonoBehaviour
{
    TextMeshProUGUI myText;

    //Update the text object this is attached to with current player's money from playerprefs
    void Start()
    {
        myText = this.GetComponent<TextMeshProUGUI>();
    }
    void Update()
    {
        myText.text = "<b>" + PlayerPrefs.GetInt("MM_Money", 0) + "</b>$";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGColorSetter : MonoBehaviour
{
    //Attached Gameobject with Image will change color based on player's customization
    void Start()
    {
        string htmlPlayerColor = PlayerPrefs.GetString("MM_BGColor", "");
        if (htmlPlayerColor != "")
        {
            Color inputColor;
            if (ColorUtility.TryParseHtmlString(htmlPlayerColor, out inputColor))
                this.GetComponent<Image>().color = inputColor;
        }
    }
}

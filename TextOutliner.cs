using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextOutliner : MonoBehaviour {

    public Text TextToOutline;
    public TextMeshProUGUI ProTextToOutline;

    Text[] myTexts;
    TextMeshProUGUI[] myProTexts;

    //Script is responsible for outlining a certain text object with other texts

    // This is handled by update which changes the text's value and size to the outlined text
    void LateUpdate()
    {
        if (myTexts == null)
        {
            myTexts = this.GetComponentsInChildren<Text>();
        }
        else
        {
            for (int i = 0; i < myTexts.Length; i++)
            {
                myTexts[i].text = TextToOutline.text;
                myTexts[i].fontSize = TextToOutline.fontSize;
            }
        }
        if (myProTexts == null)
        {
            myProTexts = this.GetComponentsInChildren<TextMeshProUGUI>();
        }
        else
        {
            for (int i = 0; i < myProTexts.Length; i++)
            {
                myProTexts[i].text = ProTextToOutline.text;
                myProTexts[i].fontSize = ProTextToOutline.fontSize;
            }
        }
    }
}

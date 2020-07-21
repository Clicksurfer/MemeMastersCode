using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameCode : MonoBehaviour 
{

    public string PlayerName;
    private string playerNameFiltered;
    private string playerNameUnfiltered;
    private Text[] myTexts;

    //This script is attached to text objects which should show the player's name but without HTML color coding

	void Start () {
        playerNameUnfiltered = "";
        playerNameFiltered = "";
        myTexts = this.transform.GetChild(0).GetComponentsInChildren<Text>();
	}

    private static string RemoveHTMLColorCoding(string input)
    {
        string output = input;
        output = output.Replace("</color>", "");
        string colorText = "<color=#XXXXXX>";
        for (int i = 0; i < output.Length; i++)
        {
            if (i + colorText.Length <= output.Length)
            {
                bool foundMatch = true;
                for (int j=0;j<colorText.Length;j++)
                {
                    if (j <8 || j > 13)
                    {
                        if (colorText[j] != output[i + j])
                        {
                            foundMatch = false;
                            break;
                        }
                    }
                }
                if (foundMatch)
                {
                    output = output.Substring(0, i) + output.Substring(i + 15, output.Length - i - 15);
                    break;
                }
            }
        }
        return (output);
    }

    void LateUpdate()
    {
        if (playerNameUnfiltered != PlayerName)
        {
            playerNameUnfiltered = PlayerName;
            playerNameFiltered = RemoveHTMLColorCoding(PlayerName);
            foreach (Text myText in myTexts)
            {
                if (myText.name == "TextMain")
                    myText.text = playerNameUnfiltered;
                else
                    myText.text = playerNameFiltered;
            }
        }
    }

    public void Destroy()
    {
        StartCoroutine(DestroyObject(gameObject));
    }

    IEnumerator DestroyObject(GameObject animated)
    {
        animated.GetComponent<Animator>().SetBool("WindowOpen", false);
        yield return new WaitForSeconds(Time.deltaTime);
        while (animated.GetComponent<RectTransform>().localScale.x != 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
        }
        Destroy(gameObject);
    }
}

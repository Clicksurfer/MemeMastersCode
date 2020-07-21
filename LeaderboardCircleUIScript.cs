using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardCircleUIScript : MonoBehaviour {

    public TextMeshProUGUI TextName;
    public TextMeshProUGUI TextScore;
    public TextMeshProUGUI TextPosition;
    public GameObject FullNamePanel;
    public Image CircleBackground;
    public AudioClip WindowOpen;
    public AudioClip WindowClose;
    public Image CircleOutline;

    private Color firstColor = Color.yellow;
    private Color secondColor = new Color(204f / 255f, 204f / 255f, 204f / 255f);
    private Color thirdColor = new Color(183f / 255f, 121f / 255f, 34f / 255f);
    private Color otherColor = Color.white;
    private string UUID;
    private string FullName;
    private bool fullNameOpenState = false;

    //Script is used as data for LeaderboardCircleUI objects. Has some basic functions to edit looks.

    public void ChangeColorAccordingToPosition()
    {
        if (TextPosition.text.Contains("1"))
            TextPosition.color = firstColor;
        else if (TextPosition.text.Contains("2"))
            TextPosition.color = secondColor;
        else if (TextPosition.text.Contains("3"))
            TextPosition.color = thirdColor;
        else
            TextPosition.color = otherColor;
    }

    public void SetBackgroundColor(Color input)
    {
        CircleBackground.color = input;
    }

    public void SetOutlineColor(Color input)
    {
        CircleOutline.color = input;
    }

    public void SetUUID(string input)
    {
        UUID = input;
    }

    public string GetUUID()
    {
        return UUID;
    }

    public void SetFullName(string input)
    {
        FullName = input;
    }

    public void ActivateFullnamePanel()
    {
        fullNameOpenState = !fullNameOpenState;
        FullNamePanel.GetComponentInChildren<TextMeshProUGUI>().text = FullName;
        FullNamePanel.GetComponent<Animator>().SetBool("FullNameAppear", fullNameOpenState);
        if (fullNameOpenState)
            this.GetComponent<AudioSource>().PlayOneShot(WindowOpen);
        else
            this.GetComponent<AudioSource>().PlayOneShot(WindowClose);
    }

    public void SetPositionText(bool appear)
    {
        TextMeshProUGUI[] outliners = TextPosition.transform.parent.Find("TextOutlinerRank").GetComponentsInChildren<TextMeshProUGUI>();
        TextPosition.enabled = appear;
        for (int i=0;i<outliners.Length;i++)
        {
            outliners[i].enabled = appear;
        }
    }
}

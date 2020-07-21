using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FullTutorialScript : MonoBehaviour
{
    public int fullTutorialPage = -1;
    public GameObject TutorialObject;
    public GameObject AskIfToTutorial;
    public Sprite[] TutorialImages;
    public Image DisplayImage;
    public Image DisplayImageShadow;
    public TextMeshProUGUI DisplayText;
    public GameObject LeftArrow;
    public GameObject RightArrow;
    public TextMeshProUGUI TextPageCounter;
    public Button SkipToEndButton;
    public GameObject Logo;

    private Dictionary<int, Sprite> pageSprites = new Dictionary<int, Sprite>();
    private Dictionary<int, string> pageText = new Dictionary<int, string>();

    // Script is responsible for setting up and managing the full tutorial
    void Start()
    {
        SetupTutorialPages();
    }

    //Function defines the different texts and images to show in each tutorial page.
    private void SetupTutorialPages()
    {
        SetupTutorialPage(0, 0, "In Meme Masters, you compete with others online to make the funniest memes");

        SetupTutorialPage(1, 1, "The first to win 3 rounds - wins the match");

        SetupTutorialPage(2, 2, "You will be able to see the players who are winning in the top bar");

        SetupTutorialPage(3, 3, "You can click on their icon for their full nickname");

        SetupTutorialPage(4, 4, "Every round, you'll be given a meme image and you'll need to write a caption for it");

        SetupTutorialPage(5, 5, "Press the 'Lock In' button to submit your answer");

        SetupTutorialPage(6, 6, "After that, you will see the memes others created by swiping left and right");

        SetupTutorialPage(7, 7, "Vote for the meme you liked the most by tapping on it");

        SetupTutorialPage(8, 8, "After everyone voted, you will see who won the round and who voted for who");

        SetupTutorialPage(9, 9, "Before the next round starts, you'll be given a chance to vote for the next round's image");

        SetupTutorialPage(10, 0, "Moving on…");

        SetupTutorialPage(11, 10, "This is the main menu");

        SetupTutorialPage(12, 11, "Tap 'Quick Play' to join/create a public match");

        SetupTutorialPage(13, 12, "Tap 'Private Match' to play with friends");

        SetupTutorialPage(14, 13, "Tap 'Shop' to change your nickname and purchase titles");

        SetupTutorialPage(15, 14, "In the shop, you can scroll through the titles by swiping up and down");

        SetupTutorialPage(16, 15, "If you change the sounds settings by tapping the 'Settings' button");

        SetupTutorialPage(17, 16, "Tap 'How To Play' if you ever want to see the tutorial again");
    }

    private void SetupTutorialPage(int pageIndex, int imageIndex, string textString)
    {
        pageSprites.Add(pageIndex, TutorialImages[imageIndex]);
        pageText.Add(pageIndex, textString);
    }

    //Function is responsible for turning pages in the tutorial, and enabling/disabling buttons accordingly
    public void TurnPage(int pageNumber)
    {
        DisplayText.text = pageText[pageNumber];
        DisplayImage.sprite = pageSprites[pageNumber];
        DisplayImageShadow.sprite = pageSprites[pageNumber];
        TextPageCounter.text = (pageNumber + 1).ToString() + "/" + pageSprites.Count.ToString();
        if (pageNumber > 0)
            LeftArrow.SetActive(true);
        else
            LeftArrow.SetActive(false);
        if (pageNumber == pageSprites.Count - 1)//If player is on final page of tutorial, allow him to start playing the game
        {
            SkipToEndButton.GetComponent<Image>().color = Color.green;
            SkipToEndButton.GetComponentInChildren<TextMeshProUGUI>().text = "Play";
            RightArrow.SetActive(false);
        }
        else
        {
            SkipToEndButton.GetComponent<Image>().color = new Color(243f / 255f, 255f / 255f, 148f / 255f);
            SkipToEndButton.GetComponentInChildren<TextMeshProUGUI>().text = "Skip Tutorial";
            RightArrow.SetActive(true);
        }

    }

    public void TutorialYes()//Button shows the tutorial
    {
        AskIfToTutorial.SetActive(false);
        TutorialObject.SetActive(true);
        fullTutorialPage = 0;
        TurnPage(fullTutorialPage);
    }

    public void TutorialNo()//Button skips the tutorial or exits it, depending on context
    {
        if (fullTutorialPage == pageSprites.Count - 1)
        {
            this.GetComponent<AudioSource>().Play();
            StartCoroutine(ClosePanelTransition());
        }
        else
        {
            AskIfToTutorial.SetActive(false);
            TutorialObject.SetActive(true);
            fullTutorialPage = pageSprites.Count - 1;
            TurnPage(fullTutorialPage);
        }
    }

    IEnumerator ClosePanelTransition()//Handles graceful animation of object before disabling it
    {
        Logo.GetComponent<Animator>().Play("GameTitleSlideIn", 0, 0f);
        this.GetComponent<Animator>().enabled = true;
        this.GetComponent<Animator>().SetBool("WindowOpen", false);
        yield return new WaitForSeconds(Time.deltaTime);
        while (this.GetComponent<RectTransform>().localScale.x != 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            //Debug.Log("Still waiting for animation to end!");
        }
        gameObject.SetActive(false);
    }

    public void PressRight()
    {
        fullTutorialPage += 1;
        TurnPage(fullTutorialPage);
    }

    public void PressLeft()
    {
        fullTutorialPage -= 1;
        TurnPage(fullTutorialPage);
    }
}

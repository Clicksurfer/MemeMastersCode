using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSettingsButton : MonoBehaviour {

    public string TitleText;
    public int TitlePrice;
    public bool Owned;
    public bool Equipped;
    public GameObject BuyTitlePanel;
    public TextMeshProUGUI BuyTitleText;
    public AudioClip WindowClosing;

    //Script is used to handle the basic titpe shop in the player settings window

    //When button is pressed, either show a relevant purchase window
    //or equip the item according to item status
    public void SettingsButtonPress()
    {
        if (Owned && Equipped)
        {
            ChangeEquippedTitle();
        }
        else if (Owned && Equipped == false)
        {
            ChangeEquippedTitle();
        }
        else if (Owned == false)
        {
            if (PlayerPrefs.GetInt("MM_Money") >= TitlePrice)
                CreatePurchasePrompt();
        }
    }

    private void ChangeEquippedTitle()
    {
        string wasEquipped = this.GetComponent<ScrollRectContentSnap>().EquippedTitleString;
        PlayerPrefs.SetInt("MM_" + wasEquipped, 0);
        PlayerPrefs.SetInt("MM_" + TitleText, 1);
        this.GetComponent<ScrollRectContentSnap>().EquippedTitleString = TitleText;
        this.GetComponent<ScrollRectContentSnap>().OnEnable();
    }

    private void CreatePurchasePrompt()
    {
        BuyTitlePanel.SetActive(true);
        BuyTitleText.text = "Are you sure you want to buy <b>'" + TitleText + "'</b> for <b>" + TitlePrice + "</b>$?";
        this.GetComponent<AudioSource>().Play();
    }

    public void PanelBuyTitleYes()
    {
        PurchaseItem();
    }

    public void PanelBuyTitleNo()
    {
        StartCoroutine(ClosePanelTransition());
    }

    public void PurchaseItem ()
    {
        PlayerPrefs.SetInt("MM_Money", (PlayerPrefs.GetInt("MM_Money") - TitlePrice));
        ChangeEquippedTitle();
        StartCoroutine(ClosePanelTransition());
    }

    IEnumerator ClosePanelTransition()
    {
        this.GetComponent<AudioSource>().PlayOneShot(WindowClosing);
        BuyTitlePanel.GetComponentInChildren<Animator>().SetBool("WindowOpen", false);
        yield return new WaitForSeconds(Time.deltaTime);
        while (BuyTitlePanel.GetComponentInChildren<Animator>().GetComponent<RectTransform>().localScale.x != 0)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            Debug.Log("Still waiting for animation to end!");
        }
        BuyTitlePanel.SetActive(false);
    }
}

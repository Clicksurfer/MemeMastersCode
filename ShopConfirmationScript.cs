using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ShopConfirmationScript : MonoBehaviour
{
    public TextMeshProUGUI QuestionText;
    public GameObject PreviewBGColor;
    public GameObject PreviewPlayerTitle;
    public GameObject PreviewMemePackage;
    public TextMeshProUGUI PlayerLetterText;
    public TextMeshProUGUI PlayerNameText;
    public Image BGColor;
    public GridLayoutGroup MemePackageParentObj;

    private ShopItemScript myItemScript;
    private ShopItem MyItem;
    

    //Script is used by shop confirmation panel


    //Function is called when window loads
    public void SetupPanel(ShopItemScript itemScript)
    {
        myItemScript = itemScript;
        MyItem = itemScript.MyItem;
        SetupPreview();
        QuestionText.text = "Are you sure you want to buy "+ MyItem.itemName + " for " + MyItem.itemPrice + "$?";
    }

    //This function sets up the preview pane of the item in question
    private void SetupPreview()
    {
        switch (MyItem.itemType)
        {
            case SubShop.BGColors:
                PreviewBGColor.SetActive(true);
                PreviewPlayerTitle.SetActive(false);
                PreviewMemePackage.SetActive(false);
                Color bgColor;
                if (ColorUtility.TryParseHtmlString(MyItem.itemValue , out bgColor))
                    BGColor.color = bgColor;
                break;
            case SubShop.PlayerTitle:
                PreviewBGColor.SetActive(false);
                PreviewPlayerTitle.SetActive(true);
                PreviewMemePackage.SetActive(false);
                PlayerNameText.text = PlayerPrefs.GetString("MM_Username", "Default_Name") + " " + MyItem.itemValue.Replace("-Empty-","");
                PlayerLetterText.text = PlayerPrefs.GetString("MM_Username", "Default_Name")[0].ToString();
                break;
            case SubShop.MemePackage:
                PreviewBGColor.SetActive(false);
                PreviewPlayerTitle.SetActive(false);
                PreviewMemePackage.SetActive(true);
                foreach (Transform child in MemePackageParentObj.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                Sprite[] memeSprites = GameObject.Find("MemeLoader").GetComponent<MemeLoaderScript>().GetAvailableMemes(MyItem.itemValue);
                int objectCount = 0;
                foreach (Sprite memeSprite in memeSprites)
                {
                    GameObject newMeme = new GameObject();
                    Image newMemeImage = newMeme.AddComponent(typeof(Image)) as Image;
                    newMeme.transform.SetParent(MemePackageParentObj.transform);
                    newMemeImage.sprite = memeSprite;
                    newMemeImage.preserveAspect = true;
                    objectCount++;
                }
                MemePackageParentObj.GetComponent<RectTransform>().sizeDelta = new Vector2(MemePackageParentObj.GetComponent<RectTransform>().sizeDelta.x, (MemePackageParentObj.cellSize.y + MemePackageParentObj.spacing.y) * Mathf.Clamp(Mathf.CeilToInt((float)objectCount / 3.0f), 1, objectCount));
                MemePackageParentObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(MemePackageParentObj.GetComponent<RectTransform>().anchoredPosition.x, 0);
                break;
        }
    }

    public void ButtonBuy()
    {
        PlayerPrefs.SetInt("MM_Money", (PlayerPrefs.GetInt("MM_Money") - MyItem.itemPrice));
        myItemScript.Owned = true;
        myItemScript.ButtonPress();
        ButtonBuyNo();
    }

    public void ButtonBuyNo()
    {
        gameObject.GetComponentInChildren<PanelMenuButtonsNoNetwork>().ToggleUIObject();
        gameObject.GetComponentInChildren<PanelMenuButtonsNoNetwork>().GetComponent<AudioSource>().Play();
    }
}

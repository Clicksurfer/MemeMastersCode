using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemScript : MonoBehaviour
{
    public ShopItem MyItem;
    public Image BannerOnButton;
    public Image CircleOnButton;
    public bool Owned;

    private TextMeshProUGUI BannerOnButtonText;
    private TextMeshProUGUI ButtonText;
    private TextMeshProUGUI CircleOnButtonText;
    private Image ButtonBG;
    private Color DefaultBGColor = Color.gray;
    private Color BuyColor = new Color(248f / 255f, 104f / 255f, 104f / 255f);
    private Color EquippedColor = new Color(150f / 255f, 255f / 255f, 129f / 255f);
    private Color EquipColor = new Color(255 / 255f, 231 / 255f, 129f / 255f);
    private ShopConfirmationScript ConfirmationPanel;
    private ShopManager myManager;

    //Script is attached to button in shop representing an item to be purchased/equipped
    //It contains all the item's data, as well as functionality to alter the button/item's status

    //Sets up local vars upon boot
    private void Awake()
    {
        ButtonBG = this.GetComponent<Image>();
        ButtonText = this.GetComponentInChildren<TextMeshProUGUI>();
        BannerOnButtonText = BannerOnButton.GetComponentInChildren<TextMeshProUGUI>();
        CircleOnButtonText = CircleOnButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ButtonPress()
    {
        if (Owned)// Check if item owned
        {
            if (MyItem.itemType != SubShop.MemePackage)
            {
                myManager.SetEquipped(gameObject);
                EquipLocally();
            }
            else
            {
                //Check if item is equipped. If not, equip it locally.
                if (!IsEquippedItem())
                    EquipLocally();
                else
                {
                    //If it is eqquiped, do the following:
                    //Ask server to check if at least one other button is equipped
                    if (myManager.CheckIfAtLeastOneIsEquipped(gameObject))
                        Unequip();//If so, allow to unequip.
                }
            }
        }
        else
        {
            ConfirmationPanel.gameObject.SetActive(true);
            ConfirmationPanel.SetupPanel(this);
        }
    }

    //Equips the item & updates UI without affecting other buttons
    public void EquipLocally()
    {
        PlayerPrefs.SetInt("MM_" + MyItem.itemName, 1);
        switch (MyItem.itemType)
        {
            case SubShop.BGColors:
                PlayerPrefs.SetString("MM_BGColor", MyItem.itemValue);
                break;
            case SubShop.PlayerTitle:
                PlayerPrefs.SetString("MM_PlayerTitle", MyItem.itemValue.Replace("-Empty-",""));
                break;
            case SubShop.MemePackage:
                PlayerPrefs.SetInt(MyItem.itemValue, 1);
                break;
        }
        ConfigureButtonOverlay("Equipped", EquippedColor, false);
    }

    public bool IsEquippedItem()
    {
        if (!Owned)
            return false;
        else if (PlayerPrefs.GetInt("MM_" + MyItem.itemName) == 1)
            return true;

        return false;
    }

    public void Unequip()
    {
        if (Owned)
        {
            PlayerPrefs.SetInt("MM_" + MyItem.itemName, 0);
            ConfigureButtonOverlay("Equip", EquipColor, false);
        }
    }

    public void SetButtonProperties(ShopItem item, ShopConfirmationScript spt, ShopManager mngr)
    {
        ConfirmationPanel = spt;
        myManager = mngr;
        MyItem = item;
        ButtonText.text = MyItem.itemName;
        if (MyItem.itemType == SubShop.BGColors)
        {
            Color inputColor;
            if (ColorUtility.TryParseHtmlString(MyItem.itemValue, out inputColor))
                ButtonBG.color = inputColor;
        }
        else
            ButtonBG.color = DefaultBGColor;
        SetButtonTextColorBasedOnBGColor();
        Owned = (PlayerPrefs.HasKey("MM_" + MyItem.itemName) != false);
        this.GetComponent<Button>().interactable = (PlayerPrefs.GetInt("MM_Money", 0)-MyItem.itemPrice >=0);
        SetButtonOverlay();
    }

    private void SetButtonTextColorBasedOnBGColor()
    {
        float bgColorSum = ButtonBG.color.r * 255f + ButtonBG.color.g * 255f + ButtonBG.color.b * 255f;
        if (bgColorSum < 255f)
            ButtonText.color = new Color(1, 1, 1, ButtonText.color.a);
        else
            ButtonText.color = new Color(0, 0, 0, ButtonText.color.a);
    }

    public void SetButtonOverlay()
    {
        if (!Owned)
            ConfigureButtonOverlay(MyItem.itemPrice + "$", BuyColor, true);
        else if (PlayerPrefs.GetInt("MM_" + MyItem.itemName) == 1)
            ConfigureButtonOverlay("Equipped", EquippedColor, false);
        else if (PlayerPrefs.GetInt("MM_" + MyItem.itemName) == 0)
            ConfigureButtonOverlay("Equip", EquipColor, false);
    }

    public void ConfigureButtonOverlay(string TextToShow, Color ButtonColor, bool EnabledCircle)
    {
        BannerOnButton.color = ButtonColor;
        if (!EnabledCircle)
            BannerOnButtonText.text = TextToShow;
        else
            CircleOnButtonText.text = TextToShow;
        CircleOnButton.gameObject.SetActive(EnabledCircle);
        BannerOnButton.gameObject.SetActive(!EnabledCircle);
    }
}

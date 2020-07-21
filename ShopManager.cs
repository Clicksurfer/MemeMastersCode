using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum SubShop { BGColors, PlayerTitle, MemePackage }

public class ShopManager : MonoBehaviour
{
    public GameObject ShopItemButtonPrefab;
    public TextMeshProUGUI ShopPanelTitle;
    public TextMeshProUGUI ShopPanelSubtext;
    public GridLayoutGroup ShopItemsGrid;
    public ShopConfirmationScript ConfirmationPanel;

    private List<ShopItem> shopItems;

    //Script is used to manage the shop panel
    //It handles loading and handling all the shop items

    //Upon boot, prepare the data of items to be sold
    private void Awake()
    {
        DefineShopCatalog();
    }

    //Function loads up a shop subwindow, with all the items of a certain type
    public void OpenSubwindow(string subShopText)
    {
        SubShop subShop;
        if (subShopText.Equals(SubShop.BGColors.ToString()))
            subShop = SubShop.BGColors;
        else if (subShopText.Equals(SubShop.PlayerTitle.ToString()))
            subShop = SubShop.PlayerTitle;
        else if (subShopText.Equals(SubShop.MemePackage.ToString()))
            subShop = SubShop.MemePackage;
        else
            subShop = SubShop.PlayerTitle;

        //Clear children
        int itemsCount = 0;
        foreach (Transform child in ShopItemsGrid.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        ShopItemScript DefaultItem = null;
        ShopItemScript EquippedItem = null;

        //Begin instantiating display items into panel
        for (int i = 0; i < shopItems.Count; i++)
            if (shopItems[i].itemType == subShop)
            {
                itemsCount++;
                GameObject newButton = Instantiate(ShopItemButtonPrefab, ShopItemsGrid.transform);
                newButton.GetComponent<ShopItemScript>().SetButtonProperties(shopItems[i], ConfirmationPanel, this);
                if (newButton.GetComponent<ShopItemScript>().IsEquippedItem())
                    EquippedItem = newButton.GetComponent<ShopItemScript>();

                if (itemsCount == 1)
                    DefaultItem = newButton.GetComponent<ShopItemScript>();
            }

        //If no item is equipped, equip the first item which is supposed to be equipped by default
        if (EquippedItem == null)
        {
            if (DefaultItem != null)
            {
                DefaultItem.Owned = true;
                DefaultItem.EquipLocally();
            }
        }

        //Alter UI elements accordingly
        ShopItemsGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(ShopItemsGrid.GetComponent<RectTransform>().sizeDelta.x, (ShopItemsGrid.cellSize.y + ShopItemsGrid.spacing.y) * Mathf.Clamp(Mathf.CeilToInt((float)itemsCount / 3.0f), 1, itemsCount));
        ShopItemsGrid.GetComponent<RectTransform>().anchoredPosition = new Vector2(ShopItemsGrid.GetComponent<RectTransform>().anchoredPosition.x, 0);

        //Edit displayed text
        switch (subShop)
        {
            case SubShop.BGColors:
                ShopPanelTitle.text = "Background colors";
                ShopPanelSubtext.text = "Pick a background color for matches";
                break;
            case SubShop.PlayerTitle:
                ShopPanelTitle.text = "Player titles";
                ShopPanelSubtext.text = "Pick a player title";
                break;
            case SubShop.MemePackage:
                ShopPanelTitle.text = "Meme packages";
                ShopPanelSubtext.text = "Pick a meme package to enhance your game!";
                break;
        }
    }

    //Function receives item that was just equipped, and unequips all other items for UI
    public void SetEquipped(GameObject NewEquippedItem)
    {
        foreach (Transform child in ShopItemsGrid.transform)
        {
            if (NewEquippedItem != child.gameObject)
                child.GetComponent<ShopItemScript>().Unequip();
        }
    }

    public bool CheckIfAtLeastOneIsEquipped(GameObject clickedItem)
    {
        foreach (Transform child in ShopItemsGrid.transform)
        {
            if (clickedItem != child.gameObject)
            {
                bool atLeastOneEquipped = child.GetComponent<ShopItemScript>().IsEquippedItem();
                if (atLeastOneEquipped)
                    return true;
            }
        }
        return false;
    }

    public void DefineShopCatalog()
    {
        shopItems = new List<ShopItem>();
        shopItems.AddRange(ShopCatalogPlayerTitles());
        shopItems.AddRange(ShopCatalogBGColors());
        shopItems.AddRange(ShopCatalogMemePackages());
    }

    public List<ShopItem> ShopCatalogBGColors()
    {
        List<ShopItem> retList = new List<ShopItem>();
        retList.Add(new ShopItem("Default", "#434343", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Pure Black", "#000000", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Absolute White", "#FFFFFF", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Boring Beige", "#EFCB74", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Brown", "#74451B", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Rich Red", "#a41d1d", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Blue", "#3439A8", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Green", "#55CF48", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Pure Yellow", "#FFF500", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Cyan", "#44A7C6", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Orange", "#FF8500", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Teal", "#04EA8B", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Dark Green", "#2E7627", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Deep Purple", "#8600A6", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Pink Purple", "#9D255B", 20, SubShop.BGColors));
        retList.Add(new ShopItem("Peppy Pink", "#FF4CBB", 20, SubShop.BGColors));
        return retList;
    }

    public List<ShopItem> ShopCatalogMemePackages()
    {
        List<ShopItem> retList = new List<ShopItem>();
        retList.Add(new ShopItem("Base Package", "MM_Pack_base", 0, SubShop.MemePackage));
        retList.Add(new ShopItem("Rage Comics Package", "MM_Pack_ragecomics", 50, SubShop.MemePackage));
        return retList;
    }

    public List<ShopItem> ShopCatalogPlayerTitles()
    {
        List<ShopItem> retList = new List<ShopItem>();
        retList.Add(new ShopItem("-Empty-", "-Empty-", 0, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Unimportant", "The Unimportant", 0, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Humorous", "The Humorous", 0, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Peasant", "The Peasant", 0, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Pleb", "The Pleb", 0, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Beginner", "The Beginner", 0, SubShop.PlayerTitle));

        retList.Add(new ShopItem("The Journeyman", "The Journeyman", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Journeywoman", "The Journeywoman", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Funny", "The Funny", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Entertainer", "The Entertainer", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Adventurer", "The Adventurer", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Hero", "The Hero", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Giggly", "The Giggly", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Great", "The Great", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Terrible", "The Terrible", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Just", "The Just", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Kind", "The Kind", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Fair", "The Fair", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Wise", "The Wise", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Unready", "The Unready", 5, SubShop.PlayerTitle));
        retList.Add(new ShopItem("The Okay", "The Okay", 5, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#54FF00>The Apple</color>", "<color=#54FF00>The Apple</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FFAF00>The Orange</color>", "<color=#FFAF00>The Orange</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#4FDB10>The Pear</color>", "<color=#4FDB10>The Pear</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#EBFF00>The Banana</color>", "<color=#EBFF00>The Banana</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#69EA45>The Kiwi</color>", "<color=#69EA45>The Kiwi</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#C8BE3E>The Potato</color>", "<color=#C8BE3E>The Potato</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FFB400>The Carrot</color>", "<color=#FFB400>The Carrot</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FFE300>The Pineapple</color>", "<color=#FFE300>The Pineapple</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F8E863>The Sandwich</color>", "<color=#F8E863>The Sandwich</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#888780>The Knight</color>", "<color=#888780>The Knight</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#30DF44>The Archer</color>", "<color=#30DF44>The Archer</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#BA3EF9>The Wizard</color>", "<color=#BA3EF9>The Wizard</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#FF0000>The Scout</color>", "<color=#FF0000>The Scout</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Soldier</color>", "<color=#FF0000>The Soldier</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Pyro</color>", "<color=#FF0000>The Pyro</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Heavy</color>", "<color=#FF0000>The Heavy</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Demoman</color>", "<color=#FF0000>The Demoman</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Engineer</color>", "<color=#FF0000>The Engineer</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Medic</color>", "<color=#FF0000>The Medic</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Sniper</color>", "<color=#FF0000>The Sniper</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>The Spy</color>", "<color=#FF0000>The Spy</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#8B8B8B>The Unseen</color>", "<color=#8B8B8B>The Unseen</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#727272>The Unfunny</color>", "<color=#727272>The Unfunny</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF00D0>The Undressed</color>", "<color=#FF00D0>The Undressed</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#FF0000>The Red</color>", "<color=#FF0000>The Red</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#0002FF>The Blue</color>", "<color=#0002FF>The Blue</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F5FF00>The Yellow</color>", "<color=#F5FF00>The Yellow</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#0DFF00>The Green</color>", "<color=#0DFF00>The Green</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#000000>The Black</color>", "<color=#000000>The Black</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FFFFFF>The White</color>", "<color=#FFFFFF>The White</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#F4C726>The Good</color>", "<color=#F4C726>The Good</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F4C726>The Bad</color>", "<color=#F4C726>The Bad</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F4C726>The Ugly</color>", "<color=#F4C726>The Ugly</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#7D7A71>The Seaslug</color>", "<color=#7D7A71>The Seaslug</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#959285>The Elephant</color>", "<color=#959285>The Elephant</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F7EB0F>The Giraffe</color>", "<color=#F7EB0F>The Giraffe</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#6E6D65>The Rhino-saurus</color>", "<color=#6E6D65>The Rhino-saurus</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#E8BE4F>The Monkey</color>", "<color=#E8BE4F>The Monkey</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#D0A22C>The Doge</color>", "<color=#D0A22C>The Doge</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#DEDEDE>The Grumpy Cat</color>", "<color=#DEDEDE>The Grumpy Cat</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F8F8F8>The Smug Cat</color>", "<color=#F8F8F8>The Smug Cat</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#B272EE>The Parrot</color>", "<color=#B272EE>The Parrot</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#893C91>The Squid</color>", "<color=#893C91>The Squid</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#7F5F9F>The Pufferfish</color>", "<color=#7F5F9F>The Pufferfish</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#A57F25>The Wombat</color>", "<color=#A57F25>The Wombat</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#9F9F9F>The Koala</color>", "<color=#9F9F9F>The Koala</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#005F9E>The Whale</color>", "<color=#005F9E>The Whale</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#6A6A6A>The Fly</color>", "<color=#6A6A6A>The Fly</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#E0A82C>The Dingo</color>", "<color=#E0A82C>The Dingo</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#0057FF>Goes To The Polls</color>", "<color=#0057FF>Goes To The Polls</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF0000>Will Not Divide Us</color>", "<color=#FF0000>Will Not Divide Us</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF005F>, But Better</color>", "<color=#FF005F>, But Better</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#00FF06>, Harambe's Preacher</color>", "<color=#00FF06>, Harambe's Preacher</color>", 10, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#3A3A3A>Is A Cool Cat</color>", "<color=#3A3A3A>Is A Cool Cat</color>", 10, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#F5FF00>The King</color>", "<color=#F5FF00>The King</color>", 50, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#460084>The Master</color>", "<color=#460084>The Master</color>", 50, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F5FF00>The Rich</color>", "<color=#F5FF00>The Rich</color>", 50, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#FF9300>The Trump</color>", "<color=#FF9300>The Trump</color>", 50, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#00FFE0>The Lord</color>", "<color=#00FFE0>The Lord</color>", 50, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#00FFE0>The Unbeatable</color>", "<color=#00FFE0>The Unbeatable</color>", 60, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#F5FF00>McDuck</color>", "<color=#F5FF00>McDuck</color>", 60, SubShop.PlayerTitle));
        retList.Add(new ShopItem("<color=#00FF06>The Rare Pepe</color>", "<color=#00FF06>The Rare Pepe</color>", 70, SubShop.PlayerTitle));

        retList.Add(new ShopItem("<color=#F7FF35>The Meme Master</color>", "<color=#F7FF35>The Meme Master</color>", 100, SubShop.PlayerTitle));
        return retList;
    }
}

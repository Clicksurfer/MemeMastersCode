using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemClass : MonoBehaviour
{

}

//Class is used to define an item in the shop
[System.Serializable]
public class ShopItem
{
    public string itemName { get; set; }
    public string itemValue { get; set; }
    public int itemPrice { get; set; }
    public SubShop itemType { get; set; }

    public ShopItem (string itemName, string itemValue, int itemPrice, SubShop itemType)
    {
        this.itemName = itemName;
        this.itemValue = itemValue;
        this.itemPrice = itemPrice;
        this.itemType = itemType;
    }
}

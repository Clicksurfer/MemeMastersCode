using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollRectContentSnap : MonoBehaviour
{
    public GameObject TitleGameobjectPrefab;
    public GameObject myButton;
    public float TitleHeight = 133.333333333333333333f;
    public string EquippedTitleString = "";
    public int indexOfEquipped = -1;

    private Dictionary<string, int> titles = new Dictionary<string, int>();

    //This script does two things:
    //Snap the content of a vertical scrollrect to the nearest object in the content
    //Define  & setup the objects in the content, the titles

	void Start ()
    {
        CreateTitles();
        SetupTitles();
	}

    public void OnEnable()
    {
        CreateTitles();
        SetupTitles();
        PositionForEquipped();
    }

    //Method resets the position of the scrollrect to show the currently equipped item
    private void PositionForEquipped()
    {
        float setPos = 0f;
        this.GetComponent<RectTransform>().localPosition = new Vector3(this.GetComponent<RectTransform>().localPosition.x, (setPos + indexOfEquipped * TitleHeight), this.GetComponent<RectTransform>().localPosition.z);

    }

    // Update is used to snap to nearest object in content when released
    void Update ()
    {
	    if (Input.touchCount == 0)
        {
            float newYPos = FindClosestEntryPosition(this.GetComponent<RectTransform>().localPosition.y + TitleHeight/2f) - TitleHeight / 2f;
            if (newYPos != -1f)
                this.GetComponent<RectTransform>().localPosition = new Vector3(this.GetComponent<RectTransform>().localPosition.x, newYPos , this.GetComponent<RectTransform>().localPosition.z);
        }
        GameObject currentTitleObj = FindTitleObjectThroughPosition(this.GetComponent<RectTransform>().localPosition.y + TitleHeight / 2f);
        ChangeButtonBasedOnTitle(currentTitleObj);
    }

    private float FindClosestEntryPosition (float currentPos)
    {
        //Minimal position (Entry 0) - half of TitleHeight POSITIVE
        float underNumber = TitleHeight / 2f;
        float overNumber = -1f;
        float maxValue = TitleHeight * GetAllEntries() + TitleHeight / 2f;
        while (overNumber == -1f && underNumber < maxValue)
        {
            if (underNumber + TitleHeight > currentPos)
                overNumber = underNumber + TitleHeight;
            else
                underNumber = underNumber + TitleHeight;
        }
        if (currentPos - underNumber < overNumber - currentPos || overNumber < 0)
            return underNumber;
        else
            return overNumber;
    }

    private int GetAllEntries()
    {
        int i = 0;
        foreach (Transform child in transform)
        {
            i++;
        }
        return (i);
    }

    private GameObject FindTitleObjectThroughPosition(float currentPos)
    {
        //Minimal position (Entry 0) - half of TitleHeight POSITIVE
        float yNumber = TitleHeight / 2f;
        int index = 0;
        float maxValue = TitleHeight * GetAllEntries() + TitleHeight / 2f;
        GameObject a = this.gameObject;
        while (yNumber < maxValue)
        {
            if (Mathf.Abs(yNumber - currentPos) < 0.1f)
            {
                int childrenIndex = 0;
                foreach (Transform child in transform)
                {
                    if (index == childrenIndex)
                    {
                        return child.gameObject;
                    }
                    else
                        childrenIndex++;
                    a = child.gameObject;
                }
                index++;
            }
            else
            {
                index++;
            }
            yNumber += TitleHeight;
        }
        return a;
    }

    //This method changes the content of the button depending on the status of the object shown in the scrollrect
    private void ChangeButtonBasedOnTitle(GameObject titleObject)
    {
        foreach (KeyValuePair<string, int> title in titles)
        {
            if (title.Key == titleObject.GetComponent<TextMeshProUGUI>().text)
            {
                if (PlayerPrefs.HasKey("MM_" + title.Key) == false)
                {
                    myButton.GetComponentInChildren<TextMeshProUGUI>().text = "Buy for "+ title.Value.ToString() +"$";
                    myButton.GetComponent<Image>().color = new Color(248f/255f, 104f/255f, 104f/255f);
                    this.GetComponent<PlayerSettingsButton>().Owned = false;
                    this.GetComponent<PlayerSettingsButton>().Equipped = false;
                    myButton.GetComponent<Button>().interactable = (PlayerPrefs.GetInt("MM_Money") >= title.Value);
                }
                else if (PlayerPrefs.GetInt("MM_" + title.Key) == 1)
                {
                    PlayerPrefs.SetString("MM_PlayerTitle", title.Key);
                    if (title.Key == "-Empty-")
                        PlayerPrefs.SetString("MM_PlayerTitle", "");
                    myButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equipped";
                    myButton.GetComponent<Image>().color = new Color(150f/255f, 255f/255f, 129f/255f);
                    this.GetComponent<PlayerSettingsButton>().Owned = true;
                    this.GetComponent<PlayerSettingsButton>().Equipped = true;
                }
                else
                {
                    myButton.GetComponentInChildren<TextMeshProUGUI>().text = "Equip";
                    myButton.GetComponent<Image>().color = new Color(255/255f, 231/255f, 129f/255f);
                    this.GetComponent<PlayerSettingsButton>().Owned = true;
                    this.GetComponent<PlayerSettingsButton>().Equipped = false;
                    myButton.GetComponent<Button>().interactable = true;
                }
                this.GetComponent<PlayerSettingsButton>().TitlePrice = title.Value;
                this.GetComponent<PlayerSettingsButton>().TitleText = title.Key;
            }
        }
    }

    //Function sets up titles for purchasing/equipping
    public void SetupTitles()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, TitleHeight * titles.Count);
        int index = 0;
        foreach (KeyValuePair<string, int> title in titles)
        {
            GameObject newTitle = Instantiate(TitleGameobjectPrefab, transform);
            newTitle.GetComponent<TextMeshProUGUI>().text = title.Key;

            if (PlayerPrefs.HasKey("MM_" + title.Key))
            {
                if (PlayerPrefs.GetInt("MM_" + title.Key) == 1)
                {
                    EquippedTitleString = title.Key;
                    indexOfEquipped = index;
                }
            }
            else if (title.Value == 0)
            {
                PlayerPrefs.SetInt("MM_" + title.Key, 0);
            }
            index++;
        }
        if (indexOfEquipped == -1)
        {
            Debug.Log("Equipped?");
            indexOfEquipped = 0;
            EquippedTitleString = "-Empty-";
            PlayerPrefs.SetInt("MM_-Empty-", 1);
        }
    }

    private void CreateTitles()
    {
        titles.Clear();
        titles.Add("-Empty-", 0);
        titles.Add("The Unimportant", 0);
        titles.Add("The Humorous", 0);
        titles.Add("The Peasant", 0);
        titles.Add("The Pleb", 0);
        titles.Add("The Beginner", 0);

        titles.Add("The Journeyman", 5);
        titles.Add("The Journeywoman", 5);
        titles.Add("The Funny", 5);
        titles.Add("The Entertainer", 5);
        titles.Add("The Adventurer", 5);
        titles.Add("The Hero", 5);
        titles.Add("The Giggly", 5);
        titles.Add("The Great", 5);
        titles.Add("The Terrible", 5);
        titles.Add("The Just", 5);
        titles.Add("The Kind", 5);
        titles.Add("The Fair", 5);
        titles.Add("The Wise", 5);
        titles.Add("The Unready", 5);
        titles.Add("The Okay", 5);

        titles.Add("<color=#54FF00>The Apple</color>", 10);
        titles.Add("<color=#FFAF00>The Orange</color>", 10);
        titles.Add("<color=#4FDB10>The Pear</color>", 10);
        titles.Add("<color=#EBFF00>The Banana</color>", 10);
        titles.Add("<color=#69EA45>The Kiwi</color>", 10);
        titles.Add("<color=#C8BE3E>The Potato</color>", 10);
        titles.Add("<color=#FFB400>The Carrot</color>", 10);
        titles.Add("<color=#FFE300>The Pineapple</color>", 10);
        titles.Add("<color=#F8E863>The Sandwich</color>", 10);

        titles.Add("<color=#888780>The Knight</color>", 10);
        titles.Add("<color=#30DF44>The Archer</color>", 10);
        titles.Add("<color=#BA3EF9>The Wizard</color>", 10);

        titles.Add("<color=#FF0000>The Scout</color>", 10);
        titles.Add("<color=#FF0000>The Soldier</color>", 10);
        titles.Add("<color=#FF0000>The Pyro</color>", 10);
        titles.Add("<color=#FF0000>The Heavy</color>", 10);
        titles.Add("<color=#FF0000>The Demoman</color>", 10);
        titles.Add("<color=#FF0000>The Engineer</color>", 10);
        titles.Add("<color=#FF0000>The Medic</color>", 10);
        titles.Add("<color=#FF0000>The Sniper</color>", 10);
        titles.Add("<color=#FF0000>The Spy</color>", 10);

        titles.Add("<color=#8B8B8B>The Unseen</color>", 10);
        titles.Add("<color=#727272>The Unfunny</color>", 10);
        titles.Add("<color=#FF00D0>The Undressed</color>", 10);

        titles.Add("<color=#FF0000>The Red</color>", 10);
        titles.Add("<color=#0002FF>The Blue</color>", 10);
        titles.Add("<color=#F5FF00>The Yellow</color>", 10);
        titles.Add("<color=#0DFF00>The Green</color>", 10);
        titles.Add("<color=#000000>The Black</color>", 10);
        titles.Add("<color=#FFFFFF>The White</color>", 10);

        titles.Add("<color=#F4C726>The Good</color>", 10);
        titles.Add("<color=#F4C726>The Bad</color>", 10);
        titles.Add("<color=#F4C726>The Ugly</color>", 10);

        titles.Add("<color=#7D7A71>The Seaslug</color>", 10);
        titles.Add("<color=#959285>The Elephant</color>", 10);
        titles.Add("<color=#F7EB0F>The Giraffe</color>", 10);
        titles.Add("<color=#6E6D65>The Rhino-saurus</color>", 10);
        titles.Add("<color=#E8BE4F>The Monkey</color>", 10);
        titles.Add("<color=#D0A22C>The Doge</color>", 10);
        titles.Add("<color=#DEDEDE>The Grumpy Cat</color>", 10);
        titles.Add("<color=#F8F8F8>The Smug Cat</color>", 10);
        titles.Add("<color=#B272EE>The Parrot</color>", 10);
        titles.Add("<color=#893C91>The Squid</color>", 10);
        titles.Add("<color=#7F5F9F>The Pufferfish</color>", 10);
        titles.Add("<color=#A57F25>The Wombat</color>", 10);
        titles.Add("<color=#9F9F9F>The Koala</color>", 10);
        titles.Add("<color=#005F9E>The Whale</color>", 10);
        titles.Add("<color=#6A6A6A>The Fly</color>", 10);
        titles.Add("<color=#E0A82C>The Dingo</color>", 10);

        titles.Add("<color=#0057FF>Goes To The Polls</color>", 10);
        titles.Add("<color=#FF0000>Will Not Divide Us</color>", 10);
        titles.Add("<color=#FF005F>, But Better</color>", 10);
        titles.Add("<color=#00FF06>, Harambe's Preacher</color>", 10);
        titles.Add("<color=#3A3A3A>Is A Cool Cat</color>", 10);

        titles.Add("<color=#F5FF00>The King</color>", 50);
        titles.Add("<color=#460084>The Master</color>", 50);
        titles.Add("<color=#F5FF00>The Rich</color>", 50);
        titles.Add("<color=#FF9300>The Trump</color>", 50);
        titles.Add("<color=#00FFE0>The Lord</color>", 50);
        titles.Add("<color=#00FFE0>The Unbeatable</color>", 60);
        titles.Add("<color=#F5FF00>McDuck</color>", 60);
        titles.Add("<color=#00FF06>The Rare Pepe</color>", 70);

        titles.Add("<color=#F7FF35>The Meme Master</color>", 100);
    }
}


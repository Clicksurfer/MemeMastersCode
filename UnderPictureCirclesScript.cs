using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnderPictureCirclesScript : MonoBehaviour {

    public GameObject CirclePrefab;
    public GameObject MemeChoiceContent;
    public float SizeNormal = 50f;
    public float NormalScale = 1080f;
    public int CircleCount = 0;
    public Color UnselectedColor;
    public Color SelectedColor;

    private float baseSize;
    private float selectedSize;
    private float smallestSize;
    private float scaleModifier;

    //Script is responsible for showing dots symbolizing images in horizontal scroll rect
    //It manages their look, number & size depending on which item in the horizontal scroll rect is shown
	// Use this for initialization

    //Start just sets up some local vars
	void Start () 
    {
        CircleCount = transform.childCount;
        scaleModifier = Screen.width / NormalScale;
        baseSize = SizeNormal * scaleModifier;
        selectedSize = baseSize * 2;
        smallestSize = baseSize / 2;
        UnselectedColor = new Color(1, 1, 1 ,200f / 255f);
        SelectedColor = Color.white;
	}

    // Update will check for changes in scroll rect
	void Update () {
        CheckCircleCount();
        UpdateCircles();
	}

    private void CheckCircleCount()
    {
        int memeCount = MemeChoiceContent.transform.childCount;
        if (CircleCount != memeCount)
            InstantiateCircles(memeCount);
    }

    public void InstantiateCircles(int Count)
    {
        CircleCount = Count;
        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);
        for (int i = 0; i < Count; i++)
        {
            GameObject newCircle = (Instantiate(CirclePrefab, transform)) as GameObject;
            newCircle.GetComponent<RectTransform>().sizeDelta = new Vector2(baseSize, baseSize);
            newCircle.GetComponent<Image>().enabled = true;
        }
        UpdateCircles();
    }
	

    private void UpdateCircles()
    {
        int i = 0;
        float modifiedSize = Mathf.Lerp(baseSize, smallestSize, (CircleCount - 4) / 6);//Will make circles smaller only after there are more than 4 circles.
        foreach (Transform circle in transform)
        {
            float percentageOnScreen = PercentageOfImageOnScreen(i);
            float newSize = Mathf.Lerp(modifiedSize, selectedSize, percentageOnScreen);
            circle.GetComponent<RectTransform>().sizeDelta = new Vector2(newSize, newSize);
            circle.GetComponent<Image>().color = Color.Lerp(UnselectedColor, SelectedColor, percentageOnScreen);
            i++;
        }
    }

    private float PercentageOfImageOnScreen(int index)
    {
        float memeWidth = MemeChoiceContent.GetComponent<ChangeContentSize>().screenWidth;
        float contentRight = - MemeChoiceContent.GetComponent<RectTransform>().offsetMax.x;
        return (Mathf.Clamp01((memeWidth - Mathf.Abs(index * memeWidth - contentRight)) / memeWidth));
    }
}

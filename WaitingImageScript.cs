using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingImageScript : MonoBehaviour 
{

    public Sprite[] WaitingImages;

    //Script simply loads one of a few images randomly into the attached Image

	void Start () 
    {
        Image[] images = gameObject.GetComponentsInChildren<Image>();
        int imageIndex = Random.Range(0, WaitingImages.Length);
        foreach (Image img in images)
            img.sprite = WaitingImages[imageIndex];
	}
}

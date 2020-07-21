using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AspectRatioFitterUpdater : MonoBehaviour {

    private AspectRatioFitter aspectRatioFitter;
    private Image image;

//Script is attached to Gameobject with image and with parent as object
//It updates the image to equal parent image, and aspect ratio fitter according to sprite ratio
	void Start () 
    {
        image = this.GetComponent<Image>();
        aspectRatioFitter = this.GetComponent<AspectRatioFitter>();
	}
	
	void Update ()
    {
        try
        {
            image.sprite = gameObject.transform.parent.GetComponent<Image>().sprite;
        }
        catch
        {

        }
        float imageRatio = image.sprite.rect.width / image.sprite.rect.height;
        aspectRatioFitter.aspectRatio = imageRatio;
	}
}

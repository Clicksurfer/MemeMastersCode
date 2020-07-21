using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeInScript : MonoBehaviour
{

    public bool ProgressFadeAutomatically = true;
    public float FadeInDuration = 1f;
    public float FadeInDelay = 0f;
    private float timer = 0f;

    private List<Transform> subObjects = new List<Transform>();
    private List<Color> initialColors = new List<Color>();

    //This script is responsible for fading in objects in attached object and its children
    void Start()
    {
        //Add all objects and subobjects to tracked objects
        subObjects.Add(transform);
        initialColors.Add(GetColor(gameObject));
        foreach (Transform child in gameObject.GetComponentsInChildren<Transform>())
        {
            if (!child.name.Contains("NOFADE"))//In the case we want an object to not be faded, just add NOFADE to its name
            {
                subObjects.Add(child);
                initialColors.Add(GetColor(child.gameObject));
            }
        }
    }

    private Color GetColor(GameObject obj) //Generic function for getting color of an object if it has one
    {
        if (obj.GetComponent<Image>() != null)
            return obj.GetComponent<Image>().color;
        else if (obj.GetComponent<TextMeshProUGUI>() != null)
            return obj.GetComponent<TextMeshProUGUI>().color;
        else if (obj.GetComponent<Text>() != null)
            return obj.GetComponent<Text>().color;
        return (Color.clear);
    }

    private void SetColor(GameObject obj, Color clr)//Generic function for setting color of an object if it has one
    {
        if (obj.GetComponent<Image>() != null)
        {
            obj.GetComponent<Image>().color = clr;
        }
        else if (obj.GetComponent<TextMeshProUGUI>() != null)
            obj.GetComponent<TextMeshProUGUI>().color = clr;
        else if (obj.GetComponent<Text>() != null)
            obj.GetComponent<Text>().color = clr;
    }

    public void ResetFade()
    {
        Debug.Log("Resetting fade!");
        timer = 0f;
    }

    //Update is responsible for handling the fade value over time
    void Update()
    {
        if (ProgressFadeAutomatically)
        {
            if (timer < FadeInDelay + FadeInDuration)//I only want this function to do processing of any kind when there's work to be done.
            {
                SetFade((timer - FadeInDelay) / FadeInDuration);
                timer += Time.deltaTime;
            }
        }
    }

    public void SetFade(float LerpVal)//Can also be used to manually set fade of objects
    {
        for (int i = 0; i < subObjects.Count; i++)
        {
            SetColor(subObjects[i].gameObject, Color.Lerp(Color.clear, initialColors[i], LerpVal));
        }
    }
}

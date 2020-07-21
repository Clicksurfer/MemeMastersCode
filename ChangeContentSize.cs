using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeContentSize : MonoBehaviour {

    public int ChildrenCount = 0;
    public int screenWidth = 1080;
    public int screenHeight = 1920;
    public int HorizontalSpace = 40;
    public float RepositionAcceleration = 10f;

    private RectTransform rect;
    private float lastHeldRight = 0f;
    private float newTargetPos = -1f;
    private float[] posArray = new float[3] { 0f, 0f, 0f};
    private float RepositionSpeed = 0f;

	//Script is used to handle dynamic content in HorizontalLayoutGroup inside a ScrollRect
    //It changes positions accordingly, and also handles 'dragging' to item alignment when user stops dragging on the screen
	void Start () {
        HorizontalSpace = this.GetComponent<HorizontalLayoutGroup>().padding.left;
        screenWidth = Screen.width;
        rect = this.GetComponent<RectTransform>();
        ChildrenCount = transform.childCount;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2((ChildrenCount * (screenWidth - 2 * HorizontalSpace) + (ChildrenCount - 1) * 2 * HorizontalSpace)/(float)screenWidth, 1);
        SetPosXToTop();
        lastHeldRight = -1f;
    }

    //Update changes content size based on amount of objects in content
    //It also handles the dragging back to item alignment
    void Update () {
        ChildrenCount = transform.childCount;
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2((ChildrenCount * (screenWidth - 2 * HorizontalSpace) + (ChildrenCount - 1) * 2 * HorizontalSpace) / (float)screenWidth, 1);

        if (Input.touchCount == 0)
        {
            if (lastHeldRight != -1f)
            {
                if (rect.offsetMax.x != newTargetPos)
                {
                    if (newTargetPos == -1f)
                        newTargetPos = -1 * (int)screenWidth * ((int)lastHeldRight / ((int)screenWidth) + (int)lastHeldRight / (((int)screenWidth) / 2) % 2);

                    float newPos = 0f;
                    if (rect.offsetMax.x < newTargetPos)
                        newPos = Mathf.Clamp((rect.offsetMax.x + RepositionSpeed * Time.deltaTime + 0.5f * RepositionAcceleration * Time.deltaTime * Time.deltaTime), rect.offsetMax.x ,newTargetPos);
                    else
                        newPos = Mathf.Clamp((rect.offsetMax.x - RepositionSpeed * Time.deltaTime - 0.5f * RepositionAcceleration * Time.deltaTime * Time.deltaTime), newTargetPos, rect.offsetMax.x);

                    RepositionSpeed = RepositionSpeed + Time.deltaTime * RepositionAcceleration;
                    rect.offsetMax = new Vector2(newPos, rect.offsetMax.y);
                    rect.offsetMin = new Vector2(newPos, rect.offsetMin.y);
                }
                else
                {
                    lastHeldRight = -1f;
                    newTargetPos = -1f;
                }
            }
            //Debug.Log("Not being touched");
        }
        else
        {
            lastHeldRight = -rect.offsetMax.x;
            UpdatePosArray(lastHeldRight);
            RepositionSpeed = 1700f;
        }
    }

    private void UpdatePosArray(float newVal)
    {
        for (int i = 0; i < posArray.Length - 1; i++)
        {
            posArray[i] = posArray[i + 1];
        }
        posArray[posArray.Length - 1] = newVal;
    }

    private float CalculateSpeedFromPosArray()
    {
        float posDelta = posArray[posArray.Length - 1] - posArray[0];
        float timeDelta = Time.deltaTime * posArray.Length;
        float speed = posDelta / timeDelta;
        return speed;
    }

    public int GetImageSize()
    {
        return (screenWidth - HorizontalSpace * 2);
    }

    //Resets starting position of ScrollRectView
    public void SetPosXToTop()
    {
        Debug.Log("Called SetPosXToTop function");
        if (rect == null)
            rect = this.GetComponent<RectTransform>();

        rect.offsetMin = new Vector2(0, 0);
        rect.offsetMax = new Vector2(0, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorDelayer : MonoBehaviour {

    public string AnimationName;
    public float AnimationDelay = 0f;
    
    private Animator myAnimator;
    private float timer = 0f;

//Script is attached to Gameobject with animation we want to delay
	void Start () 
    {
        myAnimator = this.GetComponent<Animator>();	
	}
	
	void Update ()
    {
        if (timer < AnimationDelay)
        {
            timer += Time.deltaTime;
            if (timer > AnimationDelay)
            {
                myAnimator.Play(AnimationName, -1, 0f);
            }
        }
	}
}

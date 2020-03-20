using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterMonkeyAnimation : MonoBehaviour
{

    private Animator anim;
    private float minActionTimer = 3.0f;
    private float maxActionTimer = 12.0f;
    private float actionTimer = 0.0f;
    private float timer = 0.0f;

    // Use this for initialization
    void Start()
    {
        //First, we'll get the animator.
        anim = GetComponent<Animator>();

        //To start, make them jump.  That sounds good.
        jump();

        //Next, we'll start the timer.
        timer += Time.deltaTime;

        //Also, determine what the random time the monkey will jump is.
        setActionTimer();
    }

    /// <summary>
    /// This will simply tell the animator to set the jump trigger
    /// running the jump animation.
    /// </summary>
    public void jump()
    {
        anim.SetTrigger("trgJump");
    }

    // Update is called once per frame
    void Update()
    {
        //To start, see if the action timer has been met.
        if(timer > actionTimer)
        {
            //Yay!  Run the animation!
            jump();

            //Also, reset the timer.
            setActionTimer();

            //Rest timer to start that whole thing over.
            timer = 0.0f;
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// This will set the action timer to some number between the min and max.
    /// </summary>
    private void setActionTimer()
    {
        actionTimer = Random.Range(minActionTimer, maxActionTimer);
    }
}

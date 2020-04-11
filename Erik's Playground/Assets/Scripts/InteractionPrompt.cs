using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    SpriteRenderer promptRend;         // Image that will instruct the player on how to interact.
                                       // Can be changed to a Text component if preferable.

    Vector3 initPos;                   // initial position of Prompt
    Vector3 hiddenPos;                 // position of prompt when hidden - slightly lower/higher than initial pos.

    public float fadeDistance = -0.2f; // distance the prompt moves up or down when appearing or disappearing respectively
    public float fadeSpeed = 0.1f;     // how quickly the image fades in and out
    public float lerpSpeed = 0.5f;     // how quickly the prompt moves up or down when fading in/out

    bool showPrompt = false;           // true when prompt should be visible
    bool lerping = true;               // stops Lerp function when it becomes unecessary
    bool destroyOnHide = false;        // when true, the object was picked up and the prompt will destroy itself after fading out

    private void Start()
    {
        promptRend = GetComponent<SpriteRenderer>();
        initPos = transform.position;
        hiddenPos = transform.position - new Vector3(0, fadeDistance,0);
    }



    private void Update()
    {
        if (showPrompt)       // shown when player is near the pickup
        {
            if (lerping)
            {
                // compares current y to the target height value (the initial y position)
                // comparing squares is the equivelent of comparing distance, but avoids the expensive SquareRoot Function
                // 0.2 is used instead of 0 as the last few millimeters of a lerp take quite a while to complete
                if (Mathf.Abs( Mathf.Pow(transform.position.y, 2) - Mathf.Pow(initPos.y, 2) ) <=  0.2f)     // lerp is finished 
                {
                    lerping = false;
                }
                else        // lerp is not yet complete
                {
                    // move the prompt up/down
                    transform.position = Vector3.Lerp(transform.position, initPos, lerpSpeed * Time.deltaTime);

                    // fade the prompt's alpha in
                    if (promptRend.color.a < 1)
                    {
                        promptRend.color = new Color(promptRend.color.r, promptRend.color.g, promptRend.color.b,
                            promptRend.color.a + (fadeSpeed * Time.deltaTime));
                    }
                }
            }
        }
        else    // player not near the pickup - thus the prompt can be hidden
        {
            if (lerping)
            {
                // compares current y to the target height value (the hiddenPos y position)
                // comparing squares is the equivelent of comparing distance, but avoids the expensive SquareRoot Function
                // 0.2 is used instead of 0 as the last few millimeters of a lerp take quite a while to complete
                if (Mathf.Abs( Mathf.Pow(transform.position.y, 2) - Mathf.Pow(hiddenPos.y, 2) ) <= 0.2f)     // lerp is finished 
                {
                    lerping = false;

                    // checks if object was picked up, and if so, destroys the prompt once it's hidden
                    if (destroyOnHide)
                    {
                        Destroy(gameObject);
                    }
                }
                else        // lerp is not yet complete
                {
                    // move the prompt up/down
                    transform.position = Vector3.Lerp(transform.position, hiddenPos, lerpSpeed * Time.deltaTime);

                    // fade the prompt's alpha out
                    if (promptRend.color.a > 0)
                    {
                        promptRend.color = new Color(promptRend.color.r, promptRend.color.g, promptRend.color.b,
                            promptRend.color.a - (fadeSpeed * Time.deltaTime));
                    }
                }
            }
        }
    }

    ///<summary>
    /// Cause the Pickup's prompt - on how to interact with the item - to show itself
    ///</summary>
    // Sets up bools correctly to cause the prompt to appear
    public void ShowPrompt()
    {
        lerping = true;
        showPrompt = true;
    }


    ///<summary>
    /// Cause the Pickup's prompt - on how to interact with the item - to hide itself
    ///</summary>
    // Sets up bools correctly to cause the prompt to disappear
    public void HidePrompt()
    {
        lerping = true;
        showPrompt = false;
    }


    ///<summary>
    /// Makes prompt neatly fade away after pickup is picked up
    ///</summary>
    // Destroys pickup object
    // also destroys the prompt after it neatly fades away.
    public void ObjectPickedUp()
    {
        transform.parent = null;
        destroyOnHide = true;
    }
}

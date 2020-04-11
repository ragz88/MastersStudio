using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // all variables to do with standard movement
    [Header("Horizontal Movement")]
    public string movementAxis = "Horizontal"; 
    public float movementSpeed = 2f;              // how fast character walks
    public float deadSpeed = 0.1f;                // speed at which character stops walk animation

    CapsuleCollider2D charCollider;
    Rigidbody2D rb;

    [Header("Sprite and Animation")]
    public SpriteRenderer sprite;                 // character's sprite - potentially going to be a child
    Animator animator;
    
    [Header("Grounded Checks")]
    public bool showGroundedRays = true;          // visualises the ground check raycasts when true
    public float rayLength = 0.1f;                // length of ground check raycasts
    public LayerMask groundedMask;                // Layers the ground check raycasts should interact with
    bool grounded = false;                        // true when character on ground



    public float jumpSpeed = 10;



    // Start is called before the first frame update
    void Start()
    {
        charCollider = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float movement = 0;

        // calculate new velocity value based on inputs
        movement = Input.GetAxis(movementAxis) * movementSpeed;

        // move charracter
        rb.velocity = new Vector2(movement, rb.velocity.y);

        // check the player's speed and direction to orient them and play relevant animation
        // deadSpeed prevents a walking animation from playing when the character is moving very slowly - it would look unnatural.
        if (Mathf.Abs(/*rb.velocity.x*/ movement) >= deadSpeed * movementSpeed)
        {
            animator.SetBool("walking", true);
            if (rb.velocity.x > 0)
            {
                sprite.flipX = false;
            }
            else if (rb.velocity.x < 0)
            {
                sprite.flipX = true;
            }
        }
        else
        {
            animator.SetBool("walking", false);
        }




        // Grounded Check
        // This for loop sends a short raycast down at the player's center and at it's bottom corners, 
        // checking if there's platform below
        for (int i = -1; i < 2; i++)
        {
            RaycastHit2D rayHit = 
                Physics2D.Raycast((new Vector2((transform.position.x + (i * charCollider.bounds.extents.x)), transform.position.y) 
                - new Vector2(0, charCollider.bounds.extents.y)), -Vector2.up, rayLength, groundedMask);

            // visualise the rays being drawn in editor
            if (showGroundedRays)
            {
                Debug.DrawRay((new Vector2(transform.position.x + (i * charCollider.bounds.extents.x), transform.position.y)
                    - new Vector2(0, charCollider.bounds.extents.y)), -Vector2.up * rayLength, Color.yellow);
            }

            if (rayHit.collider != null)
            {

                if (rayHit.collider.gameObject.CompareTag("Ground"))
                {
                    grounded = true;
                    animator.SetBool("jumping", false);
                    animator.SetBool("falling", false);
                    break;
                }
                else
                {
                    grounded = false;
                    if (rb.velocity.y > 0)
                    {
                        animator.SetBool("jumping", true);
                        animator.SetBool("falling", false);
                        animator.SetBool("jumpCharging", false);
                    }
                    else if (rb.velocity.y <= 0)
                    {
                        animator.SetBool("jumping", false);
                        animator.SetBool("falling", true);
                        animator.SetBool("jumpCharging", false);
                    }
                }
            }
            else
            {
                grounded = false;
                if (rb.velocity.y > 0)
                {
                    animator.SetBool("jumping", true);
                    animator.SetBool("falling", false);
                    animator.SetBool("jumpCharging", false);
                }
                else if (rb.velocity.y <= 0)
                {
                    animator.SetBool("jumping", false);
                    animator.SetBool("falling", true);
                    animator.SetBool("jumpCharging", false);
                }
            }
        }


        if (Input.GetButtonDown("Jump") && grounded /* && !lockMovement*/)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            animator.SetBool("jumpCharging", true);
        }

    }
}

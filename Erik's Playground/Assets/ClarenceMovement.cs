using System.Collections;
using System.Collections.Generic;
using UnityEngine;
////using Cinemachine;

public class ClarenceMovement : MonoBehaviour
{
    public string movementAxis = "Horizontal";
    public float movementSpeed = 2f;
    public float deadSpeed = 0.05f;

    public float jumpSpeed = 10;

    Rigidbody2D rb;
    CapsuleCollider2D charCollider;

    public LayerMask mask;
    public float rayLength = 0.1f;

    public bool grounded = false;
    public Animator animator;
    public SpriteRenderer sprite;

    public float horizontalSpeedAdjustment = 0;

    Vector3 initPosition;

    bool losingLife = false;

    public float respawnTime = 2;
    public float blinkingTime = 2;

    float respawnTimer = 0;

    public BoxCollider2D stabilisingCollider;

    ////public SceneChange sceneChanger;
    ////public CinemachineVirtualCamera virtualCam;

    bool lifeLost = false;

    bool lockMovement = false;

    // Start is called before the first frame update
    void Start()
    {
        initPosition = transform.position;

        rb = GetComponent<Rigidbody2D>();
        charCollider = GetComponent<CapsuleCollider2D>();

        ////ClarenceGameController.instance.CheckLife();
    }

    // Update is called once per frame
    void Update()
    {
        if (!losingLife)
        {
            float movement = 0;


            // prevents movement when in text areas
            if (!lockMovement)
            {
                movement = Input.GetAxis(movementAxis) * movementSpeed + horizontalSpeedAdjustment;
            }
                

            // move charracter
            rb.velocity = new Vector2(movement, rb.velocity.y);

            // check the player's speed and direction to orient them and play relevant animation
            if (Mathf.Abs(rb.velocity.x) >= deadSpeed)
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

            for (int i = -1; i < 2; i++)
            {
                RaycastHit2D rayHit = Physics2D.Raycast((new Vector2((transform.position.x + (i * charCollider.size.x)), transform.position.y) - new Vector2(0, charCollider.size.y*2)), -Vector2.up, rayLength, mask);
                Debug.DrawRay((new Vector2(transform.position.x + (i * charCollider.size.x), transform.position.y) - new Vector2(0, charCollider.size.y*2)), -Vector2.up * rayLength, Color.red);

                if (rayHit.collider != null)
                {
                    
                    if (rayHit.collider.gameObject.tag == "Ground")
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
                        }
                        else if (rb.velocity.y <= 0)
                        {
                            animator.SetBool("jumping", false);
                            animator.SetBool("falling", true);
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
                    }
                    else if (rb.velocity.y <= 0)
                    {
                        animator.SetBool("jumping", false);
                        animator.SetBool("falling", true);
                    }
                }
            }


            if (Input.GetButtonDown("ClarenceJump") && grounded && !lockMovement)
            {
                rb.velocity = rb.velocity + new Vector2(0, jumpSpeed);
            }
        }
        /*else
        {
            if (respawnTimer > respawnTime)
            {
                animator.SetBool("jumping", false);
                animator.SetBool("falling", false);
                animator.SetBool("walking", false);
                animator.SetBool("dying", false);

                gameObject.transform.position = initPosition;

                stabilisingCollider.enabled = true;
                charCollider.enabled = true;
                
                losingLife = false;
            }
            else
            {
                respawnTimer += Time.deltaTime;
            }
        }*/
    }

    public void LoseLife()
    {

        animator.SetBool("jumping", false);
        animator.SetBool("falling", false);
        animator.SetBool("walking", false);
        animator.SetBool("dying", true);

        losingLife = true;

        //make Char intangible
        stabilisingCollider.enabled = false;
        charCollider.enabled = false;

        //respawnTimer = 0;
        rb.velocity = new Vector2(0, jumpSpeed);

        ////virtualCam.Follow = null;

        if (lifeLost == false)
        {
            //ClarenceGameController.instance.LoseLife();
            lifeLost = true;
        }

        ////if (ClarenceGameController.health <= 0)
        ////{
        ////    sceneChanger.GameOver();
        ////}
        ////else
        ////{
        ////    sceneChanger.RestartScene();
        ////}
        
    }

    public void SetLockMovement(bool setting)
    {
        lockMovement = setting;
    }
}

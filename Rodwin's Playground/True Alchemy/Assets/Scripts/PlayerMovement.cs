using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10;
    public float jumpSpeed = 10;
    public float jumpHeight = 5;
    public float rotationSpeed = 10;
    public float fallspeedMultiplier = 5;
    public float analogueInputThreshold = .3f;
    float tempHeight;
    bool isGrounded = false;
    Rigidbody rigidbody;
    SpriteRenderer spriteRendered;
    Renderer shadowMat;
    bool boostdrop = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        spriteRendered = this.GetComponent<SpriteRenderer>();
        shadowMat = this.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rigidbody.velocity.y);
        if (Input.GetAxis("Horizontal") >= analogueInputThreshold)
        {
            rigidbody.velocity = new Vector3(speed, rigidbody.velocity.y, 0);
        }
        else if (Input.GetAxis("Horizontal") <= -analogueInputThreshold)
        {
            rigidbody.velocity = new Vector3(-speed, rigidbody.velocity.y, 0);
        }
        else
        {
            rigidbody.velocity = new Vector3(0, rigidbody.velocity.y, 0);
        }

        if (Input.GetButtonDown("Jump"))
        {
            shadowMat.material.SetColor("_Color", Color.green);
            tempHeight = transform.position.y;
            isGrounded = false;
            //Debug.Log("Jumping");
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpSpeed, 0);

        }

        if ((transform.position.y - tempHeight) >= jumpHeight)
        {
            rigidbody.AddForce(0, -jumpSpeed * fallspeedMultiplier, 0);
        }

        if (rigidbody.velocity.y > 0 && !isGrounded)
        {
            //boostdrop = true;
            transform.Rotate(0, 0, -rotationSpeed * Input.GetAxis("Horizontal"));

        }


        if (isGrounded == false && (rigidbody.velocity.y <= 0) /*&& boostdrop == true*/)
        {
            transform.rotation = new Quaternion(0, 0, 0, 0);
            shadowMat.material.SetColor("_Color", Color.white);
            //Debug.Log("falling");
            //rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y * fallspeedMultiplier, 0);
            //boostdrop = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }
}

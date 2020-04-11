using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{

    public float rotationSpeed = 4;              // how fast the pickup spins
    public float hoverSpeed = 1;                 // how fast the pickup bobs up and down
    public float hoverDistance = 0.1f;           // how large said bob is

    public InteractionPrompt interactionPrompt;  // an image that will appear when the player is near a pickup
    public Transform spriteTrans;                // the pickup's sprite - which is a child object

    public GameObject pickUpParticles;           // particle effect to spawn when object picked up
    
    Vector3 initPosition;                        // stores initial position of pickup, as a reference for the hover behaviour
    bool playerNearby = false;                   // true when player within the pikcup's detection trigger

    // Start is called before the first frame update
    void Start()
    {
        initPosition = spriteTrans.position;
    }

    // Update is called once per frame
    void Update()
    {
        // pickup bobbing
        float newPosY = (hoverDistance * Mathf.Sin(Time.time * hoverSpeed)) + initPosition.y;       // Calculates a new Y position based on a simple sin graph
        spriteTrans.position = new Vector3(initPosition.x, newPosY, initPosition.z);

        //pickup spinning
        spriteTrans.Rotate(0, rotationSpeed * 0.1f, 0);

        if (playerNearby)
        {
            if (Input.GetButtonDown("PickUp"))
            {
                // Instantiate particles can be added here
                interactionPrompt.ObjectPickedUp();
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))           // checks if it's the player colliding with the pickup
        {
            interactionPrompt.ShowPrompt();
        }

        playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))           // checks if it's the player colliding with the pickup
        {
            interactionPrompt.HidePrompt();
        }

        playerNearby = false;
    }


}

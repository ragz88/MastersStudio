using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{
    public Ability ability;                      // this is a reference to the ability and it's visual characteristics to
                                                 // represent it in the inventory menu

    public float rotationSpeed = 4;              // how fast the pickup spins
    public float hoverSpeed = 1;                 // how fast the pickup bobs up and down
    public float hoverDistance = 0.1f;           // how large said bob is

    public InteractionPrompt interactionPrompt;  // an image that will appear when the player is near a pickup
    public Transform spriteTrans;                // the pickup's sprite - which is a child object

    public GameObject pickUpParticles;           // particle effect to spawn when object picked up
    
    Vector3 initPosition;                        // stores initial position of pickup, as a reference for the hover behaviour
    bool playerNearby = false;                   // true when player within the pikcup's detection trigger

    SpriteRenderer spriteRend;            // a reference to this pickup's sprite renderer. Used to edit it's colour during runtime.

    // Start is called before the first frame update
    void Start()
    {
        initPosition = spriteTrans.localPosition;                            // We use local position so the pickup still reacts to gravity with it's parent
        spriteRend = spriteTrans.gameObject.GetComponent<SpriteRenderer>();
        spriteRend.sprite = ability.icon;
        SetPickupColour();
    }

    // Update is called once per frame
    void Update()
    {
        // pickup bobbing
        float newPosY = (hoverDistance * Mathf.Sin(Time.time * hoverSpeed)) + initPosition.y;       // Calculates a new Y position based on a simple sin graph
        spriteTrans.localPosition = new Vector3(0, newPosY, initPosition.z);                        

        //pickup spinning
        spriteTrans.Rotate(0, rotationSpeed * 0.1f, 0);

        if (playerNearby)
        {
            if (Input.GetButtonDown("PickUp"))
            {
                // Instantiate particles can be added here
                interactionPrompt.ObjectPickedUp();
                InventoryManager.inventoryInstance.StoreAbility(ability);
                ability.abilityEquipped = false;                             // We need to reset this property the first time the player encounters
                                                                             // the ability, as it can persist accross sessions with scriptable objects.
                // we want the pickup to disappear, but we still want the promp to fade out neatly, so we don't destroy the whole object
                Destroy(spriteTrans.gameObject);                             // makes the pickup seem to disappear
                interactionPrompt.HidePrompt();                              // starts the prompt's neat fade-out
                Destroy(this);                                               // prevents further interation
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))           // checks if it's the player colliding with the pickup
        {
            interactionPrompt.ShowPrompt();

            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))           // checks if it's the player that's no longer colliding with the pickup
        {
            interactionPrompt.HidePrompt();

            playerNearby = false;
        }
    }

    /// <summary>
    /// Analyses the category of our ability and assigns it the relevant colour. Potentially only useful with temporary sprites.
    /// </summary>
    void SetPickupColour()
    {
        // we'll look through our array of category properties from our inventory manager to find the category that matches our current ability's
        // category. Linked to that, in the CategoryProperties class, is a colour we can use.
        for (int i = 0; i < InventoryManager.inventoryInstance.categoryPanels.Length; i ++)
        {
            if (InventoryManager.inventoryInstance.categoryPanels[i].Category == ability.abilityCategory)  // implies we've found the correct category, and so the correct properties
            {
                // we can now set our pickup's colour to something relevant to it's category
                spriteRend.color = InventoryManager.inventoryInstance.categoryPanels[i].imageColour;
                break;    // we've done what we needed to, so we exit the loop.
            }
        }
        
    }


    /// <summary>
    /// Sets the ability associated with this pickup to the one given.
    /// </summary>
    /// <param name="newAbility">The new ability to link to this pickup</param>
    public void AssignAbility(Ability newAbility)
    {
        ability = newAbility;
        spriteRend = spriteTrans.gameObject.GetComponent<SpriteRenderer>();        // Sometimes when dropping, the start function seems incomplete before we try assign the pickup colour
                                                                                   // and so we get a nullReference error. This line should combat that.
        SetPickupColour();
        spriteRend.sprite = newAbility.icon;                                       // Makes sure the pickup looks like the ability it represents
    }


}

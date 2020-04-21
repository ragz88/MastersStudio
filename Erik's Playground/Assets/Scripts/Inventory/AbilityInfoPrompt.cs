using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Controls the response of each type of prompt in the Ability Information Panel to its associated button being pushed.
/// 
/// <para>When an ability is seleceted, the player can perform various actions to it. There are multiple prompts shown
/// in the Ability Information panel that describe these actions and how to perform them.</para>
/// </summary>
public class AbilityInfoPrompt : MonoBehaviour
{
    public EquippedAbilitiesController equippedAbilitiesController;     // Reference allows us to trigger the equiping and unequiping of abilities
    public AbilityInfoDisplay abilityInfoDisplay;
    //public InventoryManager inventoryManager;

    public Image loadingBarImage;            // This is the filled Image that fills up as the player holds this prompts button down.
    public float loadingBarSpeed = 0.1f;     // This is how quickly the prompt's loading bar fills up. More dangerous actions will take more time.
    public string promptButton;              // Name of this prompt's associated button in the Input Settings.  

    public UnityEvent PromptButtonPressed;   // This is a special, customisable event we will access in inspector, 
                                             // much like the OnClick event of a button.

    bool barReturning = false;               // set to true if the player lets go of a prompt's button halfway
                                             // leads to the prompt bar emptying itsel again.

    bool waitForFingerLift = false;          // We don't want the player's holding of a button for a previous action to affect any new buttons on screen.
                                             // When this bool is true, the script will wait for the player to lift their finger before accepting their input
                                             // from that button again.

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        // We check if the player was holding this prompt's button in the exact moment it appeared on screen.
        // If so, they were probably holding that button for a previous action, so we'll wait for them to lift their finger.
        // PromptButtonStillHeld();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton(promptButton) && !waitForFingerLift)  // The button associated with the prompt is being purposefully pressed by the player...
        {
            barReturning = false;                 // bar no longer needs to be emptying itself
            
            // ... so we start filling our loading bar. 
            if (loadingBarImage.fillAmount >= 1)  // first, we check if the bar is already full
            {
                loadingBarImage.fillAmount = 0;   // In which case, we reset the loading bar,
                PromptButtonPressed.Invoke();     // and we invoke the function associated with the prompt from the list below.
                                                  // Note that the specific function is chosen in the inspector, like with a Unity UI Button.
            }
            else   // implies the br still has some filling to do
            {
                loadingBarImage.fillAmount += loadingBarSpeed;   // We increase the bar's fill percent.
            }
            
        }

        // We check if the player let go half way, and if so tell the script that the bar should start receding.
        if (Input.GetButtonUp(promptButton))
        {
            if (waitForFingerLift == true)
            {
                waitForFingerLift = false;
            }

            barReturning = true;
        }

        // if these conditions are true, the bar needs to be receding.
        if (loadingBarImage.fillAmount > 0)
        {
            if (barReturning == true)
            {
                loadingBarImage.fillAmount -= loadingBarSpeed;   // We decrease the bar's fill percent.

                // and reset our bool if it's empty
                if (loadingBarImage.fillAmount <= 0)
                {
                    barReturning = false;
                }
            }
        }
    }

    public void OnEquip()
    {
        equippedAbilitiesController.EquipAbility(abilityInfoDisplay.currentlySelectedAbility);
        abilityInfoDisplay.AbilitySelected(abilityInfoDisplay.currentlySelectedAbility);         // This ensures the UI and information that the info
                                                                                                 // panel displays is updated.
    }

    public void OnDrop()
    {
        Ability abilityBeingDropped = abilityInfoDisplay.currentlySelectedAbility;

        // We store the coordinates we want the object to spawn at - the player's current position.
        // The second vector prevents 2 pickups spawning exactly on top of each other, which prevent's their colliders pushing them away from one another.
        Vector3 dropPoint = GameManager.gameManagerInstance.player.transform.position + new Vector3(Random.Range(0.01f, 0.1f), 0.2f);  

        // Then, we instantiate a default pickup, while keeping a reference to it.
        GameObject droppedPickupGO = Instantiate(InventoryManager.inventoryInstance.pickupPrefab, dropPoint, Quaternion.identity) as GameObject;

        // We then get a reference to it's pickup behaviour and set up it's properties to match the dropped ability
        PickupBehaviour droppedPickupBehaviour = droppedPickupGO.GetComponentInChildren<PickupBehaviour>();
        droppedPickupBehaviour.AssignAbility(abilityBeingDropped);

        // Finally, we need to remove the ability from all relevant arrays
        if (abilityBeingDropped.abilityEquipped)                                 // check if the ability is equipped
        {
            equippedAbilitiesController.UnequipAbility(abilityBeingDropped);     // and if so, unequip it                                              
        }

        InventoryManager.inventoryInstance.RemoveAbility(abilityBeingDropped);   // then, we remove it from the inventory

        abilityInfoDisplay.AbilitySelected(abilityInfoDisplay.currentlySelectedAbility);     // This ensures the UI and information that the info
                                                                                             // panel displays is updated.
    }

    public void OnUnequip()
    {
        equippedAbilitiesController.UnequipAbility(abilityInfoDisplay.currentlySelectedAbility);
        abilityInfoDisplay.AbilitySelected(abilityInfoDisplay.currentlySelectedAbility);         // This ensures the UI and information that the info
                                                                                                 // panel displays is updated.
    }

    public void OnEquipPrimary()
    {
        // First, we must check if this ability is already equipped in the Secondary slot. If so, we unequip it.
        if (abilityInfoDisplay.currentlySelectedAbility.abilityEquipped == true)
        {
            equippedAbilitiesController.UnequipAbility(abilityInfoDisplay.currentlySelectedAbility);
        }

        equippedAbilitiesController.EquipAbility(abilityInfoDisplay.currentlySelectedAbility, Ability.AbilityType.Primary);

        // We tell the ability that we're using it as a primary ability
        abilityInfoDisplay.currentlySelectedAbility.currentHybridType = Ability.AbilityType.Primary;

        abilityInfoDisplay.AbilitySelected(abilityInfoDisplay.currentlySelectedAbility);         // This ensures the UI and information that the info
                                                                                                 // panel displays is updated.
    }

    public void OnEquipSecondary()
    {
        // First, we must check if this ability is already equipped in the Primary slot. If so, we unequip it.
        if (abilityInfoDisplay.currentlySelectedAbility.abilityEquipped == true)
        {
            equippedAbilitiesController.UnequipAbility(abilityInfoDisplay.currentlySelectedAbility);
        }

        equippedAbilitiesController.EquipAbility(abilityInfoDisplay.currentlySelectedAbility, Ability.AbilityType.Secondary);

        // We tell the ability that we're using it as a secondary ability
        abilityInfoDisplay.currentlySelectedAbility.currentHybridType = Ability.AbilityType.Secondary;  

        abilityInfoDisplay.AbilitySelected(abilityInfoDisplay.currentlySelectedAbility);         // This ensures the UI and information that the info
                                                                                                 // panel displays is updated.
    }


    /// <summary>
    /// Checks if the user is still holding this prompt's button down from a previous interaction, and if so, prevent it affecting this prompt
    /// until they lift their finger.
    /// </summary>
    public void PromptButtonStillHeldCheck()
    {
        if (gameObject.activeInHierarchy)             // We need only check this on the prompts that are currently active and visible
        {
            if (Input.GetButton(promptButton))
            {
                waitForFingerLift = true;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityInfoDisplay : MonoBehaviour
{
    // All elements that show some information about the current ability
    [Header("Info Display Elements")]
    public Text abilityNameText;          // Displays the name of the current ability
    public Text abilityTypeText;          // Displays the type and category of the current ability 
    public Text abilityDescriptionText;   // Displays the ability's description
    public Image abilityDisplayImage;     // Displays a representation of the ability in the info panel

    // Instructive Images on the Ability Info page - they display the options for players, which are activated through indicated controls.
    [Header("Option Prompts")]
    public GameObject equipPrompt;             // Shows which button will equip selected ability. Only visible when ability not already equipped.
    public GameObject equipPrimaryPrompt;      // Shows which button will equip selected ability to Primary slot. Only visible when ability is Hybrid and not already equipped.
    public GameObject equipSecondaryPrompt;    // Shows which button will equip selected ability to Secondary slot. Only visible when ability is Hybrid and not already equipped.
    public GameObject unequipPrompt;           // Shows which button will unequip selected ability. Only visible when ability already equipped.
    public GameObject unequipPrimaryPrompt;    // Shows which button will unequip selected ability. Only visible when ability already equipped.
    public GameObject unequipSecondaryPrompt;  // Shows which button will unequip selected ability. Only visible when ability already equipped.
    public GameObject dropPrompt;              // Shows which button will drop ability from inventory in the form of a pickup.
    public GameObject sellPrompt;              // Shows which button will sell ability for the listed price. Only available at shop. Not implemented yet.
    public GameObject buyPrompt;               // Shows which button will buy the selected ability when at a shop. Not implemented yet.

    public Ability currentlySelectedAbility = null;   // Stores a reference to the ability currently described by the Ability Information panel

    AbilityInfoPrompt[] infoPagePrompts;       // We'll use this to cache a reference to the AbilityInfoPrompt components on each of our prompt gameObjects

    private void Start()
    {
        
    }

    /// <summary>
    /// Caches a reference to each prompt's AbilityInfoPrompt component.
    /// </summary>
    public void CacheAbilityInfoPromptComponents()
    {
        // It's not pretty, but it's more efficient than continually using GetComponent<> or .gameObject - this way we only do that once.
        infoPagePrompts = new AbilityInfoPrompt[9];

        infoPagePrompts[0] = equipPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[1] = equipPrimaryPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[2] = equipSecondaryPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[3] = unequipPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[4] = unequipPrimaryPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[5] = unequipSecondaryPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[6] = dropPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[7] = sellPrompt.GetComponent<AbilityInfoPrompt>();
        infoPagePrompts[8] = buyPrompt.GetComponent<AbilityInfoPrompt>();
    }


    /// <summary>
    /// Run each time a new ability is selected in the menu. Updates the information in the 'Ability Information' panel
    /// to match the details of the newly selected ability.
    /// </summary>
    /// <param name="selectedAbility"></param>
    public void AbilitySelected (Ability selectedAbility)
    {
        currentlySelectedAbility = selectedAbility;     // Update reference to ability we're currently displaying.
                                                        // This allows the AbilityInfoPrompt events to access this ability.
        UpdateVisiblePrompts(selectedAbility);
        UpdateInformationElements(selectedAbility);
    }



    /// <summary>
    /// Checks the current properties of the selected abilty and shows all the prompts that are relevent in the current
    /// situation, while hiding those that aren't.
    /// </summary>
    /// <param name="selectedAbility">The ability currently selected in the UI</param>
    void UpdateVisiblePrompts(Ability selectedAbility)
    {
        // Note that we may want to display Synnergy abilities, so the player can read up on them.
        // These abilities, however, cannot be directly equipped, unequipped or dropped.
        // Thus, no prompts will be active when they are selected.
        if (selectedAbility.abilityType == Ability.AbilityType.Synergised)
        {
            equipPrompt.SetActive(false);
            equipPrimaryPrompt.SetActive(false);
            equipSecondaryPrompt.SetActive(false);
            unequipPrompt.SetActive(false);
            unequipPrimaryPrompt.SetActive(false);
            unequipSecondaryPrompt.SetActive(false);
            sellPrompt.SetActive(false);
            dropPrompt.SetActive(false);
            buyPrompt.SetActive(false);
            return;                                      // We don't want to run the rest of the function, as we've already deactivated everything here.
        }

        if (selectedAbility.abilityEquipped == false)   // implies ability is not currently equipped
        {
            // Hybrid abilities have different equip prompts, thus we must check if this is a hybrid.
            if (selectedAbility.abilityType == Ability.AbilityType.Hybrid)
            {
                equipPrompt.SetActive(false);          // we deactivate the regular equip prompt
                equipPrimaryPrompt.SetActive(true);    // and activate the primary and secondary specific equip prompts
                equipSecondaryPrompt.SetActive(true);
            }
            else       // if not a hybrid, we can use the standard equip prompt
            {
                equipPrompt.SetActive(true);            // We can use the regular equip prompt...
                equipPrimaryPrompt.SetActive(false);    // and deactivate the primary and secondary specific equip prompts,
                equipSecondaryPrompt.SetActive(false);  // just in case a hybrid was selected previously.
            }

            // These prompts are needed by both Hybrid and Regular abilities
            dropPrompt.SetActive(true);     // We have to activate this again, just in case a synnergy was selected.          

            // These prompts should always be hidden if the selected ability isn't equipped.
            unequipPrompt.SetActive(false);
            unequipPrimaryPrompt.SetActive(false);
            unequipSecondaryPrompt.SetActive(false);
        }
        else    // implies selected ability is already equipped
        {
            // Hybrid abilities will still show 1 of their 2 equip prompts when equipped, thus they are handled slightly differently
            if (selectedAbility.abilityType == Ability.AbilityType.Hybrid)
            {
                // If the hybrid is equipped as a Primary, we still want the player to have the option to equip it as a Secondary,
                // thus unequipping it from Primary and replacing the current Secondary all in one click.
                // The same applies if it's equipped as a Secondary, but vice versa.
                if (selectedAbility.currentHybridType == Ability.AbilityType.Primary)      // Implies it's currently equipped as a Primary
                {
                    equipSecondaryPrompt.SetActive(true);      // We make sure the equip Secondary prompt is showing,
                    equipPrimaryPrompt.SetActive(false);       // but have no use for the equip Primary prompt right now.
                                                               // The ability is already equipped as a primary

                    unequipPrimaryPrompt.SetActive(true);      // We also give the player the option to remove the ability from their primary slot
                    unequipSecondaryPrompt.SetActive(false);   // and take away the impossible option of unequipping it from secondary
                }
                else      // Implies it's currently equipped as a Secondary
                {
                    equipPrimaryPrompt.SetActive(true);        // We make sure the equip Primary prompt is showing,
                    equipSecondaryPrompt.SetActive(false);     // but have no use for the equip Secondary prompt right now.
                                                               // The ability is already equipped as a Secondary.

                    unequipSecondaryPrompt.SetActive(true);   // We also give the player the option to remove the ability from their secondary slot 
                    unequipPrimaryPrompt.SetActive(false);    // and take away the impossible option of unequipping it from Primary
                }

                // if this is a hybrid ability, we don't want to show the regular unequip prompt
                unequipPrompt.SetActive(false);
            }
            else       // if not a hybrid, we don't want any equip prompts to show, nor do we want the specific unequip prompts to show
            {
                equipPrimaryPrompt.SetActive(false);        
                equipSecondaryPrompt.SetActive(false);
                unequipPrimaryPrompt.SetActive(false);
                unequipSecondaryPrompt.SetActive(false);

                unequipPrompt.SetActive(true);   // We do, however, want to show the regular unequip button
            }

            // These prompts will never be active if the selected ability is already equipped.
            equipPrompt.SetActive(false);
            dropPrompt.SetActive(true);       // We have to activate this again, just in case a synnergy was selected.

        }

        // we make sure that once the on-screen prompts are updated, the player's currently held input does not affect the new prompts
        CheckContinualPromptPress();
    }



    /// <summary>
    /// Updates all of the text and image elements in the information panel to match the selected ability.
    /// </summary>
    /// <param name="selectedAbility">The ability currently selected in the UI</param>
    void UpdateInformationElements(Ability selectedAbility)
    {
        abilityNameText.text = selectedAbility.abilityName;                  // Display the ability's name
        abilityTypeText.text = GenerateAbilityTypeText(selectedAbility);     // Generates a descriptive text for the ability's type and category
        abilityDescriptionText.text = selectedAbility.abilityDescription;    // Display description of how ability functions
        abilityDisplayImage.sprite = selectedAbility.icon;                   // Display an image that represents the ability
        abilityDisplayImage.gameObject.SetActive(true);                      // Ensures image is active again, in case it was deactivated
                                                                             // when no ability was selected.
    }



    /// <summary>
    /// Analyses the properties of an ability and generates a string describing its type and category.
    /// </summary>
    /// <param name="ability">The ability which the string will describe.</param>
    /// <returns></returns>
    string GenerateAbilityTypeText(Ability ability)
    {
        string abilityTypeText = "";     // We'll construct our new string piece by piece and store it in here.

        // Here we analyse the ability's type for the first word in our string.
        switch(ability.abilityType)
        {
            case Ability.AbilityType.Primary:            // Primary ability
                abilityTypeText = "Primary ";
                break;

            case Ability.AbilityType.Secondary:          // Secondary ability
                abilityTypeText = "Secondary ";
                break;

            case Ability.AbilityType.Hybrid:             // Hybrid ability
                abilityTypeText = "Hybrid ";
                break;

            case Ability.AbilityType.Synergised:         // Synergised ability. Later we'll add the two foundation abilities that cause it.
                abilityTypeText = "Synergy Ability";
                return abilityTypeText;                  // Synnergies don't really fall into categories, so we'll return here (this exits the function).
        }


        // Here we analyse the ability's category for the second word in our string
        switch(ability.abilityCategory)
        {
            case Ability.AbilityCategory.Jump:                    // Ability is some sort of jump
                abilityTypeText = abilityTypeText + "Jump";
                break;

            case Ability.AbilityCategory.Melee:                   // Ability is some sort of close range attack
                abilityTypeText = abilityTypeText + "Melee";
                break;

            case Ability.AbilityCategory.Defense:                 // Ability is some sort of defense
                abilityTypeText = abilityTypeText + "Defense";
                break;
                 
            case Ability.AbilityCategory.Ranged:                  // Ability is some sort of ranged attack
                abilityTypeText = abilityTypeText + "Ranged Attack";
                break;
        }

        return abilityTypeText;
    }

    /// <summary>
    /// This should be called if an ability is not selected. 
    /// <para>Adds some relevent text telling the player that they have nothing selected, and hides all prompts.</para>
    /// </summary>
    public void NoAbilitySelected()
    {
        currentlySelectedAbility = null;  // Removes the reference to the previous ability we were displaying.

        // Adjust visible information
        abilityNameText.text = "No Ability Selected.";              // Tell the player they have not selected an ability
        abilityTypeText.text = "...";
        abilityDisplayImage.sprite = null;                          // remove any display image that may have been there
        abilityDisplayImage.gameObject.SetActive(false);            // we also deactivate it, to prevent an unsighlty block of colour
        abilityDescriptionText.text = "Try choosing an ability " +
            "to look at.\nIf you don't have any yet, try " +
            "looking around for pickups or visiting a store.";

        // Deactivate all prompts
        equipPrompt.SetActive(false);
        equipPrimaryPrompt.SetActive(false);
        equipSecondaryPrompt.SetActive(false);
        unequipPrompt.SetActive(false);
        unequipPrimaryPrompt.SetActive(false);
        unequipSecondaryPrompt.SetActive(false);
        sellPrompt.SetActive(false);
        dropPrompt.SetActive(false);
        buyPrompt.SetActive(false);

    }


    /// <summary>
    /// Checks all the prompts that are currently active to see if their button is still being held from the previous
    /// action the user performed. If so, all interaction with this prompt is prevented until the user lifts their finger
    /// </summary>
    void CheckContinualPromptPress()
    {
        for (int i = 0; i < infoPagePrompts.Length; i++)
        {
            infoPagePrompts[i].PromptButtonStillHeldCheck();
        }
    }

}

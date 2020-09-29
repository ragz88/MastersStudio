using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;      // used for our array sort function in IniitialiseSlots()

public class EquippedAbilitiesController : MonoBehaviour
{
    
    /// <summary>
    /// Correlates each plausibly equipped ability type to a specific integer for cross referencing 
    /// it, and it's core components, in various arrays.
    /// </summary>
    enum AbilityNameIndex
    {
        // Primary Abilities
        PrimaryJump,
        PrimaryAttack,
        PrimaryDefense,
        PrimaryRanged,

        //Secondary Abilities
        SecondaryJump,
        SecondaryAttack,
        SecondaryDefense,
        SecondaryRanged

        // will potentially add synnergy indexes too
    }

    public InventoryManager inventoryManager;

    int numberOfEquipableSlots;                      // The number of slots a player can equip an ability into. One for each category

    public InventorySlot[] equippedAbilitySlots;     // stores references to each slot in the 'equiped abilities' panel.

    public Ability[] equippedAbilities = new Ability[8];
    

    /// <summary>
    /// Re-organises the slots in one's "Equipped Abilities" panel, clearing slots that have been unequipped, and updating
    /// newly equipped ones.
    /// </summary>
    public void UpdateUI()
    {
        Ability clearedAbility = null;              // will only store an ability if the slot we have currently selected is cleared.

        // Loops through all plausible spaces for equipped abilities
        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i] != null)      // checks if there is an ability equipped in each specific space
            {
                equippedAbilitySlots[i].AddAbility(equippedAbilities[i]);     // updates the UI to the correct ability if there is on present
            }
            else                                   // implies no ability equipped in this slot
            {
                // This implies we're about to clear the slot we have selected right now.
                // This will deselect all slots, which is an issue for controller players.
                if (equippedAbilitySlots[i].gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    clearedAbility = equippedAbilitySlots[i].ability;         // Let's remember the ability we had selected when this happened
                }

                equippedAbilitySlots[i].ClearSlot();                          // cleans out the slot if no ability is present in that slot's index
            }
        }

        // This implies we currently have nothing selected, as we disabled the button we had selected. This would leave contoller players stranded.
        if (clearedAbility != null)
        {
            // So we try to select a relevant ability to prevent this.
            InventoryManager.inventoryInstance.selectSpecificAbility(clearedAbility);
        }
    }



    /// <summary>
    /// Gets a reference to all the slot UI elements related to equipped abilities and
    /// stores them in an internal array. Also sorts this array into a consistent order based on the slot's object names (from the heirarchy).
    /// </summary>
    public void InitialiseSlots()
    {
        // As this script is attached to the parent objects of slots, we can simply get all the slots at once from the children objects.
        // The OrderBy() function sorts the slot objects by name in case they are added in an incorrct order.
        // Provided they are named with a correct number prefix in heirarchy, they'll be sorted to corrolate to our AbilityNameIndex enum.
        equippedAbilitySlots = GetComponentsInChildren<InventorySlot>().OrderBy(slot => slot.name).ToArray();
    }



    /// <summary>
    /// Adds this ability to the correct 'Equipped Abilities' slot and unequips what was initially there.
    /// This overloaded version is for primary, secondary and synnergised abilities.
    /// </summary>
    /// <param name="newAbility">Ability to be equipped.</param>
    public void EquipAbility(Ability newAbility)
    {
        ProcessNewAbility(newAbility, newAbility.abilityType);
    }

    /// <summary>
    /// Adds this ability to the correct 'Equipped Abilities' slot and unequips what was initially there.
    /// <para>Use this overloaded version for Hybrid abilities, as they require the user to choose whether they want to use the 
    /// ability as a primary or secondary ability.</para>
    /// </summary>
    /// <param name="newAbility">Ability to be equipped.</param>
    /// <param name="newAbilityType">The type of ability the user wants to treat this as. Either Primary or Secondary.</param>
    public void EquipAbility(Ability newAbility, Ability.AbilityType newAbilityType)
    {
        ProcessNewAbility(newAbility, newAbilityType);
    }



    /// <summary>
    /// Internal Function. Finds the correct "Equipped Abilities" slot to put the newly equipped ability in, 
    /// inserts it and removes what was there before.
    /// <para> Hybrid abilities can be equipped in either slot of their category, thus the function can be told which of the two slots the 
    /// player wants to equip the ability to. </para>
    /// </summary>
    /// <param name="equippedAbility">The ability to be equipped.</param>
    /// <param name="equippedAbilityType">Which slot the ability should be equipped in. Either Primary, Secondary or Synnergised.</param>
    void ProcessNewAbility(Ability newAbility, Ability.AbilityType newAbilityType)
    {
        // first we check if the new ability is a primary or secondary ability.
        if (newAbilityType == Ability.AbilityType.Primary)                     // deals with all the primary abilities
        {
            // Then we must check the ability's category - this tells us where to put it in the UI and arrays.
            switch (newAbility.abilityCategory)
            {
                case Ability.AbilityCategory.Jump:         // This ability is a Primary Jump
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.PrimaryJump);
                    }
                    break;

                case Ability.AbilityCategory.Melee:        // This ability is a Primary Attack
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.PrimaryAttack);
                    }
                    break;

                case Ability.AbilityCategory.Defense:      // This ability is a Primary Defense
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.PrimaryDefense);
                    }
                    break;

                case Ability.AbilityCategory.Ranged:       // This ability is a Primary Ranged
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.PrimaryRanged);
                    }
                    break;

            }
        }
        else if (newAbilityType == Ability.AbilityType.Secondary)              // Deals with all the secondary abilities
        {
            // We must check the ability's category - this tells us where to put it in the UI and arrays.
            switch (newAbility.abilityCategory)
            {
                case Ability.AbilityCategory.Jump:         // This ability is a Secondary Jump
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.SecondaryJump);
                    }
                    break;

                case Ability.AbilityCategory.Melee:        // This ability is a Secondary Attack
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.SecondaryAttack);
                    }
                    break;

                case Ability.AbilityCategory.Defense:      // This ability is a Secondary Defense
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.SecondaryDefense);
                    }
                    break;

                case Ability.AbilityCategory.Ranged:       // This ability is a Secondary Ranged
                    {
                        AddAbility(newAbility, (int)AbilityNameIndex.SecondaryRanged);
                    }
                    break;

            }
        }
        else if (newAbilityType == Ability.AbilityType.Synergised)            // Ability is a synnergised ability - arising from special
        {                                                                     // ability combinations. Will implement later.
            Debug.LogWarning("Synnergy Functionality not implemented yet." +
                        " Cannot equip " + newAbility.name + ".");
        }
        else if (newAbilityType == Ability.AbilityType.Hybrid)                // Ability entered was a Hybrid ability - which shouldn't have happened.
        {                                                                     // The user must choose to treat this as either primary or secondary.
            Debug.LogWarning("Player choice for hybrid's usage not defined. Please choose either primary or secondary type" +
                        " to equip " + newAbility.name + ".");
            return;
        }

        // I would have used nested switch statements, but that was far harder to read than this - so for the 
        // sake of understanding I used if statements.
    }


    /// <summary>
    /// Internal function used to add an ability to all the relevent arrays once it's been categorised and the correct index is found.
    /// </summary>
    /// <param name="abilityToAdd">The ability that's been categorised and is ready to be added to the relevent arrays</param> 
    /// <param name="index">The position in the arrays the ability should be added to</param>
    void AddAbility(Ability abilityToAdd, int index)
    {
        // Check if there was already an ability equipped here
        if (equippedAbilities[index] != null)
        {
            // and if so, we tell it it's being unequipped
            equippedAbilities[index].abilityEquipped = false;
        }

        // Equipping the new ability. 
        // Automatically overwrites what was originally there.
        equippedAbilities[index] = abilityToAdd;

        // Remove the new ability from the regular inventory

        // Updates 'Equipped Abilities' Inventory slots
        UpdateUI();

        // Lastly, after successfully completing all the previous steps, we can update the ability's status to equipped.
        abilityToAdd.abilityEquipped = true;
    }


    public void UnequipAbility(Ability unequippedAbility)
    {
        int abilityIndex = -1;      // this number will be updated if the correct ability is found while looping through all equipped abilities
        
        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i] == unequippedAbility)       // implies the correct ability was found
            {
                abilityIndex = i;   // we save the index of the correct ability
                break;
            }
        }

        if (abilityIndex != -1)     // this implies we were able to find a matching ability to unequip
        {
            // Puts the ability back into the general inventory

            // We return the ability's status to it's initial, unequipped state. This must only be done if we're sure we can unequip it, hence it's done here.
            unequippedAbility.abilityEquipped = false;

            // Unequips the ability, removing it from the relevent arrays
            equippedAbilities[abilityIndex] = null;

            // Updates 'Equipped Abilities' inventory slots
            UpdateUI();
        }
        else                        // ability index is still -1, thus no matching ability was found
        {
            Debug.LogWarning("Could not unequip " + unequippedAbility.name + " as it is not present in the current list of " +
                        "equipped abilities.");
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;      // used for our array sort function in IniitialiseSlots()


/// <summary>
/// NOTE: THIS IS A DUMMY CLASS!!!! It contains the few functions the musicManager would need to function - 
/// but is just a placeholder for the actual EquippedAbilitiesController
/// </summary>
public class EquippedAbilitiesController : MonoBehaviour
{
    /// <summary>
    /// Correlates each plausibly equipped ability type to a specific integer for cross referencing 
    /// it, and it's core components, in various arrays.
    /// </summary>
    enum AbilityNameIndex
    {
        Mobility,
        Offense,
        Defense
    }

    public Ability[] equippedAbilities = new Ability[3];
    

    



    /// <summary>
    /// Adds this ability to the correct 'Equipped Abilities' slot and unequips what was initially there.
    /// This overloaded version is for primary, secondary and synnergised abilities.
    /// </summary>
    /// <param name="newAbility">Ability to be equipped.</param>
    public void EquipAbility(Ability newAbility)
    {
        if (newAbility.abilityCategory == Ability.AbilityCategory.Mobility)
        {
            AddAbility(newAbility, (int)AbilityNameIndex.Mobility);
        }
        else if (newAbility.abilityCategory == Ability.AbilityCategory.Offense)
        {
            AddAbility(newAbility, (int)AbilityNameIndex.Offense);
        }
        else if (newAbility.abilityCategory == Ability.AbilityCategory.Defense)
        {
            AddAbility(newAbility, (int)AbilityNameIndex.Defense);
        }
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
        }
        else                        // ability index is still -1, thus no matching ability was found
        {
            Debug.LogWarning("Could not unequip " + unequippedAbility.name + " as it is not present in the current list of " +
                        "equipped abilities.");
        }
    }

}

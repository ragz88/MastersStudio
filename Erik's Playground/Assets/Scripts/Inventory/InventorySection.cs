using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class should be attached to the various parent objects of groups of inventory slots.
/// Handles updating of UI in said panel and stores categorisation info of that panel.
/// </summary>
public class InventorySection : MonoBehaviour
{
    
    public List<Ability> abilities = new List<Ability>();      // a list of abilities stored within this section
    [HideInInspector]
    public InventorySlot[] slots;                              // a reference to each slot in the system, in which abilities can be 
                                                               // represented and stored

    // We can re-use our enums from the ability class to define which abilities this section should store.
    public Ability.AbilityType sectionAbilityType;             // This is the type of ability : Primary, Secondary or Hybrid
    public Ability.AbilityCategory sectionAbilityCategory;     // This is the Category : Jump, Melee, Defense or Ranged.
                                                               // Note that synergised abilities will not be storable - they appear automatically
                                                               // when the correct combo of regular abilities are equipped.


    /// <summary>
    /// Re-organises the slots in one's inventory, removing empty spaces and any abilities no longer within it.
    /// </summary>
    public void UpdateUI()
    {
        Ability clearedAbility = null;              // will only store an ability if the slot we have currently selected is cleared.
        
        // Loops through entire slot collection for a specific section
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < abilities.Count)       // Ensures we only add as many abilities as there are in our list
            {
                slots[i].AddAbility(abilities[i]);
            }
            else
            {
                // This implies we're about to clear the slot we have selected right now.
                // This will deselect all slots, which is an issue for controller players.
                if (slots[i].gameObject == EventSystem.current.currentSelectedGameObject)
                {
                    clearedAbility = slots[i].ability;         // Let's remember the ability we had selected when this happened

                }

                slots[i].ClearSlot();      // Any extra slots would be empty, thus they are cleared
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
    /// Finds all the Inventory Slots within this section and puts them into an internally controlled list.
    /// </summary>
    public void InitialiseSlots()
    {
        // as this script is attached to the parent objects of slots, we can simply get all the 
        // slots at once from the children objects - using the function below.
        slots = GetComponentsInChildren<InventorySlot>();
    }
}

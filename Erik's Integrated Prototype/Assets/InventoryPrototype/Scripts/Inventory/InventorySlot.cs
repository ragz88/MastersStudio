using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;      // Needed to detect when a slot is selected

/// <summary>
/// This component should be attached to each individual inventory slot.
/// It contains simple functions that either clear a slot or fill it with a particular ability.
/// </summary>
public class InventorySlot : MonoBehaviour, ISelectHandler
{
    public Image icon;          // The image presented within a slot when it's filled
    public Button slotButton;   // Allows us to make slot interactable or not, and change it's colour.
    public Ability ability;     // This will store a reference to a specific ability if there is one in a slot


    /// <summary>
    /// This is called wheneverthis slot is selected in UI.
    /// We're going to use it to display that slot's information in the information panel.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSelect(BaseEventData eventData)
    {
        // Here, we tell our inventory manager to send a message to our AbilityInfoDisplay containing the properties and
        // characteristics of this slots currently contained ability
        InventoryManager.inventoryInstance.OnAbilitySelected(ability);
    }

    /// <summary>
    /// Adds an ability to the inventory after picking it up in the game.
    /// Makes the slot in question interactable and puts relevant image in it.
    /// This, in turn, leads to the ability equipable.
    /// </summary>
    /// <param name="newAbility">The ability to be added to the inventory</param>
    public void AddAbility(Ability newAbility)
    {
        ability = newAbility;

        icon.sprite = ability.icon;
        icon.enabled = true;

        slotButton.interactable = true;
    }

    /// <summary>
    /// Removes any ability within the selected slot.
    /// Clears the image and makes the button non-interactable.
    /// </summary>
    public void ClearSlot()
    {
        ability = null;

        icon.sprite = null;
        icon.enabled = false;

        slotButton.interactable = false;
    }

    /// <summary>
    /// Removes the ability in this slot from our abilities list in the Inventory manager.
    /// </summary>
    public void OnRemoveButton()
    {
        InventoryManager.inventoryInstance.RemoveAbility(ability);
    }

    /*/// <summary>
    /// Used to activate the slot's ability in the player's current moveset.
    /// The player can now use this ability, and the previously equipped ability is
    /// returned to the general inventory.
    /// </summary>
    public void EquipAbility()
    {
        if (ability != null)
        {
            ability.EquipAbility();
        }
    }
    */
}

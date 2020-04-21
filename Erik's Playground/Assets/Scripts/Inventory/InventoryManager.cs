using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the sorting of abilities when adding and removing them, and the navigation of inventory menu pages.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public Transform inventorySectionsParent;                       // A Transform that parents all the Inventory Section Components
    public InventorySection[] inventorySections;                    // Stores the different Inventory Sections split up by category and type
    public EquippedAbilitiesController equippedAbilityController;   // Stores a reference to the inventory manager for our 8 equipped ability slots

    public CategoryProperties[] categoryPanels;    // Stores a reference to each inventory page (panel) and its properties - i.e Jumps, Melee, Ranged and Defences
    int currentCategoryPanel = 0;                  // Index of the currently showing category panel within the categoryPanels array

    public AbilityInfoDisplay abilityInfoDisplay;  // Reference tto the controller for the ability information panel on the right of the menu.

    public GameObject pickupPrefab;                // Used to spawn pickups in the game space when dropped from inventory

    // The following region contains all the information needed to update text and image colours as menus change from page to page
    #region DisplayColoursAndHeadings

    public Text[] colourChangingTexts;     // reference to all text elements in the inventory menu that must change colour when the menu page changes
    public Text previousPageTipText;       // reference to the tip showing what the previous page is.
    public Text nextPageTipText;           // reference to the tip showing what the previous page is.
    public Text pageCategoryTitle;         // reference to the heading showing the page's category.
    public Image[] colourChangingImages;   // reference to any images that link to the current page category, and so should change colour

    #endregion 

    // The singleton ensures that only one instance of this script will ever exist at a given time. This makes the script easy to reference.
    #region Singleton
    public static InventoryManager inventoryInstance;

    private void Awake()
    {
        // We check if another instance of this exists - if it does, we destroy this. 
        // This ensures that only one of these objects can ever exist at a time.
        if (inventoryInstance == null)
        {
            inventoryInstance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    #endregion

    private void Start()
    {
        // Allows us to cache a reference to all the different inventory sections
        inventorySections = inventorySectionsParent.GetComponentsInChildren<InventorySection>();

        // Sections each find the slots in their children and store them in an internal list.
        // This must be done here, as the inventorySection gameobjects are not active initially, preventing their start functions 
        // from running.
        for (int i = 0; i < inventorySections.Length; i++)
        {
            inventorySections[i].InitialiseSlots();
        }

        equippedAbilityController.InitialiseSlots();           // allows the EquippedAbilityController to find a reference to all of the InventorySlots in it's children
        
        categoryPanels[0].gameObject.SetActive(true);          // we set one of our category panels to active - this will be the default when the menu opens...                 

        // ...and all the rest are made inactive. Only one can show at a time.
        for (int i = 1; i < categoryPanels.Length; i++)
        {
            categoryPanels[i].gameObject.SetActive(false);
        }

        abilityInfoDisplay.CacheAbilityInfoPromptComponents(); // gets a reference to all the information screen prompts' AbilityInfoPrompt components

        UpdateMenuColours();                                   // We set the various heading colours to match the currently displayed category.
        abilityInfoDisplay.NoAbilitySelected();                // Initialises the information panel in the menu - now does not show any specific info
                                                               // until the player selects an ability.

    }

    /// <summary>
    /// Finds the correct inventory section for an ability and adds it to that section's ability list. 
    /// This happens when it is picked up or unequipped.
    /// </summary>
    /// <param name="newAbility">The ability to be added.</param>
    public void StoreAbility(Ability newAbility)
    {
        int correctSectionIndex = -1;  // will be set to the index i of the section within inventorySections
                                       // that should be storing the new ability
        
        // using a for loop, we search for the inventory section that should store this ability, based 
        // on the ability's type and category
        for (int i = 0; i < inventorySections.Length; i++)
        {
            if (inventorySections[i].sectionAbilityType == newAbility.abilityType)   // if true, this is the corect type of section
            {                                                                        // (Primary, Secondary or Hybrid)
 
                if (inventorySections[i].sectionAbilityCategory == newAbility.abilityCategory)   // if true, this is the correct category's section                                                             // 
                {                                                                                // (Jump, Melee, Ranged or Defense)
                    // Correct Section Found!
                    correctSectionIndex = i;
                    break;
                }
            }
        }

        if (correctSectionIndex != -1)  // ensures we found a correlating section
        {
            inventorySections[correctSectionIndex].abilities.Add(newAbility);        // Adds ability to the correct section's list
            inventorySections[correctSectionIndex].UpdateUI();                       // Updates the slots and UI of that section
        }
        else   // Implies correct section could not be found
        {
            Debug.LogWarning("Correct ability section could not be found when trying to add " + newAbility.name);
        }
         
    }


    /// <summary>
    /// Finds the section that the ability would be stored in, then removes 
    /// it from that section's list of stored abilities.
    /// This happens if it is discarded or equipped.
    /// </summary>
    /// <param name="removedAbility">The ability to remove.</param>
    public void RemoveAbility(Ability removedAbility)
    {
        int correctSectionIndex = -1;  // will be set to the index i of the section within inventorySections
                                       // that would be storing the ability we want removed

        // using a for loop, we search for the inventory section that should store this ability, based 
        // on the ability's type and category
        for (int i = 0; i < inventorySections.Length; i++)
        {
            if (inventorySections[i].sectionAbilityType == removedAbility.abilityType)   // if true, this is the corect type of section
            {                                                                            // (Primary, Secondary or Hybrid)

                if (inventorySections[i].sectionAbilityCategory == removedAbility.abilityCategory)   // if true, this is the correct category's section                                                             // 
                {                                                                                    // (Jump, Melee, Ranged or Defense)
                    // Correct Section Found!
                    correctSectionIndex = i;
                    break;
                }
            }
        }

        if (correctSectionIndex != -1)  // ensures we found a correlating section
        {
            inventorySections[correctSectionIndex].abilities.Remove(removedAbility);     // Removes ability from the correct section's list
            inventorySections[correctSectionIndex].UpdateUI();                           // Updates the slots and UI of that section
        }
        else   // Implies correct section could not be found
        {
            Debug.LogWarning("Correct ability section could not be found when trying to remove " + removedAbility.name);
        }


    }


    /// <summary>
    /// Use when menu opened, page changed or currently selected slot is disabled. Selects the first available slot in the inventory if possible.
    /// If not, it will clear the information panel.
    /// </summary>
    public void SelectFirstAvailableSlot()
    {
        Button firstAvailableSlotButton = null;    // Once we find the first available slot, we'll store a reference to it in here.
                                                   // We reference its Button component as we need a Selectable UI Element to work with

        // First we look through our inventory sections...
        for (int i = 0; i < inventorySections.Length; i++)
        {
            // ...and specifically check the ones of the same category as the current menu page
            if (inventorySections[i].sectionAbilityCategory == categoryPanels[currentCategoryPanel].Category)   // implies this section is on the currently active page
            {
                if (inventorySections[i].abilities.Count > 0)         // implies at least 1 ability is stored in this Inventory Section
                {
                    firstAvailableSlotButton = inventorySections[i].slots[0].GetComponent<Button>();   // and so we get a reference to the first slot's button
                    break;                                                                             // We have what we need, so we exit the loop.
                }
            }
        }

        // Now, let's check if we actually found a filled slot.
        if (firstAvailableSlotButton == null)                      // If this is still null, no selectable slot was found...   
        {
            // so we'll look through our equipped abilities to see if there's something available there.
            for (int i = 0; i < equippedAbilityController.equippedAbilitySlots.Length; i++)
            {
                if (equippedAbilityController.equippedAbilitySlots[i].ability != null)  // implies there is an ability in that slot...
                {
                    // ...so we can select it! We'll reference that slot's button with our variable to select it later.
                    firstAvailableSlotButton = equippedAbilityController.equippedAbilitySlots[i].GetComponent<Button>();
                    break;                      // We have what we need, so we'll leave the loop
                }
            }
        }

        // We need to check if the variable is STILL null. This implies there are no abilities in the current menu.
        // In this case, the best we can do is update the displayed info to show that nothing is selected.
        if (firstAvailableSlotButton == null)
        {
            abilityInfoDisplay.NoAbilitySelected();
        }
        else      // this implies that we found a viable slot at some point, so we can select it.
        {
            firstAvailableSlotButton.Select();   
            
            // in theory, the following two lines are not necessary, but there is a bug within Unity's UI event systems that leads to their requirement.
            firstAvailableSlotButton.OnSelect(null);                                                              // This ensures the button is highlighted
            abilityInfoDisplay.AbilitySelected(firstAvailableSlotButton.GetComponent<InventorySlot>().ability);   // This is not run automatically by the 
                                                                                                                  // OnSelect event in InventorySlot, so we trigger it manually.
        }
    }


    /// <summary>
    /// Selects a defined ability in the currently open menu page. Use when unequipping an ability from the Equipped Abilities
    /// page to select the ability just unequipped in the regular menu. 
    /// <para>This gives the player a more seamless experience. If the function can't find the ability in the regular menu, 
    /// it will selecte the first available slot.</para>
    /// </summary>
    /// <param name="abilityToSelect">The ability we want to select in the regular menu</param>
    public void selectSpecificAbility(Ability abilityToSelect)
    {
        Button firstAvailableSlotButton = null;    // Once we find the ability's slot, we'll store a reference to it in here.
                                                   // We reference its Button component as we need a Selectable UI Element to work with

        // First we'll look through our equipped abilities to see if this was a hybrid ability that changed from primary to secondary (or vice versa).
        for (int i = 0; i < equippedAbilityController.equippedAbilitySlots.Length; i++)
        {
            if (equippedAbilityController.equippedAbilitySlots[i].ability == abilityToSelect)  // implies this slot contains the ability we're looking for
            {
                // ...so we can select it! We'll reference that slot's button with our variable to select it later.
                firstAvailableSlotButton = equippedAbilityController.equippedAbilitySlots[i].GetComponent<Button>();
                break;                      // We have what we need, so we'll leave the loop
            }
        }


        // Now, let's check if we actually found the ability in a slot.
        if (firstAvailableSlotButton == null)                      // If this is still null, the ability wasn't found...   
        {
            // so we go on to look through our inventory sections...
            for (int i = 0; i < inventorySections.Length; i++)
            {
                // ...and specifically check the ones of the same category as the current menu page
                if (inventorySections[i].sectionAbilityCategory == categoryPanels[currentCategoryPanel].Category)   // implies this section is on the currently active page
                {
                    for (int j = 0; j < inventorySections[i].slots.Length; j++)
                    {
                        if (inventorySections[i].slots[j].ability == abilityToSelect)  // Implies we've found the slot containing our ability
                        {
                            firstAvailableSlotButton = inventorySections[i].slots[j].GetComponent<Button>();   // and so we get a reference to that slot's button
                            break;                                                                             // We have what we need, so we exit the loop.
                        }
                    }

                    if (firstAvailableSlotButton != null)    // implies we found the ability in the loop above
                    {
                        break;    // so we can break out of the second loop too
                    }
                }
            }
        }

        // We need to check if the variable is STILL null. This implies that the ability we're looking for is not in this menu.
        // In this case, we'll just select the first available slot in the menu
        if (firstAvailableSlotButton == null)
        {
            SelectFirstAvailableSlot();
        }
        else      // this implies that we found the ability at some point, so we can select it's button.
        {
            firstAvailableSlotButton.Select();

            // in theory, the following two lines are not necessary, but there is a bug within Unity's UI event systems that leads to their requirement.
            firstAvailableSlotButton.OnSelect(null);                                                              // This ensures the button is highlighted
            abilityInfoDisplay.AbilitySelected(firstAvailableSlotButton.GetComponent<InventorySlot>().ability);   // This is not run automatically by the 
                                                                                                                  // OnSelect event in InventorySlot, so we trigger it manually.
        }
    }


    /// <summary>
    /// Deactivates the current categoryPanel and activates the next one.
    /// To be called when the 'Next Page' control is pressed by player.
    /// <para>In the final, this will be upgraded to a fade or a lerp.</para>
    /// </summary>
    public void NextInventoryPage()
    {
        categoryPanels[currentCategoryPanel].gameObject.SetActive(false);     // deactivate the current page

        //Before we increase the number, we can use the current categoryPanel number to find our 'previous page' title - as the current page will become our new previous page!
        previousPageTipText.text = categoryPanels[currentCategoryPanel].Category.ToString();  // Change the text tips to reflect our new 'Previous' page

        // This mod function results in the number increasing cyclically - when the current number increases to the 
        // number of category panels, the mod sets it back to 0 automatially.
        currentCategoryPanel = (currentCategoryPanel + 1) % categoryPanels.Length;

        //Update the title of the current page - note that currentCategoryPanel number has changed
        pageCategoryTitle.text = categoryPanels[currentCategoryPanel].Category.ToString();

        int nextPageNum = (currentCategoryPanel + 1) % categoryPanels.Length; // we get the next page's number using our mod again...
        nextPageTipText.text = categoryPanels[nextPageNum].Category.ToString(); // and set our next page tip up using this number

        categoryPanels[currentCategoryPanel].gameObject.SetActive(true);      // activate the next page
        UpdateMenuColours();
        SelectFirstAvailableSlot();     // ensure a slot is selected for controller navigation
    }

    /// <summary>
    /// Deactivates the current categoryPanel and activates the previous one.
    /// To be called when the 'Previous Page' control is pressed by player.
    /// <para>In the final, this will be upgraded to a fade or a lerp.</para>
    /// </summary>
    public void PreviousInventoryPage()
    {
        categoryPanels[currentCategoryPanel].gameObject.SetActive(false);     // deactivate the current page

        //Before we reduce the number, we can use the current categoryPanel number to find our 'next page' title - as the current page will become our new next page!
        nextPageTipText.text = categoryPanels[currentCategoryPanel].Category.ToString();  // Change the text tips to reflect what the new next page is

        // We reduce the current number by 1
        currentCategoryPanel -= 1;

        // Then check that it isn't below 0 - in which case we set it back to (the length of the categoryPanels array) - 1
        // Note that our clever mod function won't work here, as % is not a perfect mod - it is a signed remainder function.
        if (currentCategoryPanel < 0)
        {
            currentCategoryPanel = categoryPanels.Length - 1;
        }

        //Update the title of the current page - note that currentCategoryPanel number has changed
        pageCategoryTitle.text = categoryPanels[currentCategoryPanel].Category.ToString();

        int previousPageNum = currentCategoryPanel - 1; // we get the previous page's number using our special if statement again...

        if (previousPageNum < 0)   // we ensure a cyclic number...
        {
            previousPageNum = categoryPanels.Length - 1;
        }

        previousPageTipText.text = categoryPanels[previousPageNum].Category.ToString(); // and set our next page tip up using this number

        categoryPanels[currentCategoryPanel].gameObject.SetActive(true);      // activate the next page
        UpdateMenuColours();
        SelectFirstAvailableSlot();     // ensure a slot is selected for controller navigation
    }

    /// <summary>
    /// Used to change all the text and image colours in the menu to match the currently selected category.
    /// </summary>
    void UpdateMenuColours()
    {
        for (int i = 0; i < colourChangingTexts.Length; i++)
        {
            colourChangingTexts[i].color = categoryPanels[currentCategoryPanel].textColour;
        }

        // We'll use these to colour our next and previous page tips appropriately.
        int nextPageNum = (currentCategoryPanel + 1) % 4;     // The % function creates a cyclic nature, setting this back to 0 should it get to 4
        int previousPageNum = currentCategoryPanel - 1;

        if (previousPageNum < 0)                                 // as % doesn't work on negative numbers, we use an if statement to create the cyclic nature
        {
            previousPageNum = categoryPanels.Length - 1;                                  
        }

        // Set the tip colours to their relative category colours
        previousPageTipText.color = categoryPanels[previousPageNum].textColour;
        nextPageTipText.color = categoryPanels[nextPageNum].textColour;

        for (int i = 0; i < colourChangingImages.Length; i++)
        {
            colourChangingImages[i].color = categoryPanels[currentCategoryPanel].imageColour;
        }
    }

    /// <summary>
    /// Links the Inventory Slots to the AbilityInfoDisplay - this way is easier to reference in the Inventory slots, 
    /// as this manager is a singleton.
    /// </summary>
    /// <param name="selectedAbility">The ability within the Inventory Slot that was just selected.</param>
    public void OnAbilitySelected(Ability selectedAbility)
    {
        abilityInfoDisplay.AbilitySelected(selectedAbility);
    }

}

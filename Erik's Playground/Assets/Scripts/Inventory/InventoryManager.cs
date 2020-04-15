using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public Transform inventorySectionsParent;        // A Transform that parents all the Inventory Section Components
    public InventorySection[] inventorySections;     // Stores the different Inventory Sections split up by category and type

    public GameObject[] categoryPanels;              // Stores a reference to each inventory page (panel) - i.e Jumps, Melee, Ranged and Defences
    int currentCategoryPanel = 0;                    // Index of the currently showing category panel within the categoryPanels array

    // The singleton ensures that only one instance of this script will ever exist at a given time
    #region Singleton
    public static InventoryManager inventoryInstance;

    private void Awake()
    {
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
        
        categoryPanels[0].SetActive(true);         // we set one of our category panels to active                 

        // and all the rest are made inactive. Only one can show at a time.
        for (int i = 1; i < categoryPanels.Length; i++)
        {
            categoryPanels[i].SetActive(false);
        }

    }

    /// <summary>
    /// Finds the corrct inventory section for an ability and adds it to that section's ability list. 
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
    /// Deactivates the current categoryPanel and activates the next one.
    /// To be called when the RightArrow is clicked in the UI
    /// In the final, this will be upgraded to a fade or a lerp.
    /// </summary>
    public void NextInventoryPage()
    {
        categoryPanels[currentCategoryPanel].SetActive(false);     // deactivate the current page

        // This mod function results in the number increasing cyclically - when the current number increases to the 
        // number of category panels, the mod sets it back to 0 automatially.
        currentCategoryPanel = (currentCategoryPanel + 1) % categoryPanels.Length;

        categoryPanels[currentCategoryPanel].SetActive(true);      // activate the next page
    }

    /// <summary>
    /// Deactivates the current categoryPanel and activates the previous one.
    /// To be called when the RightArrow is clicked in the UI
    /// In the final, this will be upgraded to a fade or a lerp.
    /// </summary>
    public void PreviousInventoryPage()
    {
        categoryPanels[currentCategoryPanel].SetActive(false);     // deactivate the current page

        // We reduce the current number by 1
        currentCategoryPanel -= 1;

        // Then check that it isn't below 0 - in which case we set it back to (the length of the categoryPanels array) - 1
        // Note that our clever mod function won't work here, as % is not a perfect mod - it is a signed remainder function.
        if (currentCategoryPanel < 0)
        {
            currentCategoryPanel = categoryPanels.Length - 1;
        }

        categoryPanels[currentCategoryPanel].SetActive(true);      // activate the next page
    }

}

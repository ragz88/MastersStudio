using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the opening and closing of the games menus, triggering the changing of menu pages as well as the pausing of gameplay.
/// </summary>
public class MenuManager : MonoBehaviour
{
    public GameObject InventoryMenu;   // reference to the gameObject containing all the inventory management UI
    public GameObject PauseMenu;       // reference to the gameObject containing all the pause menu UI

    bool pauseMenuOpen = false;        // set to true when Pause Menu is open
    bool inventoryOpen = false;        // set to true when Inventory Menu is open
    bool pauseGameplay = false;        // true when any menu is open

    //public InventoryManager inventoryManager;   // reference to inventory manager, to trigger the changing of pages


    // Update is called once per frame
    void Update()
    {
        // checks if the player attempts to open their inventory
        if (Input.GetButtonDown("OpenInventory"))
        {
            if (inventoryOpen == true)  // inventory already open, thus button was pressed again - it gets closed and game resumes
            {
                inventoryOpen = false;
                pauseGameplay = false;
                InventoryMenu.SetActive(false);   // deactivates the inventory UI elements
                Time.timeScale = 1f;
            }
            else   // inventory opened
            {
                inventoryOpen = true;
                pauseGameplay = true;
                InventoryMenu.SetActive(true);    // activates the inventory UI elements
                InventoryManager.inventoryInstance.SelectFirstAvailableSlot();     // ensure a slot is selected for controller navigation
                Time.timeScale = 0f;
            }
        }

        // checks if the player presses the pause button
        if (Input.GetButtonDown("Pause"))
        {
            if (pauseMenuOpen == true)  // game already paused - menu closed and game resumes
            {
                pauseMenuOpen = false;
                pauseGameplay = false;
                Time.timeScale = 1f;
            }
            else                        // game gets paused
            {
                pauseMenuOpen = true;
                pauseGameplay = true;
                Time.timeScale = 0f;
            }
        }

        // Here we chack if the player tries to change pages in the inventory
        if (inventoryOpen == true)                             // First, we ensure the inventory is open
        {
            if (Input.GetButtonDown("MenuPageLeft"))           // Check if the 'Previous Page' control is being pressed
            {
                InventoryManager.inventoryInstance.PreviousInventoryPage();
            }
            else if (Input.GetButtonDown("MenuPageRight"))     // Check if the 'Next Page' control is being pressed
            {
                InventoryManager.inventoryInstance.NextInventoryPage();
            }
        }
        
    }
}

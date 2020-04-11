﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject InventoryMenu;   // reference to the gameObject containing all the inventory management UI
    public GameObject PauseMenu;       // reference to the gameObject containing all the pause menu UI

    bool pauseMenuOpen = false;        // set to true when Pause Menu is open
    bool inventoryOpen = false;        // set to true when Inventory Menu is open
    bool pauseGameplay = false;        // ture when any menu is open

    // Start is called before the first frame update
    void Start()
    {
        
    }

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

    }
}

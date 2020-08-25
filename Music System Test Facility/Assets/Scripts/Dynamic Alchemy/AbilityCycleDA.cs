using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCycleDA : MonoBehaviour
{
    
    public Ability[] abilities;

    int currentIndex = -1;

    public Image abilityFace;
    public Sprite noAbilityImage;


    private void Start()
    {
        // For sake of simplicity, we'll just set our abiity back to what it originally was when the level restarts
        MusicControllerDA.musicControllerInstance.RemoveInstrument(abilities[abilities.Length - 1].abilityCategory);

        // and then tell it to select the first instrument in the list of available ones
        NextAbility();
    }

    /// <summary>
    /// Cycles to next ability in the public array - equipping it in the Music Manager. 
    /// If the array is complete, it will unequip all abilities from it's category 
    /// for the next step in the cycle, before equipping the first one again.
    /// </summary>
    public void NextAbility()
    {
        currentIndex++;

        if (currentIndex > abilities.Length - 1)
        {
            currentIndex = -1;                                                                           // We'll unequip the ability for one of the cycles steps (for testing)
           
            // we'll access the final element of the array - ie the last ability that was playing music
            MusicControllerDA.musicControllerInstance.RemoveInstrument(abilities[abilities.Length - 1].abilityCategory);  
            
            abilityFace.sprite = noAbilityImage;                                                         // change the display image
        }
        else
        {
            MusicControllerDA.musicControllerInstance.EquipInstrument(abilities[currentIndex]);          // equip the ability
            abilityFace.sprite = abilities[currentIndex].icon;                                           // change the display image
        }

        
    }
}

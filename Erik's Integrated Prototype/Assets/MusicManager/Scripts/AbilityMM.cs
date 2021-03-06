﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Inventory/Ability")]
public class AbilityMM : ScriptableObject
{
    /// <summary>
    /// Stores the category that this ability falls under.
    /// Note: 'Synergised' are passives that appear with specific combinations of abilities.
    /// </summary>
    public enum AbilityCategory
    {
        Mobility,
        Offense,
        Defense
    }

    /// <summary>
    /// Stores the type of ability - either Primary, Secondary or a hybrid
    /// of the two. Hybrids can be equipped in either a primary or secondary slot.
    /// </summary>
    public enum AbilityType
    {
        Primary,
        Secondary,
        Hybrid,
        Synergised
    }

    public string abilityName = "New Ability";                          // The name of the ability in menus and gameplay

    [TextArea(5, 12)]                                                   // Makes our text box in inspector a little bigger, so writing the description is more comfortable.
    public string abilityDescription = "Ability Description";           // A description of how the ability functions
    public Sprite icon = null;                                          // The ability's icon to show in the inventory menu

    public AbilityCategory abilityCategory = AbilityCategory.Mobility;  // The ability's core category, 
                                                                        // defining which menu it appears in

    public AbilityType abilityType = AbilityType.Primary;               // The ability's type, defining which of the slots within
                                                                        // its core category it can be equipped to

    public bool abilityEquipped = false;                                // Set to true if the ability is currently equipped
    public AbilityType currentHybridType = AbilityType.Primary;         // Used exclusively for Hybrid abilies. Set to either primary or secondary,
                                                                        // based on what they're acting as currently   

    /// <summary>
    /// Stores a reference to a Song and the instrument this ability would play in said song
    /// </summary> 
    [Serializable]
    public struct InstrumentSongPair
    {
        public Song specificSong;                    
        public Instrument abilityInstrument;                             // The Instrument this ability would activate in the song specified
    }

    // An array of the specific instrument this ability would activate in each specific song
    public InstrumentSongPair[] abilityInstruments;


    /// <summary>
    /// Links the abilities functionality to the correct control set, allowing the character to
    /// use the ability.
    /// </summary>
    public virtual void EquipAbility()
    {

    }

}

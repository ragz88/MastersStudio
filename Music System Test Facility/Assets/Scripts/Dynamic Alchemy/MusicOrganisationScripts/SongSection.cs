using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a reference to a collections of instruments that make up the section, the background music clip for the section
/// an overall rating of the energy in the section, as well as a list of sections that this one can naturally flow into.
/// </summary>
[CreateAssetMenu(fileName = "New Song Section", menuName = "Dynamic Music/Song Section")]
public class SongSection : ScriptableObject
{
    public string sectionName;

    // Stores and audio clip that will always feature in the song regardless of the instruments the player equips
    public AudioClip sectionBackground;

    //  References an overal score summarising the speed, volume and complexity of a section.
    //  Will be used to find an appropriate section to match the player's playstyle
    [Range(0f, 5f)]
    public float sectionEnergy;

    // Various instrument clips that make up this song section
    public Instrument[] instruments;

    // Ssections that this section can flow into naturally
    public SongSection[] forwardLinks;

}

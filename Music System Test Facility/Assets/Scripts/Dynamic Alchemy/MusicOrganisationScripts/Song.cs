using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


/// <summary>
/// Stores information about a single song. Holds links to all the individual song sections.
/// </summary>
[CreateAssetMenu(fileName = "New Song", menuName = "Dynamic Music/Song")]
public class Song : ScriptableObject
{
    public string songName;                          // The name of the song
    public MusicNode[] musicNodes;                   // All the nodes that have a place in this song. These are sorted when the song is initialised.
    
    [HideInInspector]
    public int finalSection = 0;                     // Stores the index of the final section of the song after it's sorted
    [HideInInspector]
    public int lowestSection = 0;                    // Stores the index of the section of the song with the least energy after it's sorted
    [HideInInspector]
    public int highestSection = 0;                   // Stores the index of the section of the song with the highest energy after it's sorted

    /// <summary>
    /// List of all unique instruments in the song. 
    /// Automatically populated based on the nodes that are publicly assigned to this song. 
    /// </summary>
    [HideInInspector]
    public List<Instrument> songInstruments = new List<Instrument>();


    /// <summary>
    /// Stores all the important information partaining to a single song section.
    /// <br>This includes the <b>Section Energy</b> and the <b>Forward Links</b> of a given section.</br>
    /// </summary>
    [Serializable]
    public struct SectionInfo
    {
        /// <summary>
        /// Each section has a corresponding energy level based on it's speed, the frequency of notes, its pitch, volume etc.
        /// These values are predefined by Rodwin and entered in inspector for each song.
        /// </summary>
        [Range(0, 5)]
        public float songSectionEnergy;

        /// <summary>
        /// The indeces of the song sections that this section naturally flows into.
        /// </summary>
        public int[] forwardLinks;
    }


    /// <summary>
    /// This stores <b>energy levels</b> and <b>forward links</b> for each individual song section in an easily navigable array.
    /// <br>The <b>index correlates with the section number</b> to which those forward links and energy levels  belong.</br>
    /// </summary>
    public SectionInfo[] sectionDetails;


    /// <summary>
    /// This 2D array sorts the songs individual nodes by <b>Instrument</b> and <b>Section</b> for easy reference and use. 
    /// <br>X:  Instrument</br>
    /// <br>Y:  Section</br>
    /// </summary>
    public MusicNode[,] sortedNodes;             



    /// <summary>
    /// Examines all the nodes that make up the song, and categorises them based on song Section and instrument.
    /// These are then stored in a sorted 2D array, allowing easy access to any instrument/sections nodes.
    /// </summary>
    public void InitialiseSong()
    {
        // We start by populating our instrument list with each unique instrument from our node array.
        // We'll also examine each node to find the int representing the final section in the song (largest int)
        // These will be used to create our 2D sortedNodes array.
        finalSection = 0;

        for (int i = 0; i < musicNodes.Length; i++)                  // we examine each node's instrument...
        {
            bool instrumentPresent = false;                                // this is set to true if the current node's instrument has already been found
            
            // Update finalSection if this node's section value is higher than it's current value
            if (musicNodes[i].nodeSection > finalSection)
            {
                finalSection = musicNodes[i].nodeSection;
            }

            for (int j = 0; j < songInstruments.Count; j++)          // ...and compare it to those we've already put in our list
            {
                if (songInstruments[j] == musicNodes[i].nodeInstrument)
                {
                    instrumentPresent = true;
                    break;                                           // we already found a match in our list of instruments - we can stop searching
                }
            }

            if (instrumentPresent == false)                          // this implies we couldn't find a matching instrument in our list
            {
                songInstruments.Add(musicNodes[i].nodeInstrument);   // so we add the current node's instrument to the list.
            }
        }

        // The next step is to initialise a 2D array of the correct size - [NumberOfInstruments : NumberOfSections]
        sortedNodes = new MusicNode[songInstruments.Count, finalSection + 1];              // We add 1 to final section because of the 0-index offset

        // Now all that's left is to populate our new 2D array with nodes.
        for (int i = 0; i < musicNodes.Length; i++)
        {
            // IndexOf() loops through the list and finds the first index which contains a match to the given Instrument in brackets
            sortedNodes[songInstruments.IndexOf(musicNodes[i].nodeInstrument), musicNodes[i].nodeSection] = musicNodes[i];
        }

        // Here I just check that the number of sections found and number of assigned energies lines up - 
        // and throw out a warning if they don't.
        if (sectionDetails.Length != finalSection + 1)
        {
            Debug.LogWarning("Number of sections found does not match the number of assigned section energies!");
        }

        // Finally, with our 2D array set up, we can loop through our sectionDetails and find the lowest and highest energy sections
        for (int i = 0; i < sectionDetails.Length; i++)
        {
            // check if the current section energy is lower than the current lowest section
            if (sectionDetails[lowestSection].songSectionEnergy > sectionDetails[i].songSectionEnergy)
            {
                lowestSection = i;                  // if so, we save it's index as the lowest energy section
            }

            // check if the current section energy is higher than the current highest section
            if (sectionDetails[highestSection].songSectionEnergy < sectionDetails[i].songSectionEnergy)
            {
                highestSection = i;                 // if so, we save it's index as the highest energy section
            }
        }
    }


    /// <summary>
    /// Shows the song's 2d array in the console for debugging purposes.
    /// </summary>
    public void LogArrayToConsole()
    {
        for(int i = 0; i < sortedNodes.GetLength(0); i++)
        {
            for (int j = 0; j < sortedNodes.GetLength(1); j++)
            {
                Debug.Log(i + ":" + j + "  -  "  + sortedNodes[i,j].name);
            }
        }
    }
}

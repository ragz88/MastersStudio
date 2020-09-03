using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores a Single Instrument's AudioClips within a single section of a song. Please see accompanying documentation.
/// </summary>
[CreateAssetMenu(fileName = "New Instrument", menuName = "Dynamic Music/Music Node")]
public class MusicNode : ScriptableObject
{
    public Song nodeSong;                   // The song this smaller piece of music forms a part of.
    public int nodeSection;                 // The section of a Song that this node forms a part of. 
    public Instrument nodeInstrument;       // I'm using an instrument scriptable object instead of a string to identify unique instruments.
                                            // This makes adding an instrument safer (no spelling errors) and potentially allows for expansion
                                            // I may potentially revert to a string later if this proves unnecessary.
    
    
    // Stores the audio for this instrument's notes in a section that should play at 
    // low energy moments in gameplay
    public AudioClip lowClip;

    // Stores the audio for this instrument's notes in a section that should play at 
    // average energy moments in gameplay
    public AudioClip medClip;

    // Stores the audio for this instrument's notes in a section that should play at 
    // high energy moments in gameplay
    public AudioClip highClip;

}

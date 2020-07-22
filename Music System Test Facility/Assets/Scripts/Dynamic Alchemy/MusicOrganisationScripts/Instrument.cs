using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Stores the audio clips of a specific instrument in a section. These inlude the low, medium and high energy versions of that instrument.
/// </summary>
[CreateAssetMenu(fileName = "New Instrument", menuName = "Dynamic Music/Instrument")]
public class Instrument : ScriptableObject
{
    public string instrumentName;

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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Eases the process of creating of new instruments. Used to categorise musicNodes within a Song.
/// </summary>
[CreateAssetMenu(fileName = "New Instrument", menuName = "Dynamic Music/Instrument")]
public class Instrument : ScriptableObject
{
    public string instrumentName;         // pointless right now
    //public MusicNode instrumentNodes;   // These are all the nodes linked to this instrument, stored in sectional order.
}

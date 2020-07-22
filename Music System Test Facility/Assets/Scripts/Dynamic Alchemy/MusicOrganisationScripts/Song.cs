using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores information about a single song. Holds links to all the individual song sections.
/// </summary>
[CreateAssetMenu(fileName = "New Song", menuName = "Dynamic Music/Song")]
public class Song : ScriptableObject
{
    public string songName;
    public SongSection[] sections;
}

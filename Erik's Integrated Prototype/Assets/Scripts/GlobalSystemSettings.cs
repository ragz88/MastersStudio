using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This will be used to store information that needs to be accessed system-wide. This way, if we want to change any big decisions,
/// we just need to change the settings in here.
/// </summary>
public class GlobalSystemSettings
{
    public enum AbilityCategories
    {
        Red,
        Green,
        Blue,
        Yellow
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores information and characteristics which are common among all abilities in a certain category.
/// </summary>
public class CategoryProperties : MonoBehaviour
{
    public GlobalSystemSettings.AbilityCategories Category;     // The category that all the information below applies to.

    [Header("Colours")]
    public Color textColour;   // This colour should be used for all text on menu pages relating to this category.
    public Color imageColour;  // This colour should be used for all ability display images on menu pages relating to this category.
}

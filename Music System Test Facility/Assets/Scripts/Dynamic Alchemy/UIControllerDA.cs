using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

/// <summary>
/// This allows all the Check Box elements to speak to the various scripts they're associated with.
/// </summary>
public class UIControllerDA : MonoBehaviour
{

    public GameObject alchemyVisualisations;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Switches visualisations to the opposite of their current state (either on or off)
    /// </summary>
    public void ToggleVisualisations()
    {
        alchemyVisualisations.SetActive(!alchemyVisualisations.activeSelf);
    }
}

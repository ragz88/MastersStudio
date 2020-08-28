using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This allows all the Check Box elements to speak to the various scripts they're associated with.
/// It will also represent some important stats through Text elements in the UI.
/// Used exclusively for testing.
/// </summary>
public class UIControllerDA : MonoBehaviour
{
    [Header("Main UI Containers")]
    public GameObject developerUI;
    public GameObject alchemyVisualisations;
    public GameObject statsContainer;

    [Header("Text Objects")]
    public Text currentTotalEnergy;
    public Text currentSectionEnergy;
    public Text currentDecayRate;
    public Text mediumThreshold;
    public Text highThreshold;

    public Text mobilityValue;
    public Text offenseValue;
    public Text defenseValue;

    public Text mobilityVol;
    public Text offenseVol;
    public Text defenseVol;
    public Text backgroundVol;

    public Text mobilityClip;
    public Text offenseClip;
    public Text defenseClip;
    public Text backgroundClip;

    public int debriefSceneIndex = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (statsContainer.activeSelf)
        {
            // Represent some general information through text
            currentTotalEnergy.text = "Current Playstyle Enegy: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetPlaystyleEnergy(), 2).ToString();
            currentSectionEnergy.text = "Current Section Enegy: " + MusicControllerDA.musicControllerInstance.GetCurrentSectionEnergy().ToString();
            currentDecayRate.text = "Current Rate of Decay: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetCurrentRateOfDecay(), 2).ToString();
            mediumThreshold.text = "Medium Threshold: " + MusicControllerDA.musicControllerInstance.medClipThreshold.ToString();
            highThreshold.text = "High Threshold: " + MusicControllerDA.musicControllerInstance.highClipThreshold.ToString();

            // Each specific category's energy level is represented through text
            mobilityValue.text = "Mobility: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetMobilityLevel(), 2).ToString();
            offenseValue.text  = "Offense: "  + System.Math.Round(MusicControllerDA.musicControllerInstance.GetOffenseLevel(), 2).ToString();
            defenseValue.text  = "Defense: "  + System.Math.Round(MusicControllerDA.musicControllerInstance.GetDefenseLevel(), 2).ToString();

            // The calculated volume (which will be lerped to) of each category is represented through text
            mobilityVol.text   = "Vol: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetMobilityVolume(), 2).ToString();
            offenseVol.text    = "Vol: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetOffenseVolume(), 2).ToString();
            defenseVol.text    = "Vol: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetDefenseVolume(), 2).ToString();
            backgroundVol.text = "Vol: " + System.Math.Round(MusicControllerDA.musicControllerInstance.GetBackgroundVolume(), 2).ToString();

            // We cache a reference to each of the categories main audio sources' clips
            AudioClip mobClip = MusicControllerDA.musicControllerInstance.musicSources[(int)MusicControllerDA.MusicSourceIndex.Mobility].clip;
            AudioClip offClip = MusicControllerDA.musicControllerInstance.musicSources[(int)MusicControllerDA.MusicSourceIndex.Offense].clip;
            AudioClip defClip = MusicControllerDA.musicControllerInstance.musicSources[(int)MusicControllerDA.MusicSourceIndex.Defense].clip;
            AudioClip backClip = MusicControllerDA.musicControllerInstance.musicSources[(int)MusicControllerDA.MusicSourceIndex.Background].clip;
            
            // ... to check if there is currently a clip equipped, and (if so) extract and display its name through text.
            // Get Mobility clip's name
            if (mobClip != null)
            {
                mobilityClip.text = "Clip: " + mobClip.name;
            }
            else
            {
                mobilityClip.text = "Clip: None";
            }

            // Get Offense clip's name
            if (offClip != null)
            {
                offenseClip.text = "Clip: " + offClip.name;
            }
            else
            {
                offenseClip.text = "Clip: None";
            }

            // Get Defense clip's name
            if (defClip != null)
            {
                defenseClip.text = "Clip: " + defClip.name;
            }
            else
            {
                defenseClip.text = "Clip: None";
            }

            // Get Defense clip's name
            if (backClip != null)
            {
                backgroundClip.text = "Clip: " + backClip.name;
            }
            else
            {
                backgroundClip.text = "Clip: None";
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            developerUI.SetActive(!developerUI.activeSelf);
        }
    }

    /// <summary>
    /// Switches visualisations to the opposite of their current state (either on or off)
    /// </summary>
    public void ToggleVisualisations()
    {
        alchemyVisualisations.SetActive(!alchemyVisualisations.activeSelf);
    }

    /// <summary>
    /// Switches statistics UI elements to the opposite of their current state (either on or off)
    /// </summary>
    public void ToggleStats()
    {
        statsContainer.SetActive(!statsContainer.activeSelf);
    }


    /// <summary>
    /// When off, the sections will just progress in numeric order. When on, the next section is chosen based on the currentplaystyle energy
    /// </summary>
    public void ToggleSectionControl()
    {
        MusicControllerDA.musicControllerInstance.SectionControl = !MusicControllerDA.musicControllerInstance.SectionControl;
    }

    /// <summary>
    /// When off, clips will just play their standard energy level. When on, they will dynamically switch between higher and 
    /// lower eneergy versions based on the current playstyle
    /// </summary>
    public void ToggleNodeLevels()
    {
        MusicControllerDA.musicControllerInstance.NodeLevels = !MusicControllerDA.musicControllerInstance.NodeLevels;
    }

    /// <summary>
    /// Takes player to the debreif scene when they press the continue button
    /// </summary>
    public void LoadDebrief()
    {
        SceneManager.LoadScene(debriefSceneIndex);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button alchemyButton;
    public Button prefabricatedButton;

    public Text alchemyText;
    public Text prefabricatedText;

    public Color buttonDisableColour;

    public int tutorialSceneNumber = 2;
    public int ppSceneNumber = 3;
    public int daSceneNumber = 4;

    // The quit buttons will only spawn in once these two variables are true
    public static bool prefabricatedPlayed = false;
    public static bool alchemyPlayed = false;

    // Start is called before the first frame update
    void Start()
    {
        if (TutorialController.tutComplete == false)
        {
            alchemyButton.interactable = false;
            prefabricatedButton.interactable = false;

            alchemyText.color = buttonDisableColour;
            prefabricatedText.color = buttonDisableColour;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    /// <summary>
    /// Loads the sce associated with the tutorialSceNumber
    /// </summary>
    public void LoadTutorial()
    {
        SceneManager.LoadScene(tutorialSceneNumber);
    }

    /// <summary>
    /// Loads the sce associated with the ppSceNumber
    /// </summary>
    public void LoadPrefabricatedPlay()
    {
        SceneManager.LoadScene(ppSceneNumber);
    }

    /// <summary>
    /// Loads the sce associated with the daSceNumber
    /// </summary>
    public void LoadDynamicAlchemy()
    {
        SceneManager.LoadScene(daSceneNumber);
    }

    /// <summary>
    /// Exits Game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}

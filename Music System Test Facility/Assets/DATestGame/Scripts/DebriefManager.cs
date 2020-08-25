using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebriefManager : MonoBehaviour
{
    public float fadeSpeed = 2;

    public Image daDebrief;          // shown when DA game finished
    public Image ppDebrief;          // shown when PP game finished
    public Image finalDebrief;       // shown when both finished
    public Image defaultImage;       // shown if something goes wrong

    Image imageToShow;               // This will be used to fade the selected image in

    // Start is called before the first frame update
    void Start()
    {
        if (MenuManager.alchemyPlayed && MenuManager.prefabricatedPlayed)
        {
            imageToShow = finalDebrief;
        }
        else if(MenuManager.alchemyPlayed)
        {
            imageToShow = daDebrief;
        }
        else if(MenuManager.prefabricatedPlayed)
        {
            imageToShow = ppDebrief;
        }
        else
        {
            imageToShow = defaultImage;
        }


        // We want to kill our old music manager - both so that the music stops and so that it doesn't
        // conflict with the other game's one if the player chooses a different game mode.
        GameObject oldMusicController = GameObject.Find("MusicController");

        if (oldMusicController != null)
        {
            Destroy(oldMusicController);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (imageToShow.color.a < 1)
        {
            imageToShow.color = imageToShow.color + new Color(0, 0, 0, fadeSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Takes us back to the game selection screen
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }


    /// <summary>
    /// Exits Game.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}

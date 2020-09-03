using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;


/// <summary>
/// A hard-coded class used to teach the player.
/// </summary>
public class TutorialController : MonoBehaviour
{
    public float fadeSpeed = 2;
    public Image[] tutPages;

    public int sceneToLoad = 0;

    public string newButtonCaption = "Get Started";       // The button text on the final tut page will change to this string
    public Text buttonText;

    public static bool tutComplete = false;               // Set to true once the tutorial has been finished

    public GameObject loadingBarObj;                      // Images used to represent the next scene's loading progress
    public Image loadingBar;                              // The actual filled image used to represent load percentage

    int currentTutPage = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // We don't want to fade anything out on the first page
        if (currentTutPage > 0)
        {
            // fade out old pages
            if (tutPages[currentTutPage - 1].color.a > 0)
            {
                tutPages[currentTutPage - 1].color = tutPages[currentTutPage - 1].color - new Color(0, 0, 0, fadeSpeed * Time.deltaTime);
            }
        }

        // We don't want to fade anything out on the last page
        if (currentTutPage < tutPages.Length)
        {
            // fade in new pages
            if (tutPages[currentTutPage].color.a < 1)
            {
                tutPages[currentTutPage].color = tutPages[currentTutPage].color + new Color(0, 0, 0, fadeSpeed * Time.deltaTime);
            }
        }

        // Some flavourful changing text
        if (currentTutPage == tutPages.Length - 1)
        {
            buttonText.text = newButtonCaption;
        }
    }


    /// <summary>
    /// moves to the next page of the tutorial. Destroys all of the current page's objects and switches the next page's ones on
    /// </summary>
    public void NextPage()
    {
        currentTutPage++;

        // Here, we activate our loading bar just before it's needed
        if (currentTutPage == tutPages.Length - 1)
        {
            loadingBarObj.SetActive(true);
        }

        // Here we'll go to the next scene
        if (currentTutPage == tutPages.Length)
        {
            tutComplete = true;

            // Load the next scene
            StartCoroutine(LoadAsynchronously(sceneToLoad));
        }
    }

    IEnumerator LoadAsynchronously (int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while(!operation.isDone)
        {
            // Because of the way loading asynch works, 0.9 actually represents when the scene is ready - 
            // so we adjust that value to represent 100% loaded
            float progress = Mathf.Clamp01 (operation.progress / 0.9f);
            loadingBar.fillAmount = progress;
            //Debug.Log("Load Progress: " + progress);

            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class DAGameManager : MonoBehaviour
{
    public float deathSlomoRate = 0.3f;               // Rate at which the game slows down after player death
    public float finalDeathSlomoSpeed = 0.05f;        // The timescale we will ultimately reduce to upon player death

    [HideInInspector]
    public float score = 0;                           // tracks player kills

    public DAPlayerController playerController;      
    public EnemySpawnController spawner;              // The script spawning enemies. Must be switched off when the player dies

    public GameObject gameOverTitle;                  // The big words Game Over - activated at the end of the game
    public Text scoreText;                            // The text showing the score at the end of the game
    Text gameOverText;                                // We want to edit the Game Over screen's text based on the game end conditions

    public Text[] fadeInTextOnDeath;                  // These text objects are designed to fade their alphas in after the player's death
    public float fadeInSpeed = 2f;                    // speed at which the above elements fade in

    public  bool playerDead = false;


    // The singleton ensures that only one instance of this script will ever exist at a given time. 
    // This makes the script easy to reference, and prevents inteference from multiple game managers.
    #region Singleton

    public static DAGameManager GameManagerInstance;

    private void Awake()
    {
        // We check if another instance of this exists - if it does, we destroy this. 
        // This ensures that only one of these objects can ever exist at a time.
        if (GameManagerInstance == null)
        {
            GameManagerInstance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gameOverText = gameOverTitle.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerDead)
        { 
            // fade in various game over texts
            for (int i = 0; i < fadeInTextOnDeath.Length; i++)
            {
                if (fadeInTextOnDeath[i].color.a < 1)
                {
                    fadeInTextOnDeath[i].color = (fadeInTextOnDeath[i].color + new Color(0, 0, 0, fadeInSpeed * Time.deltaTime));
                }
            }

            // activate slomo effects
            /*if (Time.timeScale > finalDeathSlomoSpeed)
            {
                Time.timeScale -= deathSlomoRate * Time.deltaTime;
            }*/
        }
    }


    /// <summary>
    /// To be called when the player collides with an enemy
    /// </summary>
    public void PlayerDeath()
    {
        spawner.enabled = false;         // prevents additional spawning and nullreference errors

        playerController.PlayerDeath();  // We tell the player that they've died

        playerDead = true;               // Tracks if the player is alive for slomo effects

        gameOverTitle.SetActive(true);

        scoreText.text = "Score " + score;
    }

    /// <summary>
    /// To be called when the player survives long enough to break the enemy spawners (#MakingBugsWorkForMe)
    /// </summary>
    public void PlayerWon()
    {
        spawner.enabled = false;         // prevents additional spawning and nullreference errors

        playerController.PlayerDeath();  // We tell the player that they've died

        playerDead = true;               // Tracks if the player is alive for slomo effects

        gameOverText.text = "YOU WON!";  // In this gameover instance, the player did well enough to be rewarded

        gameOverTitle.SetActive(true);

        scoreText.text = "Score " + score;
    }


    public void Replay()
    {
        // Reset any potentially persistent variables
        //Time.timeScale = 1;
        playerDead = false;
        score = 0;

        // Reset our music values
        MusicControllerDA.musicControllerInstance.SetMobilityLevel(0);
        MusicControllerDA.musicControllerInstance.SetOffenseLevel(0);
        MusicControllerDA.musicControllerInstance.SetDefenseLevel(0);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}

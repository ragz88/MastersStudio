using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public GameObject player;
    public float enemyMaxSpeed = 2;
    public float enemyMinSpeed = 0.4f;
    public float rotationSpeed = 3f;

    public LayerMask playerRayMask;          // We only want our rays to see the player

    public GameObject deathParticles;        // Particle system to be instantiated on the enemy's death
    public GameObject tripleDeathParticles;  // These will also be instantiated on enemy death if the player landed a triple kill

    DAPlayerController playerController;

    Vector2 enemyDirection;
    Rigidbody2D enemyBody;

    float enemySpeed;

    // This tracks how many enemies have collided with eachother in succession
    // At a certain point, so many spawn that this basically happends infinitely. Using this number,
    // we'll detect that point and give the game an appropriate ending.
    public static float enemyCollisions = 0;

    // Start is called before the first frame update
    void Start()
    {
        enemyBody = GetComponent<Rigidbody2D>();

        enemySpeed = Random.Range(enemyMinSpeed, enemyMinSpeed);
        
        // We cache a reference to our player controller
        playerController = player.GetComponent<DAPlayerController>();

        // We want the enemies to be attracted to the shield when it's present, so we'll check if the currentShield variable
        // in our player controller is null or not
        if (playerController.currentShield != null)
        {
            // and calculate our starting direction based on their position
            enemyDirection = (playerController.currentShield.transform.position - transform.position);
        }
        else // If it is null, we target the player
        {
            // and calculate our starting direction based on their position
            enemyDirection = (player.transform.position - transform.position);
        }

        // which we'll then use to set our initial velocity
        enemyBody.velocity = enemyDirection.normalized * enemySpeed;
    }

    // Update is called once per frame
    void Update()
    {
        // Makes the enemy spin - #JustAesthetics
        gameObject.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Cast a ray in the direction the enemy is headed. This is to check if it's about to hit the player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, enemyDirection, 2, playerRayMask);
        Debug.DrawLine(transform.position, enemyDirection, Color.cyan);

        // If it hits something...
        if (hit.collider != null)
        {
            // ...and that something is the player
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                // and the player is both dashing and hasn't been credit for doing this yet
                if (playerController.dashing && !playerController.specialPastDash)
                {
                    // We'll tell the player that they pulled off a special dash
                    playerController.specialPastDash = true;

                    if (!playerController.simulatePrefabPlay)
                    {
                        // And give them some extra points in mobility for doing so
                        MusicControllerDA.musicControllerInstance.AdjustMobilityLevel(playerController.dashingAwayFromEnemy);

                        // We also want to set the speed at which category levels decay back to a minimum
                        MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                    }
                }

                // We also reset our counter for enemies killing eachother, becuase this implies enemies are still in the main field
                enemyCollisions = 0;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Let's examine what killed this guy. If it was a bullet, we'll add some score to the offense level and start
        // the tracking for combos
        if (collision.gameObject.CompareTag("Bullet"))
        {
            DAGameManager.GameManagerInstance.score++;   // Increase the score

            if (playerController != null)
            {
                playerController.killCount++;                // We increase our combo kill counter
                playerController.killTimer = 0;              // And reset our timer that checks the interval between kills
                enemyCollisions = 0;                         // As well as reset our counter for enemies killing eachother

                // If the player has successfully completed the hattrick, we give them a bonus and spawn some feedback particles
                if (playerController.killCount >= 3)
                {
                    // reset the killcount for the next hattrick
                    playerController.killCount = 0;

                    // spawn some dope particles for feedback
                    playerController.SpawnKillComboParticles();
                    Instantiate(tripleDeathParticles, transform.position, Quaternion.identity);

                    if (!playerController.simulatePrefabPlay)
                    {
                        // award a larger point score
                        MusicControllerDA.musicControllerInstance.AdjustOffenseLevel(playerController.tripleKill);

                        // We also want to set the speed at which category levels decay back to a minimum
                        MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                    }
                }
                else  // Implies just a standard kill
                {
                    if (!playerController.simulatePrefabPlay)
                    {
                        MusicControllerDA.musicControllerInstance.AdjustOffenseLevel(playerController.shotEnemy);

                        // We also want to set the speed at which category levels decay back to a minimum
                        MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                    }
                }
            }
        }
        else
        {
            // But if it was the player, we trigger the end of the game
            if (collision.gameObject.CompareTag("Player"))
            {
                enemyCollisions = 0;
                DAGameManager.GameManagerInstance.PlayerDeath();
            }
            else if (collision.gameObject.CompareTag("Shield"))
            {
                DAGameManager.GameManagerInstance.score++;   // Increase the score
                enemyCollisions = 0;                   // We reset our counter for enemies killing eachother
            }
            else if (collision.gameObject.CompareTag("KillBox"))
            {
                enemyCollisions = 0;                   // We reset our counter for enemies killing eachother
            }
            // and if it was an enemy, we'll keep track of how many times they've killed eachother
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                // If that number exceeds a logical amount, the game is probably over
                enemyCollisions++; 

                if (enemyCollisions > 500)
                {
                    // Here we end the game with a win message
                    DAGameManager.GameManagerInstance.PlayerWon();
                }
            }
        }

        // Destroy the enemy
        Instantiate(deathParticles, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class DAPlayerController : MonoBehaviour
{
    public float playerSpeed;                      // Speed of the player's regular movement

    public float dashSpeed = 15;                   // How fast the player moves when dashing
    public float dashLength = 0.5f;                // How long the player's dash lasts

    public float shieldCooldown = 2.5f;            // How long the player must wait before they can spawn a second shield
    private float shieldTimer = 0;                 // Used to check if player can use the shield
    
    public GameObject bulletPrefab;
    public GameObject powerBulletPrefab;
    public GameObject shieldPrefab;
    public TrailRenderer[] dashTrails;

    private Rigidbody2D playerBody;                // Rigidbody attached to the player
    private Collider2D playerCollider;             // The collider attached to the player
    private Camera mainCam;                        // Stores a reference to the main camera
    private Vector2 dashVelocity;                  // Specialised Vector calculated based on the current movement direction and dash speed
     
    [HideInInspector]
    public ShieldController currentShield;         // Stores a reference to the last shield spawned

    public SpriteRenderer windowSprite;            // The little ship's window. Used to represent shield cooldown
    public GameObject shieldReadyParticles;        // Spawned when shield ready to be used again
    public GameObject tripleKillParticles;         // Spawned when the player gets 3 kills in quick succession

    public ParticleSystem tripleKillPowerupParticles;     // Switched on when a power shot is ready after a triple kill

    public GameObject playerDeathParticles;        // Spawned upon player death

    public Color windowGrey;                       // Standard color of window
    public Color windowBlue;                       // Color when shield is ready

    private bool shieldPowerReady = false;
    public bool gotTripleKill = false;             // Used to check if the player should fire a regular shot or power shot
    public int killCount = 0;                      // Used to track combos - more than 3 kills in quick succession gives a hattrick bonus
    [HideInInspector]
    public float killTimer = 0;                    // tracks how much time has past since the last kill
    public float killComboTime = 0.65f;            // The maximum time interval between kills to achieve the combo bonus

    [HideInInspector]
    public bool dashing = false;
    [HideInInspector]
    public bool specialPastDash = false;           // set to true when dashing dangerously past enemies
    [HideInInspector]
    public bool specialThroughDash = false;        // set to true when dashing dangerously through past enemies

    [Header("Category Point Values")]
    public float dashingThroughEnemy = 0.45f;
    public float dashingAwayFromEnemy = 0.3f;
    public float dashingPlain = 0.1f;

    public float shieldPresent = 0.1f;             // Constantly added to Defense level if shield is in the field
    public float shieldBlockedEnemy = 0.3f;        // Added to Defense level if shield kills and enemy
    public float shieldSavedPlayer = 0.55f;        // Added to defense level when shield kills an enemy with the player inside

    public float shotBullet = 0.1f;                // Standard increase just for shooting
    public float shotEnemy = 0.3f;                 // Points for shooting an enemy
    public float tripleKill = 0.6f;                // Big bonus points for landing 3 kills in quick succession


    #region Prefabricated Play Settings

    [Header("Prefabricated Play Settings")]
    // Rather than recode everything - I've set this all up in a way where we can simulate the simpler prefabricated play version with the
    // adjustment of some playtesting booleans. 
    public bool simulatePrefabPlay = false;        // When this is true, it changes the conditions that need to be met when activating
                                                   // and ability - forcing the controller to examine a set of cooldowns

    public float dashCoolDownTime = 1.5f;          // The time it takes before the player can use their dash again.
    public float powerShotCoolDownTime = 2f;       // The time it takes before the player can use their dash again.

    float dashTimer = 0;                           // Used to track the Dash cooldown
    float shootTimer = 0;                          // Used to track the PowerShot cooldown

    public GameObject dashReadyParticles;          // Particle SSystem to be spawned when the dash is ready
    public GameObject powerShotReadyParticles;     // Particle SSystem to be spawned when the dash is ready

    bool dashPowerReady = false;
    bool shootPowerReady = false;

    public Image dashBar;                          // The image being used as a loading bar for the Dash ability
    public Image shootBar;                         // The image being used as a loading bar for the PowerShot ability
    public Image shieldBar;                        // The image being used as a loading bar for the Shield ability

    public float lowEnergy = 0.76f;                // The musical energy level to set when something is recharging
    public float medEnergy = 1.5f;                 // The musical energy level to set when something is ready
    public float highEnergy = 2.99f;               // The musical energy level to set when something is in use

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        playerBody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        mainCam = Camera.main;

        if (simulatePrefabPlay)
        {
            // We inform our menu (using a static variable) that the PP game has been played
            MenuManager.prefabricatedPlayed = true;
        }
        else
        {
            // We inform our menu (using a static variable) that the DA game has been played
            MenuManager.alchemyPlayed = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Movement =============================================================
        // Find the player's movement input
        float xMovement = Input.GetAxis("Horizontal");
        float yMovement = Input.GetAxis("Vertical");

        // The make a directional vector with that information 
        // (We conserve direction, but make the magnitude 1)
        Vector2 moveDirection = new Vector2(xMovement, yMovement).normalized;


        // Dash ================================================================================================
            // If the previous dash is essentially done, we allow a new one to happen
            if ( (!simulatePrefabPlay && Input.GetButtonDown("Dash") && dashVelocity.magnitude < 0.1f) || 
                 (simulatePrefabPlay && dashPowerReady && Input.GetButtonDown("Dash")) )                  // These are the PP conditions
            {
                // we create a vector with magnitude dashSpeed in the player's current movement direction
                // this is automatically added to the player's velocity in the movement section
                dashVelocity = moveDirection * dashSpeed;

                // We also activate the dash trails for some visual feedback
                for (int i = 0; i < dashTrails.Length; i++)
                {
                    dashTrails[i].emitting = true;
                }

                dashing = true;

                // We finally want to set our collider to intangible
                playerCollider.isTrigger = true;


                // When this is a prefabricated Play game, we need to tell the system that the player used their dash
                if (simulatePrefabPlay)
                {
                    dashPowerReady = false;
                    
                    // We also want to make our colour a little brighter while this baby is active
                    dashBar.color = new Color(dashBar.color.r, dashBar.color.g, dashBar.color.b, 1);

                    // Set the mobility music level to High
                    MusicControllerDA.musicControllerInstance.SetMobilityLevel(highEnergy);
                }

            }

            if (dashVelocity.magnitude > 0.1f)
            {
                dashVelocity = Vector2.MoveTowards(dashVelocity, Vector2.zero, (1 / dashLength) * Time.deltaTime);
            }
            else
            {
                // We ensure the dash velocity has decreased completely
                dashVelocity = Vector2.zero;

                // And then switch off our trails
                for (int i = 0; i < dashTrails.Length; i++)
                {
                    dashTrails[i].emitting = false;
                }

                // We only want to run the following if this is the first fram after a completed dash - hence we check dashing
                if (dashing)
                {
                    // We want to check if anything special happened in this dash. If not, we'll increase our mobility level by
                    // just a small amount
                    if (specialPastDash || specialThroughDash)
                    {
                        // We need to reset these for the next dash
                        specialPastDash = false;
                        specialThroughDash = false;
                    }
                    else
                    {
                        if (!simulatePrefabPlay)
                        {
                            // Add a small participation prize to mobility
                            MusicControllerDA.musicControllerInstance.AdjustMobilityLevel(dashingPlain);

                            // We also want to set the speed at which category levels decay back to a minimum
                            MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                        }
                    }

                    // If this is the PP system, we need to reset our cooldown timer when the dash ends. We also want to reset our 
                    // button image
                    if (simulatePrefabPlay)
                    {
                        dashTimer = 0;
                        // We also want to make our colour a little duller while this is inactive, and reset the loading bar
                        dashBar.color =  new Color(dashBar.color.r, dashBar.color.g, dashBar.color.b, 0.4f);
                        dashBar.fillAmount = 0;

                        // Set the mobility music level to Low
                        MusicControllerDA.musicControllerInstance.SetMobilityLevel(lowEnergy);
                    }

                dashing = false;
                }

                // We finally want to set our collider to tangible again
                playerCollider.isTrigger = false;
            }

        // End Dash ============================================================================================


        // Update Player's their velocity based on that movement
        // Dash Velocity is a specialised vector calculated in the dash section
        playerBody.velocity = (moveDirection * playerSpeed) + dashVelocity;

        // End Movement =========================================================


        // Rotation ===========================================================================================================
        // Here we update the player's direction to face the mouse pointer
        transform.LookAt(mainCam.ScreenToWorldPoint(Input.mousePosition),Vector3.forward);

        // Next we make sure the only rotation happening is in the z axis, and correct it to align to the mouse correctly
        transform.eulerAngles = new Vector3(0, 0, -transform.eulerAngles.z);

        // End Rotation =======================================================================================================


        // Shoot ========================================================================
        // We spawn a bullet each time the player hits shoot
        // For the PP version, we spawn a powershot bullet everytime the player shoots when their cooldown is ready
        if ( (Input.GetButtonDown("Fire1") && !simulatePrefabPlay) || 
             (Input.GetButtonDown("Fire1") && simulatePrefabPlay && shootPowerReady) )      // Prefabricated Play Conditions
        {
            
            // We check if the player landed a triple kill, and so, we award a power shot
            if (gotTripleKill || simulatePrefabPlay)
            {
                // These settings updates only need happen if we aren't simulating prefabricated play
                if (!simulatePrefabPlay)
                {
                    gotTripleKill = false;

                    // Here we deactivate the looping property of our particles, allowing them to stop
                    var main = tripleKillPowerupParticles.main;
                    main.loop = false;

                    Instantiate(powerBulletPrefab, transform.position, transform.rotation);
                }
                else if (shootPowerReady)  // otherwise we check if the shoot power in the prefabricated play game is ready
                {
                    // We reset our timer and bool
                    shootPowerReady = false;
                    shootTimer = 0;

                    // and dull down our button colour
                    shootBar.color = new Color(shootBar.color.r, shootBar.color.g, shootBar.color.b, 0.4f);

                    // Set the offense music level to High
                    MusicControllerDA.musicControllerInstance.SetOffenseLevel(highEnergy);

                    GameObject shotObj = Instantiate(powerBulletPrefab, transform.position, transform.rotation) as GameObject;
                    shotObj.GetComponent<BulletMotion>().playerController = this;
                }
            }
            else // If not, we'll spawn a normal bullet
            {
                Instantiate(bulletPrefab, transform.position, transform.rotation);
            }

            if (!simulatePrefabPlay)
            {
                // Add a small participation prize to offense
                MusicControllerDA.musicControllerInstance.AdjustOffenseLevel(shotBullet);
            }
        }

        // End Shoot ====================================================================


        // Kill Combo ===================================================================
        // Let's check if the player's last kill happened more that killComboTime seconds ago
        if (killTimer > killComboTime)
        {
            // And if so, we reset their kill count
            killCount = 0;
        }
        else
        {
            // If not, we keep tracking the time
            killTimer += Time.deltaTime;
        }
        // End Kill Combo ===============================================================


        // Shield =======================================================================
        // We first check if the shield power has been indicated as ready
        if (shieldPowerReady)
        {
            if (Input.GetButtonDown("Shield"))
            {
                GameObject shield = Instantiate(shieldPrefab, transform.position, transform.rotation) as GameObject;
                currentShield = shield.GetComponent<ShieldController>();
                currentShield.playerController = this;

                // We also reset our cooldown timer each time a shield is spawned
                shieldTimer = 0;

                // ... set our boolean to false
                shieldPowerReady = false;

                // ... and represent the used ability with the ship's window
                windowSprite.color = windowGrey;

                // and, If this is the prefabricated play game, update the state of our button
                if (simulatePrefabPlay)
                {
                    // we'll brighten the button's colour a bit
                    shieldBar.color = new Color(shieldBar.color.r, shieldBar.color.g, shieldBar.color.b, 1f);


                    // Set the defense music level to High
                    MusicControllerDA.musicControllerInstance.SetDefenseLevel(highEnergy);
                }
            }
        }
        else  // if not, we increment our timer by the time the last frame took
        {
            if (!simulatePrefabPlay)
            {
                shieldTimer += Time.deltaTime;
            }
            else
            {
                // In the PP version, we only want to increase our timer once the old shield was destroyed
                if (currentShield == null && shieldTimer < shieldCooldown)
                {
                    shieldTimer += Time.deltaTime;
                    shieldBar.fillAmount = shieldTimer/shieldCooldown;

                    // we'll also dull the button's colour a bit once the shield has disappeared
                    shieldBar.color = new Color(shieldBar.color.r, shieldBar.color.g, shieldBar.color.b, 0.4f);
                }
            }

            // We then check if that was enough to make it past our cooldown time, and if there isn't still a shield in the field
            if (shieldTimer >= shieldCooldown && currentShield == null)
            {
                // and represent the cooldown visually using the window on the little triangle ship, and our particles, if that was the case
                windowSprite.color = windowBlue;
                Instantiate(shieldReadyParticles, windowSprite.transform.position, Quaternion.identity);

                // as well as update our boolean value
                shieldPowerReady = true;

                // and, If this is the prefabricated play game, update the state of our button
                if (simulatePrefabPlay)
                {
                    shieldBar.fillAmount = 1;

                    // we'll brighten the button's colour a bit
                    shieldBar.color = new Color(shieldBar.color.r, shieldBar.color.g, shieldBar.color.b, 0.85f);

                    // Set the defense music level to Medium
                    MusicControllerDA.musicControllerInstance.SetDefenseLevel(medEnergy);
                }
            }
        }

        // End Shield ===================================================================


        // PP Cooldown Timers ===================================================================================================================
        #region PP Cooldowns and Particles

        // We only want to run the following if we're simulating a prefabricated play controller
        if (simulatePrefabPlay)
        {
            // We only want to check our dash timer information when the power is no longer readied
            if (!dashPowerReady)
            {
                // We only want to start counting after the player has finished the dash
                if (!dashing)
                {
                    // We check whether the timer has finished counting or not
                    if (dashTimer > dashCoolDownTime)
                    {
                        // this implies that a dash was finished and that the timer has finished counting
                        dashPowerReady = true;
                        dashBar.fillAmount = 1;

                        // We also want to make our colour a little brighter
                        dashBar.color =  new Color(dashBar.color.r, dashBar.color.g, dashBar.color.b, 0.85f);

                        // Lets show some cool partiles too
                        Instantiate(dashReadyParticles, transform.position, Quaternion.identity);

                        // Set the mobility music level to Medium
                        MusicControllerDA.musicControllerInstance.SetMobilityLevel(medEnergy);
                    }
                    else
                    {
                        // This implies the timer still needs to tick down a bit
                        dashTimer += Time.deltaTime;
                        dashBar.fillAmount = dashTimer/dashCoolDownTime;
                    }
                }
            }

            // We only want to check our power shot timer information when the power is no longer readied
            if (!shootPowerReady)
            {
                // We check whether the timer has finished counting or not
                if (shootTimer > powerShotCoolDownTime)
                {
                    // this implies that the timer has finished counting
                    shootPowerReady = true;

                    // So we'll ensure our bar is full and brighten the colour a bit
                    shootBar.fillAmount = 1;
                    shootBar.color =  new Color(shootBar.color.r, shootBar.color.g, shootBar.color.b, 1f);

                    // We also want to spawn some feedback particles
                    Instantiate(powerShotReadyParticles, transform.position, Quaternion.identity);

                    // Set the offense music level to Medium
                    MusicControllerDA.musicControllerInstance.SetOffenseLevel(medEnergy);
                }
                else
                {
                    // This implies the timer still needs to tick down a bit
                    shootTimer += Time.deltaTime;
                    shootBar.fillAmount = shootTimer / powerShotCoolDownTime;
                }
            }
        }


        #endregion
        // end PP Cooldown Timers ===============================================================================================================

    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (dashing && !simulatePrefabPlay)      // we don't want to do this if it's a prefabricated play manager
        {
            // This implies we've dashed through an enemy - this awesome feat will give a lot of mobility points
            if (collision.CompareTag("Enemy") && !specialThroughDash)
            {
                // We indicate that this dash was special
                specialThroughDash = true;

                if (!simulatePrefabPlay)
                {
                    // and then adjust our mobility level
                    MusicControllerDA.musicControllerInstance.AdjustMobilityLevel(dashingThroughEnemy);

                    // We also want to set the speed at which category levels decay back to a minimum
                    MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                }
            }

            // We also reset our counter for enemies killing eachother, becuase this implies there are still enemies in the main field
            EnemyController.enemyCollisions = 0;                   
        }
    }


    /// <summary>
    /// Activates the feedback particles which indicate the player has a power shot (as a reward for landing a triple kill)
    /// </summary>
    public void SpawnKillComboParticles()
    {
        //Instantiate(tripleKillParticles, transform.position, Quaternion.identity);

        // Let's indicate that the player just landed a triple kill
        gotTripleKill = true;

        // Here we activate the looping property of our triple kill feedback particles, and make sure they're playing
        var main = tripleKillPowerupParticles.main;
        main.loop = true;

        tripleKillPowerupParticles.Play();
    }


    /// <summary>
    ///  To be called when the player collides with an enemy
    /// </summary>
    public void PlayerDeath()
    {   
        // We'll make some juicy visual effects, then destroy the player
        if (!DAGameManager.GameManagerInstance.playerDead)
        {
            Instantiate(playerDeathParticles, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        
    }
}

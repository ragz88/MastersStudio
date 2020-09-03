using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{

    public float decayRate = 2;                    // The rate at which the shield shrinks
    public float decayDelay = 1;                   // seconds before shrinking starts
    public float destroySize = 0.2f;               // the size at which the shield should die (after shrinking for a while)

    public GameObject shieldBreakParts;            // Particle system to spawn when shield breaks

    float timer = 0;
    CircleCollider2D shieldCollider;               // the collider on the shield

    [Header("Sound Effect Settings")]
    public GameObject tempAudioEffect;             // Specialised GameObject designed to die after playing its sound
    public AudioClip breakSoundEffect;             // The sound that should play when the shield breaks

    public float breakVolume = 1;                  // Volume of shield break sound effect

    [HideInInspector]
    public DAPlayerController playerController;    // cached reference to our player

    // Start is called before the first frame update
    void Start()
    {
        shieldCollider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.simulatePrefabPlay)
        {
            // While our shield exists in the field, we want it to give a nominal amount towards the defense level
            MusicControllerDA.musicControllerInstance.AdjustDefenseLevel(playerController.shieldPresent * Time.deltaTime);
        }

        // first we check if decayDelay seconds have passed
        if (timer < decayDelay)
        { 
            // if not, we increase our timing variable by the length of the previous frame
            timer += Time.deltaTime;
        }
        else // Otherwise, we begin the shrinking
        { 
            // Once the local scale is small enough, we'll destroy the shield and allow the player to spawn a new one
            if (transform.localScale.y < destroySize)
            {
                Instantiate(shieldBreakParts, transform.position, Quaternion.identity);

                if (playerController.simulatePrefabPlay)
                {
                    // Set the defense music level to Low
                    MusicControllerDA.musicControllerInstance.SetDefenseLevel(playerController.lowEnergy);
                }

                // Play our shield's break sound effect
                GameObject tempSourceObj = Instantiate(tempAudioEffect) as GameObject;
                AudioSource tempAudioSource = tempSourceObj.GetComponent<AudioSource>();
                tempAudioSource.clip = breakSoundEffect;
                tempAudioSource.volume = breakVolume;
                tempAudioSource.GetComponent<TempSoundEffect>().effectPlayed = true;
                tempAudioSource.Play();

                // and destroy it
                Destroy(gameObject);
            }
            else  // If it hasn't reached that point yet, we continue shrinking it
            {
                transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, decayRate * Time.deltaTime);

                // I increase decayRate to create a more appealing visual effect
                decayRate += (Time.deltaTime * 1);
            }
            
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Instantiate(shieldBreakParts, transform.position, Quaternion.identity);

            if (playerController != null)
            {
                // This implies that the player is mostly within the shield
                if (Vector3.Distance(playerController.transform.position, transform.position) < shieldCollider.radius)
                {
                    if (!playerController.simulatePrefabPlay)
                    {
                        // So we give a huge bonus to defense level
                        MusicControllerDA.musicControllerInstance.AdjustDefenseLevel(playerController.shieldSavedPlayer);

                        // We also want to set the speed at which category levels decay back to a minimum
                        MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                    }
                }
                else
                {
                    if (!playerController.simulatePrefabPlay)
                    {
                        // Otherwise, we still give a moderate bonus for killing an enemy with the shield
                        MusicControllerDA.musicControllerInstance.AdjustDefenseLevel(playerController.shieldBlockedEnemy);

                        // We also want to set the speed at which category levels decay back to a minimum
                        MusicControllerDA.musicControllerInstance.ResetRateOfDecay();
                    }
                }

                if (playerController.simulatePrefabPlay)
                {
                    // Set the defense music level to Low
                    MusicControllerDA.musicControllerInstance.SetDefenseLevel(playerController.lowEnergy);
                }
            }

            // Play our shield's break sound effect
            GameObject tempSourceObj = Instantiate(tempAudioEffect) as GameObject;
            AudioSource tempAudioSource = tempSourceObj.GetComponent<AudioSource>();
            tempAudioSource.clip = breakSoundEffect;
            tempAudioSource.volume = breakVolume;
            tempAudioSource.GetComponent<TempSoundEffect>().effectPlayed = true;
            tempAudioSource.Play();

            // and destroy it
            Destroy(gameObject);
        }
    }

}

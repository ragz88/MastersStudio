﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControllerDA : MonoBehaviour
{
    public float minLevelValue = 0.25f;     // Minimum value any of the categories can ever sink to.
    public float maxLevelValue = 2.99f;     // Maximum value any of the categories can ever rise to.
    
    [SerializeField][Range(0.25f, 2.99f)]
    private float OffenseLevel = 1;         // Increases each time player lands a hit/shot/deals damage ().

    [SerializeField][Range(0.25f, 2.99f)]
    private float DefenseLevel = 1;         // Increases each time player blocks/evades/parries a hit ().

    [SerializeField][Range(0.25f, 2.99f)]
    private float MobilityLevel = 1;        // Increases each time player jumps/dashes/slides etc. ().

    public float categoryMax = 2f;          // Max amount a category can be

    // Stores a reference to each AudioSource used to play music and the temp AudioSources used for transitioning between musical states.
    // The order these sources are assigned with is specific, and represented below.
    // I considered making a struct and storing each with an accompanying name, but this was more efficient in the long run (that requires looping through the array).
    public AudioSource[] musicSources;

    // Used to improve readability later. Each represents the index of their respective audio source in our MusicSources array
    public enum MusicSourceIndex
    {
        Mobility,
        MobTemp,                             // Temps are used to seamlessly transition between two songs/musical states
        Offense,
        OffTemp,
        Defense,
        DefTemp,
        Background,
        BackTemp,
        SetPieceSource                       // Used to play set pieces of music in cutscenes/boss fights/transitions etc.
    }

    public GameObject tempFadeOutSource;     // A special prefab that plays a sound with constantly decreasing volume until it deletes itself.


    Song currentSong;                        // The current song in this area (collection of MusicNodes)
    int currentSongSection = 0;              // Stores the index of the current song section in the 2D sortedNode array
    float currentSectionLength = 0;          // Stores the length (in seconds) of the current song section playing
    float currentSectionTimer = 0;           // used to check if the current section is complete. Increases each frame.

    // We initialise these to -1 in case the player has no ability (and thus, no instrument) equipped in that slot

    int mobilityInstrumentIndex = -1;        // The index of the player's mobility ability's inherent instrument within the song's 2D sortedNodes array.
    int OffenseInstrumentIndex  = -1;        // The index of the player's mobility ability's inherent instrument within the song's 2D sortedNodes array.
    int DefenseInstrumentIndex  = -1;        // The index of the player's mobility ability's inherent instrument within the song's 2D sortedNodes array.


    // These are specifically for playtesting now - TO BE REMOVED ONCE WE DECIDE WHAT WE LIKE
    #region Playtesting Bools
    [HideInInspector]
    public bool SectionControl = true;    // Whether or not sections should change based on their energy in relation to the gampley

    [HideInInspector]
    public bool NodeLevels = true;        // Whether or not nodes should change between their low, med and high clips, or just use a default clip

    #endregion


    // The singleton ensures that only one instance of this script will ever exist at a given time. 
    // This makes the script easy to reference, and prevents inteference from multiple music controllers.
    #region Singleton

    public static MusicControllerDA musicControllerInstance;

    private void Awake()
    {
        // We check if another instance of this exists - if it does, we destroy this. 
        // This ensures that only one of these objects can ever exist at a time.
        if (musicControllerInstance == null)
        {
            musicControllerInstance = this;
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
        DontDestroyOnLoad(gameObject);

        LoadNewSong(currentSong);

        // currentSong.InitialiseSong();
        // currentSong.LogArrayToConsole();
    }

    // Update is called once per frame
    void Update()
    {
        #region Temporary Testing Controls
        // ----------------------------------------------
        if (Input.GetKey(KeyCode.W))
        {
            MobilityLevel += 0.05f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            MobilityLevel -= 0.05f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            OffenseLevel += 0.05f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            OffenseLevel -= 0.05f;
        }
        if (Input.GetMouseButton(0))
        {
            DefenseLevel += 0.05f;
        }
        if (Input.GetMouseButton(1))
        {
            DefenseLevel -= 0.05f;
        }

        MobilityLevel = Mathf.Clamp(MobilityLevel, 0.25f, 2.99f);
        OffenseLevel = Mathf.Clamp(OffenseLevel, 0.25f, 2.99f);
        DefenseLevel = Mathf.Clamp(DefenseLevel, 0.25f, 2.99f);

        #endregion

        if (SectionDonePlaying())
        {
            NextSongSection();
        }

        //CheckLowMedHigh();


        // LERP SONG VOLUME - DON'T SET THEM!!!
    }



    /// <summary>
    /// Transitions the current song into the new song for the next area. 
    /// </summary>
    public void LoadNewSong(Song newSong)
    {
        newSong.InitialiseSong();
    }


    /// <summary>
    /// Examines current ability category levels as well as energy levels of the sectiona that can naturally follow the
    /// current section and chooses an appropriate section to transition into.
    /// </summary>
    public void NextSongSection()
    {
        if (SectionControl)
        {

        }
        else
        {
            // If we aren't using section control, we'll just load the next song section in the array.
            // The mod function below makes the number automatically cycle back to 0 if it was about to go out of the array bounds
            LoadSongSection( (currentSongSection + 1) % currentSong.finalSection );    
        }
    }


    /// <summary>
    /// Loads a specific section of a song using it's index from the current song's 2D sortedNode array.
    /// </summary>
    /// <param name="sectionToLoad">The index of the section to load in the song's 2D sortedNode array</param>
    public void LoadSongSection(int sectionToLoad)
    {
        currentSongSection = sectionToLoad;    // first we update our cached index

        // We cache the length of the current section for easy reference. As all of the section's clips are the same length,
        // we can just look at 1 of them to extract the length we're looking for.
        currentSectionLength = currentSong.sortedNodes[0, currentSongSection].lowClip.length;
        
        // We reset the timer, so it can start timing the new section that was just loaded.
        currentSectionTimer = 0;

        // =======================================================================================================================================================================
        // Find the instruments linked to the currently equipped abilities
        // Extract their indeces and store them
        // Load the relative audio clips into the manager's Audio sources and play them

        // Write the 
    }


    /// <summary>
    /// Checks if the current song section is still busy playing or not.
    /// </summary>
    /// <returns>Returns true if the section has played completely</returns>
    public bool SectionDonePlaying()
    {
        // We check if our timer's been running for the full length of the song (give or take a frame) *************************************************
        if (currentSectionLength > (currentSectionTimer + Time.deltaTime))       
        {
            currentSectionTimer += Time.deltaTime;            // If not, we increase it by the frame length and return false
            return false;
        }
        else
        {
            return true;                                      // if so, the section is complete and we can return true
        }
    }


    /// <summary>
    /// Considers the category of an ability, as well as the instruments it's linked to, and fade's out that category's current music,
    /// while fading in the new ability's associated music.
    /// </summary>
    /// <param name="newAbility">Ability that was just equipped</param>
    public void EquipInstrument(Ability newAbility)
    {
        Instrument newInstrument = null;

        for (int i = 0; i < newAbility.abilityInstruments.Length; i++)
        {
            if (newAbility.abilityInstruments[i].specificSong == currentSong)         // Finds the correct song/instrument pair to match current song
            {
                newInstrument = newAbility.abilityInstruments[i].abilityInstrument;   // and caches the instrument the new ability should be playing in this song
                break;                                                                // since we have what we want, we can leave the loop
            }
        }

        int newInstrumentIndex = -1;                                                  // The index of the new instrument in our 2d sorted node array. -1 if no new instrument is found

        if (newInstrument != null)                                                    // We make sure that an InstrumentSongPair was actually found
        {
            // if a song-instrument pair WAS found, we'll find the index of the relevent instrument in our 2d sorted node array.
            // We loop through the instruments in our sorted 2D array to find the new instrument, and store it's index.
            for (int i = 0; i < currentSong.sortedNodes.GetLength(0); i++)
            {
                if (currentSong.sortedNodes[i, currentSongSection].nodeInstrument == newInstrument)
                {
                    newInstrumentIndex = i;
                    break;
                }
            }
        }
        else                                                                          // if one wasn't found, we throw an error and allow the instrument index to get set to -1
        {
            Debug.LogWarning("Ability " + newAbility.name + " has no instrument assigned for the " + currentSong.name + " song." );
        }

        
        // Next, we figure out which category this new instrument belongs to, update that category's audio sources and fade out the music that was
        // initially playing.
        switch (newAbility.abilityCategory)
        {
            case Ability.AbilityCategory.Mobility:

                FadeOut(musicSources[(int)MusicSourceIndex.Mobility], 2);                       // Creates a temporary object that will gently fade this instrument away
                mobilityInstrumentIndex = newInstrumentIndex;                                   // Updates the current mobility instrument index
                musicSources[(int)MusicSourceIndex.Mobility].volume = 0;                        // This will give an automatic fade-in effect, 
                                                                                                // due to how volume responds to category levels in Update()

                if (mobilityInstrumentIndex == -1)                                              // implies no instrument equipped/found
                {
                    musicSources[(int)MusicSourceIndex.Mobility].Stop();
                    musicSources[(int)MusicSourceIndex.Mobility].clip = null;
                }
                else                                                                            // Implies there IS a new clip to play
                {
                    musicSources[(int)MusicSourceIndex.Mobility].clip =
                      currentSong.sortedNodes[newInstrumentIndex, currentSongSection].lowClip;  // Assigns the new clip to the correct AudioSource
                    musicSources[(int)MusicSourceIndex.Mobility].Play();
                }
                
                break;

            case Ability.AbilityCategory.Offense:

                FadeOut(musicSources[(int)MusicSourceIndex.Offense], 2);                        // Creates a temporary object that will gently fade this instrument away
                OffenseInstrumentIndex = newInstrumentIndex;                                    // Updates the current offense instrument index
                musicSources[(int)MusicSourceIndex.Offense].volume = 0;                         // This will give an automatic fade-in effect, 
                                                                                                // due to how volume responds to category levels in Update()

                if (OffenseInstrumentIndex == -1)                                               // implies no instrument equipped/found
                {
                    musicSources[(int)MusicSourceIndex.Offense].Stop();
                    musicSources[(int)MusicSourceIndex.Offense].clip = null;
                }
                else                                                                            // Implies there IS a new clip to play
                {
                    musicSources[(int)MusicSourceIndex.Offense].clip =
                      currentSong.sortedNodes[newInstrumentIndex, currentSongSection].lowClip;  // Assigns the new clip to the correct AudioSource
                    musicSources[(int)MusicSourceIndex.Offense].Play();
                }

                break;

            case Ability.AbilityCategory.Defense:

                FadeOut(musicSources[(int)MusicSourceIndex.Defense], 2);                        // Creates a temporary object that will gently fade this instrument away
                DefenseInstrumentIndex = newInstrumentIndex;                                    // Updates the current defense instrument index
                musicSources[(int)MusicSourceIndex.Defense].volume = 0;                         // This will give an automatic fade-in effect, 
                                                                                                // due to how volume responds to category levels in Update()

                if (DefenseInstrumentIndex == -1)                                               // implies no instrument equipped/found
                {
                    musicSources[(int)MusicSourceIndex.Defense].Stop();
                    musicSources[(int)MusicSourceIndex.Defense].clip = null;
                }
                else                                                                            // Implies there IS a new clip to play
                {
                    musicSources[(int)MusicSourceIndex.Defense].clip =
                      currentSong.sortedNodes[newInstrumentIndex, currentSongSection].lowClip;  // Assigns the new clip to the correct AudioSource
                    musicSources[(int)MusicSourceIndex.Defense].Play();
                }

                break;
        }
    }



    /// <summary>
    /// Creates a temporary AudioSource that will fade the given source's clip out gently, then delete itself.
    /// <br></br>Note that this does not stop the source given to it. 
    /// </summary>
    public void FadeOut(AudioSource sourceToFadeOut, float fadeSpeed)
    {
        // First we need to intantiate the temporary audio fade out prefab - we'll put it where the initial source was, just to keep a consistent 
        // 3D sound (if that becomes a factor)
        GameObject tempAudioGObj = Instantiate(tempFadeOutSource, Vector3.zero, Quaternion.identity, sourceToFadeOut.transform) as GameObject;

        // Then we just tell the new TempAudioFadeOut component to initialise with the given source and speed
        tempAudioGObj.GetComponent<TempAudioFadeOut>().InitialiseFadeOut(sourceToFadeOut, fadeSpeed);
    }



    public void RemoveInstrument()
    {

    }






    /// <summary>
    /// Sets the current Mobility level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for mobility Level</param>
    public void SetMobilityLevel(float newLevel)
    {
        MobilityLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
        UpdateMobilityMusic();
    }

    /// <summary>
    /// Considers current Mobility Level and changes music volume and clip accordingly.
    /// </summary>
    public void UpdateMobilityMusic()
    {

    }



    /// <summary>
    /// Sets the current Offense level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for Offense Level</param>
    public void SetOffenseLevel(float newLevel)
    {
        OffenseLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
        UpdateOffenseMusic();
    }

    /// <summary>
    /// Considers current Offense Level and changes music volume and clip accordingly.
    /// </summary>
    public void UpdateOffenseMusic()
    {

    }


    /// <summary>
    /// Sets the current Defense level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for Defense Level</param>
    public void SetDefenseLevel(float newLevel)
    {
        DefenseLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
        UpdateDefenseMusic();
    }

    /// <summary>
    /// Considers current Defense Level and changes music volume and clip accordingly.
    /// </summary>
    public void UpdateDefenseMusic()
    {

    }


    // The Level values are made private to ensure they are properly clamped everytime they get set to something new
    #region Getters (Functions used to access private values)

    /// <summary>
    /// Returns current MobilityLevel of MusicManager.
    /// </summary>
    /// <returns>Current Mobility Level</returns>
    public float GetMobilityLevel()
    {
        return MobilityLevel;
    }

    /// <summary>
    /// Returns current Offense Level of MusicManager.
    /// </summary>
    /// <returns>Current Offense Level</returns>
    public float GetOffenseLevel()
    {
        return OffenseLevel;
    }

    /// <summary>
    /// Returns current Defense Level of MusicManager.
    /// </summary>
    /// <returns>Current Defense Level</returns>
    public float GetDefenseLevel()
    {
        return DefenseLevel;
    }

    #endregion

}

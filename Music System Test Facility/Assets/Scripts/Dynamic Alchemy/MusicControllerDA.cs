using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;
using System.Transactions;

public class MusicControllerDA : MonoBehaviour
{
    public float dynamicMusicVolume = 0;
    
    public float minLevelValue = 0.25f;                // Minimum value any of the categories can ever sink to.
    public float maxLevelValue = 2.99f;                // Maximum value any of the categories can ever rise to.
    public float categoryMax = 2f;                     // The value at which a category will hit max energy and volume.
    public float standardMusicFadeSpeed = 0.2f;        // Speed at which songs/clips fade out and fade in
    public float standardVolumeMorphSpeed = 0.2f;      // Speed at which volume changes in response to changing player energy

    public float decayRate = 0.5f;                     // The rate at which each category level decays when not in use


    [Header("Current Category Energy Levels")]

    [SerializeField][Range(0.25f, 2.99f)]
    private float MobilityLevel = 1;          // Increases each time player jumps/dashes/slides etc. ().

    [SerializeField][Range(0.25f, 2.99f)]
    private float OffenseLevel = 1;           // Increases each time player lands a hit/shot/deals damage ().

    [SerializeField][Range(0.25f, 2.99f)]
    private float DefenseLevel = 1;           // Increases each time player blocks/evades/parries a hit ().

    

    [Header("Thresholds and Ratios")]
    public float medClipThreshold = 1.3f;     // The point at which the low clip of a node will stop playing, and the medium one will start playing
    public float highClipThreshold = 3;       // The point at which the med clip of a node will stop playing, and the high one will start playing  -- For this testing ground, this is still out of the possible range
    // Note - this is modelled around a schmitt trigger. I.e. there is a small amount of wiggle room in which the clip won't immediately
    // switch back to it's original state - to smooth out the experience overall.
    public float medLowerThreshold = 0.95f;   // The point at which the med clip of a node will stop playing, and the low one will start playing
    public float highLowerThreshold = 2.7f;   // The point at which the med clip of a node will stop playing, and the low one will start playing

    public float lowClipMin   = 0.0f;         // The volume (percent) that a source should use when at the lowest energy level.
    public float lowClipMax   = 0.4f;         // The max volume a lowEnergy clip will play at. 
    public float medClipMin   = 0.5f;         // The minimum volume a Medium Energy clip will play at.
    public float medClipMax  = 0.85f;         // The max volume a Medium Energy clip will play at.
    public float highClipMin = 0.97f;         // The minimum volume a High Energy clip will play at.
    public float highClipMax    = 1f;         // The max volume a High Energy clip will play at.

    private float mobilityVolume = 0;         // The mobility dynamic music sources will lerp their volumes to this value smoothly
    private float offenseVolume  = 0;         // The offense dynamic music sources will lerp their volumes to this value smoothly
    private float defenseVolume  = 0;         // The defense dynamic music sources will lerp their volumes to this value smoothly


    private float playstyleEnergy = 0;        // Sum of all three energy levels, each with a max value of categoryMax
                                              // Note that the 0.99 buffer is just for timing purposes - the value used for logic will never be higher that 
                                              // categoryMax

    [Header("Audio Sources and Components")]
    // Stores a reference to each AudioSource used to play music and the temp AudioSources used for transitioning between musical states.
    // The order these sources are assigned with is specific, and represented below.
    // I considered making a struct and storing each with an accompanying name, but this was more efficient in the long run (that requires looping through the array).
    public AudioSource[] musicSources;

    // Used to improve readability later. Each represents the index of their respective audio source in our MusicSources array
    public enum MusicSourceIndex
    {
        Mobility,
        MobTemp,                               // Temps are used to seamlessly transition between two songs/musical states
        Offense,
        OffTemp,
        Defense,
        DefTemp,
        Background,
        BackTemp,
        SetPieceSource                         // Used to play set pieces of music in cutscenes/boss fights/transitions etc.
    }

    // This is used to edit the volume, among other qualities, of our music Audio sources all at once
    public AudioMixer MusicMasterMixer;

    public GameObject tempFadeOutSource;       // A special prefab that plays a sound with constantly decreasing volume until it deletes itself.


    [Header("Custom Storage Objects")]

    public Song currentSong;                   // The current song in this area (collection of MusicNodes)
    int currentSongSection = 0;                // Stores the index of the current song section in the 2D sortedNode array
    float currentSectionLength = 0;            // Stores the length (in seconds) of the current song section playing
    float currentSectionTimer = 0;             // used to check if the current section is complete. Increases each frame.

    // We initialise these to -1 in case the player has no ability (and thus, no instrument) equipped in that slot

    int MobilityInstrumentIndex = -1;          // The index of the player's mobility ability's inherent instrument within the song's 2D sortedNodes array.
    int OffenseInstrumentIndex  = -1;          // The index of the player's offense ability's inherent instrument within the song's 2D sortedNodes array.
    int DefenseInstrumentIndex  = -1;          // The index of the player's defense ability's inherent instrument within the song's 2D sortedNodes array.
    int BackgroundInstrumentIndex = -1;        // The index of the background clips within the song's 2D sortedNodes array.


    public Instrument backgroundInstrument;

    // Used to access the list of currently equipped abilities and find instrument-song pairs when a song changes
    public EquippedAbilitiesController equippedAbilitiesController;

    // These are specifically for playtesting now - TO BE REMOVED ONCE WE DECIDE WHAT WE LIKE
    #region Playtesting Bools
    [HideInInspector]
    public bool SectionControl = true;    // Whether or not sections should change based on their energy in relation to the gampley

    [HideInInspector]
    public bool NodeLevels = true;        // Whether or not nodes should change between their low, med and high clips, or just use a default clip

    public bool simulatePrefabricatedPlay = false;  // Whether we want this to act like a Dynamic Alchemy System, or PP system
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
        // We don't want our music to stop when jumping across scenes - we'll smoothly transition it
        DontDestroyOnLoad(gameObject);

        //We want to get the current mixer setting for volume - later we'll get this value from a PlayerPrefs file
        MusicMasterMixer.GetFloat("DynamicMusicVolume", out dynamicMusicVolume);

        LoadNewSong(currentSong);
    }

    // Update is called once per frame
    void Update()
    {

        #region Temporary Testing Controls
        /*
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

        */
        #endregion

        if (!simulatePrefabricatedPlay)
        {
            // We want each of our categories to decay naturally over time
            AdjustMobilityLevel(-decayRate * Time.deltaTime);
            AdjustOffenseLevel(-decayRate * Time.deltaTime);
            AdjustDefenseLevel(-decayRate * Time.deltaTime);
        }


        // We check if the current section is done, then activate an algorithm to figure out what the next section should be
        if (SectionDonePlaying())
        {
            NextSongSection();
        }

        
        // If the player chooses to here changing energy in the individual clips (Scale Overlays), then we calculate dynamic
        // volumes and play clips based on the player's playstyle energy.
        if (NodeLevels)
        {
            UpdateClipEnergies();
        }
        

        // Here, we increase current mixer's volume to the user-defined max level if it's too low =================================
        // This temporary float is used to extract and examine the current volume of our dynamic music in our music mixer
        float currentDynamicMusicVolume = 0;
        MusicMasterMixer.GetFloat("DynamicMusicVolume", out currentDynamicMusicVolume);

        // Let's check if the current volume is high enough - if not, we'll increase it gradually.
        if (currentDynamicMusicVolume < dynamicMusicVolume)
        {
            MusicMasterMixer.SetFloat("DynamicMusicVolume", currentDynamicMusicVolume + (Time.deltaTime*(standardMusicFadeSpeed * 100)));
        }
        // ========================================================================================================================
    }



    /// <summary>
    /// Transitions the current song into the new song for the next area. 
    /// </summary>
    public void LoadNewSong(Song newSong)
    {
        newSong.InitialiseSong();

        // Next block may be redundant - delete after testing
        // Here we check that this isn't the first song we're loading, and if not we slowly fade the current song out
        /*if (currentSong != null)
        {
            FadeOutCurrentSong();
        }*/

        // Now that the old song is safely fading away, we can update the currently cached song
        currentSong = newSong;

        // Once the new song is loaded, we'll need to reload our instrument/song pairs, as the relevent instruments for each ability 
        // will potentially change from song to song. These are specifically the instruments related to abilities - not background tracks
        InitialiseNewSongInstruments();


        // After initialising a song, we also want to cache any background tracks that should always be playing 
        // =============================================================================================================================
        // We'll start by checking if any backing track is already playing
        if (BackgroundInstrumentIndex != -1)                                       // implies background instrument exists
        {
            // if one is playing, we'll fade it out nicely
            FadeOut(musicSources[(int)MusicSourceIndex.Background], standardMusicFadeSpeed);
        }

        // We can then reinitialise this index variable before searching for a new relevent index
        // That way, if this is still -1 at the end of the loop, we know a backing instrument wasn't found
        BackgroundInstrumentIndex = -1;

        // The loop through the instruments looking for a Background Instrument
        for (int i = 0; i < newSong.songInstruments.Count; i++)
        {
            if (newSong.songInstruments[i] == backgroundInstrument)
            {
                BackgroundInstrumentIndex = i;
                break;
            }
        }
        // This new index's clip will be automatically played when loading our first song section
        // End Background Instrument Initialisation ====================================================================================


        // We can then start the first section of the new song. We want to start it with a volume of 0 - allowing it to naturally fade in (in update this happens)
        LoadSongSection(0);
        ResetSongVolume();
    }


    /// <summary>
    /// Loops through all the currently equipped instruments, finds their song-instrument pai relevent to the current song, and 
    /// initialises the new set on instruments relevent to the current song.
    /// </summary>
    void InitialiseNewSongInstruments()
    {
        // we'll look through all the potentially equipped instruments
        for (int i = 0; i < equippedAbilitiesController.equippedAbilities.Length; i++)
        {
            if (equippedAbilitiesController.equippedAbilities[i] != null)      // We then check if an ability is actually equipped in that slot
            { 
                // ...and reinitialise it's instrument if one is present
                EquipInstrument(equippedAbilitiesController.equippedAbilities[i]);
            }
        }
    }


    /// <summary>
    /// This makes sure that the dynamic audio sources are each playing the correct energy level clip from each of their associated
    /// music nodes. It will also adjust the volume of each of these audio sources based on the current category levels.
    /// </summary>
    void UpdateClipEnergies()
    {
        // We'll Start by updating which clips are playing based on the current energy levels
        // ================================================================================================================================

        // Update Mobility Clip
        if (MobilityInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Mobility].clip =
            GetCorrectEnergyClip(currentSong.sortedNodes[MobilityInstrumentIndex, currentSongSection], MobilityLevel,
            musicSources[(int)MusicSourceIndex.Mobility].clip);
            
            // If the clip was reassigned, the source will stop playing. In this case, we set the correct time and tell it to continue playing
            if (!musicSources[(int)MusicSourceIndex.Mobility].isPlaying)
            {
                musicSources[(int)MusicSourceIndex.Mobility].Play();         // We'll start the new clip

                // ...make sure the currentSectionTimer is still within the length of our clip
                if (currentSectionTimer <= musicSources[(int)MusicSourceIndex.Mobility].clip.length)
                {
                    // ...and if it is, we make sure the clip plays from the right timestamp
                    musicSources[(int)MusicSourceIndex.Mobility].time = currentSectionTimer;          
                }
                
            }
        } 

        // Update Offense Clip
        if (OffenseInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Offense].clip =
            GetCorrectEnergyClip(currentSong.sortedNodes[OffenseInstrumentIndex, currentSongSection], OffenseLevel,
            musicSources[(int)MusicSourceIndex.Offense].clip);

            // If the clip was reassigned, the source will stop playing. In this case, we set the correct time and tell it to continue playing
            if (!musicSources[(int)MusicSourceIndex.Offense].isPlaying)
            {
                musicSources[(int)MusicSourceIndex.Offense].Play();          // We'll start the new clip

                // ...make sure the currentSectionTimer is still within the length of our clip
                if (currentSectionTimer <= musicSources[(int)MusicSourceIndex.Offense].clip.length)
                {
                    // ...and if it is, we make sure the clip plays from the right timestamp
                    musicSources[(int)MusicSourceIndex.Offense].time = currentSectionTimer;
                }
            }
        }

        //Update Defense Clip
        if (DefenseInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Defense].clip =
            GetCorrectEnergyClip(currentSong.sortedNodes[DefenseInstrumentIndex, currentSongSection], DefenseLevel,
            musicSources[(int)MusicSourceIndex.Defense].clip);

            // If the clip was reassigned, the source will stop playing. In this case, we set the correct time and tell it to continue playing
            if (!musicSources[(int)MusicSourceIndex.Defense].isPlaying)
            {
                musicSources[(int)MusicSourceIndex.Defense].Play();          // We'll start the new clip

                // ...make sure the currentSectionTimer is still within the length of our clip
                if (currentSectionTimer <= musicSources[(int)MusicSourceIndex.Defense].clip.length)
                {
                    // ...and if it is, we make sure the clip plays from the right timestamp
                    musicSources[(int)MusicSourceIndex.Defense].time = currentSectionTimer;
                }
            }
        }
        // End clip assignment ============================================================================================================



        // Update the current volume values ===============================================================================================

        // Update our volume variables based on their current category levels
        mobilityVolume = GetMusicSourceVolume(MobilityLevel);
        offenseVolume = GetMusicSourceVolume(OffenseLevel);
        defenseVolume = GetMusicSourceVolume(DefenseLevel);

        // Then lerp towards these volume levels in our actual volume settings for our audio sources
        musicSources[(int)MusicSourceIndex.Mobility].volume =
            Mathf.Lerp(musicSources[(int)MusicSourceIndex.Mobility].volume, mobilityVolume, standardVolumeMorphSpeed * Time.deltaTime);
        musicSources[(int)MusicSourceIndex.Offense].volume =
            Mathf.Lerp(musicSources[(int)MusicSourceIndex.Offense].volume, offenseVolume, standardVolumeMorphSpeed * Time.deltaTime);
        musicSources[(int)MusicSourceIndex.Defense].volume =
            Mathf.Lerp(musicSources[(int)MusicSourceIndex.Defense].volume, defenseVolume, standardVolumeMorphSpeed * Time.deltaTime);

        // End volume assignment ==========================================================================================================
    }


    /// <summary>
    /// Takes the current level of a category and converts it to a volume level that falls between the minimum and
    /// maximum volumes of the type of clip playing (low, med or high), based on the publicly assigned values for these.
    /// </summary>
    /// <param name="categoryLevel">The energy level of a category to be converted to a volume value</param>
    /// <returns>A volume value between 0 and 1</returns>
    float GetMusicSourceVolume(float categoryLevel)
    {
        float newVolume = 0;  // We'll use this to temporarily store the result of our volume calculation

        if (categoryLevel < medClipThreshold)                 // Low energy clip is playing
        {
            // We want to rescale the energy value (lying between 0 and medClipThreshold) to a volume level 
            // (lying between lowClipMin and lowClipMax)
            newVolume = RescaleValue(categoryLevel, 0, medClipThreshold,  lowClipMin, lowClipMax);
        }
        else if (categoryLevel < highClipThreshold)           // Medium energy clip is playing
        {
            // We want to rescale the energy value (lying between medClipThreshold and highClipThreshold) to a volume level 
            // (lying between medClipMin and medClipMax)
            newVolume = RescaleValue(categoryLevel, medClipThreshold, highClipThreshold, medClipMin, medClipMax);
        }
        else                                                  // High energy clip is playing
        {
            // We want to rescale the energy value (lying between HighClipThreshold and categoryMaxLevel) to a volume level 
            // (lying between medClipMin and medClipMax)
            newVolume = RescaleValue(categoryLevel, highClipThreshold, categoryMax, highClipMin, highClipMax); 
        }

        // Finally, we never want our volume to go higher than 100% - so we make sure the returned value is 1 or less.
        if (newVolume > 1)
        {
            return 1;
        }
        else
        {
            return newVolume;
        }
    }


    /// <summary>
    /// Takes a value between a minimum and maximum and converts it to a new value that keeps the same proportional size, 
    /// but lies between a new maximum and minimum.
    /// </summary>
    /// <param name="currentValue">The value lying somewhere between oldMin and oldMax</param>
    /// <param name="oldMin">The current minimum value of currentValue's scale</param>
    /// <param name="oldMax">The current maximum value of currentValue's scale</param>
    /// <param name="newMin">The minimum value of the new scale the result will proportionately fall into</param>
    /// <param name="newMax">The maximum value of the new scale the result will proportionately fall into</param>
    /// <returns>A new value that falls between a new max and min, but is proportionately equal to the initial value given</returns>
    float RescaleValue(float currentValue, float oldMin, float oldMax, float newMin, float newMax)
    {
        // First we'll check if we've exceeded the maximum on the scale - implying we have a high energy in the 0.99 buffer above CategoryMax
        if (currentValue >= oldMax)
        {
            // In this case, we'll just return or new value representing 100%
            return newMax;
        }
        
        // Otherwise, we'll start by converting our value to a percentage of the original scale
        float percentageRepresentation = ((currentValue - oldMin) / (oldMax - oldMin));

        // Then we'll calculate a magnitude relative to the new scale based on that percentage
        // (Total Magnitude of New Scale) * Calculated Percentage
        float newMagnitude = ((newMax - newMin) * percentageRepresentation);

        // Finally, we add that to the new min to put it back within the bounds of our new scale
        return (newMagnitude + newMin);
    }


    /// <summary>
    /// Creates temporary FadeOut objects for each Audio Source currently playing, fading out the entire song in one go.
    /// </summary>
    void FadeOutCurrentSong()
    {
        // We'll look through all the audio sources used to play music
        for (int i = 0; i < musicSources.Length; i++)
        {
            // ...and if we find one that is currently playing something...
            if (musicSources[i].isPlaying)
            {
                // ...we tell it to fade out what it's playing
                FadeOut(musicSources[i], standardMusicFadeSpeed);
            }
        }
    }


    /// <summary>
    /// Sets the current song's volume to 0 using the DynamicMusic Mixer Group. This will normally automatically be lerped back to the 
    /// user defined volume level in the update loop. Will conserve the various source proportions, save a bit of processing power
    /// and won't affect Temp sources (which run through a different mixer group)
    /// </summary>
    public void ResetSongVolume()
    {
        MusicMasterMixer.SetFloat("DynamicMusicVolume", -60);
    }


    /// <summary>
    /// Examines current ability category levels as well as energy levels of the sections that can naturally follow the
    /// current section and chooses an appropriate section to transition into.
    /// </summary>
    public void NextSongSection()
    {
        if (SectionControl)  // When using Section control, we Have to analyse the current energy of the player's playstyle and choose a section with energy to match
        {
            UpdatePlaystyleEnergy();              // Calculate a new total energy reflecting the player's playstyle

            // First we'll check if our playstyle energy is smaller than the lowest rated section of this song, and if we're already playing that lowest section
            if (currentSongSection == currentSong.lowestSection && currentSong.sectionDetails[currentSongSection].songSectionEnergy > playstyleEnergy)
            {
                // if so (because each individual section is loopable), we'll just load this section again
                LoadSongSection(currentSongSection);
            }
            // if that wasn't the case, we check if our playstyle energy is bigger than the highest rated section of this song, and if we're already playing that highest section
            else if (currentSongSection == currentSong.highestSection && currentSong.sectionDetails[currentSongSection].songSectionEnergy < playstyleEnergy)
            {
                // if so (because each individual section is loopable), we'll just load this section again
                LoadSongSection(currentSongSection);
            }
            // if not, our current energy is somewhere in the middle of the possible energies of the song. We'll need to search for a section with a similar 
            // energy to match it from our list of possible forward links
            else
            {    
                // The next step is a little wild - but what we're doing is looking for all the song sections that have an energy lower 
                // than our current playstyle energy, and from these caching the index of the one that is closest to the current
                // playstyle energy. 

                int newSongSection = -1;              // this will store the most eligible section index from the list of forward links we search through.
                int[] currentSectionForwardLinks = currentSong.sectionDetails[currentSongSection].forwardLinks;  // caching our current section's forward links (for easy, and efficient reference)

                for (int i = 0; i < currentSectionForwardLinks.Length; i++)
                {
                    // first, we check if the link we're looking at has lower energy than our current playstyle
                    if (currentSong.sectionDetails[currentSectionForwardLinks[i]].songSectionEnergy < playstyleEnergy)
                    {
                        // then we check if we'd already found an eligible link with a lower energy than our current playstyle
                        if (newSongSection != -1)
                        {
                            // if so, we check if it has higher energy than any previous eligible links we had found.
                            if (currentSong.sectionDetails[newSongSection].songSectionEnergy <
                                currentSong.sectionDetails[currentSectionForwardLinks[i]].songSectionEnergy)
                            {
                                // and if it is, we cache this index instead
                                newSongSection = currentSectionForwardLinks[i];
                            }
                        }
                        else
                        {
                            newSongSection = currentSectionForwardLinks[i];    // we found our first eligible link! Let's cache it.
                        }
                    }
                }


                if (newSongSection != -1)  // This implies we found the nearest linked section with an energy lower than our current playstyle.
                {
                    LoadSongSection(newSongSection);
                }
                else  // this implies that no lower index was found - so we'll find the lowest energy section possible in our forwardLinks
                {
                    // first we'll equip the first index from our forward links to give us a reference point to compare to the other forward links
                    newSongSection = currentSectionForwardLinks[0];

                    for (int i = 0; i < currentSectionForwardLinks.Length; i++)
                    {
                        // we compare our current index's energy to the ith index's energy to find the smallest energy in the list of forward links
                        if (currentSong.sectionDetails[currentSectionForwardLinks[i]].songSectionEnergy <
                            currentSong.sectionDetails[newSongSection].songSectionEnergy)
                        {
                            newSongSection = currentSectionForwardLinks[i];
                        }
                    }

                    // now that we have the lowest energy section's index from our list of forward links, we can load it
                    LoadSongSection(newSongSection);
                }

            }

        }
        else  // Implies we aren't using section control
        {
            // If we aren't using section control, we'll just load the next song section in the array.
            // The mod function below makes the number automatically cycle back to 0 if it was about to go out of the array bounds
            LoadSongSection( (currentSongSection + 1) % currentSong.finalSection );    
        }
    }



    /// <summary>
    /// Takes the three category levels, clamps them to a maximum of categoryMax, and calculates a total value that reflects
    /// the energy of the player's current playstyle.
    /// </summary>
    public void UpdatePlaystyleEnergy()
    {
        playstyleEnergy = Mathf.Clamp(MobilityLevel, 0, categoryMax) +
            Mathf.Clamp(OffenseLevel, 0, categoryMax) +
            Mathf.Clamp(DefenseLevel, 0, categoryMax);
    }


    /// <summary>
    /// Loads a specific section of a song using it's index from the current song's 2D sortedNode array.
    /// </summary>
    /// <param name="sectionToLoad">The index of the section to load in the song's 2D sortedNode array</param>
    public void LoadSongSection(int sectionToLoad)
    {
        int previousSection = currentSongSection;  // First we want to keep a reference to the previous section
        currentSongSection = sectionToLoad;        // while updating our cached index

        // We cache the length of the current section for easy reference. As all of the section's clips are the same length,
        // we can just look at 1 of them to extract the length we're looking for.
        currentSectionLength = currentSong.sortedNodes[0, currentSongSection].lowClip.length;
        
        // We reset the timer, so it can start timing the new section that was just loaded.
        currentSectionTimer = 0;


        // Next we update the clips in our audio sources to those of the new sections. We'll have to check the current energy levels too
        // =============================================================================================================

        // Mobility clip updating
        if (MobilityInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Mobility].clip =
                        GetCorrectEnergyClip(currentSong.sortedNodes[MobilityInstrumentIndex, currentSongSection],
                        currentSong.sortedNodes[MobilityInstrumentIndex, previousSection], MobilityLevel,
                        musicSources[(int)MusicSourceIndex.Mobility].clip);
            musicSources[(int)MusicSourceIndex.Mobility].time = 0;                            // We make sure the new clip starts from the beginning
            musicSources[(int)MusicSourceIndex.Mobility].Play();
        }

        // Offense clip updating
        if (OffenseInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Offense].clip =
                    GetCorrectEnergyClip(currentSong.sortedNodes[OffenseInstrumentIndex, currentSongSection],
                    currentSong.sortedNodes[OffenseInstrumentIndex, previousSection], OffenseLevel,
                    musicSources[(int)MusicSourceIndex.Offense].clip);
            musicSources[(int)MusicSourceIndex.Offense].time = 0;                             // We make sure the new clip starts from the beginning
            musicSources[(int)MusicSourceIndex.Offense].Play();
        }


        // Defense clip updating
        if (DefenseInstrumentIndex != -1)    // We want to make sure an instrument is actually equipped for this category
        {
            musicSources[(int)MusicSourceIndex.Defense].clip =
                    GetCorrectEnergyClip(currentSong.sortedNodes[DefenseInstrumentIndex, currentSongSection],
                    currentSong.sortedNodes[DefenseInstrumentIndex, previousSection], DefenseLevel,
                    musicSources[(int)MusicSourceIndex.Defense].clip);
            musicSources[(int)MusicSourceIndex.Defense].time = 0;                             // We make sure the new clip starts from the beginning
            musicSources[(int)MusicSourceIndex.Defense].Play();
        }

        if (BackgroundInstrumentIndex != -1)
        {
            // Note that the background will only have one clip level (I assume), hence we just load the low clip
            musicSources[(int)MusicSourceIndex.Background].clip = currentSong.sortedNodes[BackgroundInstrumentIndex, currentSongSection].lowClip;
            musicSources[(int)MusicSourceIndex.Background].Play();
        }
        // =============================================================================================================

    }


    /// <summary>
    /// Compares the given value to the Clip Thresholds and returns either the low energy, medium energy or high energy clip associated
    /// with the given node.
    /// </summary>
    /// <param name="node">The node that the clip will be extracted from.</param> <param name="currentEnergyLevel">The current energy level
    /// relevent to this node.</param> <param name="currentClip">The clip currently being played by the Audio Source in question</param>
    /// <returns>Low, Medium or High audio clip associated with the given node</returns>
    public AudioClip GetCorrectEnergyClip(MusicNode node, float currentEnergyLevel, AudioClip currentClip)
    {
        if (currentEnergyLevel >= highClipThreshold)          // Playstyle is high energy
        {
            return node.highClip;   // No need to put a buffer here - there's no higher clip to fall from
        }
        else if (currentEnergyLevel < highClipThreshold && currentEnergyLevel >= highLowerThreshold) // Implies we're still in the buffer space
        {
            // this implies the energy was high, but has now dropped to medium. In this case, we want to wait a little while 
            // before switching back to smooth out the experience a little
            if (currentClip == node.highClip)
            {
                return node.highClip;
            }

            // If that's not the case, we were coming from the low clip and don't have to apply the buffer
            return node.medClip;
        }
        else if (currentEnergyLevel >= medClipThreshold)      // Playstyle is medium energy
        {
            return node.medClip;
        }
        else if (currentEnergyLevel < medClipThreshold && currentEnergyLevel >= medLowerThreshold) // Implies we're still in the buffer space
        {
            // this implies the energy was med, but has now dropped to low. In this case, we want to wait a little while 
            // before switching back to smooth out the experience a little
            if (currentClip == node.medClip)
            {
                return node.medClip;
            }

            // If that's not the case, we were coming from the low clip anyway and don't have to apply the buffer
            return node.lowClip;
        }
        else                                                  // Playstyle is full-on low energy
        {
            return node.lowClip;
        }
    
    }


    /// <summary>
    /// Compares the given value to the Clip Thresholds and returns either the low energy, medium energy or high energy clip associated
    /// with the given current node. This overloaded version analyses the node that was previously playing to find the 
    /// information it needs. Use it in conjunction with section changes.
    /// </summary>
    /// <param name="currentNode">The node that the clip will be returned from.</param> 
    /// <param name="previousNode">The node that provided the last clip to be played by the audio source</param> 
    /// <param name="currentEnergyLevel">The current energy level relevent to this node.</param>
    /// <param name="currentClip">The clip currently being played by the Audio Source in question</param>
    /// <returns>Low, Medium or High audio clip associated with the given node</returns>
    public AudioClip GetCorrectEnergyClip(MusicNode currentNode, MusicNode previousNode, float currentEnergyLevel, AudioClip currentClip)
    {
        if (currentEnergyLevel >= highClipThreshold)          // Playstyle is high energy
        {
            return currentNode.highClip;   // No need to put a buffer here - there's no higher clip to fall from
        }
        else if (currentEnergyLevel < highClipThreshold && currentEnergyLevel >= highLowerThreshold) // Implies we're still in the buffer space
        {
            // this implies the energy was high, but has now dropped to medium. In this case, we want to wait a little while 
            // before switching back to smooth out the experience a little
            if (currentClip == previousNode.highClip)
            {
                return currentNode.highClip;
            }

            // If that's not the case, we were coming from the low clip and don't have to apply the buffer
            return currentNode.medClip;
        }
        else if (currentEnergyLevel >= medClipThreshold)      // Playstyle is medium energy
        {
            return currentNode.medClip;
        }
        else if (currentEnergyLevel < medClipThreshold && currentEnergyLevel >= medLowerThreshold) // Implies we're still in the buffer space
        {
            // this implies the energy was med, but has now dropped to low. In this case, we want to wait a little while 
            // before switching back to smooth out the experience a little
            if (currentClip == previousNode.medClip)
            {
                return currentNode.medClip;
            }

            // If that's not the case, we were coming from the low clip anyway and don't have to apply the buffer
            return currentNode.lowClip;
        }
        else                                                  // Playstyle is full-on low energy
        {
            return currentNode.lowClip;
        }

    }


    /// <summary>
    /// Checks if the current song section is still busy playing or not.
    /// </summary>
    /// <returns>Returns true if the section has played completely</returns>
    public bool SectionDonePlaying()
    {
        // We check if our timer's been running for the full length of the song (give or take a frame) *************************************************
        if (currentSectionLength > (currentSectionTimer /*+ Time.deltaTime*/))       
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
        else                                                                            // if one wasn't found, we throw an error and allow the instrument index to get set to -1
        {
            Debug.LogWarning("Ability " + newAbility.name + " has no instrument assigned for the " + currentSong.name + " song." );
        }

        
        // Next, we figure out which category this new instrument belongs to, update that category's audio sources and fade out the music that was
        // initially playing.
        switch (newAbility.abilityCategory)
        {
            case Ability.AbilityCategory.Mobility:

                FadeOut(musicSources[(int)MusicSourceIndex.Mobility], standardMusicFadeSpeed);  // Creates a temporary object that will gently fade this instrument away
                MobilityInstrumentIndex = newInstrumentIndex;                                   // Updates the current mobility instrument index
                musicSources[(int)MusicSourceIndex.Mobility].volume = 0;                        // This will give an automatic fade-in effect, 
                                                                                                // due to how volume responds to category levels in Update()

                if (MobilityInstrumentIndex == -1)                                              // implies no instrument equipped/found
                {
                    musicSources[(int)MusicSourceIndex.Mobility].Stop();
                    musicSources[(int)MusicSourceIndex.Mobility].clip = null;
                }
                else                                                                            // Implies there IS a new clip to play
                {
                    musicSources[(int)MusicSourceIndex.Mobility].clip =                         // Assigns the new clip to the correct AudioSource
                        GetCorrectEnergyClip(currentSong.sortedNodes[newInstrumentIndex, currentSongSection], MobilityLevel,
                        musicSources[(int)MusicSourceIndex.Mobility].clip);
                    musicSources[(int)MusicSourceIndex.Mobility].time = currentSectionTimer;    // Make sure the clip's playing at the correct place
                    musicSources[(int)MusicSourceIndex.Mobility].Play(); 
                }
                
                break;

            case Ability.AbilityCategory.Offense:

                FadeOut(musicSources[(int)MusicSourceIndex.Offense], standardMusicFadeSpeed);   // Creates a temporary object that will gently fade this instrument away
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
                    musicSources[(int)MusicSourceIndex.Offense].clip =                          // Assigns the new clip to the correct AudioSource
                        GetCorrectEnergyClip(currentSong.sortedNodes[newInstrumentIndex, currentSongSection], OffenseLevel,
                        musicSources[(int)MusicSourceIndex.Offense].clip);
                    musicSources[(int)MusicSourceIndex.Offense].time = currentSectionTimer;     // Make sure the clip's playing at the correct place
                    musicSources[(int)MusicSourceIndex.Offense].Play();
                }

                break;

            case Ability.AbilityCategory.Defense:

                FadeOut(musicSources[(int)MusicSourceIndex.Defense], standardMusicFadeSpeed);   // Creates a temporary object that will gently fade this instrument away
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
                    musicSources[(int)MusicSourceIndex.Defense].clip =                          // Assigns the new clip to the correct AudioSource
                        GetCorrectEnergyClip(currentSong.sortedNodes[newInstrumentIndex, currentSongSection], DefenseLevel,
                        musicSources[(int)MusicSourceIndex.Defense].clip);
                    musicSources[(int)MusicSourceIndex.Defense].time = currentSectionTimer;     // Make sure the clip's playing at the correct place
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


    // Note to Self: ---------------------> MAKE OVERLOAD THAT TAKES IN AN INSTRUMENT!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    /// <summary>
    /// Takes in a given ability category, and resets any variables and Audio Sources relating to that category in the manager.
    /// </summary>
    /// <param name="category">The category of the ability to remove</param>
    public void RemoveInstrument(Ability.AbilityCategory category)
    {
        // We'll check which category of ability is being removed, and reset all the audio sources and variables related to it
        switch (category)
        {
            case Ability.AbilityCategory.Mobility:

                FadeOut(musicSources[(int)MusicSourceIndex.Mobility], standardMusicFadeSpeed);  // Creates a temporary object that will gently fade this instrument away
                MobilityInstrumentIndex = -1;                                                   // Updates the current mobility instrument index - -1 implies nothing is equipped
                musicSources[(int)MusicSourceIndex.Mobility].Stop();                            // With nothing to play, we can stop the source to save some processing power.
                musicSources[(int)MusicSourceIndex.Mobility].clip = null;                       

                break;

            case Ability.AbilityCategory.Offense:

                FadeOut(musicSources[(int)MusicSourceIndex.Offense], standardMusicFadeSpeed);   // Creates a temporary object that will gently fade this instrument away
                OffenseInstrumentIndex = -1;                                                    // Updates the current mobility instrument index - -1 implies nothing is equipped
                musicSources[(int)MusicSourceIndex.Offense].Stop();                             // With nothing to play, we can stop the source to save some processing power.
                musicSources[(int)MusicSourceIndex.Offense].clip = null;

                break;

            case Ability.AbilityCategory.Defense:

                FadeOut(musicSources[(int)MusicSourceIndex.Defense], standardMusicFadeSpeed);   // Creates a temporary object that will gently fade this instrument away
                DefenseInstrumentIndex = -1;                                                    // Updates the current mobility instrument index - -1 implies nothing is equipped
                musicSources[(int)MusicSourceIndex.Defense].Stop();                             // With nothing to play, we can stop the source to save some processing power.
                musicSources[(int)MusicSourceIndex.Defense].clip = null;

                break;
        }
    }


    /// <summary>
    /// Adjusts the current Mobility level by the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="adjustment">How much mobility Level will change by</param>
    public void AdjustMobilityLevel(float adjustment)
    {
        MobilityLevel = Mathf.Clamp(MobilityLevel + adjustment, minLevelValue, maxLevelValue);
    }



    /// <summary>
    /// Adjusts the current Offense level by the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="adjustment">How much the Offense Level will change by</param>
    public void AdjustOffenseLevel(float adjustment)
    {
        OffenseLevel = Mathf.Clamp(OffenseLevel + adjustment, minLevelValue, maxLevelValue);
    }


    /// <summary>
    /// Adjusts the current Defense level by the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="adjustment">How much the Defense Level will change by</param>
    public void AdjustDefenseLevel(float adjustment)
    {
        DefenseLevel = Mathf.Clamp(DefenseLevel + adjustment, minLevelValue, maxLevelValue);
    }


    /// <summary>
    /// Sets the current Mobility level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for mobility Level</param>
    public void SetMobilityLevel(float newLevel)
    {
        MobilityLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
    }



    /// <summary>
    /// Sets the current Offense level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for Offense Level</param>
    public void SetOffenseLevel(float newLevel)
    {
        OffenseLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
    }


    /// <summary>
    /// Sets the current Defense level to the float given. Value will be clamped, and any necessary music transitions will be called.
    /// </summary>
    /// <param name="newLevel">The new value for Defense Level</param>
    public void SetDefenseLevel(float newLevel)
    {
        DefenseLevel = Mathf.Clamp(newLevel, minLevelValue, maxLevelValue);
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


    /// <summary>
    /// Returns sum of all three energy levels, each with a max value of categoryMax
    /// </summary>
    /// <returns>Current sum of all 3 category energies - each capped at CategoryMax</returns>
    public float GetPlaystyleEnergy()
    {
        return playstyleEnergy;
    }

    /// <summary>
    /// Returns the energy rating (0-5) of the currently playing section.
    /// </summary>
    /// <returns>Float representing energy level of the song section playing</returns>
    public float GetCurrentSectionEnergy()
    {
        return currentSong.sectionDetails[currentSongSection].songSectionEnergy;
    }


    /// <summary>
    /// Returns the current calculated volume for mobility. Note that this is not necessarily the actual volume of the audio source
    /// playing the mobility music - this audio source's volume will be lerped toward the value this returns.
    /// </summary>
    /// <returns>The current calculated volume for mobility.</returns>
    public float GetMobilityVolume()
    {
        return mobilityVolume;
    }

    /// <summary>
    /// Returns the current calculated volume for offense. Note that this is not necessarily the actual volume of the audio source
    /// playing the offense music - this audio source's volume will be lerped toward the value this returns.
    /// </summary>
    /// <returns>The current calculated volume for offense.</returns>
    public float GetOffenseVolume()
    {
        return offenseVolume;
    }

    /// <summary>
    /// Returns the current calculated volume for defense. Note that this is not necessarily the actual volume of the audio source
    /// playing the defense music - this audio source's volume will be lerped toward the value this returns.
    /// </summary>
    /// <returns>The current calculated volume for defense.</returns>
    public float GetDefenseVolume()
    {
        return defenseVolume;
    }

    #endregion

}

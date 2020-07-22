using System.Collections;
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

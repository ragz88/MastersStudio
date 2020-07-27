using UnityEngine;

/// <summary>
/// Used for a temporary prefab with an AudioSource. This prefab will decrease it's volume at a constant rate, and delete itself once it falls silent.
/// </summary>
public class TempAudioFadeOut : MonoBehaviour
{
    [HideInInspector]
    public AudioSource source;                  // Source that the song will play from while fading out.

    public float fadeSpeed = 2f;                // Speed at which the new source will fade away.

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        source.volume -= (fadeSpeed * Time.deltaTime);         // Decreases current volume at a constant rate.

        if (source.volume <= 0)                                // Once the source has finished fading out, it deletes itself;
        {
            Destroy(gameObject);
        }
    }


    /// <summary>
    /// Ensures that all the important information from the original Audio Source which is being faded out is conserved in the newly instantiated source.
    /// </summary>
    /// <param name="originalSource">The source playing the sound which is to be faded out</param>
    /// <param name="newFadeSpeed">The speed at which this sound should reduce to nothing</param>
    public void InitialiseFadeOut(AudioSource originalSource, float newFadeSpeed)
    {
        // Here we make sure all the important properties of the initial source are copied exactly to the new source
        source.clip = originalSource.clip;
        source.time = originalSource.time;
        source.pitch = originalSource.pitch;
        source.volume = originalSource.volume;
        source.outputAudioMixerGroup = originalSource.outputAudioMixerGroup;

        fadeSpeed = newFadeSpeed;
    }
}

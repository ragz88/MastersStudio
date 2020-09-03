using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempSoundEffect : MonoBehaviour
{
    AudioSource effectSource;
    public bool effectPlayed = false;
    public bool detachFromParent = false;

    private void Awake()
    {
        if (detachFromParent)
        {
            transform.parent = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        effectSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (effectSource.isPlaying == false && effectPlayed == true)
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderingInitializer2D : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().receiveShadows = true;
        GetComponent<SpriteRenderer>().castShadows = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

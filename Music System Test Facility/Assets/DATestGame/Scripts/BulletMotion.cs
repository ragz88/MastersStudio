using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMotion : MonoBehaviour
{
    Vector2 direction;
    public float speed = 2;
    public float rotationSpeed = 2;

    public bool PowerShot = false;

    [HideInInspector]
    public DAPlayerController playerController;

    private Rigidbody2D bulletBody;

    void Start()
    {
        bulletBody = GetComponent<Rigidbody2D>();
        direction = transform.up;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet (using the given direction and speed)
        // normalisation conserves direction, but makes the Vector's magnitude 1 - ensuring consistent speed
        bulletBody.velocity = direction.normalized * speed;

        // Makes the bullet spin - #JustAesthetics
        gameObject.transform.Rotate(0, 0, rotationSpeed*Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!PowerShot || collision.gameObject.CompareTag("KillBox"))
        {
            if (playerController != null)  // This already implies that we're simulating a prefabricated play game
            {
                // We'll make the energy low as soon as the bullet dies
                MusicControllerDA.musicControllerInstance.SetOffenseLevel(playerController.lowEnergy);
            }

            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public float ringRotateSpeed = 0.4f;   // Speed at which outer ring will rotate
    public Transform outsideRing;          // The outer ring of the cursor. We'll rotate this for visual juice.

    private Camera mainCam;                // cached reference to main camera to improve performance
    
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Move cursor to the current mouse position
        Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        // Rotate the outer ring
        outsideRing.Rotate(0,0, ringRotateSpeed*Time.deltaTime);
    }
}

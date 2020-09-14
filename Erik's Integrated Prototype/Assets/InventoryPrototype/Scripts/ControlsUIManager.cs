using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


/// <summary>
/// Checks whether there is a controller present or not, and updates the relevant UI.
/// </summary>
public class ControlsUIManager : MonoBehaviour
{
    /// <summary>
    /// Contains an image and the sprites it will show, depending on whether a gamepad is connected or the player is using the keyboard.
    /// </summary>
    [System.Serializable]                         // Allows us to access this struct in the inspector
    public struct ControlSpriteSet
    {
        public Image controlFace;                 // The image that actually shows which input is linked to what action
        public Sprite keyboardFaceSprite;         // The sprite to display when playing with keyboard
        public Sprite gamepadFaceSprite;          // The sprite to display when playing with a controller
    }

    public ControlSpriteSet[] controlSpriteSets;  // stores a reference to all the images that must change depending on the input type the player uses

    bool currentlyKeyboard = true;                // allows us to check a bool first, reducing the processing draw of the first if statement.
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Whenever we show a prompt, we'll check if it should be relaying controller or keyboard information.
        // I'll add the XBox support later. I know how, but it's a bit of work, as I have to remap joystick buttons 0 - 3 in the Input Settings.
        string[] controllers = Input.GetJoystickNames();

        if (controllers.Length > 0)
        {
            if (controllers[0] != "")// this implies at least 1 controller is connected
            {
                if (currentlyKeyboard)           // prevents this function from running every update frame. Reduces processing power draw
                {                                // now it only runs the first time we move from keyboard to gamepad
                    ChangeToGamepadControls();
                }
            }
            else
            {
                if (!currentlyKeyboard)  // prevents this function from running every update frame. Reduces processing power draw
                {
                    ChangeToKeyboardControls();
                }
            }
        }
        else
        {
            if (!currentlyKeyboard)  // prevents this function from running every update frame. Reduces processing power draw
            {
                ChangeToKeyboardControls();
            }
        }

    }

    /// <summary>
    /// Updates all the images in controlSpriteSets array to show their gamepad control variants
    /// </summary>
    void ChangeToGamepadControls()
    {
        currentlyKeyboard = false;  

        for (int i = 0; i < controlSpriteSets.Length; i++)
        {
            controlSpriteSets[i].controlFace.sprite = controlSpriteSets[i].gamepadFaceSprite;
        }
    }


    /// <summary>
    /// Updates all the images in controlSpriteSets array to show their keyboard control variants
    /// </summary>
    void ChangeToKeyboardControls()
    {
        currentlyKeyboard = true;

        for (int i = 0; i < controlSpriteSets.Length; i++)
        {
            controlSpriteSets[i].controlFace.sprite = controlSpriteSets[i].keyboardFaceSprite;
        }
    }
}

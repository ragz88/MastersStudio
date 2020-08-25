using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Place this on the main menu buttons to control the fading Icon at the bottom of the menu
/// </summary>
public class MenuIconFader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{ 
    public Image iconToEnter;            // The icon to fade in and out 

    public float iconFadeSpeed = 2;      // How quickly the icon will fade in and out

    Button button;                       // The button this script is attached to
    bool enterImage = false;             // Used to track when the mouse is present in the button's bounds


    /// <summary>
    /// This function runs each time the mouse enters this button's bounds
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        enterImage = true;
    }


    /// <summary>
    /// This function runs each time the mouse exits this button's bounds
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        enterImage = false;
    }


    private void Start()
    {
        button = GetComponent<Button>();
    }

    private void Update()
    {
        // We only want the image to show when the buttons are actually interactable
        if (button.interactable)
        {
            // if button is highlighted and the alpha isn't full yet
            if (enterImage && iconToEnter.color.a < 1)
            {
                //... we increase the alpha over time
                iconToEnter.color = iconToEnter.color + new Color(0, 0, 0, iconFadeSpeed * Time.deltaTime);
            }
            // Otherwise, if the button is not selected
            else if (!enterImage && iconToEnter.color.a > 0)
            {
                // We decrease the alpha over time
                iconToEnter.color = iconToEnter.color - new Color(0, 0, 0, iconFadeSpeed * Time.deltaTime);
            }
        }
    }
}

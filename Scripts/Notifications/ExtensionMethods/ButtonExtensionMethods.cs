using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonExtensionMethods
{
    /// <summary>
    /// Extension Metod. This method removes all listeners from button same as onClick.RemoveAllListeners() method. Additionally reassigns methods resposnible 
    /// for click sound.
    /// Use it with caution as it calls GetComponent of button's gameobjects which can be devastating in Update() method.
    /// </summary>
    /// <param name="button"></param>

    public static void RemoveAllOnClickListeners(this Button button)
    {
        button.onClick.RemoveAllListeners();
        ButtonClickSound component = button.gameObject.GetComponent<ButtonClickSound>();
        if (component!=null)
        {
            component.ReassignSound();
        }
    }
}

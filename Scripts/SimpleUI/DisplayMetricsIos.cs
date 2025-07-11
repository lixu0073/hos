using UnityEngine;
using System.Collections;

public class DisplayMetricsIos
{

    // The absolute height of the display in pixels
    public static int HeightPixels { get; protected set; }

    // The absolute width of the display in pixels
    public static int WidthPixels { get; protected set; }

    // The exact physical pixels per inch of the screen
    public static float DPI { get; protected set; }

    // The exact values of diagonal inches
    public static float Inch { get; protected set; }

    // The value of screen ratio
    public static float Ratio { get; protected set; }

    static DisplayMetricsIos()
    {
        // Early out if we're not on an Android device
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            return;
        }

        DPI = Screen.dpi;
        HeightPixels = Screen.height;
        WidthPixels = Screen.width;

        Inch = Mathf.Sqrt(Mathf.Pow(Screen.width / DPI, 2) + Mathf.Pow(Screen.height / DPI, 2));
        Ratio = (float)WidthPixels / (float)HeightPixels;
    }
}

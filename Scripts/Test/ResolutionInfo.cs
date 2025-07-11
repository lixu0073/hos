using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class ResolutionInfo : MonoBehaviour
{
    
    public Text resolutionInfo;

    // Use this for initialization
    void Start()
    {
            #if UNITY_EDITOR
                                        resolutionInfo.text = "W:" + Screen.width + " H:" + Screen.height + " R:" + ((float)Screen.width / (float)Screen.height).ToString() + " DPI: " + ExtendedCanvasScaler.GetDiagonalValue() ;
            #elif UNITY_ANDROID
                                        resolutionInfo.text = "W:" + DisplayMetricsAndroid.WidthPixels + " H:" + DisplayMetricsAndroid.HeightPixels + " DPI:" + DisplayMetricsAndroid.XDPI + " Inch:" + DisplayMetricsAndroid.Inch + " R:" + DisplayMetricsAndroid.Ratio;
            #elif UNITY_IOS
                                        resolutionInfo.text = "W:" + DisplayMetricsIos.WidthPixels + " H:" + DisplayMetricsIos.HeightPixels + " DPI:" + DisplayMetricsIos.DPI + " Inch:" + DisplayMetricsIos.Inch + " R:" + DisplayMetricsIos.Ratio;
            #endif
    }

    // for tests only
    /*
    void Update()
    {
        Start();
    }
    */
}

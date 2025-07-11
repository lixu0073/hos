using UnityEngine;
using UnityEngine.UI;
using System;

[ExecuteInEditMode]
public class ExtendedCanvasScaler : CanvasScaler
{
    [System.Serializable]
    private class PlatformSettings
    {
        public int UIEditorScallingFactor = 1;
        public int EditorDPI = 320;
    }

    [SerializeField]
    private PlatformSettings FallbackSettings = null;

    [SerializeField]
    private PlatformSettings PCSettings = null;

    [SerializeField]
    private PlatformSettings MobileSettings = null;

    private PlatformSettings ActualCurrentSettings
    {
        get
        {
            switch (CurrentSettings)
            {
                case Settings.FallbackSettings:
                    return FallbackSettings;

                case Settings.MobileSettings:
                    return MobileSettings;

                case Settings.PCSettings:
                    return PCSettings;
            }

#if UNITY_IOS || UNITY_ANDROID
            return MobileSettings;
#elif UNITY_STANDALONE
            return PCSettings;
#else
            return FallbackSettings;
#endif
        }
    }

    private enum Settings
    {
        PlatformDependent,
        FallbackSettings,
        MobileSettings,
        PCSettings,
    }

    [SerializeField]
    private Settings CurrentSettings = Settings.PlatformDependent;


    public string CurrentSettingsName
    {
        get
        {
#if UNITY_IOS || UNITY_ANDROID
            return "Mobile Settings";
#elif UNITY_STANDALONE
            return "PC Settings";
#else
            return "Fallback Settings";
#endif
        }
    }

    private float GetDPI()
    {
#if UNITY_EDITOR
        return ActualCurrentSettings.EditorDPI;
#elif UNITY_ANDROID
        return DisplayMetricsAndroid.DensityDPI;
#elif UNITY_IOS
        return DisplayMetricsIos.DPI;
#endif
    }

    public static float GetDiagonalValue()
    {
#if UNITY_EDITOR
        return Mathf.Sqrt(Mathf.Pow(Screen.width / Screen.dpi, 2) + Mathf.Pow(Screen.height / Screen.dpi, 2));
#elif UNITY_ANDROID
        return DisplayMetricsAndroid.Inch;
#elif UNITY_IOS
        return DisplayMetricsIos.Inch;
#endif
    }

    public static bool isPhone()
    {
        if (GetDiagonalValue() < 6.5f)
        {
            return true;
        }
        return false;
    }

    public static bool IsIpad()
    {
#if UNITY_EDITOR
        return true;
#else
        return SystemInfo.deviceModel.Contains("iPad");
#endif
    }

    public static bool HasNotch()
    {
#if UNITY_EDITOR
        try
        {
            return DeveloperParametersController.Instance().parameters.isIphoneX;
        }
        catch (Exception e)
        {
            Debug.LogError("TO FIX!!!. Error: " + e.Message + " \n stack " + e.StackTrace);
            return false;
        }
#elif UNITY_IOS
        return Screen.safeArea.xMin > 0f || Screen.safeArea.xMax < Screen.width;
#else
        return false;
#endif
    }
    /// <summary>
    /// Returns the pixels of the safe area of the screen from left side or right side
    /// </summary>
    /// <param name="left"></param>
    /// <returns></returns>
    public static float GetIphoneNotchOffset(bool left = true)
    {
#if UNITY_IOS
        if (Screen.safeArea.xMin > 0f && left)
        {
            return Screen.safeArea.xMin;
        }
        else if (Screen.safeArea.xMax < Screen.width && !left)
        {
            return Screen.safeArea.xMax;
        }
#endif
        return 0f;
    }

    public void Recalculate()
    {
        HandleConstantPhysicalSize();
    }

    protected override void HandleConstantPhysicalSize()
    {
        float currentDpi = GetDPI() / ActualCurrentSettings.UIEditorScallingFactor;
        float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
        float targetDPI = 1;
        switch (m_PhysicalUnit)
        {
            case Unit.Centimeters: targetDPI = 2.54f; break;
            case Unit.Millimeters: targetDPI = 25.4f; break;
            case Unit.Inches: targetDPI = 1; break;
            case Unit.Points: targetDPI = 72; break;
            case Unit.Picas: targetDPI = 6; break;
        }

        SetScaleFactor(dpi / targetDPI);
        SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * targetDPI / m_DefaultSpriteDPI);
    }

    public static float CalcScreenRatioWtoH()
    {
        return (float)Screen.width / (float)Screen.height;
    }
}

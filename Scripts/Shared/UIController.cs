using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Text;
using TMPro;

public class UIController : MonoBehaviour
{

    private static UIController uiController = null;

    public static BaseUIController get
    {
        get {
            if(uiController == null)
                return null;
            return uiController.GetBaseController();
        }
    }

    public static HospitalUIController getHospital
    {
        get
        {
            if (uiController != null && uiController.hospitalUIController != null)
                return uiController.hospitalUIController;
            return null;
        }
    }

    public static MaternityUIController getMaternity
    {
        get
        {
            if (uiController != null && uiController.maternityUIController != null)
                return uiController.maternityUIController;
            return null;
        }
    }

    private HospitalUIController hospitalUIController = null;
    private MaternityUIController maternityUIController = null;

	void Awake()
    {
        uiController = this;
        hospitalUIController = GetComponent<HospitalUIController>();
        maternityUIController = GetComponent<MaternityUIController>();
    }

    public BaseUIController GetBaseController()
    {
        if (hospitalUIController != null)
            return hospitalUIController;
        return maternityUIController;
    }

    #region Helpers, set up fonts, colors, shaders, grayscales, UI manipulations

    public static void SetGameObjectActiveSecure(GameObject objectToSet, bool setActive)
    {
        if (objectToSet == null)
        {
            Debug.LogError("objectToSet is null");
            return;
        }
        if (objectToSet.activeSelf == setActive)
        {
            return;
        }
        objectToSet.SetActive(setActive);
    }

    public static void SetImageGrayscale(Image image, bool setGrayscale, Material defaultMaterial = null)
    {
        if (image == null)
        {
            Debug.LogError("image is null");
            return;
        }
        if (setGrayscale)
        {
            image.material = ResourcesHolder.Get().GrayscaleMaterial;
        }
        else
        {
            image.material = defaultMaterial;
        }
    }

    public static void SetTMProUGUITextSecure(TextMeshProUGUI text, string toSet)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        if (toSet == null)
        {
            Debug.LogError("toSet is null");
            return;
        }
        text.text = toSet;
    }

    public static void SetTMProUGUITextGrayscaleFace(TextMeshProUGUI text, bool setGrayscale, Color defaultFace)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        if (setGrayscale)
        {
            Color grayscaleFace = new Color(defaultFace.grayscale, defaultFace.grayscale, defaultFace.grayscale);

            text.color = grayscaleFace;
        }
        else
        {
            text.color = defaultFace;
        }
    }

    public static void SetTMProUGUITextGrayscaleOutline(TextMeshProUGUI text, bool setGrayscale, Color defaultOutline)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        if (setGrayscale)
        {
            Color grayscaleOutline = new Color(defaultOutline.grayscale, defaultOutline.grayscale, defaultOutline.grayscale);

            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, grayscaleOutline);
        }
        else
        {
            text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, defaultOutline);
        }
    }



    public static void SetTMProUGUITextGrayscaleUnderlay(TextMeshProUGUI text, bool setGrayscale, Color defaultUnderlay)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        if (setGrayscale)
        {
            Color grayscaleUnderlay = new Color(defaultUnderlay.grayscale, defaultUnderlay.grayscale, defaultUnderlay.grayscale);

            text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, grayscaleUnderlay);
        }
        else
        {
            text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, defaultUnderlay);
        }
    }

    public static void SetTMProUGUITextFaceColor(TextMeshProUGUI text, Color faceColor)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        text.color = faceColor;
    }

    public static void SetTMProUGUITextOutlineColor(TextMeshProUGUI text, Color outlineColor)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
    }

    public static void SetTMProUGUITextFaceDilate(TextMeshProUGUI text, float thickness)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        text.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, thickness);
    }

    public static void SetTMProUGUITextOutlineThickness(TextMeshProUGUI text, float thickness)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
    }

    public static void SetTMProUGUITextUnderlayColor(TextMeshProUGUI text, Color underlayColor)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        text.fontMaterial.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
    }

    public static void SetTMProUGUITextOutlineActive(TextMeshProUGUI text, bool setActive)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }

        if (setActive)
        {
            text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Outline);
        }
        else
        {
            text.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Outline);
        }
    }

    public static void SetTMProUGUITextUnderlayActive(TextMeshProUGUI text, bool setActive)
    {
        if (text == null)
        {
            Debug.LogError("text is null");
            return;
        }
        if (setActive)
        {
            text.fontMaterial.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        }
        else
        {
            text.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Underlay);
        }
    }

    public static void SetLocalizeSecondaryTerm(I2.Loc.Localize localize, string secondaryTerm)
    {
        if (localize == null)
        {
            Debug.LogError("localize is null");
            return;
        }
        localize.SecondaryTerm = secondaryTerm;
    }

    public static void SetImageSpriteSecure(Image image, Sprite spriteToSet)
    {
        if (image == null)
        {
            Debug.LogError("image is null");
            return;
        }
        if (spriteToSet == null)
        {
            Debug.LogWarning("spriteToset is null");
        }
        image.sprite = spriteToSet;
    }

    public static void SetButtonOnClickActionSecure(Button button, UnityAction onClick)
    {
        if (button == null)
        {
            Debug.LogError("button is null");
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }

    public static void SetButtonClickSoundInactiveSecure(GameObject objectClicked, bool isInactive)
    {
        if (objectClicked == null)
        {
            Debug.LogError("objectClicked is null");
            return;
        }
        ButtonClickSound clickSoundController = objectClicked.GetComponent<ButtonClickSound>();

        if (clickSoundController == null)
        {
            Debug.LogError("clickSoundController is null");
            return;
        }
        clickSoundController.clickInactive = isInactive;
    }

    public static void PlayClickSoundSecure(GameObject objectClicked)
    {
        if (objectClicked == null)
        {
            Debug.LogError("objectClicked is null");
            return;
        }
        ButtonClickSound clickSoundController = objectClicked.GetComponent<ButtonClickSound>();

        if (clickSoundController == null)
        {
            Debug.LogError("clickSoundController is null");
            return;
        }
        clickSoundController.ClickSound();
    }

    #endregion

    #region Other Helpers

    public static string StringToLowerFirstUpper(string toChange)
    {
        toChange = toChange.ToLower();
        char[] c = toChange.ToCharArray();
        c[0] = char.ToUpper(c[0]);
        return new String(c);
    }

    public static string DivideStringIntoLines(string toDivide, int maxLineLength)
    {
        if (string.IsNullOrEmpty(toDivide) || maxLineLength < 1)
        {
            return "";
        }

        if (toDivide.Length <= maxLineLength)
        {
            return toDivide;
        }
        string[] toDivideArr = toDivide.Split(' ');
        if (toDivideArr.Length < 2)
        {
            return toDivide;
        }

        StringBuilder builtString = new StringBuilder();
        StringBuilder actualLine = new StringBuilder();

        for (int i = 0; i < toDivideArr.Length; ++i)
        {

            if (actualLine.Length + toDivideArr[i].Length < maxLineLength)
            {
                actualLine.Append(toDivideArr[i]);
                actualLine.Append(" ");
            }
            else if (toDivideArr[i].Length + 2 < maxLineLength)
            {
                --i;
                builtString.Append(actualLine);
                actualLine.Length = 0;
                actualLine.Capacity = 0;
                actualLine.Append(Environment.NewLine);
            }
            else
            {
                builtString.Append(actualLine);
                actualLine.Length = 0;
                actualLine.Capacity = 0;
                actualLine.Append(Environment.NewLine);
                actualLine.Append(toDivideArr[i]);
                actualLine.Append(" ");
            }
        }

        if (actualLine.Length > 1)
        {
            builtString.Append(actualLine);
        }

        return builtString.ToString();
    }

    #endregion

    #region Time & Date

    public static string GetFormattedTime(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        StringBuilder builder = new StringBuilder();
        if (timeSpan.Days != 0)
        {
            builder.Append(timeSpan.Days);
            builder.Append(" day ");
        }
        if (timeSpan.Hours != 0)
        {
            builder.Append(timeSpan.Hours);
            builder.Append(" h ");
        }
        if (timeSpan.Minutes != 0)
        {
            builder.Append(timeSpan.Minutes);
            builder.Append(" min ");
        }
        if (timeSpan.Seconds != 0 && timeSpan.Hours == 0)
        {
            builder.Append(timeSpan.Seconds);
            builder.Append(" sec");
        }

        if (builder.Length == 0)    //this is in case where timer reaches 0h0min0sec
            builder.Append("0 sec");

        return builder.ToString();
    }

    public static string GetFormattedShortTime(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        StringBuilder builder = new StringBuilder();
        if (timeSpan.Days != 0)
        {
            builder.Append(timeSpan.Days);
            builder.Append("d ");
        }
        if (timeSpan.Hours != 0)
        {
            builder.Append(timeSpan.Hours);
            builder.Append("h ");
        }
        else
        {
            if (timeSpan.Days != 0)
            {
                builder.Append(timeSpan.Minutes);
                builder.Append("m ");
            }
        }
        if (timeSpan.Minutes != 0)
        {
            if (timeSpan.Days == 0)
            {
                builder.Append(timeSpan.Minutes);
                builder.Append("m ");
            }
        }
        if (timeSpan.Hours < 1)
        {
            if (timeSpan.Seconds != 0 && timeSpan.Hours == 0 && timeSpan.Days == 0)
            {
                builder.Append(timeSpan.Seconds);
                builder.Append("s");
            }
        }
        if (builder.Length == 0)    //this is in case where timer reaches 0h0min0sec
            builder.Append("1s");

        return builder.ToString();
    }


    public static string GetFormattedShortTime(long seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        StringBuilder builder = new StringBuilder();
        if (timeSpan.Days != 0)
        {
            builder.Append(timeSpan.Days);
            builder.Append("d ");
        }
        if (timeSpan.Hours != 0)
        {
            builder.Append(timeSpan.Hours);
            builder.Append("h ");
        }
        if (timeSpan.Minutes != 0)
        {
            if (timeSpan.Days == 0)
            {
                builder.Append(timeSpan.Minutes);
                builder.Append("m ");
            }
        }
        if (timeSpan.Hours < 1)
        {
            if (timeSpan.Seconds != 0 && timeSpan.Hours == 0)
            {
                builder.Append(timeSpan.Seconds);
                builder.Append("s");
            }
        }
        if (builder.Length == 0)    //this is in case where timer reaches 0h0min0sec
            builder.Append("1s");

        return builder.ToString();
    }

    #endregion

}

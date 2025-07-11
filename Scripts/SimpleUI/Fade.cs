using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class Fade : MonoBehaviour
{

    static Fade instance;
    static bool isClickable = true;

    Image img;
    float fadeInTime = .5f;
    float fadeOutTime = .3f;


    void Awake()
    {
        instance = this;
        img = GetComponent<Image>();
        img.CrossFadeAlpha(0f, 0f, true);
    }

    public void ButtonClick()
    {
        //Debug.Log("FadeClicked " + isClicable);
        if (isClickable)
            UIController.get.FadeClicked();
    }

    void FadingIn(float colorMax)
    {
        img.raycastTarget = true;

        img.color = new Color(0, 0, 0, colorMax);
        gameObject.SetActive(true);

        img.CrossFadeAlpha(1f, fadeInTime, true);
    }

    void FadingOut()
    {
        img.raycastTarget = false;
        img.CrossFadeAlpha(0f, fadeOutTime, true);
    }

    void DisableGO()
    {
        gameObject.SetActive(false);
    }

    public static void FadeIn(int index)
    {
        if (instance != null)
        {
            instance.FadingIn(0.75f);
        }
        UpdateFadePosition(index);
    }

    public static void FadeOut()
    {
        if (instance != null)
        {
            instance.FadingOut();
        }
    }

    public static void UpdateFadePosition(int index)
    {
        if (instance != null)
        {
            if (index > 1)
                instance.transform.SetSiblingIndex(index - 1);
            else
                SetAsFirstSibling();
        }
        //instance.transform.SetAsFirstSibling();//instance.transform.parent.childCount - 2);
    }

    public static void SetClickable(bool value)
    {
        isClickable = value;
    }

    public static void SetAsFirstSibling()
    {
        instance.transform.SetAsFirstSibling();
    }
}

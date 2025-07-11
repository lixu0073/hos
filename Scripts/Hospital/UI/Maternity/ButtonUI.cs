using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ButtonUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI buttonLabel = null;

    [SerializeField]
    private Image buttonIcon = null;

    [SerializeField]
    private Button button = null;
    /// <summary>The action that will be executed when the button is clicked.</summary>
    private UnityAction onClickAction;

    [SerializeField]
    private Sprite defaultSprite = null;
    [SerializeField]
    private Sprite grayscaleSprite = null;

    public void SetButtonActive(bool setActive)
    {
        gameObject.SetActive(setActive);
    }
    /// <summary>
    /// Set the button.
    /// </summary>
    public void SetButton(UnityAction action, string buttonText = null, Sprite buttonIcon = null, bool isBlinking = false)
    {
        SetOnClickAction(action);
        SetButtonLabel(buttonText);
        SetButtonIcon(buttonIcon);
        SetButtonBlinking(isBlinking);
    }
    
    private void SetButtonBlinking(bool blink)
    {
        Image img = button.GetComponent<Image>();
        if(img != null)
        {
            if(blink)
            {
                TutorialUIController.Instance.BlinkImage(img);
            }
            else
            {
                TutorialUIController.Instance.StopBlinking();
            }
        }
    }
    /// <summary>
    /// Sets the action that will be executed when the button is clicked.
    /// </summary>
    /// <param name="action"></param>
    public void SetOnClickAction(UnityAction action)
    {
        onClickAction = action;
    }

    private void SetButtonLabel(string buttonText)
    {
        if (buttonLabel == null)
        {
            return;
        }

        buttonLabel.gameObject.SetActive(!string.IsNullOrEmpty(buttonText));

        buttonLabel.text = buttonText;
    }

    public void SetButtonIcon(Sprite icon)
    {
        if (buttonIcon == null)
        {
            return;
        }

        buttonIcon.gameObject.SetActive(icon != null);

        buttonIcon.sprite = icon;
    }
    /// <summary>
    /// Called when button is pressed. Set in Unity Editor
    /// </summary>
    public void OnClick()
    {
        if (onClickAction != null && button.interactable)
        {
            onClickAction();
        }
    }

    public void SetButtonGrayscale(bool isInteractive)
    {
        if (button != null)
        {
            SetMaterial(button.image, isInteractive);
        }
        if (buttonIcon != null)
        {
            SetMaterial(buttonIcon, isInteractive);
        }
    }

    public void SetButtonSprite(bool isGrayscale)
    {
        button.image.sprite = isGrayscale ? grayscaleSprite : defaultSprite;
    }

    private void SetMaterial(Image image, bool isInteractive)
    {
        image.material = isInteractive ? null : ResourcesHolder.Get().GrayscaleMaterial;
    }
}

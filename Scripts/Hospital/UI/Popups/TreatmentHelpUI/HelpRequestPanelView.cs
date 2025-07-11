using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class HelpRequestPanelView : MonoBehaviour {

    [SerializeField]
    private Button askForHelpButton = null;
    [SerializeField]
    private Image buttonsHelpBadge = null;
    [SerializeField]
    private TextMeshProUGUI availableHelpRequestsText = null;
    [SerializeField]
    private TextMeshProUGUI helpRequestedText = null;

    [SerializeField]
    private GameObject activeHelpRequestPanel = null;

    [SerializeField]
    private Animator activeHelpRequestPanelAnimator = null;


    private Color defaultFaceColor;
    private Color defaultOutlineColor;
    private Color defaultUnderlayColor;

    private bool isAskForHelpButtonBlinking = false;

    private void Awake() {
       
        defaultFaceColor = availableHelpRequestsText.fontMaterial.GetColor(ShaderUtilities.ID_FaceColor);
        defaultOutlineColor = availableHelpRequestsText.fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
        defaultUnderlayColor = availableHelpRequestsText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
        SetHelpRequestedTextColorMagenta();
    }
    
    public void ShowAskForHelpButton() {
        SetActiveHelpRequestPanelActive(false);
        SetAskForHelpButtonActive(true);
    }

    public void ShowActiveHelpRequestPanel() {
        SetActiveHelpRequestPanelActive(true);
        SetAskForHelpButtonActive(false);
        ScaleUpActiveHelpRequestPanel(false);
    }

    /// <summary>
    /// Use it in treatment help request controller to set AskForHelpButton
    /// </summary>
    /// <param name="amountLeft"></param>
    /// <param name="maxAmount"></param>
    /// <param name="action"></param>
    public void SetAvailableHelpRequestsButton(int amountLeft, int maxAmount, UnityAction action) {
        SetAskForHelpRequestButtonOnclickAction(action);
        SetAskForHelpButtonGrayscale(amountLeft < 1);
        SetAvailableHelpRequestsText(amountLeft.ToString() + "/" + maxAmount.ToString());
    }

    public void SetHelpRequestedTextColorMagenta()
    {
        UIController.SetTMProUGUITextOutlineActive(helpRequestedText, true);
        UIController.SetTMProUGUITextUnderlayActive(helpRequestedText, false);
        UIController.SetTMProUGUITextOutlineColor(helpRequestedText, BaseUIController.magentaColor);
        UIController.SetTMProUGUITextFaceDilate(helpRequestedText, BaseUIController.pinkDilate);
        UIController.SetTMProUGUITextOutlineThickness(helpRequestedText, BaseUIController.pinkOutlineThickness);
    }

    public void SetHelpRequestedText(bool isHelpCompleted) {
        if (isHelpCompleted) {
            UIController.SetTMProUGUITextSecure(helpRequestedText, I2.Loc.ScriptLocalization.Get("DONATE_COMPLETED"));
        } else {
            UIController.SetTMProUGUITextSecure(helpRequestedText, I2.Loc.ScriptLocalization.Get("TREATMENT_HELP_REQUESTED"));
        }
    }

    public void BlinkAskForHelpButton(bool isBlinking)
    {
        if (isBlinking)
        {
            TutorialUIController.Instance.BlinkImage(askForHelpButton.image, 1.1f, true);
        } else if (isAskForHelpButtonBlinking) {
            TutorialUIController.Instance.StopBlinking();
        }
        isAskForHelpButtonBlinking = isBlinking;

    }

    public void ScaleUpActiveHelpRequestPanel(bool scaleUp) {
        activeHelpRequestPanel.gameObject.SetActive(false);
        activeHelpRequestPanel.transform.localScale = new Vector3(1, 1, 1);
        activeHelpRequestPanel.gameObject.SetActive(true);

        if (scaleUp)
        {
            activeHelpRequestPanelAnimator.SetTrigger("scaleUp");
        }
    }


    private void SetAskForHelpRequestButtonOnclickAction(UnityAction action) {
        UIController.SetButtonOnClickActionSecure(askForHelpButton, () => 
        {
            UIController.PlayClickSoundSecure(askForHelpButton.gameObject);
            action.Invoke();
        });
    }

    private void SetAvailableHelpRequestsText(string toSet) {
        UIController.SetTMProUGUITextSecure(availableHelpRequestsText, toSet);
    }

    private void SetAskForHelpButtonGrayscale(bool setGrayscale) {
        SetAskForHelpButtonBackgroundGrayscale(setGrayscale);
        SetAvailableHelpRequestsTextGrayscale(setGrayscale);
        SetButtonsHelpBadgeGrayscale(setGrayscale);
        UIController.SetButtonClickSoundInactiveSecure(askForHelpButton.gameObject, setGrayscale);
    }

    private void SetAskForHelpButtonBackgroundGrayscale(bool setGrayscale) {
        if (setGrayscale)
        {
            UIController.SetImageSpriteSecure(askForHelpButton.image, ResourcesHolder.Get().blue9SliceButton);
        }
        else {
            UIController.SetImageSpriteSecure(askForHelpButton.image, ResourcesHolder.Get().pink9SliceButton);
        }

        UIController.SetImageGrayscale(askForHelpButton.image, setGrayscale);
    }

    private void SetAvailableHelpRequestsTextGrayscale(bool setGrayscale) {
        UIController.SetTMProUGUITextGrayscaleFace(availableHelpRequestsText, setGrayscale, defaultFaceColor);
        UIController.SetTMProUGUITextGrayscaleOutline(availableHelpRequestsText, setGrayscale, defaultOutlineColor);
        UIController.SetTMProUGUITextGrayscaleUnderlay(availableHelpRequestsText, setGrayscale, defaultUnderlayColor);
    }

    private void SetButtonsHelpBadgeGrayscale(bool setGrayscale) {
        UIController.SetImageGrayscale(buttonsHelpBadge, setGrayscale);
    }

    private void SetAskForHelpButtonActive(bool setActive)
    {
        UIController.SetGameObjectActiveSecure(askForHelpButton.gameObject, setActive);
    }

    private void SetActiveHelpRequestPanelActive(bool setActive) {
        UIController.SetGameObjectActiveSecure(activeHelpRequestPanel, setActive);
    }
}

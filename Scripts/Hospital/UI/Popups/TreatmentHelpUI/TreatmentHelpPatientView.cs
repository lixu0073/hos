using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TreatmentHelpPatientView : MonoBehaviour
{
    [SerializeField]
    private GameObject selectedIndicator = null;
#pragma warning disable 0414
    [SerializeField]
    private GameObject regularBackground = null;
#pragma warning restore 0414
    [SerializeField]
    private GameObject helpPossibleIndicator = null;
    [SerializeField]
    private GameObject helpReadyIndicator = null;

    [SerializeField]
    private Image avatarHead = null;
    [SerializeField]
    private Image avatarBody = null;

    [SerializeField]
    private Button patientButton = null;
#pragma warning disable 0414
    [SerializeField]
    private BacteriaAvatarBackground bacteriaBackGround = null;
#pragma warning restore 0414

    public void SetTreatmentHelpPatientView(HospitalCharacterInfo info, bool isSelected,bool isHelpReady, bool isHelpPossible, UnityAction action)
    {
        HospitalCharacterInfo infectedBy = null;
        SetBackGround(info.HasBacteria, info.GetTimeTillInfection(out infectedBy) > 0);
        SetAvatarHead(info.AvatarHead);
        SetAvatarBody(info.AvatarBody);
        SetPatientButtonOnClickAction(action);
        SetSelected(isSelected);
        SetHelpPossibleIndicatorActive(isHelpPossible && !isHelpReady ? true : false);
        SetHelpReadyIndicatorActive(isHelpReady);
    }

    public void SetSelected(bool setSelected)
    {
        UIController.SetGameObjectActiveSecure(selectedIndicator, setSelected);

        if (setSelected)
        {
            transform.localScale = new Vector3(1.05f, 1.05f, 1);
        }
        else
        {
            transform.localScale = new Vector3(1.05f, 1.05f, 1);
        }
    }

    public void SetHelpPossibleIndicatorActive(bool setActive)
    {
        UIController.SetGameObjectActiveSecure(helpPossibleIndicator, setActive);
    }

    public void SetHelpReadyIndicatorActive(bool setActive)
    {
        UIController.SetGameObjectActiveSecure(helpReadyIndicator, setActive);
    }

    private void SetBackGround(bool isInfected, bool becomeInfected)
    {
        // Disabled 
        /*
        UIController.SetGameObjectActiveSecure(regularBackground, !isInfected && !becomeInfected);
        if (isInfected) {
            bacteriaBackGround.SetBlinking(true, 0, false);
            return;
        }
        if (becomeInfected)
        {
            bacteriaBackGround.SetBlinking(true, 1, false);
            return;
        }
        bacteriaBackGround.SetBlinking(true, 0, false);
        */
    }

    private void SetAvatarHead(Sprite headSprite)
    {
        UIController.SetImageSpriteSecure(avatarHead, headSprite);
    }

    private void SetAvatarBody(Sprite bodySprite)
    {
        UIController.SetImageSpriteSecure(avatarBody, bodySprite);
    }

    private void SetPatientButtonOnClickAction(UnityAction action)
    {
        UIController.SetButtonOnClickActionSecure(patientButton, () => {
            UIController.PlayClickSoundSecure(patientButton.gameObject);
            action();
        });
    }

}

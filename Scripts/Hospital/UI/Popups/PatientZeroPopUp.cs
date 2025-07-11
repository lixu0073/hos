using UnityEngine.UI;
using TMPro;
using SimpleUI;
using TutorialSystem;
using System.Collections.Generic;

public class PatientZeroPopUp : UIElement, ITutorialTrigger
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI diseaseText;
    public TextMeshProUGUI infoText;
    public Image diseaseImage;
    public Image doctorRoomImage;
    public Image avatarHeadImage;
    public Image avatarBodyImage;

    public Image okButtonImage;

    public Dictionary<string, TutorialTriggerEvent> triggerEvents = new Dictionary<string, TutorialTriggerEvent>()
    {
        {"Popup_Opened", new TutorialTriggerEvent()},
        {"Popup_Closed", new TutorialTriggerEvent()}
    };
    public Dictionary<string, TutorialTriggerEvent> TriggerEvents
    {
        get
        {
            return triggerEvents;
        }
    }

    public void Open(ClinicCharacterInfo info)
    {
        CoroutineInvoker.Instance.StartCoroutine(base.Open(true, false, () =>
        {
            SetPatientData(info);

            NotificationCenter.Instance.PatientZeroOpen.Invoke(new PatientZeroOpenEventArgs(info));

            if (Game.Instance.gameState().GetHospitalLevel() <= 5)    //yellow and green patient (they are in tutorial)
                TutorialUIController.Instance.BlinkImage(UIController.getHospital.PatientZeroPopUp.okButtonImage, 1.2f);

            if (TriggerEvents.ContainsKey("Popup_Opened") && TriggerEvents["Popup_Opened"] != null)
                TriggerEvents["Popup_Opened"].Invoke(this, new TutorialTriggerArgs());

            SoundsController.Instance.PlayButtonClick2();
        }));
    }

    void SetPatientData(ClinicCharacterInfo info)
    {
        if (!info.Name.Contains("_"))        
            nameText.text = info.Name + " " + info.Surname;        
        else        
            nameText.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + info.Surname);
        
        diseaseText.text = I2.Loc.ScriptLocalization.Get(info.clinicDisease.Name);
        infoText.text = string.Format(I2.Loc.ScriptLocalization.Get("TO_CURE_BUILD"), I2.Loc.ScriptLocalization.Get(info.clinicDisease.Name).ToUpper(), I2.Loc.ScriptLocalization.Get(info.clinicDisease.Doctor.ShopTitle).ToUpper());
        avatarHeadImage.sprite = info.AvatarHead;
        avatarBodyImage.sprite = info.AvatarBody;
        diseaseImage.sprite = info.clinicDisease.DiseasePic;
        doctorRoomImage.sprite = info.clinicDisease.Doctor.ShopImage;
    }

    public override void Exit(bool hidePopupWithShowMainUI = true)
    {
        base.Exit(hidePopupWithShowMainUI);
        NotificationCenter.Instance.PatientZeroClosed.Invoke(new PatientZeroClosedEventArgs());
        if (TriggerEvents.ContainsKey("Popup_Closed") && TriggerEvents["Popup_Closed"] != null)
            TriggerEvents["Popup_Closed"].Invoke(this, new TutorialTriggerArgs());
    }

    public void ButtonExit()
    {
        Exit();
    }
}

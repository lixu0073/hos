using UnityEngine;
using TMPro;
using System;

public class HospitalNameTab : PopupTab
{
    [SerializeField] private TMP_InputField nameInputField = null;
    [SerializeField] private TextMeshProUGUI nameDescription = null;

    protected override void OnPopupOpen()
    {
        SetHospitalNameDescription();
    }

    protected override void PrepareContent()
    {
        UIController.getHospital.hospitalSignPopup.SetDescriptionTextActive(false);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonActive(true);
        UIController.getHospital.hospitalSignPopup.SetConfirmButtonInteractable(true);
        UIController.getHospital.hospitalSignPopup.SetBuyButtonActive(false);

        //nameInputField.shouldHideMobileInput = true;

        SetPlaceholderName(GameState.Get().HospitalName);
        SetNameInputFieldText(GameState.Get().HospitalName);
        //nameInputField.text = "";
#if !UNITY_ANDROID || UNITY_EDITOR
        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.name_hospital_open || TutorialController.Instance.CurrentTutorialStepTag == StepTag.name_hospital_close)
        {
            nameInputField.ActivateInputField();
        }
#endif
        Debug.Log("HospitalNameTab prepared");
    }

    public void HospitalNameChanged(string input)
    {
        if (input.Length > 0)
        {
            char incorporatedCharacter = input[input.Length - 1];
            if (input[0] == ' ')
            {
                input = input.Remove(input.Length - 1);
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("HOSPITAL_NAME_NOT_VALID"));
            }
            else if (!Char.IsLetterOrDigit(incorporatedCharacter) && !Char.IsWhiteSpace(incorporatedCharacter))
            {
                input = input.Remove(input.Length - 1);
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("HOSPITAL_NAME_NOT_VALID"));
            }
        }
        Debug.LogWarning(input);
        nameInputField.text = input;
        nameInputField.text = nameInputField.text.ReplaceBadWords();
        ReferenceHolder.GetHospital().signControllable.SetTempHospitalName(nameInputField.text);

        UIController.getHospital.hospitalSignPopup.SetPreviewName();
    }

    public int GetNameCharLimit()
    {
        return nameInputField.characterLimit;
    }

    private void SetHospitalNameDescription()
    {
        string tempText = UIController.StringToLowerFirstUpper(I2.Loc.ScriptLocalization.Get("HOSPITAL_NAME"));

        nameDescription.SetText(tempText);
    }

    private void SetPlaceholderName(string placeholderName)
    {
        if (string.IsNullOrEmpty(placeholderName))
            nameInputField.placeholder.GetComponent<TextMeshProUGUI>().SetText("My Hospital");
        else
            nameInputField.placeholder.GetComponent<TextMeshProUGUI>().SetText(placeholderName);
    }

    private void SetNameInputFieldText(string name)
    {
        nameInputField.text = (string.IsNullOrEmpty(name)) ? "" : name;
    }

    protected override void OnPopupClose()
    {
    }

    protected override void OnTabSwitchFromCurrent()
    {
    }
}

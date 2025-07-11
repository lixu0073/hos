using SimpleUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Maternity;

public class MaternityInfoPopup : UIElement
{

    [Header("BloodTest Info References")]
    public TextMeshProUGUI bloodTestInfoLeftText;
    public TextMeshProUGUI bloodTestInfoRightText;
    public Image bloodTestInfoImage;
    public Image bloodTestResultImage;
    public TextMeshProUGUI coinCostText;
    public TextMeshProUGUI timeLenghtText;
    public TextMeshProUGUI bloodTestedAmountText;

    public void OpenMaternityInfo(MaternityBloodTestRoomInfo roomInfo)
    {
        gameObject.SetActive(true);
        StartCoroutine(base.Open(true, true, () =>
        {
            SetupView(roomInfo);
        }));
    }

    private void SetupView(MaternityBloodTestRoomInfo roomInfo)
    {
        coinCostText.text = roomInfo.GetCoinCost().ToString();
        timeLenghtText.text = roomInfo.GetDiagnoseTime().ToString();
        bloodTestInfoLeftText.text = I2.Loc.ScriptLocalization.Get(roomInfo.BloodTestRoomDecription);
        bloodTestInfoRightText.text = I2.Loc.ScriptLocalization.Get(roomInfo.InfoDescription);
        bloodTestInfoImage.sprite = roomInfo.ShopImage;
        bloodTestedAmountText.text = MaternityBloodTestRoomController.Instance.GetBloodTestRoom().GetAmountOfBloodTestsPerformed().ToString();
    }

    public void SeePatientCardButtonOnClick()
    {
        Debug.Log("Info ButtonSeePatientCards");
        ShowDiagnosePatientCard();
    }

    private void ShowDiagnosePatientCard()
    {
        MaternityPatientAI patientThatNeedDiagnosis = null;
        patientThatNeedDiagnosis = FindPatientWaitingForBloodTest(patientThatNeedDiagnosis);
        MaternityWaitingRoomBed bedToMoveCameraTo = MaternityWaitingRoomController.Instance.GetBedForPatient(patientThatNeedDiagnosis);
        Exit();
        if (bedToMoveCameraTo != null)
            UIController.getMaternity.patientCardController.Open(bedToMoveCameraTo, true, false);
        else
            StartCoroutine(UIController.getMaternity.patientCardController.Open(true, false));
    }

    private MaternityPatientAI FindPatientWaitingForBloodTest(MaternityPatientAI patientThatNeedDiagnosis)
    {
        foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
        {
            if (patient.Person.State.GetTag() == Maternity.PatientStates.MaternityPatientStateTag.WFSTD)
                patientThatNeedDiagnosis = patient;
        }

        return patientThatNeedDiagnosis;
    }

    public void ButtonExit()
    {
        Exit(false);
    }

    internal void RefreshPopup()
    {
        Debug.LogError("Refresh popup");
    }
}

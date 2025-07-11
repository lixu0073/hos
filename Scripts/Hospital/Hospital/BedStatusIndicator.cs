using UnityEngine;
using UnityEngine.UI;
using Hospital;

public enum BedStatus
{
    None,
    New,
    Cure,
    Wait,
    Follow,
    DiagnosisRequired,
    LastMedicine,
    CureHelped,
    HelpRequested,
    HelpRequestFulfilled
}

public enum HelpStatus
{
    None,
    HelpRequested,
    HelpRequestFulfilled
}

public class BedStatusIndicator : MonoBehaviour
{
    BedStatus status;

    [SerializeField] GameObject cureIndicator = null;
    [SerializeField] GameObject newIndicator = null;
    [SerializeField] GameObject waitIndicator = null;
    [SerializeField] GameObject followIndicator = null;
    [SerializeField] GameObject diagnosisRequredIndicator = null;
    [SerializeField] GameObject lastMedicineIndicator = null;
    [SerializeField] GameObject cureHelpedIndicator = null;
    [SerializeField] GameObject helpRequestedIndicator = null;
    [SerializeField] GameObject helpRequestFulfilledIndicator = null;

    [SerializeField] RectTransform followIndicatorIcon = null;
    [SerializeField] RectTransform diagnosisRequredIndicatorIcon = null;
    [SerializeField] RectTransform lastMedicineIndicatorIcon = null;
    [SerializeField] GameObject plagueIndicator = null;
#pragma warning disable 0414
    [SerializeField] SpriteRenderer plagueIndicatorIcon = null;
#pragma warning restore 0414
    [SerializeField] SpriteRenderer plagueBiohazardIndicatorIcon = null;
    [SerializeField] Animator plagueIndicatorAnimator = null;

    public void ClearIndicator()
    {
        HideAllIndicators();
    }

    private HospitalBedController.HospitalBed bed;

    public bool ReadyToCure()
    {
        return status == BedStatus.Cure;
    }

    public void SetIndicator(HospitalBedController.HospitalBed bed, BedStatus status, Sprite image = null)
    {
        this.status = status;
        this.bed = bed;

        if (Game.Instance.gameState().GetHospitalLevel() < 3)
            return;

        switch (status)
        {
            case BedStatus.None:
                HideAllIndicators();
                break;
            case BedStatus.New:
                SetNewIndicator();
                break;
            case BedStatus.Cure:
                SetCureIndicator();
                cureIndicator.GetComponent<Button>().onClick.AddListener(() =>
                {
                    //  ReportClick();
                });
                break;
            case BedStatus.Wait:
                SetWaitIndicator();
                HidePlagueIndicator();
                break;
            case BedStatus.Follow:
                SetFollowIndicator();
                HidePlagueIndicator();
                break;
            case BedStatus.DiagnosisRequired:
                SetDiagnosisRequiredIndicator();
                break;
            case BedStatus.LastMedicine:
                SetLastMedicineIndicator(image);
                break;
            case BedStatus.CureHelped:
                SetCureHelpedIndicator();
                break;
            case BedStatus.HelpRequested:
                SetHelpRequestedIndicator();
                break;
            case BedStatus.HelpRequestFulfilled:
                SetHelpRequestFulfilledIndicator();
                break;
            default:
                HideAllIndicators();
                HidePlagueIndicator();
                break;
        }
    }

    void HideAllIndicators()
    {
        if (cureIndicator != null)
            cureIndicator.SetActive(false);

        if (newIndicator != null)
            newIndicator.SetActive(false);

        if (waitIndicator != null)
            waitIndicator.SetActive(false);

        if (followIndicator != null)
            followIndicator.SetActive(false);

        if (diagnosisRequredIndicator != null)
            diagnosisRequredIndicator.SetActive(false);

        if (lastMedicineIndicator != null)
            lastMedicineIndicator.SetActive(false);

        if (cureHelpedIndicator != null)
            cureHelpedIndicator.SetActive(false);

        if (helpRequestedIndicator != null)
            helpRequestedIndicator.SetActive(false);

        if (helpRequestFulfilledIndicator != null)
            helpRequestFulfilledIndicator.SetActive(false);
    }

    void SetCureIndicator()
    {
        if (cureIndicator != null && cureIndicator.activeSelf)
            return;

        HideAllIndicators();

        if (cureIndicator != null)
            cureIndicator.SetActive(true);
    }

    void SetNewIndicator()
    {
        if (newIndicator != null && newIndicator.activeSelf)
            return;

        HideAllIndicators();

        if (newIndicator != null)
            newIndicator.SetActive(true);
    }

    void SetWaitIndicator()
    {
        if (waitIndicator != null && waitIndicator.activeSelf)
            return;

        HideAllIndicators();
        if (waitIndicator != null)
        {
            waitIndicator.SetActive(true);

            //this is a fix for clock animation on indicator going coutnerclockwise when a room is in mirrored rotation.
            if (waitIndicator.transform.parent.lossyScale.x < 0)
                waitIndicator.transform.localScale = new Vector3(-1, 1, 1);
            else
                waitIndicator.transform.localScale = Vector3.one;
        }
    }

    public void SetPlagueIndicator(bool isGettingInfection)
    {
        if (plagueIndicatorAnimator.isActiveAndEnabled)
            plagueIndicatorAnimator.SetBool("isGettingInfection", isGettingInfection);

        if (plagueIndicator != null && plagueIndicator.activeSelf)
            return;

        plagueIndicator.SetActive(true);
    }

    public void HidePlagueIndicator()
    {
        if (plagueIndicator != null && !plagueIndicator.activeSelf)
            return;

        plagueIndicator.SetActive(false);
    }

    void SetFollowIndicator()
    {
        if (followIndicator.activeSelf)
            return;

        HideAllIndicators();
        followIndicator.SetActive(true);
    }

    void SetDiagnosisRequiredIndicator()
    {
        if (diagnosisRequredIndicator == null)
            return;
        if (diagnosisRequredIndicator.activeSelf)
            return;

        HideAllIndicators();
        diagnosisRequredIndicator.SetActive(true);
    }

    void SetLastMedicineIndicator(Sprite image)
    {
        if (lastMedicineIndicator == null)
            return;

        if (lastMedicineIndicator.activeSelf)
            return;

        HideAllIndicators();
        lastMedicineIndicatorIcon.GetComponent<Image>().sprite = image;
        lastMedicineIndicator.SetActive(true);
    }

    void SetCureHelpedIndicator()
    {
        if (cureHelpedIndicator != null && cureHelpedIndicator.activeSelf)
            return;

        HideAllIndicators();

        if (cureHelpedIndicator != null)
            cureHelpedIndicator.SetActive(true);
    }

    void SetHelpRequestedIndicator()
    {
        if (helpRequestedIndicator != null && helpRequestedIndicator.activeSelf)
            return;

        HideAllIndicators();

        if (helpRequestedIndicator != null)
        {
            helpRequestedIndicator.SetActive(true);
            if (helpRequestedIndicator.transform.parent.lossyScale.x < 0)
                helpRequestedIndicator.transform.localScale = new Vector3(-1, 1, 1);
            else
                helpRequestedIndicator.transform.localScale = Vector3.one;
        }
    }

    void SetHelpRequestFulfilledIndicator()
    {
        if (helpRequestFulfilledIndicator != null && helpRequestFulfilledIndicator.activeSelf)
            return;

        HideAllIndicators();

        if (helpRequestFulfilledIndicator != null)
        {
            helpRequestFulfilledIndicator.SetActive(true);
            if (helpRequestFulfilledIndicator.transform.parent.lossyScale.x < 0)
                helpRequestFulfilledIndicator.transform.localScale = new Vector3(-1, 1, 1);
            else
                helpRequestFulfilledIndicator.transform.localScale = Vector3.one;
        }
    }

    public void OpenPatientCard()
    {
        if (bed != null)
        {
            int bedID = HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedID(bed);

            if (Hospital.HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                var patients = ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.GetPatientsWithRequiredHelp();

                if (bed.Patient != null && bedID < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
                {
                    for (int i = 0; i < patients.Count; i++)
                    {
                        if (patients[i].ID == ((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>().ID)
                        {
                            HospitalDataHolder.Instance.Emergency.OpenTreatmentDonatePopup(patients, i);
                            return;
                        }
                    }
                }

                HospitalDataHolder.Instance.Emergency.OpenTreatmentDonatePopup(patients);
            }
            else
            {
                if (Game.Instance.gameState().GetHospitalLevel() < 3)
                {
                    MessageController.instance.ShowMessage(22);
                }
                else if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.patient_card_open))
                {
                    //this is for time when first patient is on his way to the bed
                    MessageController.instance.ShowMessage(16);
                }
                else
                {
                    if (bed.Patient != null && bedID < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
                        UIController.getHospital.PatientCard.Open(((BasePatientAI)bed.Patient).GetComponent<HospitalCharacterInfo>(), bedID);
                    else if (bedID >= 0)
                        UIController.getHospital.PatientCard.Open(null, bedID);
                }
            }
        }

    }

    public BedStatus GetIndicatorStatus()
    {
        return status;
    }

    public void FlipXBadges(bool flipX)
    {
        followIndicatorIcon.localScale = new Vector3((flipX == true ? 1 : -1) * followIndicatorIcon.localScale.x, followIndicatorIcon.localScale.y, followIndicatorIcon.localScale.z);
        diagnosisRequredIndicatorIcon.localScale = new Vector3((flipX == true ? 1 : -1) * diagnosisRequredIndicatorIcon.localScale.x, diagnosisRequredIndicatorIcon.localScale.y, diagnosisRequredIndicatorIcon.localScale.z);
        lastMedicineIndicatorIcon.localScale = new Vector3((flipX == true ? 1 : -1) * lastMedicineIndicatorIcon.localScale.x, lastMedicineIndicatorIcon.localScale.y, lastMedicineIndicatorIcon.localScale.z);
        plagueIndicator.transform.localPosition = new Vector3(plagueIndicator.transform.localPosition.x, plagueIndicator.transform.localPosition.y, plagueIndicator.transform.localPosition.z);
        //plagueIndicatorIcon.flipX = flipX;
        plagueBiohazardIndicatorIcon.flipX = flipX;

        Quaternion rotBiohazard = plagueBiohazardIndicatorIcon.transform.localRotation;

        // rotBiohazard.x = flipX ? rotBiohazard.x : -rotBiohazard.x;
        // rotBiohazard.z = flipX ? rotBiohazard.z : -rotBiohazard.z;

        plagueBiohazardIndicatorIcon.transform.localRotation = rotBiohazard;
        //plagueBiohazardIndicatorIcon.transform.localPosition = new Vector3(flipX ? -plagueBiohazardIndicatorIcon.transform.localPosition.x : plagueBiohazardIndicatorIcon.transform.localPosition.x, plagueBiohazardIndicatorIcon.transform.localPosition.y, plagueBiohazardIndicatorIcon.transform.localPosition.z); ;
    }
}

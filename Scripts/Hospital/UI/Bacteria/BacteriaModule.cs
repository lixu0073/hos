using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
using UnityEngine.EventSystems;

public class BacteriaModule : MonoBehaviour 
{
#pragma warning disable 0649
    [SerializeField] GameObject[] spineBacteria;
    [SerializeField] Image statusIcon;
    [SerializeField] Sprite statusSpriteSpreading;
    [SerializeField] Sprite statusSpriteClear;

    [SerializeField] TextMeshProUGUI bacteriaTimer;
    [SerializeField] GameObject bonusGroup;
    
    [SerializeField] Animator infoTooltipAnim;
    [SerializeField] GameObject infoTooltipCureGroup;
    [SerializeField] GameObject infoTooltipSpreadGroup;
    [SerializeField] TextMeshProUGUI infoTooltipBacteriaName;
    [SerializeField] TextMeshProUGUI infoTooltipTimer;
    [SerializeField] TextMeshProUGUI infoTooltipReward;
    [SerializeField] TextMeshProUGUI infoTooltipPatientName;
    [SerializeField] Image infoTooltipAvatarHead;
    [SerializeField] Image infoTooltipAvatarBody;

    [SerializeField] GameObject textTooltip;
    [SerializeField] TextMeshProUGUI textTooltipName;
    [SerializeField] TextMeshProUGUI textTooltipDescription;
    [SerializeField] Animator statusAnim;
#pragma warning restore 0649
    HospitalCharacterInfo selectedInfo;     //patient who is currently selected on PatientCard
    HospitalCharacterInfo spreaderInfo;     //patient who is spreating bacteria to selected patient
    int timeRemaining = 0;
    int positiveReward = 1;
    public bool tutorialInfoLocked = false;
#pragma warning disable 0649
    [SerializeField]
    private EventTrigger trigger;
#pragma warning restore 0649

    void OnEnable()
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void SetInfos(HospitalCharacterInfo selectedInfo, HospitalCharacterInfo spreaderInfo)
    {
        this.selectedInfo = selectedInfo;
        this.spreaderInfo = spreaderInfo;
    }

    public void SetPositiveEnergy(int positive)
    {
        positiveReward = positive;
        infoTooltipReward.text = "+" + positiveReward;
    }

    public void SetBacteriaIcon(bool isSpreading, int bacteriaId = -1)
    {
        spineBacteria[0].SetActive(false);
        spineBacteria[1].SetActive(false);
        spineBacteria[2].SetActive(false);
        statusIcon.gameObject.SetActive(false);

        if (bacteriaId >= 0)
        {
            spineBacteria[bacteriaId].SetActive(true);
            statusAnim.SetBool("IsBumping", false);
        }
        else if (isSpreading)
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 42);
            statusIcon.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 42);
            statusIcon.sprite = statusSpriteSpreading;
            statusAnim.SetBool("IsBumping", true);
        }
        else
        {
            statusIcon.gameObject.SetActive(true);
            statusIcon.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 42);
            statusIcon.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 42);
            statusIcon.sprite = statusSpriteClear;
            statusAnim.SetBool("IsBumping", false);
        }
    }

    public void SetDiseaseTooltip(DiseaseDatabaseEntry disease, MedicineRef medicine)
    {
        infoTooltipBacteriaName.text = I2.Loc.ScriptLocalization.Get(disease.Name);
        textTooltipName.text = I2.Loc.ScriptLocalization.Get(disease.Name);
        textTooltipDescription.text = I2.Loc.ScriptLocalization.Get("TOOLTIP_CURED_WITH") + " " + I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().GetMedicineInfos(medicine).Name);

    }

    public void SetTimer(int timeRemaining)
    {
        this.timeRemaining = timeRemaining;

        if (timeRemaining > 0)
            bacteriaTimer.text = UIController.GetFormattedShortTime(timeRemaining);
        else
            bacteriaTimer.text = "";

        SetBonusInfo();
    }

    public void SetBonusInfo()
    {
        if(selectedInfo && selectedInfo.HasBacteria && timeRemaining > 0)
            bonusGroup.SetActive(true);
        else
            bonusGroup.SetActive(false);
    }
    
    public void InfoButtonDown()
    {
        ShowTooltip();        
    }

    public void InfoButtonUp()
    {
        HideTooltip();
    }

    void ShowTooltip()
    {
        if (!selectedInfo)
            return;

        if (selectedInfo.HasBacteria && timeRemaining <= 0)
        {
            textTooltip.SetActive(true);
            return;
        }

        if (!selectedInfo.HasBacteria && timeRemaining <= 0)
        {
            MessageController.instance.ShowMessage(52,true);
            return;
        }

        infoTooltipAnim.SetBool("Show", true);
        SoundsController.Instance.PlayInfoButton();
        AnalyticsController.instance.ReportButtonClick("patient_card", "bacteria_info");
        StartCoroutine(UpdateTooltipTimer());

        if (selectedInfo.HasBacteria && timeRemaining > 0)
        {
            infoTooltipCureGroup.SetActive(true);
            infoTooltipSpreadGroup.SetActive(false);
            infoTooltipReward.text = "+" + positiveReward;
        }
        else
        {
            infoTooltipCureGroup.SetActive(false);
            infoTooltipSpreadGroup.SetActive(true);

            infoTooltipPatientName.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + spreaderInfo.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + spreaderInfo.Surname);
            infoTooltipAvatarHead.sprite = spreaderInfo.AvatarHead;
            infoTooltipAvatarBody.sprite = spreaderInfo.AvatarBody;
        }
    }

    IEnumerator UpdateTooltipTimer()
    {
        while (infoTooltipAnim.GetBool("Show"))
        {
            infoTooltipTimer.text = bacteriaTimer.text;
            yield return null;
        }
    }

    public void HideTooltip()
    {
        if (!tutorialInfoLocked)
            infoTooltipAnim.SetBool("Show", false);

        textTooltip.SetActive(false);
    }

    internal void SetEventListener(MedicineDatabaseEntry medicineDatabaseEntry)
    {
        trigger.triggers[2].callback.RemoveAllListeners();
        trigger.triggers[2].callback.AddListener((eventData) =>
        {
            if (!UIController.getHospital.PatientCard.showedMedicineDatabase.Contains(medicineDatabaseEntry))
            {
                
                UIController.getHospital.PatientCard.showedMedicineDatabase.Add(medicineDatabaseEntry);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnAPatient));
            }
        }
        );
    }

    public class SetUpBacteriaData
    {
        public bool isSpreading;
        public int bacteriaID;
        public MedicineDatabaseEntry patientMedicines;
        public DiseaseDatabaseEntry disease;
        public MedicineRef medicine;

        public SetUpBacteriaData(bool isSpreading, int bacteriaID, MedicineDatabaseEntry patientMedicines, DiseaseDatabaseEntry disease, MedicineRef medicine)
        {
            this.isSpreading = isSpreading;
            this.bacteriaID = bacteriaID;
            this.patientMedicines = patientMedicines;
            this.disease = disease;
            this.medicine = medicine;
        }
    }
}
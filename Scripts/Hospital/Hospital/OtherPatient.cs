using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MovementEffects;
using Hospital;


public class OtherPatient : MonoBehaviour {

    [SerializeField]
    GameObject newIndicator = null;
    [SerializeField]
    GameObject diagnosisIndicator = null;
    [SerializeField]
    GameObject cureIndicator = null;
    [SerializeField]
    GameObject cureHelpIndicator = null;
    [SerializeField]
    GameObject helpIndicator = null;
    [SerializeField]
    GameObject selectedIndicator = null;
    [SerializeField]
    GameObject clockBadge = null;
    [SerializeField]
    GameObject footBadge = null;
    [SerializeField]
    GameObject avatar = null;
    [SerializeField]
    Image avatarBody = null;
    [SerializeField]
    Image avatarHead = null;
    [SerializeField]
    GameObject rewardArea = null;
    [SerializeField]
    TextMeshProUGUI expRewardAmount = null;
    [SerializeField]
    TextMeshProUGUI coinRewardAmount = null;
    [SerializeField]
    TextMeshProUGUI timer = null;
    [SerializeField]
    GameObject backgroundDefault = null;
    [SerializeField]
    GameObject backgroundVIP = null;
    [SerializeField]
    Button btn = null;
    [SerializeField]
    BacteriaAvatarBackground backgroundBacteria = null;

    [HideInInspector] public HospitalCharacterInfo info;
    int status;
    int timerSeconds;
    IEnumerator<float> timerCoroutine;

    public bool isSet = false;

    private BalanceableInt ExpForTreatmentRoom;
    private BalanceableInt GoldForTreatmentRoom;

    public void SetInfo(HospitalCharacterInfo info, int status, int timerSeconds = 0)
    {
        this.info = info;
        this.status = status;
        this.timerSeconds = timerSeconds;

        UpdateAllInfo();
        isSet = true;
    }

    public void UpdateAllInfo()
    {
        SetStatus();
        SetBackground();
        SetBacteriaBackground();
        SetAvatar();
        SetBottomInfo();
    }

    void SetStatus()
    {
        if (info!=null)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured)
                status = 3;
            else if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
                status = 0;
        }

        if (status == 0) {
            clockBadge.SetActive(true);
            footBadge.SetActive(false);
        } else if (status == 3) {
            footBadge.SetActive(true);
            clockBadge.SetActive(false);
        } else {
            clockBadge.SetActive(false);
            footBadge.SetActive(false);
        }
    }

    void SetBackground()
    {
        backgroundDefault.SetActive(true);
        backgroundVIP.SetActive(false);

        if (info && info.IsVIP)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured || info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
                return;

            backgroundDefault.SetActive(false);
            backgroundVIP.SetActive(true);
            transform.SetSiblingIndex(0);
        }
    }

    void SetAvatar()
    {
        avatar.SetActive(false);
        if (info != null)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured || info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
                return;

            avatar.SetActive(true);
            avatarBody.sprite = info.AvatarBody;
            avatarHead.sprite = info.AvatarHead;
        }
    }

    void SetBottomInfo()
    {
        rewardArea.SetActive(false);
        timer.gameObject.SetActive(false);
        Timing.KillCoroutine(timerCoroutine);

        if (info != null)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured || info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
                return;

            SetReward();
        }
        else if (status == 0)
            SetTimer();
    }

    void SetReward()
    {
        rewardArea.SetActive(true);

        ExpForTreatmentRoom = BalanceableFactory.CreateXPForTreatmentRoomsBalanceable(info.EXPForCure);
        GoldForTreatmentRoom = BalanceableFactory.CreateGoldForTreatmentRoomsBalanceable(info.CoinsForCure);

        expRewardAmount.text = ExpForTreatmentRoom.GetBalancedValue().ToString();
        coinRewardAmount.text = GoldForTreatmentRoom.GetBalancedValue().ToString();
    }

    void SetTimer()
    {
        timer.gameObject.SetActive(true);
        if (timerSeconds > 0)
            timerCoroutine = Timing.RunCoroutine(UpdateTimer(timerSeconds));
        else
            timer.text = "-";
    }

    public void SetButton(UnityEngine.Events.UnityAction call)
    {
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(call);
    }

    public void SetIndicator()
    {
        if (info != null)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured || info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
            {
                newIndicator.SetActive(false);
                diagnosisIndicator.SetActive(false);
                cureIndicator.SetActive(false);
                cureHelpIndicator.SetActive(false);
                helpIndicator.SetActive(false);
                UpdateAllInfo();
                return;
            }

            bool cureWithHelp;

            if (info.CheckCurePosible(out cureWithHelp))
            {
                newIndicator.SetActive(false);
                diagnosisIndicator.SetActive(false);

                if (cureWithHelp)
                {
                    cureIndicator.SetActive(false);
                    cureHelpIndicator.SetActive(true);
                }
                else
                {
                    cureIndicator.SetActive(true);
                    cureHelpIndicator.SetActive(false);
                }

                helpIndicator.SetActive(false);
            } else if (!info.WasPatientCardSeen)
            {
                cureIndicator.SetActive(false);
                cureHelpIndicator.SetActive(false);
                diagnosisIndicator.SetActive(false);
                newIndicator.SetActive(true);
                helpIndicator.SetActive(false);
            }
            else if (info.RequiresDiagnosis)
            {
                newIndicator.SetActive(false);
                cureIndicator.SetActive(false);
                cureHelpIndicator.SetActive(false);
                diagnosisIndicator.GetComponent<Image>().sprite = ResourcesHolder.GetHospital().diagnosisBadgeGfx.GetDiagnosisBadge(HospitalDataHolder.Instance.ReturnDiseaseTypeRoomType((int)(info.DisaseDiagnoseType)));
                diagnosisIndicator.SetActive(true);
                helpIndicator.SetActive(false);
            } else if (info.HelpRequested) {
                newIndicator.SetActive(false);
                diagnosisIndicator.SetActive(false);
                cureIndicator.SetActive(false);
                cureHelpIndicator.SetActive(false);
                helpIndicator.SetActive(true);
            }
            else
            {
                cureIndicator.SetActive(false);
                cureHelpIndicator.SetActive(false);
                newIndicator.SetActive(false);
                diagnosisIndicator.SetActive(false);
                helpIndicator.SetActive(false);
            }
        }
        else {
            newIndicator.SetActive(false);
            diagnosisIndicator.SetActive(false);
            cureIndicator.SetActive(false);
            cureHelpIndicator.SetActive(false);
            helpIndicator.SetActive(false);
        }
    }

    public void SetBadge(int mode = 0)
    {
        if (mode == 0)
        {
            clockBadge.SetActive(true);
            footBadge.SetActive(false);
            avatar.SetActive(false);
        }
        else if (mode == 3)
        {
            clockBadge.SetActive(false);
            footBadge.SetActive(true);
            avatar.SetActive(false);
        }
        else
        {
            clockBadge.SetActive(false);
            footBadge.SetActive(false);
            avatar.SetActive(true);
        }

    }

    public void SetSelected(bool isSelected)
    {
        if (isSelected)
        {
            selectedIndicator.SetActive(true);
            transform.localScale = Vector3.one * 1.1f;
        }
        else
        {
            selectedIndicator.SetActive(false);
            transform.localScale = Vector3.one;
        }
    }
    //
    void SetBacteriaBackground()
    {
        if (info != null)
        {
            if (info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Cured || info.patientCardStatus == HospitalCharacterInfo.PatientCardInfoStatus.Discharged)
            {
                backgroundBacteria.SetBlinking(false, 0, info.IsVIP);
                return;
            }

            if (info.HasBacteria)
            {
                backgroundBacteria.SetBlinking(true, 0, info.IsVIP);
            }
            else
            {
                HospitalCharacterInfo infectedBy = null;
                int timeToInfection = info.GetTimeTillInfection(out infectedBy);

                if (timeToInfection > 0)
                    backgroundBacteria.SetBlinking(true, 1, info.IsVIP);
                else
                    backgroundBacteria.SetBlinking(false, 0, info.IsVIP);
            }
        }
        else
        {
            backgroundBacteria.SetBlinking(false, 0, false);
        }
    }

    IEnumerator<float> UpdateTimer(int timeRemaining)
    {
        while (true && timeRemaining >= 0)
        {
            timer.text = UIController.GetFormattedShortTime(timeRemaining);
            timeRemaining--;
            yield return Timing.WaitForSeconds(1f);
        }
    }
}
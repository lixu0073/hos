using UnityEngine;
using System.Collections.Generic;
using MovementEffects;
using System.Text;
using System;
using TMPro;

namespace Hospital
{
    /// <summary>
    /// 增益管理器，负责管理游戏中各种增益道具的存储、激活、UI显示和效果计算。
    /// 提供增益道具的基础管理功能，包括图标显示、数量统计和状态管理。
    /// </summary>
    public class BoosterManager : MonoBehaviour
    {
        //[SerializeField] private Sprite noBoosterBackground = null;
        //[SerializeField] private Image boosterIcon = null;
        //[SerializeField] private GameObject boosterButton = null;

        public VertexGradient boosterGradient = new VertexGradient();
        public Color boosterStrokeColor;

        [HideInInspector] public bool boosterActive = false;
        [HideInInspector] public int currentBoosterID = -1;
        [HideInInspector] public int[] boosterStorage;

        public Sprite[] boosterIcons = new Sprite[4];
        public Sprite[] boosterBadges = new Sprite[2];

        protected int newBoostersCount = 0;
        protected virtual int NewBoostersCount
        {            
            get { return newBoostersCount; }
            set
            {
                if (UIController.getHospital != null)
                {
                    if (value <= 0)
                    {
                        BoosterButtonController boosterButtonController = UIController.getHospital.BoosterButton.GetComponent<BoosterButtonController>();

                        boosterButtonController.Badge.SetActive(false);
                        boosterButtonController.SetPulse(false);
                    }
                    else if (!boosterActive)
                    {
                        BoosterButtonController boosterButtonController = UIController.getHospital.BoosterButton.GetComponent<BoosterButtonController>();

                        boosterButtonController.Badge.SetActive(true);
                        boosterButtonController.BadgeText.SetText(value.ToString());
                        boosterButtonController.SetPulse(true);
                    }
                }
                newBoostersCount = value;
            }
        }

        [HideInInspector] public bool[] newBoosters;

        protected int boosterEndTime = 0;
        protected int boosterTimeLeft = 0;

        public int BoosterTimeLeft
        {
            get { return boosterTimeLeft; }
            private set { }
        }

        public int BoosterEndTime
        {
            get { return boosterEndTime; }
            private set { }
        }

        void Start()
        {
            boosterStorage = new int[ResourcesHolder.Get().boosterDatabase.boosters.Length];
            newBoosters = new bool[ResourcesHolder.Get().boosterDatabase.boosters.Length];
            for (int i = 0; i < boosterStorage.Length; i++)
            {
                boosterStorage[i] = 0;
                newBoosters[i] = false;
            }
        }

        public bool BoosterActive(int boosterID)
        {
            return boosterActive && currentBoosterID == boosterID;
        }

        public virtual void SetBooster(int boosterID, int duration, bool onLoad = false)
        {
            ClearBooster();
            if (duration <= 0)
            {
                //	UIController.get.BoosterStartsPopup.Open (boosterID, false, onLoad ? 2 : 0 + 0.5f);
                return;
            }

            if (!onLoad)
            {
                UIController.getHospital.BoosterStartsPopup.Open(boosterID, true, /*onLoad ? 2 : 0 +*/ 0.5f);
                AnalyticsController.instance.ReportInGameItem(EconomyAction.Spend, ResourceType.Booster, EconomySource.UseBooster, 1, 0, -1, boosterID);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UseABooster));
            }            

            boosterEndTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) + duration;

            currentBoosterID = boosterID;

            //boosterIcon.gameObject.SetActive (false);
            //boosterButton.GetComponent<Image> ().sprite = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon;

            boosterActive = true;
            UIController.getHospital.BoosterButton.GetComponent<BoosterButtonController>().TurnOnCounting(boosterID);
            Timing.RunCoroutine(turnOffBoosterDelay(duration));

            switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType)
            {
                case BoosterType.Coin:
                    switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                    {
                        case BoosterTarget.PatientCard:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.Coin, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.DoctorPatient:
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.Coin, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.AllPatients:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.Coin, true);
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.Coin, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case BoosterType.Exp:
                    switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                    {
                        case BoosterTarget.PatientCard:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.Exp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.DoctorPatient:
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.Exp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.AllPatients:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.Exp, true);
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.Exp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        default:
                            break;
                    }
                    break;
                case BoosterType.CoinAndExp:
                    switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                    {
                        case BoosterTarget.PatientCard:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.CoinAndExp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.DoctorPatient:
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.CoinAndExp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        case BoosterTarget.AllPatients:
                            BoosterEffectManager.Instance.ActivateHospitalRoomBoosters(BoosterType.CoinAndExp, true);
                            BoosterEffectManager.Instance.ActivateDoctorBoosters(BoosterType.CoinAndExp, true);
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().coinsDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpPatientCard = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            Game.Instance.gameState().GetHospitalBoosterSystem().xpDoctorPatient = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].modifier;
                            break;
                        default:
                            break;
                    }
                    break;                    
                case BoosterType.Action:
                    switch (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget)
                    {
                        case BoosterTarget.Lab:
                            BoosterEffectManager.Instance.ActivateHappyHourBoosters(true);
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }

        public void AddBooster(int boosterID, EconomySource source, bool updateIndicators = true, int amount = 1)
        {
            if (boosterID >= boosterStorage.Length)
                return;

            boosterStorage[boosterID] += amount;
            if (updateIndicators)
            {
                NewBoostersCount += amount;
                newBoosters[boosterID] = true;
            }
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.BoosterAdded);
            //AnalyticsController.instance.ReportInGameItem(EconomyAction.Earn, ResourceType.Booster, source, amount, 0, -1, boosterID);
        }

        public void ClearIndicators()
        {
            for (int i = 0; i < newBoosters.Length; ++i)
                newBoosters[i] = false;

            NewBoostersCount = 0;
        }

        public int GetNewBoosterIndex()
        {
            for (int i = 0; i < newBoosters.Length; ++i)
            {
                if (newBoosters[i] == true)
                    return i;
            }

            return -2;
        }

        public virtual void ClearBooster()
        {
            //boosterIcon.gameObject.SetActive (true);
            //boosterButton.GetComponent<Image> ().sprite = noBoosterBackground;

            Timing.KillCoroutine(turnOffBoosterDelay().GetType());

            currentBoosterID = -1;
            boosterActive = false;
            NewBoostersCount = newBoostersCount; // show badge after booster ends
            UIController.getHospital.BoosterButton.GetComponent<BoosterButtonController>().TurnOffCounting();
            Game.Instance.gameState().GetHospitalBoosterSystem().coinsPatientCard = 1;
            Game.Instance.gameState().GetHospitalBoosterSystem().coinsDoctorPatient = 1;
            Game.Instance.gameState().GetHospitalBoosterSystem().xpPatientCard = 1;
            Game.Instance.gameState().GetHospitalBoosterSystem().xpDoctorPatient = 1;

            BoosterEffectManager.Instance.DeactivateAllBoosters();

            if (UIController.getHospital.PatientCard.gameObject.activeSelf)
            {
                if (UIController.getHospital.PatientCard.CurrentCharacter != null)
                    UIController.getHospital.PatientCard.RefreshView(UIController.getHospital.PatientCard.CurrentCharacter);

                UIController.getHospital.PatientCard.UpdateOtherPatients();
            }
            UIController.getHospital.MainPatientCardPopUpController.UpdatePatients();

            if (UIController.getHospital.HospitalInfoPopUp.CurrentInfo != null && UIController.getHospital.HospitalInfoPopUp.CurrentType == InfoType.Doctor)
                UIController.getHospital.HospitalInfoPopUp.SetDoctorInfo((DoctorRoomInfo)(UIController.getHospital.HospitalInfoPopUp.CurrentInfo));
        }

        public string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Checkers.CheckedAmount(currentBoosterID, -1, ResourcesHolder.Get().boosterDatabase.boosters.Length - 1, "currentBoosterID: ").ToString());
            builder.Append(";");
            builder.Append(Checkers.CheckedAmount(boosterEndTime, 0, int.MaxValue, "boosterEndtime: ").ToString());
            builder.Append(";");
            for (int i = 0; i < boosterStorage.Length; ++i)
            {
                builder.Append(Checkers.CheckedAmount(boosterStorage[i], 0, int.MaxValue, "Booster " + i + " amount: ").ToString());
                if (i < boosterStorage.Length - 1)
                    builder.Append("?");
            }

            return builder.ToString();
        }

        public virtual void LoadFromString(string saveString, bool visitingMode)
        {
            ClearIndicators();

            if (!visitingMode)
            {
                if (!string.IsNullOrEmpty(saveString))
                {
                    var save = saveString.Split(';');

                    currentBoosterID = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
                    boosterEndTime = int.Parse(save[1], System.Globalization.CultureInfo.InvariantCulture);
                    var savedStored = save[2].Split('?');

                    if (ResourcesHolder.Get().boosterDatabase.boosters.Length > savedStored.Length)
                    {
                        boosterStorage = new int[ResourcesHolder.Get().boosterDatabase.boosters.Length];
                        Debug.LogError("Booster storage should be biger than in save so i created new. Convert old save to new!");
                    }
                    else boosterStorage = new int[savedStored.Length];

                    for (int i = 0; i < boosterStorage.Length; ++i)
                    {
                        if (i < savedStored.Length)
                            boosterStorage[i] = int.Parse(savedStored[i], System.Globalization.CultureInfo.InvariantCulture);
                        else 
                            boosterStorage[i] = 0;
                    }
                    if (currentBoosterID > -1)
                    {
                        int duration = boosterEndTime - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                        SetBooster(currentBoosterID, duration, true);
                    }
                    else
                        UIController.getHospital.BoosterButton.GetComponent<BoosterButtonController>().TurnOffCounting();
                }
                else
                {
                    ClearBooster();
                    for (int i = 0; i < boosterStorage.Length; ++i)
                    {
                        boosterStorage[i] = 0;
                    }
                }
            }
            else
            {
                ClearBooster();
                for (int i = 0; i < boosterStorage.Length; ++i)
                {
                    boosterStorage[i] = 0;
                }
            }
        }

        public Sprite GetBoosterIcon(int boosterID)
        {
            Sprite boosterIcon = null;
            BoosterType boosterType;
            BoosterTarget boosterTarget;
            boosterType = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType;
            boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget;

            if (boosterTarget == BoosterTarget.DoctorPatient)
                boosterIcon = HospitalAreasMapController.HospitalMap.boosterManager.boosterIcons[0];

            if (boosterTarget == BoosterTarget.AllPatients)
                boosterIcon = HospitalAreasMapController.HospitalMap.boosterManager.boosterIcons[1];

            if (boosterTarget == BoosterTarget.PatientCard)
                boosterIcon = HospitalAreasMapController.HospitalMap.boosterManager.boosterIcons[2];

            if (boosterTarget == BoosterTarget.Lab)
                boosterIcon = HospitalAreasMapController.HospitalMap.boosterManager.boosterIcons[3];

            return boosterIcon;
        }

        public Sprite GetBoosterBadge(int boosterID)
        {
            Sprite boosterBadge = null;
            BoosterType boosterType = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType;
            BoosterTarget boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget;

            if (boosterType == BoosterType.Coin)
                boosterBadge = HospitalAreasMapController.HospitalMap.boosterManager.boosterBadges[0];

            if (boosterType == BoosterType.Exp)
                boosterBadge = HospitalAreasMapController.HospitalMap.boosterManager.boosterBadges[1];

            if (boosterType == BoosterType.Action)
                boosterBadge = null;

            if (boosterType == BoosterType.CoinAndExp)
            {
                Debug.LogError("Booster CoinAndEXP badge");
                boosterBadge = null;
            }
            return boosterBadge;
        }

        public String GetBoosterTimeLeftString()
        {
            return UIController.GetFormattedTime(boosterTimeLeft);
        }

        public String GetBoosterTimeLeftShortString()
        {
            return UIController.GetFormattedShortTime(boosterTimeLeft);
        }

        public static void TintWithBoosterGradient(TextMeshProUGUI textLabel)
        {
            textLabel.enableVertexGradient = true;
            textLabel.colorGradient = HospitalAreasMapController.HospitalMap.boosterManager.boosterGradient;
            textLabel.outlineColor = HospitalAreasMapController.HospitalMap.boosterManager.boosterStrokeColor;
        }

        protected IEnumerator<float> turnOffBoosterDelay(int turnOffTime = 1)
        {
            boosterTimeLeft = turnOffTime;
            while (boosterTimeLeft > 0)
            {
                yield return Timing.WaitForSeconds(1);
                --boosterTimeLeft;
            }

            ClearBooster();
        }

        #region editorMethods
        public void TestTurnOffBooster()
        {
            boosterTimeLeft = 0;
        }
        #endregion
    }
}
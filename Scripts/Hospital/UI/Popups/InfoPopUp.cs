using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using System.Linq;
using IsoEngine;
using I2.Loc;
using System.Collections;
using System;

namespace Hospital
{
    public enum InfoType
    {
        Doctor,
        Machine,
        Diagnose,
        VitaminMaker,
    }

    public class InfoPopUp : UIElement
    {
        public TextMeshProUGUI titleText;

        [Header("Doctor Info References")]
        public GameObject doctorInfo;
        public TextMeshProUGUI doctorInfoLeftText;
        public TextMeshProUGUI doctorInfoRightText;
        public Image doctorInfoImage;
        public TextMeshProUGUI medicineAmountText;
        public Image medicineImage;
        public TextMeshProUGUI cureTimeText;
        public TextMeshProUGUI expRewardText;
        public TextMeshProUGUI coinRewardText;
        public TextMeshProUGUI positiveEnergyRewardText;
        public TextMeshProUGUI positiveEnergyRewardTextCloseBracket;
        public TextMeshProUGUI curedPatientsText;
        public PointerDownListener positiveEnergyPointDownListener;
        public RectTransform rewardInfoRect;
        public GameObject positiveEnergyReward;

        [TermsPopup]
        [SerializeField] private string positiveEnergyTerm = "-";
        [TermsPopup]
        [SerializeField] private string positiveEnergyDescriptionTerm = "-";

        [Header("Machine Info References")]
        public GameObject machineInfo;
        public TextMeshProUGUI machineInfoLeftText;
        public TextMeshProUGUI machineInfoRightText;
        public TextMeshProUGUI producedMedicinesText;
        public Image machineInfoImage;
        public List<InfoMedicine> infoMedicines;
        public Material overlayMaterial;

        [Header("Diagnose Info References")]
        public GameObject diagnosisInfo;
        public TextMeshProUGUI diagnoseInfoLeftText;
        public TextMeshProUGUI diagnoseInfoRightText;
        public Image diagnoseInfoImage;
        public Image diagnosedOrganImage;
        public TextMeshProUGUI positiveEnergyText;
        public TextMeshProUGUI diagnoseTimeText;
        public TextMeshProUGUI diagnosedPatientsText;
        HospitalDataHolder.DiagRoomType diagnoseRoomType;

        List<DoctorRoom> builtDoctorRooms = null;
        List<DiagnosticRoom> builtDiagnosticRooms = null;
        List<MedicineProductionMachine> builtMachines = null;
        VitaminMaker vitaminMaker;

        InfoType currentType;
        public InfoType CurrentType
        {
            get { return currentType; }
        }

        ShopRoomInfo currentInfo;
        public ShopRoomInfo CurrentInfo
        {
            get { return currentInfo; }
        }

        int currentIndex = -1;

        [Header("Device specific layout refs")]
        public Transform backAnchor;
        public Transform nextAnchor;


        void Awake()
        {
            SetDeviceLayout();
        }

        void SetDeviceLayout()
        {
            if (ExtendedCanvasScaler.isPhone())
            {
                backAnchor.localScale = new Vector3(1.3f, 1.3f, 1f);
                nextAnchor.localScale = new Vector3(1.3f, 1.3f, 1f);
            }
            else
            {
                backAnchor.localScale = Vector3.one;
                nextAnchor.localScale = Vector3.one;
            }
        }

        public void OpenVitaminsInfo(ShopRoomInfo info)
        {
            gameObject.SetActive(true);
            StartCoroutine(Open(info, InfoType.VitaminMaker, 2, () =>
            {
                ShowVitaminsBookmark(true);
            }));
        }

        public IEnumerator Open(ShopRoomInfo info, InfoType type, int tabID = 0, Action whenDone = null)
        {
            currentType = type;
            currentInfo = info;
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, true, () =>
            {
                SetContent();
                SelectTab(tabID);
                SetBookmarks();
                ShowVitaminsBookmark(false);

                //for (int i = 0; i < tabs.Length; ++i)
                //{
                //    tabs[i].PopupOpen();
                //}

                whenDone?.Invoke();
            }));
            yield return null;
        }

        private void ShowVitaminsBookmark(bool show)
        {
            bookmarks[2].gameObject.SetActive(show);
            ToggleSwapArrows(!show);
            MoveOtherTabs(show);
        }

        private void ToggleSwapArrows(bool setActive)
        {
            backAnchor.gameObject.SetActive(setActive);
            nextAnchor.gameObject.SetActive(setActive);
        }

        private void MoveOtherTabs(bool vitaminIncluded)
        {
            if (vitaminIncluded)
            {
                bookmarksRectt[0].anchorMin = new Vector2(-0.1275f, 0.58f);
                bookmarksRectt[0].anchorMax = new Vector2(0.02f, 0.785f);

                bookmarksRectt[1].anchorMin = new Vector2(-0.1275f, 0.3968571f);
                bookmarksRectt[1].anchorMax = new Vector2(0.02f, 0.599f);
            }
            else
            {
                bookmarksRectt[0].anchorMin = new Vector2(-0.1275f, 0.755f);
                bookmarksRectt[0].anchorMax = new Vector2(0.02f, 0.96f);

                bookmarksRectt[1].anchorMin = new Vector2(-0.1275f, 0.58f);
                bookmarksRectt[1].anchorMax = new Vector2(0.02f, 0.785f);
            }
        }

        private void SetContent()
        {
            ClearCurrentInfo();
            switch (currentType)
            {
                case InfoType.Doctor:
                    FillDoctorRoomList();
                    SetDoctorInfo((DoctorRoomInfo)currentInfo);
                    if (!builtDoctorRooms[currentIndex].infoShowed)
                    {
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                        builtDoctorRooms[currentIndex].infoShowed = true;
                    }
                    break;

                case InfoType.Machine:
                    FillMachinesList();
                    SetMachineInfo((MedicineProductionMachineInfo)currentInfo, builtMachines[currentIndex].GetProducedMedicineAmount());
                    break;

                case InfoType.Diagnose:
                    FillDiagnosticRoomList();
                    SetDiagnosisInfo((DiagnosticRoomInfo)currentInfo);
                    if (!builtDiagnosticRooms[currentIndex].infoShowed)
                    {
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                        builtDiagnosticRooms[currentIndex].infoShowed = true;
                    }
                    break;

                case InfoType.VitaminMaker:
                    currentType = InfoType.VitaminMaker;
                    SetMachineInfo((MedicineProductionMachineInfo)currentInfo, vitaminMaker.GetProducedMedicineAmount());
                    break;

                default:
                    Debug.LogError("This should never fire.");
                    break;
            }
        }

        public void SetupVitaminView(List<VitaminCollectorModel> vitaminModels, VitaminMaker vitaminMaker)
        {
            this.vitaminMaker = vitaminMaker;
            for (int i = 0; i < tabs.Length; i++)
            {
                if (tabs[i] is VitaminsPopupTab)
                {
                    VitaminsPopupTab tabAsVitamin = tabs[i] as VitaminsPopupTab;
                    tabAsVitamin.SetupVitaminViews(vitaminModels);
                }
            }
        }

        public void RefreshPopup()
        {
            SetContent();
            if (currentTabID == 1)
                SelectTab(1);
        }

        public void ResetInfoShowed()
        {
            if (builtDoctorRooms != null && builtDoctorRooms.Count > 0)
            {
                for (int i = 0; i < builtDoctorRooms.Count; i++)
                {
                    builtDoctorRooms[i].infoShowed = false;
                }
            }

            if (builtDiagnosticRooms != null && builtDiagnosticRooms.Count > 0)
            {
                for (int i = 0; i < builtDiagnosticRooms.Count; i++)
                {
                    builtDiagnosticRooms[i].infoShowed = false;
                }
            }

            if (builtMachines != null && builtMachines.Count > 0)
            {
                for (int i = 0; i < builtMachines.Count; i++)
                {
                    builtMachines[i].SetInfoShowed(false);
                }
            }
        }

        void ClearCurrentInfo()
        {
            doctorInfo.SetActive(false);
            machineInfo.SetActive(false);
            diagnosisInfo.SetActive(false);
        }

        void SetTitle(ShopRoomInfo info)
        {
            titleText.text = I2.Loc.ScriptLocalization.Get(info.ShopTitle).ToUpper();
        }

        void TintWithStandardColors(TextMeshProUGUI text)
        {
            text.enableVertexGradient = false;
            text.color = Color.white;
            text.outlineColor = Color.black;
        }

        public void SetDoctorInfo(DoctorRoomInfo info)
        {
            currentInfo = info;

            if (builtDoctorRooms[currentIndex] == null)
                return;
            
            int curedPatients = builtDoctorRooms[currentIndex].CuredPatients;
            //Debug.LogError("SetDoctorInfo curedPatients: " + curedPatients);
            //Debug.LogError("SetDoctorInfo currentIndex: " + currentIndex);
            SetTitle(info);
            doctorInfoLeftText.text = I2.Loc.ScriptLocalization.Get(info.DoctorDescription);
            doctorInfoRightText.text = I2.Loc.ScriptLocalization.Get(info.InfoDescription);
            doctorInfoImage.sprite = info.ShopImage;
            if (curedPatients >= 0)
                curedPatientsText.text = curedPatients.ToString();

            medicineImage.sprite = ResourcesHolder.Get().GetSpriteForCure(info.cure);
            medicineAmountText.text = "1";

            TintWithStandardColors(coinRewardText);
            TintWithStandardColors(expRewardText);
            TintWithStandardColors(cureTimeText);
            TintWithStandardColors(positiveEnergyRewardText);
            TintWithStandardColors(positiveEnergyRewardTextCloseBracket);

            cureTimeText.text = UIController.GetFormattedShortTime(builtDoctorRooms[currentIndex].CureTimeMastered + 1);
            if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.doctorsWorkingTime_FACTOR))
                BoosterManager.TintWithBoosterGradient(cureTimeText);

            coinRewardText.text = builtDoctorRooms[currentIndex].CoinRewardMastered.ToString();

            if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.coinsForDoctorRooms_FACTOR))
                BoosterManager.TintWithBoosterGradient(coinRewardText);

            expRewardText.text = builtDoctorRooms[currentIndex].ExpRewardMastered.ToString();
            if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.expForDoctors_FACTOR))
                BoosterManager.TintWithBoosterGradient(expRewardText);

            //zmiana kolorów - Colour change
            bool boosterActive = HospitalAreasMapController.HospitalMap.boosterManager.boosterActive;
            BoosterType boosterType;
            BoosterTarget boosterTarget;
            if (boosterActive)
            {
                boosterType = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.Map.boosterManager.currentBoosterID].boosterType;
                boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.Map.boosterManager.currentBoosterID].boosterTarget;

                if (boosterTarget == BoosterTarget.DoctorPatient || boosterTarget == BoosterTarget.AllPatients)
                {
                    if (boosterType == BoosterType.Coin || boosterType == BoosterType.CoinAndExp)
                    {
                        BoosterManager.TintWithBoosterGradient(coinRewardText);
                        coinRewardText.text = builtDoctorRooms[currentIndex].CoinRewardMastered.ToString();
                    }
                    else
                    {
                        //CoinsForCure.text = info.CoinsForCure.ToString ();
                    }
                    if (boosterType == BoosterType.Exp || boosterType == BoosterType.CoinAndExp)
                    {
                        BoosterManager.TintWithBoosterGradient(expRewardText);
                        expRewardText.text = builtDoctorRooms[currentIndex].ExpRewardMastered.ToString();
                    }
                    else
                    {
                        //	EXPForCure.text = info.EXPForCure.ToString ();
                    }
                }
                else
                {
                    /*	CoinsForCure.text = info.CoinsForCure.ToString();
					EXPForCure.text = info.EXPForCure.ToString();

					CoinsForCure.color = defaultCoinPrizeColor;
					EXPForCure.color = defaultXPPrizeColor;*/
                }
            }
            else
            {
                /*	CoinsForCure.text = info.CoinsForCure.ToString();
				EXPForCure.text = info.EXPForCure.ToString();

				CoinsForCure.color = defaultCoinPrizeColor;
				EXPForCure.color = defaultXPPrizeColor;*/
            }

            if (Game.Instance.gameState().GetHospitalLevel() >= 12)
            {
                positiveEnergyReward.SetActive(true);
                positiveEnergyRewardText.text = "( " + builtDoctorRooms[currentIndex].PositiveEnergyRewardMastered;
                if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.positiveEnergyFromKids_FACTOR))
                {
                    BoosterManager.TintWithBoosterGradient(positiveEnergyRewardText);
                    BoosterManager.TintWithBoosterGradient(positiveEnergyRewardTextCloseBracket);
                }

                positiveEnergyPointDownListener.SetDelegate(() =>
                {
                    TextTooltip.Open(ScriptLocalization.Get(positiveEnergyTerm), ScriptLocalization.Get(positiveEnergyDescriptionTerm));
                });
                rewardInfoRect.anchorMax = new Vector2(0.95f, 0.4f);
                rewardInfoRect.anchorMin = new Vector2(0.05f, 0.185f);
            }
            else
            {
                positiveEnergyReward.SetActive(false);

                rewardInfoRect.anchorMax = new Vector2(0.95f, 0.4f);
                rewardInfoRect.anchorMin = new Vector2(0.05f, 0.275f);
            }

            doctorInfo.SetActive(true);

            ChoseStarsContent(builtDoctorRooms[currentIndex].masterableProperties.MasterableConfigData.MasteryGoals.Length < 4);
            SetStars(builtDoctorRooms[currentIndex].masterableProperties.MasteryLevel);

            mastershipTab.showScrollEffect = true;
            mastershipTab.PrepareMastershipContent(builtDoctorRooms[currentIndex].masterableProperties);
        }

        void SetMachineInfo(MedicineProductionMachineInfo info, int producedMedicines)
        {
            currentInfo = info;

            SetTitle(info);
            machineInfoLeftText.text = I2.Loc.ScriptLocalization.Get(info.MachineDescription);
            machineInfoRightText.text = I2.Loc.ScriptLocalization.Get(info.InfoDescription);
            machineInfoImage.sprite = info.ShopImage;

            DisableInfoMedicines();
            if (string.Compare(info.Tag, "ElixirLab") == 0)
            {
                int sum = 0;
                List<MedicineProductionMachine> elixirLabs = builtMachines.FindAll(x => string.Compare(((MedicineProductionMachineInfo)x.GetShopRoomInfo()).Tag, "ElixirLab") == 0);
                for (int i = 0; i < elixirLabs.Count; ++i)
                {
                    sum += elixirLabs[i].GetProducedMedicineAmount();
                }
                producedMedicinesText.text = sum.ToString();
            }
            else
            {
                producedMedicinesText.text = producedMedicines.ToString();
            }

            SetupInfoMedicines(info.productedMedicine);

            machineInfo.SetActive(true);

            if (currentType == InfoType.VitaminMaker)
            {
                ChoseStarsContent(vitaminMaker.GetMasterableConfigData().MasteryGoals.Length < 4);
                SetStars(vitaminMaker.GetMasteryLevel());
                mastershipTab.PrepareMastershipContent(vitaminMaker.masterableProperties);
            }
            else
            {
                ChoseStarsContent(builtMachines[currentIndex].GetMasterableConfigData().MasteryGoals.Length < 4);
                SetStars(builtMachines[currentIndex].GetMasteryLevel());
                mastershipTab.PrepareMastershipContent(builtMachines[currentIndex].masterableProperties);
            }
            mastershipTab.showScrollEffect = true;
            ShowVitaminsBookmark(info.Tag == "VitaminMaker");
        }

        void DisableInfoMedicines()
        {
            for (int i = 0; i < infoMedicines.Count; i++)
            {
                infoMedicines[i].gameObject.SetActive(false);
            }
        }

        void SetupInfoMedicines(MedicineType type)
        {
            List<MedicineDatabaseEntry> meds;
            meds = ResourcesHolder.Get().GetAllMedicinesOfType(type);

            int playerLevel = Game.Instance.gameState().GetHospitalLevel();
            int lockedMeds = 0;
            for (int i = 0; i < meds.Count; i++)
            {
                infoMedicines[i].SetSprite(meds[i].image);
                if (playerLevel >= meds[i].minimumLevel)
                {
                    infoMedicines[i].SetColor(new Color(1, 1, 1, 1f));
                    infoMedicines[i].SetMaterial(null);
                    infoMedicines[i].SetUpListener(meds[i], true);
                }
                else
                {
                    if (lockedMeds == 0)
                    {
                        infoMedicines[i].SetUpListener(meds[i], false);
                        infoMedicines[i].SetColor(new Color(1, 1, 1, .5f));
                    }
                    else
                    {
                        infoMedicines[i].ClearListener();
                        infoMedicines[i].SetMaterial(overlayMaterial);
                    }
                    lockedMeds++;
                }
                infoMedicines[i].gameObject.SetActive(true);
            }
            //Debug.LogError("meds.Count = " + meds.Count);
            //Debug.LogError("lockedMeds = " + lockedMeds);
        }

        void SetDiagnosisInfo(DiagnosticRoomInfo info)
        {
            currentInfo = info;

            SetTitle(info);
            diagnoseInfoLeftText.text = I2.Loc.ScriptLocalization.Get(info.NurseDescription);
            diagnoseInfoRightText.text = I2.Loc.ScriptLocalization.Get(info.InfoDescription);
            diagnoseInfoImage.sprite = info.ShopImage;
            diagnosedOrganImage.sprite = UIController.getHospital.PatientCard.GetDiagnosisSprite(info.TypeOfDiagRoom);
            diagnosedPatientsText.text = builtDiagnosticRooms[currentIndex].DiagnosedPatients.ToString();
            diagnoseRoomType = info.TypeOfDiagRoom;

            int positiveEnergyCost = info.GetPositiveEnergyCost();

            positiveEnergyText.text = (positiveEnergyCost > 0) ? positiveEnergyCost.ToString() : I2.Loc.ScriptLocalization.Get("FREE");
            if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.costOfDiagnosis_FACTOR))
                BoosterManager.TintWithBoosterGradient(positiveEnergyText);

            diagnoseTimeText.text = UIController.GetFormattedShortTime(builtDiagnosticRooms[currentIndex].DiagnosisTimeMastered);
            if (StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.diagnosticStationsTime_FACTOR))
                BoosterManager.TintWithBoosterGradient(diagnoseTimeText);

            diagnosisInfo.SetActive(true);

            ChoseStarsContent(builtDiagnosticRooms[currentIndex].masterableProperties.MasterableConfigData.MasteryGoals.Length < 4);
            SetStars(builtDiagnosticRooms[currentIndex].masterableProperties.MasteryLevel);

            mastershipTab.showScrollEffect = true;
            mastershipTab.PrepareMastershipContent(builtDiagnosticRooms[currentIndex].masterableProperties);
        }

        void FillDoctorRoomList()
        {
            if (builtDoctorRooms == null)
                builtDoctorRooms = new List<DoctorRoom>();

            builtDoctorRooms = FindObjectsOfType<DoctorRoom>().ToList().Where((x) => x.state == RotatableObject.State.working).ToList();
            currentType = InfoType.Doctor;
            currentIndex = GetCurrentIndex();
        }

        void FillDiagnosticRoomList()
        {
            if (builtDiagnosticRooms == null)
                builtDiagnosticRooms = new List<DiagnosticRoom>();

            builtDiagnosticRooms = FindObjectsOfType<DiagnosticRoom>().ToList().Where((x) => x.state == RotatableObject.State.working).ToList();
            currentType = InfoType.Diagnose;
            currentIndex = GetCurrentIndex();
        }

        void FillMachinesList()
        {
            if (builtMachines == null)
                builtMachines = new List<MedicineProductionMachine>();

            builtMachines = FindObjectsOfType<MedicineProductionMachine>().ToList().Where((x) => x.state == RotatableObject.State.working).ToList();
            currentType = InfoType.Machine;
            currentIndex = GetCurrentIndex();
        }

        public void ButtonNext()
        {
            SoundsController.Instance.PlayPopUp();
            //Debug.Log("ButtonNext Clicked.");
            if (currentIndex == -1)
            {
                currentIndex = GetCurrentIndex();
            }
            currentIndex = GetNextIndex();

            switch (currentType)
            {
                case InfoType.Doctor:
                    if (builtDoctorRooms != null && builtDoctorRooms.Count > 0 && builtDoctorRooms[currentIndex] != null && builtDoctorRooms.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtDoctorRooms[currentIndex].position + new Vector2i(2, 2), 1f, false);
                        builtDoctorRooms[currentIndex].ShowDoctorHoover();
                        SetDoctorInfo((DoctorRoomInfo)(builtDoctorRooms[currentIndex].GetRoomInfo()));

                        if (!builtDoctorRooms[currentIndex].infoShowed)
                        {
                            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                            builtDoctorRooms[currentIndex].infoShowed = true;
                        }
                    }
                    break;

                case InfoType.Machine:
                    if (builtMachines != null && builtMachines.Count > 0 && builtMachines[currentIndex] != null && builtMachines.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtMachines[currentIndex].GetPosition() + new Vector2i(2, 2), 1f, false);
                        builtMachines[currentIndex].ShowMachineHoover();
                        SetMachineInfo((MedicineProductionMachineInfo)(builtMachines[currentIndex].GetShopRoomInfo()), builtMachines[currentIndex].GetProducedMedicineAmount());

                        //if (!builtMachines[currentIndex].infoShowed)
                        // {
                        //     DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourLabMakers));
                        //     builtMachines[currentIndex].infoShowed = true;
                        // }
                    }
                    break;

                case InfoType.Diagnose:
                    if (builtDiagnosticRooms != null && builtDiagnosticRooms.Count > 0 && builtDiagnosticRooms[currentIndex] != null && builtDiagnosticRooms.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtDiagnosticRooms[currentIndex].position + new Vector2i(2, 2), 1f, false);
                        builtDiagnosticRooms[currentIndex].ShowDiagnosisHoover();
                        SetDiagnosisInfo((DiagnosticRoomInfo)(builtDiagnosticRooms[currentIndex].GetRoomInfo()));

                        if (!builtDiagnosticRooms[currentIndex].infoShowed)
                        {
                            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                            builtDiagnosticRooms[currentIndex].infoShowed = true;
                        }
                    }
                    break;
            }

        }

        public void ButtonBack()
        {
            SoundsController.Instance.PlayPopUp();
            //Debug.Log("ButtonBack Clicked.");
            if (currentIndex == -1)
            {
                currentIndex = GetCurrentIndex();
            }
            currentIndex = GetPreviousIndex();

            switch (currentType)
            {
                case InfoType.Doctor:
                    if (builtDoctorRooms != null && builtDoctorRooms.Count > 0 && builtDoctorRooms[currentIndex] != null && builtDoctorRooms.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtDoctorRooms[currentIndex].position + new Vector2i(2, 2), 1f, false);
                        builtDoctorRooms[currentIndex].ShowDoctorHoover();
                        SetDoctorInfo((DoctorRoomInfo)(builtDoctorRooms[currentIndex].GetRoomInfo()));

                        if (!builtDoctorRooms[currentIndex].infoShowed)
                        {
                            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                            builtDoctorRooms[currentIndex].infoShowed = true;
                        }
                    }
                    break;

                case InfoType.Machine:
                    if (builtMachines != null && builtMachines.Count > 0 && builtMachines[currentIndex] != null && builtMachines.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtMachines[currentIndex].GetPosition() + new Vector2i(2, 2), 1f, false);
                        builtMachines[currentIndex].ShowMachineHoover();
                        SetMachineInfo((MedicineProductionMachineInfo)(builtMachines[currentIndex].GetShopRoomInfo()), builtMachines[currentIndex].GetProducedMedicineAmount());

                        //  if (!builtMachines[currentIndex].infoShowed)
                        //  {
                        //      DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourLabMakers));
                        //      builtMachines[currentIndex].infoShowed = true;
                        //  }
                    }
                    break;

                case InfoType.Diagnose:
                    if (builtDiagnosticRooms != null && builtDiagnosticRooms.Count > 0 && builtDiagnosticRooms[currentIndex] != null && builtDiagnosticRooms.Count > currentIndex)
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(builtDiagnosticRooms[currentIndex].position + new Vector2i(2, 2), 1f, false);
                        builtDiagnosticRooms[currentIndex].ShowDiagnosisHoover();
                        SetDiagnosisInfo((DiagnosticRoomInfo)(builtDiagnosticRooms[currentIndex].GetRoomInfo()));

                        if (!builtDiagnosticRooms[currentIndex].infoShowed)
                        {
                            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses));
                            builtDiagnosticRooms[currentIndex].infoShowed = true;
                        }
                    }
                    break;
            }
        }

        int GetCurrentIndex()
        {
            switch (currentType)
            {
                case InfoType.Doctor:
                    for (int i = 0; i < builtDoctorRooms.Count; i++)
                    {
                        if (currentInfo == builtDoctorRooms[i].GetRoomInfo())
                        {
                            currentIndex = i;
                            return i;
                        }
                    }
                    break;

                case InfoType.Machine:
                    for (int i = 0; i < builtMachines.Count; i++)
                    {
                        if (currentInfo == builtMachines[i].GetShopRoomInfo())
                        {
                            currentIndex = i;
                            return i;
                        }
                    }
                    break;

                case InfoType.Diagnose:
                    for (int i = 0; i < builtDiagnosticRooms.Count; i++)
                    {
                        if (currentInfo == builtDiagnosticRooms[i].GetRoomInfo())
                        {
                            currentIndex = i;
                            return i;
                        }
                    }
                    break;

                case InfoType.VitaminMaker:
                    return 0;
            }

            Debug.LogError("Current index not found");
            return 0;
        }

        int GetNextIndex()
        {
            switch (currentType)
            {
                case InfoType.Doctor:
                    if (currentIndex + 1 < builtDoctorRooms.Count)
                        return currentIndex + 1;
                    break;

                case InfoType.Machine:
                    if (currentIndex + 1 < builtMachines.Count)
                        return currentIndex + 1;
                    break;
                case InfoType.Diagnose:
                    if (currentIndex + 1 < builtDiagnosticRooms.Count)
                        return currentIndex + 1;
                    break;
            }

            return 0;
        }

        int GetPreviousIndex()
        {
            switch (currentType)
            {
                case InfoType.Doctor:
                    if (currentIndex - 1 >= 0)
                        return currentIndex - 1;
                    else
                        return builtDoctorRooms.Count - 1;
                case InfoType.Machine:
                    if (currentIndex - 1 >= 0)
                        return currentIndex - 1;
                    else
                        return builtMachines.Count - 1;
                case InfoType.Diagnose:
                    if (currentIndex - 1 >= 0)
                        return currentIndex - 1;
                    else
                        return builtDiagnosticRooms.Count - 1;
            }

            return 0;
        }

        #region Diagnose See Patient Cards
        List<HospitalCharacterInfo> patientsList;
        List<int> bedsList;
        bool patientFound = false;

        public void ButtonSeePatientCards()
        {
            Debug.Log("Info ButtonSeePatientCards");
            ShowDiagnosePatientCard(false, (DiagnosticRoomInfo)CurrentInfo);
            //return;
        }

        public void ShowDiagnosePatientCard(bool fromHover, DiagnosticRoomInfo info)
        {
            diagnoseRoomType = info.TypeOfDiagRoom;

            GetAllBedPatients();

            int patientID = GetPatientID(!fromHover);
            if (!patientFound && fromHover)
            {
                string msg = I2.Loc.ScriptLocalization.Get("NO_DIAGNOSIS_PATIENTS");
                //Debug.LogError("msg1 = " + msg);
                msg = string.Format(msg, I2.Loc.ScriptLocalization.Get(info.ShopTitle)).ToUpper();
                //Debug.LogError("msg2 = " + msg);
                MessageController.instance.ShowMessage(msg);
            }
            else
            {
                if (patientsList.Count == 0)
                    UIController.getHospital.PatientCard.Open(null, 1); //1 because 0 is usually VIP
                else
                    UIController.getHospital.PatientCard.Open(patientsList[patientID], bedsList[patientID]);
                base.Exit(false);
            }
        }

        public void GetAllBedPatients()
        {
            PrepareLists();
            HospitalBedController bedController = HospitalAreasMapController.HospitalMap.hospitalBedController;
            for (int i = 0; i < bedController.Beds.Count; i++)
            {
                if (bedController.Beds[i].Patient != null &&
                    (bedController.Beds[i]._BedStatus == HospitalBedController.HospitalBed.BedStatus.OccupiedBed ||
                    bedController.Beds[i]._BedStatus == HospitalBedController.HospitalBed.BedStatus.WaitForDiagnose))
                {
                    patientsList.Add(bedController.Beds[i].Patient.GetHospitalCharacterInfo());
                    bedsList.Add(i);
                }
            }
            //Debug.LogError("Filled new patients list. Count = " + patientsList.Count);
        }

        public int GetPatientID(bool allowDiagnosed)
        {
            patientFound = false;
            //if there's patient with disease diagnosed in this room
            for (int i = 0; i < patientsList.Count; i++)
            {
                if (patientsList[i].RequiresDiagnosis && HospitalDataHolder.Instance.DiseaseMatchesRoom(patientsList[i].DisaseDiagnoseType, diagnoseRoomType))
                {
                    if (allowDiagnosed)
                    {
                        //return this patient regardles if he is before/in/after diagnosis
                        patientFound = true;
                        return i;
                    }
                    else if ((!patientsList[i].IsVIP && !HospitalDataHolder.Instance.QueueContainsPatient(patientsList[i].GetComponent<HospitalPatientAI>()))
                        || (patientsList[i].IsVIP && !HospitalDataHolder.Instance.QueueContainsPatient(patientsList[i].GetComponent<VIPPersonController>())))
                    {
                        //return patient only if he is in bed!
                        patientFound = true;
                        return i;
                    }
                }
            }

            patientFound = false;
            return 0;
        }

        public bool DiseaseMatchesRoom(DiseaseType disease)
        {
            if (disease == DiseaseType.Bone && diagnoseRoomType == HospitalDataHolder.DiagRoomType.XRay)
                return true;
            if (disease == DiseaseType.Brain && diagnoseRoomType == HospitalDataHolder.DiagRoomType.MRI)
                return true;
            if (disease == DiseaseType.Ear && diagnoseRoomType == HospitalDataHolder.DiagRoomType.UltraSound)
                return true;
            if (disease == DiseaseType.Lungs && diagnoseRoomType == HospitalDataHolder.DiagRoomType.LungTesting)
                return true;
            if (disease == DiseaseType.Kidneys && diagnoseRoomType == HospitalDataHolder.DiagRoomType.Laser)
                return true;

            return false;
        }

        void PrepareLists()
        {
            if (patientsList != null || bedsList != null)
            {
                patientsList.Clear();
                bedsList.Clear();
            }
            else
            {
                patientsList = new List<HospitalCharacterInfo>();
                bedsList = new List<int>();
            }
        }
        #endregion

        public void ButtonExit()
        {
            Exit(true);
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            //Debug.LogError("Info preservesHovers = " + preservesHovers);
            base.Exit(hidePopupWithShowMainUI);
            for (int i = 0; i < tabs.Length; ++i)
            {
                tabs[i].PopupClose();
            }
        }

        #region MastershipSystem
        [SerializeField] InfoPopupMastershipTab mastershipTab = null;

        [SerializeField] private PopupTab[] tabs = null;
        [SerializeField] private PopupBookmark[] bookmarks = null;
#pragma warning disable 0649
        [SerializeField] private RectTransform[] bookmarksRectt;
#pragma warning restore 0649
        [SerializeField] private StarsController stars3 = null;
        [SerializeField] private StarsController stars4 = null;

        [SerializeField] private int currentTabID = 0;
        [SerializeField] VerticalLayoutGroup NameLayoutGroup = null;

        StarsController activeStars = null;

        private void SetBookmarks()
        {
            bookmarks[0].SetBookmarkInteractable(true);
            bookmarks[1].SetBookmarkInteractable(Game.Instance.gameState().GetHospitalLevel() >= DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel);
        }

        public void BookmarkClicked(int tabID)
        {
            if (tabID == 0 || tabID == 2)
            {
                SelectTab(tabID);
                return;
            }
            if (Game.Instance.gameState().GetHospitalLevel() >= DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
                SelectTab(tabID);
            else
                MessageController.instance.ShowLockedMastershipMessage();
        }

        public void SelectTab(int tabID)
        {
            if (tabs == null || bookmarks == null)
                return;           
            if (tabID >= tabs.Length || tabID >= bookmarks.Length)
                return;

            tabs[currentTabID].CloseTab();
            currentTabID = tabID;
            SelectBookmark();
            ShowTab();
        }

        private void SelectBookmark()
        {
            if (bookmarks == null)
                return;

            for (int i = 0; i < bookmarks.Length; ++i)
            {
                bookmarks[i].SetBookmarkSelected(i == currentTabID);
            }
        }

        private void ShowTab()
        {
            if (tabs == null)
                return;

            for (int i = 0; i < tabs.Length; ++i)
            {
                tabs[i].SetTabSelected(i == currentTabID);
            }
        }

        private void ChoseStarsContent(bool is3)
        {
            if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                stars3.gameObject.SetActive(false);
                stars4.gameObject.SetActive(false);
                NameLayoutGroup.padding.top = 0;
                return;
            }
            else
            {
                NameLayoutGroup.padding.top = 40;
            }

            if (stars3 == null)
            {
                Debug.LogError("stars3 is null");
                return;
            }

            if (stars4 == null)
            {
                Debug.LogError("stars4 is null");
                return;
            }

            if (is3)
            {
                activeStars = stars3;
                stars3.gameObject.SetActive(true);
                stars4.gameObject.SetActive(false);
            }
            else
            {
                activeStars = stars4;
                stars3.gameObject.SetActive(false);
                stars4.gameObject.SetActive(true);
            }
        }

        private void SetStars(int level)
        {
            if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
                return;

            if (activeStars == null)
            {
                Debug.LogError("activeStars is null");
                return;
            }

            activeStars.SetStarsVisible(level);
        }

        public MasterableProperties GetCurrentMasterableObject()
        {
            switch (currentType)
            {
                case InfoType.Doctor:
                    return builtDoctorRooms[currentIndex].masterableProperties;
                case InfoType.Machine:
                    return builtMachines[currentIndex].masterableProperties;
                case InfoType.Diagnose:
                    return builtDiagnosticRooms[currentIndex].masterableProperties;
                case InfoType.VitaminMaker:
                    return vitaminMaker.masterableProperties;
                default:
                    Debug.LogError("currentType not implemented");
                    return null;
            }
        }
        #endregion
    }
}

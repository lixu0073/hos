using UnityEngine;
using System.Collections.Generic;
using System;
using System.Globalization;

namespace Hospital
{
    public class Epidemy : MonoBehaviour
    {
        public EpidemyObjectController EpidemyObject;
        public EpidemyPackageGenerator EpidemyPackageGenerator;

        public float EpidemyEndTime;

        public float TimeTillOutbreak;
        public float TimeTillEpidemyEnd;

        private BalanceableInt epidemyOutbreakTimeBalanceable;

        public float EpidemyOutbreakTime
        {
            get
            {
                if(epidemyOutbreakTimeBalanceable == null)        
                    epidemyOutbreakTimeBalanceable = BalanceableFactory.CreateEpidemyCooldownBalanceable();

                return epidemyOutbreakTimeBalanceable.GetBalancedValue();
            }
        }

        public float TimeTillNextHelpStatusRefresh;
        public float RefreshAwaitTime;

        public GameObject HelpMark;
#pragma warning disable 0649
        [SerializeField] private SpriteRenderer chestOpen;
        [SerializeField] private SpriteRenderer chestClosed;
        [SerializeField] private float alphaTransitionDuration;
        [SerializeField] private Animator signAnimator;
        [SerializeField] private Animator generatorAnim;
        [SerializeField] private Animator epidemyCharacter;
#pragma warning restore 0649

        private bool playAnimation = true;
        public static bool HasAnyHelpRequest = false;
        public EpidemyDataHolder saveState = new EpidemyDataHolder();

        private bool outbreak;
        public bool Outbreak { get { return outbreak; } }

        private bool isHelicopterInAction;
        public bool IsHelicopterInAction { get { return isHelicopterInAction; } set { isHelicopterInAction = value; } }

        private bool unwrapped;
        public bool Unwrapped { get { return unwrapped; } set { unwrapped = value; } }

        private const int numberOfDifferentMedicinesInEpidemy = 3;
        public int NumberOfDifferentMedicinesInEpidemy { get { return numberOfDifferentMedicinesInEpidemy; } }

        private List<int> selectedMedicinesIndexes = new List<int>();
        public List<int> SelectedMedicinesIndexes { get { return selectedMedicinesIndexes; } }

        private int levelWhileGeneratingMedicines;
        public int LevelWhileGeneratingMedicines { get { return levelWhileGeneratingMedicines; } }

        public int Level { get; set; }

        //private float alphaTransitionStartTime;
        private float transitionProgress;
        private bool wasGameLoaded;
        private bool hasEpidemyChanged;
        private bool shouldGenerateNewMedicines;
        private bool isNewEpidemy;

        private List<Sprite> comingMedicinesSprites = new List<Sprite>();
        private List<MedicineDatabaseEntry> availableMedicines = new List<MedicineDatabaseEntry>();

        private void StartEpidemy(float timeTillEpidemyEnd, bool useHeli)
        {
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                PackageHelpRequestManager.Instance.GetMyPackageRequests(DeletePackagesHelpRequests);

            if (UIController.getHospital.EpidemyOffPopUp.gameObject.activeSelf)
                UIController.getHospital.EpidemyOffPopUp.Exit();

            TimeTillEpidemyEnd = timeTillEpidemyEnd;
            TimeTillOutbreak = EpidemyOutbreakTime;
            UIController.getHospital.EpidemyOnPopUp.GetComponent<EpidemyOnPopUpController>().SetNewEpidemy();
            outbreak = true;

            NotificationCenter.Instance.EpidemyStarting.Invoke(new BaseNotificationEventArgs());

            if (useHeli)
                EpidemyHelicopter.Instance.FlyInCargo();
            epidemyCharacter.SetBool("Idle", false);
        }

        public void OnGameEventActivate()
        {
            if (Level >= EpidemyObject.EpidemyObjectInfo.UnlockLvl && unwrapped && wasGameLoaded && !outbreak)
            {
                if (TimeTillOutbreak > EpidemyOutbreakTime)
                    TimeTillOutbreak = EpidemyOutbreakTime;
            }
        }

        public void OnEpidemyUnwrap()
        {
            if (!unwrapped)
            {
                GenerateComingMedicines();
                StartEpidemy(EpidemyEndTime, true);
                unwrapped = true;
            }
        }

        private void GenerateComingMedicines()
        {
            levelWhileGeneratingMedicines = VisitingController.Instance.IsVisiting ? SaveLoadController.Get().GetSaveState().Level : Game.Instance.gameState().GetHospitalLevel();
            availableMedicines = ResourcesHolder.Get().GetMedicines();

            selectedMedicinesIndexes.Clear();
            comingMedicinesSprites.Clear();

            int medicineIndex;
            MedicineDatabaseEntry[] medicines = EpidemyPackageGenerator.GetAllMedicineTypeInPackages(levelWhileGeneratingMedicines);
            for (int i = 0; i < medicines.Length; ++i)
            {
                medicineIndex = GetMedicineIndex(medicines[i]);
                selectedMedicinesIndexes.Add(medicineIndex);
            }
            UIController.getHospital.EpidemyOffPopUp.SetMedicinesImages(GetComingMedicinesSprites());
            UIController.getHospital.EpidemyOffPopUp.SetMedicinesTooltips(GetComingMedicinesRefs());
        }

        private int GetMedicineIndex(MedicineDatabaseEntry medicine)
        {
            return availableMedicines.IndexOf(medicine);
        }

        public List<Sprite> GetComingMedicinesSprites()
        {
            comingMedicinesSprites.Clear();
            for (int i = 0; i < selectedMedicinesIndexes.Count; i++)
            {
                if (selectedMedicinesIndexes.Count > i && availableMedicines.Count > selectedMedicinesIndexes[i])
                    comingMedicinesSprites.Add(ResourcesHolder.Get().GetSpriteForCure(availableMedicines[selectedMedicinesIndexes[i]].GetMedicineRef()));
                else
                    comingMedicinesSprites.Add(null);
            }

            return comingMedicinesSprites;
        }

        public List<MedicineRef> GetComingMedicinesRefs()
        {
            List<MedicineRef> temp = new List<MedicineRef>();
            for (int i = 0; i < selectedMedicinesIndexes.Count; i++)
            {
                if (selectedMedicinesIndexes.Count > i && availableMedicines.Count > selectedMedicinesIndexes[i])
                    temp.Add(availableMedicines[selectedMedicinesIndexes[i]].GetMedicineRef());
                else
                    temp.Add(null);
            }
            return temp;
        }

        public int CalculateEpidemySpeedUpCost()
        {
            return DiamondCostCalculator.GetCostForBuilding(TimeTillOutbreak, EpidemyOutbreakTime);
        }

        public void SpeedUpEpidemyStartWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = CalculateEpidemySpeedUpCost();
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpEpidemy);
                    StartEpidemy(EpidemyEndTime, true);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                }, diamondTransactionMaker);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void ChangeEpidemyInputPermission()
        {
            isHelicopterInAction = !isHelicopterInAction;
        }

        public void ManageChestDeployment()
        {
            chestClosed.gameObject.SetActive(true);
            MovementEffects.Timing.CallDelayed(1f, () => SwapChests(false));
        }

        public void ManageChestPickUp()
        {
            chestClosed.gameObject.SetActive(false);
        }

        public void SetChestReadyToPickUp()
        {
            chestOpen.gameObject.SetActive(true);
            MovementEffects.Timing.CallDelayed(1f, () => SwapChests(true));
        }

        private void SwapChests(bool isEpidemyOver)
        {
            if (isEpidemyOver)
            {
                chestClosed.gameObject.SetActive(true);
				chestOpen.gameObject.SetActive (false);               	
            }
            else
            {                
                chestOpen.gameObject.SetActive(true);
				chestClosed.gameObject.SetActive (false);                
            }
        }

        private void LerpAlpha(SpriteRenderer a, SpriteRenderer b)
        {
            transitionProgress = (Time.time /*- alphaTransitionStartTime*/) / alphaTransitionDuration;

            a.color = Color.Lerp(new Color(1f, 1f, 1f, 1f), new Color(1f, 1f, 1f, 0f), transitionProgress);
            b.color = Color.Lerp(new Color(1f, 1f, 1f, 0f), new Color(1f, 1f, 1f, 1f), transitionProgress);
        }

        public void ForceEpidemyEnd()
        {
            StopEpidemy(EpidemyOutbreakTime);

            AnalyticsController.instance.ReportEpidemyFinished(true);
        }

        private void StopEpidemy(float time)
        {
            if (UIController.getHospital.EpidemyOnPopUp.gameObject.activeSelf)
                UIController.getHospital.EpidemyOnPopUp.Exit();

            HospitalAreasMapController.HospitalMap.epidemy.HelpMark.SetActive(false);
            TimeTillOutbreak = time;
            TimeTillEpidemyEnd = EpidemyEndTime;
            outbreak = false;
            GenerateComingMedicines();
            signAnimator.SetTrigger("Idle");
            generatorAnim.SetTrigger("Idle");
            epidemyCharacter.ResetTrigger("Active");
            epidemyCharacter.SetBool("Idle", true);
            epidemyCharacter.SetBool("EpidemyCompleted", false);

            playAnimation = true;

            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                PackageHelpRequestManager.Instance.GetMyPackageRequests(DeletePackagesHelpRequests);

            SetChestReadyToPickUp();
            EpidemyHelicopter.Instance.FlyInNoCargo();
        }

        //before deleting all requests from aws and create or stop epidemy tries to find all packages
        //that were fullfied by visitors and awards player
        private void DeletePackagesHelpRequests(List<PackageHelpRequest> requests)
        {
            List<EpidemyOnPopUpController.Package> packages = UIController.getHospital.EpidemyOnPopUp.Packages;
            foreach (var request in requests)
            {
                if (request.helped)
                {
                    int packageId = Convert.ToInt32(request.BoxID);
                    GameState.Get().AddCoins(packages[packageId].coinsReward, EconomySource.Epidemy);
                    GameState.Get().AddExperience(packages[packageId].coinsReward, EconomySource.Epidemy, true);
                }
                PackageHelpRequestManager.Instance.DeletePackageHelpRequest(request.BoxID);
            }

            if (HasAnyHelpRequest)
            {
                HasAnyHelpRequest = false;
                PublicSaveManager.Instance.UpdatePublicSave();
            }
        }

        private void Update()
        {
            if (Level >= EpidemyObject.EpidemyObjectInfo.UnlockLvl && unwrapped)
            {
                if (wasGameLoaded && !isHelicopterInAction)
                {
                    if (outbreak)
                    {
                        if (playAnimation)
                        {
                            signAnimator.SetTrigger("Active");
                            playAnimation = false;
                            generatorAnim.SetTrigger("Active");
                            epidemyCharacter.SetTrigger("Active");
                        }
                        if (TimeTillEpidemyEnd > 0)
                        {
                            TimeTillEpidemyEnd -= Time.deltaTime;
                            if (TimeTillNextHelpStatusRefresh > 0)
                                TimeTillNextHelpStatusRefresh -= Time.deltaTime;
                            else
                            {
                                TimeTillNextHelpStatusRefresh = RefreshAwaitTime;
                                UIController.getHospital.EpidemyOnPopUp.Refresh();
                            }
                        }
                        else
                        {
                            StopEpidemy(EpidemyOutbreakTime);
                            AnalyticsController.instance.ReportEpidemyFinished(false);
                        }
                    }
                    else
                    {
                        if (TimeTillOutbreak > 0)
                            TimeTillOutbreak -= Time.deltaTime;
                        else
                            StartEpidemy(EpidemyEndTime, true);
                    }
                }
            }
        }

        public List<string> SaveEpidemyDataToString()
        {
            List<string> saveData = new List<string>();

            saveData.AddRange(EpidemyObject.SaveToString());

            saveData.Add(unwrapped.ToString());
            saveData.Add(outbreak.ToString());

            if (unwrapped)
            {
                if (outbreak)
                {
                    saveData.Add(TimeTillEpidemyEnd.ToString());
                    saveData.Add(SaveSelectedMedcinesData());
                    saveData.AddRange(UIController.getHospital.EpidemyOnPopUp.SavePackagesToStringList());
                }
                else
                {
                    saveData.Add(TimeTillOutbreak.ToString());
                    saveData.Add(SaveSelectedMedcinesData());
                }
            }
            else
                saveData.Add(TimeTillEpidemyEnd.ToString());

            return saveData;
        }

        private string SaveSelectedMedcinesData()
        {
            string selectedMedicinesData = "";
            for (int i = 0; i < numberOfDifferentMedicinesInEpidemy; ++i)
            {
                selectedMedicinesData += selectedMedicinesIndexes[i] + "!";
            }
            selectedMedicinesData += levelWhileGeneratingMedicines;
            return selectedMedicinesData;
        }

        public void LoadEpidemyDataFromString(List<string> saveData, TimePassedObject timeSinceLastSave, bool visitingMode)
        {
            EpidemyHelicopter.Instance.ResetAfterReload();
            wasGameLoaded = false;
            HelpMark.SetActive(false);
            if (saveData != null)
            {
                //try
                //{
                EpidemyObject.LoadFromString(saveData, timeSinceLastSave);
                unwrapped = Convert.ToBoolean(saveData[2]);
                bool wasEpidemyPreviouslyActive = Convert.ToBoolean(saveData[3]);
                float timeLeftFromLastState = float.Parse(saveData[4], CultureInfo.InvariantCulture);
                float timeToEvaluate = timeLeftFromLastState - timeSinceLastSave.GetTimePassed();

                DetermineEpidemyState(wasEpidemyPreviouslyActive, timeToEvaluate);

                if (unwrapped)
                {
                    UIController.getHospital.EpidemyOnPopUp.CheckPackagesFromLastEpidemy(saveData);
                    if (outbreak)
                    {
                        chestClosed.gameObject.SetActive(false);
                        chestOpen.gameObject.SetActive(true);
                        if (hasEpidemyChanged)
                        {
                            if (shouldGenerateNewMedicines)
                            {
                                GenerateComingMedicines();
                                StartEpidemy(TimeTillEpidemyEnd, false);
                            }
                            else
                            {
                                LoadSelectedMedcinesData(saveData[5]);
                                StartEpidemy(TimeTillEpidemyEnd, false);
                            }
                        }
                        else
                        {
                            LoadSelectedMedcinesData(saveData[5]);
                            UIController.getHospital.EpidemyOnPopUp.LoadPackaesFromString(saveData);
                        }
                    }
                    else
                    {
                        chestClosed.gameObject.SetActive(false);
                        chestOpen.gameObject.SetActive(false);
                        if (hasEpidemyChanged)
                            GenerateComingMedicines();
                        else
                            LoadSelectedMedcinesData(saveData[5]);
                    }
                }

                string output = System.String.Format
                (
                    "Time since last save: {0}\nTime to evaluate: {1}\nWasEpidemyActive: {5}\nIs epidemy active: {2}\nTime till epidemy: {3}\nTime till epidemy end: {4}",
                    timeSinceLastSave, timeToEvaluate, outbreak, TimeTillOutbreak, TimeTillEpidemyEnd, wasEpidemyPreviouslyActive).Replace("\n", Environment.NewLine
                );
            }
            if (outbreak)
            {
                signAnimator.SetBool("Active", true);
                playAnimation = true;
            }
            wasGameLoaded = true;
        }

        private void LoadSelectedMedcinesData(string saveData)
        {
            selectedMedicinesIndexes.Clear();

            string[] records = saveData.Split('!');

            for (int i = 0; i < records.Length; ++i)
            {
                Debug.Log(records[i] + " selected medicines");
            }

            for (int i = 0; i < records.Length; ++i)
            {
                if (i < records.Length - 1)
                    selectedMedicinesIndexes.Add(int.Parse(records[i], System.Globalization.CultureInfo.InvariantCulture));
                else
                    levelWhileGeneratingMedicines = int.Parse(records[records.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
            }
            
            availableMedicines = ResourcesHolder.Get().GetMedicines();
            UIController.getHospital.EpidemyOffPopUp.SetMedicinesImages(GetComingMedicinesSprites());
            UIController.getHospital.EpidemyOffPopUp.SetMedicinesTooltips(GetComingMedicinesRefs());
        }

        public void GenerateDefaultSave()
        {
            Debug.LogWarning("Epidemy data corrupted. Generating default save.");

            if (Game.Instance.gameState().GetHospitalLevel() < EpidemyObject.EpidemyObjectInfo.UnlockLvl)
            {
                unwrapped = false;
                outbreak = false;
                TimeTillOutbreak = 0f;
            }
            else
            {
                GenerateComingMedicines();
                unwrapped = true;
                outbreak = false;
                TimeTillOutbreak = EpidemyOutbreakTime;
            }

            wasGameLoaded = true;
        }

        private void DetermineEpidemyState(bool wasEpidemyPreviouslyActive, float timeSinceLastSave)
        {
            if (wasEpidemyPreviouslyActive)
            {
                if (timeSinceLastSave > 0)
                {
                    hasEpidemyChanged = false;
                    TimeTillEpidemyEnd = timeSinceLastSave;
                    outbreak = true;
                    shouldGenerateNewMedicines = false;
                }
                else
                {
                    hasEpidemyChanged = true;
                    DetermineEpidemyProgressSinceLastActiveness(Mathf.Abs(timeSinceLastSave));
                    shouldGenerateNewMedicines = true;
                }
            }
            else
            {
                if (timeSinceLastSave > 0)
                {
                    hasEpidemyChanged = false;
                    TimeTillOutbreak = timeSinceLastSave;
                    outbreak = false;
                    shouldGenerateNewMedicines = false;
                }
                else
                {
                    hasEpidemyChanged = true;
                    DetermineEpidemyProgressSinceLastUnactiveness(Mathf.Abs(timeSinceLastSave));
                    if (isNewEpidemy)
                        shouldGenerateNewMedicines = true;
                    else
                        shouldGenerateNewMedicines = false;
                }
            }
        }

        private void DetermineEpidemyProgressSinceLastActiveness(float timeSinceLastSave)
        {
            float currentStateTimeLeft;

            if (timeSinceLastSave < EpidemyOutbreakTime + EpidemyEndTime)
                DetermineEpidemyStateAfterActiveEpidemy(timeSinceLastSave);
            else
            {
                currentStateTimeLeft = timeSinceLastSave % (EpidemyOutbreakTime + EpidemyEndTime);
                DetermineEpidemyStateAfterActiveEpidemy(currentStateTimeLeft);
            }
        }

        private void DetermineEpidemyStateAfterActiveEpidemy(float timeSinceLastSave)
        {
            if (timeSinceLastSave < EpidemyOutbreakTime)
            {
                TimeTillOutbreak = EpidemyOutbreakTime - timeSinceLastSave;
                outbreak = false;
            }
            else if (timeSinceLastSave - EpidemyOutbreakTime < EpidemyEndTime)
            {
                TimeTillEpidemyEnd = EpidemyEndTime - (timeSinceLastSave - EpidemyOutbreakTime);
                outbreak = true;
            }
        }

        private void DetermineEpidemyProgressSinceLastUnactiveness(float timeSinceLastSave)
        {
            float currentStateTimeLeft;
            if (timeSinceLastSave < EpidemyOutbreakTime + EpidemyEndTime)
            {
                isNewEpidemy = false;
                DetermineEpidemyStateAfterUnactiveEpidemy(timeSinceLastSave);
            }
            else
            {
                isNewEpidemy = true;
                currentStateTimeLeft = timeSinceLastSave % (EpidemyOutbreakTime + EpidemyEndTime);
                DetermineEpidemyStateAfterUnactiveEpidemy(currentStateTimeLeft);
            }
        }

        private void DetermineEpidemyStateAfterUnactiveEpidemy(float timeSinceLastSave)
        {
            if (timeSinceLastSave < EpidemyEndTime)
            {
                TimeTillEpidemyEnd = EpidemyEndTime - timeSinceLastSave;
                outbreak = true;
            }
            else if (timeSinceLastSave - EpidemyEndTime < EpidemyOutbreakTime)
            {
                TimeTillOutbreak = EpidemyOutbreakTime - (timeSinceLastSave - EpidemyEndTime);
                outbreak = false;
            }
        }

		public void ChestActive(bool active)
        {
			chestOpen.gameObject.SetActive (active);
			chestClosed.gameObject.SetActive (active);
		}       
        
        public void EmulateTime(TimePassedObject timePassed)
        {
            EpidemyObject.EmulateTime(timePassed);
        }
    }
}


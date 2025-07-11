using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using IsoEngine;
using UnityEngine.EventSystems;
using MovementEffects;
using TutorialSystem;

namespace Hospital
{
    public class HospitalAreasMapController : AreaMapController
    {

        #region references
        public GameObject GroundGrassPrefab; // out??

        public List<Sprite> DiagnosisBuildDummiesSprite;
        public Pharmacy pharmacy;
        public Reception reception;
        public Emergency emergency;
        public VipRoom vipRoom;
        public Plantation greenHouse;
        public HospitalBedController hospitalBedController;

        public CarsManager carsManager;
        public GameEventsChestsManager gameEventsChestsManager;
        public GlobalEventChestsManager globalEventsChestsManager;

        public Epidemy epidemy;
        public MaternityWard maternityWard;
        public BubbleBoyEntryOverlayController bubbleBoy;
        public GameObject FakeWallERPrefab;
        protected GameObject FakeWallER = null;
        public VisualsUpgradeController fakeWallVisualUpgrade = null;

        private List<Decoration> allDecorationsExceptLab = new List<Decoration>();
        public List<ProbeTable> allProbeTableOnMap;

        #endregion

        #region variables
        Rectangle receptionArea;
        #endregion

        #region Static Access,

        private static HospitalAreasMapController _hospitalmap;

        public static HospitalAreasMapController HospitalMap
        {
            get
            {
                if (_hospitalmap == null)
                {
                    //Debug.LogError("Scene should contain exactly one HospitalAreasMapController if you want size use this feature");
                    //throw new IsoException("There is no mapController on scene!");
                }
                return _hospitalmap;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_hospitalmap != null && _hospitalmap != this)
            {
                Debug.LogWarning("Multiple instances of HospitalAreasMapController were found!");
            }
            else
                _hospitalmap = this;
        }

        #endregion

        #region objectsManipulation

        [TutorialTriggerable]
        public void RemoveCover()
        {
            CoverController cover = (CoverController)HospitalMap.FindRotatableObject("ProbTabCover");
            if (cover != null)
                cover.RemoveCover();
        }

        public MedicineProductionMachine FindMedicineProductionMachineWithTag(string tag)
        {
            foreach (var p in areas)
            {
                var t = p.Value.FindRotatableObject(tag);
                if (t != null)
                {
                    if (t.gameObject.GetComponent<MedicineProductionMachine>() == null)
                    {
                        //Debug.LogWarning("MedicineProductionMachine not found. Tag = " + tag);
                    }
                    return t.gameObject.GetComponent<MedicineProductionMachine>();
                }
            }
            // Debug.LogWarning("MedicineProductionMachine not found. Tag = " + tag);
            return null;
        }

        public override bool AddDecorationToMap(Decoration currentDecoration)
        {
            if (allDecorationsOnMap == null)
            {
                allDecorationsOnMap = new List<Decoration>();
            }
            if (allDecorationsExceptLab == null)
            {
                allDecorationsExceptLab = new List<Decoration>();
            }

            if (decoAmountMap.Count == 0)
            {
                GetDecorationFromDataBase();
            }



            if (currentDecoration.area != HospitalArea.Laboratory && !allDecorationsExceptLab.Contains(currentDecoration))
            {
                allDecorationsExceptLab.Add(currentDecoration);
            }

            if (!allDecorationsOnMap.Contains(currentDecoration))
            {
                allDecorationsOnMap.Add(currentDecoration);
                decoAmountMap[currentDecoration.Tag]++;
                return true;
            }
            return false;
        }
        public override void RemoveDecorationFromMap(Decoration currentDecoration)
        {
            if ((allDecorationsOnMap != null) && (allDecorationsOnMap.Contains(currentDecoration)))
            {
                allDecorationsOnMap.Remove(currentDecoration);
                decoAmountMap[currentDecoration.Tag]--;
                if (decoAmountMap[currentDecoration.Tag] <= 0)
                {
                    decoAmountMap[currentDecoration.Tag] = 0;
                }
            }

            if ((allDecorationsExceptLab != null) && (allDecorationsExceptLab.Contains(currentDecoration)))
            {
                allDecorationsExceptLab.Remove(currentDecoration);
            }
        }

        public override int CanMakePathTo(Vector2i pos, PathType[] doorPathTypes, bool isDeco = false)
        {
            try
            {
                int founded_paths = 0;

                Vector2i spawn;

                for (int i = 0; i < doorPathTypes.Length; i++)
                {
                    if (doorPathTypes[i] == PathType.GoEmergencyPath)
                        spawn = new Vector2i(23, 31);
                    else if (doorPathTypes[i] == PathType.GoHomePath)
                        spawn = new Vector2i(22, 42);
                    else if (doorPathTypes[i] == PathType.GoPatioPath)
                        spawn = new Vector2i(43, 42);
                    else spawn = new Vector2i(20, 20);

                    var path = GetLevel<PFLevelController>(0).GetPath(pos, spawn, doorPathTypes[i], false);

                    if (path == null)
                    {
                        //Debug.LogWarning("No paths for: " + doorPathTypes[i]);
                        return -1;
                    }
                    else
                    {
                        if (path.path.Count == 0)
                        {
                            return -1;
                        }

                        if (!isDeco)
                        {
                            if (path.path.Count > 3) // fix for mutabe rooms
                            {
                                if (!IsPosInsideClinic(path.path[1]))
                                {
                                    return 0;
                                }
                            }
                        }
                        founded_paths++;
                    }
                }

                if (founded_paths == doorPathTypes.Length)
                    return 1;

                return -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public override bool CheckExitAvailabilityFromNotPlacedRoom(Vector2i doorPos, Vector2i pos, Vector2i size)
        {
            try
            {
                var level = GetLevel<PFLevelController>(0);
                level.SetAreaPassable(pos, size, false);
                var canMakePath = CanMakePathFromPositionToAllExits(doorPos, false, false);
                level.SetAreaPassable(pos, size, true);
                return canMakePath;
            }
            catch (Exception e)
            {
                Debug.LogError("Error while calculating exit pos: " + e.Message);
                return false;
            }
        }

        public override bool IsPosInsideClinic(Vector2i pos)
        {
            if (areas[HospitalArea.Clinic].ContainsPoint(pos))
                return true;
            else return false;
        }

        public override bool IsPosInsidePatio(Vector2i pos)
        {
            if (areas[HospitalArea.Patio].ContainsPoint(pos))
                return true;
            else return false;
        }

        public void AddProbeTableToMap(ProbeTable currentProbeTable)
        {
            if (allProbeTableOnMap == null)
            {
                allProbeTableOnMap = new List<ProbeTable>();
            }

            if (!allProbeTableOnMap.Contains(currentProbeTable))
                allProbeTableOnMap.Add(currentProbeTable);
        }

        public void RemoveProbeTableFromMap(ProbeTable currentProbeTable)
        {
            if ((allProbeTableOnMap != null) && (allProbeTableOnMap.Contains(currentProbeTable)))
            {
                allProbeTableOnMap.Remove(currentProbeTable);
            }
        }
        #endregion

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        #region Save Load
        protected override void ReconstructFromMapData()
        {
            if (!Reconstructed && _shouldReconstruct)
            {
                GetDecorationFromDataBase();
                var save = SaveLoadController.SaveState;
                if (save.Level == 0)
                {
                    AnalyticsController.instance.ReportBug("reconstruct_level_zero_1");
                    BaseUIController.ShowCriticalProblemPopup(this);
                    return;
                }

                ReferenceHolder.Get().lootBoxManager.OnMapDestroy();
                UIController.get.preloaderView.Exit();
                timePassed = new HospitalTimePassedObject(save.saveDateTime, save.MaternitySaveDateTime);
                ReferenceHolder.GetHospital().treatmentHelpAPI.StopRefreshingRequests();
                ReferenceHolder.GetHospital().ClinicAI.Reset();
                ReferenceHolder.GetHospital().HospitalSpawner.Reset();
                ReferenceHolder.Get().floorControllable.Reset();
                HospitalDataHolder.Instance.Reset(); //matward tutaj trzeba wezwac po abstrakcji najpierw trzeba ja zrobic
                HospitalAreasMapController.HospitalMap.carsManager.OnLoad();
                ReferenceHolder.GetHospital().Ambulance.ResetAmbulance();

                if (ReloadGamestate)
                {
                    VisitingController.Instance.canVisit = true;

                    GameState.Get().LoadData(save);
                    treasureManager.Load(save.Treasure);
                    DailyQuestSynchronizer.Instance.LoadFromString(save.DailyQuest, VisitingMode);
                    //GlobalEventSynchronizer.Instance.LoadFromString(save.GlobalEvent, VisitingMode);
                    TreatmentRoomHelpSynchronizer.Instance.LoadFromString(save.TreatmentHelp, VisitingMode);
                }
                else
                {
                    GiftsReceiveController.Instance.Initialize(VisitingMode);
                    GameState.Get().LoadDrawerData(save);
                    UIController.getHospital.DailyQuestMainButtonUI.Refresh();
                }

                if (Game.Instance.gameState().GetHospitalLevel() == 0)
                {
                    AnalyticsController.instance.ReportBug("reconstruct_level_zero_2", 0, VisitingController.Instance.IsVisiting);
                    BaseUIController.ShowCriticalProblemPopup(this);
                    return;
                }

                UIController.getHospital.PharmacyPopUp.salesUnlocked = save.PharmacySlots;

                ReconstructAreas(save);


                #region static objects
                //removing from here. It should be placed when the visiting mode is set.
                //SuperObject.visitingMode = VisitingMode;

                reception.Initialize(mapConfig.StaticAreas[0].defaultAreas[0].position + new Vector2i(2, 0));
                reception.SetLevel(save.ReceptionLevel, false);
                reception.isBusy = false; //save.ReceptionIsBusy;

                ReconstructStaticWalls();
                //emergency.SetLevel(save.EmergencyLevel);
                playgroud.SetLevel(save.PlaygroundLevel);

                if (!VisitingMode)
                {
                    pharmacy.LoadState(save, timePassed);
                    pharmacy.UpdateSoldOffersState();
                }
                else
                {
                    pharmacy.UpdateNewOffersState();
                    TutorialController.Instance.StopTutorialCameraFollowingForSpecialObjects();
                }

                greenHouse.LoadFromStringList(save.Plantation, timePassed, VisitingMode);

                vipRoom.LoadFromString(save.VipHouse, timePassed);
                playgroud.LoadFromString(save.PlayHouse, timePassed);
                if (!VisitingMode)
                {
                    GameState.Get().canSpawnKids = playgroud.CanGetKids();
                }

                emergency.ShowEmergencyIndicator = save.ShowEmergencyIndicator;
                emergency.CheckIndicatorStatus();
                ReferenceHolder.GetHospital().HospitalNameSign.SetIndicatorVisible(save.ShowSignIndicator);
                UIController.get.drawer.SetPaintBadgeClinicVisible(save.ShowPaintBadgeClinic);
                UIController.get.drawer.SetPaintBadgeLabVisible(save.ShowPaintBadgeLab);

                //bubbleBoy.LoadFromString(save.BubbleBoy, (int)time.TotalSeconds);
                //epidemy.Level = Convert.ToInt32(save.Level);

                BubbleBoyDataSynchronizer.Instance.LoadFromString(save.BubbleBoy, VisitingMode);
                ReferenceHolder.GetHospital().bubbleBoyEntryOverlayController.SetSessionTimer(VisitingMode);

                epidemy.LoadEpidemyDataFromString(save.EpidemyData, timePassed, VisitingMode);

                UIController.getHospital.AchievementsPopUp.ac.LoadFromStringList(save.Achievements);

                boosterManager.LoadFromString(save.Booster, VisitingMode);

                #endregion

                RotatableObject.visitingMode = VisitingMode;

                // because map must be reconstructed before
                if (ReloadGamestate)
                {
                    ObjectivesSynchronizer.Instance.LoadFromString(save.LevelGoals, VisitingMode);
                    ReferenceHolder.Get().objectiveController.CheckCompletedBadges();
                }

                if (!VisitingMode)
                {
                    GameState.Get().PatientsCount.Load(save.AdvancedPatientCounter);
                    GameState.Get().CuresCount.Load(save.AdvancedCuresCounter);
                }

                ReconstructObjects(save, timePassed);
                TryToInitializeMocks(); // it has to be after reconstrucObjects due to drawing medicine which refers to elixir storage which is instantiate in reconstructObjects method

                if (ReloadGamestate)
                {
                    GlobalEventSynchronizer.Instance.LoadFromString(save.GlobalEvent, VisitingMode);
                }

                CountDecorationsFromSecondMap(save.MaternityPatioObjectsData);
                CountDecorationsFromSecondMap(save.MaternityClinicObjectsData);
                UIController.get.drawer.UpdatePrices();

                //spawn fakewalls
                AddFakeWall();
                vipRoom.transform.GetComponent<VIPSystemManager>().LoadFromString(save.VIPSystem, timePassed);
                TutorialUIController.Instance.HideIndicator();
                //TutorialUIController.Instance.tutorialArrowUI.Hide();


                maternityWard.LoadFromString(save.MaternityWardData, timePassed);

                casesManager.LoadFromString(save.Cases, (timePassed), VisitingMode);
                ReferenceHolder.GetHospital().dailyDealController.Load(save.DailyDeal, VisitingMode);


                treasureManager.OnLoad();

                if (updateDoorInteractionCorountine == null)
                {
                    updateDoorInteractionCorountine = Timing.RunCoroutine(UpdateDoorInteractionCorountine());
                }


                reconstructed = true;
                _shouldReconstruct = false;

                if (!VisitingMode)
                {
                    //UIController.get.drawer.SetDrawerItems();
                }
                if (VisitingMode)
                    UIController.get.SetVisitingUI(save.HospitalName, save.Level, save.ID);
                else
                    UIController.get.SetStandardUI();
                if (UIController.get.PoPUpArtsFromResources != null && UIController.get.PoPUpArtsFromResources.Count > 0)
                {
                    foreach (KeyValuePair<string, GameObject> artGameObject in UIController.get.PoPUpArtsFromResources)
                    {
                        Destroy(artGameObject.Value);
                    }
                    Resources.UnloadUnusedAssets();
                }
                wanderingArea = new Rectangle(43, 37, 6, 12);
                receptionArea = new Rectangle(24, 42, 4, 2);

                ReferenceHolder.GetHospital().ClinicAI.GetComponent<IPatientSaver>().LoadFromStringList(save.OtherPatients);

                if (!VisitingMode)
                {
                    GameState.Get().RecountElixirs();
                    ReferenceHolder.GetHospital().HospitalNameSign.SetNameOnSign();
                }
                else ReferenceHolder.GetHospital().HospitalNameSign.SetNameOnSign(save.HospitalName);

                HospitalAreasMapController.HospitalMap.pharmacy.GetComponent<Pharmacy>().OnLoad();


                int UnlockLevel = ReferenceHolder.GetHospital().bubbleBoyEntryOverlayController.roomInfo.UnlockLvl;
                int ActualLevel = VisitingController.Instance.IsVisiting ? SaveLoadController.SaveState.Level : Game.Instance.gameState().GetHospitalLevel();
                TutorialController tc = TutorialController.Instance;

                if ((tc.GetStepId(tc.CurrentTutorialStepTag) > tc.GetStepId(StepTag.blink_box_button) && ActualLevel >= UnlockLevel) || SaveLoadController.SaveState.ID == "SuperWise" || HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState == ExternalRoom.EExternalHouseState.enabled)
                {
                    HospitalAreasMapController.HospitalMap.bubbleBoy.ExternalHouseState = ExternalRoom.EExternalHouseState.enabled;
                    ReferenceHolder.GetHospital().bubbleBoyCharacterAI.Initialize();
                }
                else
                {
                    ReferenceHolder.GetHospital().bubbleBoyCharacterAI.Disable();
                }

                //spawn fakewalls
                AddFakeWallER();

                HospitalCustomizationSynchronizer.Instance.LoadFromString(save.HospitalCustomizations);

                GameState.Get().CheckIfLevelUP();
                GameState.Get().RefreshXPBar();

                if (TimedOffersController.Instance != null)
                {
                    TimedOffersController.Instance.LoadFromPlayerPrefs();
                }

                if (!VisitingMode)
                {
                    GameState.Get().CheckExpDependantTutorial(new BaseNotificationEventArgs());
                    UIController.get.reportPopup.timeFromSave = (int)timePassed.GetTimePassed();
                    IAPController.instance.LoadTransactions(save.CompletedIAP);
                    ReferenceHolder.GetHospital().globalEventController.StartGlobalEventSystem();
                    if (TimedOffersController.Instance != null)
                    {
                        TimedOffersController.Instance.FetchTimedOffers();
                        if (IAPController.instance.IsInitialized())
                        {
                            TimedOffersController.Instance.onTimedOffersUpdated -= TimedOffersController.Instance.TryToShowOnLoad;
                            TimedOffersController.Instance.onTimedOffersUpdated += TimedOffersController.Instance.TryToShowOnLoad;
                        }
                        else
                        {
                            IAPController.instance.onIapInitialized -= TimedOffersController.Instance.TryToShowOnLoad;
                            IAPController.instance.onIapInitialized += TimedOffersController.Instance.TryToShowOnLoad;
                        }
                    }
                }

                ReferenceHolder.GetHospital().treatmentRoomHelpController.Initialize(VisitingMode);
                ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.Initialize(VisitingMode);

                // initialized when not set
                if (ReloadGamestate)
                {
                    ObjectivesSynchronizer.Instance.Initialize();
                }

                ReferenceHolder.GetHospital().animalsController.InitializeDeer();
                GameEventsController.Instance.Initialize();
                gameEventsChestsManager.Initialize(VisitingMode, save.lastGameEventChestSpawnTime, save.gameEventChestsCount);
                GameEventsStandController.Instance.OnMapLoaded();

                ReferenceHolder.GetHospital().camping.ResetFireplace();

                ReferenceHolder.GetHospital().BillboardAd.SetSprite();

                UIController.get.LootBoxButtonUI.Initialize();
                ReferenceHolder.Get().lootBoxManager.OnMapLoaded(VisitingMode);

                ResetOntouchAction();

                ReferenceHolder.Get().IAPShopController.Initialize();
                UIController.getHospital.GoToMaternityButton.Initialize();
                ReferenceHolder.GetHospital().MaternityStatusController.LoadFromString(save);

                if (VisitingMode)
                    ReputationSystem.ReputationController.Instance.SetIconsWhenVisiting(save.ID);
                else
                    ReputationSystem.ReputationController.Instance.Load(save.ReputationAmounts);

                StartCoroutine(DelayedUpdateMastership());
                ReferenceHolder.Get().multiSceneInformationController.InitializeFromSave(save);
                ReferenceHolder.GetHospital().DailyRewardController.LoadFromString(save.dailyRewardSave);
                ReferenceHolder.GetHospital().RecomensationGiftController.Initialize();

                if (VisitingMode)
                {
                    if (maternityWard != null)
                    {
                        maternityWard.TurnOffIndicator();
                    }
                }

                if (!VisitingMode && TutorialController.Instance.tutorialEnabled)
                {
                    bool reload = PlayerPrefs.GetInt(TutorialSystem.TutorialController.Name) == 2;
                    if (reload)
                    {
                        //ReloadTutorials
                        TutorialSystem.TutorialController.LoadNonLinearStepsCompletion(null);
                    }
                    else
                    {
                        TutorialSystem.TutorialController.LoadNonLinearStepsCompletion(save.NonLinearCompletion);
                    }

                    //TutorialController.Instance.SubscribeNonLinearSteps();
                    //
                    //
                    StepTag stepTagFromSaveFile;
                    if (reload)
                    {
                        //GET CURRENT LEVEL
                        stepTagFromSaveFile = TutorialProgressChecker.GetInstance().GetTutorialStepToLevel(Game.Instance.gameState().GetHospitalLevel());
                    }
                    else
                    {
                        //GET STEP FROM SAVE
                        stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
                    }

                    if (stepTagFromSaveFile == StepTag.emma_about_wise && Game.Instance.gameState().GetHospitalLevel() < 6)
                    {//Wise error
                        stepTagFromSaveFile = TutorialProgressChecker.GetInstance().GetTutorialStepToLevel(Game.Instance.gameState().GetHospitalLevel()); ;
                    }
                    else if (stepTagFromSaveFile == TutorialSystem.TutorialModule.Controller.finalStepTag && Game.Instance.gameState().GetHospitalLevel() < 23)
                    { //Maternity error
                        stepTagFromSaveFile = TutorialProgressChecker.GetInstance().GetTutorialStepToLevel(Game.Instance.gameState().GetHospitalLevel()); ;
                    }

                    //bool IsArrowAnimationNeededForWhiteElixir = save.IsArrowAnimationNeededForWhiteElixir;
                    if (TutorialController.Instance.StepIsInWiseHospital(stepTagFromSaveFile))
                        stepTagFromSaveFile = StepTag.blink_friends;

                    Debug.LogFormat("<color=cyan> Init tutorial to: </color> <color=green>{0} </color>", stepTagFromSaveFile);
                    TutorialSystem.TutorialController.Init(stepTagFromSaveFile);
                }

                base.ReconstructFromMapData();
            }
        }

        private static void TryToInitializeMocks()
        {
            //if (DeveloperParametersController.Instance().parameters.UseMockedIapShop)
            //{
            //    Debug.LogError("Initializing mocked iap shop");
            //    Dictionary<string, object> data = new Dictionary<string, object>();
            //    foreach (MockedDataFromDelta coinData in DeveloperParametersController.Instance().parameters.MockedIapShopCoins)
            //    {
            //        if (coinData.useThisMock)
            //        {
            //            data.Add(coinData.Key, coinData.Value);
            //        }
            //    }
            //    IAPShopConfig.InitializeCoinsPackages(data);
            //    data.Clear();
            //    foreach (MockedDataFromDelta orderData in DeveloperParametersController.Instance().parameters.MockedIapShopOrder)
            //    {
            //        if (orderData.useThisMock)
            //        {
            //            data.Add(orderData.Key, orderData.Value);
            //        }
            //    }
            //    IAPShopConfig.InitializeSections(data);
            //    data.Clear();
            //    foreach (MockedDataFromDelta bundleData in DeveloperParametersController.Instance().parameters.MockedIapShopBundles)
            //    {
            //        if (bundleData.useThisMock)
            //        {
            //            data.Add(bundleData.Key, bundleData.Value);
            //        }
            //    }
            //    IAPShopConfig.InitializeBundles(data);
            //}
            /*if (DeveloperParametersController.Instance().parameters.UseMockedStandardEvents)
            {
                Debug.LogError("Initializing mocked standard events");
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (MockedStandardEvent item in DeveloperParametersController.Instance().parameters.mockedEventData)
                {
                    if (item.loadEvent)
                    {
                        data.Add(item.eventKey, item.eventValue);
                    }
                }
                StandardEventConfig.InitializeEvent(data);
            }*/
            //if (DeveloperParametersController.Instance().parameters.UseMockedBalancables)
            //{
            //    Debug.LogError("Initializing mocked balancables");
            //    Dictionary<string, object> data = new Dictionary<string, object>();
            //    foreach (MockedBalancedValue item in DeveloperParametersController.Instance().parameters.mockedBalanceData)
            //    {
            //        if (item.loadValue)
            //        {
            //            data.Add(item.balanceKey, item.Value);
            //        }
            //    }
            //    BalancableConfig.InitializeBalancable(data);
            //}
            if (DeveloperParametersController.Instance().parameters.UseMockedDailyRewards)
            {
                Debug.LogError("Initializing mocked daily rewards");
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.mockedDailyRewards)
                {
                    if (item.useThisMock)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                //DailyRewardDeltaConfig.SaveUnparsedData(data);
            }
            if (DeveloperParametersController.Instance().parameters.UseMockedBundledRewardDefinitions)
            {
                Debug.LogError("Initializing mocked bundle reward definitions");
                Dictionary<string, object> data = new Dictionary<string, object>();
                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.mockedBundledDefinitions)
                {
                    if (item.useThisMock)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                //BundledRewardDefinitionConfig.InstantiateConfig(data);
            }
            if (DeveloperParametersController.Instance().parameters.UseMockedFakeContributionConfig)
            {
                Debug.LogError("Initializing mocked fake contribution config");
                Dictionary<string, object> data = new Dictionary<string, object>();
                int ind = 0;
                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.mockedFakeContributionConfig)
                {
                    if (item.useThisMock)
                    {
                        ind++;
                        data.Add(FakedContributionConfig.KEY_ID + ind.ToString(), item.Value);
                    }
                }
                FakedContributionConfig.InstantiateConfig(data);
            }
            //if (DeveloperParametersController.Instance().parameters.UseMockedGiftsPerLevel)
            //{
            //    Debug.LogError("Initializing mocked gitfs per level");
            //    Dictionary<string, object> data = new Dictionary<string, object>();

            //    foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.mockedGiftsPerLevel)
            //    {
            //        if (item.useThisMock)
            //        {
            //            data.Add(item.Key, item.Value);
            //        }
            //        LevelUpGiftsConfig.InstantiateConfig(data);
            //    }
            //}
            if (DeveloperParametersController.Instance().parameters.UseMockedAdSpeedUpOfMedicine)
            {
                Debug.LogError("Initializing mocked ad speedup of medicine");
                Dictionary<string, object> data = new Dictionary<string, object>();

                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.MockedAdSpeedUpOfMedicine)
                {
                    data.Add(item.Key, item.Value);
                }
                //AdsController.instance.SetAdConfig(data);
            }
            if (DeveloperParametersController.Instance().parameters.UseMockedCaseTierPrize)
            {
                Debug.LogError("Initializing mocked case tier config");
                Dictionary<string, object> data = new Dictionary<string, object>();

                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.MockedCaseTierRewards)
                {
                    if (item.useThisMock)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                //CasePrizeDeltaConfig.Initialize(data);
            }
            if (DeveloperParametersController.Instance().parameters.UseMockedDiamondFromTreasuresBool)
            {
                Debug.LogError("Initializing mocked diamonds from treasure bool config");
                Dictionary<string, object> data = new Dictionary<string, object>();

                foreach (MockedDataFromDelta item in DeveloperParametersController.Instance().parameters.MockedDiamondFromTreasuresBool)
                {
                    if (item.useThisMock)
                    {
                        data.Add(item.Key, item.Value);
                    }
                }
                //CasePrizeDeltaConfig.Initialize(data);
            }
        }

        protected override void ReconstructObjects(Save save, TimePassedObject timePassed)
        {
            areas[HospitalArea.Laboratory].LoadObjectsFromSave(save.LaboratoryObjectsData, timePassed);
            areas[HospitalArea.Clinic].LoadObjectsFromSave(save.ClinicObjectsData, timePassed);
            areas[HospitalArea.Patio].LoadObjectsFromSave(save.PatioObjectsData, timePassed);
        }

        public override void EmulateMapObjectTime(TimePassedObject timePassed)
        {
            // ReferenceHolder.Get().engine.MainCamera.ResetCamera();

            areas[HospitalArea.Laboratory].Notify((int)LoadNotification.EmulateTime, timePassed);
            areas[HospitalArea.Clinic].Notify((int)LoadNotification.EmulateTime, timePassed);
            areas[HospitalArea.Patio].Notify((int)LoadNotification.EmulateTime, timePassed);

            if (!VisitingMode)
            {
                if (pharmacy != null)
                {
                    pharmacy.EmulateTime(timePassed);
                }

                epidemy.EmulateTime(timePassed);
                vipRoom.EmulateTime(timePassed);
                playgroud.EmulateTime(timePassed);
                greenHouse.EmulateTime(timePassed);
                maternityWard.EmulateTime(timePassed);
            }
            base.EmulateMapObjectTime(timePassed);
        }

        protected void ReconstructAreas(Save save)
        {
            areas = new Dictionary<HospitalArea, GameArea>();

            if (mapConfig.Laboratory != null)
            {
                var lab = new GameArea(HospitalArea.Laboratory, this);
                ReconstructArea(lab, mapConfig.Laboratory, save.UnlockedLaboratoryAreas);
                areas.Add(HospitalArea.Laboratory, lab);
            }

            if (mapConfig.HospitalClinic != null)
            {
                var clinic = new GameArea(HospitalArea.Clinic, this);
                ReconstructArea(clinic, mapConfig.HospitalClinic, save.UnlockedClinicAreas);
                areas.Add(HospitalArea.Clinic, clinic);
            }
            if (mapConfig.Patio != null)
            {
                var patio = new GameArea(HospitalArea.Patio, this);
                ReconstructArea(patio, mapConfig.Patio, new List<int>() { 0 });
                areas.Add(HospitalArea.Patio, patio);
            }
        }

        public override void SaveGame(Save save)
        {
            base.SaveGame(save);
            try
            {
                if (areas != null && areas.Count > 0)
                {
                    foreach (var p in areas)
                    {
                        p.Value.SaveObjects(save);
                        p.Value.SaveBoughtAreas(save);
                    }
                }
                else
                {
                    AnalyticsController.instance.ReportBug("savebeforeload", Game.Instance.gameState().GetHospitalLevel(), VisitingController.Instance.IsVisiting);
                    throw new IsoException("Game want to save before it's loaded!");
                }

                GameState.Get().SaveData(save);

                save.ReceptionLevel = Checkers.CheckedAmount(reception.actualLevel, -1, reception.levelPrefabs.Count - 1, "Reception level");
                save.ReceptionIsBusy = false; //Checkers.CheckedBool(reception.isBusy);

                //save.EmergencyLevel = emergency.actualLevel;

                save.PlaygroundLevel = Checkers.CheckedAmount(playgroud.actualLevel, -1, 1, "Playground level");

                save.saveDateTime = (long)ServerTime.getTime();

                pharmacy.SaveState(save);

                save.Plantation = greenHouse.SaveToStringList();
                save.VipHouse = vipRoom.SaveToString();
                save.PlayHouse = playgroud.SaveToString();

                save.BubbleBoy = BubbleBoyDataSynchronizer.Instance.SaveToString();
                //save.BubbleBoy = bubbleBoy.SaveToString();

                save.TutorialStepTag = Checkers.CheckedTutorialStepTag(TutorialSystem.TutorialController.CurrentStep.StepTag).ToString();

                save.IsArrowAnimationNeededForWhiteElixir = Checkers.CheckedBool(TutorialController.Instance.IsArrowAnimationNeededForWhiteElixir);

                save.NonLinearCompletion = TutorialSystem.TutorialController.GetParsedNonLinearCompletion();
                save.Achievements = UIController.getHospital.AchievementsPopUp.ac.SaveToStringList();
                save.Booster = boosterManager.SaveToString();
                save.Cases = casesManager.SaveToString();
                save.VIPSystem = vipRoom.transform.GetComponent<VIPSystemManager>().SaveToString();
                save.OtherPatients = ReferenceHolder.GetHospital().ClinicAI.GetComponent<SaveOtherHospitalPatients>().SaveToStringList();
                //save.FacebookID = FacebookConnector.Instance.SaveFacebookIDToString();
                save.EpidemyData = epidemy.SaveEpidemyDataToString();
                save.MaternityWardData = maternityWard.SaveToString();
                save.DailyQuest = DailyQuestSynchronizer.Instance.SaveToString();
                save.GlobalEvent = GlobalEventSynchronizer.Instance.SaveToString();

                save.TreatmentHelp = TreatmentRoomHelpSynchronizer.Instance.SaveToString();
                save.DailyDeal = ReferenceHolder.GetHospital().dailyDealController.Save();

                save.LevelGoals = ObjectivesSynchronizer.Instance.SaveToString();
                save.Treasure = treasureManager.Save();
                save.CompletedIAP = IAPController.instance.GetParsedTransactionDictionary();
                //game event chest
                save.lastGameEventChestSpawnTime = gameEventsChestsManager.GetLastSpawnTime();
                save.gameEventChestsCount = gameEventsChestsManager.GetChestsCount();

                save.HospitalCustomizations = HospitalCustomizationSynchronizer.Instance.SaveToString();
                save.ShowEmergencyIndicator = emergency.ShowEmergencyIndicator;
                save.ShowSignIndicator = ReferenceHolder.GetHospital().HospitalNameSign.SignIndicatorVisible;

                save.ShowPaintBadgeClinic = UIController.get.drawer.IsPaintBadgeClinicVisible();
                save.ShowPaintBadgeLab = UIController.get.drawer.IsPaintBadgeLabVisible();
                save.dailyRewardSave = ReferenceHolder.GetHospital().DailyRewardController.SaveToString();
                save.ReputationAmounts = ReputationSystem.ReputationController.Instance.SaveToString();

                Game.Instance.gameState().SetSaveFilePrepared(true);
            }
            catch (SaveImpossibleException saveImpossibleException)
            {
                SaveSynchronizer.LogError(saveImpossibleException);
                GameState.saveFilePrepared = false;
            }
            catch (SaveErrorException saveErrorException)
            {
                SaveSynchronizer.LogError(saveErrorException);
                GameState.saveFilePrepared = false;
            }
            catch (Exception e)
            {
                SaveSynchronizer.LogError(e);
                GameState.saveFilePrepared = false;
                AnalyticsController.instance.ReportException("save_catch", e);
            }
        }

        protected override void AddFakeWall()
        {
            if (FakeWall == null)
            {
                FakeWall = new List<GameObject>();
                for (int i = 0; i < FakeWallPrefab.Count; i++)
                {
                    FakeWall.Add(Instantiate(FakeWallPrefab[i], FakeWallPrefab[i].transform.position + new Vector3(0.05f, -0.1f, 0.2f), FakeWallPrefab[i].transform.rotation) as GameObject);
                }
            }
            for (int i = 0; i < FakeWall.Count; i++)
            {
                if (FakeWall[i].transform.childCount >= 9)
                {
                    AddDoorToMap(FakeWall[0].transform.GetChild(4).GetComponent<Doors>());
                    AddDoorToMap(FakeWall[0].transform.GetChild(5).GetComponent<Doors>());
                    AddDoorToMap(FakeWall[0].transform.GetChild(6).GetComponent<Doors>());
                    AddDoorToMap(FakeWall[0].transform.GetChild(8).GetComponent<Doors>());
                }
            }

            if (FakeWall[0] != null)
            {
                fakeWallVisualUpgrade = FakeWall[0].GetComponent<VisualsUpgradeController>();
            }
        }

        protected void AddFakeWallER()
        {
            if (FakeWallER == null)
                FakeWallER = Instantiate(FakeWallERPrefab, FakeWallERPrefab.transform.position, FakeWallERPrefab.transform.rotation) as GameObject;

            if (FakeWallER.transform.childCount >= 2)
            {
                AddDoorToMap(FakeWallER.transform.GetChild(1).GetComponent<Doors>());
                AddDoorToMap(FakeWallER.transform.GetChild(2).GetComponent<Doors>());
            }
        }

        public override void SaveGameInVisigingMode(Save save)
        {
            GameState.saveFilePrepared = false;

            try
            {
                GameState.Get().SaveData(save, true);

                StepTag stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
                if (stepTagFromSaveFile == StepTag.wise_2)
                {
                    save.TutorialStepTag = (StepTag.emma_about_wise).ToString();
                }

                save.DailyQuest = DailyQuestSynchronizer.Instance.SaveToString();
                save.TreatmentHelp = TreatmentRoomHelpSynchronizer.Instance.SaveToString();
                save.Treasure = treasureManager.Save();
                GameState.saveFilePrepared = true;
            }
            catch (SaveImpossibleException saveImpossibleException)
            {
                SaveSynchronizer.LogError(saveImpossibleException);
                GameState.saveFilePrepared = false;
            }
            catch (SaveErrorException saveErrorException)
            {
                SaveSynchronizer.LogError(saveErrorException);
                GameState.saveFilePrepared = false;
            }
            catch (Exception e)
            {
                SaveSynchronizer.LogError(e);
                GameState.saveFilePrepared = false;
            }
        }

        #endregion

        #region area manipulation
        public Rectangle GetReceptionArea()
        {
            return receptionArea;
        }

        public bool isPositionOnPatio(Vector2i pos)
        {
            if ((pos.x >= wanderingArea.x && pos.x <= (wanderingArea.x + wanderingArea.xSize)) && (pos.y >= wanderingArea.y && pos.y <= (wanderingArea.y + wanderingArea.ySize)))
                return true;

            return false;

        }
        #endregion

        public override void IsoDestroy()
        {
            base.IsoDestroy();
            if (hospitalBedController != null)
                hospitalBedController.ClearAllBedController();
            pharmacy.IsoDestroy();
            reception.IsoDestroy();
            emergency.IsoDestroy();
            playgroud.IsoDestroy();
            greenHouse.IsoDestroy();
            vipRoom.IsoDestroy();
            ReferenceHolder.GetHospital().HospitalNameSign.IsoDestroy();


            DestroyPatients();
            allDecorationsExceptLab.Clear();
            allProbeTableOnMap.Clear();
        }
        internal override void Initialize()
        {
            base.Initialize();
        }
        #region various IsoEngine
        public Vector2i GetReceptionSpot()
        {
            for (int i = 0; i < 1000; i++)
            {
                Vector2i result = new Vector2i(receptionArea.x + GameState.RandomNumber(0, receptionArea.xSize), receptionArea.y + GameState.RandomNumber(0, receptionArea.ySize));
                //HospitalAreasMapController
                GameObject spotObject = GetObject(result);
                if (spotObject == null)
                {
                    return result;
                }
            }

            Debug.Log("!!!Jest Zero!!!");
            return Vector2i.zero;
        }
        #endregion

        public override Decoration GetAvailableDecoration(out Vector2i pos, out Rotation rot, List<Decoration> decoToConsider = null)
        {
            return base.GetAvailableDecoration(out pos, out rot, allDecorationsExceptLab);
        }

        public override void ReloadGame(Save save, bool visitingMode = false, bool reloadGameState = true)
        {
            HomeMapLoaded = false;
            SaveCacheSingleton.CacheSave(save);

            IsoDestroy();

            this.visitingMode = visitingMode;
            SuperObject.visitingMode = VisitingMode;
            ReloadGamestate = reloadGameState;


            if (reloadGameState)
            {
                try { CampaignController.Instance.SetCampaignConfigs(save); }
                catch (Exception e)
                {
                    AnalyticsController.instance.ReportException("campaign.config_failed", e);
                    Debug.LogError(e.Message + " : " + e.StackTrace);
                }
            }

            SaveLoadController.Get().LoadGame(save);

            StartGame();

            if (!visitingMode)
            {
                HomeMapLoaded = true;
            }

            ReferenceHolder.Get().engine.MainCamera.ResetCamera();

            if (HospitalUIPrefabController.Instance.isHidden)
            {
                HospitalUIPrefabController.Instance.ShowMainUI();
            }
        }

        protected override void AssignRewardToPlayerForExpansion(int expReward, bool IsForLab)
        {
            if (IsForLab)
                Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.ExpandLab, false);
            else
                GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.ExpandHospital, false);
        }

        protected override void OnDataLoaded()
        {
        }

        protected override void InitializePathTypes()
        {
            pathCheckNames = new PathType[] { PathType.GoEmergencyPath, PathType.GoPatioPath, PathType.GoHomePath };
        }

        protected override bool CanMakePathFromPositionToOtherDoors(Vector2i doorPos)
        {
            List<Vector2i> allDoorsPosition = Levels[0].GetAllDoorObjectOfArea(HospitalArea.Clinic);
            if (allDoorsPosition != null && allDoorsPosition.Count > 0)
            {
                for (int i = 0; i < allDoorsPosition.Count; i++)
                {
                    if (allDoorsPosition[i] != doorPos)
                    {
                        var makePathMode = CanMakePathFromTo(doorPos, allDoorsPosition[i]);
                        if (makePathMode == -1)
                        {
                            MessageController.instance.ShowMessageWithoutStacking(21);
                            return false;
                        }
                        else if (makePathMode == 0) // door outside fix
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }


        public override HospitalArea GetAreaTypeFromPosition(Vector2i pos)
        {
            if (areas != null && areas.Count > 0)
            {
                if (areas[HospitalArea.Clinic].ContainsPoint(pos))
                    return HospitalArea.Clinic;
                else if (areas[HospitalArea.Laboratory].ContainsPoint(pos))
                    return HospitalArea.Laboratory;
                else if (areas[HospitalArea.Patio].ContainsPoint(pos))
                    return HospitalArea.Patio;
                else return HospitalArea.Ignore;
            }
            else return HospitalArea.Ignore;
        }

        protected override void MapRotatableData(RotatableObject go)
        {
            switch (go.area)
            {
                case HospitalArea.MaternityWardClinic:
                    go.area = HospitalArea.Clinic;
                    break;
                case HospitalArea.MaternityWardPatio:
                    go.area = HospitalArea.Patio;
                    break;
                default:
                    break;
            }
        }


        protected override void MapPrefrefabData(IsoObjectPrefabData prefabData)
        {
            switch (prefabData.area)
            {
                case HospitalArea.MaternityWardClinic:
                    prefabData.area = HospitalArea.Clinic;
                    break;
                case HospitalArea.MaternityWardPatio:
                    prefabData.area = HospitalArea.Patio;
                    break;
                default:
                    break;
            }
        }
    }
}

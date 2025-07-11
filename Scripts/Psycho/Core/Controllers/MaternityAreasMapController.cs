using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using IsoEngine;
using UnityEngine;
using MovementEffects;
using Maternity;

namespace Hospital
{
    public class MaternityAreasMapController : AreaMapController
    {

        #region static

        private static MaternityAreasMapController _maternitymap;
        public static MaternityAreasMapController MaternityMap
        {
            get
            {
                if (_maternitymap == null)
                {
                    Debug.Log("Scene should contain exactly one HospitalAreasMapController if you want size use this feature");
                    //throw new IsoException("There is no mapController on scene!");
                }
                return _maternitymap;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (_maternitymap != null && _maternitymap != this)
            {
                Debug.LogWarning("Multiple instances of generalMap were found!");
            }
            else
                _maternitymap = this;
        }


        #endregion

        private const string DEFAULT_NURSE_ROOM_SAVE = "enabled$0$1";
        private const string DEFAULT_BLOOD_TEST_ROOM_SAVE = "BloodTest$(44,52)/North/working/null";

        public MaternityMapData map
        {
            get;
            private set;
        }
        public NurseRoom nurseRoom;
        public MaternityVitaminMaker maternityVitaminMaker;
        #region fields

        public ElixirStorageModel elixirStorageModel;
        public ElixirTankModel elixirTankModel;

        #endregion

        static public void ClearInstance()
        {
            _maternitymap = null;
        }

        private void AddDefaults(Save save)
        {
            if (save.MaternityClinicObjectsData.Count == 0)
            {
                save.MaternityClinicObjectsData = new List<string>()
                {
                    DEFAULT_BLOOD_TEST_ROOM_SAVE
                };
            }
            if (String.IsNullOrEmpty(save.NurseRoom))
            {
                save.NurseRoom = DEFAULT_NURSE_ROOM_SAVE;
            }
        }

        protected override void ReconstructFromMapData()
        {
            if (!Reconstructed && _shouldReconstruct)
            {
                GetDecorationFromDataBase();
                var save = SaveLoadController.SaveState;
                AddDefaults(save);
                if (save.Level == 0)
                {
                    AnalyticsController.instance.ReportBug("reconstruct_level_zero_1");
                    BaseUIController.ShowCriticalProblemPopup(this);
                    return;
                }
                MaternityPatientsHolder.Instance.SetUnparsedPatients(save.MaternityPatients);
                ReferenceHolder.Get().lootBoxManager.OnMapDestroy();
                UIController.get.preloaderView.Exit();
                //int time = (int)((long)ServerTime.getTime() - save.MaternitySaveDateTime);
                timePassed = new MaternityTimePassedObject(save.saveDateTime, save.MaternitySaveDateTime);
                MaternityDataHolder.Instance.Reset();
                ReferenceHolder.Get().floorControllable.Reset();
                if (ReloadGamestate)
                {
                    VisitingController.Instance.canVisit = true;
                    Game.Instance.gameState().LoadData(save);
                }
                else
                {
                    Game.Instance.gameState().LoadDrawerData(save);
                }
                ReconstructAreas(save);
                elixirStorageModel.InitFromSave(save, timePassed);
                elixirTankModel.InitFromSave(save, timePassed);
                ReconstructStaticWalls();
                if (VisitingMode)
                {
                    TutorialController.Instance.StopTutorialCameraFollowingForSpecialObjects();
                }
                UIController.get.drawer.SetPaintBadgeLabVisible(save.ShowPaintBadgeMaternityClinic);
                boosterManager.LoadFromString(save.Booster, VisitingMode);
                RotatableObject.visitingMode = VisitingMode;

                playgroud.LoadFromString(save.PlayHouse, timePassed);

                if (!VisitingMode)
                {
                    MaternityGameState.Get().maternityPatientsCount.Load(save.AdvancedPatientCounter);
                }

                ReconstructObjects(save, timePassed);

                CountDecorationsFromSecondMap(save.PatioObjectsData);
                CountDecorationsFromSecondMap(save.ClinicObjectsData);
                UIController.get.drawer.UpdatePrices();

                TutorialUIController.Instance.HideIndicator();
                //TutorialUIController.Instance.tutorialArrowUI.Hide();
                if (!VisitingMode && TutorialController.Instance.tutorialEnabled)
                {
                    TutorialController.Instance.LoadNonLinearStepsCompletion(save.NonLinearCompletion);
                    TutorialController.Instance.SubscribeNonLinearSteps();
                    StepTag stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.MaternityTutorialStepTag);
                    TutorialController.Instance.SetStep(stepTagFromSaveFile);
                }
                casesManager.LoadFromString(save.Cases, (timePassed), VisitingMode);

                if (updateDoorInteractionCorountine == null)
                {
                    updateDoorInteractionCorountine = Timing.RunCoroutine(UpdateDoorInteractionCorountine());
                }

                reconstructed = true;
                _shouldReconstruct = false;

                if (!VisitingMode)
                {
                    Debug.LogError("OLD DRAWER CODE!");

                    var drawer = UIController.get.drawer as MaternityShopDrawer;
                    if (drawer != null) drawer.SetDrawerItems();
                }

                if (VisitingMode)
                {
                    UIController.get.SetVisitingUI(save.HospitalName, save.Level, save.ID);
                }
                else
                {
                    UIController.get.SetStandardUI();
                }
                if (UIController.get.PoPUpArtsFromResources != null && UIController.get.PoPUpArtsFromResources.Count > 0)
                {
                    foreach (KeyValuePair<string, GameObject> artGameObject in UIController.get.PoPUpArtsFromResources)
                    {
                        Destroy(artGameObject.Value);
                    }
                    Resources.UnloadUnusedAssets();
                }
                HospitalCustomizationSynchronizer.Instance.LoadFromString(save.MaternityCustomization);
                wanderingArea = new Rectangle(43, 37, 6, 12); //matward trzeba bedzie sie zastanowic nad tym
                ReferenceHolder.GetMaternity().MaternityAI.GetComponent<IPatientSaver>().LoadFromStringList(save.OtherPatients);
                AddFakeWall();
                Game.Instance.gameState().CheckIfLevelUP();
                Game.Instance.gameState().RefreshXPBar();
                if (!VisitingMode)
                {
                    Game.Instance.gameState().CheckExpDependantTutorial(new BaseNotificationEventArgs());
                    UIController.get.reportPopup.timeFromSave = (int)timePassed.GetTimePassed();
                    IAPController.instance.LoadTransactions(save.CompletedIAP);
                }

                UIController.get.LootBoxButtonUI.Initialize();
                ReferenceHolder.Get().lootBoxManager.OnMapLoaded(VisitingMode);
                nurseRoom.LoadFromString(save.NurseRoom, timePassed);
                maternityVitaminMaker.LoadFromString(save.LaboratoryObjectsData, timePassed);
                UIController.getMaternity.vitaminMakerButton.InitializeButton(save.LaboratoryObjectsData);
                ReferenceHolder.Get().IAPShopController.Initialize();
                UIController.get.drawerButton.SetActive(true);
                base.ReconstructFromMapData();
            }
        }



        protected override void OnDataLoaded()
        {
            SampleView.Instance.HospitalName.text = MaternityGameState.Get().HospitalName;
            SampleView.Instance.SampleValueInput.text = MaternityGameState.Get().SampleVariable;
            //SampleView.Instance.CoinsText.text = player.Coins.ToString();
            SampleView.Instance.DiamondsText.text = MaternityGameState.Get().Diamonds.ToString();
            SampleView.Instance.AddSampleValueChangedListener();

            try
            {
                IAPController.instance.Initialize();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        public override void SaveGame(Save save)
        {
            base.SaveGame(save);
            try
            {
                save.MaternitySaveDateTime = (long)ServerTime.getTime();
                SaveRotatablesAndBoughtAreas(save);
                save.MaternityTutorialStepTag = Checkers.CheckedTutorialStepTag(TutorialController.Instance.CurrentTutorialStepTag, true).ToString();
                save.MaternityPatients = SavePatients();
                MaternityGameState.Get().SaveData(save);
                save.Booster = boosterManager.SaveToString();
                //save.MaternityCustomization = HospitalCustomizationSynchronizer.Instance.SaveToString(); //matward for now we wont save maternity floor customization
                Game.Instance.gameState().SetSaveFilePrepared(true);
                save.NurseRoom = nurseRoom.SaveToString();
                save.LaboratoryObjectsData = maternityVitaminMaker.VitaminMakerDataInsertionToExistingLabObjects(save.LaboratoryObjectsData);
                //MaternityMap.elixirStorageModel.Save(save);
                //MaternityMap.elixirTankModel.Save(save);
                save.LaboratoryObjectsData = MaternityMap.SaveElixirModels(save.LaboratoryObjectsData);
            }
            catch (Exception ex)
            {
                SaveSynchronizer.LogError(ex);
                Game.Instance.gameState().SetSaveFilePrepared(false);
            }
        }

        private List<string> SaveElixirModels(List<string> laboratoryObjectsData)
        {
            List<string> result = SaveElixirStorageModel(laboratoryObjectsData);
            return SaveElixirTankModel(result);
        }

        private List<string> SaveElixirStorageModel(List<string> result)
        {
            int index = result.FindIndex((obj) => ElixirStorageModel.IsElixirStorage(obj));

            if (index >= 0)
            {
                result[index] = MaternityMap.elixirStorageModel.SaveObject();
            }

            return result;
        }

        private List<string> SaveElixirTankModel(List<string> result)
        {
            int index = result.FindIndex((obj) => ElixirTankModel.IsElixirTank(obj));

            if (index >= 0)
            {
                result[index] = MaternityMap.elixirTankModel.SaveObject();
            }

            return result;
        }

        private void SaveRotatablesAndBoughtAreas(Save save)
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
                Debug.LogError("Problems");
                //AnalyticsController.instance.ReportBug("savebeforeload", Game.Instance.gameState().GetHospitalLevel(), VisitingController.Instance.IsVisiting);
                //throw new IsoException("Game want to save before it's loaded!");
            }
        }

        private List<string> SavePatients()
        {
            List<string> result = new List<string>();

            foreach (MaternityPatientAI patient in MaternityPatientsHolder.Instance.GetPatientsList())
            {
                //save.MaternityPatients.Add(patient.SaveToString());
                result.Add(patient.SaveToString());
            }

            return result;
        }

        public override void SaveGameInVisigingMode(Save save)
        {

        }

        protected override void AddFakeWall()
        {
            if (FakeWall == null)
            {
                FakeWall = new List<GameObject>();
                for (int i = 0; i < FakeWallPrefab.Count; i++)
                {
                    FakeWall.Add(Instantiate(FakeWallPrefab[i], FakeWallPrefab[i].transform.position, FakeWallPrefab[i].transform.rotation) as GameObject);
                }
            }
            for (int i = 0; i < FakeWall.Count; i++)
            {
                Doors[] doors = FakeWall[i].GetComponentsInChildren<Doors>(true);
                if (doors != null && doors.Length > 0)
                {
                    for (int j = 0; j < doors.Length; j++)
                    {
                        AddDoorToMap(doors[j]);
                    }
                }
            }
        }

        public override bool AddDecorationToMap(Decoration currentDecoration)
        {
            if (allDecorationsOnMap == null)
            {
                allDecorationsOnMap = new List<Decoration>();
            }

            if (decoAmountMap.Count == 0)
            {
                for (int i = 0; i < _drawerDatabase.DrawerItems.Count; i++)
                {
                    if (_drawerDatabase.DrawerItems[i] is DecorationInfo)
                    {
                        decoAmountMap.Add(_drawerDatabase.DrawerItems[i].Tag, 0);
                    }
                }
            }

            if (!allDecorationsOnMap.Contains(currentDecoration))
            {
                allDecorationsOnMap.Add(currentDecoration);
                decoAmountMap[currentDecoration.Tag]++;
                return true;
            }
            return false;
        }

        public override int CanMakePathTo(Vector2i pos, PathType[] doorPathTypes, bool isDeco = false)
        {
            try
            {
                int possiblePathCount = 0;

                Vector2i spawn;
                for (int i = 0; i < doorPathTypes.Length; i++)
                {
                    if (doorPathTypes[i] == PathType.GoHomePath)
                        spawn = new Vector2i(53, 56);
                    else if (doorPathTypes[i] == PathType.GoPatioPath)
                        spawn = new Vector2i(43, 50);
                    else spawn = new Vector2i(53, 65);

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
                        possiblePathCount++;
                    }
                }

                if (possiblePathCount == doorPathTypes.Length)
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
            if (areas[HospitalArea.MaternityWardClinic].ContainsPoint(pos))
                return true;
            else return false;
        }

        public override bool IsPosInsidePatio(Vector2i pos)
        {
            if (areas[HospitalArea.MaternityWardPatio].ContainsPoint(pos))
                return true;
            else return false;
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
        }

        protected override void ReconstructObjects(Save save, TimePassedObject timePassed)
        {
            areas[HospitalArea.MaternityWardClinic].LoadObjectsFromSave(save.MaternityClinicObjectsData, timePassed);
            areas[HospitalArea.MaternityWardPatio].LoadObjectsFromSave(save.MaternityPatioObjectsData, timePassed);
        }

        public override void EmulateMapObjectTime(TimePassedObject timePassed)
        {
            //matward upewnic sie ze jest ok
            areas[HospitalArea.MaternityWardClinic].Notify((int)LoadNotification.EmulateTime, timePassed);
            maternityVitaminMaker.EmulateTime(timePassed);

            base.EmulateMapObjectTime(timePassed);
        }

        protected void ReconstructAreas(Save save)
        {
            areas = new Dictionary<HospitalArea, GameArea>();

            if (mapConfig.MaternityWardClinic != null)
            {
                var maternityWardClinic = new GameArea(HospitalArea.MaternityWardClinic, this);
                ReconstructArea(maternityWardClinic, mapConfig.MaternityWardClinic, save.UnlockedMaternityWardClinicAreas);
                areas.Add(HospitalArea.MaternityWardClinic, maternityWardClinic);
            }
            if (mapConfig.MaternityWardPatio != null)
            {
                var patio = new GameArea(HospitalArea.MaternityWardPatio, this);
                ReconstructArea(patio, mapConfig.MaternityWardPatio, new List<int>() { 0 });
                areas.Add(HospitalArea.MaternityWardPatio, patio);
            }
        }

        public override Decoration GetAvailableDecoration(out Vector2i pos, out Rotation rot, List<Decoration> decoToConsider = null)
        {
            return base.GetAvailableDecoration(out pos, out rot, allDecorationsOnMap);
        }

        public override void ReloadGame(Save save, bool visitingMode = false, bool reloadGameState = true)
        {
            HomeMapLoaded = false;

            SaveCacheSingleton.CacheSave(save);

            IsoDestroy();
            //matward however you need to do some destroy when returning from a visiting maternity(Translated from Polish)

            this.visitingMode = visitingMode;
            SuperObject.visitingMode = VisitingMode;
            ReloadGamestate = reloadGameState;


            if (reloadGameState)
            {
                try { CampaignController.Instance.SetCampaignConfigs(save); }
                catch (Exception e)
                {
                    AnalyticsController.instance.ReportException("campaign.config_failed", e);
                    Debug.LogError(e.Message);
                }
            }

            SaveLoadController.Get().LoadGame(save);

            StartGame();

            if (!visitingMode)
            {
                HomeMapLoaded = true;
            }

            ReferenceHolder.Get().engine.MainCamera.ResetMaternityCamera();

            if (HospitalUIPrefabController.Instance.isHidden)
            {
                HospitalUIPrefabController.Instance.ShowMainUI();
            }
        }

        public override void IsoDestroy()
        {
            nurseRoom.IsoDestroy();

            if (areas != null)
                foreach (var p in areas)
                    p.Value.IsoDestroy();

            if (updateDoorInteractionCorountine != null)
            {
                Timing.KillCoroutine(updateDoorInteractionCorountine);
                updateDoorInteractionCorountine = null;
            }

            areas = null;
            reconstructed = false;
            PFISODestroy();

            allDoorsOnMap.Clear();
            allDecorationsOnMap.Clear();

            SaveCacheSingleton.UnlinkFromBaseGameStateEvent();
        }

        protected override void AssignRewardToPlayerForExpansion(int expReward, bool IsForLab)
        {
            if (!IsForLab)
                Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.ExpandMaternityClinic, false);
        }

        protected override void InitializePathTypes()
        {
            pathCheckNames = new PathType[] { PathType.GoPatioPath, PathType.GoHomePath };
        }

        protected override bool CanMakePathFromPositionToOtherDoors(Vector2i doorPos)
        {
            List<Vector2i> allDoorsPosition = Levels[0].GetAllDoorObjectOfArea(HospitalArea.MaternityWardClinic);
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
                if (areas[HospitalArea.MaternityWardClinic].ContainsPoint(pos))
                    return HospitalArea.MaternityWardClinic;
                else if (areas[HospitalArea.MaternityWardPatio].ContainsPoint(pos))
                    return HospitalArea.MaternityWardPatio;
                else return HospitalArea.Ignore;
            }
            else return HospitalArea.Ignore;
        }

        protected override Vector2i GetDoorsPositionMapDependent()
        {
            return Levels[0].GetAnyDoorOfType(HospitalArea.MaternityWardClinic);
        }

        protected override void MapPrefrefabData(IsoObjectPrefabData prefabData)
        {
            switch (prefabData.area)
            {
                case HospitalArea.Clinic:
                    prefabData.area = HospitalArea.MaternityWardClinic;
                    break;
                case HospitalArea.Patio:
                    prefabData.area = HospitalArea.MaternityWardPatio;
                    break;
                default:
                    break;
            }
        }

        protected override void MapRotatableData(RotatableObject go)
        {
            switch (go.area)
            {
                case HospitalArea.Clinic:
                    go.area = HospitalArea.MaternityWardClinic;
                    break;
                case HospitalArea.Patio:
                    go.area = HospitalArea.MaternityWardPatio;
                    break;
                default:
                    break;
            }
        }

        protected override bool RequiredLevelTooLow()
        {
            return Game.Instance.gameState().GetMaternityLevel() < 2;
        }

        protected override void ShowMessage()
        {
            MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), "2"));
        }

    }
}
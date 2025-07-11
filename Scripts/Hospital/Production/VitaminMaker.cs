using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using IsoEngine;
using System.Text;

namespace Hospital
{
    public class VitaminMaker : RotatableObject, IProductables, MasterablePropertiesClient
    {
        public List<VitaminCollectorModel> vitaminModels = new List<VitaminCollectorModel>();
        public int ProducedMedicines;

        private VitaminDropAnimationController plusOneDropController;
        private Animator machineAnimator;
        private VitaminMakerCapacityVisualizerController visualizerController;
        private int unlockedCollectors = 0;

        private const char SEPARATOR_VITAMIN_MAKER_DATA = '!';
        private const char SEPARATOR_VITAMIN_COLLECTORS = '#';

        protected override void OnClickWorking()
        {
            UIController.getHospital.HospitalInfoPopUp.OpenVitaminsInfo(GetRoomInfo());
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            if (Anchored && state == State.building)
                SetAlertSignMedicine();
        }

        void SetAlertSignMedicine()
        {
            try
            {
                Sprite sprite = ((MedicineProductionMachineInfo)info.infos).FirstMedicineSprite;
                isoObj.GetGameObject().GetComponent<ProductionMachineMedicine>().SetMedicineIcon(sprite);
            }
            catch
            {
                Debug.LogError("Failed to set medicine icon. If you see this a lot call Mikko");
            }
        }

        #region VitaminProduction

        public int GetSecondsToFullfillCollector()
        {
            if (state != State.working)
                return -1;
            int secondsToFullfillCollector = -1;
            foreach (VitaminCollectorModel model in vitaminModels)
            {
                int secondsToFullfillSpecificCollector = model.GetSecondsToFullfillCollector();
                if (secondsToFullfillSpecificCollector > secondsToFullfillCollector)
                {
                    secondsToFullfillCollector = secondsToFullfillSpecificCollector;
                }
            }
            return secondsToFullfillCollector;
        }

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            SetModels();
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            SetupMasterableProperties();
        }

        protected override void AddToMap()
        {
            base.AddToMap();
            GetAnimator();
            GetVisualizerController();
            SetupVitaminmaker();
            SubscribeToCollectorEvents();
            CheckUnlockedCollectorsAmount();
            if (machineAnimator != null)
            {
                machineAnimator.SetBool("IsOnMap", true);
            }
            ToggleMachineAnimation(!IsCollectorFull());
            UnlockLights();
            RefreshFillVizualizations();
            if (masterableProperties != null)
            {
                masterableProperties.RefreshMasteryView(false);
            }
        }

        private void GetVisualizerController()
        {
            if (this != null && isoObj != null)
            {
                var p = isoObj.GetGameObject();
                if (p.gameObject.GetComponent<VitaminMakerCapacityVisualizerController>() != null)
                {
                    visualizerController = p.gameObject.GetComponent<VitaminMakerCapacityVisualizerController>();
                }
            }
        }

        private void CheckUnlockedCollectorsAmount()
        {
            unlockedCollectors = 0;
            for (int i = 0; i < vitaminModels.Count; i++)
            {
                if (GameState.Get().hospitalLevel >= vitaminModels[i].GetVitaminCollectorUnlockLevel())
                {
                    unlockedCollectors++;
                }
            }
        }

        private void RefreshFillVizualizations()
        {
            for (int i = 0; i < vitaminModels.Count; i++)
            {
                if (GameState.Get().hospitalLevel >= vitaminModels[i].GetVitaminCollectorUnlockLevel() || IsWise())
                {
                    SetVisutalIndicatorBar(i, vitaminModels[i].capacity, vitaminModels[i].maxCapacity);
                    SetVisutalIndicatorLight(i, vitaminModels[i].capacity >= vitaminModels[i].maxCapacity);
                }
            }
        }

        public static bool IsWise()
        {
            return SaveLoadController.SaveState.ID.Equals(SaveLoadController.WISE);
        }

        private void GetAnimator()
        {
            if (this != null && isoObj != null)
            {
                var p = isoObj.GetGameObject();
                if (p.transform.GetChild(0).gameObject.GetComponent<Animator>() != null)
                {
                    machineAnimator = p.transform.GetChild(0).gameObject.GetComponent<Animator>();
                }
            }
        }

        protected override void OnClickWaitForUser()
        {
            base.OnClickWaitForUser();
            SetupVitaminmaker();
            SubscribeToCollectorEvents();
            CheckUnlockedCollectorsAmount();
            if (machineAnimator != null)
            {
                machineAnimator.SetBool("IsOnMap", true);
            }
            ToggleMachineAnimation(!IsCollectorFull());
            UnlockLights();
        }

        private void SetupVitaminmaker()
        {
            UIController.getHospital.HospitalInfoPopUp.SetupVitaminView(vitaminModels, this);
            HospitalDataHolder.Instance.BuiltProductionMachines.Add(this);
            plusOneDropController = ReferenceHolder.Get().plusOneVitaminMaker.GetComponent<VitaminDropAnimationController>();
        }

        private void SetModels()
        {
            foreach (MedicineDatabaseEntry med in ResourcesHolder.Get().GetAllMedicinesOfType(MedicineType.Vitamins))
            {
                vitaminModels.Add(new VitaminCollectorModel(med.GetMedicineRef()));
            }
        }

        private void SubscribeToCollectorEvents()
        {
            UnSubscribeToCollectorEvents();
            foreach (VitaminCollectorModel vitaminModels in vitaminModels)
            {
                vitaminModels.capacityChanged += VitaminModels_capacityChanged;
                vitaminModels.vitaminCollected += VitaminModels_vitaminCollected;
            }
            GameState.OnLevelUp += GameState_OnLevelUp;
        }

        private void GameState_OnLevelUp()
        {
            UnlockLights();
            RefreshFillVizualizations();
            CheckUnlockedCollectorsAmount();
        }

        private void UnlockLights()
        {
            if (visualizerController != null)
            {
                for (int i = 0; i < vitaminModels.Count; i++)
                {
                    if (GameState.Get().hospitalLevel >= vitaminModels[i].GetVitaminCollectorUnlockLevel() || IsWise())
                    {
                        visualizerController.UnlockMachine(vitaminModels[i].med.id);
                    }
                }
            }
        }

        private void UnSubscribeToCollectorEvents()
        {
            foreach (VitaminCollectorModel vitaminModels in vitaminModels)
            {
                vitaminModels.capacityChanged -= VitaminModels_capacityChanged;
                vitaminModels.vitaminCollected -= VitaminModels_vitaminCollected;
            }
            GameState.OnLevelUp -= GameState_OnLevelUp;
        }

        private void ToggleMachineAnimation(bool isWorking)
        {
            if (machineAnimator != null)
            {
                machineAnimator.SetBool("IsWorking", isWorking);
            }
        }

        private void SetVisutalIndicatorBar(int vitaminIndex, float currentCapcity, int MaxCapacity)
        {
            if (visualizerController != null)
            {
                int levelOfFill = 0;
                if (currentCapcity >= 1)
                {
                    levelOfFill = 1;
                    float fillRatio = currentCapcity / MaxCapacity;
                    if (fillRatio >= 1)
                    {
                        levelOfFill = 3;
                    }
                    else if (fillRatio >= 0.66)
                    {
                        levelOfFill = 2;
                    }
                }
                visualizerController.ToggleCapacityIndicator(vitaminIndex, levelOfFill);
            }
        }

        private void VitaminModels_vitaminCollected(int amount, MedicineRef vitamin)
        {
            ToggleMachineAnimation(!IsCollectorFull());
            SetVisutalIndicatorLight(vitamin.id, false);
        }

        private void VitaminModels_capacityChanged(float fill, float current, int max, int producedAmount, MedicineRef vitamin, int timeToDrop, VitaminCollectorModel.VitaminSource source)
        {
            if (producedAmount > 0)
            {
                ProducedMedicines += producedAmount;
                masterableProperties.AddMasteryProgress(producedAmount);
                plusOneDropController.ShowAnimation(gameObject.transform.position, vitamin, producedAmount, unlockedCollectors);
                ToggleMachineAnimation(!IsCollectorFull());
                SetVisutalIndicatorLight(vitamin.id, current >= max);
            }
            SetVisutalIndicatorBar(vitamin.id, current, max);
        }

        public bool IsCollectorFull()
        {
            for (int i = 0; i < vitaminModels.Count; i++)
            {
                if (vitaminModels[i].capacity < vitaminModels[i].maxCapacity && GameState.Get().hospitalLevel >= vitaminModels[i].GetVitaminCollectorUnlockLevel())
                {
                    return false;
                }
            }
            return true;
        }

        #endregion

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            for (int i = 0; i < vitaminModels.Count; ++i)
            {
                vitaminModels[i].Update(Time.deltaTime);
            }
        }

        #region SaveLoad

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            base.LoadFromString(save, timePassed, actionsDone);
            SetupMasterableProperties();


            UnSubscribeToCollectorEvents();

            var str = save.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (str.Length > 1)
            {
                string[] vitaminMakerData = str[1].Split(new char[] { SEPARATOR_VITAMIN_MAKER_DATA }, StringSplitOptions.RemoveEmptyEntries);
                string[] VitaminCollectors = vitaminMakerData[0].Split(new char[] { SEPARATOR_VITAMIN_COLLECTORS }, StringSplitOptions.RemoveEmptyEntries);

                int ProducedMedicineFromSave = 0;
                int.TryParse(vitaminMakerData[1], out ProducedMedicineFromSave);
                ProducedMedicines = ProducedMedicineFromSave;
                masterableProperties.LoadFromString(save, ProducedMedicines);
                for (int i = 0; i < VitaminCollectors.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(VitaminCollectors[i]))
                    {
                        VitaminCollectorModel model = new VitaminCollectorModel();

                        model.capacityChanged += VitaminModels_capacityChanged;
                        model.vitaminCollected += VitaminModels_vitaminCollected;

                        model.LoadFromString(VitaminCollectors[i], timePassed, masterableProperties);
                        vitaminModels.Add(model);
                    }
                }
            }
            GameState.OnLevelUp += GameState_OnLevelUp;
            EmulateTime(timePassed);
            SetupVitaminmaker();
            GetAnimator();
            GetVisualizerController();
            ReferenceHolder.Get().engine.AddTask(() =>
            {
                if (machineAnimator != null)
                    machineAnimator.SetBool("IsOnMap", true);
                ToggleMachineAnimation(!IsCollectorFull());
                UnlockLights();
                RefreshFillVizualizations();
            });
            // SubscribeToCollectorEvents();
            CheckUnlockedCollectorsAmount();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
            {
                TimePassedObject timePassed = (TimePassedObject)parameters;
                EmulateTime(timePassed);
                if (vitaminModels != null)
                {
                    foreach (VitaminCollectorModel model in vitaminModels)
                    {
                        model.Update(timePassed);
                    }
                }
            }
        }

        protected override void SetupMasterableProperties()
        {
            if (masterableProperties != null)
            {
                return;
            }
            masterableProperties = new VitaminMakerMasterableProperties(this);
        }

        public void SetVisutalIndicatorLight(int vitaminID, bool isFull)
        {
            if (visualizerController != null)
            {
                visualizerController.SetFullMachineLight(vitaminID, isFull);
            }
        }

        protected override string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.SaveToString());
            builder.Append(";");
            foreach (VitaminCollectorModel model in vitaminModels)
            {
                builder.Append(model.SaveToString());
                builder.Append(SEPARATOR_VITAMIN_COLLECTORS);
            }
            builder.Append(SEPARATOR_VITAMIN_MAKER_DATA);
            builder.Append(ProducedMedicines);
            builder.Append(";");
            builder.Append(masterableProperties.SaveToStringMastership());
            return builder.ToString();
        }

        public override void IsoDestroy()
        {
            UnSubscribeToCollectorEvents();
            base.IsoDestroy();
        }
        #endregion

        #region Test

        public void TestCollectFirstVitamin()
        {
            if (vitaminModels.Count == 0)
                return;
            int amount = vitaminModels[0].Collect();
            if (amount > 0)
            {
                Debug.LogError("Success collected vitamin: " + amount);
            }
        }

        public void TestUpgradeFirstVitaminCollector()
        {
            if (vitaminModels.Count == 0)
                return;
            vitaminModels[0].Upgrade();
            Debug.LogError("Success Upgrade first vitamin");
        }

        #endregion

        public int GetProducedMedicineAmount()
        {
            return ProducedMedicines;
        }

        public int GetMasteryLevel()
        {
            return masterableProperties.MasteryLevel;
        }

        public ShopRoomInfo GetShopRoomInfo()
        {
            return GetRoomInfo();
        }

        Rotations IProductables.GetInfo()
        {
            return info;
        }

        public void SetInfoShowed(bool isShowed)
        {
        }

        public State GetMachineState()
        {
            return state;
        }

        public MasterableConfigData GetMasterableConfigData()
        {
            return masterableProperties.MasterableConfigData;
        }

        public Vector2i GetPosition()
        {
            return position;
        }

        public void ShowMachineHoover()
        {
        }

        public string GetTag()
        {
            return Tag;
        }

        public string GetClientTag()
        {
            if (info != null && info.infos != null)
            {
                return info.infos.Tag;
            }
            return string.Empty;
        }
    }
}

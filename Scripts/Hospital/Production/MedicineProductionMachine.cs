using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using IsoEngine;
using SimpleUI;
using MovementEffects;
using System.Globalization;

namespace Hospital
{
    public class MedicineProductionMachine : RotatableObject, IHoverable, IProductables, MasterablePropertiesClient
    {
        public LinkedList<MedicineRef> productionQueue;
        public MedicineRef actualMedicine;
        Animator machineAnim;
        //private bool boughtWithDiamonds = false;
        [SerializeField] private int maxQueue = 9;

        private float actualMedicineProductionTime;

        public float ActualMedicineProductionTime
        {
            get { return actualMedicineProductionTime; }
            private set { actualMedicineProductionTime = value; }
        }

        public MedicineType productedMedicines
        {
            get;
            private set;
        }

        public bool ShouldWork
        {
            get;
            private set;
        }

        public MedicineRef GetActualMedicine()
        {
            return actualMedicine;
        }

        private int queueSize = 2;
        public int QueueSize
        {
            get { return queueSize; }
            private set
            {
                if (value < queueSize)
                    throw new IsoException("you cannot make queue smaller!");
                if (hover != null)
                {
                    if (value <= maxQueue)
                        hover.EnlargeQueue(value, true);
                    if (value == maxQueue)
                        hover.SetBuyingEnable(false);
                }
                queueSize = value;
            }
        }

        public int ProducedMedicines;
        public bool infoShowed = false;
        MedicineProductionHover hover = null;

        bool speedUpUsed;

        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;
        private bool firstBuilded = true;

        private bool isUnwraping = false;
        private bool isHintUpdated = false;

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            var p = (MedicineProductionMachineInfo)info.infos;

            productionQueue = new LinkedList<MedicineRef>();
            productedMedicines = p.productedMedicine;
            if (p.Tag == "ElixirLab")
                QueueSize = 3;
            else
                QueueSize = 2;
            infoShowed = false;

            SetupMasterableProperties();
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            if (!HospitalDataHolder.Instance.BuiltProductionMachines.Contains(this))
                HospitalDataHolder.Instance.BuiltProductionMachines.Add(this);

            //print(str);
            var p = (MedicineProductionMachineInfo)info.infos;
            productionQueue = new LinkedList<MedicineRef>();
            productedMedicines = p.productedMedicine;

            var str = save.Split(';');

            List<string> strs = null;
            if (str.Length > 1)
                strs = str[1].Split('/').ToList();
            else
                return;

            var num = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);

            if (strs.Count > 3 + num)
                ProducedMedicines = int.Parse(strs[3 + num], System.Globalization.CultureInfo.InvariantCulture);
            else
                ProducedMedicines = 0;

            int addition = 0;
            if (info.infos.Tag == "ElixirLab")
            {
#pragma warning disable 0252
                IProductables eliLab = HospitalDataHolder.Instance.BuiltProductionMachines.Find(x => (x != this && x.GetInfo().infos.Tag == "ElixirLab"));
#pragma warning restore 0252
                if (eliLab != null)
                    addition = eliLab.GetProducedMedicineAmount();
            }
            SetupMasterableProperties();
            masterableProperties.LoadFromString(save, ProducedMedicines);
            base.LoadFromString(save, timePassed, ProducedMedicines + addition);

            if (string.Compare(info.infos.Tag, "ElixirLab") == 0)
                SetSameMachineMasteries();

            QueueSize = int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture);
            ActualMedicineProductionTime = float.Parse(strs[1], CultureInfo.InvariantCulture);
            if (num > 0)
            {
                actualMedicine = MedicineRef.Parse(strs[3]);

                if (actualMedicine != null)
                    MedicineBadgeHintsController.Get().AddSingleMedInProduction(actualMedicine, 1);

                ReferenceHolder.Get().engine.AddTask(() =>
                    {
                        if (actualMedicine != null)
                            machineAnim.SetBool("IsWorking", true);
                    });
                for (int i = 1; i < num; ++i)
                {
                    productionQueue.AddLast(MedicineRef.Parse(strs[3 + i]));
                    MedicineBadgeHintsController.Get().AddSingleMedInProduction(MedicineRef.Parse(strs[3 + i]), 1);
                }
            }

            EmulateTime(timePassed);
        }

        protected override void SetupMasterableProperties()
        {
            if (masterableProperties != null)
                return;

            masterableProperties = new MedicineProductionMasterableProperties(this);
        }

        protected override string SaveToString()
        {
            var str = base.SaveToString() + ";" + Checkers.CheckedAmount(QueueSize, 2, maxQueue, Tag) + "/" + Checkers.CheckedAmount(ActualMedicineProductionTime, -1.0f, float.MaxValue, Tag + "actualMedicineProductiontime").ToString("n2") + "/" + (Checkers.CheckedAmount((actualMedicine != null ? 1 : 0) + productionQueue.Count, 0, int.MaxValue, Tag + " productionQueue")).ToString();
            if (actualMedicine != null)
                str += "/" + Checkers.CheckedMedicine(actualMedicine, Tag).ToString();

            for (int i = 0; i < productionQueue.Count; ++i)
            {
                str += "/" + Checkers.CheckedMedicine(productionQueue.ElementAt(i), Tag).ToString();
            }

            str += "/" + ProducedMedicines.ToString();

            return str + ";" + masterableProperties.SaveToStringMastership();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
                EmulateTime((TimePassedObject)parameters);
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            base.EmulateTime(timePassed);
            float time = timePassed.GetTimePassed();
            if (actualMedicine == null)
            {
                ActualMedicineProductionTime = 0;
                return;
            }
            while (time > 0 && actualMedicine != null)
            {
                if (ActualMedicineProductionTime > time)
                {
                    ActualMedicineProductionTime -= time;
                    time = 0;
                }
                else
                {
                    time -= ActualMedicineProductionTime;
                    ActualMedicineProductionTime = 0;
                    StopMakingMedicine();
                    MoveMedicineFromQueueToProduction();
                }
            }
        }

        private static BalanceableFloat percentBonusToProductionAfterAdBalanceable;

        public static float PercentBonusToProductionAfterWatchingAd
        {
            get
            {
                if (percentBonusToProductionAfterAdBalanceable == null)
                    percentBonusToProductionAfterAdBalanceable = BalanceableFactory.CreatePercentToMedicineProductionAfterWatchingAd();

                return percentBonusToProductionAfterAdBalanceable.GetBalancedValue();
            }
        }

        public void AdvanceProductionTimeAfterWatchingAdvertisement()
        {
            ActualMedicineProductionTime *= (1f - PercentBonusToProductionAfterWatchingAd);
            if (actualMedicine != null)
                AnalyticsController.instance.ReportSpeedUpMedicineForAdd(actualMedicine);
        }

        public bool HasFreeSlotsForProduction()
        {
            return productionQueue.Count + 1 < QueueSize;
        }

        protected override void OnClickBuilding()
        {
            Debug.LogWarning("OnClickBuilding");
            base.OnClickBuilding();
            BounceMachineObject(true);
        }

        protected override void OnClickWorking()
        {
            if (UIController.getHospital.drawer.IsVisible || UIController.getHospital.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            Debug.LogWarning("OnClickWorking");
            base.OnClickWorking();
            BounceMachineObject();

            if (collectables != null && collectables.Count > 0)
            {
                //costam tutorial wywoluje
                CollectCollectable();

                if (collectables != null && collectables.Count > 0 && collectables[0].medicine != null)
                {
                    if (collectables[0].medicine.IsMedicineForTankElixir())
                    {
                        if (!GameState.Get().CanAddAmountForTankStorage(1))
                        {
                            if (hover == null)
                            {
                                ShowQueuePopup(false);
                                MoveCameraToMachine(new Vector2(this.position.x, this.position.y));
                            }
                            else
                            {
                                Debug.Log("hover is not null!");
                                MoveCameraToShowHover();
                            }
                        }
                    }
                    else
                    {
                        if (!GameState.Get().CanAddAmountForElixirStorage(1))
                        {
                            if (hover == null)
                            {
                                ShowQueuePopup(false);
                                MoveCameraToMachine(new Vector2(this.position.x, this.position.y));
                            }
                            else
                            {
                                Debug.Log("hover is not null!");
                                MoveCameraToShowHover();
                            }
                        }
                    }
                }
            }
            else
            {
                if (hover == null)
                    ShowQueuePopup();
                //włąściwie w tooltipie dać info że HA i tu gdzie zżera zasoby żeby jak coś nie zżerało
                //"Actually, in the tooltip, provide information that HA is consuming resources here, so that something doesn't consume them unnecessarily." (Translated from Polish)
                else
                    Debug.Log("hover is not null!");
                MoveCameraToShowHover();
            }
        }

        void BounceMachineObject(bool isBeingBuilt = false)
        {
            //Timing.KillCoroutine(BounceCoroutine());
            Timing.RunCoroutine(BounceCoroutine(isBeingBuilt));
        }

        IEnumerator<float> BounceCoroutine(bool isBeingBuilt = false)
        {
            //Debug.Log("BounceCoroutine");
            float bounceTime = .15f;
            float timer = 0f;
            Transform targetTransform;
            if (isBeingBuilt)
                targetTransform = isoObj.GetGameObject().transform.GetChild(0);
            else
            {
                targetTransform = machineAnim.transform.parent;
                if (firstBuilded)
                {
                    firstBuilded = false;
                    normalScale = Vector3.zero;
                }
            }

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            // if (targetScale == Vector3.zero)
            //    targetScale = targetTransform.localScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);
            targetScale = normalScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);

            //scale up
            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
                //scale up
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(normalScale, targetScale, timer / bounceTime);
                    yield return 0;
                }
                timer = 0f;
                //scale down
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(targetScale, normalScale, timer / bounceTime);
                    yield return 0;
                }
            }
            else yield return 0;
        }

        protected override void OnClickWaitForUser()
        {
            if (!isUnwraping)
            {
                Timing.RunCoroutine(DelayedUnwrap());
                foreach (var p in BasePatientAI.patients)
                {
                    if (p != null)
                        p.Notify((int)StateNotifications.DoctorRoomUnpacked);
                }
            }
        }

        private void ShowQueuePopup(bool resetStorage = true)
        {
            //HintsController.Get().TryToHideIndicator();
            SetBorderActive(true);
            HospitalAreasMapController.HospitalMap.ChangeOnTouchType((x) => { CloseQueuePopup(); });
            hover = MedicineProductionHover.Open();
            hover.Initialize(new Vector2(position.x + actualData.rotationPoint.x - 1, position.y + actualData.rotationPoint.y - 1), productionQueue, QueueSize, this);
            if (actualMedicine != null)
                hover.SetSpeedUpCostText(ActualMedicineProductionTime, ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime);
            hover.ActualizeList(actualMedicine);
            if (maxQueue == QueueSize)
                hover.SetBuyingEnable(false);
            else
                hover.SetBuyingEnable(true);
            hover.UpdateAccordingToMode();
            TutorialController tc = TutorialController.Instance;
            if (tc.tutorialEnabled && (tc.GetCurrentStepData().StepTag == StepTag.syrup_production_start || tc.GetCurrentStepData().StepTag == StepTag.syrup_in_production || tc.GetCurrentStepData().StepTag == StepTag.syrup_collect_text))
                TutorialUIController.Instance.HideIndicator();

            if (resetStorage)
                CancelInvoke("ResetStorageFullCounter");
        }

        public void ShowMachineHoover()
        {
            base.OnClickWorking();
            ShowQueuePopup();
        }

        public override RectTransform GetHoverFrame()
        {
            if (hover == null)
                return null;

            return hover.hoverFrame;
        }

        public void CloseQueuePopup(int a = 0)
        {
            SetBorderActive(false);
            MedicineProductionHover medicineProductionHover = MedicineProductionHover.GetActive();
            if (medicineProductionHover != null)
                medicineProductionHover.Close();

            hover = null;

            CancelInvoke("ResetStorageFullCounter");
            Invoke("ResetStorageFullCounter", 1f);
        }

        void ResetStorageFullCounter()
        {
            storageFullCounter = 0;
        }

        public void NullHover()
        {
            //GameState.isHoverOn = false;
            hover = null;
        }

        private void HideQueuePopup()
        {
            //	GameState.isHoverOn = false;
            hover = null;
        }

        public bool IsHoverNull()
        {
            return (hover == null);
        }

        public void AddMedicineToQueue(MedicineRef medicine)
        {
            if (productionQueue.Count >= QueueSize - 1)
            {
                print(6);
                MessageController.instance.ShowMessage(6);
                return;
            }
            //tutaj sprawdzenie czy mamy odpowiedni buster odpalony (check here if we have the right buster fired)
            if (!(HospitalAreasMapController.HospitalMap.boosterManager.boosterActive && ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterType == BoosterType.Action && ResourcesHolder.Get().boosterDatabase.boosters[HospitalAreasMapController.HospitalMap.boosterManager.currentBoosterID].boosterTarget == BoosterTarget.Lab))
            {
                if (!GameState.Get().GetPrerequisitesForMedicine(medicine, this))
                {
                    //MessageController.instance.ShowMessage(7);
                    print("not enough prerequisite");
                    return;
                }
            }

            // HintsController.Get().RemoveMedicineNeededToHeal(medicine, 1);

            if (medicine != null)
            {
                MedicineBadgeHintsController.Get().AddSingleMedInProduction(medicine, 1);
                //HintsController.Get().UpdateAllHintsWithMedicineCount();
            }

            productionQueue.AddLast(medicine);
            BounceMachineObject();
            SoundsController.Instance.PlayCureInSlot();
            if (hover != null && actualMedicine != null)
            {
                UpdateHooverItemForMedicine();
                hover.ActualizeList(actualMedicine);
            }

            if (MedicineProductionHover.tutorialMode && medicine.type == MedicineType.Syrop && medicine.id == 1)            
                NotificationCenter.Instance.BlueSyrupProductionStarted.Invoke(new BlueSyrupProductionStartedEventArgs());

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.MedicineQueued);
        }

        public void ShowMedicineInQueue(MedicineRef medicine)
        {
            if (productionQueue.Count >= QueueSize - 1)
                return;

            if (hover != null)
            {
                //	Debug.Log("Pokaz");
                hover.ShowAdditionalOnList(medicine, actualMedicine);
            }
        }

        public void HideMedicineInQueue()
        {
            if (productionQueue.Count >= QueueSize - 1)
                return;

            if (hover != null)
            {
                //Debug.Log("Ukryj");
                hover.RemoveAdditionalFromList(actualMedicine);
            }
        }

        public int GetMedicineCountInQueue(MedicineRef med)
        {
            int count = 0;

            if (actualMedicine != null)
            {
                if (actualMedicine.type == med.type && actualMedicine.id == med.id)
                    ++count;
            }

            if (productionQueue.Count > 0)
            {
                foreach (MedicineRef queueMed in productionQueue)
                {
                    if (queueMed.type == med.type && queueMed.id == med.id)
                        ++count;
                }
            }
            return count;
        }

        public void EnlargeQueue()
        {
            if (ShouldWork)
                QueueSize = QueueSize + 1;
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            ShouldWork = obj == null && Anchored;

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

        protected override void AddToMap()
        {
            base.AddToMap();

            ShouldWork = obj == null && Anchored;
            if (this != null && isoObj != null)
            {
                var p = isoObj.GetGameObject();
                if (p.transform.GetChild(0).gameObject.GetComponent<Animator>() != null)
                {
                    machineAnim = p.transform.GetChild(0).gameObject.GetComponent<Animator>();
                    if (machineAnim != null && ActualMedicineProductionTime > 0)
                        machineAnim.SetBool("IsWorking", true);
                    else
                        machineAnim.SetBool("IsWorking", false);
                }
            }
            if (masterableProperties != null)
                masterableProperties.RefreshMasteryView(false);
        }

        private void MoveMedicineFromQueueToProduction()
        {
            if (productionQueue.Count == 0)
                return;
            actualMedicine = productionQueue.First.Value;
            productionQueue.RemoveFirst();
            ActualMedicineProductionTime = ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime;

            machineAnim.SetBool("IsWorking", true);
            BounceMachineObject();
            if (hover != null)
            {
                hover.ActualizeList(actualMedicine);
                UpdateHooverItemForMedicine();
            }
        }

        public void SpeedUpWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
        {
            bool isTank = actualMedicine.IsMedicineForTankElixir();
            if ((isTank && GameState.Get().CanAddAmountForTankStorage(1)) ||
                (!isTank && GameState.Get().CanAddAmountForElixirStorage(1))) // CV: check added to prevent using diamonds to buy while Tank or Storage is full
            {
                IGameState gs = Game.Instance.gameState();
                int cost = DiamondCostCalculator.GetCostForAction(ActualMedicineProductionTime, ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime, Tag, MedicineProductionHover.tutorialMode);
                if (gs.GetDiamondAmount() >= cost)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                    {
                        gs.RemoveDiamonds(cost, EconomySource.SpeedUpProduction, Tag);
                        actualMedicineProductionTime = 0;
                        //boughtWithDiamonds = true;
                        Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1, transform.position.z + actualData.rotationPoint.y);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                        speedUpUsed = true;
                        NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    }, diamondTransactionMaker);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
            else
            {                
                MessageController.instance.ShowMessage(isTank ? 47 : 9);
            }
        }

        public void StopMakingMedicine()
        {
            if (actualMedicine == null)
            {
                machineAnim.SetBool("IsWorking", false);
                return;
            }

            ++ProducedMedicines;
            if (string.Compare(info.infos.Tag, "ElixirLab") == 0)
            {
                List<MedicineProductionMachine> elixirLabs = GameState.Get().GetAllMedicineProductionMachines(info.infos.Tag);
                for (int i = 0; i < elixirLabs.Count; ++i)
                {
                    elixirLabs[i].masterableProperties.AddMasteryProgress(1);
                }
            }
            else
                masterableProperties.AddMasteryProgress(1);

            GameState.Get().CuresCount.AddProducedMedicines(1);

            CreateCollectable(ResourcesHolder.Get().GetSpriteForCure(actualMedicine), actualMedicine, 1, false);
            if (speedUpUsed && !MedicineProductionHover.tutorialMode)
            {
                CollectCollectable(false, false, true);
                speedUpUsed = false;
            }
            if (MedicineProductionHover.tutorialMode && actualMedicine.type == MedicineType.Syrop)//If we are in tutorial
                NotificationCenter.Instance.BlueSyrupExtractionCompleted.Invoke(new BlueSyrupExtractionCompletedArgs(speedUpUsed));

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.MedicineReady);

            MedicineBadgeHintsController.Get().RemoveMedInProduction(actualMedicine);
            actualMedicine = null;
            machineAnim.SetBool("IsWorking", false);
            BounceMachineObject();
            if (hover != null)
                hover.ActualizeList(actualMedicine);
        }

        public void SetSameMachineMasteries()
        {
            List<MedicineProductionMachine> elixirLabs = GameState.Get().GetAllMedicineProductionMachines(info.infos.Tag);
            int maxMasteryLevel = -1;
            int maxMasteryProgress = -1;

            for (int i = 0; i < elixirLabs.Count; ++i)
            {
                if (maxMasteryLevel < elixirLabs[i].masterableProperties.MasteryLevel)
                    maxMasteryLevel = elixirLabs[i].masterableProperties.MasteryLevel;
            }

            for (int i = 0; i < elixirLabs.Count; ++i)
            {
                if (maxMasteryLevel == elixirLabs[i].masterableProperties.MasteryLevel && maxMasteryProgress < elixirLabs[i].masterableProperties.MasteryProgress)
                    maxMasteryProgress = elixirLabs[i].masterableProperties.MasteryProgress;
            }

            for (int i = 0; i < elixirLabs.Count; ++i)
            {
                elixirLabs[i].masterableProperties.SetMasteryLevel(maxMasteryLevel);
                elixirLabs[i].masterableProperties.SetMasteryProgress(maxMasteryProgress);
            }
        }

        public void UpdateHooverItemForMedicine()
        {
            if (hover != null)
            {
                ProductionHoverDraggableElement[] listOfDragableElement = FindObjectsOfType<ProductionHoverDraggableElement>();
                foreach (ProductionHoverDraggableElement element in listOfDragableElement)
                {
                    if (element.GetMedicine() == actualMedicine)
                    {
                        element.UpdateBadgeVisibility();
                        break;
                    }
                }
                listOfDragableElement = null;

                if (actualMedicine != null)
                    hover.SetSpeedUpCostText(ActualMedicineProductionTime, ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime);
            }
        }

        public float GetTimeToEndProduction()
        {
            return ActualMedicineProductionTime + productionQueue.Count * ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime;
        }

        public float GetTimeToEndProductionMeds(int medsCount)
        {
            return ActualMedicineProductionTime + (medsCount - 1) * ResourcesHolder.Get().GetMedicineInfos(actualMedicine).ProductionTime;
        }

        public int CountProducingMeds()
        {
            return productionQueue.Count + 1;
        }

        public bool IsProducing()
        {
            return ShouldWork && actualMedicine != null;
        }

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            if (ShouldWork)
            {
                if (actualMedicine != null)
                    ActualMedicineProductionTime -= Time.deltaTime;
                else if (productionQueue.Count > 0)
                    MoveMedicineFromQueueToProduction();
            }

            if (actualMedicine != null && ActualMedicineProductionTime < 0)
            {
                StopMakingMedicine();
                UpdateHooverItemForMedicine();
                ActualMedicineProductionTime = 0;
            }

            if (!isHintUpdated && !IsDummy && state != State.fresh)
            {
                //HintsController.Get().RemoveHint(new BuildingHint(Tag));
                isHintUpdated = true;
            }
        }

        private IEnumerator<float> DelayedUnwrap()
        {
            isUnwraping = true;
            map.GetObject(new Vector2i(position.x, position.y)).GetComponent<Animator>().SetTrigger("Unwrap");

            yield return Timing.WaitForSeconds(0.5f);

            if (!HospitalDataHolder.Instance.BuiltProductionMachines.Contains(this))
                HospitalDataHolder.Instance.BuiltProductionMachines.Add(this);

            if (string.Compare(info.infos.Tag, "ElixirLab") == 0)
                SetSameMachineMasteries();

            base.OnClickWaitForUser();
            NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this));
            AchievementNotificationCenter.Instance.CureLabBuilt.Invoke(new AchievementProgressEventArgs(1));
            int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.MedicineProduced, false, Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });

            MedicineBadgeHintsController.Get().UpdateMedicineWillBeNeededToHealWithMachine(((ShopRoomInfo)info.infos));
            // HintsController.Get().UpdateAllHintsWithMedicineCount();
            isUnwraping = false;
        }

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
            infoShowed = isShowed;
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

        public string GetTag()
        {
            return Tag;
        }

        public string GetClientTag()
        {
            if (info != null && info.infos != null)
                return info.infos.Tag;
            return string.Empty;
        }
    }
}
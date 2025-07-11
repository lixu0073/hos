using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using MovementEffects;
using IsoEngine;
using SimpleUI;

namespace Hospital
{

    public class PlantationPatch : MonoBehaviour
    {
        [SerializeField] Sprite lockedPatch = null;
        [SerializeField] Sprite unreadyPatch = null;
        [SerializeField] Sprite readyPatch = null;
        [SerializeField] Sprite wearyPatch = null;

        [SerializeField] private Transform PlantT = null;
        [SerializeField] private Transform SignT = null;
        [SerializeField] private Transform BadgeT = null;
        [SerializeField] private Transform HelpAvailableIndicator = null;

        [SerializeField] public PlantationPlayerBadge plantationPlayerBadge;

        [SerializeField] Sprite helpSign = null;

        [SerializeField] private GameObject selection = null;
        [SerializeField] private ParticleSystem cultivationParticles = null;
        [SerializeField] private ParticleSystem seedParticles = null;
        private Guid DiamondTransactionSession = Guid.Empty;

        [HideInInspector]
        public bool patchSelected = false;

        //private bool MessageIntervalOn = false;

        public MedicineRef GrowingPlant = null;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public bool HasHelpRequest()
        {
            return SignT.gameObject.activeSelf;
        }

        private int MaxRegrowth;
        private int regrowthLeft;
        public int RegrowthLeft
        {
            get { return regrowthLeft; }
            set { regrowthLeft = value; }
        }

        [SerializeField] private int harvestAmount;
        private int harvestLeft;
        public int HarvestLeft
        {
            get { return harvestLeft; }
            set { harvestLeft = value; }
        }

        public Vector2i position;

        [HideInInspector]
        public float productionTime;
        private float timeFromSeed;
        public float TimeFromSeed
        {
            get { return timeFromSeed; }
            set { timeFromSeed = value; }
        }

        [SerializeField] private float renewTime = 0.0f;
        private EPatchState patchState = EPatchState.disabled;

        public EPatchState PatchState
        {
            get { return patchState; }
            set
            {
                patchState = value;
                Init();
            }
        }

        plantationPatchHover hover;
        Vector3 clickStartPosition;

        private bool existed_before = false;

        delegate void DClick();
        DClick OnClick;

        bool visitingMode = false;
        [SerializeField] Animator anim = null;

        //public IEnumerator<float> pathCoroutine = null;

        [SerializeField] private GameObject TYFloat = null;
        private bool waitingForHelpCallback = false;
        private bool waitingForHelpAsk = false;

        public void Init()
        {
            waitingForHelpAsk = false;
            MaxRegrowth = HospitalAreasMapController.HospitalMap.greenHouse.maxRegrowth;
            harvestAmount = HospitalAreasMapController.HospitalMap.greenHouse.harvestAmount;
            switch (patchState)
            {
                case EPatchState.disabled:
                    {
                        HideSelection();
                        regrowthLeft = MaxRegrowth;
                        gameObject.GetComponent<SpriteRenderer>().sprite = lockedPatch;
                        PlantT.gameObject.SetActive(false);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        OnClick = OnClickDisabled;
                    }
                    break;
                case EPatchState.waitingForRenew:
                    {
                        HideSelection();
                        gameObject.GetComponent<SpriteRenderer>().sprite = unreadyPatch;
                        PlantT.gameObject.SetActive(false);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        OnClick = OnClickRenew;
                    }
                    break;
                case EPatchState.renewing:
                    {
                        HideSelection();
                        gameObject.GetComponent<SpriteRenderer>().sprite = unreadyPatch;
                        PlantT.gameObject.SetActive(false);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        productionTime = renewTime;

                        //pathCoroutine = Timing.RunCoroutine(Counting(EPatchState.empty));
                        OnClick = OnClickEnabled;
                    }
                    break;
                case EPatchState.empty:
                    {
                        HideSelection();
                        gameObject.GetComponent<SpriteRenderer>().sprite = readyPatch;
                        PlantT.gameObject.SetActive(false);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        OnClick = OnClickEnabled;
                    }
                    break;
                case EPatchState.producing:
                    {
                        HideSelection();

                        gameObject.GetComponent<SpriteRenderer>().sprite = readyPatch;
                        PlantT.gameObject.SetActive(true);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        OnClick = OnClickEnabled;

                        var p = ResourcesHolder.Get().GetMedicineInfos(GrowingPlant) as BasePlantInfo;
                        productionTime = p.ProductionTime;
                        //np start coroutiny sprawdzającej ststus produkcji

                        //pathCoroutine = Timing.RunCoroutine(Counting(EPatchState.waitingForUser));

                        if (GrowingPlant != null)
                        {
                            existed_before = true;
                            MedicineBadgeHintsController.Get().AddSingleMedInProduction(GrowingPlant, 3);
                        }
                    }
                    break;
                case EPatchState.waitingForUser:
                    {
                        gameObject.GetComponent<SpriteRenderer>().sprite = readyPatch;
                        PlantT.gameObject.SetActive(true);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        SetPlantSprite();
                        OnClick = OnClickEnabled;

                        if (GrowingPlant != null && existed_before == false)
                        {
                            existed_before = true;
                            MedicineBadgeHintsController.Get().AddSingleMedInProduction(GrowingPlant, 3);
                        }
                    }
                    break;
                case EPatchState.fallow:
                    {
                        HideSelection();
                        gameObject.GetComponent<SpriteRenderer>().sprite = wearyPatch;
                        PlantT.gameObject.SetActive(true);
                        SignT.gameObject.SetActive(false);
                        BadgeT.gameObject.SetActive(false);
                        HelpAvailableIndicator.gameObject.SetActive(!HospitalAreasMapController.HospitalMap.VisitingMode && regrowthLeft > -1);
                        SetDeadPlantSprite();
                        OnClick = OnClickEnabled;
                        //np waiting for help initialisaton
                    }
                    break;
                case EPatchState.waitingForHelp:
                    {
                        gameObject.GetComponent<SpriteRenderer>().sprite = wearyPatch;
                        PlantT.gameObject.SetActive(true);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, int.MaxValue, 0f, ReferenceHolder.Get().giftSystem.particleSprites[5]);
                        SignT.gameObject.SetActive(true);
                        BadgeT.gameObject.SetActive(HospitalAreasMapController.HospitalMap.VisitingMode);
                        HelpAvailableIndicator.gameObject.SetActive(false);
                        SignT.GetComponent<SpriteRenderer>().sprite = helpSign;
                        SetDeadPlantSprite();
                        OnClick = OnClickEnabled;
                        //np waiting for help initialisaton
                    }
                    break;
            }

            SetPlantAnimation();
        }

        public void SeedHelped()
        {
            PatchState = EPatchState.empty;
            Seed(GrowingPlant, true);
        }

        public void ShowPlayerHelpBadge(string HospitalName, int Level, string FacebookID)
        {
            if (plantationPlayerBadge == null)
                return;

            if (string.IsNullOrEmpty(FacebookID) || !AccountManager.Instance.IsFacebookConnected)
            {
                plantationPlayerBadge.gameObject.SetActive(true);
                plantationPlayerBadge.SetLogin(HospitalName);
                plantationPlayerBadge.SetLevel(Level);
                plantationPlayerBadge.SetDefaultAvatar();
                plantationPlayerBadge.SetDefaultFrame();
            }
            else
            {
                plantationPlayerBadge.SetLevel(Level);
                plantationPlayerBadge.LoadFacebookData(FacebookID);
            }
        }

        public void HidePlayerHelpBadge()
        {
            plantationPlayerBadge.gameObject.SetActive(false);
        }

        public bool Seed(MedicineRef medicine, bool isRegrowth, PlantationPatchTool tool = null)
        {
            if (patchState != EPatchState.empty)
                return false;
            var p = ResourcesHolder.Get().GetMedicineInfos(medicine) as BasePlantInfo;
            if (p == null)
            {
                Debug.Log("p is NULL");
                return false;
            }
            if (!isRegrowth)
            {
                if (Game.Instance.gameState().GetCoinAmount() < p.Price)
                {
                    if (tool != null)
                        tool.Close();
                    UIController.get.BuyResourcesPopUp.Open(p.Price - Game.Instance.gameState().GetCoinAmount(), () =>
                    {
                        Seed(medicine, isRegrowth, tool);
                    }, null); ;
                    return false;
                }
                int price = (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.garden_text) ? 0 : p.Price;
                GameState.Get().RemoveCoins(price, EconomySource.PlantationRegrow);
                if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.garden_text)
                    NotificationCenter.Instance.FirstPlantPlanted.Invoke(new BaseNotificationEventArgs());
                if (price > 0)
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, price, 0f, ReferenceHolder.Get().giftSystem.particleSprites[0]);
            }


            GrowingPlant = medicine;
            productionTime = p.ProductionTime;
            timeFromSeed = 0;
            // sprite setting (Translated from polish)
            harvestLeft = harvestAmount;
            PatchState = EPatchState.producing;
            PlantT.gameObject.SetActive(false);
            Bounce();
            Timing.RunCoroutine(PlaySeed());
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.PlantationSeeded);
            return true;
        }
        /// <summary>
        /// Play the seed planting animation and sound effect.
        /// </summary>
        IEnumerator<float> PlaySeed()
        {
            yield return Timing.WaitForSeconds(0.5f);
            seedParticles.Play();
            Bounce();
            SoundsController.Instance.PlaySeeding();

            yield return Timing.WaitForSeconds(0.7f);
            PlantT.gameObject.SetActive(true);
            SetPlantSprite();
        }
        /// <summary>
        /// Collect the medicine from the patch.
        /// </summary>
        public bool Collect()
        {
            if (patchState != EPatchState.waitingForUser)
                return true;

            while (harvestLeft > 0)
            {
                if (GrowingPlant.IsMedicineForTankElixir())
                {
                    if (!GameState.Get().CanAddAmountForTankStorage(1))
                    {
                        MessageController.instance.ShowMessage(47);
                        StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(true));
                        return false;
                    }
                }
                else if (!GameState.Get().CanAddAmountForElixirStorage(1))
                {
                    MessageController.instance.ShowMessage(9);
                    StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(false));
                    return false;
                }

                //harvesting sound
                SoundsController.Instance.PlayRustle();

                UIController.get.storageCounter.Add(GrowingPlant.IsMedicineForTankElixir());
                MedicineBadgeHintsController.Get().RemoveMedInProduction(GrowingPlant);

                GameState.Get().AddResource(GrowingPlant, 1, false, EconomySource.Plantation);

                bool isTank = GrowingPlant.IsMedicineForTankElixir();
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, transform.position, 1, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(GrowingPlant), null, () =>
                {
                    UIController.get.storageCounter.Remove(1, isTank);
                });
                int expReward = ResourcesHolder.Get().GetEXPForCure(GrowingPlant);
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.PlantProduced, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                });

                GameState.Get().UpdateExtraGift(transform.position, false, SpecialItemTarget.Storage);

                HandlePickupParticle(transform.position);

                NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(GrowingPlant, GameState.Get().GetCureCount(GrowingPlant)));
                --harvestLeft;
            }

            //Achievement
            var p = ResourcesHolder.Get().GetMedicineInfos(GrowingPlant) as BasePlantInfo;
            if (p.Kingdom == BasePlantInfo.LifeKingdom.Plant)
                AchievementNotificationCenter.Instance.MedicalPlantsPicked.Invoke(new AchievementProgressEventArgs(1));
            else if (p.Kingdom == BasePlantInfo.LifeKingdom.Fungi)
                AchievementNotificationCenter.Instance.MedicalFungiPicked.Invoke(new AchievementProgressEventArgs(1));

            --regrowthLeft;
            if (regrowthLeft > 0)
            {
                PatchState = EPatchState.empty;
                Seed(GrowingPlant, true);
            }
            else
                PatchState = EPatchState.fallow;

            SetPlantAnimation();
            StartCoroutine(FireCollectionParticles(GrowingPlant));
            Bounce();
            return true;
        }

        protected void HandlePickupParticle(Vector3 pos)
        {
            RotatableObject.FireOnMedicineCollected();
            if (HospitalBedController.isNewCureAvailable)
            {
                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled() || HospitalBedController.isNewCureAvailable)
                    ReferenceHolder.Get().giftSystem.SpawnPickUpParticle(pos, HospitalBedController.isNewCureAvailable);

                SoundsController.Instance.PlayAlert();
            }
            HospitalBedController.isNewCureAvailable = false;
        }

        public void CancelHelpRequestOnPlantationPatch()
        {
            PatchState = EPatchState.fallow;
        }

        IEnumerator FireCollectionParticles(MedicineRef growingPlant)
        {
            yield return new WaitForSeconds(25f / 120f);     //in this frame of collect animation this would be fired
            GameObject explosion = Instantiate(((BasePlantInfo)ResourcesHolder.Get().GetMedicineInfos(growingPlant)).Explosion);
            explosion.transform.SetParent(transform);
            explosion.transform.localPosition = Vector3.zero;        //so the position is taken from explosion prefab
            explosion.transform.rotation = Quaternion.Euler(270, 0, 0);
            Destroy(explosion, 3f);
        }

        public bool Cultivate(PlantationPatchTool tool = null)
        {
            if ((patchState != EPatchState.fallow) && (patchState != EPatchState.waitingForHelp))
                return false;
            if (GameState.Get().GetCureCount(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef()) > 0)
            {
                //pre cultivation action ex. Hoes--
                //real cultivation
                if (patchState == EPatchState.waitingForHelp)
                    PlantationManager.Instance.DeleteHelpRequest(position.ToString());

                GameState.Get().GetCure(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef(), 1, EconomySource.Plantation);
                regrowthLeft = MaxRegrowth;
                PatchState = EPatchState.empty;
                ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, 1, 0f, ResourcesHolder.GetHospital().PatchCultivatorSprite);
                cultivationParticles.Play();
                SoundsController.Instance.PlayShoveling();
                Bounce();

                SoundsController.Instance.PlayShoveling();

                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UseShovels));
            }
            else
            {
                if (UIController.get.BuyResourcesPopUp.gameObject.activeInHierarchy)
                    return false;

                List<KeyValuePair<int, MedicineDatabaseEntry>> neededResources = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
                neededResources.Add(new KeyValuePair<int, MedicineDatabaseEntry>(1, ResourcesHolder.Get().medicines.cures[15].medicines[3]));
                UIController.get.BuyResourcesPopUp.Open(neededResources, false, false, false, () =>
                {
                    if (patchState == EPatchState.waitingForHelp)
                        PlantationManager.Instance.DeleteHelpRequest(position.ToString());
                    GameState.Get().GetCure(ResourcesHolder.Get().medicines.cures[15].medicines[3].GetMedicineRef(), 1, EconomySource.Plantation);
                    regrowthLeft = MaxRegrowth;
                    PatchState = EPatchState.empty;
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, 1, 0f, ResourcesHolder.GetHospital().PatchCultivatorSprite);
                    cultivationParticles.Play();
                    Bounce();
                    SoundsController.Instance.PlayShoveling();

                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.UseShovels));
                }, null, null);
            }

            if (tool != null)
                tool.Close();

            return true;
        }

        public void UpdateMissingData()
        {
            waitingForHelpAsk = false;
            HospitalAreasMapController.HospitalMap.greenHouse.RefreshMyNewHelpRequests();
            PatchState = EPatchState.waitingForHelp;
            AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.RequestHelpPlantation, null);
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.AskForHelp));
        }

        public bool HelpAsk()
        {
            if (patchState != EPatchState.fallow || regrowthLeft < 0 || waitingForHelpAsk)
                return false;

            Bounce();
            waitingForHelpAsk = true;
            PlantationManager.Instance.PostHelpRequest(position.ToString(), () =>
            {
                waitingForHelpAsk = false;
                HospitalAreasMapController.HospitalMap.greenHouse.RefreshMyNewHelpRequests();
                PatchState = EPatchState.waitingForHelp;
                AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.RequestHelpPlantation, null);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.AskForHelp));
            }
            );
            return true;
        }

        public void Helped()
        {
            if (patchState != EPatchState.waitingForHelp)
                return;
            PatchState = EPatchState.producing;
        }

        private BalanceableInt expForGardenHelpBalanceable;
        private int ExpForGardenHelp
        {
            get
            {
                if (expForGardenHelpBalanceable == null)
                    expForGardenHelpBalanceable = BalanceableFactory.CreateExpForGardenHelpBalanceable();

                return expForGardenHelpBalanceable.GetBalancedValue();
            }
        }

        private BalanceableFloat drawShovelChanceInGardenBalanceable;
        private float DrawShovelForGardenHelpChance
        {
            get
            {
                if (drawShovelChanceInGardenBalanceable == null)
                    drawShovelChanceInGardenBalanceable = BalanceableFactory.CreateShovelDrawChanceForGardenHelpBalanceable();

                return drawShovelChanceInGardenBalanceable.GetBalancedValue();
            }
        }

        private void HelpDone()
        {
            if (waitingForHelpCallback)
                return;

            waitingForHelpCallback = true;
            PlantationManager.Instance.FullfillHelpRequest(SaveLoadController.SaveState.ID, position.ToString(), () =>
            {
                Debug.Log("Successfully helped");
                cultivationParticles.Play();
                SoundsController.Instance.PlayShoveling();
                Bounce();
                int expReward = ExpForGardenHelp;
                if (Debug.isDebugBuild)
                {
                    Debug.Log("BALANCE -> EXP FOR GARDEN HELP: " + expReward);
                }
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.PlantationHelp, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, expReward, 0.5f, 2, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                });
                TYFloat.SetActive(true);
                Invoke("CloseTYFloat", 3f);
                timeFromSeed = 0;
                PatchState = EPatchState.producing;

                float shovelDrawChance = DrawShovelForGardenHelpChance;
                if (Game.Instance.gameState().GetHospitalLevel() >= 15 && UnityEngine.Random.value < shovelDrawChance)
                {
                    MedicineRef extraGift = new MedicineRef(MedicineType.Special, 3);
                    GameState.Get().AddExtraGift(extraGift, transform.position);
                }

                SoundsController.Instance.PlayShoveling();

                AnalyticsController.instance.ReportSocialHelp(SocialHelpAction.GiveHelpPlantation, SaveLoadController.SaveState.ID);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.HelpInMedicinalGardens));
                waitingForHelpCallback = false;
                OnPostHelp();
                GameState.Get().IncrementHelpsCounter();
            });
        }

        private void OnPostHelp()
        {
            ReferenceHolder.GetHospital().plantation.UpdateHelpRequestsStatus();
        }

        private void CloseTYFloat()
        {
            TYFloat.SetActive(false);
        }

        public void InitHelp(bool visiting)
        {
            visitingMode = visiting;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private float sqrmargin = 100.0f;
#else
		private float sqrmargin = 400.0f;
#endif

        void ShowSelection()
        {
            selection.SetActive(true);
            patchSelected = true;
        }

        void HideSelection()
        {
            if (selection.activeInHierarchy)
            {
                selection.SetActive(false);
                patchSelected = false;
            }
        }

        void OnMouseDown()
        {
            clickStartPosition = Input.mousePosition;
        }

        void OnMouseUp()
        {
            if (!BaseCameraController.IsPointerOverInterface() && (!visitingMode || PatchState == EPatchState.waitingForHelp))
                if ((clickStartPosition - Input.mousePosition).sqrMagnitude < sqrmargin)
                {
                    print("plant Patch: " + PatchState + " mode: " + visitingMode);
                    OnClick();
                    if (UIController.get.drawer.IsVisible)
                        UIController.get.drawer.SetVisible(false);
                    if (UIController.get.FriendsDrawer.IsVisible)
                        UIController.get.FriendsDrawer.SetVisible(false);
                }
        }

        private void OnClickDisabled()
        {
            if (UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }
            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            Plantation plantation = ReferenceHolder.GetHospital().plantation;
            int unlockLevel = plantation != null && plantation.unlockLevels != null && plantation.unlockLevels.Count > 0 ? plantation.unlockLevels[0] : 15;
            if (Game.Instance.gameState().GetHospitalLevel() < unlockLevel)
            {
                UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.Plantation);
            }
            else
            {
                MessageController.instance.ShowMessage(37);
            }
            SoundsController.Instance.PlayButtonClick2();
        }

        private void OnClickEnabled()
        {
            SoundsController.Instance.PlayButtonClick2();
            Bounce();

            if (visitingMode && PatchState == EPatchState.waitingForHelp)
                HelpDone();
            else
                ShowHover();
        }

        private void OnClickRenew()
        {
            Bounce();

            //oben Renewpopup or so
            timeFromSeed = 0;
            productionTime = renewTime;
            PatchState = EPatchState.renewing;
            ShowHover();
        }

        private void ShowHover()
        {
            HospitalAreasMapController.HospitalMap.ChangeOnTouchType((x) =>
            {
                CloseHover();
                HideSelection();
            });
            ShowSelection();
            var p = plantationPatchHover.Open(this);
            p.Initialize(this, patchState);
            hover = p;
            UpdateProgress();
            hover.UpdateAccordingToMode();
            MoveCameraToShowHover();
        }

        public void CloseHover()
        {
            if (hover != null)
                hover.Close();
            hover = null;
        }

        public void HideHover()
        {
            hover.Hide();
        }

        public void SpeedUpWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker, Action onSuccess)
        {
            if (patchState == EPatchState.producing || patchState == EPatchState.renewing)
            {
                int cost = DiamondCostCalculator.GetCostForAction(productionTime - timeFromSeed, productionTime);
                if (Game.Instance.gameState().GetDiamondAmount() >= cost)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                    {
                        GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpPlantation);
                        timeFromSeed = productionTime;

                        ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position + new Vector3(.5f, 0, .5f), cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                        NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                        onSuccess?.Invoke();
                    }, diamondTransactionMaker);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
        }

        public void SelectForSpeedUp()
        {
            OnClick();
            if (UIController.get.drawer.IsVisible)
                UIController.get.drawer.SetVisible(false);
            if (UIController.get.FriendsDrawer.IsVisible)
                UIController.get.FriendsDrawer.SetVisible(false);
        }

        void SetPlantSprite()
        {
            float percent = Mathf.Clamp(timeFromSeed / productionTime, 0, 1);
            var p = ResourcesHolder.Get().GetPlantSpriteForBasePlant(GrowingPlant, percent);
            PlantT.GetComponent<SpriteRenderer>().sprite = p;
        }

        void SetDeadPlantSprite()
        {
            var p = ResourcesHolder.Get().GetDeadPlantSprite(GrowingPlant);
            PlantT.GetComponent<SpriteRenderer>().sprite = p;
        }

        void SetPlantAnimation()
        {
            if (patchState == EPatchState.waitingForUser)
            {
                switch (GrowingPlant.id)
                {
                    case 0:
                        try
                        {
                            anim.Play(AnimHash.ReadyAloeVera, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case 1:
                        try
                        {
                            anim.Play(AnimHash.ReadyBlueDishFungi, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case 2:
                        try
                        {
                            anim.Play(AnimHash.ReadyDandelion, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case 3:
                        try
                        {
                            anim.Play(AnimHash.ReadyGuaranaBerries, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case 4:
                        try
                        {
                            anim.Play(AnimHash.ReadyHoneyMelon, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case 5:
                        try
                        {
                            anim.Play(AnimHash.ReadyPigTailFungi, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    default:
                        try
                        {
                            anim.Play(AnimHash.ReadyAloeVera, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                }
            }
            else if (patchState == EPatchState.waitingForHelp && HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                try
                {
                    anim.Play(AnimHash.HelpSign, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
            else
            {
                try
                {
                    anim.Play(AnimHash.PlantPatchIdle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
        }

        public float GetTimeToEndProducing()
        {
            return productionTime - timeFromSeed;
        }

        private bool updateHover = true;

        void Update()
        {
            if (PatchState == EPatchState.renewing)
            {
                if (timeFromSeed >= productionTime)
                {
                    if (hover != null)
                        hover.Initialize(this, EPatchState.empty);

                    PatchState = EPatchState.empty;
                }
                else
                {
                    EmulateTime(Time.deltaTime);

                    if (timeFromSeed % 1 < 0.5f)
                    {
                        if (updateHover)
                        {
                            UpdateProgress();
                            updateHover = false;
                        }
                    }
                    else
                        updateHover = true;
                }
            }

            if (PatchState == EPatchState.producing)
            {
                if (GrowingPlant != null)
                    SetPlantSprite();

                if (timeFromSeed >= productionTime)
                {
                    if (hover != null)
                        hover.Initialize(this, EPatchState.waitingForUser);

                    PatchState = EPatchState.waitingForUser;
                }
                else
                {
                    EmulateTime(Time.deltaTime);

                    if (timeFromSeed % 1 < 0.5f)
                    {
                        if (updateHover)
                        {
                            UpdateProgress();
                            updateHover = false;
                        }
                    }
                    else
                        updateHover = true;
                }

            }
        }

        public void EmulateTime(TimePassedObject timePassed)
        {
            EmulateTime(timePassed.GetTimePassed());
        }

        public void EmulateTime(float timePassed)
        {
            timeFromSeed += timePassed;
        }

        IEnumerator<float> Counting(EPatchState state)
        {
            for (; ; )
            {
                if (PatchState == EPatchState.producing && GrowingPlant != null)
                    SetPlantSprite();
                UpdateProgress();

                if (timeFromSeed >= productionTime)
                {
                    print(state);
                    if (hover != null)
                        hover.Initialize(this, state);
                    PatchState = state;
                    break;
                }

                ++timeFromSeed;
                yield return Timing.WaitForSeconds(1.0f);
            }
        }

        private void UpdateProgress()
        {
            if (hover != null)
            {
                hover.SetProductionBar((int)timeFromSeed, (int)productionTime);
                hover.SetSpeedUpCostText(productionTime - timeFromSeed, productionTime);
            }
        }

        public void MoveCameraToShowHover()
        {
            Debug.LogWarning("MoveCameraToShowHover plantationPatchHover");
            Vector3 newPos = ReferenceHolder.Get().engine.MainCamera.RayCast(GetMoveCameraPoint());
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(newPos.x, 0, newPos.z), 1.0f, false);
        }

        public virtual Vector3 GetMoveCameraPoint()
        {
            Vector2 lefBottomPoint = new Vector2(0f, 0f);
            Vector2 rightTopPoint = new Vector2(Screen.width, Screen.height);

            RectTransform container = GetHoverContainer();

            Vector2 currentPos = new Vector2(container.transform.position.x, container.transform.position.y);
            Vector2 currentSize = new Vector2(container.rect.width * container.lossyScale.x, container.rect.height * container.lossyScale.y);
            Vector2 deltaPosition = ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(ReferenceHolder.Get().engine.MainCamera.transform.position);

            if ((currentPos.x - currentSize.x / 2) < lefBottomPoint.x)
                deltaPosition.x = (Screen.width / 2) - (lefBottomPoint.x - (currentPos.x - currentSize.x / 2));
            if ((currentPos.y - currentSize.y / 2) < lefBottomPoint.y)
                deltaPosition.y = Screen.height / 2 - (lefBottomPoint.y - (currentPos.y - currentSize.y / 2));
            if ((currentPos.x + currentSize.x / 2) > rightTopPoint.x)
                deltaPosition.x = (Screen.width / 2) + ((currentPos.x + currentSize.x / 2) - rightTopPoint.x);
            if ((currentPos.y + currentSize.y / 2) > rightTopPoint.y)
                deltaPosition.y = (Screen.height / 2) + ((currentPos.y + currentSize.y / 2) - rightTopPoint.y);

            return deltaPosition;
        }

        public RectTransform GetHoverContainer()
        {
            return hover.transform.GetChild(0).GetComponent<RectTransform>() as RectTransform;
        }

        IEnumerator<float> MessageInterval(int messageID, float interval)
        {
            //MessageIntervalOn = true;
            MessageController.instance.ShowMessage(messageID);
            yield return Timing.WaitForSeconds(interval);
            //MessageIntervalOn = false;
        }

        void SetHelpDummy()
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = wearyPatch;
            PlantT.gameObject.SetActive(true);
            SignT.gameObject.SetActive(true);
            BadgeT.gameObject.SetActive(HospitalAreasMapController.HospitalMap.VisitingMode);
            SignT.GetComponent<SpriteRenderer>().sprite = helpSign;
            SetDeadPlantSprite();
            OnClick = null;
        }

        void Bounce()
        {
            Timing.RunCoroutine(BounceCoroutine());
        }

        public bool IsFieldOnlyToExcavate()
        {
            return patchState == EPatchState.fallow && !waitingForHelpAsk;
        }

        IEnumerator<float> BounceCoroutine()
        {
            float bounceTime = .15f;
            float timer = 0f;

            Vector3 normalScale = Vector3.one;
            Vector3 targetScale = Vector3.one * 1.1f;
            if (normalScale == Vector3.zero)
                normalScale = transform.localScale;

            targetScale = normalScale * 1.1f;

            //scale up
            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
                //scale up
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    transform.localScale = Vector3.Lerp(normalScale, targetScale, timer / bounceTime);
                    yield return 0;
                }
                timer = 0f;
                //scale down
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    transform.localScale = Vector3.Lerp(targetScale, normalScale, timer / bounceTime);
                    yield return 0;
                }
            }
        }
    }

    public enum EPatchState
    {
        disabled,
        waitingForRenew,
        renewing,
        empty,
        producing,
        waitingForUser,
        fallow,
        waitingForHelp,
    }
}

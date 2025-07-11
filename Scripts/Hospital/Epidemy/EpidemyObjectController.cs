using UnityEngine;
using System.Collections.Generic;
using System;
using SimpleUI;
using MovementEffects;

namespace Hospital
{
    public class EpidemyObjectController : SuperObject
    {
        public ExternalRoomInfo EpidemyObjectInfo = null;

        [SerializeField] private Transform hoverPoint = null;
#pragma warning disable 0649
        [SerializeField] private GameObject epidemyLocked;
        [SerializeField] private GameObject epidemyRenovating;
        [SerializeField] private GameObject epidemyGift;
        [SerializeField] private GameObject epidemyTent;
        [SerializeField] private Transform particleTransform;
        [SerializeField] private GameObject objectBorder;
#pragma warning restore 0649
        public GameObject ObjectBorder { get { return objectBorder; } }

        protected ExternalRoomState epidemyObjectState = ExternalRoomState.Disabled;
        private Epidemy epidemyController;

        private float timeSinceRenovationStarted = 0;

        public float TimeSinceRenovationStarted { get { return timeSinceRenovationStarted; } }

        private EpidemyBuildingHover hover;
        //private ProgressBarController progressBar = null;

        public bool IsRenovating()
        {
            return epidemyObjectState == ExternalRoomState.Renovating;
        }

        public bool IsEnabled()
        {
            return epidemyObjectState == ExternalRoomState.Enabled;
        }

        public ExternalRoomState State
        {
            get { return epidemyObjectState; }
            private set { }
        }

        public int GetDurationToEpidemyOutbreakEndWithOffsetInMinutes(int minutes)
        {
            if (epidemyController.Outbreak)
                return Math.Max(0, (int)epidemyController.TimeTillEpidemyEnd - minutes * 60);
                
            return Math.Max(0, (int)(epidemyController.TimeTillOutbreak + epidemyController.EpidemyEndTime) - minutes * 60);
        }

        delegate void DClick();

        DClick dOnClick;

        IEnumerator<float> buildingCoroutine;

        public override void IsoDestroy() { }

        void Start()
        {
            epidemyController = HospitalAreasMapController.HospitalMap.epidemy;
        }

        private void ChangeState(ExternalRoomState newState)
        {
            epidemyObjectState = newState;
            Init();
        }

        public virtual void Init()
        {
            epidemyLocked.SetActive(false);
            epidemyRenovating.SetActive(false);
            epidemyGift.SetActive(false);
            epidemyTent.SetActive(false);

            switch (epidemyObjectState)
            {
                case ExternalRoomState.Disabled:
                    {
                        dOnClick = OnClickDisabled;
                        epidemyLocked.SetActive(true);

                        if (EpidemyObjectInfo.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel())
                            ChangeState(ExternalRoomState.WaitingForRenovation);
                    }
                    break;
                case ExternalRoomState.WaitingForRenovation:
                    {
                        epidemyLocked.SetActive(true);
                        dOnClick = OnClickWaitingForRenew;
                    }
                    break;
                case ExternalRoomState.Renovating:
                    {
                        epidemyRenovating.SetActive(true);
                        dOnClick = OnClickRenovating;
                    }
                    break;
                case ExternalRoomState.WaitingForUser:
                    {
                        if (hover != null)
                            hover.Close();

                        epidemyGift.SetActive(true);
                        dOnClick = OnClickWaitingForUser;
                    }
                    break;
                case ExternalRoomState.Enabled:
                    {
                        epidemyTent.SetActive(true);
                        dOnClick = OnClickEnabled;
                        onInitEnabled();
                    }
                    break;
            }
        }

        public override void OnClick()
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
            dOnClick();
            //Debug.LogError("Epidemy Clicked state: " + epidemyObjectState);

            //SoundsController.Instance.PlayButtonClick2();
        }

        public virtual void OnClickDisabled()
        {
            if (visitingMode)
                return;

            if (Game.Instance.gameState().GetHospitalLevel() < 17)
                UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.Epidemy, false, false);
        }

        public virtual void OnClickWaitingForRenew()
        {
            if (visitingMode)
                return;

            UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.Epidemy, true, false, () => ConfirmRenew());
            NotificationCenter.Instance.EpidemyClicked.Invoke(new BaseNotificationEventArgs());
        }

        public virtual void OnClickRenovating()
        {
            if (visitingMode)
                return;

            hover = EpidemyBuildingHover.Open(this);
            hover.UpdateHover();
            hover.SetWorldPointHovering(hoverPoint.position);
            SoundsController.Instance.PlayConstruction();
        }

        public virtual void OnClickWaitingForUser()
        {
            if (visitingMode)
                return;

            AnalyticsController.instance.ReportRenovate(ExternalRoom.ActionType.unwrap, GetBuildingTag());
            epidemyGift.GetComponent<Animator>().SetTrigger("Unwrap");
            Timing.RunCoroutine(DelayedFX());
            // particleUnpack.SetActive(true);

            int expRecieved = EpidemyObjectInfo.ExpRecived;
            int currentAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expRecieved, EconomySource.BuildingBuilt, false, GetBuildingTag());
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expRecieved, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expRecieved, currentAmount);
            });
            epidemyController.OnEpidemyUnwrap();
            epidemyController.ChestActive(false);
            ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, EpidemyObjectInfo.roomName, ObjectiveRotatableEventArgs.EventType.Unwrap));
            dOnClick -= OnClickWaitingForUser;
            Timing.CallDelayed(0.667f, () =>
            {
                epidemyGift.SetActive(false);

                epidemyTent.gameObject.SetActive(true);
                ChangeState(ExternalRoomState.Enabled);
                NotificationCenter.Instance.EpidemyCenterBuilt.Invoke(new BaseNotificationEventArgs());
            });
        }

        private IEnumerator<float> DelayedFX()
        {
            yield return Timing.WaitForSeconds(0.5f);

            Instantiate(ResourcesHolder.GetHospital().ParticleUnpackVIP, particleTransform.position, Quaternion.identity);
            SoundsController.Instance.PlayCheering();
        }

        public virtual void OnClickEnabled()
        {
            if (!epidemyController.IsHelicopterInAction)
            {
                if (!visitingMode || (visitingMode && epidemyController.HelpMark.activeInHierarchy))
                {
                    if (!epidemyController.Outbreak)
                    {
                        UIController.getHospital.EpidemyOnPopUp.Exit();
                        StartCoroutine(UIController.getHospital.EpidemyOffPopUp.Open(true, false, () =>
                        {
                            if (visitingMode && !epidemyController.HelpMark.activeInHierarchy)
                                MessageController.instance.ShowMessage(58);
                            return;
                        }));
                    }
                    else
                    {
                        UIController.getHospital.EpidemyOffPopUp.Exit();
                        StartCoroutine(UIController.getHospital.EpidemyOnPopUp.Open(true, false, () =>
                        {
                            if (visitingMode && !epidemyController.HelpMark.activeInHierarchy)
                                MessageController.instance.ShowMessage(58);
                            NotificationCenter.Instance.EpidemyClicked.Invoke(new BaseNotificationEventArgs());
                            return;
                        }));                        
                    }
                }

                if (visitingMode && !epidemyController.HelpMark.activeInHierarchy)
                    MessageController.instance.ShowMessage(58);
            }
        }

        public void OnClickSpeedUp(IDiamondTransactionMaker diamondTransactionMaker)
        {
            BuyWithDiamonds(diamondTransactionMaker);
        }

        protected virtual void BuyWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = DiamondCostCalculator.GetCostForBuilding(EpidemyObjectInfo.RenovatingTimeSeconds - TimeSinceRenovationStarted, EpidemyObjectInfo.RenovatingTimeSeconds);
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpBuilding);
                    timeSinceRenovationStarted = EpidemyObjectInfo.RenovatingTimeSeconds;

                    ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    Instantiate(ResourcesHolder.Get().ParticleDiamondBuilding, particleTransform.position, Quaternion.identity);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    if (EpidemyBuildingHover.activeHover != null)
                        EpidemyBuildingHover.activeHover.Close();
                    ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ResetOntouchAction();
                    //progressBar = null;
                }, diamondTransactionMaker);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void OnLvlUp()
        {
            if (EpidemyObjectInfo.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel() && epidemyObjectState == ExternalRoomState.Disabled)
                ChangeState(ExternalRoomState.WaitingForRenovation);
        }

        protected void ConfirmRenew()
        {
            if (Game.Instance.gameState().GetCoinAmount() >= EpidemyObjectInfo.RenovationCost)
            {
                GameState.Get().RemoveCoins(EpidemyObjectInfo.RenovationCost, EconomySource.RenewBuilding);
                SoundsController.Instance.PlayConstruction();
                UIController.getHospital.LockedFeatureArtPopUpController.Exit();

                Debug.Log("ConfirmPurchase() enough coins");

                ReferenceHolder.Get().giftSystem.CreateItemUsed(ReferenceHolder.Get().engine.MainCamera.LookingAt, EpidemyObjectInfo.RenovationCost, .1f, ReferenceHolder.Get().giftSystem.particleSprites[0]);
                timeSinceRenovationStarted = 0;
                ChangeState(ExternalRoomState.Renovating);
                hover = EpidemyBuildingHover.Open(this);
                hover.SetWorldPointHovering(hoverPoint.position);

                SaveSynchronizer.Instance.MarkToSave(SavePriorities.RenovationStarted);

                ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, EpidemyObjectInfo.roomName));
                AnalyticsController.instance.ReportRenovate(ExternalRoom.ActionType.renovateStart, GetBuildingTag());
            }
            else
            {
                UIController.get.BuyResourcesPopUp.Open(EpidemyObjectInfo.RenovationCost - Game.Instance.gameState().GetCoinAmount(), () =>
                {
                    ConfirmRenew();
                }, null);
                MessageController.instance.ShowMessage(0);
            }
        }

        protected virtual void onInitEnabled() { }

        public float GetTimeToEndRenovation()
        {
            return EpidemyObjectInfo.RenovatingTimeSeconds - timeSinceRenovationStarted;
        }

        /*
        IEnumerator<float> Counting(ExternalRoomState state)
        {
            for (; ; )
            {
                if (hover != null)
                    hover.UpdateHover();

                if (timeSinceRenovationStarted >= EpidemyObjectInfo.RenovatingTimeSeconds)
                {
                    ChangeState(state);
                    if (EpidemyBuildingHover.activeHover != null)
                        EpidemyBuildingHover.activeHover.Close();
                    Debug.Log("wait for user");
                    break;
                }

                timeSinceRenovationStarted++;
                yield return Timing.WaitForSeconds(1.0f);
            }
        }
        */

        private void Update()
        {
            if (State == ExternalRoomState.Renovating)
            {
                if (TimeSinceRenovationStarted >= EpidemyObjectInfo.RenovatingTimeSeconds)
                {
                    ChangeState(ExternalRoomState.WaitingForUser);
                    if (EpidemyBuildingHover.activeHover != null)
                        EpidemyBuildingHover.activeHover.Close();
                }

                EmulateTime(Time.deltaTime);
            }
        }

        public List<string> SaveToString()
        {
            List<string> saveData = new List<string>();

            saveData.Add(epidemyObjectState.ToString());
            saveData.Add(timeSinceRenovationStarted.ToString());

            return saveData;
        }

        public void LoadFromString(List<string> saveData, TimePassedObject timeSinceLastSave)
        {
            try
            {
                float loadedRenovationTimeLeft = float.Parse(saveData[1], System.Globalization.CultureInfo.InvariantCulture);
                timeSinceRenovationStarted = loadedRenovationTimeLeft + timeSinceLastSave.GetTimePassed();

                ChangeState((ExternalRoomState)Enum.Parse(typeof(ExternalRoomState), saveData[0]));
            }
            catch (Exception)
            {
                GenerateDefaultSave();
            }
        }

        public void EmulateTime(TimePassedObject timePassed)
        {
            EmulateTime(timePassed.GetTimePassed());
        }

        public void EmulateTime(float timePassed)
        {
            timeSinceRenovationStarted += timePassed;
        }

        private void GenerateDefaultSave()
        {
            Debug.LogError("Epidemy object save is corrupted. Generating default save");
            timeSinceRenovationStarted = 0;
            epidemyController.Unwrapped = false;
            ChangeState(ExternalRoomState.Disabled);
            if (EpidemyObjectInfo.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel() && epidemyObjectState == ExternalRoomState.Disabled)
            {
                Debug.Log("wait for reno"); 
                ChangeState(ExternalRoomState.Enabled);
                epidemyController.Unwrapped = true;
            }
        }

        public enum ExternalRoomState
        {
            Disabled,
            WaitingForRenovation,
            Renovating,
            WaitingForUser,
            Enabled,
        }

        private string GetBuildingTag()
        {
            return "epidemic";
        }
    }
}

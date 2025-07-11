using UnityEngine;
using System.Collections.Generic;
using System;
using SimpleUI;
using MovementEffects;
using System.Text;

namespace Hospital
{
    public abstract class ExternalRoom : SuperObjectWithVisiting
    {
        [SerializeField] public ExternalRoomInfo roomInfo = null;
        //  [SerializeField] public Transform particleUnpack;
        [SerializeField] public Transform particleTransform;
        [SerializeField] public Transform particleDiamondTransform;
        [SerializeField] public GameObject Adorners;

        [SerializeField] private Transform hoverPoint = null;

        [SerializeField] private GameObject Roof = null;
        [SerializeField] private GameObject Tent = null;
        [SerializeField] private GameObject Construction = null;
        [SerializeField] private GameObject Furniture = null;

        protected EExternalHouseState externalHouseState = EExternalHouseState.disabled;

        private bool isUnwraping = false;

        public EExternalHouseState ExternalHouseState
        {
            get { return externalHouseState; }
            set
            {
                externalHouseState = value;
                Init();
            }
        }

        private float timeFromRenovationStarts = 0;

        public float TimeFromRenovationStarts
        {
            get { return timeFromRenovationStarts; }
        }

        private ExternalBuildingHover hover;
        //private ProgressBarController progressBar = null;

        delegate void DClick();

        DClick dOnClick;

        public override void IsoDestroy()
        {
            BaseGameState.OnLevelUp -= onLvlUp;
        }

        void Start()
        {
            ExternalHouseState = EExternalHouseState.disabled;
        }

        //private
        public virtual void Init()
        {
            switch (externalHouseState)
            {
                case EExternalHouseState.disabled:
                    {
                        BaseGameState.OnLevelUp -= onLvlUp;
                        BaseGameState.OnLevelUp += onLvlUp;
                        dOnClick = OnClickDisabled;
                        if (Roof != null)
                            Roof.SetActive(true);
                        if (Tent != null)
                            Tent.SetActive(false);

                        if (Construction != null)
                            Construction.SetActive(false);
                        if (Furniture != null)
                            Furniture.SetActive(false);

                        if (roomInfo.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel())
                        {
                            BaseGameState.OnLevelUp -= onLvlUp;
                            ExternalHouseState = EExternalHouseState.waitingForRenew;
                        }
                    }
                    break;
                case EExternalHouseState.waitingForRenew:
                    {
                        dOnClick = OnClickWaitingForRenew;
                        if (Roof != null)
                            Roof.SetActive(true);
                        if (Tent != null)
                            Tent.SetActive(false);

                        if (Construction != null)
                            Construction.SetActive(false);
                        if (Furniture != null)
                            Furniture.SetActive(false);
                    }
                    break;
                case EExternalHouseState.renewing:
                    {
                        //timeFromRenovationStarts = 0;

                        dOnClick = OnClickRenewing;
                        if (Roof != null)
                            Roof.SetActive(false);
                        if (Tent != null)
                            Tent.SetActive(false);

                        if (Construction != null)
                            Construction.SetActive(true);
                        if (Furniture != null)
                            Furniture.SetActive(false);
                    }
                    break;
                case EExternalHouseState.waitingForUser:
                    {
                        dOnClick = OnClickWaitingForUser;
                        if (Roof != null)
                            Roof.SetActive(false);
                        if (Tent != null)
                            Tent.SetActive(true);

                        if (Construction != null)
                            Construction.SetActive(false);
                        if (Furniture != null)
                            Furniture.SetActive(true);
                    }
                    break;
                case EExternalHouseState.enabled:
                    {
                        dOnClick = OnClickEnabled;
                        if (Roof != null)
                            Roof.SetActive(false);
                        if (Tent != null)
                            Tent.SetActive(false);

                        if (Construction != null)
                            Construction.SetActive(false);
                        if (Furniture != null)
                            Furniture.SetActive(true);

                        onInitEnabled();
                    }
                    break;
            }
        }

        public override void OnClick()
        {
            if (UIController.get.drawer != null && UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }
            if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            dOnClick();
            //SoundsController.Instance.PlayButtonClick2();
        }

        public virtual void OnClickDisabled() { }

        public virtual void OnClickWaitingForRenew()
        {
            //tutaj otwarcie popupu pytającego o remont
            UIController.getHospital.RenovatePopUp.Open(roomInfo, () => confirmRenew());
        }

        public virtual void OnClickRenewing()
        {
            hover = ExternalBuildingHover.Open(this);
            hover.UpdateHover();
            hover.SetWorldPointHovering(hoverPoint.position);
            SoundsController.Instance.PlayConstruction();
        }

        public virtual void OnClickWaitingForUser()
        {
            if (!isUnwraping)
            {
                ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, roomInfo.roomName, ObjectiveRotatableEventArgs.EventType.Unwrap));
                Timing.RunCoroutine(DelayedUnwrap());
            }
        }

        public virtual void OnClickEnabled() { }

        public void OnClickSpeedUp(IDiamondTransactionMaker diamondTransactionMaker)
        {
            BuyWithDiamonds(diamondTransactionMaker);
        }

        protected virtual void BuyWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = DiamondCostCalculator.GetCostForBuilding(roomInfo.RenovatingTimeSeconds - TimeFromRenovationStarts, roomInfo.RenovatingTimeSeconds);
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.RenewBuilding);
                    timeFromRenovationStarts = roomInfo.RenovatingTimeSeconds;
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    Instantiate(ResourcesHolder.Get().ParticleDiamondBuilding, particleDiamondTransform.position, Quaternion.identity);
                    if (ExternalBuildingHover.activeHover != null)
                        ExternalBuildingHover.activeHover.Close();
                    ReferenceHolder.Get().engine.GetMap<AreaMapController>().ResetOntouchAction();
                    //progressBar = null;
                }, diamondTransactionMaker);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void onLvlUp()
        {
            if (roomInfo.UnlockLvl <= Game.Instance.gameState().GetHospitalLevel() && externalHouseState == EExternalHouseState.disabled)
            {
                GameState.OnLevelUp -= onLvlUp;
                ExternalHouseState = EExternalHouseState.waitingForRenew;
                //Init ();
            }
        }

        protected void confirmRenew()
        {
            if (roomInfo.costResource == ResourceType.Diamonds)            
                confirmRenewUsingDiamonds();
            else
                confirmRenewUsingCoins();
        }

        private void confirmRenewUsingCoins()
        {
            if (Game.Instance.gameState().GetCoinAmount() >= roomInfo.RenovationCost)
            {
                GameState.Get().RemoveCoins(roomInfo.RenovationCost, EconomySource.RenewBuilding);
                ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position + Vector3.up, roomInfo.RenovationCost, .1f, ReferenceHolder.Get().giftSystem.particleSprites[0]);

                StartUnlocking();
            }
            else
            {
                UIController.get.BuyResourcesPopUp.Open(roomInfo.RenovationCost - Game.Instance.gameState().GetCoinAmount(), () =>
                {
                    confirmRenew();
                }, null);
                MessageController.instance.ShowMessage(0);
            }
        }

        private void confirmRenewUsingDiamonds()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= roomInfo.RenovationCost)
            {
                GameState.Get().RemoveDiamonds(roomInfo.RenovationCost, EconomySource.RenewBuilding);
                ReferenceHolder.Get().giftSystem.CreateItemUsed(transform.position + Vector3.up, roomInfo.RenovationCost, .1f, ReferenceHolder.Get().giftSystem.particleSprites[1]);

                StartUnlocking();
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void StartUnlocking()
        {
            UIController.getHospital.LockedFeatureArtPopUpController.Exit();
            SoundsController.Instance.PlayConstruction();

            timeFromRenovationStarts = 0;
            ExternalHouseState = EExternalHouseState.renewing;
            hover = ExternalBuildingHover.Open(this);
            hover.SetWorldPointHovering(hoverPoint.position);
            NotificationCenter.Instance.VipRoomStartedBuilding.Invoke(null);
            ObjectiveNotificationCenter.Instance.RenovateSpecialObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, roomInfo.roomName));
            SaveSynchronizer.Instance.MarkToSave(SavePriorities.RenovationStarted);
            AnalyticsController.instance.ReportRenovate(ActionType.renovateStart, GetBuildingTag());
        }

        protected virtual void onInitEnabled() { }

        public float GetTimeToEndRenovation()
        {
            return roomInfo.RenovatingTimeSeconds - timeFromRenovationStarts;
        }

        bool updateHover = true;

        void Update()
        {
            if (externalHouseState == EExternalHouseState.renewing)
            {
                if (timeFromRenovationStarts >= roomInfo.RenovatingTimeSeconds)
                {
                    ExternalHouseState = EExternalHouseState.waitingForUser;
                    if (ExternalBuildingHover.activeHover != null)
                        ExternalBuildingHover.activeHover.Close();
                    //ReferenceHolder.Get().engine.GetMap<AreaMapController>().ResetOntouchAction();                    
                }
                else
                {
                    EmulateTime(Time.deltaTime);

                    if (timeFromRenovationStarts % 1 < 0.5f)
                    {
                        if (updateHover && hover != null)
                        {
                            hover.UpdateHover();
                            updateHover = false;
                        }
                    }
                    else
                    {
                        updateHover = true;
                    }
                }
            }
        }

        public virtual void EmulateTime(TimePassedObject timePassed)
        {
            if (externalHouseState == EExternalHouseState.renewing)
            {
                EmulateTime(timePassed.GetTimePassed());
            }
        }

        public void EmulateTime(float timePassed)
        {
            timeFromRenovationStarts += timePassed;
        }

        IEnumerator<float> Counting(EExternalHouseState state)
        {
            for (; ; )
            {
                /*if(PatchState == EPatchState.producing)
					SetPlantSprite ();*/
                if (hover != null)
                    hover.UpdateHover();

                if (timeFromRenovationStarts >= roomInfo.RenovatingTimeSeconds)
                {
                    ExternalHouseState = state;
                    if (ExternalBuildingHover.activeHover != null)
                        ExternalBuildingHover.activeHover.Close();
                    ReferenceHolder.Get().engine.GetMap<AreaMapController>().ResetOntouchAction();
                    /*if (hover != null) {
						hover.Initialize (this, state);
					}*/
                    break;
                }

                timeFromRenovationStarts++;
                yield return Timing.WaitForSeconds(1.0f);
            }
        }

        public virtual string SaveToString()
        {
            StringBuilder saveBuilder = new StringBuilder();
            saveBuilder.Append(Checkers.CheckedExternalRoomState(externalHouseState).ToString());

            saveBuilder.Append('$');
            saveBuilder.Append(Checkers.CheckedAmount((int)timeFromRenovationStarts, 0, roomInfo.RenovatingTimeSeconds, roomInfo.ExternalRoomType + " timeFromRenovationStarts: ").ToString());
            saveBuilder.Append('$');
            saveBuilder.Append(Checkers.CheckedAmount(actualLevel, -1, int.MaxValue, roomInfo.ExternalRoomType + " level: ").ToString());

            return saveBuilder.ToString();
        }

        public virtual void LoadFromString(string save, TimePassedObject saveTime)
        {
            ExternalHouseState = EExternalHouseState.disabled;

            if (string.IsNullOrEmpty(save))
            {
                GenerateDefault();
                return;
            }

            var saveDat = save.Split('$');
            timeFromRenovationStarts = Mathf.Clamp(int.Parse(saveDat[1], System.Globalization.CultureInfo.InvariantCulture) + (int)saveTime.GetTimePassed(), 0, roomInfo.RenovatingTimeSeconds);

            ExternalHouseState = (EExternalHouseState)Enum.Parse(typeof(EExternalHouseState), saveDat[0]);
            actualLevel = int.Parse(saveDat[2], System.Globalization.CultureInfo.InvariantCulture);
        }

        void GenerateDefault()
        {
            timeFromRenovationStarts = 0;
            actualLevel = 0;
            ExternalHouseState = EExternalHouseState.disabled;
            if (roomInfo.UnlockLvl <= GameState.Get().hospitalLevel && externalHouseState == EExternalHouseState.disabled)
            {
                GameState.OnLevelUp -= onLvlUp;
                ExternalHouseState = EExternalHouseState.waitingForRenew;
            }
        }

        protected IEnumerator<float> DelayedUnwrap()
        {
            isUnwraping = true;
            Tent.GetComponent<Animator>().SetTrigger("Unwrap");
            AnalyticsController.instance.ReportRenovate(ActionType.unwrap, GetBuildingTag());
            yield return Timing.WaitForSeconds(0.5f);

            ExternalHouseState = EExternalHouseState.enabled;
            Instantiate(ResourcesHolder.Get().ParticleUnpackVIP, particleTransform.position, Quaternion.identity);
            SoundsController.Instance.PlayCheering();

            int expReward = roomInfo.ExpRecived;
            int currentExpReward = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.RenewBuilding, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpReward);
            });
            isUnwraping = false;
        }

        public enum EExternalHouseState
        {
            disabled,
            waitingForRenew,
            renewing,
            waitingForUser,
            enabled
        }

        public enum ActionType
        {
            renovateStart,
            unwrap
        }

        public abstract string GetBuildingTag();
    }
}

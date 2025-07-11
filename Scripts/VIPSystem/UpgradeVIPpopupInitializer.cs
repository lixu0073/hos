using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using I2.Loc;

namespace Hospital
{
    public class UpgradeVIPpopupInitializer : BaseUIInitializer<UpgradeVIPPopupData, UpgradeVIPPopupController>
    {
        [SerializeField]
        [TermsPopup]
        private string upgradeVipWardTerm = "-";
        [SerializeField]
        [TermsPopup]
        private string upgradeVipHelipadTerm = "-";
        [SerializeField]
        [TermsPopup]
        private string vipWardTimerTerm = "-";
        [SerializeField]
        [TermsPopup]
        private string vipHelipadTimerTerm = "-";

        TabContentType contentType = TabContentType.upgradeVipHelipad;
        private bool canUseButton = false;

        public void Initialize(TabContentType contentType, Action OnSuccess, Action OnFailure)
        {
            base.Initialize(OnSuccess, OnFailure);

            this.contentType = contentType;

            UpgradeVIPPopupData data = PreparePopupData();

            if (data != null)
            {
                gameObject.SetActive(true);
                StartCoroutine(popupController.GetPopup().Open(true, false, () =>
                {
                    popupController.Initialize(data);

                    OnSuccess?.Invoke();
                }));
            }
            else
            {
                OnFailure?.Invoke();
            }
        }

        public override void DeInitialize()
        {
            base.DeInitialize();
        }

        protected override void AddPopupControllerRuntime()
        {
            popupController = gameObject.GetComponent<UpgradeVIPPopupController>();
        }

        protected override UpgradeVIPPopupData PreparePopupData()
        {
            canUseButton = true;
            switch (contentType)
            {
                case TabContentType.upgradeVipWard:
                    return PrepareUpgradeVipWardPopupData();
                case TabContentType.upgradeVipHelipad:
                    return PrepareUpgradeVipHelipadPopupData();
                default:
                    return null;
            }
        }

        protected override void Refresh(UpgradeVIPPopupData dataType)
        {
            popupController.RefreshDataWhileOpened(dataType);
        }

        private void ClosePopup()
        {
            DeInitialize();

            if (popupController != null)
                popupController.GetPopup().Exit();
        }
        
        private void OnSpeedupButtonClickedHelipad()
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            SpeedupButtonAction(((MasterableVIPHelipadConfigData)(vipSystemManager.heliMastership.MasterableConfigData)).MasteryPrices[vipSystemManager.heliMastership.MasteryLevel], () =>
            {
                vipSystemManager.heliMastership.SetMasteryProgress(vipSystemManager.heliMastership.MasteryProgress + vipSystemManager.heliMastership.MasteryGoal);
                vipSystemManager.UpgradeHeliRoomManually();
                ClosePopup();
            });
        }

        private void OnUpgradeButtonClickedHelipad()
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;
            UpgradeButtonAction(((MasterableVIPHelipadConfigData)(vipSystemManager.heliMastership.MasterableConfigData)).UpgradeCosts[vipSystemManager.heliMastership.MasteryLevel], EconomySource.VIPHelipadUpgrade, () =>
            {
                vipSystemManager.UpgradeHeliRoomManually();
                ClosePopup();
            });
        }

        private void OnSpeedupButtonClickedVIPWard()
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            SpeedupButtonAction(((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).MasteryPrices[vipSystemManager.vipMastership.MasteryLevel], () =>
            {
                vipSystemManager.vipMastership.SetMasteryProgress(vipSystemManager.vipMastership.MasteryProgress + vipSystemManager.vipMastership.MasteryGoal);
                vipSystemManager.UpgradeVipRoomManually();
                ClosePopup();
            });
        }

        private void OnUpgradeButtonClickedVIPWard()
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;
            UpgradeButtonAction(((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).UpgradeCosts[vipSystemManager.vipMastership.MasteryLevel], EconomySource.VIPWardUpgrade, () =>
            {
                vipSystemManager.UpgradeVipRoomManually();
                ClosePopup();
            });
        }
        

        private UpgradeVIPPopupData PrepareUpgradeVipWardPopupData()
        {
            UpgradeVIPPopupData data = new UpgradeVIPPopupData();

            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            data.titleTerm = upgradeVipWardTerm;
            data.upgradePanelsData = PrepareUpgradePanelsDataForVipWard();
            data.bookmarkActions = GetBookmarkActionsForVipWard();
            data.onCloseButtonClick = ClosePopup;
            data.toScroll = Mathf.Clamp(vipSystemManager.vipMastership.MasteryLevel + 1, 0, vipSystemManager.vipMastership.MasterableConfigData.MasteryGoals.Length);

            if (vipSystemManager.vipMastership.MasteryLevel < vipSystemManager.vipMastership.MasterableConfigData.MasteryGoals.Length)
            {
                data.strategy = new PossibleUpgradeVIPPopupViewStrategy();
                data.currentCuredVipsCount = vipSystemManager.vipMastership.MasteryProgress;
                data.requiredCuredVipsCount = vipSystemManager.vipMastership.MasteryGoal;
                data.toolsData = PrepareUpgradeToolsData(((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).UpgradeCosts[vipSystemManager.vipMastership.MasteryLevel]);

                if (vipSystemManager.vipMastership.IsUpgradeAvailable())
                {
                    data.isUpgradeBadgeVisible = vipSystemManager.HasToolsForUpgradeVipWard();
                    data.onUpgradeButtonClick = OnUpgradeButtonClickedVIPWard;
                }
                else
                {
                    data.onSpeedupButtonClick = OnSpeedupButtonClickedVIPWard;
                    data.speedupCost = ((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).MasteryPrices[vipSystemManager.vipMastership.MasteryLevel];
                }

            }
            else
            {
                data.strategy = new MaxedoutUpgradeVIPPopupViewStrategy();
            }

            return data;
        }

        private UpgradeVIPPopupData PrepareUpgradeVipHelipadPopupData()
        {
            UpgradeVIPPopupData data = new UpgradeVIPPopupData();

            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            data.titleTerm = upgradeVipHelipadTerm;
            data.upgradePanelsData = PrepareUpgradePanelsDataForVipHelipad();
            data.bookmarkActions = GetBookmarkActionsForVipHelipad();
            data.onCloseButtonClick = ClosePopup;
            data.toScroll = Mathf.Clamp(vipSystemManager.heliMastership.MasteryLevel + 1, 0, vipSystemManager.heliMastership.MasterableConfigData.MasteryGoals.Length);

            if (vipSystemManager.heliMastership.MasteryLevel < vipSystemManager.heliMastership.MasterableConfigData.MasteryGoals.Length)
            {
                data.strategy = new PossibleUpgradeVIPPopupViewStrategy();
                data.currentCuredVipsCount = vipSystemManager.heliMastership.MasteryProgress;
                data.requiredCuredVipsCount = vipSystemManager.heliMastership.MasteryGoal;
                data.toolsData = PrepareUpgradeToolsData(((MasterableVIPHelipadConfigData)(vipSystemManager.heliMastership.MasterableConfigData)).UpgradeCosts[vipSystemManager.heliMastership.MasteryLevel]);

                if (vipSystemManager.heliMastership.IsUpgradeAvailable())
                {
                    data.isUpgradeBadgeVisible = vipSystemManager.HasToolsForUpgradeVipHelipad();
                    data.onUpgradeButtonClick = OnUpgradeButtonClickedHelipad;
                }
                else
                {
                    data.onSpeedupButtonClick = OnSpeedupButtonClickedHelipad;
                    data.speedupCost = ((MasterableVIPHelipadConfigData)(vipSystemManager.heliMastership.MasterableConfigData)).MasteryPrices[vipSystemManager.heliMastership.MasteryLevel];
                }
            }
            else
            {
                data.strategy = new MaxedoutUpgradeVIPPopupViewStrategy();
            }

            return data;
        }

        private SingleUpgradePanelData[] PrepareUpgradePanelsDataForVipWard()
        {
            SingleUpgradePanelData[] data = new SingleUpgradePanelData[6];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new SingleUpgradePanelData();
                SetUpgradePanelsDataVipWardPanel(data[i], i);
            }

            return data;
        }

        private void SetUpgradePanelsDataVipWardPanel(SingleUpgradePanelData data, int level)
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            data.masterableLevel = level;
            data.timerLabelTerm = vipWardTimerTerm;
            if (level == 0)
            {
                data.time = vipSystemManager.vipCureTImeSecondsBase;
            }
            else
            {
                data.time = Mathf.RoundToInt(((MasterableVIPRoomConfigData)(vipSystemManager.vipMastership.MasterableConfigData)).PatientInBedTimeMultipliers[level-1] * vipSystemManager.vipCureTImeSecondsBase);
            }

            if (level <= vipSystemManager.vipMastership.MasteryLevel)
            {
                data.strategy = new UpgradedWardSingleUpgradePanelViewStrategy();
            }
            else if (level == vipSystemManager.vipMastership.MasteryLevel + 1)
            {
                data.strategy = new CurrentWardSingleUpgradePanelViewStrategy();
                data.setBadgeActive = vipSystemManager.vipMastership.IsUpgradeAvailable() && vipSystemManager.HasToolsForUpgradeVipWard();
            }
            else
            {
                data.strategy = new LockedWardSingleUpgradePanelViewStrategy();
            }
        }

        private SingleUpgradePanelData[] PrepareUpgradePanelsDataForVipHelipad()
        {
            SingleUpgradePanelData[] data = new SingleUpgradePanelData[6];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new SingleUpgradePanelData();
                SetUpgradePanelsDataVipHeliPanel(data[i], i);
            }

            return data;
        }

        private void SetUpgradePanelsDataVipHeliPanel(SingleUpgradePanelData data, int level)
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            data.masterableLevel = level;
            data.timerLabelTerm = vipHelipadTimerTerm;
            if (level == 0)
            {
                data.time = vipSystemManager.arriveIntervalSecondsBase;
            }
            else
            {
                data.time = Mathf.RoundToInt(((MasterableVIPHelipadConfigData)(vipSystemManager.heliMastership.MasterableConfigData)).VipArrivalTimeMultipliers[level-1] * vipSystemManager.arriveIntervalSecondsBase);
            }

            if (level <= vipSystemManager.heliMastership.MasteryLevel)
            {
                data.strategy = new UpgradedHeliSingleUpgradePanelViewStrategy();
            }
            else if (level == vipSystemManager.heliMastership.MasteryLevel + 1)
            {
                data.strategy = new CurrentHeliSingleUpgradePanelViewStrategy();
                data.setBadgeActive = vipSystemManager.heliMastership.IsUpgradeAvailable() && vipSystemManager.HasToolsForUpgradeVipHelipad();
            }
            else
            {
                data.strategy = new LockedHeliSingleUpgradePanelViewStrategy();
            }
        }

        private UpgradeToolPanelData[] PrepareUpgradeToolsData(KeyValuePair<MedicineRef, int>[] requiredMedicines)
        {
            UpgradeToolPanelData[] data = new UpgradeToolPanelData[requiredMedicines.Length];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = new UpgradeToolPanelData()
                {
                    toolSprite = ResourcesHolder.Get().GetSpriteForCure(requiredMedicines[i].Key),
                    currentAmount = GameState.Get().GetCureCount(requiredMedicines[i].Key),
                    requiredAmount = requiredMedicines[i].Value,
                };
            }

            return data;
        }

        private UnityAction[] GetBookmarkActionsForVipWard()
        {
            UnityAction[] actions = new UnityAction[2];

            actions[0] = null;
            actions[1] = () =>
            {
                contentType = TabContentType.upgradeVipHelipad;
                Refresh(PreparePopupData());
                NotificationCenter.Instance.VipUpgradeTutorial4Closed.Invoke(new BaseNotificationEventArgs());
            };
            return actions;
        }

        private UnityAction[] GetBookmarkActionsForVipHelipad()
        {
            UnityAction[] actions = new UnityAction[2];

            actions[0] = () =>
            {
                contentType = TabContentType.upgradeVipWard;
                Refresh(PreparePopupData());
                NotificationCenter.Instance.VipUpgradeTutorial4Closed.Invoke(new BaseNotificationEventArgs());
            };
            actions[1] = null;
            return actions;
        }

        public enum TabContentType
        {
            upgradeVipWard = 0,
            upgradeVipHelipad = 1,
        }

        private void UpgradeButtonAction(KeyValuePair<MedicineRef, int>[] requiredTools, EconomySource economySource, Action onApprove)
        {
            if (!canUseButton)
            {
                return;
            }
            canUseButton = false;

            IGameState gs = Game.Instance.gameState();
            List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

            for (int i = 0; i < requiredTools.Length; ++i)
            {
                int itemDifference = requiredTools[i].Value - gs.GetCureCount(requiredTools[i].Key);

                if (itemDifference > 0)
                {
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(itemDifference, ResourcesHolder.Get().medicines.cures[(int)requiredTools[i].Key.type].medicines[requiredTools[i].Key.id]));
                }
            }

            if (missingMedicines.Count > 0)
            {
                UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                {
                    for (int i = 0; i < requiredTools.Length; ++i)
                    {
                        gs.GetCure(requiredTools[i].Key, requiredTools[i].Value, economySource);
                    }

                    onApprove?.Invoke();

                    ClosePopup();
                
                }, () => { canUseButton = true;}, null);
            }
            else
            {
                for (int i = 0; i < requiredTools.Length; ++i)
                {
                    gs.GetCure(requiredTools[i].Key, requiredTools[i].Value, economySource);
                }

                onApprove?.Invoke();

                ClosePopup();
            }
        }

        private void SpeedupButtonAction(int price, UnityAction onApprove)
        {
            if (!canUseButton)
            {
                return;
            }
            canUseButton = false;

            IGameState gs = Game.Instance.gameState();

            if (gs.GetDiamondAmount() >= price)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(price, delegate
                {
                    gs.RemoveDiamonds(price, EconomySource.MissingResourcesStorage);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(Input.mousePosition, price, 0f, ReferenceHolder.Get().giftSystem.particleSprites[1], false);

                    onApprove?.Invoke();
                }, popupController.GetPopup());
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                canUseButton = true;
            }
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using IsoEngine;
using System.Linq;
using System;

namespace Hospital
{
    public abstract class MasterableProperties
    {

        #region fields
        public MasterablePropertiesClient masterableClient;
        protected MastershipRoomAppearance appearanceController;
        private int masteryLevel = 0;
        private int masteryProgress = 0;
        private int masteryGoal = 0;
        protected bool sendEventToDelta = false;
        #endregion
        #region properties
        public int MasteryLevel
        {
            get { return masteryLevel; }
            private set
            {
                if (value < 0)
                    return;
                masteryLevel = value;
            }
        }
        public int MasteryProgress
        {
            get { return masteryProgress; }
            protected set
            {
                if (value < 0)
                {
                    return;
                }
                masteryProgress = value;
            }
        }
        public int MasteryGoal
        {
            get { return masteryGoal; }
            private set
            {
                if (value < 0)
                {
                    return;
                }
                masteryGoal = value;
            }
        }
        public MasterableConfigData MasterableConfigData
        {
            get { return masterableConfigData; }
            set { masterableConfigData = value; }
        }
        protected MasterableConfigData masterableConfigData;
        public float CoinRewardMultiplier { get { return coinRewardMultiplier; } private set { } }
        public float ExpRewardMultiplier { get { return expRewardMultiplier; } private set { } }
        public float ProductionTimeMultiplier { get { return productionTimeMultiplier; } private set { } }
        protected float coinRewardMultiplier;
        protected float expRewardMultiplier;
        protected float productionTimeMultiplier;
        #endregion
        #region constructors
        public MasterableProperties(MasterablePropertiesClient clientInfo)
        {
            if (Game.Instance.gameState().GetHospitalLevel() < 9)
            {
                BaseGameState.OnLevelUp += BaseGameState_OnLevelUp;
            }
        }
        #endregion

        #region publicMethods
        public void RefreshMasteryView(bool showAnimation = true)
        {
            SetAppearanceController();
            if (appearanceController == null)
            {
                return;
            }
            appearanceController.SetAppearance(MasteryLevel, showAnimation);
        }
        public void IsoDestroy()
        {
            BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
        }
        public virtual void AddMasteryProgress(int amount)
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }
            if (amount <= 0)
            {
                return;
            }

            if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
            {
                SetMasteryProgress(MasteryGoal);
                return;
            }

            MasteryProgress += amount;
            CheckMasteryProgress();
            RefreshInfoPopup();
        }
        public void BuyUpgrade(IDiamondTransactionMaker diamondTransactionMaker, Action onSuccess = null, Action onFailure = null)
        {
            int cost = CalcSpeedUpPrice();

            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(cost, EconomySource.MastershipUpgrade, masterableClient.GetClientTag());
                    sendEventToDelta = false;
                    SetMasteryProgress(masteryGoal);
                    sendEventToDelta = true;
                    onSuccess?.Invoke();
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    AnalyticsController.instance.ReportMastershipGain(masterableClient.GetClientTag(), true, MasteryLevel);
                }, diamondTransactionMaker);
            }
            else
            {
                onFailure?.Invoke();
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }
        public int CalcSpeedUpPrice()
        {
            if (masteryGoal == 0)
            {
                return 0;
            }

            if (masteryLevel > masterableConfigData.MasteryPrices.Length)
            {
                return 0;
            }
            return masterableConfigData.MasteryPrices[masteryLevel] * (masteryGoal - masteryProgress) / masteryGoal;
        }
        public void InitializeFromSave(string save, Rotations info, TimePassedObject timePassed)
        {
            //Init(info);
            //base.InitializeFromSave(save, info, timePassed);
        }
        public string SaveToStringMastership()
        {
            StringBuilder save = new StringBuilder();
            save.Append(MasteryLevel.ToString());
            save.Append("/");
            save.Append(MasteryProgress.ToString());
            return save.ToString();
        }
        public virtual void LoadFromString(string save, int actionsDone = 0)
        {
            var str = save.Split(';');
            sendEventToDelta = false;
            if (str.Length > 2)
            {
                List<string> strs = str[2].Split('/').Where(p => p.Length > 0).ToList();
                if (strs.Count > 1)
                {
                    SetMasteryLevel(int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture));
                    SetMasteryProgress(int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture));
                }
                else
                {
                    SetMasteryLevel(0);
                    SetMasteryProgress(actionsDone);
                }
            }
            else
            {
                SetMasteryLevel(0);
                SetMasteryProgress(actionsDone);
            }
            sendEventToDelta = true;
        }
        public virtual void SetMasteryProgress(int progress)
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (MasteryGoal <= 0)
            {
                return;
            }

            if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
            {
                MasteryProgress = MasteryGoal;
                return;
            }

            if (progress < 0)
            {
                return;
            }
            MasteryProgress = progress;
            CheckMasteryProgress();
            RefreshInfoPopup();
            
        }
        public void SetMasteryLevel(int level)
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (level < 0)
            {
                SetMasteryLevel(0);
                return;
            }
            if (level > MasterableConfigData.MasteryGoals.Length)
            {
                SetMasteryLevel(MasterableConfigData.MasteryGoals.Length);
                return;
            }

            MasteryLevel = level;
            UpdateMasteryGoal();
            UpdateMasteryMultipliers();
            RefreshMasteryView(false);
            RefreshInfoPopup();
        }
        public virtual void CheckMasteryProgress()
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (MasteryGoal <= 0)
            {
                return;
            }

            if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
            {
                return;
            }

            if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                return;
            }

            if (IsUpgradeAvailable())
            {
                int progressLeft = MasteryProgress - MasteryGoal;
                AddMasteryLevel(1);
                SetMasteryProgress(progressLeft);
            }
        }

        public bool IsMaxed()
        {
            return MasteryLevel >= MasterableConfigData.MasteryGoals.Length;
        }

        public bool IsUpgradeAvailable()
        {
            if (IsMaxed())
            {
                return false;
            }

            return MasteryProgress >= MasteryGoal;
        }
        #endregion

        #region privateMethods
        protected void RefreshInfoPopup()
        {
            if (UIController.getHospital.HospitalInfoPopUp.gameObject.activeInHierarchy)
            {
                UIController.getHospital.HospitalInfoPopUp.RefreshPopup();
            }
        }
        protected void AddMasteryLevel(int amount)
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (amount <= 0)
            {
                return;
            }
            if (MasteryLevel + amount > MasterableConfigData.MasteryGoals.Length)
            {
                SetMasteryLevel(MasterableConfigData.MasteryGoals.Length);
                return;
            }
            MasteryLevel += amount;
            UpdateMasteryGoal();
            UpdateMasteryMultipliers();
            RefreshMasteryView();
            UpdateHoverStars();
            RefreshInfoPopup();
            if (sendEventToDelta)
            {
                AnalyticsController.instance.ReportMastershipGain(masterableClient.GetClientTag(), false, MasteryLevel);
            }
        }
        protected void Init(MasterablePropertiesClient client)
        {
            this.masterableClient = client;
            if (MasterySystemParser.Instance == null)
            {
                Debug.LogError("MasterySystemParser.Instance is null");
                return;
            }
            MasterableConfigData = MasterySystemParser.Instance.GetMasterableConfigData(this.masterableClient.GetClientTag());
            if (masterableConfigData == null)
            {
                Debug.LogError("masterableConfigData is null");
            }
            SetMasteryLevel(0);
            SetMasteryProgress(0);
        }
        protected MastershipRoomAppearance GetAppearanceController<Type>(GameObject controllerOwner) where Type : MastershipRoomAppearance
        {
            MastershipRoomAppearance controller = controllerOwner.GetComponent<Type>();
            return controller;
        }

        private void BaseGameState_OnLevelUp()
        {
            if (Game.Instance.gameState().GetHospitalLevel() == 9)
            {
                CheckMasteryProgress();
                BaseGameState.OnLevelUp -= BaseGameState_OnLevelUp;
            }
        }
        private void UpdateMasteryGoal()
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
            {
                MasteryGoal = MasterableConfigData.MasteryGoals[MasterableConfigData.MasteryGoals.Length - 1];
                return;
            }
            MasteryGoal = MasterableConfigData.MasteryGoals[MasteryLevel];
        }
        private void UpdateHoverStars()
        {
            if (UIController.get.ActiveHover != null && UIController.get.ActiveHover is SimpleUI.MastershipHover)
            {
                ((SimpleUI.MastershipHover)UIController.get.ActiveHover).UpdateStars();
            }
        }
        #endregion

        #region abstractMethods
        protected abstract void SetAppearanceController();
        protected abstract void UpdateMasteryMultipliers();
        public abstract int CalcTimeToMastershipUpgrade();
        #endregion
    }
}

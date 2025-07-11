using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

namespace Hospital
{
    /// <summary>
    /// 增益道具面板控制器，负责管理单个增益道具卡片的显示和交互。
    /// 处理增益道具的信息显示、数量更新、购买和使用操作等功能。
    /// </summary>
    public class BoosterPanelController : MonoBehaviour, IDiamondTransactionMaker
    {
        [SerializeField]
        private Image boosterIcon = null;
        [SerializeField]
        private TextMeshProUGUI boosterInfo = null;
        [SerializeField]
        private TextMeshProUGUI boosterExtendedInfo = null;
        [SerializeField]
        private TextMeshProUGUI boosterAmountText = null;
        [SerializeField]
        private TextMeshProUGUI boosterPriceText = null;
        [SerializeField]
        private Animator boosterExtendedInfoAnim = null;
        [SerializeField]
        private GameObject buttonUseBooster = null;
        [SerializeField]
        private GameObject buttonGetBooster = null;

        public GameObject badge;

        private int boosterID = -1;
        private Guid ID;

        public void Refresh(int boosterID)
        {
            InitializeID();
            this.boosterID = boosterID;

            int boosterAmount = HospitalAreasMapController.HospitalMap.boosterManager.boosterStorage[boosterID];
            boosterAmountText.text = boosterAmount.ToString();
            if (boosterAmount > 0)
            {
                buttonUseBooster.SetActive(true);
                buttonGetBooster.SetActive(false);
            }
            else
            {
                if (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].canBuy)
                {
                    boosterPriceText.text = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].Price.ToString();
                    buttonUseBooster.SetActive(false);
                    buttonGetBooster.SetActive(true);
                }
                else
                {
                    buttonUseBooster.SetActive(false);
                    buttonGetBooster.SetActive(false);
                }
            }
        }

        public void FillCard(int boosterID)
        {
            this.boosterID = boosterID;
            BoosterType boosterType;
            BoosterTarget boosterTarget;
            boosterType = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterType;
            boosterTarget = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].boosterTarget;

            boosterInfo.SetText(I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].shortInfo));
            boosterExtendedInfoAnim.SetBool("Show", false);
            boosterIcon.sprite = (ResourcesHolder.Get().boosterDatabase.boosters[boosterID].icon);
            boosterAmountText.text = HospitalAreasMapController.HospitalMap.boosterManager.boosterStorage[boosterID].ToString();
            if (HospitalAreasMapController.HospitalMap.boosterManager.newBoosters[boosterID])
            {
                badge.SetActive(true);
            }
            else
            {
                badge.SetActive(false);
            }
        }

        public void UseBooster()
        {
            if (HospitalAreasMapController.HospitalMap.boosterManager.boosterActive)
            {
                UIController.getHospital.BoosterMenuPopUp.Exit();
                MessageController.instance.ShowMessage(27);
                return;
            }

            if (HospitalAreasMapController.HospitalMap.boosterManager.boosterStorage[boosterID] > 0)
            {
                UIController.getHospital.BoosterMenuPopUp.Exit();
                HospitalAreasMapController.HospitalMap.boosterManager.SetBooster(boosterID, ResourcesHolder.Get().boosterDatabase.boosters[boosterID].duration);
                HospitalAreasMapController.HospitalMap.boosterManager.boosterStorage[boosterID]--;
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Booster.ToString(), (int)FunnelStepBoosters.Activated, FunnelStepBoosters.Activated.ToString());
                SaveSynchronizer.Instance.InstantSave();
            }
            else
            {
                GetBoosterForDiamonds();
            }
        }


        public void GetBoosterForDiamonds()
        {
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Booster.ToString(), (int)FunnelStepBoosters.GetBooster, FunnelStepBoosters.GetBooster.ToString());
            int cost = ResourcesHolder.Get().boosterDatabase.boosters[boosterID].Price;

            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.GetBooster);

                    HospitalAreasMapController.HospitalMap.boosterManager.AddBooster(boosterID, EconomySource.GetBooster, false);
                    HospitalAreasMapController.HospitalMap.boosterManager.SetBooster(boosterID, ResourcesHolder.Get().boosterDatabase.boosters[boosterID].duration);
                    HospitalAreasMapController.HospitalMap.boosterManager.boosterStorage[boosterID]--;

                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.Booster.ToString(), (int)FunnelStepBoosters.Purchased, FunnelStepBoosters.Purchased.ToString());
                    UIController.getHospital.BoosterMenuPopUp.Exit();
                    SaveSynchronizer.Instance.InstantSave();
                }, this);

            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void OnInfoButtonDown()
        {
            boosterExtendedInfoAnim.SetBool("Show", true);
            boosterExtendedInfo.SetText(I2.Loc.ScriptLocalization.Get(ResourcesHolder.Get().boosterDatabase.boosters[boosterID].info));
        }

        public void OnInfoButtonUP()
        {
            boosterExtendedInfoAnim.SetBool("Show", false);
        }

        public void InitializeID()
        {
            ID = Guid.NewGuid();
        }

        public Guid GetID()
        {
            return ID;
        }

        public void EraseID()
        {
            ID = Guid.Empty;
        }
    }
}
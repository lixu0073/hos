using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MovementEffects;
using System;
using SimpleUI;

namespace Hospital
{
    /// <summary>
    /// 宝箱管理器抽象基类，定义宝箱系统的核心功能接口和通用逻辑。
    /// 负责管理宝箱奖励生成、开箱动画、IAP购买流程和装饰物品展示等功能。
    /// </summary>
    public abstract class CasesManager : MonoBehaviour
    {
        public delegate void OnSave();
        public event OnSave OnLootBoxIAPSave;

        [HideInInspector] public List<IAPCaseData> iapStack = new List<IAPCaseData>();
        [HideInInspector] public bool GiftFromIAP = false;
        [HideInInspector] public CasePrize casePrize = null;
        public List<ShopRoomInfo> decorationsToDraw;
        [SerializeField] protected Sprite[] caseSprites = new Sprite[3];
        public Sprite[] openBoxSprites = new Sprite[3];
        public Sprite[] openCoverSprites = new Sprite[3];

        protected CasePrizeGenerator casePrizeGenerator;
        protected float prizeMoveDuration = 0.75f;

        //[SerializeField] private GameObject boxButton = null;
        public List<GiftReward> lootBoxRewards = new List<GiftReward>();

        public abstract string SaveToString();
        public abstract void LoadFromString(string saveString, TimePassedObject timeFromSave, bool VisitingMode);
        protected abstract void RefreshCases(bool refreshStorage = true);
        public Sprite goldStack;
        public Sprite diamondsChest;
        protected bool canAddSpecial = true;

        #region unboxing methods

        public void OpenBox()
        {
            UIController.getHospital.unboxingPopUp.OpenBoxAnim();
            SoundsController.Instance.PlayBoxOpen();

            if (HospitalAreasMapController.HospitalMap.casesManager.GiftFromIAP)
            {
                HospitalAreasMapController.HospitalMap.casesManager.GivePrizesAndSave();
                HospitalAreasMapController.HospitalMap.casesManager.NotifyLootBoxIAPSave();
            }
        }

        public abstract void GivePrizesAndSave();

        public abstract void CardClicked();

        public abstract void ChooseCardType();

        public abstract void GivePrizeInstantly(CasePrize caseprize);

        public abstract void ShowPrizeAnimation(bool fromGlobalEvent = false);

        public abstract void ShowGiftRewardAnimation();

        public abstract void OpenUnboxingPopUp();

        public void NotifyLootBoxIAPSave()
        {
            OnLootBoxIAPSave?.Invoke();
        }

        public Sprite GetCaseSprite(int id)
        {
            if (caseSprites == null || id < 0 || id > caseSprites.Length - 1)
                return null;

            return caseSprites[id];
        }
        #endregion


    }

}
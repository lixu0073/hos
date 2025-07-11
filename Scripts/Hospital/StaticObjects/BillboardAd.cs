using UnityEngine;
using System.Collections;
using SimpleUI;
using System;

namespace Hospital
{
    public class BillboardAd : SuperObject
    {
        public Animator anim;
        public SpriteRenderer sr;
        public Sprite spriteAvailable;
        public Sprite spriteUnavailable;

        public ShopRoomInfo decorationReward;

        bool isAvailable;
        bool isAdOnDailyLimit = false;
            
        void Start()
        {
            Invoke("CheckBillboardAvailability", 5f);
        }

        void CheckBillboardAvailability()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < 5)
            {
                isAvailable = false;
                return;
            }
            isAvailable = AdsController.instance.IsAdAvailable(AdsController.AdType.rewarded_ad_billboard);
            isAdOnDailyLimit = AdsController.instance.IsAdOnDailyLimit(AdsController.AdType.rewarded_ad_billboard);
            SetSprite();

            CancelInvoke();
            Invoke("CheckBillboardAvailability", 2f);
        }

        public void SetSprite()
        {
            if (isAvailable && !visitingMode)
            {
                try
                {
                    sr.sprite = spriteAvailable;
                    anim.Play(AnimHash.BillboardActive, 0, 0.0f);
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
                    sr.sprite = spriteUnavailable;
                    anim.Play(AnimHash.BillboardInactive, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
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

            if (!visitingMode)
            {
                if(isAdOnDailyLimit)
                {
                    ShowErrorDailyLimitReached();
                    CheckBillboardAvailability();
                    return;
                }
                if (isAvailable)
                {
                    StartCoroutine(UIController.getHospital.BillboardAdPopUp.Open(true, false, () =>
                    {
                        AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.BillboardAd.ToString(), (int)FunnelStepBillboardAd.PopUpOpen, FunnelStepBillboardAd.PopUpOpen.ToString());
                    }));
                }
                else
                {
                    ShowError();
                    CheckBillboardAvailability();
                }
            }
        }

        public void ShowRewardedAd()
        {
            isAvailable = false;
            AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_billboard);
            SetSprite();
        }

        void ShowErrorDailyLimitReached()
        {
            MessageController.instance.ShowMessage(65);
        }

        void ShowError()
        {
            MessageController.instance.ShowMessage(36);
        }

        public void RewardPlayer(bool onlyDiamonds)
        {
            UIController.getHospital.BillboardAdPopUp.ShowRewardContent();
            CheckBillboardAvailability();
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.BillboardAd.ToString(), (int)FunnelStepBillboardAd.RewardAwarded, FunnelStepBillboardAd.RewardAwarded.ToString());
        }

        public void RewardPlayerCoin()
        {
            UIController.getHospital.BillboardAdPopUp.ShowRewardContentCoin();
        }

        public override void IsoDestroy()
        {
        }
    }
}

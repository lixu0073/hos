using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System;

namespace Hospital
{
    public class BillboardAdPopUp : UIElement
    {
        public TextMeshProUGUI[] rewardAmountTexts;
        public Button confirmButton = null;
        public GameObject contentBeforeAd;
        public GameObject contentReward;
        public GameObject watchAdButton;
#pragma warning disable 0649
        [SerializeField] private GameObject TitleCinema;
        [SerializeField] private GameObject TitleCoins;
        [SerializeField] private GameObject contentNoVideoCoin;
        [SerializeField] private GameObject contentBeforeAdCoin;
        [SerializeField] private GameObject contentRewardCoin;
#pragma warning restore 0649
        public Button collectRewardButton;
#pragma warning disable 0649
        [SerializeField] private Button confirmButtonCoin;
        [SerializeField] private Button collectRewardButtonCoin;
        [SerializeField] private Button noAdButton;
#pragma warning restore 0649
        [SerializeField] private TextMeshProUGUI coinsToGet0 = null;
        [SerializeField] private TextMeshProUGUI coinsToGet1 = null;

        int coinReward = 0;
        //bool coinsContent = false;

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers);

            TitleCinema.SetActive(true);
            TitleCoins.SetActive(false);
            confirmButton.interactable = true;
            confirmButtonCoin.interactable = false;
            contentBeforeAd.SetActive(true);
            contentReward.SetActive(false);
            contentNoVideoCoin.SetActive(false);
            contentBeforeAdCoin.SetActive(false);
            contentRewardCoin.SetActive(false);
            watchAdButton.SetActive(true);
            confirmButtonCoin.gameObject.SetActive(false);
            noAdButton.gameObject.SetActive(false);
            collectRewardButton.gameObject.SetActive(false);
            collectRewardButtonCoin.gameObject.SetActive(false);
            collectRewardButton.interactable = false;
            collectRewardButtonCoin.interactable = false;
            //coinsContent = false;

            SetRewardAmountTexts();

            SoundsController.Instance.PlayButtonClick(false);

            whenDone?.Invoke();
        }

        void SetRewardAmountTexts()
        {
            int rewardAmount = AdsController.instance.GetRewardAmount(AdsController.AdType.rewarded_ad_billboard);
            for (int i = 0; i < rewardAmountTexts.Length; ++i)
            {
                rewardAmountTexts[i].text = "+" + rewardAmount;
            }
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            if (!IsVisible)
                return;

            if (collectRewardButton.interactable)
            {
                ButtonCollectReward();
                return; //ButtonCollectReward will disable interactable and call Exit() again
            }

            if (collectRewardButtonCoin.interactable)
            {
                ButtonCollectRewardCoin();
                return; //ButtonCollectReward will disable interactable and call Exit() again
            }

            if (UIController.get.IAPShopUI.isActiveAndEnabled)
                UIController.get.IAPShopUI.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);
            // if (coinsContent) {
            //     UIController.get.AddCurrencyPopUp.Open(2);
            // }
        }

        public void ShowRewardContent()
        {
            //coinsContent = false;
            TitleCinema.SetActive(true);
            TitleCoins.SetActive(false);

            contentBeforeAd.SetActive(false);
            contentReward.SetActive(true);

            confirmButton.gameObject.SetActive(false);
            confirmButtonCoin.gameObject.SetActive(false);

            collectRewardButton.gameObject.SetActive(true);
            collectRewardButton.interactable = true;
            collectRewardButtonCoin.gameObject.SetActive(false);
            collectRewardButtonCoin.interactable = false;

            noAdButton.gameObject.SetActive(false);
        }

        public void ShowNoVideoContentCoin()
        {
            //coinsContent = true;
            TitleCinema.SetActive(false);
            TitleCoins.SetActive(true);

            contentBeforeAd.SetActive(false);
            contentReward.SetActive(false);

            contentNoVideoCoin.SetActive(true);
            contentBeforeAdCoin.SetActive(false);
            contentRewardCoin.SetActive(false);

            confirmButton.gameObject.SetActive(false);
            confirmButtonCoin.gameObject.SetActive(false);

            collectRewardButton.gameObject.SetActive(false);
            collectRewardButtonCoin.gameObject.SetActive(false);

            noAdButton.gameObject.SetActive(true);
        }

        public void ShowBeforeAdContentCoin()
        {
            //currently we don't give coins for billboard ad, and probably never will.
            //coinsContent = true;
            coinReward = AdsController.instance.GetRewardAmount(AdsController.AdType.rewarded_ad_coins);
            coinsToGet0.SetText("+" + coinReward.ToString());
            coinsToGet1.SetText("+" + coinReward.ToString());
            TitleCinema.SetActive(false);
            TitleCoins.SetActive(true);

            contentBeforeAd.SetActive(false);
            contentReward.SetActive(false);

            contentNoVideoCoin.SetActive(false);
            contentBeforeAdCoin.SetActive(true);
            contentRewardCoin.SetActive(false);

            confirmButton.gameObject.SetActive(false);
            confirmButton.interactable = false;
            confirmButtonCoin.gameObject.SetActive(true);
            confirmButtonCoin.interactable = true;

            collectRewardButton.gameObject.SetActive(false);
            collectRewardButtonCoin.gameObject.SetActive(false);

            noAdButton.gameObject.SetActive(false);
        }

        public void ShowRewardContentCoin()
        {
            //currently we don't give coins for billboard ad, and probably never will.
            //coinsContent = true;
            coinReward = AdsController.instance.GetRewardAmount(AdsController.AdType.rewarded_ad_coins);
            coinsToGet0.SetText("+" + coinReward.ToString());
            coinsToGet1.SetText("+" + coinReward.ToString());
            TitleCinema.SetActive(false);
            TitleCoins.SetActive(true);

            contentBeforeAd.SetActive(false);
            contentReward.SetActive(false);

            contentNoVideoCoin.SetActive(false);
            contentBeforeAdCoin.SetActive(false);
            contentRewardCoin.SetActive(true);

            confirmButton.gameObject.SetActive(false);
            confirmButtonCoin.gameObject.SetActive(false);

            collectRewardButton.gameObject.SetActive(false);
            collectRewardButton.interactable = false;
            collectRewardButtonCoin.gameObject.SetActive(true);
            collectRewardButtonCoin.interactable = true;

            noAdButton.gameObject.SetActive(false);
        }

        public void ButtonConfirm()
        {
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.BillboardAd.ToString(), (int)FunnelStepBillboardAd.WatchVideoClicked, FunnelStepBillboardAd.WatchVideoClicked.ToString());
            ReferenceHolder.GetHospital().BillboardAd.ShowRewardedAd();
            confirmButton.interactable = false;
        }

        public void ButtonConfirmCoin()
        {
            AdsController.instance.ShowAd(AdsController.AdType.rewarded_ad_coins);
        }

        public void ButtonNoAd()
        {
            Exit();
        }

        public void ButtonCollectReward()
        {
            Debug.Log("Rewarding player with a diamond");
            int rewardAmount = AdsController.instance.GetRewardAmount(AdsController.AdType.rewarded_ad_billboard);
            collectRewardButton.interactable = false;
            int currentDiamondAmount = Game.Instance.gameState().GetDiamondAmount();
            Game.Instance.gameState().AddResource(ResourceType.Diamonds, rewardAmount, EconomySource.RewardedVideo, false);
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.TapOnAddBillboard));
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Diamond, Vector3.zero, rewardAmount, .25f, 1.75f, Vector3.one, Vector3.one, null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Diamonds, rewardAmount, currentDiamondAmount);
            });

            Exit();
        }

        public void ButtonCollectRewardCoin()
        {
            collectRewardButtonCoin.interactable = false;
            int currentDiamondAmount = Game.Instance.gameState().GetCoinAmount();
            Game.Instance.gameState().AddCoins(coinReward, EconomySource.RewardedVideo, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, Vector3.zero, coinReward, 0f, 2f, new Vector3(2f, 2f, 1), new Vector3(1, 1, 1), IAPController.instance.coinsSprite, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinReward, currentDiamondAmount);
            });
            Exit();
        }

        public void ButtonExit()
        {
            Exit();
        }
    }
}

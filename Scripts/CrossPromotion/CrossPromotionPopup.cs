using UnityEngine;
using SimpleUI;
using System.Collections;
using System;

namespace Hospital
{
    public class CrossPromotionPopup : UIElement
    {
        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open();
            CrossPromotionController.instance.CrossPromotionStateChanged += OnCrossPromotionStateChanged;

            IGameState gameState = Game.Instance.gameState();
            if (!gameState.EverOpenedCrossPromotionPopup)
            {
                gameState.EverOpenedCrossPromotionPopup = true;
                SaveSynchronizer.Instance.InstantSave();
            }

            AnalyticsController.instance.ReportCrossPromotionOpenPopup();
            whenDone?.Invoke();
        }

        public void ButtonDownload()
        {
            AnalyticsController.instance.ReportCrossPromotionOpenStore();
#if UNITY_ANDROID
            Application.OpenURL("https://app.appsflyer.com/com.kuuhubb.tilesandtales?pid=myhospital&c=DiamondReward");
#elif UNITY_IOS
            Application.OpenURL("https://app.appsflyer.com/1471982948?pid=myhospital&c=DiamondReward");
#endif
            Exit();
        }

        public void ButtonExit()
        {
            Exit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            CrossPromotionController.instance.CrossPromotionStateChanged -= OnCrossPromotionStateChanged;
            base.Exit(hidePopupWithShowMainUI);
        }

        private void OnCrossPromotionStateChanged()
        {
            if (!CrossPromotionController.instance.ShouldShowCrossPromotion())
                Exit();
        }
    }
}
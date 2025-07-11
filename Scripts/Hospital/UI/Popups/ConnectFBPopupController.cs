using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using MovementEffects;
using System.Collections.Generic;
using System.Collections;

namespace Hospital
{
    public class ConnectFBPopupController : UIElement
    {
#pragma warning disable 0649
        [SerializeField] private Button signInButton;
        [SerializeField] private Button dontRemindButton;
#pragma warning restore 0649

        [SerializeField]
        private int diamondReward = 5;

        
        public static bool OpenIfPossible(/*MonoBehaviour ToLaunchCoroutine*/)
        {
            return VisitingController.Instance.IsVisiting; // CV: to remove FB presence in the game

            /*bool isOpenPossible = !VisitingController.Instance.IsVisiting && !Game.Instance.gameState().IsNoMoreRemindFBConnection() && !Game.Instance.gameState().IsFBConnectionRewardEnabled() && !Game.Instance.gameState().IsFBRewardConnectionClaimed() && !Game.Instance.gameState().IsEverLoggedInFB() && Game.Instance.gameState().IsHomePharmacyVisited();
            if(isOpenPossible)
            {
                ToLaunchCoroutine.StartCoroutine(UIController.getHospital.connectFBPopup.Open());
            }
            return isOpenPossible;*/
        }

        public IEnumerator Open()
        {
            yield return base.Open(true, true);
            signInButton.onClick.AddListener(delegate
            {
                ConnectWithFB();
                Exit();
            });
            dontRemindButton.onClick.AddListener(delegate
            {
                DontRemind();
                Exit();
            });
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            signInButton.onClick.RemoveAllListeners();
            dontRemindButton.onClick.RemoveAllListeners();
            base.Exit(hidePopupWithShowMainUI);
        }

        private void ConnectWithFB()
        {
            AccountManager.Instance.ConnectToFacebook(false , SocialEntryPoint.FBPopUp);
        }

        public void ExitAndOpenPharmacy()
        {
            Exit();
            UIController.getHospital.PharmacyPopUp.Open();
        }

        private void DontRemind()
        {
            Game.Instance.gameState().SetNoMoreRemindFBConnection(true);
            SaveSynchronizer.Instance.InstantSave();
            Exit();
            UIController.getHospital.PharmacyPopUp.Open();
        }

        public void OnFacebookConnected()
        {
            Game.Instance.gameState().SetFBConnectionRewardEnabled(true);
            Game.Instance.gameState().SetEverLoggedInFB(true);
            TryToShowFBConnectReward();
        }

        public static void TryToShowFBConnectReward()
        {
            if (Game.Instance.gameState().IsFBConnectionRewardEnabled() && !Game.Instance.gameState().IsFBRewardConnectionClaimed())
            {
                Game.Instance.gameState().SetEverLoggedInFB(true);
                SaveSynchronizer.Instance.InstantSave();
                UIController.getHospital.connectFBPopup.ShowFBConnectRewardCoroutine();
            }
        }

        private void ShowFBConnectRewardCoroutine()
        {
            Timing.KillCoroutine(ShowFBConnectReward().GetType());
            Timing.RunCoroutine(ShowFBConnectReward());
        }

        private IEnumerator<float> ShowFBConnectReward()
        {
            UIController.get.ExitAllPopUps();
            UIController.get.CloseActiveHover();
            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
            }

            yield return Timing.WaitForSeconds(0.1f);

            while (TutorialController.Instance.tutorialEnabled && TutorialUIController.Instance.IsFullscreenActive())
                yield return 0;

            Vector3 startPoint = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ReferenceHolder.Get().engine.MainCamera.GetCamera().nearClipPlane));

            int currentAmount = Game.Instance.gameState().GetDiamondAmount();
            GameState.Get().AddResource(ResourceType.Diamonds, diamondReward, EconomySource.FacebookConnected, false);
            Game.Instance.gameState().SetFBRewardConnectionClaimed(true);
            SaveSynchronizer.Instance.InstantSave();
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Diamond, startPoint, diamondReward, 0f, 2f, new Vector3(1.2f, 1.2f, 1), new Vector3(0.9f, 0.9f, 1), ReferenceHolder.Get().giftSystem.particleSprites[1], null, () => {
                GameState.Get().UpdateCounter(ResourceType.Diamonds, diamondReward, currentAmount);
            });
            // UIController.get.unboxingPopUp.OpenFacebookRewardCasePopup();
        }
        
    }
}
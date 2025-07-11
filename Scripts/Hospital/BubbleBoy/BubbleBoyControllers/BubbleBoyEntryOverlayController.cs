using UnityEngine;
using System.Collections.Generic;
using MovementEffects;


namespace Hospital
{
    public class BubbleBoyEntryOverlayController : ExternalRoom
    {
        //private int timeToNextSession = 0;
        private Vector3 firstMousePos;
        private float touchTime;
        private bool firstTap = true;

        [TutorialTriggerable]
        public void SetWaitingForRenew()
        {
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_available, true))
                return;
            if (ExternalHouseState != EExternalHouseState.waitingForRenew && ExternalHouseState != EExternalHouseState.enabled)
            {
                ExternalHouseState = EExternalHouseState.waitingForRenew;
            }
        }

        [TutorialTriggerable]
        public void SetWorking()
        {
            BaseGameState.OnLevelUp -= SetWorking;
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_available, true))
            {
                BaseGameState.OnLevelUp += SetWorking;
                return;
            }
            if (ExternalHouseState != EExternalHouseState.enabled)
            {
                ExternalHouseState = EExternalHouseState.enabled;
            }
        }

        [TutorialTriggerable]
        public override void OnClickEnabled()
        {
            base.OnClickEnabled();
            if (VisitingController.Instance.IsVisiting || UIController.getHospital.PatientCard.vipGiftPending)
                return;
            BaseGameState.OnLevelUp -= OnClickEnabled;
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_available, true))
            {
                BaseGameState.OnLevelUp += OnClickEnabled;
                return;
            }
            NotificationCenter.Instance.BubbleBoyClicked.Invoke(new BaseNotificationEventArgs());

            if ((HospitalTutorialController.Instance.CurrentTutorialStepTag == StepTag.bubble_boy_intro) && firstTap)
            {
                firstTap = false;
                return;
            }

            StartCoroutine(UIController.getHospital.bubbleBoyEntryOverlayUI.Open(BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable() ? 0 : GetEntryFee()));
        }

        public override void OnClickDisabled()
        {
            base.OnClickDisabled();
            if (VisitingController.Instance.IsVisiting)
                return;

            TutorialController tc = TutorialController.Instance;
            if (Game.Instance.gameState().GetHospitalLevel() < roomInfo.UnlockLvl)
                MessageController.instance.ShowMessage(41);

            NotificationCenter.Instance.BubbleBoyClicked.Invoke(new BaseNotificationEventArgs());
        }

        public override void OnClickWaitingForRenew()
        {
            if (VisitingController.Instance.IsVisiting)
                return;

            TutorialController tc = TutorialController.Instance;
            if (tc.GetStepId(tc.CurrentTutorialStepTag) > tc.GetStepId(StepTag.bubble_boy_intro))
            {
                ExternalHouseState = EExternalHouseState.enabled;
                //OnClickEnabled();
            }
            NotificationCenter.Instance.BubbleBoyClicked.Invoke(new BaseNotificationEventArgs());
        }

        public int GetEntryFee()
        {
            return ResourcesHolder.GetHospital().bubbleBoyDatabase.GetEntryFee(BubbleBoyDataSynchronizer.Instance.TotalEntries).amount;
        }

        public bool IsRefundNeeded()
        {
            return BubbleBoyDataSynchronizer.Instance.RefundExist;
        }

        public void EnterMinigame(int refundAmount = 0)
        {
            if (refundAmount == 0)
                ReferenceHolder.GetHospital().bubbleBoyCharacterAI.OnMiniGameEnter();
            else
            {
                BubbleBoyDataSynchronizer.Instance.RefundExist = true;
                BubbleBoyDataSynchronizer.Instance.RefundAmount = refundAmount;
            }
            //  BubbleBoyDataSynchronizer.Instance.NextFreeEntryDate = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds) + waitTime;
        }


        public override void OnClick()
        {
            if (UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)
                return;
            if (!TutorialSystem.TutorialController.SkippedTutorialConditionFulfilled(StepTag.bubble_boy_intro, true))
            {
                //Show bubble boy unlock message to player
                MessageController.instance.ShowMessage(41);
                return;
            }
            base.OnClick();
        }

        public void SetSessionTimer(bool visitingMode)
        {
            Timing.KillCoroutine(UpdateTimeToNextSession().GetType());
            if (!visitingMode)
                Timing.RunCoroutine(UpdateTimeToNextSession());
        }

        IEnumerator<float> UpdateTimeToNextSession()
        {
            yield return Timing.WaitForSeconds(1);

            if (BubbleBoyDataSynchronizer.Instance.IsFreeEntryAvailable())
            {
                if (TutorialController.Instance.IsTutorialStepEqual(StepTag.bubble_boy_available) && !UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)
                    NotificationCenter.Instance.BubbleBoyAvailable.Invoke(new BaseNotificationEventArgs());
            }
        }

        public override string GetBuildingTag()
        {
            return "bubbleBoy";
        }
    }
}

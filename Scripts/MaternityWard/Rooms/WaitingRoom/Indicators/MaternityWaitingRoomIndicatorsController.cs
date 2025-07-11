using Hospital;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Maternity.UI
{
    public class MaternityWaitingRoomIndicatorsController
    {

        private MaternityWaitingRoomIndicatorsPresenter presenter = null;
        private BaseIndicator indicator = null;

        private MaternityPatientAI AI;
        private MaternityCharacterInfo Info;
        private MaternityWaitingRoomBed Bed;
        private RotatableObject targetObj;

        public void SetPresenter(MaternityWaitingRoomIndicatorsPresenter presenter)
        {
            this.presenter = presenter;
        }

        public void Start(MaternityWaitingRoomBed Bed, RotatableObject rObj)
        {
            this.Bed = Bed;
            targetObj = rObj;
            if (Bed != null)
            {
                IMaternityFacilityPatient patient = Bed.GetPatient();
                AI = patient != null ? patient.GetPatientAI() : null;
                Info = patient != null ? patient.GetInfoPatient() : null;
            }
            AI_onStateChanged();
            AddListeners();
        }

        public void OnDestroy()
        {
            RemoveListeners();
            if (indicator != null)
                indicator = null;
        }

        private void AddListeners()
        {
            RemoveListeners();
            if (AI != null)
                AI.onStateChanged += AI_onStateChanged;
            if (Bed != null)
                Bed.OnStateChanged += AI_onStateChanged;
        }

        private void RemoveListeners()
        {
            if (AI != null)
                AI.onStateChanged -= AI_onStateChanged;
            if (Bed != null)
                Bed.OnStateChanged -= AI_onStateChanged;
        }

        private void AI_onStateChanged()
        {
            if (BedIsNotOccupied())
                indicator = Bed.StateManager.State.GetWaitingRoomIndicator();
            else
                indicator = AINotNull() ? AI.Person.State.GetWaitingRoomIndicator() : null;
            UpdateIndicatorState();
        }

        private bool BedIsNotOccupied()
        {
            return
                Bed != null &&
                Bed.StateManager != null &&
                Bed.StateManager.State != null &&
                Bed.StateManager.State.GetTag() != MaternityWaitingRoomBed.State.OR;
        }

        private bool AINotNull()
        {
            return
                AI != null &&
                AI.Person != null &&
                AI.Person.State != null;
        }

        public bool OverrideOnClickBehaviour()
        {
            return indicator == null ? false : indicator.OverrideOnClickBehaviour() && indicator.IsActive(targetObj);
        }

        public void OnIndicatorClicked()
        {
            if (indicator != null && indicator.OverrideOnClickBehaviour() && indicator.IsActive(targetObj))
                indicator.OnClick();
        }

        private void UpdateIndicatorState()
        {
            if (presenter == null)
                return;
            presenter.HideAll();
            if (indicator != null && indicator.IsActive(targetObj))
            {
                indicator.SetView(presenter);
            }
        }

        #region Indicators

        public class PatientOnWayIndicator : BaseIndicator
        {
            public PatientOnWayIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                if (view != null && view.patientOnWayIndicator != null)
                {
                    view.patientOnWayIndicator.SetActive(true);
                }
            }
        }

        public class WaitingForPatientIndicator : BaseIndicator
        {
            public WaitingForPatientIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                if (view != null && view.waitIndicator != null)
                    view.waitIndicator.SetActive(true);
            }
        }

        public class CureIndicator : BaseIndicator
        {
            public CureIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                if (view != null && view.cureIndicator != null)
                    view.cureIndicator.SetActive(true);
            }
        }

        public class AlertIndicator : BaseIndicator
        {
            public AlertIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                if (view != null && view.newIndicator != null)
                    view.newIndicator.SetActive(true);
            }
        }

        public class ShowChildIndicator : BaseIndicator
        {
            public ShowChildIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                // TODO
                if (view != null && view.laborEndedIndicator != null)
                    view.laborEndedIndicator.SetActive(true);
            }

            public override bool OverrideOnClickBehaviour()
            {
                return true;
            }

            public override bool IsWaitingRoomIndicator()
            {
                return false;
            }

            public override void OnClick()
            {
                AI.Notify((int)StateNotifications.OnChildCollect, null);
            }

        }

        public class ClaimRewardForLaborIndicator : BaseIndicator
        {
            public ClaimRewardForLaborIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            private bool processingGiftsGet = false;

            public override bool OverrideOnClickBehaviour()
            {
                return true;
            }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                GenderTypes gender = (GenderTypes)AI.GetBabyInfo().Sex;
                if (gender == GenderTypes.Man)
                {
                    if (view != null && view.claimRewardIndicatorBoy != null)
                        view.claimRewardIndicatorBoy.SetActive(true);
                }
                else
                {
                    if (view != null && view.claimRewardIndicatorGirl != null)
                        view.claimRewardIndicatorGirl.SetActive(true);
                }
            }

            public override void OnClick()
            {
                if (processingGiftsGet)
                    return;
                processingGiftsGet = true;
                MaternityBoxRewardGenerator.Get(OnBoxRewardsReceived);
            }

            private void OnBoxRewardsReceived(List<GiftReward> rewards)
            {
                PatientAvatarUI.PatientBackgroundType gender = PatientAvatarUI.PatientBackgroundType.boyBaby;
                BabyCharacterInfo info = AI.GetBabyInfo();
                if (info == null)
                    gender = PatientAvatarUI.PatientBackgroundType.unknownBaby;
                gender = info.Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby;
                int expReward = AI.GetInfoPatient().GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding);
                UIController.getMaternity.boxOpeningPopupUI.Open(new MaternityHealingRewardBoxModel(gender == PatientAvatarUI.PatientBackgroundType.boyBaby ? MaternityHealingRewardBoxModel.Gender.Boy : MaternityHealingRewardBoxModel.Gender.Girl, rewards,
                () =>
                {
                    AI.Notify((int)StateNotifications.CollectRewardForLabor, null);
                    //SoundsController.Instance.PlayReward();
                    SoundsController.Instance.HBGiftClaim();

                },
                () =>
                {
                    int currentExpAmount = Game.Instance.gameState().GetExperienceAmount() - expReward;
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, new Vector3(-.1f, .75f, 0), expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                    });
                }));
            }
        }

        public class ReadyForLaborIndicator : BaseIndicator
        {
            public ReadyForLaborIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed) : base(AI, Bed) { }

            public override void SetView(MaternityWaitingRoomIndicatorsPresenter view)
            {
                if (view != null && view.readyForLaborIndicator != null)
                    view.readyForLaborIndicator.SetActive(true);
            }

            public override bool OverrideOnClickBehaviour()
            {
                return true;
            }

            public override void OnClick()
            {
                AI.Notify((int)StateNotifications.SendToLabor, null);
            }

        }

        public abstract class BaseIndicator
        {
            public MaternityPatientAI AI;
            public MaternityWaitingRoomBed Bed;

            public BaseIndicator(MaternityPatientAI AI, MaternityWaitingRoomBed Bed)
            {
                this.AI = AI;
                this.Bed = Bed;
            }

            public virtual bool OverrideOnClickBehaviour()
            {
                return false;
            }

            public virtual void OnClick() { }

            public abstract void SetView(MaternityWaitingRoomIndicatorsPresenter view);

            public bool IsActive(RotatableObject rObj)
            {
                if (rObj is MaternityWaitingRoom && IsWaitingRoomIndicator())
                    return true;
                if (rObj is MaternityLabourRoom && !IsWaitingRoomIndicator())
                    return true;
                return false;
            }

            public virtual bool IsWaitingRoomIndicator()
            {
                return true;
            }

        }

        #endregion

    }
}

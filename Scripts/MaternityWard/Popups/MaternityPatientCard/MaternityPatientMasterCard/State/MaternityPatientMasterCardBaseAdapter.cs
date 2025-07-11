using Hospital;
using Maternity.UI;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Maternity.Adapter
{
    public abstract class MaternityPatientMasterCardBaseAdapter: IDiamondTransactionMaker
    {
        protected MaternityPatientMasterCardController controller;
        protected IMaternityTreatmentPanelUI ui;
        protected MaternityPatientInfo patientInfo;
        protected MaternityPatientInfo babyInfo;
        private Guid DiamondTransactionMakerID;
        public const string EMPTY = "";

        public MaternityPatientMasterCardBaseAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui)
        {
            this.controller = controller;
            this.ui = ui;

            SetUp();
        }

        public MaternityPatientInfo GetPatientInfo()
        {
            if(patientInfo == null)
            {
                patientInfo = new MaternityPatientInfo();
            }
            patientInfo.name = GetPatientName();
            patientInfo.about = GetAboutMother();
            patientInfo.head = controller.Info.AvatarHead;
            patientInfo.body = controller.Info.AvatarBody;
            patientInfo.gender = PatientAvatarUI.PatientBackgroundType.defaultAdult;
            return patientInfo;
        }

        public MaternityPatientInfo GetPatientBabyInfo()
        {
            if (babyInfo == null)
            {
                babyInfo = new MaternityPatientInfo();
            }
            babyInfo.name = GetBabyName();
            babyInfo.about = GetAboutBaby();
            babyInfo.head = GetBabyHead();
            babyInfo.body = GetBabyBody();
            babyInfo.gender = GetBabyGender();
            return babyInfo;
        }

        protected string GetPatientName()
        {
            return I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + controller.Info.Name) + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + controller.Info.Surname);
        }

        protected string GetAboutMother()
        {
            return controller.Info.GetLikesString();
        }

        protected string GetAboutBaby()
        {
            return controller.Ai.GetBabyInfo().GetLikesString();
        }

        protected string GetBabyName()
        {
            BabyCharacterInfo info = controller.Ai.GetBabyInfo();
            if (info != null)
            {
                return I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + info.Name);
            }
            return "";
        }

        protected string GetBabyInfo()
        {
            return "Baby Info";
        }

        protected PatientAvatarUI.PatientBackgroundType GetBabyGender()
        {
            BabyCharacterInfo info = controller.Ai.GetBabyInfo();
            if(info == null)
                return PatientAvatarUI.PatientBackgroundType.unknownBaby;
            return info.Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby;
        }

        protected Sprite GetBabyHead()
        {
            BabyCharacterInfo info = controller.Ai.GetBabyInfo();
            if (info == null)
                return null;
            return info.AvatarHead;
        }

        protected Sprite GetBabyBody()
        {
            BabyCharacterInfo info = controller.Ai.GetBabyInfo();
            if (info == null)
                return null;
            return info.AvatarBody;
        }

        protected void ClosePopupAndRedirectToBloodTestRoom(bool showHover = true)
        {
            UIController.getMaternity.patientCardController.Exit();
            MaternityBloodTestRoom bloodTestRoom = MaternityBloodTestRoomController.Instance.GetBloodTestRoom();
            bloodTestRoom.RedirectTo(showHover);
        }

        protected void AddExperience(int experienceAmount, EconomySource source)
        {
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, experienceAmount, source, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(
                  GiftType.Exp,
                  ui.GetExperienceSource(),
                  experienceAmount,
                  0.2f, 1, Vector3.one, Vector3.one, null, null,
                  () =>
                  {
                      Game.Instance.gameState().UpdateCounter(ResourceType.Exp, experienceAmount, currentExpAmount);
                  });
        }

        protected void AddHealingAndBondingExp()
        {
            int expReward = controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding);
            Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.MaternityHealingRewardBox, false);
        }

        protected void OnBoxRewardsReceived(List<GiftReward> rewards)
        {
			
			int expReward = controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding);
            
            UIController.getMaternity.boxOpeningPopupUI.Open(new MaternityHealingRewardBoxModel(GetBabyGender() == PatientAvatarUI.PatientBackgroundType.boyBaby ? MaternityHealingRewardBoxModel.Gender.Boy : MaternityHealingRewardBoxModel.Gender.Girl, rewards,
            () => {
                AddHealingAndBondingExp();
                controller.Ai.Notify((int)StateNotifications.CollectRewardForLabor, null);
				//SoundsController.Instance.PlayReward();
				SoundsController.Instance.HBGiftClaim();

			},
            () => {
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount() - expReward;
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, new Vector3(-.1f, .75f, 0), expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                });
            }));
        }

        public virtual void SetUp()
        {
            InitializeID();
        }

        public virtual void OnUpdate() { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        public virtual void OnDestroy() { }

        public void InitializeID()
        {
            DiamondTransactionMakerID = Guid.NewGuid();
        }

        public Guid GetID()
        {
            return DiamondTransactionMakerID;
        }

        public void EraseID()
        {
            DiamondTransactionMakerID = Guid.Empty;
        }
    }

}

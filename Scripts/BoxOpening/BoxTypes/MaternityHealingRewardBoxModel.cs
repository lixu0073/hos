using Hospital.BoxOpening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital.BoxOpening.UI;
using System;

namespace Hospital
{
    public class MaternityHealingRewardBoxModel : BaseBoxModel
    {

        private Gender gender = Gender.Girl;

        public enum Gender
        {
            Girl,
            Boy
        }

        public MaternityHealingRewardBoxModel(Gender gender, List<GiftReward> rewards, Action onSaveCallback = null, Action onCloseCallback = null) : base(rewards, onSaveCallback, onCloseCallback)
        {
            this.gender = gender;
        }

        public override BoxOpeningPopupUI.ExtendedBoxAssets GetAssets()
        {
            return gender == Gender.Boy ? ResourcesHolder.Get().boxesSprites.maternityBoyBox : ResourcesHolder.Get().boxesSprites.maternityGirlBox;
        }

        public override EconomySource GetEconomySource()
        {
            return EconomySource.MaternityHealingRewardBox;
        }

        public override string GetTopTitle()
        {
            return I2.Loc.ScriptLocalization.Get(gender == Gender.Boy ? "MATERNITY_BOY_BOX_TITLE" : "MATERNITY_GIRL_BOX_TITLE");
        }

        protected override void ReportBoxCollected()
        {

        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Hospital.BoxOpening.UI;
using Hospital.LootBox;
using UnityEngine;

namespace Hospital.BoxOpening
{
    public class XmasLootBoxModel : BaseBoxModel
    {
        public XmasLootBoxModel(List<GiftReward> rewards, Action onSaveCallback = null, Action onCloseCallback = null) : base(rewards, onSaveCallback, onCloseCallback) {}

        public override BoxOpeningPopupUI.ExtendedBoxAssets GetAssets()
        {
            return ResourcesHolder.Get().boxesSprites.xmasLootBox;
        }

        public override EconomySource GetEconomySource()
        {
            return EconomySource.GlobalEventReward;
        }

        public override string GetTopTitle()
        {
            return I2.Loc.ScriptLocalization.Get("REWARD");
        }

        protected override void ReportBoxCollected()
        {
            
        }
    }
}

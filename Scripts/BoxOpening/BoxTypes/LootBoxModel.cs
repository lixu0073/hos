using System;
using System.Collections;
using System.Collections.Generic;
using Hospital.BoxOpening.UI;
using UnityEngine;

namespace Hospital.BoxOpening
{
    public class LootBoxModel : BaseBoxModel
    {
        private LootBox.Box boxType = LootBox.Box.blue;
        private string tag;

        public LootBoxModel(LootBox.Box boxType, string tag, List<GiftReward> rewards, Action onSaveCallback = null, Action onCloseCallback = null) : base(rewards, onSaveCallback, onCloseCallback)
        {
            this.tag = tag;
            this.boxType = boxType;
        }

        public override BoxOpeningPopupUI.ExtendedBoxAssets GetAssets()
        {
            switch(boxType)
            {
                case LootBox.Box.blue:
                    return ResourcesHolder.Get().boxesSprites.blueLootBox;
                case LootBox.Box.pink:
                    return ResourcesHolder.Get().boxesSprites.purpleLootBox;
                case LootBox.Box.xmas:
                default:
                    return ResourcesHolder.Get().boxesSprites.xmasLootBox;
            }
        }

        public override EconomySource GetEconomySource()
        {
            return EconomySource.LootBox;
        }

        public override string GetTopTitle()
        {
            return I2.Loc.ScriptLocalization.Get(tag + "_BOX").ToUpper();
        }

        protected override void ReportBoxCollected()
        {
            
        }
    }
}

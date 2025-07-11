using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital.BoxOpening
{
    public abstract class BaseBoxModel
    {
        private Queue<GiftReward> rewards;

        public event Action OnSave;
        public event Action OnCloseCallback;

        public GiftReward GetReward()
        {
            return rewards.Count > 0 ? rewards.Dequeue() : null;
        }

        public BaseBoxModel(List<GiftReward> rewards, Action onSaveCallback = null, Action onCloseCallback = null)
        {
            this.rewards = new Queue<GiftReward>(rewards);
            OnSave = onSaveCallback;
            OnCloseCallback = onCloseCallback;
            SetRewardsEconomySource();
        }

        public void NotifyClose()
        {
            OnCloseCallback?.Invoke();
            OnCloseCallback = null;
        }

        public void Collect()
        {
            if(rewards == null)
            {
                Debug.LogError("[OPENING] no rewards !");
                return;
            }
            foreach(GiftReward reward in rewards)
            {
                reward.Collect();
            }
            ReportBoxCollected();
            OnSave?.Invoke();
        }

        private void SetRewardsEconomySource()
        {
            foreach(GiftReward reward in rewards)
            {
                reward.EconomySource = GetEconomySource();
            }
        }

        protected abstract void ReportBoxCollected();
        
        public abstract string GetTopTitle();
        public abstract EconomySource GetEconomySource();

        public abstract UI.BoxOpeningPopupUI.ExtendedBoxAssets GetAssets();

        public virtual string GetBottomBoxText(GiftReward reward = null)
        {
            return I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_OPEN");
        }
        public virtual string GetBottomItemText(GiftReward reward = null)
        {
            return I2.Loc.ScriptLocalization.Get("UNBOXING_TAP_TO_COLLECT");
        }
    }
}

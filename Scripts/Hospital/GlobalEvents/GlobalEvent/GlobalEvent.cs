using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SimpleUI;

public abstract class GlobalEvent
{

    private string id;

    private int minLevel;
    protected int personalProgress;
    protected int personalGoal;
    //protected List<GlobalEventRewardPackage> personalGoalsReward = new List<GlobalEventRewardPackage>();
    //protected GlobalEventRewardPackage reward;

    protected List<GlobalEventRewardModel> personalGoalsReward = new List<GlobalEventRewardModel>();
    protected List<List<GlobalEventRewardModel>> contributionRewards = new List<List<GlobalEventRewardModel>>();
    protected GlobalEventRewardModel firstPlaceReward = null;
    protected GlobalEventRewardModel secondPlaceReward = null;
    protected GlobalEventRewardModel thirdPlaceReward = null;
    protected GlobalEventRewardModel lastReward = null;
    protected GlobalEventType eventType;
    protected GlobalEventExtras eventExtras;

    protected const char globalParameterSeparator = ';';

    public string ID
    {
        private set { }
        get { return id; }
    }

    public int MinLevel
    {
        private set { }
        get { return minLevel; }
    }

    public int PersonalProgress
    {
        private set { }
        get { return personalProgress; }
    }

    public int PersonalGoal
    {
        private set { }
        get { return personalGoal; }
    }

    public GlobalEventRewardModel GlobalReward
    {
        private set { }
        get { return firstPlaceReward; }
    }

    public GlobalEventRewardModel FirstPlaceReward
    {
        private set { }
        get { return firstPlaceReward; }
    }

    public GlobalEventRewardModel SecondPlaceReward
    {
        private set { }
        get { return secondPlaceReward; }
    }

    public GlobalEventRewardModel ThirdPlaceReward
    {
        private set { }
        get { return thirdPlaceReward; }
    }

    public GlobalEventRewardModel LastReward
    {
        private set { }
        get { return lastReward; }
    }

    public List<GlobalEventRewardModel> PersonalGoalsReward
    {
        private set { }
        get { return personalGoalsReward; }
    }

    public List<List<GlobalEventRewardModel>> ContributionRewards
    {
        private set { }
        get { return contributionRewards; }
    }

    public GlobalEventType EventType
    {
        private set { }
        get { return eventType; }
    }

    public GlobalEventExtras EventExtras
    {
        private set { }
        get { return eventExtras; }
    }

    public GlobalEvent()
    {
        personalProgress = 0;
        personalGoal = 0;
        eventExtras = GlobalEventExtras.Default;
    }

    public virtual void LoadFromString(string saveString)
    {
        int beginningOfGlobalRewardsString = saveString.IndexOf('{');
        int endingOfGlobalRewardsString = saveString.IndexOf('}');

        string[] globalRewardsString = (saveString.Substring(beginningOfGlobalRewardsString + 1, endingOfGlobalRewardsString - beginningOfGlobalRewardsString - 1)).Split('^');

        firstPlaceReward = GlobalEventRewardModel.Parse(globalRewardsString[0]);
        secondPlaceReward = GlobalEventRewardModel.Parse(globalRewardsString[1]);
        thirdPlaceReward = GlobalEventRewardModel.Parse(globalRewardsString[2]);
        lastReward = GlobalEventRewardModel.Parse(globalRewardsString[3]);
        
        string newSaveString = saveString.Remove(beginningOfGlobalRewardsString, endingOfGlobalRewardsString - beginningOfGlobalRewardsString - 1);

        int beginningOfPersonalRewardsString = newSaveString.IndexOf('[');
        int endingOfPersonalRewardsString = newSaveString.IndexOf(']');
        string personalRewardString = newSaveString.Substring(beginningOfPersonalRewardsString + 1, endingOfPersonalRewardsString - beginningOfPersonalRewardsString - 1);

        newSaveString = newSaveString.Remove(beginningOfPersonalRewardsString, endingOfPersonalRewardsString - beginningOfPersonalRewardsString - 1);


        var globalEventDataSave = newSaveString.Split(';');

        id = globalEventDataSave[1];

        minLevel = int.Parse(globalEventDataSave[2], System.Globalization.CultureInfo.InvariantCulture);

        personalProgress = int.Parse(globalEventDataSave[3], System.Globalization.CultureInfo.InvariantCulture);
        personalGoal = int.Parse(globalEventDataSave[4], System.Globalization.CultureInfo.InvariantCulture);

        personalGoalsReward.Clear();

        var personalRewardData = personalRewardString.Split('%'); //globalEventDataSave[4].Split('%');

        if (personalRewardData != null && personalRewardData.Length > 0)
        {
            for (int i = 0; i < personalRewardData.Length; i++)
                personalGoalsReward.Add(GlobalEventRewardModel.Parse(personalRewardData[i]));
        }

        int beginningOfContributionRewardsString = newSaveString.IndexOf('<');
        int endingOfContributionRewardsString = newSaveString.IndexOf('>');
        string contributionRewardString = newSaveString.Substring(beginningOfContributionRewardsString + 1, endingOfContributionRewardsString - beginningOfContributionRewardsString - 1);

        contributionRewards.Clear();

        var contributionRewardDatas = contributionRewardString.Split('^');
        if (contributionRewardDatas != null && contributionRewardDatas.Length > 0)
        {
            for (int i = 0; i < contributionRewardDatas.Length; ++i)
            {
                contributionRewards.Add(new List<GlobalEventRewardModel>());

                var contributionRewardData = contributionRewardDatas[i].Split('%');
                
                for (int j = 0; j < contributionRewardData.Length; ++j)
                {
                    if (string.IsNullOrEmpty(contributionRewardData[j]))
                    {
                        continue;
                    }
                    contributionRewards[i].Add(GlobalEventRewardModel.Parse(contributionRewardData[j]));
                }
            }
        }
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(GetType().ToString());
        builder.Append(";");
        builder.Append(ID);
        builder.Append(";");
        builder.Append(MinLevel);
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(this.personalProgress, 0, int.MaxValue, "ActivityGlobalEvent progress: ").ToString());
        builder.Append(";");
        builder.Append(Checkers.CheckedAmount(this.personalGoal, 0, int.MaxValue, "ActivityGlobalEvent goal: ").ToString());
        builder.Append(";");
        builder.Append('{');
        builder.Append(firstPlaceReward.SaveToString());
        builder.Append('^');
        builder.Append(secondPlaceReward.SaveToString());
        builder.Append('^');
        builder.Append(thirdPlaceReward.SaveToString());
        builder.Append('^');
        builder.Append(lastReward.SaveToString());
        builder.Append('}');
        builder.Append(";");
        builder.Append('[');
        for (int i = 0; i < personalGoalsReward.Count; ++i)
        {
            builder.Append(personalGoalsReward[i].SaveToString());

            if (i < personalGoalsReward.Count - 1)
                builder.Append("%");
        }

        builder.Append(']');
        builder.Append(";");
        builder.Append("<");
        for (int i = 0; i < contributionRewards.Count; ++i)
        {
            for (int j = 0; j < contributionRewards[i].Count; ++j)
            {
                builder.Append(contributionRewards[i][j].SaveToString());

                if (j < contributionRewards[i].Count - 1)
                    builder.Append("%");
            }

            if (i < contributionRewards.Count - 1)
                builder.Append("^");
        }
        builder.Append(">");

        return builder.ToString();
    }

    public virtual bool Init(GlobalEventData globalEventData)
    {
        if (globalEventData != null)
        {
            id = globalEventData.Id;

            minLevel = globalEventData.MinLevel;

            firstPlaceReward = GlobalEventRewardModel.Parse(globalEventData.FirstPlaceReward);
            
            secondPlaceReward = GlobalEventRewardModel.Parse(globalEventData.SecondPlaceReward);
            
            thirdPlaceReward = GlobalEventRewardModel.Parse(globalEventData.ThirdPlaceReward);

            lastReward = GlobalEventRewardModel.Parse(globalEventData.LastReward);

            personalGoalsReward.Clear();
            for (int i = 0; i < globalEventData.PersonalGoalsRewards.Length; ++i)
            {
                var personalReward = GlobalEventRewardModel.Parse(globalEventData.PersonalGoalsRewards[i]);

                if (personalReward != null)
                    personalGoalsReward.Add(personalReward);
            }

            contributionRewards.Clear();
            for (int i = 0; i < globalEventData.ContributionRewards.Length; ++i)
            {
                contributionRewards.Add(new List<GlobalEventRewardModel>());
                for (int j = 0; j < globalEventData.ContributionRewards[i].Length; ++j)
                {
                    contributionRewards[i].Add(GlobalEventRewardModel.Parse(globalEventData.ContributionRewards[i][j]));
                }
            }

            if (globalEventData.PersonalGoalsRewards.Length == personalGoalsReward.Count)
            {
                return true;
            }
        }
        return false;
    }

    public virtual void OnStart()
    {
        GlobalEventNotificationCenter.Instance.OnEventStart.Invoke(new GlobalEventOnStateChangeEventArgs(eventType, eventExtras));
    }

    public virtual void OnDestroy()
    {
        GlobalEventNotificationCenter.Instance.OnEventEnd.Invoke(new GlobalEventOnStateChangeEventArgs(eventType, eventExtras));
    }

    public virtual void OnReload(GlobalEventData globalEventData)
    {
        GlobalEventNotificationCenter.Instance.OnEventReload.Invoke(new GlobalEventOnStateChangeEventArgs(eventType, eventExtras));
    }

    public abstract void AddPersonalProgress(int amount);

    public abstract string GetDescription(string key);

    protected void GiveContributionReward()
    {
        GlobalEventController GEController = ReferenceHolder.GetHospital().globalEventController;

        int coinsAmount = GEController.GetCurrentContributionCoinsReward();
        int expAmount = GEController.GetCurrentContributionExpReward();

        //giveReward
        Vector3 startPos = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        if (coinsAmount > 0)
        {
            int currentCoinAmount = Game.Instance.gameState().GetCoinAmount();
            GameState.Get().AddResource(ResourceType.Coin, coinsAmount, EconomySource.GlobalEventContribution, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, startPos, coinsAmount, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Coin, coinsAmount, currentCoinAmount);
            });
        }

        if (expAmount > 0)
        {
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expAmount, EconomySource.GlobalEventContribution, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, startPos, expAmount, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expAmount, currentExpAmount);
            });
        }
    }

    protected void OnSuccesAddPersonalProgress(int amount, string eventID)
    {
        if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive())
        {
            CacheManager.SetLastGlobalEventContribution(this.personalProgress);
            CacheManager.SetPrevGlobalEventId(eventID);
        }
    }

    protected void CheckPersonalGoalReward(bool showHamster)
    {
        int newGoal = ReferenceHolder.GetHospital().globalEventController.GetCurrentGoalID();
        if (newGoal > personalGoal)
        {
            personalGoalsReward[Mathf.Clamp(personalGoal, 0, personalGoalsReward.Count - 1)].CollectReward(false, true);
            if (showHamster)
            {
                UIController.getHospital.EventGoalReached.Show();

                //Vector2 startPos = new Vector2((UIController.getHospital.EventGoalReached.transform.position.x - Screen.width / 2) / UIController.get.canvas.transform.localScale.x,
                //                                  (UIController.getHospital.EventGoalReached.transform.position.y - Screen.height / 2) / UIController.get.canvas.transform.localScale.y);
            }
            personalGoal = newGoal;
        }
    }

    public enum GlobalEventType
    {
        Default,
        Contribution,
        Activity
    }

    public enum GlobalEventExtras
    {
        Default,
        ValentineHearts,
    }
}

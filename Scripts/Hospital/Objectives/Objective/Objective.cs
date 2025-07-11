using UnityEngine;
using System.Text;
using System;


public abstract class Objective
{
    private int progressCounter;
    private int uiProgressCounter;
    //private bool collected;
    private DefaultObjectiveReward reward;

    protected int progressObjective;
    protected bool completed;
  
    public ObjectiveType objectiveType;

    public bool diamondShown = false;

	public bool rewardShown = false;

    public int ProgressObjective
    {
        get { return progressObjective; }
        private set { }
    }

    public int Progress
    {
        get { return progressCounter; }
        private set { }
    }

    public int ProgressUI
    {
        get { return uiProgressCounter; }
        private set { }
    }

    public DefaultObjectiveReward Reward
    {
        get { return reward; }
        private set { }
    }

    public bool IsCompleted
    {
        get { return completed; }
        private set { }
    }

    public bool isRewardClaimed
    {
        get { return reward.Claimed; }
        private set { }
    }

    internal Objective()
    {
        objectiveType = ObjectiveType.DefaultObjective;
        progressObjective = 1;

        progressCounter = 0;
        uiProgressCounter = 0;
        completed = false;
    }

    internal void UpdateObjective(int value)
    {
        progressCounter += value;

        if (progressCounter >= progressObjective)
        {
            progressCounter = progressObjective;
            completed = true;
            UIController.getHospital.ObjectivesPanelUI.TaskCompletedUpdateListUI();  
        }

        UIController.getHospital.ObjectivesPanelUI.UpdateProgress();
    }

    public string GetProgressStringUI()
    {
        return uiProgressCounter + "/" + progressObjective;
    }

    public string GetProgressString()
    {
        return progressCounter + "/" + progressObjective;
    }

    public float GetProgressFloat()
    {
        return (float)progressCounter / (float)progressObjective;
    }

    public virtual string GetObjectiveNameForAnalitics()
    {
        return objectiveType.ToString();
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(GetType().ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(progressCounter, 0, int.MaxValue, "Objective progressCounter: ").ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(uiProgressCounter, 0, int.MaxValue, "Objective uiProgressCounter: ").ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(progressObjective, 0, int.MaxValue, "Objective progressGoal: ").ToString());
        builder.Append("!");
        builder.Append(reward.SaveToString());
        return builder.ToString();
    }

    public virtual void LoadFromString(string saveString)
    {
        var taskDataSave = saveString.Split('!');
        progressCounter = int.Parse(taskDataSave[1], System.Globalization.CultureInfo.InvariantCulture);
        uiProgressCounter = int.Parse(taskDataSave[2], System.Globalization.CultureInfo.InvariantCulture);
        progressObjective = int.Parse(taskDataSave[3], System.Globalization.CultureInfo.InvariantCulture);

        reward = DefaultObjectiveReward.Parse(taskDataSave[4]);

        if (progressCounter >= progressObjective)
            completed = true;
        else completed = false;
    }

    public void RefreshProgressUI()
    {
        uiProgressCounter = progressCounter;
    }

    public virtual void CollectReward(Vector2 startPoint = default(Vector2), bool delayed = true)
    {
        if (reward!=null)
            reward.Collect(startPoint, delayed);
    }

    public virtual void CompleteGoal()
    {
        progressCounter = progressObjective;
        completed = true;
    }

    protected void SetType()
    {
        objectiveType = (ObjectiveType)Enum.Parse(typeof(ObjectiveType), GetType().ToString());
    }

    protected void SetTaskObjectives(int amount, DefaultObjectiveReward reward)
    {
        completed = false;
        progressObjective = amount;
        progressCounter = 0;
        uiProgressCounter = 0;
        this.reward = reward;

        //collected = false;
    }

    public abstract void AddListener();
    protected abstract void RemoveListener();

    public abstract bool Init(ObjectiveData objectiveData);
    public abstract bool InitWithRandom();
    public abstract string GetDescription();
    public abstract string GetInfoDescription();

    public void OnDestroy()
    {
        RemoveListener();
    }

    public enum ObjectiveType
    {
        DefaultObjective = 0,
        BuildRotatableObjective = 1,
        CurePatientDoctorRoomObjective = 2,
        CurePatientHospitalRoomObjective = 3,
        DiagnosePatientObjective = 4,
        ExpandAreaObjective = 5,
        RenovateSpecialObjective = 6,
        LevelUpObjective = 7,
        TreatmentHelpRequestObjective = 8
    }
}

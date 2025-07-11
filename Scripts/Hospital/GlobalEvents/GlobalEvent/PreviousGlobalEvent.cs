using System.Collections.Generic;

public class PreviousGlobalEvent
{
    //private bool completed = false;
    private List<GlobalEventRewardModel> reward;

    public List<GlobalEventRewardModel> GlobalReward
    {
        private set { }
        get { return reward; }
    }

    protected GlobalEvent.GlobalEventType eventType;

    public string ID
    {
        get;
        private set;
    }

    public int PersonalGoalContributionMargin
    {
        get;
        private set;
    }

    public int GlobalEventEndTime
    {
        get;
        private set;
    }

    public int GlobalGoalFakeResult
    {
        get;
        private set;
    }

    public PreviousGlobalEvent(GlobalEventData globalEventData)
    {
        this.ID = globalEventData.Id;
        this.GlobalEventEndTime = globalEventData.GlobalEventEndTime;
        //this.completed = true;
        if (reward == null)        
            reward = new List<GlobalEventRewardModel>();

        reward.Clear();
    }
}

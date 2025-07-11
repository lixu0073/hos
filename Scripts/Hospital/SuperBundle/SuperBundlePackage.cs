using System.Collections.Generic;
using System;

public class SuperBundlePackage
{
    private string key;
    private Parser.Results results;

    private ISuperBundleReward mainReward;
    private List<ISuperBundleReward> rewards = new List<ISuperBundleReward>();

    private Parser parser = new Parser();

    public SuperBundlePackage(string key, string unparsedPackage)
    {
        this.key = key;
        results = parser.Execute(unparsedPackage);
        SuperBundleRewardFactory.Load(results.unparsedRewards, this, rewards);
        //if (GetRewards().Count > 3)
        //    throw new Exception("To many standard rewards for bundle: " + key);
    }

    public SuperBundlePackage(string unparsedRewards)
    {
        results = new Parser.Results();
        results.unparsedRewards = unparsedRewards;
        SuperBundleRewardFactory.Load(results.unparsedRewards, this, rewards);
        //if (GetRewards().Count > 3)
        //    throw new Exception("To many standard rewards for bundle: " + key);
    }

    public bool IsActive()
    {
        //temporary local bundle solution
        return true;

        /* CV: commented until we check why CPG is using this temporal local bundle solution (on top of this comment)
        if (!results.isActive)
            return false;
        DateTime now = DateTime.Now;
        return now > GetStartTime() && now < GetEndTime();*/
    }

    public void Collect(bool delayed = false, float delay = 0f)
    {
        float currentDelay = delayed ? delay : 0f;
        if (mainReward != null)
            mainReward.Collect(currentDelay);
        foreach (ISuperBundleReward reward in rewards)
        {
            currentDelay += delay;
            reward.Collect(currentDelay);
        }
    }

    public void Buy()
    {
        Collect();
    }
    
    public DateTime GetStartTime()
    {
        return results.startTime;
    }

    public DateTime GetEndTime()
    {
        return results.endTime;
    }

    public string GetPackageName()
    {
        return results.packageName;
    }

    public int GetDiamondsPrice()
    {
        return results.diamondsPrice;
    }

    public bool IsForDiamonds()
    {
        return results.diamondsPrice > 0;
    }

    public string GetKey()
    {
        return key;
    }

    public string GetProductId()
    {
        return results.productId;
    }

    public ISuperBundleReward GetMainReward()
    {
        return mainReward;
    }

    public void SetMainReward(ISuperBundleReward mainReward)
    {
        this.mainReward = mainReward;
    }

    public List<ISuperBundleReward> GetRewards()
    {
        return rewards;
    }

    public override string ToString()
    {
        string message = "key: " + GetKey() + ", product_id: " + GetProductId() + ", is_active: " + IsActive() + ", main_reward: ";
        message += GetMainReward() == null ? "null" : mainReward.ToString();
        foreach(ISuperBundleReward reward in GetRewards())
        {
            message += ", reward: " + reward.ToString();
        }
        return message;
    }

    private class Parser
    {
        public class Results
        {
            public string productId;
            public DateTime startTime;
            public  DateTime endTime;
            public bool isActive;
            public string unparsedRewards;
            public string packageName;
            public int diamondsPrice;
        }

        public Results Execute(string unparsedPackage)
        {
            string[] unparsedArrayPackage = unparsedPackage.Split('#');
            if (unparsedArrayPackage.Length != 7)
                throw new Exception("Invalid length of array - split by '#'");

            Results results = new Results();
            results.productId = ParseProductId(unparsedArrayPackage[0]);
            results.packageName = unparsedArrayPackage[1];
            results.diamondsPrice = int.Parse(unparsedArrayPackage[2], System.Globalization.CultureInfo.InvariantCulture);
            results.startTime = ParseDate(unparsedArrayPackage[3]);
            results.endTime = ParseDate(unparsedArrayPackage[4]);
            results.isActive = int.Parse(unparsedArrayPackage[5], System.Globalization.CultureInfo.InvariantCulture) == 1;
            results.unparsedRewards = unparsedArrayPackage[6];
            return results;
        }

        private string ParseProductId(string unparsedProductId)
        {
            // TDOD
            return unparsedProductId;
        }

        private DateTime ParseDate(string unparsedDate)
        {
            if (string.IsNullOrEmpty(unparsedDate))
                throw new Exception("Start or End DateTime is empty or null");
            try
            {
                return DateTime.ParseExact(unparsedDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                throw new Exception("Start or End DateTime Parse Error: " + e.Message);
            }
        }

    }

}

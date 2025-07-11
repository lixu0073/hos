using UnityEngine;
using TMPro;

public class DailyQuestUnclaimedRewardUI : MonoBehaviour
{
    [SerializeField] GameObject[] rewards = new GameObject[3];
#pragma warning disable 0649
    [SerializeField] TextMeshProUGUI dayText;
    [SerializeField] TextMeshProUGUI description;
#pragma warning restore 0649    
    RewardPackage rewardPackage;


    public void Setup(RewardPackage rewardPackage)
    {
        this.rewardPackage = rewardPackage;

        HideAllRewards();
        ShowReward();
        SetDescription();
        gameObject.SetActive(true);
    }
    
    void HideAllRewards()
    {
        for (int i = 0; i < 3; ++i)
            rewards[i].SetActive(false);
    }

    void ShowReward()
    {
        switch (rewardPackage.PackageRewardQuality)
        {
            case RewardPackage.RewardQuality.Starx1:
                rewards[0].SetActive(true);
                break;
            case RewardPackage.RewardQuality.Starx2:
                rewards[1].SetActive(true);
                break;
            case RewardPackage.RewardQuality.Starx3:
                rewards[2].SetActive(true);
                break;
            default:
                Debug.LogError("Unhandled reward quality!!! This should never happen.");
                break;
        }
    }

    void SetDescription()
    {
        dayText.text = string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/DAY_X"), rewardPackage.DayCorespondingToRewardPackage);

        switch (rewardPackage.PackageRewardQuality)
        {
            case RewardPackage.RewardQuality.Starx1:
                description.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_1_STAR");
                break;
            case RewardPackage.RewardQuality.Starx2:
                description.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_2_STAR");
                break;
            case RewardPackage.RewardQuality.Starx3:
                description.text = I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/REWARD_3_STAR");
                break;
            default:
                Debug.LogError("Unhandled reward quality!!! This should never happen.");
                break;
        }
    }
}

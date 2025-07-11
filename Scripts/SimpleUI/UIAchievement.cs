using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class UIAchievement : MonoBehaviour {

    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI rewardExp;
    public TextMeshProUGUI rewardDiamond;
    public GameObject[] medalsOff;
    public GameObject[] medalsOn;
    public Button collectButton;
    public Slider progressSlider;
    public TextMeshProUGUI progressText;

    Achievement a;
    int stage;


    public void Set(Achievement a)
    {
        this.a = a;
        stage = Mathf.Clamp(a.stage - a.ToCollect, 0, a.achievementInfo.requiredValues.Count);

        SetTitles();
        SetProgressbar();
        SetRewards();
        SetMedals();
        SetCollectButton();
    }
    
    void SetTitles()
    {
        string descriptionString;

        if (stage > 2)
        {
            descriptionString = I2.Loc.ScriptLocalization.Get(a.achievementInfo.questString).Replace("requiredValue", a.achievementInfo.requiredValues[2].ToString());
            if (a.achievementInfo.timeControlled)
                descriptionString = descriptionString.Replace("requiredTime", a.achievementInfo.requiredTimes[2].ToString());
        } else {
            descriptionString = I2.Loc.ScriptLocalization.Get(a.achievementInfo.questString).Replace("requiredValue", a.achievementInfo.requiredValues[stage].ToString());
            if (a.achievementInfo.timeControlled)
                descriptionString = descriptionString.Replace("requiredTime", a.achievementInfo.requiredTimes[stage].ToString());
        }

        title.text = I2.Loc.ScriptLocalization.Get(a.achievementInfo.titleString);
        description.text = descriptionString;
    }

    void SetProgressbar()
    {
        float currentAchievedProgress = 0;
        string progTxt;
        if (stage > 2) {
            currentAchievedProgress = ((float)a.progress / (float)a.achievementInfo.requiredValues[2]);
            progTxt = a.progress + " / " + a.achievementInfo.requiredValues[2];
        } else {
            currentAchievedProgress = ((float)a.progress / (float)a.achievementInfo.requiredValues[stage]);
            progTxt = a.progress + " / " + a.achievementInfo.requiredValues[stage];
        }

        progressSlider.value = currentAchievedProgress;
        progressText.text = progTxt;
    }

    void SetRewards()
    {
        string rewExp;
        string rewDiams;
        if (stage > 2) {
            rewExp = a.achievementInfo.starRewards[2].ToString();
            rewDiams = a.achievementInfo.diamondRewards[2].ToString();
        } else {
            rewExp = a.achievementInfo.starRewards[stage].ToString();
            rewDiams = a.achievementInfo.diamondRewards[stage].ToString();
        }

        rewardExp.text = rewExp;
        rewardDiamond.text = rewDiams;
    }

    void SetMedals()
    {
        for (int i = 0; i < 3; i++)
        {
            medalsOff[i].SetActive(true);
            medalsOn[i].SetActive(false);
        }

        for (int i = 0; i < Mathf.Clamp(stage, 0, 3); i++)
        {
            medalsOff[i].SetActive(false);
            medalsOn[i].SetActive(true);
        }
    }

    void SetCollectButton()
    {
        if (a.Collected)
        {
            progressSlider.gameObject.SetActive(true);
            collectButton.gameObject.SetActive(false);
        }
        else
        {
            collectButton.onClick.RemoveAllListeners();
            collectButton.onClick.AddListener(() =>
            {
                a.ClaimReward();
                collectButton.onClick.RemoveAllListeners();
            });

            progressSlider.gameObject.SetActive(false);
            collectButton.gameObject.SetActive(true);
        }
    }
}

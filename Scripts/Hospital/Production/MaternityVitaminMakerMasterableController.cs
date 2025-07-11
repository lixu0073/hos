using Hospital;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MaternityVitaminMakerMasterableController
{
    #region fields
    private int masteryLevel = 0;
    private int masteryProgress = 0;
    private int masteryGoal = 0;
    private MasterableConfigData masterableConfigData;
    //private bool sendEventToDelta = false;
    private const string VitaminMakerTag = "VitaminMaker";
    List<VitaminCollectorModel> vitaminModels;
    #endregion

    #region properties
    public int MasteryLevel
    {
        get { return masteryLevel; }
        private set
        {
            if (value < 0)
                return;
            masteryLevel = value;
        }
    }

    public int MasteryProgress
    {
        get { return masteryProgress; }
        protected set
        {
            if (value < 0)
                return;

            masteryProgress = value;
        }
    }

    public int MasteryGoal
    {
        get { return masteryGoal; }
        private set
        {
            if (value < 0)            
                return;

            masteryGoal = value;
        }
    }

    public MasterableConfigData MasterableConfigData
    {
        get { return masterableConfigData; }
        set { masterableConfigData = value; }
    }

    protected float coinRewardMultiplier;
    protected float expRewardMultiplier;
    protected float productionTimeMultiplier;
    public float CoinRewardMultiplier { get { return coinRewardMultiplier; } private set { } }
    public float ExpRewardMultiplier { get { return expRewardMultiplier; } private set { } }
    public float ProductionTimeMultiplier { get { return productionTimeMultiplier; } private set { } }
    #endregion

    #region publicMethods
    public MaternityVitaminMakerMasterableController(List<VitaminCollectorModel> vitaminModels)
    {
        this.vitaminModels = vitaminModels;
        Init();
    }

    public void AddMasteryProgress(int amount)
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }
        if (amount <= 0)        
            return;

        if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
        {
            SetMasteryProgress(MasteryGoal);
            return;
        }

        MasteryProgress += amount;
        CheckMasteryProgress();
        RefreshInfoPopup();
    }

    public void AddMasteryLevel(int amount)
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }

        if (amount <= 0)
            return;

        if (MasteryLevel + amount > MasterableConfigData.MasteryGoals.Length)
        {
            SetMasteryLevel(MasterableConfigData.MasteryGoals.Length);
            return;
        }
        MasteryLevel += amount;
        UpdateMasteryGoal();
        UpdateMasteryMultipliers();
        RefreshInfoPopup();
    }
    #endregion

    #region protectedMethods
    public string SaveToStringMastership()
    {
        StringBuilder save = new StringBuilder();
        save.Append(MasteryLevel.ToString());
        save.Append("/");
        save.Append(MasteryProgress.ToString());
        return save.ToString();
    }

    public void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
    {
        var str = save.Split(';');
        //sendEventToDelta = false;
        if (str.Length > 2)
        {
            List<string> strs = str[2].Split('/').Where(p => p.Length > 0).ToList();
            if (strs.Count > 1)
            {
                SetMasteryLevel(int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture));
                SetMasteryProgress(int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                SetMasteryLevel(0);
                SetMasteryProgress(actionsDone);
            }
        }
        else
        {
            SetMasteryLevel(0);
            SetMasteryProgress(actionsDone);
        }
        //sendEventToDelta = true;
    }

    protected void SetMasteryProgress(int progress)
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }

        if (MasteryGoal <= 0)        
            return;

        if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
        {
            MasteryProgress = MasteryGoal;
            return;
        }

        if (progress < 0)
            return;

        MasteryProgress = progress;
        CheckMasteryProgress();
        RefreshInfoPopup();
    }

    protected void SetMasteryLevel(int level)
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }

        if (level < 0)
        {
            SetMasteryLevel(0);
            return;
        }
        if (level > MasterableConfigData.MasteryGoals.Length)
        {
            SetMasteryLevel(MasterableConfigData.MasteryGoals.Length);
            return;
        }

        MasteryLevel = level;
        UpdateMasteryGoal();
        UpdateMasteryMultipliers();
        RefreshInfoPopup();
    }
    #endregion

    #region privateMethods
    public void CheckMasteryProgress()
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }

        if (MasteryGoal <= 0 || MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
            return;
            
        if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)        
            return;

        if (MasteryProgress >= MasteryGoal)
        {
            int progressLeft = MasteryProgress - MasteryGoal;
            AddMasteryLevel(1);
            SetMasteryProgress(progressLeft);
        }
    }

    private void Init()
    {
        if (MasterySystemParser.Instance == null)
        {
            Debug.LogError("MasterySystemParser.Instance is null");
            return;
        }
        MasterableConfigData = MasterySystemParser.Instance.GetMasterableConfigData(VitaminMakerTag);
        if (masterableConfigData == null)
        {
            Debug.LogError("masterableConfigData is null");
        }
        SetMasteryLevel(0);
        SetMasteryProgress(0);
    }

    private void UpdateMasteryGoal()
    {
        if (MasterableConfigData == null)
        {
            Debug.LogError("MasterableConfigData is null");
            return;
        }

        if (MasteryLevel >= MasterableConfigData.MasteryGoals.Length)
        {
            MasteryGoal = MasterableConfigData.MasteryGoals[MasterableConfigData.MasteryGoals.Length - 1];
            return;
        }
        MasteryGoal = MasterableConfigData.MasteryGoals[MasteryLevel];
    }

    protected void RefreshInfoPopup()
    {
        if (UIController.getMaternity.maternityInfoPopup.gameObject.activeInHierarchy)
        {
            UIController.getMaternity.maternityInfoPopup.RefreshPopup();
        }
    }
    #endregion

    #region abstractMethods
    public int CalcTimeToMastershipUpgrade(List<VitaminCollectorModel> vitaminModels, bool isCollectorFull)
    {
        float time = 0;
        int medicinesLeft = MasteryGoal - MasteryProgress;
        int vitaminToBeGenerated = 0;
        for (int i = 0; i < vitaminModels.Count; i++)
        {
            vitaminToBeGenerated += (vitaminModels[i].maxCapacity - (int)vitaminModels[i].capacity);
        }
        if (medicinesLeft > vitaminToBeGenerated || isCollectorFull)        
            return int.MaxValue;

        VitaminCollectorModelComparer vitComparer = new VitaminCollectorModelComparer();
        List<VitaminCollectorModel> sortedModels = new List<VitaminCollectorModel>(vitaminModels);
        sortedModels.Sort(vitComparer);

        for (int i = 0; i < sortedModels.Count; i++)
        {
            if (vitaminModels[i].FillRatio != 0 && vitaminModels[i].maxCapacity != 0)
            {
                int timeOfProduction = Mathf.CeilToInt(3600f / vitaminModels[i].FillRatio);
                float normalizedTimeToFinishCurrentProduction = Mathf.CeilToInt(vitaminModels[i].capacity) - vitaminModels[i].capacity;

                int amountOfVitaminsInQueue = vitaminModels[i].maxCapacity - Mathf.CeilToInt(vitaminModels[i].capacity);
                int allVitaminsInProcess = amountOfVitaminsInQueue + (normalizedTimeToFinishCurrentProduction > 0 ? 1 : 0);

                if (medicinesLeft - allVitaminsInProcess >= 0)
                {
                    medicinesLeft -= allVitaminsInProcess;
                    time += amountOfVitaminsInQueue * timeOfProduction + normalizedTimeToFinishCurrentProduction * timeOfProduction;
                    if (medicinesLeft == 0)
                        break;
                }
                else
                {
                    time += (medicinesLeft - 1) * timeOfProduction + normalizedTimeToFinishCurrentProduction * timeOfProduction;
                    break;
                }
            }
            else
            {
                return int.MaxValue;
                //Debug.LogError("Dzielenie przez zero");
            }
        }
        return Mathf.CeilToInt(time);
    }

    protected void UpdateMasteryMultipliers()
    {
        coinRewardMultiplier = MasteryLevel > 0 ? ((MasterableProductionMachineConfigData)MasterableConfigData).GoldMultiplier : 1;
        expRewardMultiplier = MasteryLevel > 1 ? ((MasterableProductionMachineConfigData)MasterableConfigData).ExpMultiplier : 1;
        productionTimeMultiplier = MasteryLevel > 2 ? ((MasterableProductionMachineConfigData)MasterableConfigData).ProductionTimeMultiplier : 1;

        for (int i = 0; i < vitaminModels.Count; i++)
        {
            //vitaminModels[i].UpgradeFillRatioDueToMastery(productionTimeMultiplier);
            vitaminModels[i].SetFillRatioMultiplierBalanceable((MasterableProductionMachineConfigData)MasterableConfigData);
        }

        for (int i = 0; i < HospitalPatientAI.Patients.Count; ++i)
        {
            HospitalPatientAI.Patients[i].UpdateReward();
        }
    }
    #endregion
}

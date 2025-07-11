using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Maternity;

public class MaternityCharacterInfo : BaseCharacterInfo
{
    public int MaxPreLabourTime;
    public int MaxLabourTime;
    public int MaxHealingAndBondingTime;
    public bool PatientShown = false;

    public Transform babyPosition;
    public Dictionary<MedicineDatabaseEntry, int> RequiredCures = new Dictionary<MedicineDatabaseEntry, int>();

    public override void Initialize() { }

    public void Randomize()
    {
        MaxHealingAndBondingTime = MaternityCoreLoopParametersHolder.GetBondingTime();
        int TotalLabourTime = MaternityCoreLoopParametersHolder.GetLaborStageTime();

        MaxPreLabourTime = UnityEngine.Random.Range((int)(TotalLabourTime * 0.25f), (int)(TotalLabourTime * 0.75f) + 1);
        MaxLabourTime = TotalLabourTime - MaxPreLabourTime;
    }

    public enum Stage
    {
        Diagnose = 1,
        Vitamines = 2,
        WaitingForLabor = 3,
        InLabor = 4,
        HealingAndBounding = 5
    }

    private static float lowerRandomFactor = 0.95f;
    private int minExpForSendToDiagnose = Mathf.Max(1, Mathf.FloorToInt(lowerRandomFactor * 4));
    private int minExpForVitamines = Mathf.Max(1, Mathf.FloorToInt(lowerRandomFactor * 1));
    private int minExpForLabor = Mathf.Max(1, Mathf.FloorToInt(lowerRandomFactor * 16));
    private int minExpForHealingAndBonding = Mathf.Max(1, Mathf.FloorToInt(lowerRandomFactor * 88));

    public int GetExpForStage(Stage stage)
    {
        if (!Game.Instance.gameState().IsMaternityFirstLoopCompleted)
        {
            switch (stage)
            {
                case Stage.Vitamines:
                case Stage.HealingAndBounding:
                    return MaternityCoreLoopParametersHolder.GetExpInFirstLoopForStage(stage);
                case Stage.WaitingForLabor:
                    return MaternityCoreLoopParametersHolder.GetExpInFirstLoopForStage(stage, (float)MaxPreLabourTime / (MaxLabourTime + MaxPreLabourTime));
                case Stage.InLabor:
                    return MaternityCoreLoopParametersHolder.GetExpInFirstLoopForStage(stage, (float)MaxLabourTime / (MaxLabourTime + MaxPreLabourTime));
            }
        }

        int temp;

        switch (stage)
        {
            case Stage.Diagnose:
                temp = MaternityCoreLoopParametersHolder.GetBloodTestExpReward();
                return Mathf.Max(minExpForSendToDiagnose, temp);
            case Stage.Vitamines:
                int sumToReturn = 0;
                foreach (KeyValuePair<MedicineDatabaseEntry, int> vitaminRequired in RequiredCures)
                {
                    temp = MaternityCoreLoopParametersHolder.GetMaternityExpForVitamin(vitaminRequired.Key, 1);
                    sumToReturn += vitaminRequired.Value * Mathf.Max(minExpForVitamines, temp);
                }
                return sumToReturn;
            case Stage.WaitingForLabor:
                temp = MaternityCoreLoopParametersHolder.GetMaternityExpForLaborStage(MaxPreLabourTime);
                return Mathf.Max(minExpForLabor, temp);
            case Stage.InLabor:
                temp = MaternityCoreLoopParametersHolder.GetMaternityExpForLaborStage(MaxLabourTime);
                return Mathf.Max(minExpForLabor, temp);
            case Stage.HealingAndBounding:
                temp = MaternityCoreLoopParametersHolder.GetMaternityExpForBondingStage(MaxHealingAndBondingTime);
                return Mathf.Max(minExpForHealingAndBonding, temp);
            default:
                return 10;
        }
    }

    public override string GetLikesString()
    {
        return I2.Loc.ScriptLocalization.Get("MOTHERS_LIKES/LIKES_" + Likes);
    }

    public override string GetDislikesString()
    {
        return "";
    }

    #region Save Load

    private char MAIN_SEPARATOR = '@';
    private char SUB_SEPARATOR = '*';
    private char CURES_SEPARATOR = '#';

    private enum INDEX
    {
        MaxPreLabourTime = 0,
        MaxLabourTime = 1,
        MaxHealingAndBondingTime = 2,
        PatientShown = 3,
        RequiredCures = 4
    }

    public void FromString(string save)
    {
        string[] main = save.Split(MAIN_SEPARATOR);
        int size = main.Length;
        if (size > (int)INDEX.MaxPreLabourTime)
        {
            MaxPreLabourTime = int.Parse(main[(int)INDEX.MaxPreLabourTime], System.Globalization.CultureInfo.InvariantCulture);
        }
        if (size > (int)INDEX.MaxLabourTime)
        {
            MaxLabourTime = int.Parse(main[(int)INDEX.MaxLabourTime], System.Globalization.CultureInfo.InvariantCulture);
        }
        if (size > (int)INDEX.MaxHealingAndBondingTime)
        {
            MaxHealingAndBondingTime = int.Parse(main[(int)INDEX.MaxHealingAndBondingTime], System.Globalization.CultureInfo.InvariantCulture);
        }
        if (size > (int)INDEX.PatientShown)
        {
            PatientShown = int.Parse(main[(int)INDEX.PatientShown], System.Globalization.CultureInfo.InvariantCulture) == 1;
        }
        if (size > (int)INDEX.RequiredCures)
        {
            LoadRequiredCuresFromString(main[(int)INDEX.RequiredCures]);
        }
    }

    private void LoadRequiredCuresFromString(string unparsedCures)
    {
        if (string.IsNullOrEmpty(unparsedCures))
            return;
        string[] data = unparsedCures.Split(SUB_SEPARATOR);
        foreach (string requiredUnparsedCure in data)
        {
            if (string.IsNullOrEmpty(requiredUnparsedCure))
                continue;
            string[] requiredUnparsedCureArray = requiredUnparsedCure.Split(CURES_SEPARATOR);
            if (requiredUnparsedCureArray.Length >= 2)
            {
                RequiredCures.Add(ResourcesHolder.Get().GetMedicineInfos(MedicineRef.Parse(requiredUnparsedCureArray[0])), int.Parse(requiredUnparsedCureArray[1]));
            }
        }
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        SaveMaxPreLabourTime(builder);
        builder.Append(MAIN_SEPARATOR);
        SaveMaxLabourTime(builder);
        builder.Append(MAIN_SEPARATOR);
        SaveMaxHealingAndBondingTime(builder);
        builder.Append(MAIN_SEPARATOR);
        builder.Append(PatientShown ? 1 : 0);
        builder.Append(MAIN_SEPARATOR);
        SaveRequiredCures(builder);
        return builder.ToString();
    }

    private void SaveMaxHealingAndBondingTime(StringBuilder builder)
    {
        builder.Append(MaxHealingAndBondingTime);
    }

    private void SaveMaxPreLabourTime(StringBuilder builder)
    {
        builder.Append(MaxPreLabourTime);
    }

    private void SaveMaxLabourTime(StringBuilder builder)
    {
        builder.Append(MaxLabourTime);
    }

    private void SaveRequiredCures(StringBuilder builder)
    {
        bool first = true;
        foreach (KeyValuePair<MedicineDatabaseEntry, int> data in RequiredCures)
        {
            if (!first)
            {
                builder.Append(SUB_SEPARATOR);
            }
            builder.Append(data.Key.GetMedicineRef().ToString());
            builder.Append(CURES_SEPARATOR);
            builder.Append(data.Value);
            first = false;
        }
    }

    #endregion

}

using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;
using System.Collections.Generic;

public class DiagnosePatientObjective : Objective
{
    public DiagnosePatientType diagnosePatientType;
    protected DiseaseType diseaseType;

    public DiagnosePatientObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData != null && objectiveData.OtherParameters != null && objectiveData.OtherParameters.Length > 1)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);

            this.diagnosePatientType = (DiagnosePatientType)Enum.Parse(typeof(DiagnosePatientType), objectiveData.OtherParameters[0]);

            if (this.diagnosePatientType == DiagnosePatientType.SingleMachine)
                this.diseaseType = (DiseaseType)Enum.Parse(typeof(DiseaseType), objectiveData.OtherParameters[1]);

            SetTaskObjectives(objectiveData.Progress, reward);

            AddListener();
            return true;
        }
        else return false;
    }

    public bool InitWithRandom(DiagnosePatientType diagnosePatientType)
    {
        this.diagnosePatientType = diagnosePatientType;

        return InitWithRandom();
    }

    public override bool InitWithRandom()
    {
        int tmp_amount = AlgorithmHolder.GetObjectiveProgressForDiagnosePatient(diagnosePatientType);

        List<MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicines();

        List<DiseaseType> unlockedDiagnoseDiseases = new List<DiseaseType>();

        foreach (MedicineDatabaseEntry med in unlockedMedicines)
        {
            if (med.Disease == null)
                continue;
            else
            {
                if (med.Disease.DiseaseType == DiseaseType.Lungs || med.Disease.DiseaseType == DiseaseType.Bone || med.Disease.DiseaseType == DiseaseType.Brain ||
                    med.Disease.DiseaseType == DiseaseType.Ear || med.Disease.DiseaseType == DiseaseType.Kidneys)
                {
                    if (diagnosePatientType == DiagnosePatientType.AnyMachine)
                    {
                        this.diseaseType = DiseaseType.None;

                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        return true;
                    }
                    else
                    {
                        if (!unlockedDiagnoseDiseases.Contains(med.Disease.DiseaseType))
                            unlockedDiagnoseDiseases.Add(med.Disease.DiseaseType);
                    }
                }
            }
        }

        if (unlockedDiagnoseDiseases.Count > 0)
        {
            this.diseaseType = unlockedDiagnoseDiseases[GameState.RandomNumber(0, unlockedDiagnoseDiseases.Count)];
            SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
            return true;
        }

        return false;
    }

    public override string GetDescription()
    {
        switch (diagnosePatientType)
        {
            case DiagnosePatientType.SingleMachine:
                string diagnoseString = GetDiagnoseName();
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_DIAGNOSE_IN_GIVEN"), diagnoseString);
            default:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_DIAGNOSE_IN_ANY");
        }
    }

    public override string GetInfoDescription()
    {
        switch (diagnosePatientType)
        {
            case DiagnosePatientType.SingleMachine:
                string diagnoseString = GetDiagnoseName();
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_DIAGNOSE_IN_GIVEN"), progressObjective, diagnoseString);
            default:
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_DIAGNOSE_IN_ANY"), progressObjective);
        }
    }

    private string GetDiagnoseName()
    {
        string diagnoseString = "";

        switch ((int)diseaseType)
        {
            case (int)DiseaseType.Brain:
                diagnoseString = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/MRI_SCANNER");
                break;
            case (int)DiseaseType.Bone:
                diagnoseString = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/X_RAY");
                break;
            case (int)DiseaseType.Ear:
                diagnoseString = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/ULTRASOUND_STATION");
                break;
            case (int)DiseaseType.Lungs:
                diagnoseString = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/LUNG_TESTING_STATION");
                break;
            case (int)DiseaseType.Kidneys:
                diagnoseString = I2.Loc.ScriptLocalization.Get("SHOP_TITELS/LASER_SCANNER");
                break;
            default:
                diagnoseString = "";
                break;
        }

        return diagnoseString;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(diagnosePatientType.ToString());
        builder.Append("!");
        builder.Append(diseaseType.ToString());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        var goalDataSave = saveString.Split('!');

        diagnosePatientType = (DiagnosePatientType)Enum.Parse(typeof(DiagnosePatientType), goalDataSave[5]);
        diseaseType = (DiseaseType)Enum.Parse(typeof(DiseaseType), goalDataSave[6]);
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.DiagnosePatientObjectiveUpdate.Notification += UpdateProgresChanged;
        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Notification += UpdateProgresWithDiseaseChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.DiagnosePatientObjectiveUpdate.Notification -= UpdateProgresChanged;
        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Notification -= UpdateProgresWithDiseaseChanged;
    }

    private void UpdateProgresChanged(ObjectiveEventArgs eventArgs)
    {
        if (!completed && diagnosePatientType == DiagnosePatientType.AnyMachine)
            base.UpdateObjective(eventArgs.amount);
    }

    private void UpdateProgresWithDiseaseChanged(ObjectiveDiagnosePatientWithDiseaseEventArgs eventArgs)
    {
        if (!completed)
        {
            switch (diagnosePatientType)
            {
                case DiagnosePatientType.SingleMachine:
                    if (eventArgs.diseaseType == this.diseaseType)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                default:
                    break;
            }
        }
    }

    public enum DiagnosePatientType
    {
        AnyMachine,
        SingleMachine,
    }
}

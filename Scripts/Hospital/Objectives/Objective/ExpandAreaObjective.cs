using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;

public class ExpandAreaObjective : Objective
{
    public HospitalArea area;

    public ExpandAreaObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData!=null && objectiveData.OtherParameters != null && objectiveData.OtherParameters.Length > 0)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);
            SetTaskObjectives(objectiveData.Progress, (HospitalArea)Enum.Parse(typeof(HospitalArea), objectiveData.OtherParameters[0]), reward);
            AddListener();
            return true;
        }
        else return false;
    }

    public override bool InitWithRandom()
    {
        int laboratoryObjectsSize = HospitalAreasMapController.HospitalMap.GetAllRotatableObjectSizeFromArea(HospitalArea.Laboratory);
        int clinicObjectsSize = HospitalAreasMapController.HospitalMap.GetAllRotatableObjectSizeFromArea(HospitalArea.Clinic);

        int laboratorySize = HospitalAreasMapController.HospitalMap.GetAreaSize(HospitalArea.Laboratory);
        int clinicSize = HospitalAreasMapController.HospitalMap.GetAreaSize(HospitalArea.Clinic);


        float percenLab = -1;
        float percenClinic = -1;

        if (laboratoryObjectsSize > 0 && laboratorySize > 0)
        {
            percenLab = (float)laboratoryObjectsSize / (float)laboratorySize;

            //Debug.LogError("Lab percent area with content: " + percenLab);
        }

        if (clinicObjectsSize > 0 && clinicSize > 0)
        {
            percenClinic = (float)clinicObjectsSize / (float)clinicSize;

            //Debug.LogError("Clinic percent area with content: " + percenClinic);
        }

        if (percenLab != -1 && percenClinic != -1)
        {
            if (percenLab >= percenClinic)
            {
                if (percenLab > 0.90f)
                {
					int laboratoryToBuyCounter = HospitalAreasMapController.HospitalMap.GetAreaToBuyCounter(HospitalArea.Laboratory);
								
                    if (laboratoryToBuyCounter>1)
                        SetTaskObjectives(2, HospitalArea.Laboratory, new DefaultObjectiveReward(ResourceType.Coin, false));
                    else
                        SetTaskObjectives(1, HospitalArea.Laboratory, new DefaultObjectiveReward(ResourceType.Coin, false));

                    return true;
                }
                else if (percenLab > 0.80f)
                {
                    SetTaskObjectives(1, HospitalArea.Laboratory, new DefaultObjectiveReward(ResourceType.Coin, false));
                    return true;
                }
            }
            else
            {
                if (percenClinic > 0.90f)
                {
					int clinicToBuyCounter = HospitalAreasMapController.HospitalMap.GetAreaToBuyCounter(HospitalArea.Clinic);
								
                    if (clinicToBuyCounter>1)
                        SetTaskObjectives(2, HospitalArea.Hospital, new DefaultObjectiveReward(ResourceType.Coin, false));
                    else
                        SetTaskObjectives(1, HospitalArea.Hospital, new DefaultObjectiveReward(ResourceType.Coin, false));

                    return true;
                }
                else if (percenClinic > 0.80f)
                {
                    SetTaskObjectives(1, HospitalArea.Hospital, new DefaultObjectiveReward(ResourceType.Coin, false));
                    return true;
                }
            }
        }

        return false;
    }

    public override string GetDescription()
    {
        if (area == HospitalArea.Laboratory)
            return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_EXPAND_LAB");
        else return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_EXPAND_HOSPITAL");
    }

    public override string GetInfoDescription()
    {
        if (area == HospitalArea.Laboratory)
            return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_EXPAND_LAB");
        else return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_EXPAND_HOSPITAL");
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(area.ToString());
        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        var goalDataSave = saveString.Split('!');
        area = (HospitalArea)Enum.Parse(typeof(HospitalArea), goalDataSave[5]);

        ValidateGoal();
    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics();
    }

    protected void SetTaskObjectives(int amount, HospitalArea area, DefaultObjectiveReward reward)
    {
        base.SetTaskObjectives(amount, reward);
        this.area = area;

        ValidateGoal();
    }

    private void ValidateGoal()
    {
        if (HospitalAreasMapController.HospitalMap != null)
        {
            int laboratoryToBuyCounter = HospitalAreasMapController.HospitalMap.GetAreaToBuyCounter(HospitalArea.Laboratory);
            int clinicToBuyCounter = HospitalAreasMapController.HospitalMap.GetAreaToBuyCounter(HospitalArea.Clinic);

            if ((clinicToBuyCounter == 0 && area == HospitalArea.Hospital) || (laboratoryToBuyCounter == 0 && area == HospitalArea.Laboratory))
                CompleteGoal();
        }
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.ExpandAreaObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.ExpandAreaObjectiveUpdate.Notification -= UpdateProgresChanged;
    }

    private void UpdateProgresChanged(ObjectiveExpandAreaEventArgs eventArgs)
    {
        if (!completed && (eventArgs.area == this.area || (eventArgs.area == HospitalArea.Clinic && area == HospitalArea.Hospital)))
            base.UpdateObjective(eventArgs.amount);
    }
}

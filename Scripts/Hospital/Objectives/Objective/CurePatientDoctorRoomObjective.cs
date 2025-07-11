using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;
using System.Collections.Generic;

public class CurePatientDoctorRoomObjective : Objective
{
    public DoctorPatientType doctorPatientType;
    public string rotatableTag;

    public CurePatientDoctorRoomObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData != null && objectiveData.OtherParameters != null && objectiveData.OtherParameters.Length > 0)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);

            this.doctorPatientType = (DoctorPatientType)Enum.Parse(typeof(DoctorPatientType), objectiveData.OtherParameters[0]);

            if (this.doctorPatientType == DoctorPatientType.SingleRoom)
                this.rotatableTag = objectiveData.OtherParameters[1];


            SetTaskObjectives(objectiveData.Progress, reward);
            AddListener();
            return true;
        }
        else return false;
    }

    public bool InitWithRandom(DoctorPatientType doctorPatientType)
    {
        this.doctorPatientType = doctorPatientType;
        return InitWithRandom();
    }

    public override bool InitWithRandom()
    {
        int tmp_amount = 0;

        switch (doctorPatientType)
        {
            case DoctorPatientType.Kid:
                if ((Game.Instance.gameState().GetHospitalLevel() >= HospitalAreasMapController.HospitalMap.playgroud.roomInfo.UnlockLvl))
                {
                    tmp_amount = AlgorithmHolder.GetObjectiveProgressForCureKidstInDoctor();
                    SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                    return true;
                }
                break;
            case DoctorPatientType.SingleRoom:
                {
                    List<DoctorRoomInfo> doctorsAvaiable = new List<DoctorRoomInfo>();

                    for (int i = 0; i < ResourcesHolder.GetHospital().ClinicDiseases.Count; i++)
                    {
                        if (ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor.unlockLVL <= Game.Instance.gameState().GetHospitalLevel())
                        {
                            doctorsAvaiable.Add(ResourcesHolder.GetHospital().ClinicDiseases[i].Doctor);
                        }
                    }

                    if (doctorsAvaiable.Count > 0)
                    {
                        // jezeli wieksze od 3 to sprawdzaj ostatnie 3 jak nie to normalnie randomuj po liscie
                        if (doctorsAvaiable.Count > 3)
                        {
                            string[] existingTags = ObjectivesSynchronizer.Instance.GetAllObjectiveParams(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientSingleDoctorRoom);

                            if (existingTags != null)
                            {
                                for (int i = 0; i < existingTags.Length; i++)
                                {
                                    if (!string.IsNullOrEmpty(existingTags[i]))
                                    {
                                        doctorsAvaiable.RemoveAll(item => item.Tag == existingTags[i]);
                                    }
                                }
                            }
                        }

                        int rnd = GameState.RandomNumber(0, doctorsAvaiable.Count);
                        tmp_amount = AlgorithmHolder.GetObjectiveProgressForCurePatientInDoctor(doctorsAvaiable[rnd].Tag);
                        this.rotatableTag = doctorsAvaiable[rnd].Tag;
                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        ObjectivesSynchronizer.Instance.UpdateParam(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientSingleDoctorRoom, this.rotatableTag);
                        return true;
                    }

                }
                break;
            default:
				tmp_amount = Mathf.FloorToInt(AlgorithmHolder.GetObjectiveProgressForCurePatientInDoctor("BlueDoc", true));
                SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                return true;
        }

        return false; // DON'T GERNEATE LEVEL GOAL FOR CURE PATIENT IN DOCTOR ROOM
    }

    public override string GetDescription()
    {
        if (doctorPatientType == DoctorPatientType.SingleRoom)
        {
            if (this.rotatableTag == "BlueDoc")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_1");
            else if (this.rotatableTag == "YellowDoc")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_3");
            else if (this.rotatableTag == "GreenDoc")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_5");
            else if (this.rotatableTag == "WhiteDoc")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_8");
            else if (this.rotatableTag == "AnyDoc")
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_12");
            else
            {
                string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(this.rotatableTag));
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_DOCTOR_CURE"), roomName);
            }
        }
        else if (doctorPatientType == DoctorPatientType.AnyRoom)
            return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_12");
        else if (doctorPatientType == DoctorPatientType.Kid)
            return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_KIDS_CURE");
        else return "";

    }

    public override string GetInfoDescription()
    {
        if (doctorPatientType == DoctorPatientType.SingleRoom)
        {
            string roomName = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(this.rotatableTag));
            return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_DOCTOR_CURE"), progressObjective, roomName);
        }
        else if (doctorPatientType == DoctorPatientType.AnyRoom)
            return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_DOCTOR_CURE_ANY"), progressObjective);
        else if (doctorPatientType == DoctorPatientType.Kid)
            return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_KIDS_CURE"), progressObjective);
        else return "";
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(doctorPatientType.ToString());

        if (doctorPatientType == DoctorPatientType.SingleRoom)
        {
            builder.Append("!");
            builder.Append(rotatableTag);
        }

        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        var goalDataSave = saveString.Split('!');
        this.doctorPatientType = (DoctorPatientType)Enum.Parse(typeof(DoctorPatientType), goalDataSave[5]);

        if (doctorPatientType == DoctorPatientType.SingleRoom)
            rotatableTag = goalDataSave[6];

    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics() + rotatableTag;
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.DoctorPatientObjectiveUpdate.Notification += UpdateProgresChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.DoctorPatientObjectiveUpdate.Notification -= UpdateProgresChanged;
    }

    private void UpdateProgresChanged(ObjectiveDoctorPatientEventArgs eventArgs)
    {
        if (!completed)
        {
            switch (doctorPatientType)
            {
                case DoctorPatientType.SingleRoom:
                    if (eventArgs.rotatableTag == this.rotatableTag)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                case DoctorPatientType.Kid:
                    if (eventArgs.isKid)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                default:
                    base.UpdateObjective(eventArgs.amount);
                    break;
            }
        }
    }

    public enum DoctorPatientType
    {
        AnyRoom,
        SingleRoom,
        Kid,
    }
}

using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;
using System.Collections.Generic;

public class CurePatientHospitalRoomObjective : Objective
{
    protected DiseaseType diseaseType;
    protected BloodType bloodType;
    protected int sex;

    HospitalPatientType hospitalPatientRandomType;

    public CurePatientHospitalRoomObjective() : base()
    {
        SetType();
    }

    public override bool Init(ObjectiveData objectiveData)
    {
        if (objectiveData != null && objectiveData.OtherParameters != null && objectiveData.OtherParameters.Length > 0)
        {
            DefaultObjectiveReward reward = DefaultObjectiveReward.Parse(objectiveData.Reward);

            this.hospitalPatientRandomType = (HospitalPatientType)Enum.Parse(typeof(HospitalPatientType), objectiveData.OtherParameters[0]);

            if (hospitalPatientRandomType == HospitalPatientType.RandomDisease)
            {
                if (objectiveData.OtherParameters.Length > 1)
                    this.diseaseType = (DiseaseType)Enum.Parse(typeof(DiseaseType), objectiveData.OtherParameters[1]);
                else return false;
            }
            else if (hospitalPatientRandomType == HospitalPatientType.RandomGender)
            {
                if (objectiveData.OtherParameters.Length > 1)
                    this.sex = int.Parse(objectiveData.OtherParameters[1], System.Globalization.CultureInfo.InvariantCulture);
                else return false;
            }
            else if (hospitalPatientRandomType == HospitalPatientType.RandomBloodType)
            {
                if (objectiveData.OtherParameters.Length > 1)
                    this.bloodType = (BloodType)Enum.Parse(typeof(BloodType), objectiveData.OtherParameters[1]);
                else return false;
            }

            SetTaskObjectives(objectiveData.Progress, reward);
            AddListener();

            return true;
        }
        else return false;
    }


    public override bool InitWithRandom()
    {
        int tmp_amount = AlgorithmHolder.GetObjectiveProgressForCurePatientInHospitalRoom(hospitalPatientRandomType);

        int counterTrigger = 0;


        switch (hospitalPatientRandomType)
        {
            case HospitalPatientType.RandomDisease:
                {
                    List<MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicines();

                    List<DiseaseType> unlockedDiseases = new List<DiseaseType>();

                    foreach (MedicineDatabaseEntry med in unlockedMedicines)
                    {
                        if (med.Disease == null)
                            continue;

                        if (med.Disease.DiseaseType == DiseaseType.Head || med.Disease.DiseaseType == DiseaseType.Bacteria)
                            continue;

                        if (!unlockedDiseases.Contains(med.Disease.DiseaseType))
                            unlockedDiseases.Add(med.Disease.DiseaseType);
                    }

                    if (unlockedDiseases.Count > 0)
                    {
                        // jezeli wieksze od 3 to sprawdzaj ostatnie 3 jak nie to normalnie randomuj po liscie
                        if (unlockedDiseases.Count > 3)
                        {
                            string[] existingTags = ObjectivesSynchronizer.Instance.GetAllObjectiveParams(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithDisease);

                            if (existingTags != null)
                            {
                                for (int i = 0; i < existingTags.Length; i++)
                                {
                                    if (!string.IsNullOrEmpty(existingTags[i]))
                                    {
                                        DiseaseType disType = (DiseaseType)Enum.Parse(typeof(DiseaseType), existingTags[i]);
                                        unlockedDiseases.RemoveAll(item => item == disType);
                                    }
                                }
                            }
                        }

                        this.diseaseType = unlockedDiseases[GameState.RandomNumber(0, unlockedDiseases.Count)];
                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        ObjectivesSynchronizer.Instance.UpdateParam(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithDisease, this.diseaseType.ToString());
                        return true;
                    }
                }
                break;
            case HospitalPatientType.Bacteria:
                {
                    bool canBeSet = false;

                    counterTrigger = ObjectivesSynchronizer.Instance.GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithBacteria);

                    List<MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicines();

                    foreach (MedicineDatabaseEntry med in unlockedMedicines)
                    {
                        if (med.Disease == null)
                            continue;
                        else if (med.Disease.DiseaseType == DiseaseType.Bacteria)
                        {
                            SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                            canBeSet = true;
                            break;
                        }
                    }

                    if (canBeSet)
                    {
                        if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 != counterTrigger)
                            canBeSet = false;

                        if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 == 0)
                            ObjectivesSynchronizer.Instance.UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithBacteria, GameState.RandomNumber(0, 3));
                    }

                    return canBeSet;

                }
            case HospitalPatientType.Plant:
                {
                    List<MedicineDatabaseEntry> unlockedMedicines = ResourcesHolder.Get().EnumerateKnownMedicines();

                    foreach (MedicineDatabaseEntry med in unlockedMedicines)
                    {
                        if (med.Disease == null)
                            continue;
                        else if (med.GetMedicineRef().type == MedicineType.BasePlant)
                        {
                                SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                                return true;
                        }
                    }
                }
                break;
            case HospitalPatientType.RandomGender:
                {
                    bool canBeSet = false;

                    counterTrigger = ObjectivesSynchronizer.Instance.GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithGender);

                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 == counterTrigger)
                    {
                        sex = GameState.RandomNumber(0, 100) > 50 ? 1 : 0;
                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        canBeSet = true;
                    }

                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 == 0)
                        ObjectivesSynchronizer.Instance.UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithGender, GameState.RandomNumber(0, 3));

                    return canBeSet;
                }
            case HospitalPatientType.RandomBloodType:
                {
                    bool canBeSet = false;

                    counterTrigger = ObjectivesSynchronizer.Instance.GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithBloodType);

                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 == counterTrigger)
                    {
                        float r = GameState.RandomNumber(0, 100);
                        if (r < 41.9f)
                            bloodType = BloodType.Op;
                        else if (r < 73.1f)
                            bloodType = BloodType.Ap;
                        else if (r < 88.5f)
                            bloodType = BloodType.Bp;
                        else if (r < 93.3f)
                            bloodType = BloodType.ABp;
                        else if (r < 96.2f)
                            bloodType = BloodType.Om;
                        else if (r < 98.9f)
                            bloodType = BloodType.Am;
                        else if (r < 99.7f)
                            bloodType = BloodType.Bm;
                        else
                            bloodType = BloodType.ABm;

                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        canBeSet = true;
                    }
                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 3 == 0)
                        ObjectivesSynchronizer.Instance.UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType.CurePatientHospitalRoomWithBloodType, GameState.RandomNumber(0, 3));

                    return canBeSet;
                }
            case HospitalPatientType.VIP:

                counterTrigger = ObjectivesSynchronizer.Instance.GetObjectiveCounterTriger(ObjectivesSaveData.ObjectiveGeneratorDataType.CureVIP);

                if (Game.Instance.gameState().GetHospitalLevel() >= HospitalAreasMapController.HospitalMap.vipRoom.roomInfo.UnlockLvl)
                {
                    bool canBeSet = false;

                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 5 == counterTrigger)
                    {
                        SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                        canBeSet = true;
                    }

                    if (ObjectivesSynchronizer.Instance.ObjectivesCounter % 5 == 0)
                        ObjectivesSynchronizer.Instance.UpdateCounterTrigger(ObjectivesSaveData.ObjectiveGeneratorDataType.CureVIP, GameState.RandomNumber(0, 5));

                    return canBeSet;
                }
                break;
            default:
                base.SetTaskObjectives(tmp_amount, new DefaultObjectiveReward(ResourceType.Coin, false));
                return true;
        }

        return false;
    }

    public bool InitWithRandom(HospitalPatientType hospitalPatientRandomType)
    {
        this.hospitalPatientRandomType = hospitalPatientRandomType;
        return InitWithRandom(); 
    }

    public override string GetDescription()
    {
        switch (hospitalPatientRandomType)
        {
            case HospitalPatientType.RandomDisease:
                string diseaseName = GetDiseaseName();
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_ORGAN"), diseaseName);
            case HospitalPatientType.RandomGender:
                string sexString = CharacterCreator.GetSexString(sex);
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_SEX"), sexString);
            case HospitalPatientType.RandomBloodType:
                string bloodString = CharacterCreator.GetBloodTypeString(bloodType);
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_BLOODTYPE"), bloodString);
            case HospitalPatientType.Bacteria:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_BACTERIA");
            case HospitalPatientType.Plant:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_PLANT");
            case HospitalPatientType.VIP:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_CURE_VIP");
            default:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_2");
        }
    }

    public override string GetInfoDescription()
    {
        switch (hospitalPatientRandomType)
        {
            case HospitalPatientType.RandomDisease:
                string diseaseName = GetDiseaseName();
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_ORGAN"), progressObjective, diseaseName);
            case HospitalPatientType.RandomGender:
                string sexString = CharacterCreator.GetSexString(sex);
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_SEX"), progressObjective, sexString);
            case HospitalPatientType.RandomBloodType:
                string bloodString = CharacterCreator.GetBloodTypeString(bloodType);
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_BLOODTYPE"), progressObjective, bloodString);
            case HospitalPatientType.Bacteria:
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_BACTERIA"), progressObjective);
            case HospitalPatientType.Plant:
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_PLANT"), progressObjective);
            case HospitalPatientType.VIP:
                return string.Format(I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/TO_DO_INFO_CURE_VIP"), progressObjective);
            default:
                return I2.Loc.ScriptLocalization.Get("LEVEL_GOAL_SYSTEM/LEVEL_BONUS_GOAL_2");
        }
    }

    private string GetDiseaseName()
    {
        string diseaseName = "";

        switch (diseaseType)
        {
            case DiseaseType.Head:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/HEAD");
                break;
            case DiseaseType.Brain:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/BRAIN");
                break;
            case DiseaseType.Bone:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/BONES");
                break;
            case DiseaseType.Ear:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/EARS");
                break;
            case DiseaseType.Lungs:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/LUNGS");
                break;
            case DiseaseType.Kidneys:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/KIDNEYS");
                break;
            case DiseaseType.Eye:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/EYES");
                break;
            case DiseaseType.Nose:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/NOSE");
                break;
            case DiseaseType.Lips:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/LIPS");
                break;
            case DiseaseType.Throat:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/THROAT");
                break;
            case DiseaseType.Heart:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/HEART");
                break;
            case DiseaseType.Skin:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/SKIN");
                break;
            case DiseaseType.Tummy:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/TUMMY");
                break;
            case DiseaseType.Hand:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/HANDS");
                break;
            case DiseaseType.Foot:
                diseaseName = I2.Loc.ScriptLocalization.Get("SICKNESS/FEET");
                break;
            default:
                break;
        }

        return diseaseName;
    }

    public override string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(base.SaveToString());
        builder.Append("!");
        builder.Append(hospitalPatientRandomType.ToString());
        if (hospitalPatientRandomType == HospitalPatientType.RandomDisease)
        {
            builder.Append("!");
            builder.Append(diseaseType.ToString());
        }
        else if (hospitalPatientRandomType == HospitalPatientType.RandomGender)
        {
            builder.Append("!");
            builder.Append(Checkers.CheckedAmount(sex, 0, int.MaxValue, "Objective gender: ").ToString());
        }
        else if (hospitalPatientRandomType == HospitalPatientType.RandomBloodType)
        {
            builder.Append("!");
            builder.Append(bloodType.ToString());
        }

        return builder.ToString();
    }

    public override void LoadFromString(string saveString)
    {
        base.LoadFromString(saveString);
        var goalDataSave = saveString.Split('!');

        hospitalPatientRandomType = (HospitalPatientType)Enum.Parse(typeof(HospitalPatientType), goalDataSave[5]);

        switch (hospitalPatientRandomType)
        {
            case HospitalPatientType.RandomDisease:
                diseaseType = (DiseaseType)Enum.Parse(typeof(DiseaseType), goalDataSave[6]);
                break;
            case HospitalPatientType.RandomGender:
                sex = int.Parse(goalDataSave[6], System.Globalization.CultureInfo.InvariantCulture);
                break;
            case HospitalPatientType.RandomBloodType:
                bloodType = (BloodType)Enum.Parse(typeof(BloodType), goalDataSave[6]);
                break;
            default:
                break;
        }
    }

    public override string GetObjectiveNameForAnalitics()
    {
        return base.GetObjectiveNameForAnalitics() + diseaseType;
    }

    public override void AddListener()
    {
        RemoveListener();
        ObjectiveNotificationCenter.Instance.HospitalPatientWithInfoObjectiveUpdate.Notification += UpdateProgresChanged;
        ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Notification += UpdateProgresWithDieseaseChanged;
    }

    protected override void RemoveListener()
    {
        ObjectiveNotificationCenter.Instance.HospitalPatientWithInfoObjectiveUpdate.Notification -= UpdateProgresChanged;
        ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Notification -= UpdateProgresWithDieseaseChanged;
    }

    private void UpdateProgresChanged(ObjectiveHospitalPatientWithInfoEventArgs eventArgs)
    {
        if (!completed)
        {
            switch (hospitalPatientRandomType)
            {
                case HospitalPatientType.VIP:
                    if (eventArgs.isVip)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                case HospitalPatientType.RandomBloodType:
                    if (eventArgs.bloodType == bloodType)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                case HospitalPatientType.AnyPatient:
                    base.UpdateObjective(eventArgs.amount);
                    break;
                case HospitalPatientType.RandomGender:
                    if (eventArgs.sex == sex)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateProgresWithDieseaseChanged(ObjectiveHospitalPatientWithDiseaseEventArgs eventArgs)
    {
        if (!completed)
        {
            switch (hospitalPatientRandomType)
            {
                case HospitalPatientType.RandomDisease:
                    if (eventArgs.diseaseType == diseaseType)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                case HospitalPatientType.Bacteria:
                    if (eventArgs.diseaseType == DiseaseType.Bacteria)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                case HospitalPatientType.Plant:
                    if (eventArgs.needPlantToCure)
                        base.UpdateObjective(eventArgs.amount);
                    break;
                default:
                    break;
            }
        }
    }


    public enum HospitalPatientType
    {
        AnyPatient,
        RandomDisease,
        RandomGender,
        Bacteria,
        Plant,
        VIP,
        RandomBloodType,
    }
}

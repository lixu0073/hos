using UnityEngine;
using System.Collections;
using System.Text;
using System;
using Hospital;

public abstract class DailyTask {

    public int taskProgressCounter;
    public DailyTaskType taskType;

    protected int progressGoal;
    protected bool completed;

    internal DailyTask()
    {
        taskType = DailyTaskType.Default;
        progressGoal = 1;

        taskProgressCounter = 0;
        completed = false;
    }

    public void SetTaskObjectives(int amount)
    {
        progressGoal = amount;
    }

    public int TaskProgressGoal
    {
        get { return progressGoal;}
        private set { }
    }

    public bool IsCompleted()
    {
        return completed;
    }

    public virtual void Init()
    {
        AddListener();
    }

    public void SetDailyTaskCompleted()
    {
        taskProgressCounter = progressGoal;
        completed = true;

        UIController.getHospital.DailyQuestMainButtonUI.SpawnStar();
        AnalyticsController.instance.ReportDailyTaskCompleted(this);

        int completedTaskId;
        completedTaskId = ReferenceHolder.GetHospital().dailyQuestController.GetCurrentDailyQuest().GetCompletedTasksCount();
        SoundsController.Instance.PlayAnySound(71 + (completedTaskId - 1));
    }

    public virtual string GetDescription()
    {
        switch(taskType)
        {
            case DailyTaskType.CureVips:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_VIP"), progressGoal);
            case DailyTaskType.CureChildern:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_CHILDREN"), progressGoal);
            case DailyTaskType.DiagnosePatients:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DIAGNOZE_PATIENTS"), progressGoal);
            case DailyTaskType.UseShovels:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_USE_SHOVELS"), progressGoal);
            case DailyTaskType.HelpInMedicinalGardens:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_HELP_MEDICINAL_PLANTS"), progressGoal);
            case DailyTaskType.HelpWithAntiEpidemicCentres:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_HELP_EPIDEMIC_CENTER"), progressGoal);
            case DailyTaskType.CompleteAntiEpidemicBoxes:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_FILL_EPIDEMIC_PACKAGES"), progressGoal);
            case DailyTaskType.VisitOtherHospitals:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_VISIT_HOSPITALS"), progressGoal);
            case DailyTaskType.TapTheDear:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_CUTE_DEER"), progressGoal);
            case DailyTaskType.PatientTalk:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_PATIENTS_CLOUDS"), progressGoal);
            case DailyTaskType.SellInThePharmacy:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_SELL_IN_PHARMACY"), progressGoal);
            case DailyTaskType.RotateDecorations:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_ROTATE_DECORATIONS"), progressGoal);
            case DailyTaskType.LevelUp:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_READ_DESCRIPTIONS_FROM_LAB"), progressGoal);
            case DailyTaskType.ReadAboutYourDoctorsAndNurses:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_READ_DESCRIPTIONS_FROM_HOSPITAL"), progressGoal);
            case DailyTaskType.FindTreasureChestsWhenVisiting:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_OPEN_TREASURE_IN_SOMEONE_HOSPITAL"), progressGoal);
            case DailyTaskType.FindTreasureChests:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_OPEN_TREASURE_IN_YOUR_HOSPITAL"), progressGoal);
            case DailyTaskType.LikeOtherHospitals:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_GIVE_FOLLOWER"), progressGoal);
            case DailyTaskType.PlayTheBubbleBoyGame:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DO_BUBBLE_BOY"), progressGoal);
            case DailyTaskType.DiscardPatients:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DISMISS_PATIENT_FROM_TREATMENT_ROOM"), progressGoal);
            case DailyTaskType.TreatmentRoomPatients:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_BED_PATIENT"), progressGoal);
            case DailyTaskType.BuyInPharmacy:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_BUY_IN_PHARMACY"), progressGoal);
            case DailyTaskType.CourierPackage:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_COURIER_PACKAGES"), progressGoal);
            case DailyTaskType.TapOnAddBillboard:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_ADD_BILLBOARD"), progressGoal);
            case DailyTaskType.TapOnCampFire:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_CAMPFIRE"), progressGoal);
            case DailyTaskType.TapOnAPatient:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_PATIENT"), progressGoal);
            case DailyTaskType.CollectGifts:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_COLLECT_GIFTS"), progressGoal);
            case DailyTaskType.UnlockDailyQuests:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_UNLOCK_DQ");
            case DailyTaskType.BlueBirdHunting:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_BLUE_BIRD"), progressGoal);
            case DailyTaskType.PatientLikesAndDislikes:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PATIENT_LIKES_DISLIKES"), progressGoal);
            case DailyTaskType.WhoAreTheVips:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_VIP_BIOS"), progressGoal);
            case DailyTaskType.PatientCardSwoosh:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PATIENT_CARD_SWIPE"), progressGoal);
            case DailyTaskType.WhatsNext:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_NEXT_LEVEL_UNLOCKS"), progressGoal);
            case DailyTaskType.UseABooster:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_USE_BOOSTER"), progressGoal);
            case DailyTaskType.DoctorWiseTreasure:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_WISE_TREASURE_CHEST"), progressGoal);
            case DailyTaskType.BuyGoods:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_SEE_BUY_GOODS"), progressGoal);
            case DailyTaskType.AskForHelp:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_ASK_FOR_HELP"), progressGoal);
            case DailyTaskType.PreventBacteriaSpread:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PREVENT_BACTERIA_SPREAD"), progressGoal);
            default:
                return taskType.ToString() + " DO " + progressGoal;
        }
    }

    public virtual string GetInfo()
    {
        switch (taskType)
        {
            case DailyTaskType.CureVips:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_VIP_INFO");
            case DailyTaskType.CureChildern:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_CHILDREN_INFO");
            case DailyTaskType.DiagnosePatients:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DIAGNOZE_PATIENTS_INFO");
            case DailyTaskType.UseShovels:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_USE_SHOVELS_INFO");
            case DailyTaskType.HelpInMedicinalGardens:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_HELP_MEDICINAL_PLANTS_INFO");
            case DailyTaskType.HelpWithAntiEpidemicCentres:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_HELP_EPIDEMIC_CENTER_INFO");
            case DailyTaskType.CompleteAntiEpidemicBoxes:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_FILL_EPIDEMIC_PACKAGES_INFO");
            case DailyTaskType.VisitOtherHospitals:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_VISIT_HOSPITALS_INFO");
            case DailyTaskType.TapTheDear:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_CUTE_DEER_INFO");
            case DailyTaskType.PatientTalk:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_PATIENTS_CLOUDS_INFO");
            case DailyTaskType.SellInThePharmacy:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_SELL_IN_PHARMACY_INFO");
            case DailyTaskType.RotateDecorations:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_ROTATE_DECORATIONS_INFO");
            case DailyTaskType.LevelUp:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_READ_DESCRIPTIONS_FROM_LAB_INFO");
            case DailyTaskType.ReadAboutYourDoctorsAndNurses:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_READ_DESCRIPTIONS_FROM_HOSPITAL_INFO");
            case DailyTaskType.FindTreasureChestsWhenVisiting:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_OPEN_TREASURE_IN_SOMEONE_HOSPITAL_INFO");
            case DailyTaskType.FindTreasureChests:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_OPEN_TREASURE_IN_YOUR_HOSPITAL_INFO");
            case DailyTaskType.LikeOtherHospitals:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_GIVE_FOLLOWER_INFO");
            case DailyTaskType.PlayTheBubbleBoyGame:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DO_BUBBLE_BOY_INFO");
            case DailyTaskType.DiscardPatients:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_DISMISS_PATIENT_FROM_TREATMENT_ROOM_INFO");
            case DailyTaskType.TreatmentRoomPatients:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_CURE_BED_PATIENT_INFO");
            case DailyTaskType.BuyInPharmacy:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_BUY_IN_PHARMACY_INFO");
            case DailyTaskType.CourierPackage:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_COURIER_PACKAGES_INFO");
            case DailyTaskType.TapOnAddBillboard:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_ADD_BILLBOARD_INFO");
            case DailyTaskType.TapOnCampFire:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_CAMPFIRE_INFO");
            case DailyTaskType.TapOnAPatient:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_PATIENT_INFO");
            case DailyTaskType.CollectGifts:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_COLLECT_GIFTS_INFO");
            case DailyTaskType.UnlockDailyQuests:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_UNLOCK_DQ_INFO");
            case DailyTaskType.BlueBirdHunting:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_TAP_ON_BLUE_BIRD_INFO");
            case DailyTaskType.PatientLikesAndDislikes:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PATIENT_LIKES_DISLIKES_INFO");
            case DailyTaskType.WhoAreTheVips:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_VIP_BIOS_INFO");
            case DailyTaskType.PatientCardSwoosh:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PATIENT_CARD_SWIPE_INFO");
            case DailyTaskType.WhatsNext:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_NEXT_LEVEL_UNLOCKS_INFO");
            case DailyTaskType.UseABooster:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_USE_BOOSTER_INFO");
            case DailyTaskType.DoctorWiseTreasure:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_WISE_TREASURE_CHEST_INFO");
            case DailyTaskType.BuyGoods:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_SEE_BUY_GOODS_INFO");
            case DailyTaskType.AskForHelp:
                return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_ASK_FOR_HELP_INFO");
            case DailyTaskType.PreventBacteriaSpread:
                return string.Format(I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/TASK_PREVENT_BACTERIA_SPREAD_INFO"), progressGoal);
            default:
                return taskType.ToString() + " INFO ";
        }
    }

    public string GetProgressString()
    {
        if (taskProgressCounter >= progressGoal)
            return I2.Loc.ScriptLocalization.Get("DAILY_QUESTS/COMPLETED");

        return taskProgressCounter + " / " + progressGoal;
    }

    public float GetProgressFloat()
    {
        return (float)taskProgressCounter / (float)progressGoal;
    }

    public virtual string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(taskType.ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(taskProgressCounter, 0, int.MaxValue, "Task taskProgressCounter: ").ToString());
        builder.Append("!");
        builder.Append(Checkers.CheckedAmount(progressGoal, 0, int.MaxValue, "Task progressGoal: ").ToString());
        return builder.ToString();
    }

    public virtual void LoadFromString(string saveString)
    {
            var taskDataSave = saveString.Split('!');

            if (ResourcesHolder.GetHospital().dailyTaskDatabase == null)
                Debug.LogError("dailyTaskDatabase jest odpięty ~ call Lukasz !");

            taskType = (DailyTaskType)Enum.Parse(typeof(DailyTaskType), taskDataSave[0]);
            taskProgressCounter = int.Parse(taskDataSave[1], System.Globalization.CultureInfo.InvariantCulture);
            SetTaskObjectives(int.Parse(taskDataSave[2], System.Globalization.CultureInfo.InvariantCulture));

            if (taskProgressCounter >= progressGoal)
                completed = true;
            else completed = false;
    }

    protected virtual void AddListener()
    {
        RemoveListener();
        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Notification += AddProgress;
    }

    protected virtual void RemoveListener()
    {
        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Notification -= AddProgress;
    }

    public virtual void AddProgress(DailyQuestProgressEventArgs args)
    {
        if (completed || taskType != args.taskType)
            return;

        taskProgressCounter += args.amount;

        if (taskProgressCounter >= progressGoal)
        {
            taskProgressCounter = progressGoal;
            SetDailyTaskCompleted();

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.DailyQuest);
        }
    }

    public void OnDestroy()
    {
        RemoveListener();
    }

    public abstract void OnSetDailyTaskCompleted();

    public enum DailyTaskType
    {
        Default = 0,
        CureVips = 1,
        CureChildern = 2,
        DiagnosePatients = 3,
        UseShovels = 4,
        HelpInMedicinalGardens = 5,
        HelpWithAntiEpidemicCentres = 6,
        CompleteAntiEpidemicBoxes = 7,
        VisitOtherHospitals = 8,
        TapTheDear = 9,
        PatientTalk = 10,
        SellInThePharmacy = 11,
        RotateDecorations = 12,
        ReadAboutYourDoctorsAndNurses = 14,
        FindTreasureChestsWhenVisiting = 15,
        FindTreasureChests = 16,
        LikeOtherHospitals = 18,
        PlayTheBubbleBoyGame = 19,
        DiscardPatients = 20,
        CurePatientsForGivenDoctor = 21,
        TreatmentRoomPatients = 22,
        ConnectToFacebook = 23,
        BuyInPharmacy = 24,
        CourierPackage = 25,
        TapOnAddBillboard = 26,
        TapOnCampFire = 27,
        TapOnAPatient = 28,
        CollectGifts = 29,
        UnlockDailyQuests = 30,
        BlueBirdHunting = 31,
        PatientLikesAndDislikes = 32,
        WhoAreTheVips = 33,
        PatientCardSwoosh = 34,
        WhatsNext = 35,
        UseABooster = 36,
        DoctorWiseTreasure = 37,
        BuyGoods = 39,
        AskForHelp = 40,
        LevelUp = 41,
        PreventBacteriaSpread = 42,

    }

    public enum DailyTaskDifficulty
    {
        Default,
        Easy,
        Medium,
        Hard,
    }
}

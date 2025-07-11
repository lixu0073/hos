using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DailyTaskContext
{

    private Dictionary<DailyTask.DailyTaskType, DailyTaskStrategy> strategies;

    public DailyTaskContext()
    {
        strategies = new Dictionary<DailyTask.DailyTaskType, DailyTaskStrategy>()
        {
            {DailyTask.DailyTaskType.BuyInPharmacy, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.ConnectToFacebook, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.TreatmentRoomPatients, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CureChildern, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CureVips, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.DiagnosePatients, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.DiscardPatients, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.PlayTheBubbleBoyGame, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CompleteAntiEpidemicBoxes, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.LikeOtherHospitals, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.HelpWithAntiEpidemicCentres, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.HelpInMedicinalGardens, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.FindTreasureChestsWhenVisiting, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.FindTreasureChests, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.ReadAboutYourDoctorsAndNurses, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.LevelUp, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.RotateDecorations, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.SellInThePharmacy, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.PatientTalk, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.TapTheDear, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.UseShovels, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.VisitOtherHospitals, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CurePatientsForGivenDoctor, new CurePatientsDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CourierPackage, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.TapOnAddBillboard, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.TapOnCampFire, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.TapOnAPatient, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.CollectGifts, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.UnlockDailyQuests, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.BlueBirdHunting, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.PatientLikesAndDislikes, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.WhoAreTheVips, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.PatientCardSwoosh, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.WhatsNext, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.UseABooster, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.DoctorWiseTreasure, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.BuyGoods, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.AskForHelp, new DefaultDailyTaskStrategy() },
            {DailyTask.DailyTaskType.PreventBacteriaSpread, new DefaultDailyTaskStrategy() },
                };
    }

    public DailyTask GetConcreteDailyTask(DailyTask.DailyTaskType taskType, int dailyTaskOccurenceDay)
    {
        DailyTaskStrategy dailyTaskStrategy;

        if (strategies.TryGetValue(taskType, out dailyTaskStrategy))
        {
            return dailyTaskStrategy.GetDailyTask(taskType, dailyTaskOccurenceDay);
        }
        return null;
    }
}

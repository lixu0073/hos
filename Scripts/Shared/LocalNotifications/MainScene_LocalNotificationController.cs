using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;
using System.Linq;

public class MainScene_LocalNotificationController : BaseLocalNotificationController, ILocalNotificationController
{

    public int LVL_6_GENERIC_PUSH_OFFSET_DURATION_IN_MINUETS = 5;
    public int LVL_7_GENERIC_PUSH_OFFSET_DURATION_IN_MINUETS = 10;
    public int PATIENT_READY_TO_CURE_OFFSET_DURATION_IN_MINUTES = 6;
    public int VIP_READY_TO_CURE_OFFSET_DURATION_IN_MINUTES = 2;
    public int REMAIND_NOTIFICATION_OFFSET_IN_HOURS = 12;

    public void SetUpSpecialNotifications()
    {
        SetUpMeachineEndBuildingOnSpecificLevels(7, 10);
    }

    public void SetUp()
    {
        SetUpMastershipNotification();
        SetUpGlobalEventMaxChestsNotification();
        SetUpGlobalEventStartNotifiaction();
        SetUpGlobalEventMidNotification();
        SetUpLevelGoalNotification();
        SetUpMedicineEndProduction();
        SetUpPatientCuredInDoctorRoom();
        SetUpReadyHelpInTreatmentRoomNotification();
        SetUpRemainderNotification();
        SetUpVIPReadyToHealNotification();
        SetUpBuildEndNotifications();
        SetUpVIPEndDiagnoseNotification();
        SetUpPatientReadyToHealNotification();
        SetUpPatientCuresReadyNotification();
        SetUpVIPBuildEndedNotification();
        SetUpPlayroomBuildEndedNotification();
        SetUpVitaminesCollectorFullfillNotification();
        SetUpGoodieBoxNotification();
        SetUpEpidemyBuildEndedNotification();
        SetUpDailyQuestDayEndingNotification();
        SetUpDailyQuestNewDayNotification();
        SetUpBubbleBoyNotification();
        SetUpEndSoonEpidemyNotification();
        SetUpAllProductionsEndedNotifications();
        SetUpAllPlantsReadyForHarvestingNotifications();
        SetUpPatientsCuredNotification();
        SetUpVIPArrivingNotification();
        SetUpLevelNotification();
        SetupDailyRewardNotification();
    }

    private void SetupDailyRewardNotification()
    {
        Debug.LogError("Setup local notif for daily reward");
        int secondsToGo = -1;
        if (Game.Instance.gameState().GetHospitalLevel() >= ContentUnlockLevel.DailyRewardUnlockLevel)
        {
            BasicLocalNotification.Type notificationType = BasicLocalNotification.Type.None;

            if (!ReferenceHolder.GetHospital().DailyRewardController.IsCurrentDayRewardClaimed())
            {
                if (DateTime.Now.ToLocalTime().Hour < WorldWideConstants.TWELVE_PM_IN_HOURS)
                {
                    notificationType = BasicLocalNotification.Type.DailyRewardToClaimMorining;
                    secondsToGo = (int)DateTime.Today.AddHours(WorldWideConstants.TWELVE_PM_IN_HOURS).Subtract(DateTime.Now.ToLocalTime()).TotalSeconds;
                }
                else if (DateTime.Now.ToLocalTime().Hour < WorldWideConstants.SEVEN_PM_IN_HOURS)
                {
                    notificationType = BasicLocalNotification.Type.DailyRewardToClaimEvening;
                    secondsToGo = (int)DateTime.Today.AddHours(WorldWideConstants.SEVEN_PM_IN_HOURS).Subtract(DateTime.Now.ToLocalTime()).TotalSeconds;
                }
                else
                {
                    notificationType = BasicLocalNotification.Type.DailyRewardToClaimMorining;
                    secondsToGo = (int)DateTime.Today.AddDays(WorldWideConstants.ONE_DAY_IN_DAYS).AddHours(WorldWideConstants.TWELVE_PM_IN_HOURS).Subtract(DateTime.Now.ToLocalTime()).TotalSeconds;
                }
            }
            else
            {
                notificationType = BasicLocalNotification.Type.DailyRewardToClaimMorining;
                secondsToGo = (int)DateTime.Today.AddDays(WorldWideConstants.ONE_DAY_IN_DAYS).AddHours(WorldWideConstants.TWELVE_PM_IN_HOURS).Subtract(DateTime.Now.ToLocalTime()).TotalSeconds;
            }
            if (secondsToGo > 0)
            {
                SetNotification(new BasicLocalNotification(GetCurrentTime().AddSeconds(secondsToGo), notificationType));
            }
        }
    }

    private void SetUpVitaminesCollectorFullfillNotification()
    {
        VitaminMaker maker = LocalNotificationController.FindObjectOfType<VitaminMaker>();
        if (maker != null)
        {
            int secondsToFullfill = maker.GetSecondsToFullfillCollector();
            if (secondsToFullfill > -1)
            {
                SetNotification(new BasicLocalNotification(now.AddSeconds(secondsToFullfill), BasicLocalNotification.Type.FullVitaminesCollector));
            }
        }
    }

    private void SetUpReadyHelpInTreatmentRoomNotification()
    {
        if (ReferenceHolder.GetHospital().treatmentRoomHelpController.AvailableRequests == 0 && ReferenceHolder.GetHospital().treatmentRoomHelpController.NextRequestTime > 0)
        {
            SetNotification(new BasicLocalNotification(ServerTime.UnixTimestampToDateTime(ReferenceHolder.GetHospital().treatmentRoomHelpController.NextRequestTime).ToLocalTime(), BasicLocalNotification.Type.PatientHelpRequest));
        }
    }

    private void SetUpGlobalEventMaxChestsNotification()
    {
        if (!HospitalAreasMapController.HospitalMap.globalEventsChestsManager.IsEventChestsEnabled())
            return;
        long when = HospitalAreasMapController.HospitalMap.globalEventsChestsManager.GetTimeStampAfterAllChestsWillBeAvailable();
        if (when == -1)
            return;
        SetNotification(new BasicLocalNotification(ServerTime.UnixTimestampToDateTime(when).ToLocalTime(), BasicLocalNotification.Type.GlobalEventMaxChests, HospitalAreasMapController.HospitalMap.globalEventsChestsManager.GetMaxChestsCount().ToString(), "dyń")); // ten tekst "dyń" nie ma znaczenia :)  
    }

    private void SetUpMeachineEndBuildingOnSpecificLevels(int startLevel, int endLevel)
    {
        if (!HasRequiredLevel(startLevel, endLevel))
            return;
        foreach (MedicineProductionMachine productionMedicineMachine in LocalNotificationController.FindObjectsOfType<MedicineProductionMachine>().ToList())
        {
            if (productionMedicineMachine.state == RotatableObject.State.building)
            {
                DateTime buildEndTime = now.AddSeconds(productionMedicineMachine.BuildStartTime);
                string name = I2.Loc.ScriptLocalization.Get(productionMedicineMachine.GetRoomInfo().ShopTitle);
                SetNotification(new BuildEndLocalNotification(buildEndTime, name, productionMedicineMachine), false, false);
            }
        }
    }

    private bool HasRequiredLevel(int startLevel, int endLevel)
    {
        return startLevel <= Game.Instance.gameState().GetHospitalLevel() && Game.Instance.gameState().GetHospitalLevel() <= endLevel;
    }

    private void SetUpGlobalEventStartNotifiaction()
    {
        int duration = ReferenceHolder.GetHospital().globalEventController.GetTimeToNextGlobalEvent();
        if (duration != -1 && duration > 0)
        {
            SetNotification(new BasicLocalNotification(now.AddSeconds(duration), BasicLocalNotification.Type.StartGlobalEvent));
        }
    }

    public void SetUpGlobalEventMidNotification()
    {
        int duration = ReferenceHolder.GetHospital().globalEventController.GetTimeAfterTwoDaysFromCurrentEventStart();
        if (duration != -1 && duration > 0)
        {
            SetNotification(new BasicLocalNotification(now.AddSeconds(duration), BasicLocalNotification.Type.MidGlobalEvent));
        }
    }

    private void SetUpLevelGoalNotification()
    {
        if (VisitingController.Instance.IsVisiting)
            return;
        List<Objective> objectives = ReferenceHolder.Get().objectiveController.GetAllObjectives();
        if (objectives == null)
            return;
        CurePatientDoctorRoomObjective curePatientDoctorRoomObjective = null;
        foreach (Objective objective in objectives)
        {
            if (objective is CurePatientDoctorRoomObjective)
            {
                curePatientDoctorRoomObjective = (CurePatientDoctorRoomObjective)objective;
                break;
            }
        }
        if (curePatientDoctorRoomObjective == null)
            return;
        if (curePatientDoctorRoomObjective.IsCompleted)
            return;
        int progressLeft = curePatientDoctorRoomObjective.ProgressObjective - curePatientDoctorRoomObjective.Progress;
        if (progressLeft < 1)
            return;
        List<DoctorRoom> doctorRooms = new List<DoctorRoom>();
        if (curePatientDoctorRoomObjective.rotatableTag == "AnyDoc")
        {
            doctorRooms = LocalNotificationController.FindObjectsOfType<DoctorRoom>().ToList();
        }
        else
        {
            RotatableObject rotObj = HospitalAreasMapController.HospitalMap.FindRotatableObject(curePatientDoctorRoomObjective.rotatableTag);
            if (rotObj == null)
                return;
            DoctorRoom doctorRoom = null;
            if (rotObj is DoctorRoom)
            {
                doctorRoom = (DoctorRoom)rotObj;
            }
            if (doctorRoom == null)
                return;
            doctorRooms.Add(doctorRoom);
        }
        if (doctorRooms.Count == 0)
            return;
        int timeToCompleteLevelGoal = GetSecondsToCompleteLevelGoal(doctorRooms, progressLeft);
        if (timeToCompleteLevelGoal < 1)
            return;
        SetNotification(new BasicLocalNotification(now.AddSeconds(timeToCompleteLevelGoal), BasicLocalNotification.Type.LevelGoal));
    }

    private int GetSecondsToCompleteLevelGoal(List<DoctorRoom> doctorRooms, int progressLeft)
    {
        return (int)AlgorithmHolder.GetShortestTimeToPushDueToDocPatientCure(doctorRooms, progressLeft);
    }

    private void SetUpGoodieBoxNotification()
    {
        int timeToGo = ((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryIntervalSeconds - Convert.ToInt32((long)ServerTime.getTime()) - ((HospitalCasesManager)AreaMapController.Map.casesManager).countingStartTime;
        if (timeToGo > 0)
        {
            SetNotification(new BasicLocalNotification(now.AddSeconds(timeToGo), BasicLocalNotification.Type.GoodieBox));
        }
    }

    private void SetUpVIPEndDiagnoseNotification()
    {
        if (HospitalAreasMapController.HospitalMap.vipRoom.currentVip != null)
        {
            VIPPersonController vipPersonController = HospitalAreasMapController.HospitalMap.vipRoom.currentVip.gameObject.GetComponent<VIPPersonController>();
            if (vipPersonController.IsInDiagnose())
            {
                int timeToEndDiagnose = vipPersonController.GetTimeToEndDiagnose();
                if (timeToEndDiagnose != -1)
                {
                    SetNotification(new BasicLocalNotification(now.AddSeconds(timeToEndDiagnose), BasicLocalNotification.Type.VIPEndDiagnose));
                }
            }
        }
    }

    private void SetUpDailyQuestDayEndingNotification()
    {
        if (DailyQuestSynchronizer.Instance.IsDailyQuestFuncionalityStarted() && !ReferenceHolder.GetHospital().dailyQuestController.isWeekPassed())
        {
            int TimeTillNextDay = ReferenceHolder.GetHospital().dailyQuestController.TimeTillNextDay();
            if (TimeTillNextDay > 3600)
            {
                SetNotification(new BasicLocalNotification(now.AddSeconds(TimeTillNextDay), BasicLocalNotification.Type.DailyQuestDayEnding));
            }
        }
    }

    private void SetUpDailyQuestNewDayNotification()
    {
        if (DailyQuestSynchronizer.Instance.IsDailyQuestFuncionalityStarted() && !ReferenceHolder.GetHospital().dailyQuestController.isWeekPassed())
        {
            SetNotification(new BasicLocalNotification(now.AddSeconds(ReferenceHolder.GetHospital().dailyQuestController.TimeTillNextDay()), BasicLocalNotification.Type.DailyQuestNewDay));
        }
    }

    private void SetUpBubbleBoyNotification()
    {
        BubbleBoyDataSynchronizer bubbleBoyDataSynchronizer = BubbleBoyDataSynchronizer.Instance;
        if (bubbleBoyDataSynchronizer != null && bubbleBoyDataSynchronizer.IsBubbleBoyEnabled())
        {
            if (!bubbleBoyDataSynchronizer.IsFreeEntryAvailable())
            {
                int NextFreeEntryDate = BubbleBoyDataSynchronizer.Instance.NextFreeEntryDate;
                SetNotification(new BasicLocalNotification(now.AddSeconds(Convert.ToInt32(NextFreeEntryDate) - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds)), BasicLocalNotification.Type.BubbleBoyFreeEntry));
            }
        }
    }

    private void SetUpEndSoonEpidemyNotification()
    {
        EpidemyObjectController epidemyObjectController = HospitalAreasMapController.HospitalMap.epidemy.GetComponent<EpidemyObjectController>();
        if (epidemyObjectController.IsEnabled())
        {
            int timeToSoonEpidemyEnd = epidemyObjectController.GetDurationToEpidemyOutbreakEndWithOffsetInMinutes(180);
            SetNotification(new BasicLocalNotification(now.AddSeconds(timeToSoonEpidemyEnd), BasicLocalNotification.Type.EpidemyEndSoon));
        }
    }

    private void SetUpRemainderNotification()
    {
        SetNotification(new BasicLocalNotification(now.AddHours(REMAIND_NOTIFICATION_OFFSET_IN_HOURS), BasicLocalNotification.Type.Remaind), false);
    }

    private void SetUpVIPReadyToHealNotification()
    {
        foreach (HospitalBedController.HospitalBed bed in HospitalAreasMapController.HospitalMap.hospitalBedController.Beds)
        {
            if (bed.Patient != null && bed.room == null && bed.Indicator.ReadyToCure())
            {
                VIPSystemManager vipSystemManager = HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>();
                if (vipSystemManager.GetSecondsToLeave() > VIP_READY_TO_CURE_OFFSET_DURATION_IN_MINUTES)
                {
                    SetNotification(new BasicLocalNotification(now.AddMinutes(VIP_READY_TO_CURE_OFFSET_DURATION_IN_MINUTES), BasicLocalNotification.Type.VIPReadyToCure));
                }
            }
        }
    }

    private void SetUpPatientReadyToHealNotification()
    {
        foreach (HospitalBedController.HospitalBed bed in HospitalAreasMapController.HospitalMap.hospitalBedController.Beds)
        {
            if (bed.Patient != null && bed.room != null && bed.Indicator.ReadyToCure())
            {
                SetNotification(new BasicLocalNotification(now.AddMinutes(PATIENT_READY_TO_CURE_OFFSET_DURATION_IN_MINUTES), BasicLocalNotification.Type.PatientReadyToCure));
            }
        }
    }

    private void SetUpPatientCuresReadyNotification()
    {
        DateTime global = now;
        bool isGlobalSet = false;
        DateTime productionsToHealEndTime = now;
        foreach (HospitalBedController.HospitalBed bed in HospitalAreasMapController.HospitalMap.hospitalBedController.Beds)
        {
            if (bed.Patient != null && bed.room != null && !bed.Indicator.ReadyToCure() && !bed.Patient.GetHospitalCharacterInfo().RequiresDiagnosis)
            {
                List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = bed.Patient.GetHospitalCharacterInfo().GetMissingMedicines();
                if (missingMedicines.Count > 0)
                {
                    productionsToHealEndTime = now;
                    bool sendPush = true;
                    for (int i = 0; i < missingMedicines.Count; ++i)
                    {
                        int amount = missingMedicines[i].Key;
                        MedicineDatabaseEntry med = missingMedicines[i].Value;
                        RotatableObject rotatableObject = HospitalAreasMapController.HospitalMap.FindRotatableObject(med.producedIn.Tag);
                        if (rotatableObject == null)
                        {
                            sendPush = false;
                            break;
                        }
                        else
                        {
                            if (rotatableObject is ProbeTable)
                            {
                                ProbeTable probeTable = (ProbeTable)rotatableObject;
                                if (amount == 1 && probeTable.state == RotatableObject.State.working && probeTable.GetTableState() == TableState.producing)
                                {
                                    DateTime productionEndTime = now.AddSeconds(probeTable.ProductionTimeLeft);
                                    if (productionsToHealEndTime < productionEndTime)
                                    {
                                        productionsToHealEndTime = productionEndTime;
                                    }
                                }
                                else
                                {
                                    sendPush = false;
                                    break;
                                }
                            }
                            else if (rotatableObject is MedicineProductionMachine)
                            {
                                MedicineProductionMachine productionMachine = (MedicineProductionMachine)rotatableObject;
                                if (productionMachine.IsProducing() && productionMachine.CountProducingMeds() >= amount)
                                {
                                    DateTime productionEndTime = now.AddSeconds(productionMachine.GetTimeToEndProductionMeds(amount));
                                    if (productionsToHealEndTime < productionEndTime)
                                    {
                                        productionsToHealEndTime = productionEndTime;
                                    }
                                }
                                else
                                {
                                    sendPush = false;
                                    break;
                                }
                            }
                            else
                            {
                                sendPush = false;
                                break;
                            }
                        }
                    }
                    if (sendPush)
                    {
                        if (global > productionsToHealEndTime || !isGlobalSet)
                        {
                            isGlobalSet = true;
                            global = productionsToHealEndTime;
                        }
                    }
                }
            }
        }
        if (isGlobalSet)
        {
            SetNotification(new BasicLocalNotification(global, BasicLocalNotification.Type.PatientCuresReady));
        }
    }

    private void SetUpBuildEndNotifications()
    {
        bool shouldAdd = false;
        DateTime fastestBuildingEndTime = now;
        RotatableObject tempRotatableObject = null;
        string name = "";
        foreach (RotatableObject rotatableObject in LocalNotificationController.FindObjectsOfType<RotatableObject>().ToList())
        {
            if (rotatableObject.state == RotatableObject.State.building)
            {
                if (rotatableObject is MedicineProductionMachine && HasRequiredLevel(7, 10))
                    continue;
                DateTime buildEndTime = now.AddSeconds(rotatableObject.BuildStartTime);
                if (!shouldAdd || fastestBuildingEndTime > buildEndTime)
                {
                    shouldAdd = true;
                    fastestBuildingEndTime = buildEndTime;
                    name = I2.Loc.ScriptLocalization.Get(rotatableObject.GetRoomInfo().ShopTitle);
                    tempRotatableObject = rotatableObject;
                }
            }
        }
        if (shouldAdd)
        {
            SetNotification(new BuildEndLocalNotification(fastestBuildingEndTime, name, tempRotatableObject));
        }
    }

    private void SetUpMedicineEndProduction()
    {
        bool shouldAdd = false;
        DateTime medicineProductionEndTime = now;
        MedicineRef med = null;
        foreach (ProbeTable probeTable in LocalNotificationController.FindObjectsOfType<ProbeTable>().ToList())
        {
            if (probeTable.state == RotatableObject.State.working && probeTable.GetTableState() == TableState.producing)
            {
                DateTime productionEndTime = now.AddSeconds(probeTable.ProductionTimeLeft);
                if (!shouldAdd || (shouldAdd && medicineProductionEndTime > productionEndTime))
                {
                    medicineProductionEndTime = productionEndTime;
                    med = probeTable.producedElixir;
                }
                shouldAdd = true;
            }
        }
        foreach (MedicineProductionMachine productionMachine in LocalNotificationController.FindObjectsOfType<MedicineProductionMachine>().ToList())
        {
            if (productionMachine.IsProducing())
            {
                DateTime productionEndTime = now.AddSeconds(productionMachine.ActualMedicineProductionTime);
                if (!shouldAdd || (shouldAdd && medicineProductionEndTime > productionEndTime))
                {
                    medicineProductionEndTime = productionEndTime;
                    med = productionMachine.GetActualMedicine();
                }
                shouldAdd = true;
            }
        }
        if (shouldAdd && med != null)
        {
            SetNotification(new BasicLocalNotification(medicineProductionEndTime, BasicLocalNotification.Type.SingleMedicineEnd, med));
        }
    }

    private void SetUpAllProductionsEndedNotifications()
    {
        bool shouldAdd = false;
        DateTime allProductionsEndTime = now;
        foreach (ProbeTable probeTable in LocalNotificationController.FindObjectsOfType<ProbeTable>().ToList())
        {
            if (probeTable.state == RotatableObject.State.working && probeTable.GetTableState() == TableState.producing)
            {
                shouldAdd = true;
                DateTime productionEndTime = now.AddSeconds(probeTable.ProductionTimeLeft);
                if (allProductionsEndTime < productionEndTime)
                {
                    allProductionsEndTime = productionEndTime;
                }
            }
        }
        foreach (MedicineProductionMachine productionMachine in LocalNotificationController.FindObjectsOfType<MedicineProductionMachine>().ToList())
        {
            if (productionMachine.IsProducing())
            {
                shouldAdd = true;
                DateTime productionEndTime = now.AddSeconds(productionMachine.GetTimeToEndProduction());
                if (allProductionsEndTime < productionEndTime)
                {
                    allProductionsEndTime = productionEndTime;
                }
            }
        }
        if (shouldAdd)
        {
            SetNotification(new AllProductionsEndLocalNotification(allProductionsEndTime));
        }
    }


    private void SetUpVIPBuildEndedNotification()
    {
        SetUpExternalRoomBuildEndedNotification(ReferenceHolder.GetHospital().vipSpawner.GetComponent<VipRoom>());
    }

    private void SetUpPlayroomBuildEndedNotification()
    {
        SetUpExternalRoomBuildEndedNotification(HospitalAreasMapController.HospitalMap.playgroud);
    }

    private void SetUpEpidemyBuildEndedNotification()
    {
        EpidemyObjectController epidemy = HospitalAreasMapController.HospitalMap.epidemy.GetComponent<EpidemyObjectController>();
        if (epidemy.IsRenovating())
        {
            SetNotification(new BuildEndLocalNotification(now.AddSeconds(epidemy.GetTimeToEndRenovation()), I2.Loc.ScriptLocalization.Get(epidemy.EpidemyObjectInfo.roomName)));
        }
    }

    private void SetUpExternalRoomBuildEndedNotification(ExternalRoom room)
    {
        if (room.ExternalHouseState == ExternalRoom.EExternalHouseState.renewing)
        {
            SetNotification(new BuildEndLocalNotification(now.AddSeconds(room.GetTimeToEndRenovation()), I2.Loc.ScriptLocalization.Get(room.roomInfo.roomName)));
        }
    }

    private void SetUpAllPlantsReadyForHarvestingNotifications()
    {
        bool shouldAdd = false;
        DateTime allReadyForHarvestTime = now;
        Plantation plantation = ReferenceHolder.GetHospital().plantation;
        for (int j = 0; j < plantation.actualPlantationSize.y; j++)
        {
            for (int i = 0; i < plantation.plantationMaxSize.x; i++)
            {
                if (plantation.patches[i, j].GrowingPlant != null)
                {
                    if (plantation.patches[i, j].PatchState == EPatchState.producing)
                    {
                        DateTime readyForHarvestTime = now.AddSeconds(plantation.patches[i, j].GetTimeToEndProducing());
                        if (allReadyForHarvestTime < readyForHarvestTime)
                        {
                            shouldAdd = true;
                            allReadyForHarvestTime = readyForHarvestTime;
                        }
                    }
                }
            }
        }
        if (shouldAdd)
        {
            SetNotification(new BasicLocalNotification(allReadyForHarvestTime, BasicLocalNotification.Type.Plantation));
        }
    }

    private void SetUpPatientCuredInDoctorRoom()
    {
        bool shouldAdd = false;
        DateTime CurePatientTime = now;
        DoctorRoom docRoom = null;
        foreach (DoctorRoom doctorRoom in LocalNotificationController.FindObjectsOfType<DoctorRoom>().ToList())
        {
            if (doctorRoom.state == RotatableObject.State.working && doctorRoom.IsCuring())
            {
                DateTime lastCurePatientTime = now.AddSeconds(doctorRoom.GetTimeToEndCuringCurrentPatient());
                if (!shouldAdd || (shouldAdd && CurePatientTime > lastCurePatientTime))
                {
                    CurePatientTime = lastCurePatientTime;
                    docRoom = doctorRoom;
                }
                shouldAdd = true;
            }
        }
        if (shouldAdd && docRoom != null)
        {
            SetNotification(new BasicLocalNotification(CurePatientTime, BasicLocalNotification.Type.SingleCurePatient, docRoom));
        }
    }

    private void SetUpPatientsCuredNotification()
    {
        bool shouldAdd = false;
        DateTime allLastCurePatientTime = now;
        foreach (DoctorRoom doctorRoom in LocalNotificationController.FindObjectsOfType<DoctorRoom>().ToList())
        {
            if (doctorRoom.state == RotatableObject.State.working && doctorRoom.IsCuring())
            {
                shouldAdd = true;
                DateTime lastCurePatientTime = now.AddSeconds(doctorRoom.GetTimeToEndCuring());
                if (allLastCurePatientTime < lastCurePatientTime)
                {
                    allLastCurePatientTime = lastCurePatientTime;
                }
            }
        }
        if (shouldAdd)
        {
            SetNotification(new BasicLocalNotification(allLastCurePatientTime, BasicLocalNotification.Type.DoctorRoom));
        }
    }

    private void SetUpLevelNotification()
    {
        if (data != null)
        {
            SetUpLevelNotification(6, 0, 50, data.Level6Part1NotificationSend, () =>
            {
                data.Level6Part1NotificationSend = true;
            });
            SetUpLevelNotification(6, 51, 100, data.Level6Part2NotificationSend, () =>
            {
                data.Level6Part2NotificationSend = true;
            });
            SetUpLevelNotification(6, 101, 220, data.Level6Part3NotificationSend, () =>
            {
                data.Level6Part3NotificationSend = true;
            });

            SetUpLevelNotification(7, 0, 50, data.Level7Part1NotificationSend, () =>
            {
                data.Level7Part1NotificationSend = true;
            });
            SetUpLevelNotification(7, 51, 100, data.Level7Part2NotificationSend, () =>
            {
                data.Level7Part2NotificationSend = true;
            });
            SetUpLevelNotification(7, 101, 220, data.Level7Part3NotificationSend, () =>
            {
                data.Level7Part3NotificationSend = true;
            });
            SetUpLevelNotification(7, 221, 350, data.Level7Part4NotificationSend, () =>
            {
                data.Level7Part4NotificationSend = true;
            });
        }
    }

    private void SetUpLevelNotification(int targetLevel, int minExp, int maxExp, bool notificationAlreadySend, OnSuccess onSuccess)
    {
        if (!notificationAlreadySend && Game.Instance.gameState().GetHospitalLevel() == targetLevel && Game.Instance.gameState().GetExperienceAmount() >= minExp && Game.Instance.gameState().GetExperienceAmount() <= maxExp)
        {
            onSuccess?.Invoke();
            SetNotification(new BasicLocalNotification(now.AddMinutes(targetLevel == 6 ? LVL_6_GENERIC_PUSH_OFFSET_DURATION_IN_MINUETS : LVL_7_GENERIC_PUSH_OFFSET_DURATION_IN_MINUETS), BasicLocalNotification.Type.Level));
        }
    }

    private void SetUpVIPArrivingNotification()
    {
        VIPSystemManager vipSystemManager = HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>();
        if (vipSystemManager.CanPushVIP())
        {
            SetNotification(new BasicLocalNotification(now.AddSeconds(vipSystemManager.GetSecondsToNextVIP()), BasicLocalNotification.Type.VIPArraving));
        }
    }

    private void SetUpMastershipNotification()
    {
        string name = "";
        DateTime mastershipUpgradeTime;
        int minTime = int.MaxValue;

        foreach (RotatableObject rotatableWithMasterable in LocalNotificationController.FindObjectsOfType<RotatableObject>().ToList().Where(x => x.masterableProperties != null))
        {
            int timeToUpgrade = rotatableWithMasterable.masterableProperties.CalcTimeToMastershipUpgrade();
            if (minTime > timeToUpgrade)
            {
                minTime = timeToUpgrade;
                name = I2.Loc.ScriptLocalization.Get(HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectNameFromShopInfo(rotatableWithMasterable.info.infos.Tag));
            }
        }
        if (minTime < int.MaxValue)
        {
            mastershipUpgradeTime = now.AddSeconds(minTime);
            SetNotification(new BasicLocalNotification(mastershipUpgradeTime, BasicLocalNotification.Type.Masteries, name, null));
        }
    }

    public void CacheNotifications(List<Hospital.LocalNotification> notifications)
    {
        if (BundleManager.Instance == null)
            return;
        BundleManager.Instance.hospitalNotifications = notifications;
    }

    public List<Hospital.LocalNotification> GetCachedNotifications()
    {
        if (BundleManager.Instance == null)
            return null;
        return BundleManager.Instance.maternityNotifications;
    }
}

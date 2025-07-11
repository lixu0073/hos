using UnityEngine;
using System.Collections;
using System;

namespace Hospital
{
    public class BasicLocalNotification : LocalNotification
    {

        public enum Type {
            None,
            Plantation,
            DoctorRoom,
            Level,
            VIPArraving,
            PatientReadyToCure,
            VIPReadyToCure,
            PatientCuresReady,
            Remaind,
            EpidemyEndSoon,
            BubbleBoyFreeEntry,
            DailyQuestNewDay,
            DailyQuestDayEnding,
            VIPEndDiagnose,
            GoodieBox,
            LevelGoal,
            SingleMedicineEnd,
            SingleCurePatient,
            StartGlobalEvent,
            MidGlobalEvent,
            GlobalEventMaxChests,
            Masteries,
            PatientHelpRequest,
            FullVitaminesCollector,
            MotherInBed,
            MotherLaborReady,
            MotherLaborEnded,
            MotherBondingEnded,
            MotherBloodTestEnded,
            DailyRewardToClaimMorining,
            DailyRewardToClaimEvening
        };

        public Type type = Type.None;
        public MedicineRef med;
        public RotatableObject rotatableObject = null;
        private string firstParam;
        private string secondParam;

        public BasicLocalNotification(DateTime fireDate, Type type) : base(fireDate)
        {
            this.type = type;
        }

        public BasicLocalNotification(DateTime fireDate, Type type, string firstParam, string secondParam) : base(fireDate)
        {
            this.type = type;
            this.firstParam = firstParam;
            this.secondParam = secondParam;
        }

        public BasicLocalNotification(DateTime fireDate, Type type, MedicineRef med) : base(fireDate)
        {
            this.type = type;
            this.med = med;
        }

        public BasicLocalNotification(DateTime fireDate, Type type, RotatableObject rotatableObject) : base(fireDate)
        {
            this.type = type;
            this.rotatableObject = rotatableObject;
        }

        public override string GetTagToAnalitics ()
		{
			if (type == Type.None)
            {
				return "Custom";
			}
			return Enum.GetName (typeof(Type), type);
		}

        public override int getCooldown()
        {
            if(type == Type.VIPReadyToCure || type == Type.Level || type == Type.PatientReadyToCure)
            {
                return 0;
            }
            return 6;
        }

        public override string GetBody()
        {
            switch(type)
            {
                case Type.Plantation:
                    return GetRandomBodyOfTag("plants_ready_", 3);
                case Type.DoctorRoom:
                    return GetRandomBodyOfTag("doctors_finished_", 3);
                case Type.Level:
                case Type.Remaind:
                    return GetRandomBodyOfTag("generic_push_", 5);
                case Type.VIPArraving:
                    return Translate("VIP_arrived");
                case Type.PatientReadyToCure:
                    return GetRandomBodyOfTag("patient_ready_", 3);
                case Type.VIPReadyToCure:
                    return GetRandomBodyOfTag("VIP_ready_", 3);
                case Type.PatientCuresReady:
                    return GetRandomBodyOfTag("patient_cures_ready_", 2);
                case Type.EpidemyEndSoon:
                    return Translate("Epidemic_leaving_soon");
                case Type.BubbleBoyFreeEntry:
                    return Translate("Bubble_Boy_free_game");
                case Type.DailyQuestDayEnding:
                    return Translate("Daily_quests_day_ending");
                case Type.DailyQuestNewDay:
                    return Translate("Daily_quests_new_day");
                case Type.VIPEndDiagnose:
                    return GetRandomBodyOfTag("VIP_finished_diagnosis_", 2);
                case Type.GoodieBox:
                    return Translate("GOODIE_BOX_arrived");
                case Type.LevelGoal:
                    return Translate("PUSH_LEVEL_GOALS");
                case Type.SingleMedicineEnd:
                    if(med.IsMedicineForTankElixir())
                    {
                        int random = UnityEngine.Random.Range(0, 2);
                        return string.Format(random == 0 ? Translate("ELIXIR_READY") : Translate("ELIXIR_READY_2"), ResourcesHolder.Get().GetNameForCure(med));
                    }
                    else
                    {
                        return string.Format(Translate("CURE_READY"), ResourcesHolder.Get().GetNameForCure(med));
                    }
                case Type.SingleCurePatient:
                    return string.Format(Translate("DOCTOR_FINISHED_4"), I2.Loc.ScriptLocalization.Get(rotatableObject.GetRoomInfo().ShopTitle));
                case Type.StartGlobalEvent:
                    return Translate("G_E_NOTIFICATION_STARTED");
                case Type.MidGlobalEvent:
                    return Translate("G_E_NOTIFICATION");
                case Type.GlobalEventMaxChests:
                    /*if (HospitalAreasMapController.HospitalMap.globalEventsChestsManager.GetActivityCollectableType() == CollectOnMapActivityGlobalEvent.ActivityCollectableType.ChristmasGift)
                    {
                        return Translate("GINGERBREAD_READY");
                    }
                    else
                    {
                        return Translate("EASTER_EGGS_READY﻿");
                    }*/
                    break;
                case Type.Masteries:
                    return string.Format(Translate("OBJECT_UPGRADED"), firstParam);
                case Type.PatientHelpRequest:
                    return Translate("HELP_NEW_REQUEST_AVAILABLE");
                case Type.FullVitaminesCollector:
                    return Translate("VITAMIN_COLLECTOR_FULL");
                case Type.MotherInBed:
                    return Translate("MOTHER_IN_BED");
                case Type.MotherLaborReady:
                    return Translate("MOTHER_LABOR_READY");
                case Type.MotherLaborEnded:
                    return Translate("MOTHER_LABOR_ENDED");
                case Type.MotherBondingEnded:
                    return Translate("MOTHER_BONDING_ENDED");
                case Type.MotherBloodTestEnded:
                    return Translate("MOTHER_BLOOD_TEST_ENDED");
                case Type.DailyRewardToClaimMorining:
                    return GetRandomBodyOfTag("DAILY_REWARD_MORNING_", 5);
                case Type.DailyRewardToClaimEvening:
                    return GetRandomBodyOfTag("DAILY_REWARD_EVENING_", 3);
            }
            return "";
        }

        public override LocalNotificationController.Group GetGroup()
        {
            switch (type)
            {
                case Type.Plantation:
                    return LocalNotificationController.Group.Garden;
                case Type.DoctorRoom:
                case Type.PatientHelpRequest:
                    return LocalNotificationController.Group.Hospital;
                case Type.Level:
                case Type.Remaind:
                    return LocalNotificationController.Group.Custom;
                case Type.VIPArraving:
                case Type.VIPEndDiagnose:
                    return LocalNotificationController.Group.VIP;
                case Type.PatientReadyToCure:
                    return LocalNotificationController.Group.Hospital;
                case Type.VIPReadyToCure:
                    return LocalNotificationController.Group.VIP;
                case Type.PatientCuresReady:
                    return LocalNotificationController.Group.Hospital;
                case Type.EpidemyEndSoon:
                    return LocalNotificationController.Group.Epidemy;
                case Type.BubbleBoyFreeEntry:
                    return LocalNotificationController.Group.BubbleBoy;
                case Type.DailyQuestDayEnding:
                case Type.DailyQuestNewDay:
                    return LocalNotificationController.Group.DailyQuest;
                case Type.SingleMedicineEnd:
                case Type.SingleCurePatient:
                case Type.Masteries:
                    return LocalNotificationController.Group.Hospital;
                case Type.StartGlobalEvent:
                case Type.MidGlobalEvent:
                case Type.GlobalEventMaxChests:
                    return LocalNotificationController.Group.GameEvent;
                case Type.FullVitaminesCollector:
                    return LocalNotificationController.Group.Laboratory;
                case Type.MotherInBed:
                case Type.MotherLaborReady:
                case Type.MotherLaborEnded:
                case Type.MotherBondingEnded:
                case Type.MotherBloodTestEnded:
                    return LocalNotificationController.Group.Maternity;
            }
            return LocalNotificationController.Group.Custom;
        }

        public override string GetId()
        {
            return GetTagToAnalitics();
        }
    }
}

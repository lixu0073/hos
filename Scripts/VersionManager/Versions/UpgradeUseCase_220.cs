using System;
using System.Collections.Generic;

namespace Hospital
{
    public class UpgradeUseCase_220 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        private readonly string defaultPanaceaString = "PanCollector$(56,54)/West/working;0/0";
        //private readonly string panaceaCollectorIdentifier = "PanCollector";

        Dictionary<StepTag, StepTag> stepTagUpdateMap = new Dictionary<StepTag, StepTag>()
        {
            { StepTag.first_patient_follow, StepTag.build_doctor_text },
            { StepTag.yellow_doc_unpack, StepTag.yellow_doc_elixir_deliver },
            { StepTag.green_doc_build, StepTag.green_doc_unpack },
            { StepTag.blink_box_button, StepTag.bubble_boy_arrow },
            { StepTag.beds_room_text_lvl6, StepTag.level_7 },
            { StepTag.drawer_opened_for_beds_lvl6, StepTag.level_7 },
            { StepTag.beds_room_text_again_lvl6, StepTag.level_7 },
            { StepTag.drawer_opened_for_beds_again_lvl6, StepTag.level_7 },
            { StepTag.treasure_first_spawn, StepTag.vip_flyby_emma_1 },
            { StepTag.treasure_arrow, StepTag.vip_flyby_emma_1 },
            { StepTag.treasure_collected, StepTag.vip_flyby_emma_1 },
            { StepTag.beds_room_text, StepTag.level_8 },
            { StepTag.drawer_opened_for_beds, StepTag.level_8 },
            { StepTag.beds_room_text_again, StepTag.level_8 },
            { StepTag.drawer_opened_for_beds_again, StepTag.level_8 },
            { StepTag.newspaper_2, StepTag.level_goals_ended },
            { StepTag.Wise_booster_1, StepTag.level_goals_ended },
            { StepTag.Wise_booster_2, StepTag.level_goals_ended },
            { StepTag.kids_emma, StepTag.kids_arrow },
            { StepTag.kids_info, StepTag.kids_open },
            { StepTag.diagnose_george, StepTag.diagnose_open_patient_card },
            { StepTag.diagnose_xray_add, StepTag.emma_on_Xray },
            { StepTag.bacteria_bob_1, StepTag.level_19 },
            { StepTag.bacteria_bob_2, StepTag.level_19 },
            { StepTag.bacteria_emma_18, StepTag.level_19 },
            { StepTag.bacteria_george_1, StepTag.bacteria_george_2 },
            { StepTag.maternity_tap_to_open, StepTag.maternity_ready_to_be_unlocked}
        };

        HashSet<StepTag> tagsBeforePanacea = new HashSet<StepTag>()
        {
            StepTag.zero, StepTag.first_msg, StepTag.open_reception_action, StepTag.first_patient_spawn, StepTag.first_patient_follow, StepTag.build_doctor_text,
            StepTag.build_doctor_finish, StepTag.build_doctor_unpack, StepTag.elixir_deliver, StepTag.doctor_speed_up, StepTag.doctor_reward_collect, StepTag.patient_text_2,
            StepTag.name_hospital_close, StepTag.patient_text_3, StepTag.newspaper_1, StepTag.lab_intro_big, StepTag.lab_intro_small, StepTag.elixir_collect_text,
            StepTag.elixir_seed_text_after, StepTag.lab_collector_tap
        };

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeTutorialStepTags(save);
            UpgradePanaceaCollector(save);
            return save;
        }

        private void UpgradeTutorialStepTags(Save save)
        {
            StepTag stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
            if (stepTagUpdateMap.ContainsKey(stepTagFromSaveFile))
                save.TutorialStepTag = stepTagUpdateMap[stepTagFromSaveFile].ToString();
        }

        private void UpgradePanaceaCollector(Save save)
        {
            StepTag stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
            if (tagsBeforePanacea.Contains(stepTagFromSaveFile))
            {
                for (int i = 0; i < save.LaboratoryObjectsData.Count; ++i)
                {
                    if (save.LaboratoryObjectsData[i].Contains("PanCollector"))
                    {
                        save.LaboratoryObjectsData[i] = defaultPanaceaString;
                        return;
                    }
                }
            }
        }
    }
}

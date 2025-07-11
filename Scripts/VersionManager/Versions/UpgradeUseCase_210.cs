
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class UpgradeUseCase_210 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        Dictionary<StepTag, StepTag> stepTagUpdateMap = new Dictionary<StepTag, StepTag>()
        {
            { StepTag.build_doctor_anim,StepTag.build_doctor_text },
            { StepTag.build_doctor_drawer, StepTag.build_doctor_text},
            { StepTag.elixir_deliver_text_before, StepTag.elixir_deliver},
            { StepTag.doctor_reward_text, StepTag.doctor_reward_collect},
            { StepTag.name_hospital_open ,StepTag.name_hospital_close},
            { StepTag.lab_intro_text, StepTag.lab_collector_tap},
            { StepTag.lab_collector_works ,StepTag.elixir_collect_text},
            { StepTag.lab_collector_info ,StepTag.elixir_collect_text},
            { StepTag.elixir_collect_anim ,StepTag.elixir_collect_text},
            { StepTag.elixir_deliver_anim, StepTag.elixir_deliver },
            { StepTag.patient_text_1, StepTag.patient_text_2},
            { StepTag.elixir_seed_text_before ,StepTag.elixir_seed_text_after},
            { StepTag.elixir_seed_anim ,StepTag.elixir_seed_text_after},
            { StepTag.bed_patient_arrived ,StepTag.patient_card_open},
            { StepTag.syrup_production_anim ,StepTag.syrup_production_start},
            { StepTag.emma_on_george ,StepTag.level_4},
            { StepTag.olivia_letter ,StepTag.level_4},
            { StepTag.emma_on_olivia ,StepTag.level_4},

            { StepTag.patio_tidy_1 ,StepTag.patio_tidy_5},
            { StepTag.patio_tidy_2 ,StepTag.patio_tidy_5},
            { StepTag.patio_tidy_3 ,StepTag.patio_tidy_5},
            { StepTag.patio_tidy_4 ,StepTag.patio_tidy_5},
            { StepTag.patio_tidy_6 ,StepTag.level_5},
            { StepTag.new_probe_tables ,StepTag.level_6},
            { StepTag.expand_text ,StepTag.expand_arrow},
            { StepTag.arrange_anim ,StepTag.arrange_text_before},
            { StepTag.wise_2 ,StepTag.wise_pharmacy},

            { StepTag.vip_tease_leo2 ,StepTag.treasure_from_Leo},
            { StepTag.vip_tease_emma ,StepTag.level_9},
            { StepTag.new_cures_1 ,StepTag.new_cures_2},
            { StepTag.Close_Vip_Patient_Card ,StepTag.Emma_on_VIP_Special_rewards},
            { StepTag.daily_quests_button_arrow ,StepTag.daily_quests_unlocked},
        };

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeTutorialStepTags(save);
            UpgradePatioObstaclesSave(save);

            return save;
        }

        private void UpgradeTutorialStepTags(Save save)
        {
            StepTag stepTagFromSaveFile = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
            if (stepTagUpdateMap.ContainsKey(stepTagFromSaveFile))
            {
                save.TutorialStepTag = stepTagUpdateMap[stepTagFromSaveFile].ToString();
            }
        }

        private void UpgradePatioObstaclesSave(Save save)
        {
            List<string> newPatioDecos = new List<string>();
            newPatioDecos.AddRange(save.PatioObjectsData);
            for (int i = newPatioDecos.Count - 1; i >= 0; i--)
            {
                if (newPatioDecos[i].Contains("removable_pond"))
                {
                    string temp = newPatioDecos[i].Replace("removable_pond", "pond");
                    newPatioDecos[i] = temp;
                }
                else if (newPatioDecos[i].Contains("removable_patio_pack"))
                {
                    newPatioDecos.RemoveAt(i);
                }
            }
            save.PatioObjectsData.Clear();
            save.PatioObjectsData.AddRange(newPatioDecos);
        }
    }
}
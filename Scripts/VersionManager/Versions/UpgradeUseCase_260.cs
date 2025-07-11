using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Hospital
{
    public class UpgradeUseCase_260 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        Dictionary<StepTag, StepTag> stepTagUpdateMap = new Dictionary<StepTag, StepTag>()
        {
            { StepTag.keep_curing_patients_l2, StepTag.level_3 },
            { StepTag.syrup_lab_elixirs_missing, StepTag.syrup_production_start },
            { StepTag.yellow_doc_build, StepTag.yellow_doc_add },
            { StepTag.green_doc_add, StepTag.green_doc_add_text },
            { StepTag.pharmacy_tap_to_open, StepTag.pharmacy_open },            
            { StepTag.maternity_tutorial_ended, StepTag.maternity_waiting_for_enter },            
        };

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeTutorialStepTags(save);
            Debug.LogError("Forced Game version");
            save.gameVersion = Application.version;
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
    }
}

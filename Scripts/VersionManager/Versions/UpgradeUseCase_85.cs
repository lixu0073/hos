using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;


namespace Hospital
{
    public class UpgradeUseCase_85 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeTutorial(save);
            UpgradePackagesSave(save);
            UpgradeNewFeaturesBadges(save);
            return save;
        }

        private void UpgradeTutorial(Save save)
        {
            Debug.LogError("Running upgrade use case 85 for tutorial");
            //because we moved daily quests tutorial from level 7 and 50xp to level 10, we must handle users who were on daily quests steps before updating the game,
            //and users who were post level 10, but did not build vip room, so tutorial did not skip to level 10 yet.
            if (save.TutorialStepTag == StepTag.daily_quests_unlocked.ToString()
                || save.TutorialStepTag == StepTag.daily_quests_button_arrow.ToString()
                || save.TutorialStepTag == StepTag.daily_quests_popup_1.ToString()
                || save.TutorialStepTag == StepTag.daily_quests_popup_2.ToString())
            {
                save.TutorialStepTag = StepTag.vip_flyby_emma_1.ToString();
            }

            if (save.Level >= 10
                && TutorialController.Instance.GetStepId((StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag)) < TutorialController.Instance.GetStepId(StepTag.daily_quests_unlocked))
            {
                save.TutorialStepTag = StepTag.daily_quests_unlocked.ToString();
            }
        }
        private void UpgradePackagesSave(Save save) {
            Debug.LogError("Running upgrade use case 85 for packages");
            int tempAmount = DefaultConfigurationProvider.GetConfigCData().PackageIntervals.Count - 1;
            save.Cases = tempAmount.ToString() + "?" + save.Cases;
        }

        private void UpgradeNewFeaturesBadges(Save save) {
            if (save.Level > 7) {
                save.ShowSignIndicator = true;
            }

            if (save.Level > 8) {
                save.ShowPaintBadgeClinic = true;
                save.ShowPaintBadgeLab = true;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Hospital
{
    public class UpgradeUseCase_150 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        private const int DAILY_QUEST_UNLOCK_LEVEL = 10;
        private const int INDEXES_TO_BE_CHANGED = 2;

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeDailyQuestWeekEndField(save);
            return save;
        }

        private void UpgradeDailyQuestWeekEndField(Save save)
        {
            if (string.IsNullOrEmpty(save.DailyQuest)) return;
            var dailyQuestData = save.DailyQuest.Split(';');
            int weeklyEnd = int.Parse(dailyQuestData[0], System.Globalization.CultureInfo.InvariantCulture);
            int playerLevel = save.Level;
            StepTag tutorialStepOfConcern = StepTag.daily_quests_button_arrow;
            StepTag currentTutorialStep = (StepTag)Enum.Parse(typeof(StepTag), save.TutorialStepTag);
            

            if (IsValueDifferentThanDeafault(weeklyEnd) && IsPlayerProgressApplicableToUpdate(playerLevel,currentTutorialStep,tutorialStepOfConcern))
            {
                StringBuilder sb = new StringBuilder();
                string[] valueToChange = save.DailyQuest.Split(';');
                for (int i = 0; i < valueToChange.Length; i++)
                {
                    if (i <= INDEXES_TO_BE_CHANGED)
                    {
                        sb.Append("0;");
                    }
                    else
                    {
                        sb.Append(valueToChange[i] + ";");
                    }
                }
                sb.Remove(sb.Length - 1, 1);
                save.DailyQuest = sb.ToString();
            }
        }

        private bool IsValueDifferentThanDeafault(int value)
        {
            return value != 0;
        }

        private bool IsPlayerProgressApplicableToUpdate(int playerLevel, StepTag currentTutorialStepTag, StepTag tutorialStepOfConcern)
        {
            int tutorialStepOfConcernIndex = TutorialController.Instance.GetStepId(tutorialStepOfConcern);
            int currentTutorialStepIndex = TutorialController.Instance.GetStepId(currentTutorialStepTag);
            return playerLevel <= DAILY_QUEST_UNLOCK_LEVEL && (currentTutorialStepIndex <= tutorialStepOfConcernIndex);
        }
    }
}

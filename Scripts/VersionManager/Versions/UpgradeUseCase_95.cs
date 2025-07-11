using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;


namespace Hospital
{
    public class UpgradeUseCase_95 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public Save Upgrade(Save save, bool visitingPurpose)
        {
            UpgradeLevelGoalsToObjectives(save);
            UpdateCampaingSaveData(save);
            UpgradeLevelGoalWithNewParameters(save);
            return save;
        }

        private void UpgradeLevelGoalsToObjectives(Save save)
        {
            // DO MAGIC STUFF HERE

                if (!string.IsNullOrEmpty(save.LevelGoals))
                {
                    var saveLevelGoal = save.LevelGoals.Split('#');

                    if (save != null && saveLevelGoal.Length > 1)
                    {
                        var levelGoalsSaveData = saveLevelGoal[1].Split('?');

                        if (levelGoalsSaveData.Length > 0)
                        {
                            for (int i = 0; i < levelGoalsSaveData.Length; i++)
                            {
                                var objectiveDataSave = levelGoalsSaveData[i].Split('!');

                                switch (objectiveDataSave[0])
                                {
                                    case "ExpandAreaLevelGoal":
                                        save.LevelGoals = save.LevelGoals.Replace(levelGoalsSaveData[i], UpgradeLevelGoal(levelGoalsSaveData[i], "ExpandAreaObjective"));
                                    break;
                                    case "CurePatientLevelGoal":
                                        save.LevelGoals = save.LevelGoals.Replace(levelGoalsSaveData[i], UpgradeCurePatientLevelGoal(levelGoalsSaveData[i]));
                                        break;
                                    case "BuildRotatableObjectLevelGoal":
                                        save.LevelGoals = save.LevelGoals.Replace(levelGoalsSaveData[i], UpgradeLevelGoal(levelGoalsSaveData[i], "BuildRotatableObjective"));
                                    break;
                                    case "RenovateSpecialObjectLevelGoal":
                                        save.LevelGoals = save.LevelGoals.Replace(levelGoalsSaveData[i], UpgradeLevelGoal(levelGoalsSaveData[i], "RenovateSpecialObjective"));
                                    break;
                                    default:
                                        save.LevelGoals = save.LevelGoals.Replace(levelGoalsSaveData[i], UpgradeLevelGoal(levelGoalsSaveData[i], objectiveDataSave[0]));
                                    break;
                                }
                            }

                            // ADD LEVEL UP GOAL
                            //save.LevelGoals = save.LevelGoals + "?LevelUpObjective!0!0!1!Diamonds^1^False";

                        }
                    }
            }
        }

        private string UpgradeLevelGoal(string levelGoalStr, string newTypeName)
        {
            var goalDataSave = levelGoalStr.Split('!');

            string tmp = newTypeName;

            for (int i = 1; i< goalDataSave.Length; i++)
            {
                if (i < 4 || i > 5)
                    tmp = tmp + "!" + goalDataSave[i];
                else if (i == 5)
                {
                    DefaultObjectiveReward rewad = new DefaultObjectiveReward(int.Parse(goalDataSave[4], System.Globalization.CultureInfo.InvariantCulture), ResourceType.Diamonds, bool.Parse(goalDataSave[5]));
                    tmp = tmp + "!" + rewad.SaveToString();
                }

            }

            return tmp;
        }

        private string UpgradeCurePatientLevelGoal(string levelGoalStr)
        {
            var goalDataSave = levelGoalStr.Split('!');

            string goalType = goalDataSave[0];
            int progressCounter = int.Parse(goalDataSave[1], System.Globalization.CultureInfo.InvariantCulture);
            int uiProgressCounter = int.Parse(goalDataSave[2], System.Globalization.CultureInfo.InvariantCulture);
            int progressGoal = int.Parse(goalDataSave[3], System.Globalization.CultureInfo.InvariantCulture);

            DefaultObjectiveReward rewad = new DefaultObjectiveReward(int.Parse(goalDataSave[4], System.Globalization.CultureInfo.InvariantCulture),ResourceType.Diamonds, bool.Parse(goalDataSave[5]));

            string patientType = goalDataSave[6];
            string rotatableTag = goalDataSave[7];

            if (patientType == "HospitalRoomPatient")
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("CurePatientHospitalRoomObjective");
                builder.Append("!");
                builder.Append(progressCounter);
                builder.Append("!");
                builder.Append(uiProgressCounter);
                builder.Append("!");
                builder.Append(progressGoal);
                builder.Append("!");
                builder.Append(rewad.SaveToString());
                builder.Append("!");
                builder.Append("AnyPatient");  // set cure for Any Room
                return builder.ToString();
            }
            else if (patientType == "DoctorRoomPatient")
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("CurePatientDoctorRoomObjective");
                builder.Append("!");
                builder.Append(progressCounter);
                builder.Append("!");
                builder.Append(uiProgressCounter);
                builder.Append("!");
                builder.Append(progressGoal);
                builder.Append("!");
                builder.Append(rewad.SaveToString());
                builder.Append("!");
                builder.Append("SingleRoom");
                builder.Append("!");
                builder.Append(rotatableTag);
                return builder.ToString();
            }

            return "";
        }

        private void UpgradeLevelGoalWithNewParameters(Save save)
        {
            if (!string.IsNullOrEmpty(save.LevelGoals))
            {
                var saveLevelGoal = save.LevelGoals.Split('#');

                if (save != null && saveLevelGoal.Length > 1)
                {
                    StringBuilder toDos = new StringBuilder();
                    toDos.Append(saveLevelGoal[0]);
                    toDos.Append("#");
                    toDos.Append("false");
                    toDos.Append("#");
                    toDos.Append(saveLevelGoal[1]);

                    save.LevelGoals = toDos.ToString();
                }
            }
        }

        private void UpdateCampaingSaveData(Save save)
        {
            if (!string.IsNullOrEmpty(save.CampaignConfigs))
            {
                var campaing = save.CampaignConfigs.Split('!');

                if (save != null && campaing.Length > 0)
                {
                    for (int i = 0; i < campaing.Length; i++)
                    {
                        var campaignsData = campaing[i].Split('?');

                        if (campaignsData != null && campaignsData.Length > 1)
                        {
                            if (campaignsData[0] == "LevelGoals")
                            {
                                save.CampaignConfigs = save.CampaignConfigs.Replace(campaing[i], "Objectives?ObjectivesDynamic");
                                return;
                            }
                        }
                    }
                    save.CampaignConfigs = save.CampaignConfigs + "Objectives?ObjectivesDynamic";
                }
            }
            else save.CampaignConfigs = "Objectives?ObjectivesHint";
        }
        // SET CAMPAIGN FOR OLD HINT SYSTEM PLAYERS

        private void UpdateMaxGameVersion(Save save) {
            save.maxGameVersion = Application.version;
        }
    }
}
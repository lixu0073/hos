using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;


namespace Hospital
{
    public class UpgradeUseCase_70 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        ExpandlableAreaAdapter expandableAreaAdapter;
        Dictionary<int, int> resources;

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            string[] p;
            resources = new Dictionary<int, int>(save.Elixirs.Count);
            save.Elixirs.ForEach(x =>
            {
                p = x.Split('+');
                resources.Add(int.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture), int.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture));
            });

            UpgradeHospitalRoomPatients(save);
            UpgradeVip(save);
            UpgradeCaseCounter(save);
            UpgradeTutorial(save);
            UpgradeEpidemyMedicineDataIndexAlteration(save);
            return save;
        }

        private void UpgradeEpidemyMedicineDataIndexAlteration(Save save)
        {
            int[] newIndexes = new int[] { 12, 48 }; // provide new indexes in ascending order
            var epidemyData = save.EpidemyData;
            if (epidemyData != null && epidemyData.Count > 0)
            {
                if (Convert.ToBoolean(epidemyData[2]))
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    string[] packagesIndexes = epidemyData[5].Split('!');
                    int levelWhileGeneratingMedicines = Int32.Parse(packagesIndexes.Last());

                    for (int i = 0; i < packagesIndexes.Length - 1; i++)
                    {
                        int medicineIndex = Int32.Parse(packagesIndexes[i]);
                        int updatedMedicineIndex = UpdateMedicineIndex(medicineIndex, newIndexes);
                        stringBuilder.Append(updatedMedicineIndex + "!");
                    }
                    stringBuilder.Append(levelWhileGeneratingMedicines);
                    save.EpidemyData[5] = stringBuilder.ToString();

                    stringBuilder.Length = 0;
                    stringBuilder.Capacity = 0;

                    if (save.EpidemyData.Count > 6)
                    {
                        string[] packageRecord;
                        for (int i = 7; i < epidemyData.Count; i++)
                        {
                            packageRecord = epidemyData[i].Split('!');
                            int medicineIndex = Int32.Parse(packageRecord[0]);
                            int updatedMedicineIndex = UpdateMedicineIndex(medicineIndex, newIndexes);
                            packageRecord[0] = updatedMedicineIndex.ToString();
                            for (int j = 0; j < packageRecord.Length; j++)
                            {
                                stringBuilder.Append(packageRecord[j] + "!");
                            }
                            save.EpidemyData[i] = stringBuilder.ToString();
                            stringBuilder.Length = 0;
                            stringBuilder.Capacity = 0;
                        } 
                    }
                }
            }
        }

        private int UpdateMedicineIndex(int medicineIndex, int[] newMedicineIndexes)
        {
            if (medicineIndex < newMedicineIndexes[0])
            {
                return medicineIndex;
            }

            for (int i = newMedicineIndexes.Length - 1; i >= 0; i--)
            {
                if (medicineIndex >= newMedicineIndexes[i] - Array.IndexOf(newMedicineIndexes, newMedicineIndexes[i]))
                {
                    int indexOffset = i + 1;
                    int updatedMedicineIndex = medicineIndex + indexOffset;
                    return updatedMedicineIndex;
                }
                else
                {
                    continue;
                }
            }

            return 1;   //make sure to pick blue elixir
        }

        private void UpgradeHospitalRoomPatients(Save save)
        {
            var z = save.ClinicObjectsData;

            if (save.ClinicObjectsData != null && save.ClinicObjectsData.Count > 0)
            {
                for (int i = 0; i < save.ClinicObjectsData.Count; i++)
                {
                    var p = z[i].Split('/');
                    if (p.Length > 0)
                    {
                        var stringData1 = p[0].Split('$');
                        if (p[2] == "working")
                        {
                            if (stringData1.Length > 0)
                            {
                                var info = HospitalAreasMapController.HospitalMap.GetPrefabInfo(stringData1[0]);
                                if (info != null)
                                {
                                    var infoController = info.infos.roomController;

                                    switch (infoController.GetType().ToString())
                                    {
                                        case "HospitalRoom":

                                            var oldString = p[3];
                                            var newString = UpgradePatients(p[3]);

                                            save.ClinicObjectsData[i] = save.ClinicObjectsData[i].Replace(oldString, newString);

                                            //Debug.LogError("HospitalRoom patients parameters upgraded in save");

                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string UpgradePatients(string str)
        {
            var patientStrs = str.Split('?').ToList();
            var bedStates = patientStrs[2].Split('%');

            if (patientStrs.Count >= 4)
            {
                for (int i = 03; i < patientStrs.Count; i++)
                {
                    if (patientStrs[i].Length >= 1)
                    {
                        var infos = patientStrs[i].Split('^');
                        if (!string.IsNullOrEmpty(infos[0]))
                        {
                            var patStr = infos[0].Split('!');
                            if (patStr.Length > 6)
                            {
                                var oldStr = patStr[5];
                                var newStr = UpgradeRequiredCures(patStr[5]) + "@False*-1";

                                str = str.Replace(oldStr, newStr);
                            }
                        }
                    }
                }
            }

            return str;
        }

        private void UpgradeVip(Save save)
        {
            var z = save.VIPSystem;

            var str = z.Split('?');
            var patientInfo = str[3].Split('!');

            if (patientInfo.Length > 5)
            {
                var oldStr = patientInfo[4];
                var newStr = patientInfo[4] + "@False*-1";

                save.VIPSystem = save.VIPSystem.Replace(oldStr, newStr);

                Debug.LogError("VIP patient parameters upgraded in save");
            }
        }

        private void UpgradeCaseCounter(Save save)
        {
            string z = save.Cases;
            if (!string.IsNullOrEmpty(z))
            {
                var caseSave = z.Split('?');
                caseSave[0] = (save.saveDateTime + int.Parse(caseSave[0], System.Globalization.CultureInfo.InvariantCulture) - ((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryIntervalSeconds).ToString();
                z = "";
                for (int i = 0; i < caseSave.Length; ++i)
                {
                    z += caseSave[i];
                    if (i < caseSave.Length - 1)
                    {
                        z += "?";
                    }
                }
                save.Cases = z;
                Debug.Log("caseUpgraded: " + z);
            }
        }

        private string UpgradeRequiredCures(string saveString)
        {
            var strs1 = saveString.Split('*');
            var rh = ResourcesHolder.Get();

            bool flag;
            bool canParse = Boolean.TryParse(strs1[6], out flag);
            int medsCount = strs1.Length - (canParse == true ? 8 : 6);

            // STEP1: CHECK IS THERE ANY REQUIRED CURE WHICH IS NOT IN STORAGE
            if (medsCount > 4)
            {
                Dictionary<MedicineRef, int> medicinesToCompare = new Dictionary<MedicineRef, int>();

                for (int i = (canParse == true ? 8 : 6); i < strs1.Length; i++)
                {
                    var pairKeyValue = strs1[i].Split('#');

                    MedicineRef currentMedicine = MedicineRef.Parse(pairKeyValue[0]);
                    if (currentMedicine != null)
                    {
                        var p = rh.GetMedicineInfos(currentMedicine);
                        var amount = int.Parse(pairKeyValue[1], System.Globalization.CultureInfo.InvariantCulture);

                        if (!p.IsDiagnosisMedicine())
                        {
                            if (!resources.ContainsKey(100 * (int)currentMedicine.type + currentMedicine.id))
                            {
                                // DELETE REQUIRED CURE IF MED IS NOT IN STORAGE
                                saveString = saveString.Replace("*" + strs1[i], "");
                                if (medicinesToCompare.Count > 0)
                                    medicinesToCompare.Clear();

                                Debug.LogError("HospitalRoom patients delete REQUIRED CURE in step1: " + strs1[i]);

                                break;
                            }
                            else if (resources[100 * (int)currentMedicine.type + currentMedicine.id] == 0)
                            {
                                // DELETE REQUIRED CURE IF MED IS NOT IN STORAGE
                                saveString = saveString.Replace("*" + strs1[i], "");
                                if (medicinesToCompare.Count > 0)
                                    medicinesToCompare.Clear();

                                Debug.LogError("HospitalRoom patients delete REQUIRED CURE in step1: " + strs1[i]);

                                break;
                            }
                            else
                            {
                                medicinesToCompare.Add(currentMedicine, amount);
                            }
                        }
                    }
                }

                bool step3 = false;

                float time = -1;
                MedicineRef medToRemove = null;

                if (medicinesToCompare.Count > 0)
                {
                    // IF NOT THEN STEP2: GET MEDICINE WITH THE LONGEST TIME TO PRODUCE (BASED ON PREREQUISITES)

                    foreach (var med in medicinesToCompare)
                    {
                        if (med.Value - resources[100 * (int)med.Key.type + med.Key.id] > 0)
                        {
                            var current_time = CalcMedicineTime(rh.GetMedicineInfos(med.Key), med.Value);

                            if (time > current_time)
                            {
                                time = current_time;
                                medToRemove = med.Key;
                            }
                        }
                    }

                    // IF NOT THEN STEP3: GET MEDICINE WITH THE SMALEST REWARD

                    int reward = int.MaxValue;

                    if (medToRemove == null)
                    {
                        step3 = true;

                        foreach (var med in medicinesToCompare)
                        {
                            var current_reward = (ResourcesHolder.Get().GetMaxPriceForCure(med.Key) * med.Value);

                            if (current_reward < reward)
                            {
                                reward = current_reward;
                                medToRemove = med.Key;
                            }
                        }
                    }

                    if (medToRemove != null)
                    {
                        for (int i = (canParse == true ? 8 : 6); i < strs1.Length; i++)
                        {
                            var pairKeyValue = strs1[i].Split('#');

                            MedicineRef currentMedicine = MedicineRef.Parse(pairKeyValue[0]);

                            if (currentMedicine != null && (currentMedicine.id == medToRemove.id && currentMedicine.type == medToRemove.type))
                            {
                                saveString = saveString.Replace("*" + strs1[i], "");

                                if (medicinesToCompare.Count > 0)
                                    medicinesToCompare.Clear();

                                if (!step3)
                                    Debug.LogError("HospitalRoom patients delete REQUIRED CURE in step2 : " + strs1[i]);
                                else
                                    Debug.LogError("HospitalRoom patients delete REQUIRED CURE in step3 : " + strs1[i]);

                                break;
                            }
                        }
                    }
                }



            }

            return saveString;
        }

        private float CalcMedicineTime(MedicineDatabaseEntry med, int amount)
        {
            float time = 0;

            var rh = ResourcesHolder.Get();

            int medsToMakeAmount = amount - resources[100 * (int)med.GetMedicineRef().type + med.GetMedicineRef().id];

            if (medsToMakeAmount > 0)
            {
                time = medsToMakeAmount * med.ProductionTime;

                if (med.Prerequisities != null && med.Prerequisities.Count > 0)
                {
                    for (int i = 0; i < med.Prerequisities.Count; i++)
                    {
                        time += CalcMedicineTime(med.Prerequisities[i].medicine, med.Prerequisities[i].amount);
                    }
                }
            }

            return time;
        }

        private void UpgradeTutorial(Save save)
        {
            //to test
            if (save.TutorialStepTag == StepTag.treasure_first_spawn.ToString())
                save.TutorialStepTag = StepTag.beds_room_text_lvl6.ToString();
        }
    }
}
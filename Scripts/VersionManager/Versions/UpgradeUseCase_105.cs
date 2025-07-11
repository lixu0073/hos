using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;

namespace Hospital
{
    public class UpgradeUseCase_105 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        public bool visitingPurpose;
        public Save Upgrade(Save save,bool visitingPurpose)
        {
            Debug.Log("UpgradeUseCase_105");
            this.visitingPurpose = visitingPurpose;
            UpgradeMasterableRotatableObjects(save);
            return save;
        }

        private void UpgradeMasterableRotatableObjects(Save save)
        {
            UpgradeHospitalObjectsToMasterable(ref save.ClinicObjectsData);
            UpgradeHospitalObjectsToMasterable(ref save.LaboratoryObjectsData);
            UpgradeTreatmentRoomPatients(ref save);
        }

        private void UpgradeHospitalObjectsToMasterable(ref List<string> objectlist)
        {
            int elixirLabTotalProgress = 0;
            List<int> elixirLabIndexes = new List<int>();
            if (objectlist != null && objectlist.Count > 0)
            {
                for (int i = 0; i < objectlist.Count; ++i)
                {
                    var p = objectlist[i].Split(';');
                    if (p.Length < 2)
                    {
                        continue;
                    }
                    var r = p[0].Split('/');
                    if (r.Length < 1)
                    {
                        continue;
                    }
                    var tag = r[0].Split('$');
                    var data = p[1].Split('/');
                    var info = HospitalAreasMapController.HospitalMap.GetPrefabInfo(tag[0]);
                    if (info != null)
                    {
                        if (info.infos.roomController is MedicineProductionMachine || info.infos.roomController is VitaminMaker || info.infos.roomController is DoctorRoom || info.infos.roomController is DiagnosticRoom)
                        {
                            int masteryLevel = 0;
                            int masteryProgress = int.Parse(data[data.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
                            if (string.Compare(info.infos.Tag, "ElixirLab") == 0) {
                                elixirLabTotalProgress += int.Parse(data[data.Length - 1], System.Globalization.CultureInfo.InvariantCulture);
                                elixirLabIndexes.Add(i);
                                continue;
                            }
                            if (!visitingPurpose)
                            {
                                ReportToDeltaMasteryInstaUpgrade(info, masteryProgress);
                            }
                            Debug.Log(tag[0] + " to upgrade");
                            StringBuilder upgraded = new StringBuilder();
                            upgraded.Append(p[0]);
                            
                            if (p.Length > 1)
                            {
                                upgraded.Append(";");
                                upgraded.Append(p[1]);
                            }

                            upgraded.Append(";");
                            upgraded.Append(masteryLevel);
                            upgraded.Append("/");
                            upgraded.Append(masteryProgress);

                            Debug.Log(upgraded.ToString());
                            objectlist.Insert(i, upgraded.ToString());
                            objectlist.RemoveAt(i + 1);
                        }
                    }
                }

                for (int i = 0; i < elixirLabIndexes.Count; ++i)
                {
                    var p = objectlist[elixirLabIndexes[i]].Split(';');
                    if (p.Length < 2)
                    {
                        continue;
                    }
                    var r = p[0].Split('/');
                    if (r.Length < 1)
                    {
                        continue;
                    }
                    var tag = r[0].Split('$');
                    var data = p[1].Split('/');
                    var info = HospitalAreasMapController.HospitalMap.GetPrefabInfo(tag[0]);
                    if (info != null)
                    {
                        if (info.infos.roomController is MedicineProductionMachine || info.infos.roomController is VitaminMaker || info.infos.roomController is DoctorRoom || info.infos.roomController is DiagnosticRoom)
                        {
                            int masteryLevel = 0;
                            if (!visitingPurpose)
                            {
                                ReportToDeltaMasteryInstaUpgrade(info, elixirLabTotalProgress);
                            }
                            Debug.Log(tag[0] + " to upgrade");
                            StringBuilder upgraded = new StringBuilder();
                            upgraded.Append(p[0]);

                            if (p.Length > 1)
                            {
                                upgraded.Append(";");
                                upgraded.Append(p[1]);
                            }

                            upgraded.Append(";");
                            upgraded.Append(masteryLevel);
                            upgraded.Append("/");
                            upgraded.Append(elixirLabTotalProgress);

                            Debug.Log(upgraded.ToString());
                            objectlist.Insert(elixirLabIndexes[i], upgraded.ToString());
                            objectlist.RemoveAt(elixirLabIndexes[i] + 1);
                        }
                    }
                }
            }
        }

        private void UpgradeTreatmentRoomPatients(ref Save save) {
            //
            if (save.ClinicObjectsData != null && save.ClinicObjectsData.Count > 0)
            {
                for (int i = 0; i < save.ClinicObjectsData.Count; ++i)
                {
                    string[] p = save.ClinicObjectsData[i].Split(';');
                    if (p.Length < 2)
                    {
                        continue;
                    }
                    string[] r = p[0].Split('/');
                    if (r.Length < 1)
                    {
                        continue;
                    }
                    var tag = r[0].Split('$');
                    var data = p[1].Split('/');
                    var info = HospitalAreasMapController.HospitalMap.GetPrefabInfo(tag[0]);
                    if (info != null)
                    {
                        if (info.infos.roomController is HospitalRoom)
                        {
                            StringBuilder upgraded = new StringBuilder();
                            List<string> patients = save.ClinicObjectsData[i].Split('?').ToList();
                            if (patients.Count < 4) {
                                continue;
                            }
                            for (int j = 3; j < patients.Count; ++j) {
                                if (string.IsNullOrEmpty(patients[j])) {
                                    continue;
                                }
                                patients.Insert(j, UpgradeTreatmentRoomPatient(patients[j]));
                                patients.RemoveAt(j + 1);
                            }
                            for (int j = 0; j < patients.Count; ++j)
                            {
                                upgraded.Append(patients[j]);
                                if (j < patients.Count - 1) {
                                    upgraded.Append("?");
                                }
                            }

                            Debug.Log(upgraded.ToString());
                            save.ClinicObjectsData.Insert(i, upgraded.ToString());
                            save.ClinicObjectsData.RemoveAt(i + 1);
                        }
                    }
                }
            }
        }

        private string UpgradeTreatmentRoomPatient(string toUpgrade) {
            StringBuilder upgraded = new StringBuilder();
            List<string> patient = toUpgrade.Split('!').ToList();
            string[] interestingPart = patient[5].Split('@');
            string[] infoPart = interestingPart[0].Split('*');

            List<string> medicines = new List<string>();
            for (int i = 8; i < infoPart.Length; ++i)
            {
                medicines.Add(infoPart[i]);
            }
            int sum = 0;

            for (int i = 0; i < medicines.Count; ++i)
            {
                string[] medicine = medicines[i].Split('#');
                MedicineRef med = MedicineRef.Parse(medicine[0]);
                int amount = int.Parse(medicine[1], System.Globalization.CultureInfo.InvariantCulture);
                MedicineDatabaseEntry medData = ResourcesHolder.Get().GetMedicineInfos(med);
                sum += medData.maxPrice * amount;
                if (medData.IsDiagnosisMedicine()) {
                    int positiveEnergyValue = 50;   //this was the max selling price in pharmacy when PE was still a medicine object
                    int positiveEnergyAmount = 0;
                    switch (medData.Disease.DiseaseType)
                    {
                        case DiseaseType.Bone:
                            positiveEnergyAmount = ((DiagnosticRoomInfo)HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag("Xray")).GetPositiveEnergyCost();
                            break;
                        case DiseaseType.Lungs:
                            positiveEnergyAmount = ((DiagnosticRoomInfo)HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag("BloodPressure")).GetPositiveEnergyCost();
                            break;
                        case DiseaseType.Kidneys:
                            positiveEnergyAmount = ((DiagnosticRoomInfo)HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag("Laser")).GetPositiveEnergyCost();
                            break;
                        case DiseaseType.Ear:
                            positiveEnergyAmount = ((DiagnosticRoomInfo)HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag("UltraSound")).GetPositiveEnergyCost();
                            break;
                        case DiseaseType.Brain:
                            positiveEnergyAmount = ((DiagnosticRoomInfo)HospitalAreasMapController.HospitalMap.drawerDatabase.GetObjectInfoWithTag("Mri")).GetPositiveEnergyCost();
                            break;
                        default:
                            break;
                    }
                    sum += positiveEnergyValue * positiveEnergyAmount;
                }
            }
            if (sum == 0) {
                return toUpgrade;
            }
            int exp = int.Parse(infoPart[2], System.Globalization.CultureInfo.InvariantCulture);
            int coins = int.Parse(infoPart[3], System.Globalization.CultureInfo.InvariantCulture);

            Debug.Log("TreatmentRoom patient old exp: " + exp.ToString() +", old coins: " + coins.ToString());

            float sumRandomizer = (float)(exp + coins) / (float)sum;
            float coinFactor = (float)coins / (float)(exp + coins);

            patient[5] += "@" + sumRandomizer.ToString() + "*" + coinFactor.ToString();

            for (int i = 0; i < patient.Count; ++i)
            {
                upgraded.Append(patient[i]);
                if (i < patient.Count - 1) {
                    upgraded.Append("!");
                }
            }

            return upgraded.ToString();
        }

        private void ReportToDeltaMasteryInstaUpgrade(Rotations info, int masteryProgress)
        {
            int masteryLevelThaWillBeGivenInstantly = 0;
            int progressCounter = masteryProgress;
            MasterableConfigData data = MasterySystemParser.Instance.GetMasterableConfigData(info.infos.Tag);
            if (data != null && data.MasteryGoals != null && data.MasteryGoals.Length > 0)
            {
                for (int i = 0; i < data.MasteryGoals.Length; i++)
                {
                    if (progressCounter >= data.MasteryGoals[i])
                    {
                        progressCounter -= data.MasteryGoals[i];
                        masteryLevelThaWillBeGivenInstantly++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            AnalyticsController.instance.ReportMastershipInstantAfterUpgrade(info.infos.Tag, masteryLevelThaWillBeGivenInstantly);
        }
    }
}
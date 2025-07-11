using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using IsoEngine;

namespace Hospital
{
    public class UpgradeUseCase_110 : BaseUpgradeUseCase, IUpgradeUseCase
    {
        private int patientID;

        public Save Upgrade(Save save, bool visitingPurpose)
        {
            patientID = 0;
            // CLEAR ALL HELP REQUEST FOR CURRENT COGNITO

            UpgradeHospitalRoomPatients(save);
            RepairBoughtAreas(save);
            return save;
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
                                var newStr = patStr[5] + "@" + patientID.ToString() + "@False";
                                str = str.Replace(oldStr, newStr);
                                patientID++;
                            }
                        }
                    }
                }
            }

            return str;
        }

        private void RepairBoughtAreas(Save save) {
            save.UnlockedLaboratoryAreas = save.UnlockedLaboratoryAreas.Distinct().ToList();
            save.UnlockedClinicAreas = save.UnlockedClinicAreas.Distinct().ToList();
        }
    }
}
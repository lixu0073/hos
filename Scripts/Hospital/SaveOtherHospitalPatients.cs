using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;


namespace Hospital
{
    public class SaveOtherHospitalPatients : MonoBehaviour, IPatientSaver
    {
        //matward opcjonalnie mozna dodac. To sie robi dla jakis dziwnychprzypadkow pacjentow.
        public List<string> SaveToStringList()
        {
            List<string> saveList = new List<string>();

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < ClinicPatientAI.Patients.Count; i++)
            {
                if (ClinicPatientAI.Patients[i] != null && (ClinicPatientAI.Patients[i].room == null || (ClinicPatientAI.Patients[i].isFirstEver && ClinicPatientAI.Patients[i].isGoHome)))
                {
                    builder.Append(ClinicPatientAI.Patients[i].SaveToString());
                    saveList.Add(builder.ToString());
                    builder.Length = 0;
                    builder.Capacity = 0;
                }

            }


            return saveList;
        }
        //matward opcjonalnie mozna dodac. To sie robi dla jakis dziwnychprzypadkow pacjentow.
        public void LoadFromStringList(List<string> saveList)
        {
            if (ClinicPatientAI.Patients != null && ClinicPatientAI.Patients.Count > 0)
            {
                for (int i = 0; i < ClinicPatientAI.Patients.Count; i++)
                {
                    if (ClinicPatientAI.Patients[i] != null && ClinicPatientAI.Patients[i].room == null || ClinicPatientAI.Patients[i].isFirstEver || (!ClinicPatientAI.Patients[i].isFirstEver && ClinicPatientAI.Patients[i].isGoHome))
                    {
                        Destroy(ClinicPatientAI.Patients[i].gameObject);
                        ClinicPatientAI.Patients.RemoveAt(i);
                        i--;
                    }
                }
            }


            if (saveList != null && saveList.Count > 0)
            {
                for (int i = 0; i < saveList.Count; i++)
                {
                    //if(!string.IsNullOrEmpty (saveList[i])){
                    var spawner = ReferenceHolder.GetHospital().ClinicAI;

                    var infos = saveList[i].Split('^');
                    if (infos != null)
                    {
                        if (string.IsNullOrEmpty(infos[1]))
                        {
                            var p = spawner.SpawnPatient(null, infos[0]) as ClinicPatientAI;
                            if (p != null)
                            {
                                if (!ClinicPatientAI.Patients.Contains(p))
                                    ClinicPatientAI.Patients.Add(p);
                            }
                        }
                        else
                        {
                            var p = spawner.LoadPatient(null, infos[0], infos[1], bool.Parse(infos[3]), ResourcesHolder.GetHospital().GetClinicDisease(int.Parse(infos[2], System.Globalization.CultureInfo.InvariantCulture))) as ClinicPatientAI;

                            if (p != null)
                            {
                                if (!ClinicPatientAI.Patients.Contains(p))
                                    ClinicPatientAI.Patients.Add(p);
                            }

                            //Debug.Log(ResourcesHolder.Get().GetClinicDisease(int.Parse(infos[2])).ToString());

                            //p.GetComponent<ClinicCharacterInfo>().ShowClinicSicknessCloud();
                            //p.setStateCheckinReception ();
                        }
                    }

                    //}
                }

            }

        }
    }
}
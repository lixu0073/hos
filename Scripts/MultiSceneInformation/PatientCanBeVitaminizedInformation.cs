using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientCanBeVitaminizedInformation : MultiSceneInformation
{
    public static event System.Action PatientCanBeVitaminized;
    List<Dictionary<MedicineRef, int>> listOfVitaminsRequiredForPatients;

    public static PatientCanBeVitaminizedInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;
        if (maternityPatientData.Count > 0)
        {
            List<Dictionary<MedicineRef, int>> listOfVitaminsRequiredForPatients = new List<Dictionary<MedicineRef, int>>();
            for (int i = 0; i < maternityPatientData.Count; i++)
            {
                string[] basePatientInfo = maternityPatientData[i].Split('^');
                string patientMainInfo = basePatientInfo[0];
                MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
                if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.WFC)
                {
                    listOfVitaminsRequiredForPatients.Add(new Dictionary<MedicineRef, int>());
                    string[] medicineInfo = patientMainInfo.Split('!')[3].Split('@')[4].Split('*');
                    foreach (string requiredUnparsedCure in medicineInfo)
                    {
                        if (string.IsNullOrEmpty(requiredUnparsedCure))
                            continue;
                        string[] requiredUnparsedCureArray = requiredUnparsedCure.Split('#');
                        if (requiredUnparsedCureArray.Length >= 2)
                        {
                            int lastIndex = listOfVitaminsRequiredForPatients.Count - 1;
                            listOfVitaminsRequiredForPatients[lastIndex].Add(MedicineRef.Parse(requiredUnparsedCureArray[0]), int.Parse(requiredUnparsedCureArray[1], System.Globalization.CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
            if (listOfVitaminsRequiredForPatients.Count > 0)
            {
                return new PatientCanBeVitaminizedInformation(listOfVitaminsRequiredForPatients);
            }
        }
        return null;
    }

    public void CheckMultiSceneInformatin()
    {
        if (HasEnoughVitaminInStorage())
        {
            OnInformationReadyToSend();
        }
    }

    private bool HasEnoughVitaminInStorage()
    {
        for (int i = 0; i < listOfVitaminsRequiredForPatients.Count; i++)
        {
            int counter = 0;
            int vitaminGoalCounter = listOfVitaminsRequiredForPatients[i].Count;
            foreach (KeyValuePair<MedicineRef, int> medicineData in listOfVitaminsRequiredForPatients[i])
            {
                if (Game.Instance.gameState().GetCureCount(medicineData.Key) < medicineData.Value)
                {
                    break;
                }
                counter++;
                if (counter == vitaminGoalCounter)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = PatientCanBeVitaminized;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }

    private PatientCanBeVitaminizedInformation(List<Dictionary<MedicineRef, int>> listOfVitaminsRequiredForPatients)
    {
        this.listOfVitaminsRequiredForPatients = listOfVitaminsRequiredForPatients;
    }
}

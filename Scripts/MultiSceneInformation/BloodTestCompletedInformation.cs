using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodTestCompletedInformation : MultiSceneInformation
{
    public static event System.Action BloodTestCompleted;
    private int timeOfInfoBroadcast = 0;

    public static BloodTestCompletedInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;

        int shortestTime = -1;

        for (int i = 0; i < maternityPatientData.Count; i++)
        {
            string[] basePatientInfo = maternityPatientData[i].Split('^');
            string patientMainInfo = basePatientInfo[0];
            MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
            if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.ID)
            {
                Maternity.PatientStates.MaternityPatientInDiagnoseStateData ID_data = Maternity.PatientStates.MaternityPatientInDiagnoseState.Parse(patientMainInfo);
                float timeLeft = ID_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
        }

        if (shortestTime != -1)
        {
            return new BloodTestCompletedInformation((int)saveData.MaternitySaveDateTime + shortestTime);
        }
        return null;
    }

    public void CheckMultiSceneInformatin()
    {
        if (ServerTime.getTime() > timeOfInfoBroadcast)
        {
            OnInformationReadyToSend();
        }
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = BloodTestCompleted;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }

    private BloodTestCompletedInformation()
    {
    }

    private BloodTestCompletedInformation(int timeOfInfoBroadcast)
    {
        this.timeOfInfoBroadcast = timeOfInfoBroadcast;
    }
}

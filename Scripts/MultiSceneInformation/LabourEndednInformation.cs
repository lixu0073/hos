using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabourEndednInformation : MultiSceneInformation
{
    public static event System.Action LaborEnded;
    private int timeOfInfoBroadcast = 0;

    public static LabourEndednInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;
        int shortestTime = -1;
        for (int i = 0; i < maternityPatientData.Count; i++)
        {
            string[] basePatientInfo = maternityPatientData[i].Split('^');
            string patientMainInfo = basePatientInfo[0];
            MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
            if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.IL)
            {
                Maternity.PatientStates.MaternityPatientInLabourStateParsedData IL_data = Maternity.PatientStates.MaternityPatientInLaborState.Parse(patientMainInfo);

                float timeLeft = IL_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
            else if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.GTLR)
            {
                Maternity.PatientStates.MaternityPatientGoToLaborStateParsedData GTLR_data = Maternity.PatientStates.MaternityPatientGoToLaborRoomState.Parse(patientMainInfo);

                float timeLeft = GTLR_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
        }
        if (shortestTime != -1)
        {
            return new LabourEndednInformation((int)saveData.MaternitySaveDateTime + shortestTime);
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

    private LabourEndednInformation()
    {
    }

    private LabourEndednInformation(int timeOfInfoBroadcast)
    {
        this.timeOfInfoBroadcast = timeOfInfoBroadcast;
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = LaborEnded;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }
}

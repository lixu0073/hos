using Hospital;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ReadyForLabourInformation : MultiSceneInformation
{
    public static event System.Action ReadyForLabour;
    private int timeOfInfoBroadcast = 0;

    public static ReadyForLabourInformation TryGetInfoFromSave(Save saveData)
    {
        List<string> maternityPatientData = saveData.MaternityPatients;
        int shortestTime = -1;
        for (int i = 0; i < maternityPatientData.Count; i++)
        {
            string[] basePatientInfo = maternityPatientData[i].Split('^');
            string patientMainInfo = basePatientInfo[0];
            MaternityPatientParsedGeneralData data = Maternity.MaternityPatientAI.ParsePatientGeneralData(patientMainInfo);
            if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.WFL)
            {
                Maternity.PatientStates.MaternityPatientWaitingForLabourParsedData WFL_data = Maternity.PatientStates.MaternityPatientWaitingForLaborState.Parse(patientMainInfo);

                float timeLeft = WFL_data.timeLeft;
                if (shortestTime == -1 || timeLeft < shortestTime)
                {
                    shortestTime = (int)timeLeft;
                }
            }
            else if (data.stateTag == Maternity.PatientStates.MaternityPatientStateTag.RFL)
            {
                return new ReadyForLabourInformation();
            }
        }
        if (shortestTime != -1)
        {
            return new ReadyForLabourInformation((int)saveData.MaternitySaveDateTime + shortestTime);
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

    private ReadyForLabourInformation()
    {
    }

    private ReadyForLabourInformation(int timeOfInfoBroadcast)
    {
        this.timeOfInfoBroadcast = timeOfInfoBroadcast;
    }

    private void OnInformationReadyToSend()
    {
        var eventToRise = ReadyForLabour;
        if (eventToRise != null)
        {
            eventToRise();
        }
    }
}
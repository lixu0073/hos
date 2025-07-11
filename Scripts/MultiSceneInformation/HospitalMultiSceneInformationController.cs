using Hospital;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class HospitalMultiSceneInformationController : MultiSceneInformationController
{
    protected override void FillList(Save saveData)
    {
        AddInfoToInformationData(ReadyForLabourInformation.TryGetInfoFromSave(saveData));
        AddInfoToInformationData(PatientArrivedToBedInformation.TryGetInfoFromSave(saveData));
        AddInfoToInformationData(BloodTestCompletedInformation.TryGetInfoFromSave(saveData));
        AddInfoToInformationData(LabourEndednInformation.TryGetInfoFromSave(saveData));
        AddInfoToInformationData(BondingEndedInformation.TryGetInfoFromSave(saveData));
        AddInfoToInformationData(PatientCanBeVitaminizedInformation.TryGetInfoFromSave(saveData));
    }
}

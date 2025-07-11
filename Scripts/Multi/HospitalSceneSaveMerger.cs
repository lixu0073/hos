using System;
using System.Collections;
using System.Collections.Generic;
using Hospital;
using UnityEngine;

[Obsolete]
public class HospitalSceneSaveMerger : IOmniSceneSaveMerger
{
    Save maternitySaveData;

    public void Destroy()
    {
        
    }

    public void Initialize(Save save)
    {
        maternitySaveData = save;
    }
    // saves all maternity things that shall not be affected druing saving in hospital. I.E maternity patients will be saved as "" because thare are no mother patients in hospital.
    public Save MergeSave(Save save)
    {
        Save saveToReturn = save;
        saveToReturn.MaternityClinicObjectsData = maternitySaveData.MaternityClinicObjectsData;
        saveToReturn.MaternityPatientCounter = maternitySaveData.MaternityPatientCounter;
        saveToReturn.MaternityPatioObjectsData = maternitySaveData.MaternityPatioObjectsData;
        saveToReturn.ShowPaintBadgeMaternityClinic = maternitySaveData.ShowPaintBadgeMaternityClinic;
        saveToReturn.UnlockedMaternityWardClinicAreas = maternitySaveData.UnlockedMaternityWardClinicAreas;
        saveToReturn.MaternityCustomization = maternitySaveData.MaternityCustomization;
        saveToReturn.MaternityPatients = maternitySaveData.MaternityPatients;
        saveToReturn.MaternitySaveDateTime = maternitySaveData.MaternitySaveDateTime;
        saveToReturn.MaternityExperience = maternitySaveData.MaternityExperience;
        saveToReturn.MaternityLevel = maternitySaveData.MaternityLevel;
        saveToReturn.NurseRoom = maternitySaveData.NurseRoom;
        saveToReturn.MaternityTutorialStepTag = maternitySaveData.MaternityTutorialStepTag;
        saveToReturn.MaternityIsFirstLoopCompleted = maternitySaveData.MaternityIsFirstLoopCompleted;
        return saveToReturn;
    }
}

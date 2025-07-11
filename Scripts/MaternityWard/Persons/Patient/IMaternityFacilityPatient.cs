using Maternity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMaternityFacilityPatient
{
    MaternityCharacterInfo GetInfoPatient();
    MaternityPatientAI GetPatientAI();
    Transform GetMotherBabyPosition();
    Hospital.RotatableObject GetCurrentOccupiedRoom();
    string GetPatientID();
    void DestroyPatient();
}

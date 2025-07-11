using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class TreatmentPanelData
{
    public MedicineDatabaseEntry medicineData;
    public int cureAmount;
    public int helpAmount;

    public TreatmentPanelData(MedicineDatabaseEntry medicineData, int cureAmount, int helpAmount)
    {
        this.medicineData = medicineData;
        this.cureAmount = cureAmount;
        this.helpAmount = helpAmount;
    }
}

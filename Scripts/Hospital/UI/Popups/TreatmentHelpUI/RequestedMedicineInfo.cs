using UnityEngine;

public class RequestedMedicineInfo {
    public long patientID;
    public MedicineRef med;
    public bool isTankMedicine;
    public int donatedAmount;
    public int donateToSendAmount;
    public int requestedAmount;
    public Transform particlePos;


    public bool IncreaseDonation(int storageAmount)
    {
        int toMaxDonate = GetMaxValToDonate();

        if (donateToSendAmount < storageAmount && donateToSendAmount < toMaxDonate)
        {
            donateToSendAmount++;
            return true;
        }

        return false;
    }

    public bool DecreaseDonation()
    {
        if (donateToSendAmount > 0)
        {
            donateToSendAmount--;
            return true;
        }

        return false;
    }

    public int GetMaxValToDonate()
    {
        return  Mathf.Clamp(requestedAmount - donatedAmount, 0, int.MaxValue);
    }
}

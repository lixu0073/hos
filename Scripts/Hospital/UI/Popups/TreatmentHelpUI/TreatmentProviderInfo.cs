using System.Collections.Generic;
using Hospital;

public class TreatmentProviderInfo : BaseUserModel
{
    List<MedicineAmount> medicines = new List<MedicineAmount>();
    public string ID;

    public TreatmentProviderInfo(string ID, PublicSaveModel saveModel) : base(saveModel.SaveID)
    {
        this.ID = ID;
        SetSave(saveModel);
    }

    public void AddProvidedMedicines(MedicineRef medicine, int amount)
    {
        int id = -1;

        if (isMedExistInArray(medicine, out id))
        {
            medicines[id].amount += amount;
        }
        else
        {
            medicines.Add(new MedicineAmount(medicine,amount));
        }
    }

    public override void SetSave(PublicSaveModel publicSave)
    {
        if (save==null)
            base.SetSave(publicSave);
    }

    private bool isMedExistInArray(MedicineRef medRef, out int id)
    {
        if (medicines.Count > 0)
        {
            for (int i = 0; i < medicines.Count; i++)
            {
                if (medicines[i].medicine.id == medRef.id && medicines[i].medicine.type == medRef.type)
                {
                    id = i; 
                    return true;
                }
            }
        }
 
        id = -1;
        return false;
    }

    public List<ProvidedMedicineInfo> GetProvidedMedicines()
    {
        List<ProvidedMedicineInfo> providedMedsInfo = new List<ProvidedMedicineInfo>();

        foreach (var tst in medicines)
        {
            ProvidedMedicineInfo medInfo = new ProvidedMedicineInfo();
            medInfo.medicine = tst.medicine;
            medInfo.donatedAmount = tst.amount;
            providedMedsInfo.Add(medInfo);
        }

        return providedMedsInfo;
    }
}

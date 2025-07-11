using UnityEngine;
using System.Collections;
using Hospital;

public class ClinicCharacterInfo : BaseCharacterInfo {

    public ClinicDiseaseDatabaseEntry clinicDisease;
    public CharacterInfoCloud infoCloud;
    public GameObject positiveEnergyIcon;

    public override void Initialize()
    {
        //  Debug.Log("ClinicCharacterInfo Initialize with disease: " + clinicDisease.Name);
        base.Initialize();
        this.clinicDisease = null;
        HideClinicSicknessCloud();
    }

    public void Initialize(ClinicDiseaseDatabaseEntry clinicDisease)
    {
      //  Debug.Log("ClinicCharacterInfo Initialize with disease: " + clinicDisease.Name);
        base.Initialize();
        if (clinicDisease == null)
            this.clinicDisease = ScriptableObject.CreateInstance(typeof(ClinicDiseaseDatabaseEntry)) as ClinicDiseaseDatabaseEntry; //  new ClinicDiseaseDatabaseEntry();
        else this.clinicDisease = clinicDisease;
        HideClinicSicknessCloud();
    }

    public void ShowClinicSicknessCloud()
    {
		if(AreaMapController.Map.VisitingMode || clinicDisease.Doctor.Tag == "BlueDoc")
        {
            HideClinicSicknessCloud();
            return;
        }
        // Debug.Log("ShowClinicSicknessCloud");
        //infoCloud.SetInfo(clinicDisease.DiseasePic, clinicDisease.Doctor.DoctorColor);
        infoCloud.gameObject.SetActive(true);
    }

    public void HideClinicSicknessCloud()
    {
       // Debug.Log("HideClinicSicknessCloud");
        infoCloud.gameObject.SetActive(false);
    }

    public void SetSickness(ClinicDiseaseDatabaseEntry clinicDisease)
    {
        this.clinicDisease = clinicDisease;
    }

    public void SetPositiveEnergyIcon(bool isActive)
    {
        positiveEnergyIcon.SetActive(isActive);
    }

    public void ButtonCloud()
    {
        UIController.getHospital.PatientZeroPopUp.Open(this);
    }

	public void SetHeartsActive()
	{
		if (heartsCured != null) {
			heartsCured.SetActive (false);
		}
	}
}

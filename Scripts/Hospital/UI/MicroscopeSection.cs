using UnityEngine;

public class MicroscopeSection : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    BacteriaModule bacteriaModule;
    [SerializeField]
    VitaminModule vitaminModule;
#pragma warning restore 0649
    public delegate void ToggleBacteriaStatus(bool hasBacteria, float frequency, bool IsVip);
    public event ToggleBacteriaStatus BacteriaStatusChanged;


    public void UpdateBacteriaCounter(HospitalCharacterInfo currentCharacter)
    {
        int timeToInfection = -1;
        if (currentCharacter != null)
        {
            HospitalCharacterInfo infectedBy = null;
            timeToInfection = currentCharacter.GetTimeTillInfection(out infectedBy);
        }

        bacteriaModule.SetTimer(timeToInfection);
    }

    public void SetupBacteria(BacteriaModule.SetUpBacteriaData data)
    {
        bacteriaModule.SetBacteriaIcon(data.isSpreading, data.bacteriaID);
        bacteriaModule.SetEventListener(data.patientMedicines);
        bacteriaModule.SetDiseaseTooltip(data.disease, data.medicine);
    }

    public void SetupVitamin()
    {
        vitaminModule.SetupVitamin();
    }

    public void InfoButtonUp()
    {
        bacteriaModule.InfoButtonUp();
    }

    public void ShowBacterialTutorialInfo()
    {   
        bacteriaModule.tutorialInfoLocked = true;
        bacteriaModule.InfoButtonDown();
        NotificationCenter.Instance.TapAnywhere.Invoke(new TapAnywhereEventArgs());
    }

    public void HideBacteriaTutorialInfo()
    {
        bacteriaModule.tutorialInfoLocked = false;
        bacteriaModule.InfoButtonUp();
    }

    public void SetupVitaminInfo(HospitalCharacterInfo info)
    {
        vitaminModule.SetInfos(info);
    }

    public void SetupBacteriaInfo(HospitalCharacterInfo info)
    {
        bacteriaModule.SetInfos(null, null);
        bacteriaModule.HideTooltip();

        if (info != null)
        {
            HospitalCharacterInfo infectedBy = null;
            int timeToInfection = info.GetTimeTillInfection(out infectedBy);
            bacteriaModule.SetInfos(info, infectedBy);

            bacteriaModule.SetTimer(timeToInfection);

            if (infectedBy == null && timeToInfection > 0)
                bacteriaModule.SetPositiveEnergy(info.PositiveEnergyForCure);
            else
                bacteriaModule.SetPositiveEnergy(0);

            if (info.HasBacteria)
            {
                OnBacteriaStatusChanged(true, 0, info.IsVIP);
            }
            else
            {
                if (timeToInfection > 0)
                {
                    OnBacteriaStatusChanged(true, 1, info.IsVIP);
                    bacteriaModule.SetBacteriaIcon(true);
                }
                else
                {
                    OnBacteriaStatusChanged(false, 0, info.IsVIP);
                    bacteriaModule.SetBacteriaIcon(false);
                }
            }
        }
    }

    private void OnBacteriaStatusChanged(bool hasBacteria, float frequency, bool isVip)
    {
        BacteriaStatusChanged?.Invoke(hasBacteria, frequency, isVip);
    }
}

using UnityEngine;
using System.Collections.Generic;
using Hospital;
using System;
using System.Globalization;

public class HospitalCharacterInfo : BaseCharacterInfo
{
    /// <summary>
    /// What medicines does the character need to be healed.
    /// </summary>
    public KeyValuePair<MedicineDatabaseEntry, int>[] requiredMedicines = null;
    public List<Sprite> OriginalClothes;
    public List<SpriteRenderer> BodyParts;
    public DiseaseType DisaseDiagnoseType = DiseaseType.None;
    public bool WasPatientCardSeen = false;
    public bool WasPatientCardSwipe = false;
    public bool WasPatientInfoSeen = false;

    public bool isUpdatedNonDiagnosableMedicineNeeded = false;
    public bool isUpdatedDiagnosableMedicineNeeded = false;

    public GameObject VIPWrapper;

    public Sprite lastMedicine = null;
    public PatientCardInfoStatus patientCardStatus = PatientCardInfoStatus.Default;

    private float savedRandomRewardMultiplier = 1.0f;
    private float savedRandomCoinRewardFactor = 0.5f;

    public long ID;

    public HospitalCharacterInfo()
    {
    }

    public override void Initialize()
    {
        base.Initialize();
        OriginalClothes = new List<Sprite>();

        if (BodyParts != null)
        {
            for (int i = 0; i < BodyParts.Count; ++i)
            {
                OriginalClothes.Add(BodyParts[i].sprite);
            }
        }

        patientCardStatus = PatientCardInfoStatus.Default;
    }

    private BalanceableFloat patientNeedsDiagnosisBalanceable;
    private float PatientNeedsDiagnosis
    {
        get
        {
            if (patientNeedsDiagnosisBalanceable == null)
            {
                patientNeedsDiagnosisBalanceable = BalanceableFactory.CreatePatientToDiagnozeChanceBalanceable();
            }

            return patientNeedsDiagnosisBalanceable.GetBalancedValue();
        }
    }

    private bool CanRandomizeDiagnosis(int diagnosableMedicinesCount)   // RANDOMIZE FOR DIAGNOSIS
    {
        float diagnosisChance = PatientNeedsDiagnosis;

        if (GameState.RandomFloat(0, 1) < diagnosisChance && diagnosableMedicinesCount >= 1)
            return true;
        return false;
    }

    private bool CanRandomizeVitamin(int vitaminMedicineCount)   //  RANDOMIZE FOR VITAMIN
    {
        float vitaminChance = PatientVitaminRequired;
        return GameState.RandomFloat(0, 1) < vitaminChance && vitaminMedicineCount >= 1;
    }

    private BalanceableFloat patientVitaminRequiredBalanceable;
    private float PatientVitaminRequired
    {
        get
        {
            if (patientVitaminRequiredBalanceable == null)
            {
                patientVitaminRequiredBalanceable = BalanceableFactory.CreatePatientVitaminRequiredChanceBalanceable();
            }

            return patientVitaminRequiredBalanceable.GetBalancedValue();
        }
    }

    private BalanceableFloat patientWithBacteriaBalanceable;
    private float PatientWithBacteria
    {
        get
        {
            if (patientWithBacteriaBalanceable == null)
            {
                patientWithBacteriaBalanceable = BalanceableFactory.CreatePatientWithBacteriaChanceBalanceable();
            }

            return patientWithBacteriaBalanceable.GetBalancedValue();
        }
    }

    private bool CanRandomizeBacteria(HospitalRoom room, int bacteriaMedicineCount)   // RANDOMIZE FOR BACTERIA
    {
        if (!TutorialController.Instance.IsTutorialStepCompleted(StepTag.bacteria_emma_micro_5))
            return false;

        float bacteriaChance = PatientWithBacteria;

        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bacteria_spawn)
            bacteriaChance = 1f;

        if (GameState.RandomFloat(0, 1) < bacteriaChance && bacteriaMedicineCount >= 1)
        {
            if ((room != null && !room.HasAnyPatientWithPlague()) || room == null)
                return true;
        }

        return false;
    }

    public void Randomize(HospitalRoom room, bool fromSave = false)
    {
        RequiredMedicinesRandomizer randomizer = new RequiredMedicinesRandomizer();
        requiredMedicines = randomizer.GetRandomMedicines(room, fromSave, IsVIP);
        randomizer = null;

        CalculateReward();
        CheckForDiagnosis();
        CheckForBacteria();
        SetIDForHelp();
    }
    /// <summary>
    /// INFO currently only used for VIP
    /// </summary>
    /// <param name="requiredMedicinesToSet"></param>
    public void SetRequiredMedicines(KeyValuePair<MedicineDatabaseEntry, int>[] requiredMedicinesToSet)
    {
        if (requiredMedicinesToSet == null || requiredMedicinesToSet.Length == 0)
        {
            Randomize(null);
            return;
        }
        requiredMedicines = requiredMedicinesToSet;

        CalculateReward();

        CheckForDiagnosis();
        CheckForBacteria();
        SetIDForHelp();
    }

    private void CheckForDiagnosis()
    {
        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            if (requiredMedicines[i].Key.IsDiagnosisMedicine())
            {
                RequiresDiagnosis = true;
                DisaseDiagnoseType = requiredMedicines[i].Key.Disease.DiseaseType;
                return;
            }
        }
        RequiresDiagnosis = false;
    }

    private void CheckForBacteria()
    {
        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            if (requiredMedicines[i].Key.GetMedicineRef().type == MedicineType.Bacteria)
            {
                HasBacteria = true;
                SetBacteriaValues(requiredMedicines[i].Key.GetMedicineRef().id);
                return;
            }
        }
        HasBacteria = false;
    }

    private void SetIDForHelp()
    {
        ID = (long)ServerTime.getMilliSecTime();
    }

    void CalculateReward()
    {
        string debugInfo = "CalculateReward Report \n";

        float coinRewardMasteryAddition = 0;
        float expRewardMasteryAddition = 0;

        float randomRewardMultiplier = GameState.RandomFloat(.95f, 1.1f);
        float randomCoinRewardFactor = GameState.RandomFloat(.18f, .82f);

        savedRandomRewardMultiplier = randomRewardMultiplier;
        savedRandomCoinRewardFactor = randomCoinRewardFactor;

        debugInfo += "randomRewardMultiplier: " + randomRewardMultiplier.ToString() + " \n";
        debugInfo += "randomCoinRewardFactor: " + randomCoinRewardFactor.ToString() + " \n";

        bool diagnosisMedicinePresent = false;

        //First patient
        if (!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.bed_patient_arrived)
        && Game.Instance.gameState().GetHospitalLevel() <= 3)
        {
            SetFirstPatientReward();
            return;
        }
        int rewardSum = 0;

        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            float medicineReward = (ResourcesHolder.Get().GetMaxPriceForCure(requiredMedicines[i].Key.GetMedicineRef()) * requiredMedicines[i].Value);
            rewardSum += (int)medicineReward;
            MasterableProperties machine = Game.Instance.gameState().GetMasterableProductionMachine(requiredMedicines[i].Key);

            if (machine != null)
            {
                medicineReward *= randomRewardMultiplier;
                float coinsPart = medicineReward * randomCoinRewardFactor;
                float expPart = medicineReward - coinsPart;

                coinRewardMasteryAddition += coinsPart * (machine.CoinRewardMultiplier - 1);
                expRewardMasteryAddition += expPart * (machine.ExpRewardMultiplier - 1);
                debugInfo += "Medicine: " + ResourcesHolder.Get().GetNameForCure(requiredMedicines[i].Key.GetMedicineRef()) + " RewardForCure: " + medicineReward.ToString() + " CoinPart: " + coinsPart.ToString() + " CoinPartBonus: " + coinRewardMasteryAddition.ToString() + " ExpPart: " + expPart.ToString() + " ExpPartBonus: " + expRewardMasteryAddition.ToString() + " \n";
            }

            if (requiredMedicines[i].Key.IsDiagnosisMedicine())
            {
                diagnosisMedicinePresent = true;
            }
        }
        if (diagnosisMedicinePresent)
        {
            HospitalDataHolder hdh = HospitalDataHolder.Instance;
            int positiveEnergyValue = 50;   //this was the max selling price in pharmacy when PE was still a medicine object
            int positiveEnergyAmount = 0;

            switch (DisaseDiagnoseType)
            {
                case DiseaseType.Bone:
                    positiveEnergyAmount = hdh.XRayRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Lungs:
                    positiveEnergyAmount = hdh.LungTestingRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Kidneys:
                    positiveEnergyAmount = hdh.LaserRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Ear:
                    positiveEnergyAmount = hdh.UltrasoundRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Brain:
                    positiveEnergyAmount = hdh.MRIRoom.GetPositiveEnergyCost();
                    break;
                default:
                    break;
            }
            debugInfo += "positiveEnergyBonus: " + (positiveEnergyValue * positiveEnergyAmount).ToString() + " \n";

            rewardSum += positiveEnergyValue * positiveEnergyAmount;
            rewardSum = Mathf.FloorToInt(rewardSum * randomRewardMultiplier);
            CoinsForCure = Mathf.FloorToInt(rewardSum * randomCoinRewardFactor);
            EXPForCure = rewardSum - CoinsForCure;
        }
        else
        {
            rewardSum = Mathf.FloorToInt(rewardSum * randomRewardMultiplier);
            CoinsForCure = Mathf.FloorToInt(rewardSum * randomCoinRewardFactor);
            EXPForCure = rewardSum - CoinsForCure;
        }

        debugInfo += "CoinsBefore: " + CoinsForCure.ToString() + " \n";
        debugInfo += "ExpBefore: " + EXPForCure.ToString() + " \n";


        CoinsForCure += Mathf.CeilToInt(coinRewardMasteryAddition);
        EXPForCure += Mathf.CeilToInt(expRewardMasteryAddition);

        debugInfo += "CoinsAfter: " + CoinsForCure.ToString() + " \n";
        debugInfo += "ExpAfter: " + EXPForCure.ToString() + " \n";

        if (IsVIP)  //VIPs have much higher rewards
        {
            CoinsForCure = CoinsForCure * 2;
            EXPForCure = (int)(EXPForCure * 1.5f);
        }

        debugInfo.Replace("\n", Environment.NewLine);
        Debug.Log(debugInfo);
        TestTextController.SetText(debugInfo);
    }
    /// <summary>
    /// Set the parameters for the first patient ever.
    /// </summary>
    private void SetFirstPatientReward()
    {
        //this is for first patient ever. No random so we can have better control over player's exp.
        CoinsForCure = 14;
        EXPForCure = 10;
        return;
    }
    public void UpdateReward()
    {
        float coinRewardMasteryAddition = 0;
        float expRewardMasteryAddition = 0;

        bool diagnosisMedicinePresent = false;

        Debug.Log("Update reward. tutorialskip " + !TutorialSystem.TutorialController.ShowTutorials + ". Level is under three " + (Game.Instance.gameState().GetHospitalLevel() <= 3));

        Debug.Log("Required medicines count: " + requiredMedicines.Length + " for patient: " + name);

        //First patient
        if (!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.bed_patient_arrived)
        && (Game.Instance.gameState().GetHospitalLevel() <= 3))
        {
            SetFirstPatientReward();
            return;
        }
        int rewardSum = 0;

        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            float medicineReward = (ResourcesHolder.Get().GetMaxPriceForCure(requiredMedicines[i].Key.GetMedicineRef()) * requiredMedicines[i].Value);
            rewardSum += (int)medicineReward;

            MedicineProductionMachine machine = GameState.Get().GetMedicineProductionMachine(requiredMedicines[i].Key);

            if (machine != null)
            {
                medicineReward *= savedRandomRewardMultiplier;
                float coinsPart = medicineReward * savedRandomCoinRewardFactor;
                float expPart = medicineReward - coinsPart;
                coinRewardMasteryAddition += coinsPart * (machine.masterableProperties.CoinRewardMultiplier - 1);
                expRewardMasteryAddition += expPart * (machine.masterableProperties.ExpRewardMultiplier - 1);
            }

            if (requiredMedicines[i].Key.IsDiagnosisMedicine())
            {
                diagnosisMedicinePresent = true;
            }
        }
        if (diagnosisMedicinePresent)
        {
            HospitalResourcesHolder rh = ResourcesHolder.GetHospital();
            HospitalDataHolder hdh = HospitalDataHolder.Instance;
            int positiveEnergyValue = 50;   //this was the max selling price in pharmacy when PE was still a medicine object
            int positiveEnergyAmount = 0;
            switch (DisaseDiagnoseType)
            {
                case DiseaseType.Bone:
                    positiveEnergyAmount = hdh.XRayRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Lungs:
                    positiveEnergyAmount = hdh.LungTestingRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Kidneys:
                    positiveEnergyAmount = hdh.LaserRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Ear:
                    positiveEnergyAmount = hdh.UltrasoundRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Brain:
                    positiveEnergyAmount = hdh.MRIRoom.GetPositiveEnergyCost();
                    break;
                default:
                    break;
            }

            rewardSum += positiveEnergyValue * positiveEnergyAmount;
            rewardSum = Mathf.FloorToInt(rewardSum * savedRandomRewardMultiplier);
            CoinsForCure = Mathf.FloorToInt(rewardSum * savedRandomCoinRewardFactor);
            EXPForCure = rewardSum - CoinsForCure;
        }
        else
        {
            rewardSum = Mathf.FloorToInt(rewardSum * savedRandomRewardMultiplier);
            CoinsForCure = Mathf.FloorToInt(rewardSum * savedRandomCoinRewardFactor);
            EXPForCure = rewardSum - CoinsForCure;
        }

        CoinsForCure += Mathf.CeilToInt(coinRewardMasteryAddition);
        EXPForCure += Mathf.CeilToInt(expRewardMasteryAddition);

        if (IsVIP)  //VIPs have much higher rewards
        {
            CoinsForCure = CoinsForCure * 2;
            EXPForCure = (int)(EXPForCure * 1.5f);
        }
    }

    public int GetTimeTillInfection(out HospitalCharacterInfo infectedBy)
    {
        infectedBy = null;

        if (!IsVIP)
        {
            if (BacteriaGlobalTime > 0)
            {
                infectedBy = GetComponent<HospitalPatientAI>().GetOtherRoomPatient(this);
                if (infectedBy != null)
                {
                    return ((BacteriaGlobalTime + infectedBy.AWS_InfectionTime) - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds));
                }
                else return 0;
            }
            else
            {
                if (HasBacteria)
                {
                    HospitalCharacterInfo otherPatient = GetComponent<HospitalPatientAI>().GetOtherRoomPatient(this);

                    if (otherPatient != null)
                    {
                        return ((otherPatient.BacteriaGlobalTime + AWS_InfectionTime) - Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds));
                    }
                    else
                    {
                        double time = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
                        return (int)(AWS_InfectionTime - time);
                    }
                }
                else return 0;
            }
        }
        else return 0;
    }

    bool IsNewDiseaseDiagnosable(List<MedicineDatabaseEntry> diagnosableMedicines)
    {
        for (int i = 0; i < diagnosableMedicines.Count; i++)
        {
            if (diagnosableMedicines[i].minimumLevel == Game.Instance.gameState().GetHospitalLevel())
                return true;
        }
        return false;
    }

    public void InfectByDisease(MedicineDatabaseEntry infection, int diseasesAmount)
    {
        bool containsInfection = false;
        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            if (requiredMedicines[i].Key == infection)
            {
                containsInfection = true;
                break;
            }
        }

        if (!containsInfection)
        {
            KeyValuePair<MedicineDatabaseEntry, int>[] tempRequiredMedicines = new KeyValuePair<MedicineDatabaseEntry, int>[requiredMedicines.Length + 1];

            for (int i = 0; i < requiredMedicines.Length; ++i)
            {
                tempRequiredMedicines[i] = requiredMedicines[i];
            }
            tempRequiredMedicines[tempRequiredMedicines.Length - 1] = new KeyValuePair<MedicineDatabaseEntry, int>(infection, diseasesAmount);

            requiredMedicines = tempRequiredMedicines;
            UpdateReward();
        }

        SetBacteriaValues(infection.GetMedicineRef().id);

        // ADD Bacteria to hint system
        MedicineBadgeHintsController.Get().AddMedNeededToHeal(infection, diseasesAmount);
        MedicineBadgeHintsController.Get().AddOnlyNeededMedicine(infection.GetMedicineRef(), diseasesAmount);

        BacteriaGlobalTime = 0;
        HasBacteria = true;
    }

    public string SaveToString()
    {
        var result = Checkers.CheckedBool(IsVIP) + "*" + Checkers.CheckedAmount(VIPTime, -1, float.MaxValue, name + " VIPTime").ToString("n2") + "*" + Checkers.CheckedAmount(EXPForCure, 0, int.MaxValue, name + " EXPForCure") + "*" + Checkers.CheckedAmount(CoinsForCure, 0, int.MaxValue, name + " CoinsForCure") + "*"
            + Checkers.CheckedBool(WasPatientCardSeen) + "*" + Checkers.CheckedBool(RequiresDiagnosis) + "*" + Checkers.CheckedBool(WasPatientCardSwipe) + "*" + Checkers.CheckedBool(WasPatientInfoSeen);

        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            result += "*" + Checkers.CheckedMedicine(requiredMedicines[i].Key.GetMedicineRef(), name).ToString();
            result += "#" + Checkers.CheckedAmount(requiredMedicines[i].Value, 1, int.MaxValue, name + "RequiredCuresAmount").ToString();
        }


        result += "@" + Checkers.CheckedBool(HasBacteria) + "*" + Checkers.CheckedAmount(BacteriaGlobalTime, -1, int.MaxValue, name + " PlaqueTime");
        result += "@" + savedRandomRewardMultiplier + "*" + savedRandomCoinRewardFactor;
        result += "@" + ID;
        result += "@" + Checkers.CheckedBool(HelpRequested);

        return result;
    }

    public void FromString(string info)
    {
        var firstPartOfString = info.Split('@');

        // OLD PARTS OF STRING

        var strs1 = firstPartOfString[0].Split('*');
        IsVIP = bool.Parse(strs1[0]);
        VIPTime = float.Parse(strs1[1], CultureInfo.InvariantCulture);

        EXPForCure = int.Parse(strs1[2], System.Globalization.CultureInfo.InvariantCulture);
        CoinsForCure = int.Parse(strs1[3], System.Globalization.CultureInfo.InvariantCulture);

        WasPatientCardSeen = bool.Parse(strs1[4]);
        RequiresDiagnosis = bool.Parse(strs1[5]);

        var rh = ResourcesHolder.Get();

        // PARSE FROM OLD VERSION OF DQ FOR VISITING

        bool flag;
        bool canParse = Boolean.TryParse(strs1[6], out flag);

        WasPatientCardSwipe = (canParse == true ? flag : false);
        WasPatientInfoSeen = (canParse == true ? bool.Parse(strs1[7]) : false);
        requiredMedicines = new KeyValuePair<MedicineDatabaseEntry, int>[strs1.Length - (canParse ? 8 : 6)];
        for (int i = (canParse ? 8 : 6); i < strs1.Length; ++i)
        {
            var pairKeyValue = strs1[i].Split('#');
            MedicineRef med = MedicineRef.Parse(pairKeyValue[0]);
            if (med != null)
            {
                var p = rh.GetMedicineInfos(med);
                var amount = int.Parse(pairKeyValue[1], System.Globalization.CultureInfo.InvariantCulture);

                if (p.IsDiagnosisMedicine())
                {
                    DisaseDiagnoseType = p.Disease.DiseaseType;
                }

                if (p.Disease.DiseaseType == DiseaseType.Bacteria)
                {
                    SetBacteriaValues(med.id);
                }

                requiredMedicines[i - (canParse ? 8 : 6)] = new KeyValuePair<MedicineDatabaseEntry, int>(p, amount);
            }
        }

        // NEW PARTS OF STRING AND PLACE FOR FUTURE PARAMETERS AND UPGRADE SAVE MECHANICS

        var strs2 = firstPartOfString[1].Split('*');
        HasBacteria = bool.Parse(strs2[0]);
        BacteriaGlobalTime = int.Parse(strs2[1], System.Globalization.CultureInfo.InvariantCulture);

        patientCardStatus = PatientCardInfoStatus.Default;

        if (firstPartOfString.Length > 2)
        {
            var strs3 = firstPartOfString[2].Split('*');

            if (strs3.Length > 1)
            {
                savedRandomRewardMultiplier = float.Parse(strs3[0], CultureInfo.InvariantCulture);
                savedRandomCoinRewardFactor = float.Parse(strs3[1], CultureInfo.InvariantCulture);
            }
            else
            {
                savedRandomRewardMultiplier = GameState.RandomFloat(.95f, 1.1f);
                savedRandomCoinRewardFactor = GameState.RandomFloat(.18f, .82f);
            }
        }

        // LOAD HELP REQUEST PATIENT ID FROM SAVE

        if (firstPartOfString.Length > 3)
        {
            if (long.TryParse(firstPartOfString[3], out var id))
            {
                ID = id;
            }
            else
            {
                Debug.LogError("Failed to cast " + firstPartOfString[3] + " as long from character data " + info);
            }
        }
    }

    public List<KeyValuePair<int, MedicineDatabaseEntry>> GetMissingMedicines()
    {
        int amount1, amount2;

        List<KeyValuePair<int, MedicineDatabaseEntry>> missing = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

        if (requiredMedicines != null && requiredMedicines.Length > 0)
        {
            for (int i = 0; i < requiredMedicines.Length; ++i)
            {
                if (!((amount1 = requiredMedicines[i].Value) <= (amount2 = GameState.Get().GetCureCount(requiredMedicines[i].Key.GetMedicineRef()))))
                {
                    missing.Add(new KeyValuePair<int, MedicineDatabaseEntry>(amount1 - amount2, requiredMedicines[i].Key));
                }
            }
        }
        return missing;
    }

    public bool CheckCurePosible(out bool cureWithHelp)
    {
        if (RequiresDiagnosis)
        {
            cureWithHelp = false;
            return false;
        }

        if (!IsVIP && GetComponent<HospitalPatientAI>().state == HospitalPatientAI.CharacterStatus.Diagnose)
        {
            cureWithHelp = false;
            return false;
        }

        if (IsVIP && GetComponent<VIPPersonController>().state == VIPPersonController.CharacterStatus.Diagnose)
        {
            cureWithHelp = false;
            return false;
        }

        int check = 0;
        int amount1, amount2;
        List<KeyValuePair<int, MedicineDatabaseEntry>> missing = new List<KeyValuePair<int, MedicineDatabaseEntry>>();


        Dictionary<MedicineRef, int> requestedMeds = ReferenceHolder.GetHospital().treatmentRoomHelpController.GetHelpedMedicinesForPatient(this);

        if (requiredMedicines != null && requiredMedicines.Length > 0)
        {
            for (int i = 0; i < requiredMedicines.Length; ++i)
            {
                if ((amount1 = requiredMedicines[i].Value) <= (amount2 = (GameState.Get().GetCureCount(requiredMedicines[i].Key.GetMedicineRef()))) + GetHelpedCureAmount(requestedMeds, requiredMedicines[i].Key.GetMedicineRef()))
                {
                    check++;
                }
                else
                    missing.Add(new KeyValuePair<int, MedicineDatabaseEntry>(amount1 - amount2, requiredMedicines[i].Key));
            }

            if (check == requiredMedicines.Length)
            {
                if (requestedMeds.Count > 0)
                    cureWithHelp = true;
                else
                    cureWithHelp = false;

                return true;
            }
            else
            {
                cureWithHelp = false;
                return false;
            }
        }
        else
        {
            cureWithHelp = false;
            return false;
        }
    }

    int GetHelpedCureAmount(Dictionary<MedicineRef, int> requestedMeds, MedicineRef medToFind)
    {
        if (requestedMeds != null && requestedMeds.Count > 0)
        {
            int val = 0;

            if (requestedMeds.TryGetValue(medToFind, out val))
            {
                return val;
            }
        }

        return 0;
    }


    public bool CheckIfLastCureNeeded()
    {
        if (RequiresDiagnosis)
            return false;

        int check = 0;
        bool oneLeft = false;
        int amount1, amount2;

        if (requiredMedicines != null && requiredMedicines.Length > 0)
        {
            oneLeft = false;
            for (int i = 0; i < requiredMedicines.Length; ++i)
            {
                //required
                amount1 = requiredMedicines[i].Value;
                //in storage
                amount2 = GameState.Get().GetCureCount(requiredMedicines[i].Key.GetMedicineRef());
                if (amount1 <= amount2)
                {
                    check++;
                }
                else if (amount1 - 1 == amount2)
                {
                    lastMedicine = requiredMedicines[i].Key.image;
                    oneLeft = true;
                }
            }

            if (check == requiredMedicines.Length - 1 && oneLeft)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else return false;
    }

    public void SetHeartsActive()
    {
        if (heartsCured != null)
        {
            heartsCured.SetActive(false);
        }
    }

    public bool HasVitamin()
    {
        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            if (requiredMedicines[i].Key.GetMedicineRef().type == MedicineType.Vitamins)
            {
                return true;
            }
        }
        return false;
    }

    public MedicineRef GetBacteria()
    {
        if (requiredMedicines != null)
        {
            for (int i = 0; i < requiredMedicines.Length; ++i)
            {
                if (requiredMedicines[i].Key.Disease.DiseaseType == DiseaseType.Bacteria)
                    return requiredMedicines[i].Key.GetMedicineRef();
            }
        }

        return null;
    }

    public void SetBacteriaValues(int id)
    {
        PositiveEnergyForCure = BundleManager.BacteriaReward(id);
        AWS_InfectionTime = BundleManager.BacteriaInfectionTime(id);

    }

    public bool CanGetInfection()
    {
        if (HasBacteria == false && BacteriaGlobalTime > 0)
            return true;
        else return false;
    }

    public enum PatientCardInfoStatus
    {
        Default,
        Discharged,
        Cured,
    }
}

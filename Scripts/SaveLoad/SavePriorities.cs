using UnityEngine;
using System.Collections;

public static class SavePriorities  {

    public static readonly int SaveThreshold = 60;         //this is maximum save interval when player is idle
    public static readonly int MinimumSaveInterval = 3;

    public static readonly int MedicineManipulation = 2;     //per 1 medicine added/removed
    public static readonly int MedicineQueued = 10;
    public static readonly int MedicineReady = 5;
    public static readonly int ElixirSeeded = 3;
    public static readonly int PlantationSeeded = 15;
    public static readonly int PositiveEnergyManipulation = 5;
    public static readonly int DiamondManipulation = 120;
    public static readonly int CoinManipulation = 3;
    public static readonly int TutorialStepCompleted = 8;
    public static readonly int PlantationHelpManipulation = 10;
    public static readonly int PharmacyOrderManipulation = 20;
    public static readonly int ExpandHospital = 30;
    public static readonly int BoosterAdded = 50;
    public static readonly int DoctorPatientHealed = 5;
    public static readonly int BedPatientHealed = 30;
    public static readonly int VIPHealed = 100;
    public static readonly int VIPArrived = 60;
    public static readonly int DiagnoseQueued = 20;
    public static readonly int DiagnoseCompleted = 20;
    public static readonly int StorageUpgraded = 40;        //also for Panacea Collector
    public static readonly int RenovationStarted = 60;
    public static readonly int BuildingPurchased = 40;
    public static readonly int BuildingMoved = 5;
    public static readonly int GameResumed = 40;
    public static readonly int DailyQuest = 60;

    ///Actions that require instant save:
    /// DIAMOND MANIPULATION
    /// LEVEL UP
    /// IAP
    /// FACEBOOK REMINDER POPUP
    /// BOOSTER USED
    /// GIFT BOX OPEN OR COLLECTED
    /// BEFORE VISIT
    /// AFTER OUR HOSPITAL IS LOADED
}

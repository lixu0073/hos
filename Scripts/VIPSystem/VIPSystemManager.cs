using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Text;
using MovementEffects;
using Hospital;

public class VIPSystemManager : MonoBehaviour
{
    [SerializeField] private VIPRoomMasterableClient vipRoomMasterableClient = null;
    [SerializeField] private VIPHelipadMasterableClient vipHelipadMasterableClient = null;

    [SerializeField] private VipSign vipSign = null;
    [SerializeField] private VipHelipad vipHelipad = null;

    public GameObject Heli = null;
    public GameObject Helipod = null;
    /// <summary>
    /// Seconds until the next vip spawns
    /// </summary>
    private int secondsLeft = 0;
    [HideInInspector] public bool VIPinside = false;

    private const int REFRESH_TIME = 1;

    public bool VIPInHeli = false;

    private BalanceableInt nextVIPArriveBalanceableBase;
    private BalanceableInt nextVIPArriveBalanceable;
    private BalanceableInt nextVIPArriveBalanceableUpgraded;
    private BalanceableInt vipCureTimeBalanceableBase;
    private BalanceableInt vipCureTimeBalanceable;
    private BalanceableInt vipCureTimeBalanceableUpgraded;

    public int TotalVIPCured = 0;

    public VipRoomMasterableProperties vipMastership;
    public HeliPadMasterableProperties heliMastership;

    public UnityAction onUpgradeUpdate = null;

    private KeyValuePair<MedicineDatabaseEntry, int>[] requiredMedicines = null;

    public int arriveIntervalSecondsBase
    {
        get
        {
            if (nextVIPArriveBalanceableBase == null)
            {
                nextVIPArriveBalanceableBase = BalanceableFactory.CreateBaseVIPArriveCooldownBalanceable();
            }

            return nextVIPArriveBalanceableBase.GetBalancedValue();
        }
    }

    public int arriveIntervalSeconds
    {
        get
        {
            if (nextVIPArriveBalanceable == null)
            {
                nextVIPArriveBalanceable = BalanceableFactory.CreateVIPArriveCooldownBalanceable(heliMastership);
            }

            return nextVIPArriveBalanceable.GetBalancedValue();
        }
    }

    public int arriveIntervalSecondsUpgraded
    {
        get
        {
            if (nextVIPArriveBalanceableUpgraded == null)
            {
                nextVIPArriveBalanceableUpgraded = BalanceableFactory.CreateAfterUpgradeVIPArriveCooldownBalanceable(heliMastership);
            }

            return nextVIPArriveBalanceableUpgraded.GetBalancedValue();
        }
    }

    public int vipCureTImeSecondsBase
    {
        get
        {
            if (vipCureTimeBalanceableBase == null)
            {
                vipCureTimeBalanceableBase = BalanceableFactory.CreateBaseVIPCureTimeBalanceable();
            }

            return vipCureTimeBalanceableBase.GetBalancedValue();
        }
    }

    public int vipCureTImeSeconds
    {
        get
        {
            if (vipCureTimeBalanceable == null)
            {
                vipCureTimeBalanceable = BalanceableFactory.CreateVIPCureTimeBalanceable(vipMastership);
            }

            return vipCureTimeBalanceable.GetBalancedValue();
        }
    }

    public int vipCureTImeSecondsUpgraded
    {
        get
        {
            if (vipCureTimeBalanceableUpgraded == null)
            {
                vipCureTimeBalanceableUpgraded = BalanceableFactory.CreateAfterUpgradeVIPCureTimeBalanceable(vipMastership);
            }

            return vipCureTimeBalanceableUpgraded.GetBalancedValue();
        }
    }

    public void Initialize()
    {
        if (vipMastership == null)
        {
            vipMastership = new VipRoomMasterableProperties(vipRoomMasterableClient);
        }

        if (heliMastership == null)
        {
            heliMastership = new HeliPadMasterableProperties(vipHelipadMasterableClient);
        }
    }

    private string VIPDefinition;

    [SerializeField] private int speedUpFeeLo = 12;
    [SerializeField] private int speedUpFeeHi = 20;
    private int currentSpeedUpFee = 12;
    public int CurrentSpeedUpFee
    {
        get { return currentSpeedUpFee; }
        private set { currentSpeedUpFee = value; }
    }

    private bool lastVIPHealed = true;
    public bool LastVIPHealed
    {
        get { return lastVIPHealed; }
        set
        {
            lastVIPHealed = value;
            CurrentSpeedUpFee = value ? speedUpFeeLo : speedUpFeeHi;
        }
    }
    private List<int> freeVIPIDs = new List<int>();
    private List<string> VIPdefs = new List<string>();
    IEnumerator<float> VIPWaitingCoroutine;

    [TutorialCondition]
    public bool IsVipAlreadyInBed()
    {
        return GetComponent<VipRoom>().currentVip != null && GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().pajamaOn;
    }

    [TutorialCondition]
    public bool LastVIPWasNotHealed()
    {
        return !LastVIPHealed;
    }

    void Awake()
    {
        CreateVIPdefs();
    }

    public void StartCountingExtraSeconds()
    {
        StartCounting(Mathf.Max(arriveIntervalSeconds, 0));
    }
    /// <summary>
    /// Set the counting for the next vip spawn.
    /// </summary>
    /// <param name="secondsLeft">seconds left until the vip arrives</param>
    public void StartCounting(int secondsLeft)
    {
        gameObject.GetComponent<HospitalBedController>().Beds[0].TimeToNextSpawn = secondsLeft;
        gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatientSpawn;
        //countingStartTime = Convert.ToInt32((ServerTime.Get().GetServerTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds);
        //secondsLeft = secondsToGo;
        if (VIPWaitingCoroutine != null)
        {
            Timing.KillCoroutine(VIPWaitingCoroutine);
            VIPWaitingCoroutine = null;
        }

        // Prevent showing the speed up popup immediately after building the VIP ward
        if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.Vip_Leo_sick_heli)
            VIPInHeli = true;

        VIPWaitingCoroutine = Timing.RunCoroutine(ArriveWaiting(secondsLeft));
    }

    public void CureVip(int amount)
    {
        TotalVIPCured += amount;
        vipMastership.AddMasteryProgress(amount);
        heliMastership.AddMasteryProgress(amount);
    }

    private void StartHeli()
    {
        VIPInHeli = true;
        Heli.SetActive(true);
        Heli.GetComponent<HeliController>().VehicleBusy = true;
        Heli.GetComponent<HeliController>().EngineOn();
        NotificationCenter.Instance.SkipVipSpeedupTutorial.Invoke(new BaseNotificationEventArgs());
    }

    private void SpawnHeli()
    {
        Heli.SetActive(true);
        Heli.GetComponent<HeliController>().EngineOff();
        Heli.GetComponent<HeliController>().VehicleBusy = true;
    }

    public int GetSecondsToLeave()
    {
        if (HospitalAreasMapController.HospitalMap.vipRoom.currentVip == null)
        {
            return -1;
        }
        return (int)HospitalAreasMapController.HospitalMap.vipRoom.currentVip.GetComponent<VIPPersonController>().GetHospitalCharacterInfo().VIPTime;
    }

    public int GetSecondsToNextVIP()
    {
        if (TryGetComponent<VipRoom>(out VipRoom vipRoomComp))
        {
            if (vipRoomComp.currentVip != null && vipRoomComp.currentVip.GetComponent<VIPPersonController>() != null
            && vipRoomComp.currentVip.GetComponent<VIPPersonController>().GetHospitalCharacterInfo() != null)
            {
                return Mathf.Clamp((int)vipRoomComp.currentVip.GetComponent<VIPPersonController>().GetHospitalCharacterInfo().VIPTime + arriveIntervalSeconds, 0, vipCureTImeSeconds + arriveIntervalSeconds);
            }
        }

        return secondsLeft;
    }

    public void EmulateTime(TimePassedObject timePassed)
    {
        if (secondsLeft > 0)
        {
            secondsLeft -= (int)timePassed.GetTimePassed();
            if (secondsLeft < 0)
            {
                secondsLeft = 0;
            }
            gameObject.GetComponent<HospitalBedController>().Beds[0].TimeToNextSpawn = secondsLeft;
        }
    }

    public bool IsVIPRoomUnlocked()
    {
        return (HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState == ExternalRoom.EExternalHouseState.enabled);
    }

    public bool CanPushVIP()
    {
        return IsVIPRoomUnlocked() && GetComponent<VipRoom>().currentVip == null && GetSecondsToNextVIP() > 0;
    }

    public void DebugNextVIPTime()
    {
        if (CanPushVIP())
        {
            Debug.Log("Next VIP in " + GetSecondsToNextVIP().ToString() + "s");
        }
        else
        {
            Debug.Log("Can't push VIP");
        }
    }

    public void UpgradeVipRoomManually()
    {
        vipMastership.UpgradeOnPlayersDemand();
        AnalyticsController.instance.ReportFacilityUpgrade("VipWard", GameState.Get().hospitalLevel, vipMastership.MasteryLevel, ReferenceHolder.GetHospital().vipSystemManager.TotalVIPCured);
    }

    public void UpgradeHeliRoomManually()
    {
        heliMastership.UpgradeOnPlayersDemand();
        AnalyticsController.instance.ReportFacilityUpgrade("VipHelipad", GameState.Get().hospitalLevel, heliMastership.MasteryLevel, ReferenceHolder.GetHospital().vipSystemManager.TotalVIPCured);
    }

    public void InvokeOnUpgradeUpdate()
    {
        onUpgradeUpdate?.Invoke();
    }

    public string SaveToString()
    {
        StringBuilder builder = new StringBuilder();
        int timeLeft = secondsLeft;
        Checkers.CheckInsideVIP(VIPinside, GetComponent<VipRoom>().currentVip);
        builder.Append(Checkers.CheckedAmount(timeLeft, 0, int.MaxValue, "VIP timeLeft"));
        builder.Append("?");
        builder.Append(VIPinside.ToString());
        builder.Append("?");
        if (!string.IsNullOrEmpty(VIPDefinition))
        {
            builder.Append(Checkers.CheckedVIPDEfinition(VIPDefinition));
        }
        else
        {
            builder.Append("noVIP");
        }
        builder.Append("?");
        if (GetComponent<VipRoom>().currentVip != null)
        {
            builder.Append(GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SaveToString());
        }
        else
        {
            builder.Append("noVIP");
        }
        builder.Append("?");
        builder.Append(lastVIPHealed.ToString());
        builder.Append("?");
        for (int i = 0; i < freeVIPIDs.Count; ++i)
        {
            builder.Append(freeVIPIDs[i]);
            if (i < freeVIPIDs.Count - 1)
            {
                builder.Append(";");
            }
        }
        builder.Append("?");
        builder.Append(TotalVIPCured);
        if (vipMastership == null)
        {
            return builder.ToString();
        }
        builder.Append("?");
        builder.Append(vipMastership.SaveToStringMastership());
        if (heliMastership == null)
        {
            return builder.ToString();
        }
        builder.Append("?");
        builder.Append(heliMastership.SaveToStringMastership());
        builder.Append("?");
        builder.Append(GetRequiredMedicinesSaveString());

        return builder.ToString();
    }

    private string GetRequiredMedicinesSaveString()
    {
        if (requiredMedicines == null)
        {
            return "";
        }
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < requiredMedicines.Length; ++i)
        {
            builder.Append(requiredMedicines[i].Key.GetMedicineRef().ToString());
            builder.Append("!");
            builder.Append(requiredMedicines[i].Value.ToString());
            if (i < requiredMedicines.Length - 1)
            {
                builder.Append(";");
            }
        }

        return builder.ToString();
    }

    public void LoadFromString(string saveString, TimePassedObject timeFromSave)
    {
        gameObject.GetComponent<HospitalBedController>().Beds[0].Patient = null;
        gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.NoExist;

        VIPinside = false;

        HospitalAreasMapController.HospitalMap.vipRoom.MakeAVIPBed();
        HospitalAreasMapController.HospitalMap.vipRoom.SetAdorners(false);

        Heli.GetComponent<HeliController>().VehicleBusy = false;
        Heli.GetComponent<HeliController>().HideHeli();

        requiredMedicines = null;

        if (transform.GetComponent<VipRoom>().currentVip != null)
        {
            Destroy(transform.GetComponent<VipRoom>().currentVip);
            transform.GetComponent<VipRoom>().currentVip = null;
        }

        if (HospitalAreasMapController.HospitalMap.vipRoom.ExternalHouseState != ExternalRoom.EExternalHouseState.enabled)
        {
            Initialize();
            vipMastership.LoadFromString(String.Empty, TotalVIPCured);
            heliMastership.LoadFromString(String.Empty, TotalVIPCured);

            if (gameObject.GetComponent<HospitalBedController>().Beds[0].Indicator == null) return;
            if (!IsVIPRoomUnlocked()) gameObject.GetComponent<HospitalBedController>().Beds[0].Indicator.ClearIndicator();

            return;
        }

        gameObject.GetComponent<HospitalBedController>().Beds[0].Indicator.ClearIndicator();

        if (!string.IsNullOrEmpty(saveString))
        {
            if (VIPWaitingCoroutine != null)
            {
                Timing.KillCoroutine(VIPWaitingCoroutine);
                VIPWaitingCoroutine = null;
            }
            var save = saveString.Split('?');
            secondsLeft = int.Parse(save[0], System.Globalization.CultureInfo.InvariantCulture);
            VIPinside = bool.Parse(save[1]);

            Initialize();

            if (save.Length > 7)
            {
                vipMastership.LoadFromString(save[7], TotalVIPCured);
            }
            else
            {
                vipMastership.LoadFromString(String.Empty, TotalVIPCured);
            }

            if (save.Length > 8)
            {
                heliMastership.LoadFromString(save[8], TotalVIPCured);
            }
            else
            {
                heliMastership.LoadFromString(String.Empty, TotalVIPCured);
            }

            if (save.Length > 9)
            {
                requiredMedicines = ParseRequiredMedicines(save[9]);
            }
            else
            {
                SetRequiredMedicines();
            }

            if (save[2] != "noVIP")
            {
                VIPDefinition = save[2];
            }
            else
            {
                VIPDefinition = null;
            }

            if (HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                if (VIPDefinition == null)
                {
                    DefineVIP();
                }
                SpawnHeli();
                GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);

                gameObject.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetInBedState();
                gameObject.GetComponent<HospitalBedController>().Beds[0].Indicator.ClearIndicator();
                return;
            }
            
            LastVIPHealed = save.Length <= 4 || bool.Parse(save[4]);

            if (save.Length > 5)
            {
                if (!string.IsNullOrEmpty(save[5]))
                {
                    var savedIDs = save[5].Split(';');
                    freeVIPIDs.Clear();
                    for (int i = 0; i < savedIDs.Length && i < VIPdefs.Count; ++i)
                    {
                        freeVIPIDs.Add(int.Parse(savedIDs[i], System.Globalization.CultureInfo.InvariantCulture));
                    }
                }
                else
                {
                    GenerateFreeVIPIDs();
                }
            }
            else
            {
                GenerateFreeVIPIDs();
            }

            if (save.Length > 6)
            {
                int.TryParse(save[6], out TotalVIPCured);
            }

            if (secondsLeft > 0)
            {
                VIPinside = false;
                StartCounting(secondsLeft);
                gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatientSpawn;
            }
            else
            {
                secondsLeft = 0;
                if (VIPinside)
                {
                    if (save[3] != "noVIP")
                    {
                        SpawnDefinedVIPOnLoad();
                        gameObject.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().Initialize(save[3], (int)timeFromSave.GetTimePassed());
                        gameObject.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().StartVIPCounting();
                    }
                    else
                    {
                        SpawnDefinedVIP();
                    }

                    if (VIPinside)
                    {
                        SpawnHeli();
                        if (!(transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().Person.State is VIPPersonController.GoToRoom))
                        {
                            gameObject.GetComponent<HospitalBedController>().Beds[0].Patient = transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>();
                            gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.OccupiedBed;
                        }
                        else
                        {
                            gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatient;
                        }
                    }
                    else
                    {
                        Debug.Log("Where is VIP");
                        gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatientSpawn;
                    }
                }
                else
                {
                    gameObject.GetComponent<HospitalBedController>().Beds[0]._BedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatient;
                    StartHeli();
                }
            }
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                gameObject.GetComponent<HospitalBedController>().Beds[0].Indicator.ClearIndicator();
            }
            gameObject.GetComponent<HospitalBedController>().SetIndicator(gameObject.GetComponent<HospitalBedController>().Beds[0]);
            EmulateTime(timeFromSave);
        }
        else
        {
            GenerateDefault();
        }
    }

    private KeyValuePair<MedicineDatabaseEntry, int>[] ParseRequiredMedicines(string toParse)
    {
        if (string.IsNullOrEmpty(toParse))
        {
            return null;
        }

        string[] saveSplit = toParse.Split(';');
        KeyValuePair<MedicineDatabaseEntry, int>[] toReturn = new KeyValuePair<MedicineDatabaseEntry, int>[saveSplit.Length];
        string[] medicineSplit;

        for (int i = 0; i < toReturn.Length; ++i)
        {
            medicineSplit = saveSplit[i].Split('!');
            toReturn[i] = new KeyValuePair<MedicineDatabaseEntry, int>(ResourcesHolder.Get().GetMedicineInfos(MedicineRef.Parse(medicineSplit[0])), int.Parse(medicineSplit[1], System.Globalization.CultureInfo.InvariantCulture));
        }

        return toReturn;
    }

    private void GenerateDefault()
    {
        LastVIPHealed = true;
        GenerateFreeVIPIDs();
    }

    public void GenerateFreeVIPIDs()
    {
        freeVIPIDs.Clear();
        for (int i = 0; i < VIPdefs.Count; ++i)
        {
            freeVIPIDs.Add(i);
        }
    }

    [TutorialTriggerable]
    public void SpawnVipTease()
    {
        VIPinside = true;
        VIPDefinition = "0/0/0";
        GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);
        transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetTutorialTeaseGreetingsState();
    }

    [TutorialTriggerable]
    public void MoveVipToHeliTease()
    {
        if (!TutorialSystem.TutorialController.ShowTutorials)
            return;

        if (!transform.TryGetComponent<VipRoom>(out VipRoom vipRoom))
            return;
        
        var currentVIP = vipRoom.currentVip;

        if (currentVIP == null)
            return;

        if (!currentVIP.TryGetComponent<VIPPersonController>(out VIPPersonController VIPPersonCtrl))
            return;

        bool isLeo = string.Compare(VIPPersonCtrl.definition, "0/0/0") == 0;

        if (!isLeo || (isLeo && GameState.Get().hospitalLevel >= 9))
            return;

        VIPinside = false;

        VIPPersonCtrl.SetTutorialGoToHeliState();
    }

    public void SpawnFirstVIP()
    {
        VIPinside = true;
        VIPDefinition = "0/0/0";
        GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);
        transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetJustArrivedState();
    }

    public void SpawnNewVIP()
    {
        DefineVIP();
        SpawnDefinedVIP();
    }

    private void SpawnDefinedVIP()
    {
        VIPinside = true;
        GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);
        transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetJustArrivedState(); //może to odpalać dopiero kiedy VIP położy się do łóżka
    }

    private void SpawnDefinedVIPToBed()
    {
        VIPinside = true;
        GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);
        transform.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetInBedState();
    }

    private void SpawnDefinedVIPOnLoad()
    {
        VIPinside = true;
        GetComponent<VIPSpawner>().SpawnVIP(VIPDefinition);
    }

    private void DefineVIP()
    {
        int id = GameState.RandomNumber(0, freeVIPIDs.Count);
        int VIPID = freeVIPIDs[id];
        VIPDefinition = VIPdefs[VIPID];
        if (freeVIPIDs.Count < 2)
        {
            GenerateFreeVIPIDs();
        }
        freeVIPIDs.Remove(VIPID);
    }

    [TutorialTriggerable]
    public void DepartVIP()
    {
        VIPinside = false;

        if (transform.TryGetComponent<VipRoom>(out VipRoom vipRoom))
        {
            GameObject currentVIP = vipRoom.currentVip;
            if (currentVIP != null)
                DestroyImmediate(vipRoom.currentVip);
        }

        if (Heli.activeInHierarchy)
            Heli.GetComponent<HeliController>().DepartVIP();

        if (UIController.getHospital.vIPPopUp.gameObject.activeSelf)
        {
            UIController.getHospital.vIPPopUp.Exit();
            Timing.RunCoroutine(DelayedOpenPopup());
        }
    }

    public void SendVIPtoElevator()
    {
        gameObject.GetComponent<VipRoom>().currentVip.GetComponent<VIPPersonController>().SetGoToElevatorState();
    }

    private void CreateVIPdefs()
    {
        VIPdefs.Clear();
        for (int i = 0; i < GetComponent<VIPSpawner>().VIPdatabase.VIPgender.Length; ++i)
        {
            for (int j = 0; j < GetComponent<VIPSpawner>().VIPdatabase.VIPgender[i].VIPrace.Length; ++j)
            {
                for (int k = 0; k < GetComponent<VIPSpawner>().VIPdatabase.VIPgender[i].VIPrace[j].VIPbio.Length - 1; ++k)
                {//-1 because pajamas
                    VIPdefs.Add(i.ToString() + "/" + j.ToString() + "/" + k.ToString());
                }
            }
        }
    }

    public void OnGameEventActivate()
    {
        int time = arriveIntervalSeconds;
        ChangeVIPCountingTime(time);
    }

    public void ChangeVIPCountingTime(int time) //TODO reference this in debug menu if we want to add button to change vip spawn time
    {
        if (VIPWaitingCoroutine == null)
        {
            return;
        }
        if (GetSecondsToNextVIP() > time)
        {
            StartCounting(time);
        }
    }

    IEnumerator<float> ArriveWaiting(int timeLeft)
    {
        secondsLeft = timeLeft;
        while (secondsLeft > 0)
        {
            secondsLeft -= REFRESH_TIME;
            if (secondsLeft < 0)
            {
                secondsLeft = 0;
            }
            yield return Timing.WaitForSeconds(REFRESH_TIME);
        }

        VIPInHeli = true;
        while (Heli.GetComponent<HeliController>().VehicleBusy)
        {
            yield return Timing.WaitForSeconds(1);
        }

        StartHeli();
        VIPWaitingCoroutine = null;
    }

    IEnumerator<float> DelayedOpenPopup()
    {
        yield return 0;
        HospitalAreasMapController.HospitalMap.vipRoom.OnClick();
    }

    public void SetRequiredMedicines()
    {
        RequiredMedicinesRandomizer randomizer = new RequiredMedicinesRandomizer();
        requiredMedicines = randomizer.GetRandomMedicines(null, false, true);
        randomizer = null;
    }

    public KeyValuePair<MedicineDatabaseEntry, int>[] GetRequiredMedicines()
    {
        if (requiredMedicines == null)
        {
            SetRequiredMedicines();
        }
        return requiredMedicines;
    }

    public void BuyHelipadUpgrade()
    {
        //IAPController.instance.BuyHelipadUpgrade(heliMastership.MasteryLevel);
    }

    public void BuyVipWardUpgrade()
    {
        //IAPController.instance.BuyVipWardUpgrade(vipMastership.MasteryLevel);
    }

    public bool HasToolsForUpgradeVipWard()
    {
        KeyValuePair<MedicineRef, int>[] requiredCures = ((MasterableVIPRoomConfigData)(vipMastership.MasterableConfigData)).UpgradeCosts[vipMastership.MasteryLevel];
        int currentAmount;
        int requiredAmount;
        for (int i = 0; i < requiredCures.Length; ++i)
        {
            currentAmount = GameState.Get().GetCureCount(requiredCures[i].Key);
            requiredAmount = requiredCures[i].Value;
            if (currentAmount < requiredAmount)
            {
                requiredCures = null;
                return false;
            }
        }

        requiredCures = null;
        return true;
    }

    public bool HasToolsForUpgradeVipHelipad()
    {
        KeyValuePair<MedicineRef, int>[] requiredCures = ((MasterableVIPHelipadConfigData)(heliMastership.MasterableConfigData)).UpgradeCosts[heliMastership.MasteryLevel];
        int currentAmount;
        int requiredAmount;
        for (int i = 0; i < requiredCures.Length; ++i)
        {
            currentAmount = GameState.Get().GetCureCount(requiredCures[i].Key);
            requiredAmount = requiredCures[i].Value;
            if (currentAmount < requiredAmount)
            {
                requiredCures = null;
                return false;
            }
        }

        requiredCures = null;
        return true;
    }

    [TutorialTriggerable]
    public void SubscribeShowTutorialArrows()
    {
        vipSign.SubscribeShowTutorialArrows();
        vipHelipad.SubscribeShowTutorialArrows();
    }

    [TutorialTriggerable]
    public void SubscribeHideTutorialArrows()
    {
        vipSign.SubscribeHideTutorialArrows();
        vipHelipad.SubscribeHideTutorialArrows();
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hospital;
using IsoEngine;
using SimpleUI;
using MovementEffects;
using System;

public class DiagnosticRoom : RotatableObject, MasterablePropertiesClient
{
    public bool isTaken = false;
    protected Vector2i entrance;
    protected Vector2i machine;
    public IDiagnosePatient currentPatient;
    public RefactoredHospitalPatientAI currentPatient2;
    public HospitalDataHolder.DiagRoomType RoomType;
    public LinkedList<IDiagnosePatient> privateQueue;
    GameObject machineObject;
    int workAnimation;
    int clickAnimation;
    //public int healingDuration;
    public int positiveEnergyCost = 1;
    public bool isHealing = false;
    public bool IsHealing
    {
        get { return IsHealing; }
        set
        {
            if (value || HospitalAreasMapController.HospitalMap.VisitingMode)
                HideIndicator();
            else
            {
                if (HospitalDataHolder.Instance.DiagnosisPatientExists(RoomType))
                    ShowIndicator();
                else
                    HideIndicator();
            }
            isHealing = value;
        }
    }

    public bool shouldWork;
    private int queueSize = 2;
    private Vector3 normalScale = Vector3.zero;
    private Vector3 targetScale = Vector3.zero;
    private bool firstBuilded = true;

    private bool isUnwraping = false;

    protected Vector3 machinePosPatient;
    protected Vector3 nursePos;

    protected NurseController nurse;

    public bool done = false;

    GameObject diagnosisIndicator;

    #region masteryProperties
    private int diagnosisTimeMastered;

    private BalanceableInt DiagnosisTimeBalanceable;

    public int DiagnosisTimeMastered
    {
        get
        {
            if (DiagnosisTimeBalanceable == null)
                DiagnosisTimeBalanceable = BalanceableFactory.CreateDiagnosisTime(((DiagnosticRoomInfo)info.infos).CureTime, masterableProperties);

            return DiagnosisTimeBalanceable.GetBalancedValue();
        }
    }
    #endregion

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public int QueueSize
    {
        get { return queueSize; }
        private set
        {
            if (hover != null)
            {
                if (value <= HospitalDataHolder.Instance.MaxQueueSize)
                {
                    hover.EnlargeQueue(value - queueSize);
                    HospitalDataHolder.Instance.EnlargeQueue(this);
                }
                if (value >= HospitalDataHolder.Instance.MaxQueueSize)
                    hover.SetBuyingEnable(false);
            }
            queueSize = value;

            HospitalDataHolder.Instance.SetQueueSize(((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom, queueSize);
        }
    }

    public bool infoShowed = false;

    public int DiagnosedPatients
    {
        get;
        private set;
    }

    DiagnosticHover hover;

    public void EnlargeQueue()
    {
        if (shouldWork)
            QueueSize += 1;
    }
#pragma warning disable 0649
    Transform floor;
#pragma warning restore 0649
    public override void IsoDestroy()
    {
        if (nurse != null)
            nurse.IsoDestroy();

        if (floor != null)
            GameObject.Destroy(floor.gameObject);

        base.IsoDestroy();
    }

    void Start()
    {
        CheckToShowIndicator();
    }

    protected override void OnStateChange(State newState, State oldState)
    {
        base.OnStateChange(newState, oldState);
        if (floor != null)
            GameObject.Destroy(floor.gameObject);
    }

    protected override void OnClickWaitForUser()
    {
        if (isUnwraping == false)
        {
            Timing.RunCoroutine(DiagnosticRoomUnwrap());
            ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, Tag, ObjectiveRotatableEventArgs.EventType.Unwrap));
            if (!TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_newspaper_diagnosis) && !VisitingController.Instance.IsVisiting)
            {
                if (FindObjectsOfType<DiagnosticRoom>().Length == 2)
                    NotificationCenter.Instance.SecondDiagnosticMachineOpen.Invoke(new BaseNotificationEventArgs());
            }
        }
    }

    IEnumerator<float> DiagnosticRoomUnwrap()
    {
        isUnwraping = true;
        map.GetObject(new Vector2i(position.x, position.y)).GetComponent<Animator>().SetTrigger("Unwrap");

        yield return Timing.WaitForSeconds(.4f);

        var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(position.x + actualData.rotationPoint.x, 0, position.y + actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
        fp.transform.localScale = Vector3.one * .8f;
        fp.SetActive(true);

        yield return Timing.WaitForSeconds(.1f);
        Anchored = false;
        RemoveFromMap();
        Anchored = true;
        state = State.working;
        actualData = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>().prefabData;
        AddToMap();// add without validation because it's not needed
        NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this));

        AddRoom();
        nurse = ReferenceHolder.GetHospital().ClinicAI.SpawnNurse(this);

        UpdateNursePosition(nursePos);

        NotificationCenter.Instance.ObjectExistOnLevel.Invoke(new ObjectExistOnLevelEventArgs());
        CheckToShowIndicator();
    }

    public void UpdateNurseRotation()
    {
        nurse.SetNurseRotation(isHealing == true ? false : true);
    }

    public void UpdateNursePosition(Vector3 npos)
    {
        nurse.UpdateNursePosition(this, npos);
        UpdateNurseRotation();
    }

    protected override void OnClickWorking()
    {
        if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
        {
            Debug.Log("Click won't work because drawer is visibile");
            return;
        }

        //Debug.LogError("OnClickWorking");
        base.OnClickWorking();
        SetBorderActive(true);
        BounceDiagnosticObject();
        ShowDiagnosisHoover(false);
    }

    IEnumerator ShowQueuePopup(bool newPatient = false)
    {
        yield return new WaitForEndOfFrame();   //so Update function assigns currentPatient
        queueSize = GetQueueSize();
        ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ChangeOnTouchType((x) =>
        {
            CloseQueuePopup();
            if (GetBorder() != null)
                SetBorderActive(false);
        });

        yield return new WaitForEndOfFrame();   //so hover properly closes and destroys avatars

        hover = DiagnosticHover.Open();
        hover.gameObject.transform.localScale = Vector3.one * 0.5f;
        hover.Initialize(new Vector2(position.x + actualData.rotationPoint.x, position.y + actualData.rotationPoint.y), privateQueue, queueSize, this);

        hover.ActualizePatientList(currentPatient);
        hover.SetBuyingEnable(HospitalDataHolder.Instance.MaxQueueSize > QueueSize);
        hover.UpdateAccordingToMode();

        MoveCameraToShowHover();
        if (newPatient)
            Timing.RunCoroutine(DelayedShowPositiveEnergyUsed());
    }

    public override RectTransform GetHoverFrame()
    {
        if (hover == null)
            return null;
        return hover.hoverFrame;
    }

    public void ShowDiagnosisHoover(bool newPatient = false)
    {
        StartCoroutine(ShowQueuePopup(newPatient));
    }

    public void ShowPositiveEnergyUsed()
    {
        if (privateQueue != null && privateQueue.Count > 0 && hover != null)
        {
            Vector3 pos = hover.GetAvatarPosition(privateQueue.Count - 1);
            ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, ((DiagnosticRoomInfo)info.infos).GetPositiveEnergyCost(), 0f, ReferenceHolder.Get().giftSystem.particleSprites[4], false);
            SoundsController.Instance.PlayCureInSlot();
        }
    }

    protected override void SetupMasterableProperties()
    {
        if (masterableProperties != null)
            return;

        masterableProperties = new DiagnosticRoomMasterableProperties(this);
    }

    private void CloseQueuePopup()
    {
        if (DiagnosticHover.GetActive() != null)
            DiagnosticHover.GetActive().Close();

        hover = null;
        HospitalAreasMapController.HospitalMap.ResetOntouchAction();
    }

    public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
    {
        base.Initialize(info, position, rotation, _state, shouldDisappear);
        ReinitializeSpots();
        RoomType = ((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom;
        positiveEnergyCost = ((DiagnosticRoomInfo)info.infos).GetPositiveEnergyCost();

        AnimChooser();
        SetupMasterableProperties();

        infoShowed = false;
    }

    public override void RotateRight()
    {
        base.RotateRight();
    }

    private void AnimChooser()
    {
        switch (RoomType)
        {
            case HospitalDataHolder.DiagRoomType.LungTesting:
                workAnimation = AnimHash.Lungtesting_work;
                clickAnimation = AnimHash.Lungtesting_click;
                break;
            case HospitalDataHolder.DiagRoomType.Laser:
                workAnimation = AnimHash.Laser_work;
                clickAnimation = AnimHash.Laser_click;
                break;
            case HospitalDataHolder.DiagRoomType.MRI:
                workAnimation = AnimHash.MSI_work;
                clickAnimation = AnimHash.MSI_click;
                break;
            case HospitalDataHolder.DiagRoomType.UltraSound:
                workAnimation = AnimHash.Ultrasound_work;
                clickAnimation = AnimHash.Ultrasound_click;
                break;
            case HospitalDataHolder.DiagRoomType.XRay:
                workAnimation = AnimHash.Xray_work;
                clickAnimation = AnimHash.Xray_click;
                break;
        }
    }

    protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
    {
        QueueSize = 2;
        DiagnosedPatients = 0;

        if (state == State.working)
            AddRoom();

        var str = save.Split(';');

        List<string> strs = null;

        if (str.Length > 1)
        {
            strs = str[1].Split('/').ToList();
            QueueSize = int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture);
            if (strs.Count > 1)
                DiagnosedPatients = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
            else
                DiagnosedPatients = 0;
        }
        SetupMasterableProperties();
        masterableProperties.LoadFromString(save, DiagnosedPatients);
        base.LoadFromString(save, timePassed, DiagnosedPatients);

        AnimChooser();
        if (state == State.working)
        {
            nurse = ReferenceHolder.GetHospital().ClinicAI.SpawnNurse(this);

            UpdateNursePosition(nursePos);
        }
        EmulateTime(timePassed);
    }

    protected override string SaveToString()
    {
        return base.SaveToString() + ";" + Checkers.CheckedAmount(queueSize, 2, HospitalDataHolder.Instance.MaxQueueSize, "Diagnostic QueueSize") + "/" + DiagnosedPatients.ToString() + ";" + masterableProperties.SaveToStringMastership();
    }

    private float timePassed = 0;

    public override void Notify(int id, object parameters)
    {
        base.Notify(id, parameters);
        if (id == (int)LoadNotification.EmulateTime)
            EmulateTime((TimePassedObject)parameters);
    }

    public override void EmulateTime(TimePassedObject time)
    {
        base.EmulateTime(time);
        timePassed = time.GetTimePassed();
    }

    protected override void AddToMap()
    {
        base.AddToMap();
        ReinitializeSpots();

        if (isoObj != null)
        {
            var p = isoObj.GetGameObject();
            machineObject = p.transform.GetChild(p.transform.childCount - 1).gameObject;

            if (p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.childCount > 2)
            {
                machinePosPatient = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(2).gameObject.transform.position;
                nursePos = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
            }
            else if (p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.childCount > 1) // temporary solution before change prefabs
            {
                machinePosPatient = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
                nursePos = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
            }
        }

        else if (obj != null)
        {
            machineObject = obj.transform.GetChild(obj.transform.childCount - 1).gameObject;

            if (obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.childCount > 2)
            {
                machinePosPatient = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(2).gameObject.transform.position;
                nursePos = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
            }
            else if (obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.childCount > 1)  // temporary solution before change prefabs
            {
                machinePosPatient = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
                nursePos = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
            }
        }

        SetFloor(isoObj != null);

        if (state == State.working && currentPatient != null)
            currentPatient.Notify((int)StateNotifications.OfficeMoved, obj == null);

        if (nurse != null)
        {
            nurse.Notify((int)StateNotifications.OfficeMoved, obj == null);
            UpdateNursePosition(nursePos);
        }
        if (masterableProperties != null)        
            masterableProperties.RefreshMasteryView(false);
    }

    private void SetFloor(bool val)
    {
        if (floor == null)
        {
            //throw new IsoException("diagnostic floor doesn't exist!!");
            return;
        }
        floor.gameObject.SetActive(val);
        floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));
    }

    public GameObject GetMachineObject()
    {
        return machineObject;
    }

    public Vector2i GetMachineSpot()
    {
        return machine + position;
    }

    public void StartHealingAnimation()
    {
        Timing.RunCoroutine(HealingAnimStart());
    }

    public void StopHealingAnimation()
    {
        try
        {
            machineObject.transform.GetChild(0).GetComponent<Animator>().Play(clickAnimation, 0, 0.0f);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Animator - exception: " + e.Message);
        }
    }

    private void AddRoom()
    {
        RoomType = ((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom;
        switch (RoomType)
        {
            case HospitalDataHolder.DiagRoomType.LungTesting:
                privateQueue = HospitalDataHolder.Instance.LungTestingQueue;
                HospitalDataHolder.Instance.LungTestingRoomList.Add(this);
                break;
            case HospitalDataHolder.DiagRoomType.Laser:
                privateQueue = HospitalDataHolder.Instance.LaserQueue;
                HospitalDataHolder.Instance.LaserRoomList.Add(this);
                break;
            case HospitalDataHolder.DiagRoomType.MRI:
                privateQueue = HospitalDataHolder.Instance.MRIQueue;
                HospitalDataHolder.Instance.MRIRoomList.Add(this);
                break;
            case HospitalDataHolder.DiagRoomType.UltraSound:
                privateQueue = HospitalDataHolder.Instance.UltrasoundQueue;
                HospitalDataHolder.Instance.UltrasoundRoomList.Add(this);
                break;
            case HospitalDataHolder.DiagRoomType.XRay:
                privateQueue = HospitalDataHolder.Instance.XRayQueue;
                HospitalDataHolder.Instance.XRayRoomList.Add(this);
                break;
        }
    }

    private int GetQueueSize()
    {
        RoomType = ((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom;

        int count = 0;
        switch (RoomType)
        {
            case HospitalDataHolder.DiagRoomType.LungTesting:
                count = HospitalDataHolder.Instance.LungTestingQueueSize;
                break;
            case HospitalDataHolder.DiagRoomType.Laser:
                count = HospitalDataHolder.Instance.LaserQueueSize;
                break;
            case HospitalDataHolder.DiagRoomType.MRI:
                count = HospitalDataHolder.Instance.MRIQueueSize;
                break;
            case HospitalDataHolder.DiagRoomType.UltraSound:
                count = HospitalDataHolder.Instance.UltrasoundQueueSize;
                break;
            case HospitalDataHolder.DiagRoomType.XRay:
                count = HospitalDataHolder.Instance.XRayQueueSize;
                break;
            default:
                Debug.LogError("QUEUE SIZE BUG!! RoomType = " + RoomType);
                count = 2;
                break;
        }
        if (count < 2)
            count = 2;
        return count;
    }

    public override void SetAnchored(bool value)
    {
        base.SetAnchored(value);

        if (nurse != null)
        {
            nurse.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            UpdateNursePosition(nursePos);
            UpdateNurseRotation();
        }

        foreach (var p in BasePatientAI.patients)
        {
            if (p != null)
            {
                p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            }
        }

        shouldWork = obj == null && Anchored;
    }

    public HospitalDataHolder.DiagRoomType ReturnMachineType()
    {
        return ((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom;
    }

    public Vector2i GetEntrancePosition()
    {
        return entrance + position;
    }

    public Vector2i GetMachinePosition()
    {
        return machine + position;
    }

    public Vector3 GetMachinePositionForPatient()
    {
        return machinePosPatient;// + new Vector3(position.x,0,position.y);
    }

    public void HealNow(IDiamondTransactionMaker diamondTransactionMaker)
    {
        int cost = GetSpeedUpDiagnoseCost();

        if (Game.Instance.gameState().GetDiamondAmount() >= cost)
        {
            DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
            {
                GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpDiagnose, Tag);
                Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1, transform.position.z + actualData.rotationPoint.y);
                ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                currentPatient.StopDiagnose(this);
                UpdateNurseRotation();
            }, diamondTransactionMaker);
        }
        else
        {
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
            UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
        }
    }

    public int GetSpeedUpDiagnoseCost()
    {
        if (currentPatient == null)
            return 0;
        return DiamondCostCalculator.GetCostForAction(DiagnosisTimeMastered - currentPatient.GetDiagnoseTime(), DiagnosisTimeMastered); //dafuk is that???
    }

    public void DischargePatient(HospitalPatientAI pat) { }

    private void ReinitializeSpots()
    {
        foreach (var value in actualData.spotsData)
        {
            switch ((SpotTypes)value.id)
            {
                case SpotTypes.Door:
                    entrance = new Vector2i(value.x, value.y);
                    break;
                case SpotTypes.Machine:
                    machine = new Vector2i(value.x, value.y);
                    break;
                default:
                    break;
            }
        }
    }

    public override void IsoUpdate()
    {
        base.IsoUpdate();
        if (timePassed > 0)
        {
            if (state == State.working && privateQueue.Count > 0)
            {
                currentPatient = null;
                // int maxPatients = 50;
                for (int i = 0; i < QueueSize + 1; ++i)
                {
                    if (timePassed <= 0 || privateQueue.First == null)
                        break;

                    IDiagnosePatient diagnosePatient = privateQueue.First.Value;
                    if (diagnosePatient == null)                    
                        break;

                    float TimeMinus;
                    bool LocalIsTaken;
                    bool successSpeedUp = diagnosePatient.SpeedUp(out TimeMinus, timePassed, out LocalIsTaken);
                    timePassed -= TimeMinus;
                    if (successSpeedUp)
                    {
                        privateQueue.RemoveFirst();

                        ++DiagnosedPatients;
                        masterableProperties.AddMasteryProgress(1);
                        GameState.Get().PatientsCount.AddPatientsDiagnosed(1);
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.DiagnosePatients));

                        ObjectiveNotificationCenter.Instance.DiagnosePatientObjectiveUpdate.Invoke(new ObjectiveEventArgs(1));

                        switch (RoomType)
                        {
                            case HospitalDataHolder.DiagRoomType.LungTesting:
                                ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Lungs));
                                break;
                            case HospitalDataHolder.DiagRoomType.XRay:
                                ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Bone));
                                break;
                            case HospitalDataHolder.DiagRoomType.Laser:
                                ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Kidneys));
                                break;
                            case HospitalDataHolder.DiagRoomType.MRI:
                                ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Brain));
                                break;
                            case HospitalDataHolder.DiagRoomType.UltraSound:
                                ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Ear));
                                break;
                            default:
                                break;
                        }

                        isTaken = LocalIsTaken;
                    }
                    else
                    {
                        isTaken = LocalIsTaken;
                        break;
                    }
                }
                done = true;
                if (isTaken)
                    currentPatient = privateQueue.First.Value;
            }
            timePassed = 0;
        }

        if (state == State.working && currentPatient != null && currentPatient.DoneHealing())
        {
            isTaken = false;
            currentPatient = null;
            privateQueue.RemoveFirst();
            done = true;

            if (!VisitingController.Instance.IsVisiting)
            {
                ++DiagnosedPatients;
                masterableProperties.AddMasteryProgress(1);
                GameState.Get().PatientsCount.AddPatientsDiagnosed(1);
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.DiagnosePatients));

                ObjectiveNotificationCenter.Instance.DiagnosePatientObjectiveUpdate.Invoke(new ObjectiveEventArgs(1));

                switch (RoomType)
                {
                    case HospitalDataHolder.DiagRoomType.LungTesting:
                        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Lungs));
                        break;
                    case HospitalDataHolder.DiagRoomType.XRay:
                        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Bone));
                        break;
                    case HospitalDataHolder.DiagRoomType.Laser:
                        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Kidneys));
                        break;
                    case HospitalDataHolder.DiagRoomType.MRI:
                        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Brain));
                        break;
                    case HospitalDataHolder.DiagRoomType.UltraSound:
                        ObjectiveNotificationCenter.Instance.DiagnosePatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveDiagnosePatientWithDiseaseEventArgs(1, DiseaseType.Ear));
                        break;
                    default:
                        break;
                }
            }

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.DiagnoseCompleted);

            if (privateQueue.Count == 0 && hover != null)
                hover.Close();
        }

        if (state == State.working && !isTaken && privateQueue != null && privateQueue.Count > 0)
        {
            privateQueue.First.Value.GetAI().GetComponent<IDiagnosePatient>().StateToDiagRoom(this.GetComponent<DiagnosticRoom>());
            currentPatient = privateQueue.First.Value;
            isTaken = true;
            HideIndicator();
        }

        if (hover != null && done)
        {
            if (currentPatient != null)
            {
                hover.ActualizePatientList(currentPatient);
                done = false;
            }
        }
    }

    public void SetPatient(IDiagnosePatient patient)
    {
        isTaken = true;
        currentPatient = patient;
    }

    public void SetPatient(RefactoredHospitalPatientAI patient)
    {
        isTaken = true;
        currentPatient2 = patient;
    }

    void BounceDiagnosticObject(bool isBeingBuilt = false)
    {
        //Timing.KillCoroutine(BounceCoroutine());
        Timing.RunCoroutine(BounceCoroutine(isBeingBuilt));
    }

    IEnumerator<float> BounceCoroutine(bool isBeingBuilt = false)
    {
        //Debug.Log("BounceCoroutine");
        float bounceTime = .15f;
        float timer = 0f;
        Transform targetTransform;
        if (isBeingBuilt)
            targetTransform = isoObj.GetGameObject().transform;
        else
        {
            targetTransform = isoObj.GetGameObject().transform;
            if (firstBuilded)
            {
                firstBuilded = false;
                normalScale = Vector3.zero;
            }
        }

        if (normalScale == Vector3.zero)
            normalScale = targetTransform.localScale;

        // if (targetScale == Vector3.zero)
        //    targetScale = targetTransform.localScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);
        targetScale = normalScale * 1.05f; //new Vector3(1.1f, 1.1f, 1.1f);

        //scale up
        if (normalScale != Vector3.zero && normalScale != Vector3.zero)
        {
            //scale up
            while (timer < bounceTime)
            {
                timer += Time.deltaTime;
                targetTransform.localScale = Vector3.Lerp(normalScale, targetScale, timer / bounceTime);
                yield return 0;
            }
            timer = 0f;
            //scale down
            while (timer < bounceTime)
            {
                timer += Time.deltaTime;
                targetTransform.localScale = Vector3.Lerp(targetScale, normalScale, timer / bounceTime);
                yield return 0;
            }
        }
        else yield return 0;
    }

    public void SetIsHealingTag(bool isHealing)
    {
        machineObject.transform.GetChild(0).GetComponent<Animator>().SetBool("isHealing", isHealing);
    }

    public void CheckToShowIndicator()
    {
        if (HospitalDataHolder.Instance.DiagnosisPatientExists(RoomType))
            ShowIndicator();
        else        
            HideIndicator();
    }

    public string GetClientTag()
    {
        if (info != null && info.infos != null)
        {
            return info.infos.Tag;
        }
        return string.Empty;
    }

    public void ShowIndicator()
    {
        if (isHealing || HospitalAreasMapController.HospitalMap.VisitingMode || state != State.working)
            return;

        if (diagnosisIndicator == null)
        {
            diagnosisIndicator = Instantiate(ResourcesHolder.GetHospital().diagnosisIndicatorPrefab);
            diagnosisIndicator.transform.SetParent(transform);
            diagnosisIndicator.transform.localRotation = Quaternion.Euler(45, 45, 0);
            diagnosisIndicator.transform.localPosition = ((DiagnosticRoomInfo)info.infos).IndicatorOffset[(int)actualRotation];
            diagnosisIndicator.GetComponent<DiagnosisIndicatorController>().SetIcon(RoomType);
        }
        if (diagnosisIndicator != null)
            diagnosisIndicator.SetActive(true);
    }

    public void HideIndicator()
    {
        if (diagnosisIndicator == null)
            return;

        diagnosisIndicator.SetActive(false);
    }

    IEnumerator<float> HealingAnimStart()
    {
        while (!HospitalAreasMapController.HospitalMap.IsLoaded)
        {
            yield return 0f;
        }
        if (isHealing)
        {
            try
            {
                machineObject.transform.GetChild(0).GetComponent<Animator>().Play(workAnimation, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SetIsHealingTag(true);
        }
        else
            SetIsHealingTag(false);
    }

    IEnumerator<float> DelayedShowPositiveEnergyUsed()
    {
        yield return Timing.WaitForSeconds(1);
        ShowPositiveEnergyUsed();
    }
}
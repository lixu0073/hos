using UnityEngine;
using IsoEngine;
using System.Collections.Generic;
using Hospital;
using SimpleUI;
using MovementEffects;
using System.Linq;
using System;

public class HospitalRoom : RotatableObject
{
    [System.Serializable]
    public class SpawnedPatient
    {
        public float timeLeft;
        public int bedID;
    }

    public List<SpawnedPatient> pendingPatients;

    private float maxSpawnTime = 0.0f;
    private const float MinSpawnTime = 5.0f;
    private const float MaxSpawnTime = 10.0f;

    public LinkedList<HospitalPatientAI> patientList;
    protected Vector3 roomEntrance = new Vector3(0, 0, 0);
    protected List<Vector2i> freeBedSpots;
    protected bool[] takenBedSpots;
    //protected bool[] unlockedSpots;

    public static bool tutorialMode = false;

    public bool[] TakenBedSpots
    {
        get { return takenBedSpots; }
    }

    //private int bedsCount = 0;
    protected int takenSpotsCounter = 0;
    GameObject flr;
    Transform floor;
    Renderer mat;

    HospitalAmbulance ambulance;

    private Action<HospitalAmbulance> lastSpawnEvent;

    private bool isUnwraping = false;

    public override void SetAnchored(bool value)
    {
        base.SetAnchored(value);

        foreach (var p in BasePatientAI.patients)
        {
            if (p != null)
            {
                // update all patients or all hospitalPatient for this room after anchored
                if (!(p is HospitalPatientAI) || (p is HospitalPatientAI && (!(p as HospitalPatientAI).IsInBedState() && (p as HospitalPatientAI).GetDestRoom() == this)))
                    p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            }
        }

        if (!value)
            HospitalAreasMapController.HospitalMap.DisableMutalForStaticDoor();
    }

    public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
    {
        base.Initialize(info, position, rotation, _state, shouldDisappear);
        patientList = new LinkedList<HospitalPatientAI>();
        pendingPatients = new List<SpawnedPatient>();
        SetSpots();
        //turnoffbed indicators
    }

    protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
    {
        takenSpotsCounter = 0;
        base.LoadFromString(save, timePassed);
        patientList = new LinkedList<HospitalPatientAI>();
        pendingPatients = new List<SpawnedPatient>();

        var str = save.Split(';');

        var patientStrs = str[1].Split('?').ToList();

        var tmpX = int.Parse(patientStrs[0], System.Globalization.CultureInfo.InvariantCulture);

        var bedTimes = patientStrs[1].Split('%');

        if (isoObj != null && isoObj.GetGameObject() != null && isoObj.GetGameObject().GetComponent<HospitalBedController>() != null)
        {
            for (int i = 0; i < bedTimes.Length; ++i)
            {
                float.TryParse(bedTimes[i], out float timeTNS);

                HospitalBedController controller = isoObj.GetGameObject().GetComponent<HospitalBedController>();
                timeTNS = Mathf.Max(0, timeTNS - timePassed.GetTimePassed());
                controller.Beds[i].TimeToNextSpawn = timeTNS;
                if (timeTNS > 0)
                    controller.StartTimerCoroutine(controller.Beds[i]);
            }
        }

        var bedStates = patientStrs[2].Split('%');

        if (isoObj != null && isoObj.GetGameObject() != null && isoObj.GetGameObject().GetComponent<HospitalBedController>() != null && patientStrs[2] != "")
        {
            for (int i = 0; i < bedStates.Length; ++i)
            {
                HospitalBedController.HospitalBed.BedStatus bedStatus = (HospitalBedController.HospitalBed.BedStatus)Enum.Parse(typeof(HospitalBedController.HospitalBed.BedStatus), bedStates[i]);
                if (bedStatus == HospitalBedController.HospitalBed.BedStatus.OccupiedBed || bedStatus == HospitalBedController.HospitalBed.BedStatus.WaitForDiagnose)
                {
                    bool reallyTaken = false;
                    if (patientStrs.Count > (3 + i))
                    {
                        for (int j = 3; j < patientStrs.Count; ++j)
                        {
                            var data = patientStrs[j].Split('!');
                            if (data.Length > 1 && int.Parse(data[1], System.Globalization.CultureInfo.InvariantCulture) == i)
                                reallyTaken = true;
                        }
                    }

                    if (!reallyTaken)
                    {
                        Debug.LogError("There was a bad status in bed and no patient so i validate it");
                        bedStatus = HospitalBedController.HospitalBed.BedStatus.WaitForPatient;
                    }
                }

                isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds[i]._BedStatus = bedStatus;

                if (GameState.Get().PatientsCount.PatientsCuredCountBed > 0 || isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds[i].LocalBedId == 0 || !TutorialController.Instance.tutorialEnabled)
                    isoObj.GetGameObject().GetComponent<HospitalBedController>().SetIndicator(isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds[i]);
            }
        }

        if (state == State.working)
        {
            var spawner = ReferenceHolder.GetHospital().HospitalSpawner;
            if (patientStrs.Count >= 4)
                for (int i = 03; i < patientStrs.Count; ++i)
                {
                    if (patientStrs[i].Length >= 1)
                    {
                        var infos = patientStrs[i].Split('^');
                        if (string.IsNullOrEmpty(infos[1]))
                            patientList.AddLast(spawner.SpawnPerson(this, infos[0]));
                        else
                            patientList.AddLast(spawner.LoadPerson(this, infos[0], infos[1]));
                    }
                }
        }

        SetSpots();
        for (int i = 0; i < takenBedSpots.Length; ++i)
        {
            takenBedSpots[i] = false;
        }

        if (patientStrs.Count >= 4)
        { 
            for (int i = 03; i < patientStrs.Count; ++i)
            {
                if (patientStrs[i].Length >= 1)
                {
                    var pat = patientStrs[i].Split('!').ToArray();
                    takenBedSpots[int.Parse(pat[1], System.Globalization.CultureInfo.InvariantCulture)] = true;
                    takenSpotsCounter++;
                }
            }
        }
        EmulateTime(timePassed);
    }

    protected override string SaveToString()
    {
        string bedTimes = "";
        string bedStates = "";
        if (isoObj != null && isoObj.GetGameObject() != null && isoObj.GetGameObject().GetComponent<HospitalBedController>() != null)
        {
            for (int i = 0; i < isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds.Count; ++i)
            {
                bedTimes += Checkers.CheckedAmount(HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(this, i).TimeToNextSpawn, -1.0f, float.MaxValue, Tag + " timeToNextSpawn ").ToString("n2");
                bedStates += Checkers.CheckedBedStatus(HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(this, i)._BedStatus, Tag).ToString();
                if (i < isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds.Count - 1)
                {
                    bedTimes += "%";
                    bedStates += "%";
                }
            }
        }

        Checkers.CheckedBedSave(bedTimes, bedStates, state, IsDummy);

        var tmp = "";
        if (patientList != null && patientList.Count > 0)
        {
            for (int i = 0; i < patientList.Count; ++i)
            {
                tmp += patientList.ElementAt(i).SaveToString();
                if (i < patientList.Count - 1)
                    tmp += "?";
            }
        }
        return base.SaveToString() + ";" /*+ shouldSpawn + "?"*/ + Checkers.CheckedAmount(takenBedSpots.Length, 0, takenBedSpots.Length, Tag + "unlockedSpotsCounter") + "?" + bedTimes + "?" + bedStates + "?" + (tmp.Length > 0 ? tmp : "");
    }

    public override void Notify(int id, object parameters)
    {
        base.Notify(id, parameters);
        if (id == (int)LoadNotification.EmulateTime)
            EmulateTime((TimePassedObject)parameters);
    }

    #region IsoDestroy
    public override void IsoDestroy()
    {
        ambulance.ReachedDestination -= lastSpawnEvent;
        lastSpawnEvent = null;

        //HospitalAreasMapController.HospitalMap.hospitalBedController.KillBedControllerCorountine();
        FireOnIsoDestroy();

        if (patientList != null)
        {
            var tempList = patientList.Select(x => x).ToList();
            foreach (var p in tempList)
                p.IsoDestroy();
        }

        patientList.Clear();

        if (pendingPatients != null && pendingPatients.Count > 0)
            pendingPatients.Clear();
        DestroyFloorMaterial();
        base.IsoDestroy();
    }

    private Action OnIsoDestroy;

    public void RegisterToOnIsoDestroy(Action action)
    {
        OnIsoDestroy = action;
    }

    private void FireOnIsoDestroy()
    {
        OnIsoDestroy?.Invoke();
    }
    #endregion

    protected override Vector2 GetHoverPosition()
    {
        Debug.Log(actualRotation);
        float xChange = 0;
        float yChange = 0;
        switch (actualRotation)
        {
            case Rotation.East:
                yChange = 1f;
                xChange = 1f;
                break;
            case Rotation.North:
                yChange = 1f;
                break;
            case Rotation.South:
                xChange = 1f;
                yChange = 1f;
                break;
            case Rotation.West:
                xChange = 1f;
                break;
        }
        return new Vector2(position.x + xChange, position.y + yChange);
    }

    protected override void AddToMap()
    {
        base.AddToMap();
        SetSpots();

        if (state == State.working)
        {
            if (isoObj != null && isoObj.GetGameObject() != null && isoObj.GetGameObject().GetComponent<HospitalBedController>() != null)
            {
                foreach (HospitalBedController.HospitalBed tmpBed in isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds)
                    tmpBed.room = this;
                HospitalAreasMapController.HospitalMap.hospitalBedController.AddBedsToController(isoObj.GetGameObject().GetComponent<HospitalBedController>().Beds, this);
            }

            if (patientList != null && patientList.Count > 0)
            {
                for (int i = 0; i < (patientList.Count < freeBedSpots.Count ? patientList.Count : freeBedSpots.Count); ++i)
                {
                    patientList.ElementAt(i).Notify((int)StateNotifications.OfficeMoved, obj == null);
                }
            }
        }

        if (isoObj != null)
            SetFloor(true);
        else
            SetFloor(false);

        //Timing.KillCoroutine(CheckBedIndicatorsCoroutine());
        //Timing.RunCoroutine(CheckBedIndicatorsCoroutine());

        ambulance = ReferenceHolder.GetHospital().Ambulance;

        TutorialController tc = TutorialController.Instance;
        if (tc.tutorialEnabled && tc.GetCurrentStepData().NecessaryCondition == Condition.SetTutorialArrow && Tag == "2xBedsRoom" && tc.GetCurrentStepData().CameraTargetRotatableObjectTag == "2xBedsRoom")
        {
            TutorialUIController.Instance.ShowIndictator(this);
        }
    }

    private void SetFloor(bool val)
    {
        if (floor == null)
            throw new IsoException("doctor floor doesn't exist!!");
        floor.gameObject.SetActive(val);
        ActualiseFloor();
    }

    private void ActualiseFloor()
    {
        if (floor != null)
        {
            //actualRotation
            Vector2 rot = new Vector2(((int)actualRotation + 1) % 2, ((int)actualRotation) % 2);

            if (((actualData.tilesX - 1) % 2) != (actualData.tilesY % 2))
                floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2) + rot.x * ((actualRotation == Rotation.North || actualRotation == Rotation.West) ? -0.5f : 0.5f), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2) + rot.y * ((actualRotation == Rotation.North || actualRotation == Rotation.West) ? -0.5f : 0.5f));
            else
                floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));

            if ((info.infos.dummyType == BuildDummyType.Hospital2xRoom) || (info.infos.dummyType == BuildDummyType.Hospital3xRoom) || ((actualData.tilesX) % 2) == (actualData.tilesY % 2))            
                floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));

            if (state == State.building)
            {
                if (((actualData.tilesX) % 2) == (actualData.tilesY % 2))
                {
                    floor.localScale = new Vector2(Mathf.Min(actualData.tilesX, actualData.tilesY), Mathf.Max(actualData.tilesY, actualData.tilesX));
                    floor.rotation = Quaternion.Euler(90, actualData.tilesX > actualData.tilesY ? 90 : 0, 0);
                    mat.material.mainTextureScale = floor.localScale;
                }
                else
                {
                    floor.rotation = Quaternion.Euler(90, 0, 0);
                    floor.localScale = new Vector2(actualData.tilesX, actualData.tilesY) - rot;
                    mat.material.mainTextureScale = floor.localScale;
                }
            }
            else
            {
                if ((info.infos.dummyType == BuildDummyType.Hospital2xRoom) || (info.infos.dummyType == BuildDummyType.Hospital3xRoom))
                {
                    floor.localScale = new Vector2(Mathf.Min(actualData.tilesX, actualData.tilesY), Mathf.Max(actualData.tilesY, actualData.tilesX));
                    floor.rotation = Quaternion.Euler(90, actualData.tilesX > actualData.tilesY ? 90 : 0, 0);
                    //mat.material.mainTextureScale = Vector2.one;
                }
                else
                {
                    floor.localScale = new Vector2(Mathf.Min(actualData.tilesX, actualData.tilesY), Mathf.Max(actualData.tilesY, actualData.tilesX) - 1);
                    floor.rotation = Quaternion.Euler(90, actualData.tilesX > actualData.tilesY ? 90 : 0, 0);
                    mat.material.mainTextureScale = Vector2.one;
                }
            }
            floor.transform.SetParent(transform);
        }
    }

    private void DestroyFloorMaterial()
    {
        EngineController.DestroyMaterial(floor.gameObject);
    }

    protected override void OnStateChange(State newState, State oldState)
    {
        base.OnStateChange(newState, oldState);
        if (floor != null)
        {
            DestroyFloorMaterial();
            GameObject.Destroy(flr);
        }
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        if (newState == State.building)
        {
            flr = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mat = flr.GetComponent<Renderer>();
            mat.material = HospitalAreasMapController.HospitalMap.wallDatabase.materialPrefab;
            mat.GetPropertyBlock(block);
            block.SetTexture("_MainTex", ResourcesHolder.GetHospital().UnderConstructionTile);
            mat.SetPropertyBlock(block);
            //mat.material.mainTextureScale = Vector2.one * 4;
        }
        if (newState == State.working || newState == State.fresh || newState == State.waitingForUser)
        {
            flr = GameObject.CreatePrimitive(PrimitiveType.Quad);
            mat = flr.GetComponent<Renderer>();
            mat.material = HospitalAreasMapController.HospitalMap.wallDatabase.materialPrefab;
            mat.GetPropertyBlock(block);
            var p = info.infos.dummyType;
            if (p == BuildDummyType.Hospital2xRoom)
                block.SetTexture("_MainTex", ResourcesHolder.GetHospital().Roomx2Floor);
            if (p == BuildDummyType.Hospital3xRoom)
                block.SetTexture("_MainTex", ResourcesHolder.GetHospital().Roomx3Floor);
            if (p == BuildDummyType.HospitalVipRoom)
                block.SetTexture("_MainTex", ResourcesHolder.GetHospital().VipRoomFloor);

            mat.SetPropertyBlock(block);
            //mat.material.mainTextureScale = Vector2.one;
        }

        floor = flr.transform;
        floor.localScale = Vector3.one * 4;
        floor.rotation = Quaternion.Euler(90, 0, 0);
    }

    public Vector2i ReacquireTakenSpot(int id)
    {
        return freeBedSpots[id] + position;
    }

    private void SetSpots()
    {
        freeBedSpots = new List<Vector2i>();
        int counter = 0;
        foreach (var value in actualData.spotsData)
        {
            switch ((SpotTypes)value.id)
            {
                case SpotTypes.Door:
                    if (roomEntrance.x == 0 && roomEntrance.z == 0)
                        roomEntrance = new Vector3(value.x, 0, value.y);
                    else
                        roomEntrance = new Vector3((roomEntrance.x + value.x) / 2, 0, (roomEntrance.z + value.y) / 2);
                    // Debug.LogError("Entrence " + roomEntrance.x + " " + roomEntrance.y);
                    break;
                case SpotTypes.HospitalBed:
                    freeBedSpots.Add(new Vector2i(value.x, value.y));
                    counter++;
                    break;
                default:
                    break;
            }
        }
        if (takenBedSpots == null || counter != takenBedSpots.Length)
            takenBedSpots = new bool[counter];
    }

    public Vector3 GetEntrancePosition()
    {
        return new Vector3(roomEntrance.x + position.x, 0, roomEntrance.z + position.y);
    }

    public Vector2i GetEntrancePosition(bool getEntrenceFirst)
    {
        return new Vector2i((int)roomEntrance.x + position.x, (int)roomEntrance.z + position.y);
    }

    private void SpawnPersonImmediateDelegate(HospitalAmbulance ambulance, int bedId) { }


    public void SpawnPersonImmediate(HospitalAmbulance ambulance, int localBedId)
    {
        ambulance.ReachedDestination -= lastSpawnEvent;

        if (!takenBedSpots[localBedId])
        {
            if (tutorialMode && patientList.Count > 0)
            {
                tutorialMode = false;
            }
            //Debug.LogError("IsFirstEver = " + isFirstEver);
            //Debug.LogError("CurrentTutorialStepTag = " + TutorialController.Instance.CurrentTutorialStepTag);
            patientList.AddLast(ReferenceHolder.GetHospital().HospitalSpawner.SpawnPersonGoToBedWithID(
                ResourcesHolder.GetHospital().SpawnPoints[GameState.RandomNumber(0, ResourcesHolder.GetHospital().SpawnPoints.Count)],
                this, localBedId, false, tutorialMode));
            //Debug.Log("Spawned person at " + Time.realtimeSinceStartup);
            takenBedSpots[localBedId] = true;
            if (TutorialController.Instance.tutorialEnabled)
            {
                if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.bed_patient_arrived ||
                    TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_open_patient_card)
                {
                    NotificationCenter.Instance.FirstEmergencyPatientSpawned.Invoke(new FirstEmergencyPatientSpawnedEventArgs(patientList.Last.Value.gameObject));
                }
            }
        }
        lastSpawnEvent = null;
    }

    public void SpawnPerson(int bedId, float spawnTime = 0)
    {

        if (!takenBedSpots[bedId] && !AnyPendingpatientIsOnSameSpot(bedId))
        {
            SpawnedPatient spawnedPatient = new SpawnedPatient();
            spawnedPatient.timeLeft = maxSpawnTime = spawnTime;
            spawnedPatient.bedID = bedId;
            if (pendingPatients.Count == 0)
                spawnedPatient.timeLeft = 0f;
            pendingPatients.Add(spawnedPatient);
        }

        List<SpawnedPatient> toDelete = new List<SpawnedPatient>();

        if (pendingPatients.Count > 0 && !ambulance.IsBlockedByTutorial)
        {
            foreach (SpawnedPatient patient in pendingPatients.ToArray())
            {
                patient.timeLeft -= Time.deltaTime;

                if (patient.timeLeft <= 0.0f)
                {
                    if (ambulance.IsWaitingOut && lastSpawnEvent == null)
                    {
                        ambulance.DriveIn();

                        if (!takenBedSpots[bedId])
                        {
                            lastSpawnEvent = SpawnPersonImmediateDelegate => { SpawnPersonImmediate(ambulance, patient.bedID); };
                            ambulance.ReachedDestination += lastSpawnEvent;
                            toDelete.Add(patient);
                            pendingPatients.Remove(patient);
                        }
                    }
                    else if (ambulance.IsWaitingIn && ambulance.CanDriveOut)
                        ambulance.DriveOut();
                }
            }
        }
    }

    public bool AnyPendingpatientIsOnSameSpot(int spot)
    {
        foreach (SpawnedPatient pendingPatient in pendingPatients)
        {
            if (pendingPatient.bedID == spot) return true;
        }

        return false;
    }

    public bool RemovePendingpatientOnSameSpot(int spot)
    {
        if (pendingPatients != null && pendingPatients.Count > 0)
        {
            foreach (SpawnedPatient pendingPatient in pendingPatients.ToArray())
            {
                if (pendingPatient.bedID == spot)
                {
                    pendingPatients.Remove(pendingPatient);
                    return true;
                }
            }
        }

        return false;
    }

    public HospitalPatientAI SpawnPersonInBed()
    {
        if (patientList.Count >= freeBedSpots.Count)
            return null;
        if (((HospitalRoomInfo)info.infos).IsVIPRoom)
        {
            HospitalPatientAI spawnedPatientAi = ReferenceHolder.GetHospital().HospitalSpawner.SpawnPersonToBed(freeBedSpots.First(), this, 0, true, true);
            patientList.AddLast(spawnedPatientAi);
            return spawnedPatientAi;
        }
        else
        {
            HospitalPatientAI spawnedPatientAi = ReferenceHolder.GetHospital().HospitalSpawner.SpawnPersonToBed(freeBedSpots.First(), this, 0, false, true);
            patientList.AddLast(spawnedPatientAi);
            return spawnedPatientAi;
        }
    }

    public HospitalPatientAI SpawnPersonInBedOnID(int id)
    {
        if (patientList.Count >= freeBedSpots.Count)
            return null;
        if (((HospitalRoomInfo)info.infos).IsVIPRoom)
        {
            HospitalPatientAI spawnedPatientAi = ReferenceHolder.GetHospital().HospitalSpawner.SpawnPersonToBed(freeBedSpots[id], this, id, true, true);
            patientList.AddLast(spawnedPatientAi);
            return spawnedPatientAi;
        }
        else
        {
            HospitalPatientAI spawnedPatientAi = ReferenceHolder.GetHospital().HospitalSpawner.SpawnPersonToBed(freeBedSpots[id], this, id, false, true);
            patientList.AddLast(spawnedPatientAi);
            return spawnedPatientAi;
        }
    }

    public bool isAmbulanceWaiting()
    {
        return ambulance.IsWaitingIn || ambulance.IsWaitingOut || ambulance.CanDriveOut;
    }

    public bool isAmbulanceWaitingInside()
    {
        return ambulance.IsWaitingIn;
    }

    public bool isAmbulanceWaitingOutside()
    {
        return ambulance.IsWaitingOut;
    }

    public bool isAmbulanceCanDrive()
    {
        return ambulance.CanDriveOut;
    }

    public void ReturnTakenSpot(int id)
    {
        if (takenBedSpots != null && takenBedSpots.Length > id && id >= 0)
        {
            takenBedSpots[id] = false;
            --takenSpotsCounter;
        }
    }

    public bool HasAnyPatientWithPlague()
    {
        if (patientList != null && patientList.Count > 0)
        {
            foreach (HospitalPatientAI pat in patientList)
            {
                if (pat != null && pat.GetComponent<HospitalCharacterInfo>() != null)
                {
                    if (pat.GetComponent<HospitalCharacterInfo>().HasBacteria)
                        return true;
                }
            }

        }
        return false;
    }

    public bool HasOtherPatientIsPlague(HospitalCharacterInfo info)
    {
        if (patientList != null && patientList.Count > 0)
        {
            foreach (HospitalPatientAI pat in patientList)
            {
                if (pat != null && pat.GetComponent<HospitalCharacterInfo>() != null)
                {
                    if (pat.GetComponent<HospitalCharacterInfo>() != info && pat.GetComponent<HospitalCharacterInfo>().HasBacteria)
                        return true;
                }
            }
        }
        return false;
    }

    public HospitalCharacterInfo GetOtherPatient(HospitalCharacterInfo info)
    {
        if (patientList != null && patientList.Count > 0)
        {
            foreach (HospitalPatientAI pat in patientList)
            {
                if (pat != null && pat.GetComponent<HospitalCharacterInfo>() != null)
                {
                    if (pat.GetComponent<HospitalCharacterInfo>() != info)
                        return pat.GetComponent<HospitalCharacterInfo>();
                }
            }
        }
        return null;
    }

    protected override void OnClickWorking()
    {
        if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
        {
            Debug.Log("Click won't work because drawer is visibile");
            return;
        }

        base.OnClickWorking();
        Vector3 click = ReferenceHolder.Get().engine.MainCamera.RayCast(Input.mousePosition) - transform.position;

        int id = -1;

        if (!RotatableObject.visitingMode)
        {
            HospitalBedController.HospitalBed closestBed = HospitalAreasMapController.HospitalMap.hospitalBedController.GetClosestBed(this, click, out id);
            Debug.LogFormat("<color=red>Closest bed: {0}, TutorialMode: {1}, Closest bed patient: {2}</color>", closestBed, tutorialMode, closestBed.Patient);
            // Ignore click on empty bed in tutorial mode
            if (tutorialMode && closestBed.Patient == null)
                return;

            if (Game.Instance.gameState().GetHospitalLevel() < 3)
            {
                MessageController.instance.ShowMessage(22);
            }
            else if ((!TutorialSystem.TutorialController.IsTutorialStepCompleted(StepTag.patient_card_open) && closestBed == null) ||
                     (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patient_card_open && closestBed.Patient == null))
            {
                //this is for time when first patient is on his way to the bed
                MessageController.instance.ShowMessage(16);
            }
            else
            {
                if (closestBed.Patient != null && id < HospitalAreasMapController.HospitalMap.hospitalBedController.Beds.Count)
                {
                    //Debug.LogWarning("Open for: " + closestBed.Patient.name + " " + id);
                    UIController.getHospital.PatientCard.Open(((BasePatientAI)closestBed.Patient).GetComponent<HospitalCharacterInfo>(), id);
                }
                else if (id >= 0)            
                    UIController.getHospital.PatientCard.Open(null, id);
            }
        }
        else
        {
            HospitalBedController.HospitalBed closestBed = HospitalAreasMapController.HospitalMap.hospitalBedController.GetClosestBed(this, click, out id, true);

            if (closestBed != null)
            {
                var patients = ReferenceHolder.GetHospital().treatmentRoomHelpProviderController.GetPatientsWithRequiredHelp();

                if (closestBed.Patient != null)
                {
                    for (int i = 0; i < patients.Count; ++i)
                    {
                        if (patients[i].ID == ((BasePatientAI)closestBed.Patient).GetComponent<HospitalCharacterInfo>().ID)
                        {
                            HospitalDataHolder.Instance.Emergency.OpenTreatmentDonatePopup(patients, i);
                            return;
                        }
                    }
                }
            }
        }
    }

    public override void IsoUpdate()
    {
        base.IsoUpdate();
    }

    public bool CanSpawnpatientInRoom()
    {
        if (state == State.working && freeBedSpots.Count > patientList.Count && takenSpotsCounter < actualData.spotsData.Length && !ambulance.IsBlockedByTutorial)
        {
            return true;
        }

        return false;
    }

    protected override void OnClickWaitForUser()
    {
        if (!isUnwraping)
        {
            Timing.RunCoroutine(DelayedUnwrap());
        }
        //UnlockBeds();
    }

    public enum SpotTypes
    {
        Door,
        CorridorChair,
        InteriorChair,
        DoctorChair,
        Machine,
        FileCase,
        DiagnosticConsole,
        HospitalBed
    }

    public GameObject ReturnBed(int spotID)
    {

        if (isoObj == null || isoObj.GetGameObject() == null)
            return null;

        if (isoObj.GetGameObject().GetComponent<HospitalBedController>() != null)
            return HospitalAreasMapController.HospitalMap.hospitalBedController.GetBedWithIDFromRoom(this, spotID).Bed;

        return null;
    }

    public void CoverBed(int spotID, HospitalPatientAI patient)
    {
        RemovePendingpatientOnSameSpot(spotID);
        HospitalAreasMapController.HospitalMap.hospitalBedController.SetPatientInBed(this, spotID, patient);
    }

    public void UnCoverBed(int spotID, HospitalPatientAI patient)
    {
        HospitalAreasMapController.HospitalMap.hospitalBedController.UnCoverBed(this, spotID, patient);
    }

    public void FreeBed(int spotID)
    {
        takenBedSpots[spotID] = false;
        HospitalAreasMapController.HospitalMap.hospitalBedController.FreePatientFromBed(this, spotID);
    }

    public void DischargeBed(int spotID)
    {
        takenBedSpots[spotID] = false;
        HospitalAreasMapController.HospitalMap.hospitalBedController.DischargePatientFromBed(this, spotID);
    }

    IEnumerator<float> DelayedUnwrap()
    {
        isUnwraping = true;

        map.GetObject(new Vector2i(position.x, position.y)).GetComponent<Animator>().SetTrigger("Unwrap");
        yield return Timing.WaitForSeconds(0.5f);
        base.OnClickWaitForUser();

        int expReward = ((ShopRoomInfo)info.infos).buildXPReward;

        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
        GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false, Tag);
        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
        {
            GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
        });
        int amountOfRoomOnMap = HospitalAreasMapController.HospitalMap.GetRotatableObjectCounter(Tag);
        if (amountOfRoomOnMap == 3)
        {
            HospitalAreasMapController.HospitalMap.emergency.SetIndicatorVisible();
        }
        isUnwraping = false;
    }

}

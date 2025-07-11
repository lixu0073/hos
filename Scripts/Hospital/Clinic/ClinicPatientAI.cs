using UnityEngine;
using System.Collections.Generic;
using IsoEngine;
using System.Linq;
using System;
using MovementEffects;
using System.Globalization;

namespace Hospital
{
    public class ClinicPatientAI : BasePatientAI
    {
        public static List<ClinicPatientAI> Patients; //pewnie do zmiany! (skopiowane z HospitalPatientAI)      
        static ClinicPatientAI()
        {
            Patients = new List<ClinicPatientAI>();
        }

        public void OnDestroy()
        {
            Patients.Remove(this);
        }

        public static void ResetAllClinicPatients()
        {
            if (Patients != null && Patients.Count > 0)
            {
                for (int i = 0; i < Patients.Count; ++i)
                {
                    Patients[i].IsoDestroy();
                }

                Patients.Clear();
            }
        }

        StateManager AI;
        [HideInInspector]
        public DoctorRoom room;

        [HideInInspector]
        public int waitingSpotID = -1;

        public bool isGoHome = false;
        public bool isReceptionCheckIn = false;

        [HideInInspector]
        public Vector2i startingPoint;

        public int queueReceptionSpotID = -1;

        //[HideInInspector]
        public bool isFirstEver = false;

        [SerializeField] HospitalTutorialStep step;

        public void SetAIState(IState state)
        {
            this.AI.State = state;
        }

        public static void UpdatePatientsWithoutRoom(DoctorRoom newRoom)
        {
            for (int i = 0; i < Patients.Count; ++i)
            {
                if (Patients[i] != null && !Patients[i].isGoHome && Patients[i].room == null && Patients[i].GetComponent<ClinicCharacterInfo>().clinicDisease.Doctor == (DoctorRoomInfo)(newRoom.GetRoomInfo()))   //FIXME: && this is the room he has been waiting for
                {
                    Patients[i].room = newRoom;
                    newRoom.AddPatientToQueue(Patients[i]);
                    //Debug.Log("UpdatePatientsWithoutRoom Patient going to room now");
                    Patients[i].AI.State = new GoingToDoctorRoomState(Patients[i], 0, Patients[i].room.ReacquireTakenSpot(0));        //?? not sure about spots, logically first spot will be free when it's first patient
                }
            }
        }

        public static void SendFirstPatientToReception()
        {
            Debug.Log("First patient moves to Reception Here");
            if (Patients[0] != null)
                Patients[0].AI.State = new GoToReceptionState(Patients[0], false);
            else
                Debug.LogError("First patient does not exist");
        }

        public static void SpawnPatientOnLevelUp()
        {
            //Debug.LogWarning("Spawning new clinic patient on Level Up");
            switch (Game.Instance.gameState().GetHospitalLevel())
            {
                case 4:
                    ClinicPatientAI c4 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(1));
                    c4.AI.State = new GoToReceptionState(c4, HospitalAreasMapController.HospitalMap.FindRotatableObject("YellowDoc"));
                    break;
                case 5:
                    ClinicPatientAI c5 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(2));
                    c5.AI.State = new GoToReceptionState(c5, HospitalAreasMapController.HospitalMap.FindRotatableObject("GreenDoc"));
                    break;
                case 6:
                    ClinicPatientAI c6 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(3));
                    c6.AI.State = new GoToReceptionState(c6, HospitalAreasMapController.HospitalMap.FindRotatableObject("WhiteDoc"));
                    break;
                case 7:
                    ClinicPatientAI c7 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(4));
                    c7.AI.State = new GoToReceptionState(c7, HospitalAreasMapController.HospitalMap.FindRotatableObject("SunnyDoc"));
                    break;
                case 9:
                    ClinicPatientAI c9 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(5));
                    c9.AI.State = new GoToReceptionState(c9, HospitalAreasMapController.HospitalMap.FindRotatableObject("RedDoc"));
                    break;
                case 10:
                    ClinicPatientAI c10 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(6));
                    c10.AI.State = new GoToReceptionState(c10, HospitalAreasMapController.HospitalMap.FindRotatableObject("PinkDoc"));
                    break;
                case 16:
                    ClinicPatientAI c16 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(7));
                    c16.AI.State = new GoToReceptionState(c16, HospitalAreasMapController.HospitalMap.FindRotatableObject("SkyDoc"));
                    break;
                case 32:
                    ClinicPatientAI c32 = SpawnPatientForNewDoctor(new Vector2i(22, 42), ResourcesHolder.GetHospital().GetClinicDisease(8));
                    c32.AI.State = new GoToReceptionState(c32, HospitalAreasMapController.HospitalMap.FindRotatableObject("PurpleDoc"));
                    break;
                default:
                    //Debug.Log("No new doctor office on this level. First patient won't be spawned.");
                    break;
            }
        }

        /// <summary>
        /// Remember to Set patient room when you will want character to start walking.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static ClinicPatientAI SpawnPatientForNewDoctor(Vector2i position, ClinicDiseaseDatabaseEntry disease, bool isFirstEver = false)
        {            
            var p = ReferenceHolder.GetHospital().ClinicAI.SpawnPatient(null, "A" + position.ToString(), disease, isFirstEver) as ClinicPatientAI;
            try
            {
                p.anim.Play(AnimHash.Headache_2, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            Patients.Add(p);
            return p;
        }

        public void Initialize(Vector2i pos, DoctorRoom room)
        {
            this.room = room;
            base.Initialize(pos);
            AI = new StateManager();
            GoToReception(room != null);
        }

        public override void Initialize(RotatableObject room, string info)
        {
            //Debug.LogWarning("ClinicPatientAI Initialize(RotatableObject room, string info)");
            var strs = info.Substring(1).Split('!');
            base.Initialize(Vector2i.Parse(strs[0]));
            this.room = room as DoctorRoom;
            AI = new StateManager();
            if (strs.Length > 1)
            {
                int spot;
                switch (strs[1])
                {
                    case "GTR":
                        var state = (int)((ReceptionState)Enum.Parse(typeof(ReceptionState), strs[2], true));//int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        var queueID = int.Parse(strs[3], System.Globalization.CultureInfo.InvariantCulture);

                        if (room != null)
                            GoToReception(true, state, queueID);
                        else
                            GoToReception(false, state, queueID);
                        break;
                    case "WFCS":
                        AI.State = new WaitForCheckInState(this);
                        break;
                    case "RCIS":
                        if (room != null)
                            AI.State = new ReceptionCheckInState(this, true, float.Parse(strs[2], CultureInfo.InvariantCulture));
                        else
                            AI.State = new ReceptionCheckInState(this, false, float.Parse(strs[2], CultureInfo.InvariantCulture));
                        break;
                    case "GTDR":
                        spot = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        AI.State = new GoingToDoctorRoomState(this, spot, this.room.ReacquireTakenSpot(spot));
                        break;
                    case "SOWS":
                        spot = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        var pos = this.room.ReacquireTakenSpot(spot);
                        TeleportTo(pos);
                        AI.State = new SittingOnRoomWaitingSpot(this, spot);
                        break;
                    case "WIRS":
                        spot = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        AI.State = new WaitingInReceptionState(this, spot, HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotOnChairs(spot));
                        break;
                    case "WAS":
                        AI.State = new WanderingAroundState(this, true, true);
                        break;
                    case "WARS":
                        AI.State = new WanderingAroundReceptionState(this, true, true);
                        break;
                    case "HS":
                        //room.PatientReady(this,true);
                        AI.State = new HealingState(this);
                        break;
                    case "WIDRS":
                        AI.State = new WalkingIntoDoctorRoomState(this);
                        break;
                    case "GHS":
                        isGoHome = true;
                        AI.State = new GoingHomeState(this);
                        break;
                    case "GNFH":  // great name for hte hospital stae
                        AI.State = new GreatNameForHospitalState(this);
                        break;
                    case "NULL":
                        AI.State = null;
                        break;
                    default:
                        // var state = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        //  AI.State = new GoToReceptionState(this,true,state);
                        AI.State = new GoToReceptionState(this, true);
                        break;
                }
            }
        }

        protected override void Update()
        {
            base.Update();
            AI.Update();
        }

        public void GoToReception(bool isDoctorBuilt = true, int stateFromSave = 0, int queueSpot = -1)
        {
            if (!isGoHome)
            {
                if (AI.State != null)
                    AI.State.Notify((int)StateNotifications.AbortState, null);
                AI.State = new GoToReceptionState(this, isDoctorBuilt, stateFromSave, queueSpot);
            }
        }

        public void GoHome(bool withHurray = true)
        {
            isGoHome = true;
            if (AI.State != null)
                AI.State.Notify((int)StateNotifications.AbortState, null);

            if (!isFirstEver)
                AI.State = new GoingHomeState(this, true, withHurray);
            else
                AI.State = new GreatNameForHospitalState(this);
        }

        public override string SaveToString()
        {
            var z = GetComponent<ClinicCharacterInfo>();
            if (AI.State != null)
                return "A" + Checkers.CheckedPosition(position, z.name).ToString() + "!" + Checkers.CheckedClinicAIState(AI.State.SaveToString()) + "^" + Checkers.CheckedPatientBIO(z.personalBIO, z.name) + "^" + Checkers.CheckedAmount(z.clinicDisease.id, 0, 8, "clinicDisease").ToString() + "^" + Checkers.CheckedBool(isFirstEver);

            return "A" + Checkers.CheckedPosition(position, z.name).ToString() + "!" + "NULL" + "^" + Checkers.CheckedPatientBIO(z.personalBIO, z.name) + "^" + Checkers.CheckedAmount(z.clinicDisease.id, 0, 8, "clinicDisease").ToString() + "^" + Checkers.CheckedBool(isFirstEver);
        }

        protected override void ResetCharacterInfo()
        {
            ReferenceHolder.Get().engine.RemoveTask(this);
            AI.State.OnExit();

            base.ResetCharacterInfo();
        }

        public string GetState()
        {
            return AI.State.SaveToString();
        }

        public bool IsStandNearSignState()
        {
            if (AI.State is GreatNameForHospitalState)
                return (AI.State as GreatNameForHospitalState).IsWaitingNearHospitalBilboard();

            return false;
        }

        public void StartHealingAnimation()
        {
            base.SetHealingAnimation(room, 0);
        }

        public void StopHealingAnimation()
        {
            transform.position = new Vector3(room.GetMachineSpot().x, 0, room.GetMachineSpot().y);
            position = room.GetMachineSpot();
        }

        public void SetChairsOffestPosition()
        {
            if (isFront)
            {
                if (isRight)
                    transform.position += BaseResourcesHolder.ChairOffsetEast;
                else
                    transform.position += BaseResourcesHolder.ChairOffsetSouth;
            }
            else
            {
                if (isRight)
                    transform.position += BaseResourcesHolder.ChairOffsetNorth;
                else
                    transform.position += BaseResourcesHolder.ChairOffsetWest;
            }
        }

        public void StartSittingAnimation(RotatableObject room)
        {
            base.StartDoctorWaitingSitAnimation(room);
        }

        public void SittingAnim()
        {
            try
            {
                anim.Play(AnimHash.Sit_Idle, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        protected override void ReachedDestination()
        {
            base.ReachedDestination();
            if (AI.State != null)
                AI.State.Notify((int)StateNotifications.FinishedMoving, null);
        }

        public override void Notify(int id, object parameters)
        {
            if (id == (int)StateNotifications.ForceRemoveFromDoctorQueue && waitingSpotID != -1)
            {
                room.ReturnTakenSpot(waitingSpotID);
                waitingSpotID = -1;
                return;
            }

            if (isGoHome)
                return;

            switch ((StateNotifications)id)
            {
                case StateNotifications.GoHome:
                    GoHome();
                    return;
                case StateNotifications.GoToDoctorWaitingSpot:
                    if (waitingSpotID == -1)
                    {
                        waitingSpotID = (int)parameters;
                        room.GetWaitingSpotByID(waitingSpotID, out Vector2i pos);

                        if (AI.State == null || (AI.State != null && AI.State.GetType() != typeof(GoingToDoctorRoomState)))
                            AI.State = new GoingToDoctorRoomState(this, waitingSpotID, pos);
                        return;
                    }
                    break;
                default:
                    break;
            }

            if (AI.State != null)
                AI.State.Notify(id, parameters);
        }

        #region states
        public class GoToReceptionState : BaseState
        {
            Vector2i receptionSpot;
            Vector2i queueSpot;
            int queueSpotID = -1;
            int saveQueueSpotID = -1;
            ReceptionState state = ReceptionState.Default;

            bool IsDoctorBuilt;
            bool isFinishMovement = true;
            bool tutorialCharacter = false;
            float TimeLeft;
            int stateFromSave = 0;

            public GoToReceptionState(ClinicPatientAI parent, bool isDoctorBuilt = true,
                int stateFromSave = 0, int queueSpotID = -1, bool tutorialCharacter = false) : base(parent)
            {
                IsDoctorBuilt = isDoctorBuilt;
                this.stateFromSave = stateFromSave;
                this.queueSpotID = queueSpotID;
                this.saveQueueSpotID = queueSpotID;
                this.tutorialCharacter = tutorialCharacter;
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void OnEnter()
            {
                base.OnEnter();

                switch (stateFromSave)
                {
                    case 1:
                        state = ReceptionState.WaitnQueue;
                        Vector2i poqueuePos = HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotPosInQueueFromID(queueSpotID);

                        if (poqueuePos != Vector2i.zero)
                            parent.TeleportTo(poqueuePos);

                        isFinishMovement = true;
                        TimeLeft = -1;

                        if (queueSpotID == 0)
                        {
                            HospitalAreasMapController.HospitalMap.reception.FreeReceptionSpot();
                            state = ReceptionState.CheckInReception;

                            if (queueSpotID > -1)
                            {
                                HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(queueSpotID);
                                parent.queueReceptionSpotID = -1;
                            }

                            if (parent.room != null)
                                parent.AI.State = new ReceptionCheckInState(parent, IsDoctorBuilt);
                            else
                                parent.AI.State = new ReceptionCheckInState(parent, false);
                        }
                        break;
                    case 2:
                        state = ReceptionState.CheckInReception;
                        ActionIfNotInQueue();
                        break;
                    default:
                        state = ReceptionState.Default;
                        ActionIfNotInQueue();
                        break;
                }
            }

            public void ActionIfNotInQueue()
            {
                if (!IsDoctorBuilt)
                    parent.GoTo(new Vector2i(22, 43), PathType.GoHealingPath);
                else
                {
                    if (HospitalAreasMapController.HospitalMap.reception.IsWaitingQueueFull())
                        parent.GoTo(new Vector2i(parent.startingPoint.x, parent.startingPoint.y + 1), PathType.GoHealingPath);
                    else
                        parent.GoTo(new Vector2i(23, 42), PathType.GoHealingPath);
                }

                isFinishMovement = false;
            }

            public void PlayAnim(int hash)
            {
                try
                { 
                    parent.anim.Play(hash, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                if (id == (int)StateNotifications.FinishedMoving)
                {
                    isFinishMovement = true;

                    if (tutorialCharacter)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Headache_2, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else
                    {
                        try
                        { 
                            parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }

                    System.Random tmp = new System.Random(DateTime.Now.Second);

                    if (state == ReceptionState.CheckInReception) // Go to Reception and Check In
                    {
                        if (queueSpotID > -1)
                        {
                            HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(queueSpotID);
                            parent.queueReceptionSpotID = -1;
                        }

                        if (parent.room != null)
                            parent.AI.State = new ReceptionCheckInState(parent, IsDoctorBuilt);
                        else
                            parent.AI.State = new ReceptionCheckInState(parent, false);

                        TimeLeft = (float)tmp.NextDouble() * 2;
                        return;
                    }

                    TimeLeft = -1;
                    state = ReceptionState.WaitnQueue;
                }

                if ((int)StateNotifications.OfficeReadyToWalkIn == id)
                {
                    if (queueSpotID > -1)
                    {
                        HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(queueSpotID);
                        parent.queueReceptionSpotID = -1;
                    }
                    parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                if (isFinishMovement)
                {
                    TimeLeft -= Time.deltaTime;

                    if (TimeLeft < 0)
                    {
                        if (state == ReceptionState.WaitnQueue)
                        {
                            if ((!HospitalAreasMapController.HospitalMap.reception.isBusy) && queueSpotID == 0) // If reception isn't busy and you're first in queue
                            {
                                receptionSpot = HospitalAreasMapController.HospitalMap.reception.GetReceptionSpot();
                                parent.isReceptionCheckIn = true;
                                parent.GoTo(receptionSpot, PathType.GoHealingPath);
                                state = ReceptionState.CheckInReception;
                                isFinishMovement = false;
                                return;
                            }
                            if (queueSpotID > 0) // if you are in the queue but not first
                            {
                                Vector2i newQueueSpot = Vector2i.zero;

                                if (HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotInQueue(queueSpotID - 1, out newQueueSpot) == true)
                                {
                                    if (newQueueSpot != Vector2i.zero)
                                    {
                                        // Debug.LogWarning("I'm in Spot " + queueSpotID + " and next in queue " + (queueSpotID - 1).ToString() + "is empty so i go there");
                                        parent.GoTo(newQueueSpot, PathType.GoHealingPath);
                                        HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(queueSpotID);
                                        queueSpotID--;
                                        saveQueueSpotID = queueSpotID;
                                        parent.queueReceptionSpotID = queueSpotID;
                                        queueSpot = newQueueSpot;
                                        state = ReceptionState.WaitnQueue;
                                        isFinishMovement = false;
                                        return;
                                    }
                                }
                            }
                            else if (queueSpotID == -1)// if you are not in queue then gest spot in queue
                            {
                                Vector2i newQueueSpot = Vector2i.zero;
                                if ((queueSpotID = HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotInQueue(out newQueueSpot)) >= 0)
                                {
                                    if ((newQueueSpot != Vector2i.zero) && (queueSpotID != -1))
                                    {
                                        if (queueSpotID == 0)
                                        {
                                            receptionSpot = HospitalAreasMapController.HospitalMap.reception.GetReceptionSpot();
                                            parent.isReceptionCheckIn = true;
                                            parent.GoTo(receptionSpot, PathType.GoHealingPath);
                                            state = ReceptionState.CheckInReception;
                                            isFinishMovement = false;
                                            return;
                                        }

                                        queueSpot = newQueueSpot;
                                        saveQueueSpotID = queueSpotID;
                                        parent.queueReceptionSpotID = queueSpotID;
                                        parent.GoTo(newQueueSpot, PathType.GoHealingPath);
                                        state = ReceptionState.WaitnQueue;
                                    }
                                }
                            }
                        }
                    }
                }

            }

            public override void OnExit()
            {
                base.OnExit();
                if (queueSpotID > -1)
                {
                    HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(queueSpotID);
                    parent.queueReceptionSpotID = -1;
                    queueSpotID = -1;
                }
            }

            public override string SaveToString()
            {
                return "GTR!" + state.ToString() + "!" + saveQueueSpotID;
            }
        }

        public class WaitForCheckInState : BaseState
        {
            //bool isDoctorBuilt;

            public WaitForCheckInState(ClinicPatientAI parent) : base(parent) { }

            public override void OnEnter()
            {
                try
                {
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }

                base.OnEnter();
            }

            public override void OnUpdate() { }

            public override string SaveToString()
            {
                return "WFCS";
            }

            public override void OnExit()
            {
                base.OnExit();
            }
        }

        public class ReceptionCheckInState : BaseState
        {
            float TimeLeft;
            bool IsDoctorBuilt = false;

            public ReceptionCheckInState(ClinicPatientAI parent, bool isDoctorBuilt = true, float timeLeft = 3.0f) : base(parent)
            {
                TimeLeft = timeLeft;
                IsDoctorBuilt = isDoctorBuilt;
                if (!isDoctorBuilt)
                    parent.GetComponent<ClinicCharacterInfo>().ShowClinicSicknessCloud();

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void OnEnter()
            {
                base.OnEnter();
                try
                {
                    parent.SetAnimationDirection(false, false);
                    parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.OfficeReadyToWalkIn:
                        parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        break;
                    default:
                        break;
                }
            }

            public override void OnUpdate()
            {
                int positionID;
                Vector2i pos;
                base.OnUpdate();
                TimeLeft -= Time.deltaTime;

                if ((TimeLeft < 0) && (!parent.isGoHome))
                {
                    if (!IsDoctorBuilt)
                    {
                        positionID = ((HospitalAreasMapController)(ReferenceHolder.Get().engine.Map)).reception.GetWaitingSpotOnChairs(out pos);
                        if (positionID >= 0)
                            parent.AI.State = new WaitingInReceptionState(parent, positionID, pos);
                        else
                            parent.AI.State = new WanderingAroundReceptionState(parent);
                    }
                    else if (parent.waitingSpotID >= 0) // GO TO DOCTOR ROOM IF GET WAITINGSPOT CHAIR FROM NOTIFICATION
                    {
                        Vector2i waitingPos = Vector2i.zero;
                        parent.room.GetWaitingSpotByID(parent.waitingSpotID, out waitingPos);
                        parent.AI.State = new GoingToDoctorRoomState(parent, parent.waitingSpotID, waitingPos);
                    }
                    else
                        parent.AI.State = new WanderingAroundState(parent, true);
                }
            }

            public override void OnExit()
            {
                base.OnExit();
                HospitalAreasMapController.HospitalMap.reception.FreeReceptionSpot();
            }

            public override string SaveToString()
            {
                return "RCIS!" + TimeLeft.ToString();
            }
        }

        public class GoingToDoctorRoomState : BaseState
        {
            int spotID;
            bool shouldReturn = true;

            public GoingToDoctorRoomState(ClinicPatientAI parent, int spotID, Vector2i pos) : base(parent)
            {
                shouldReturn = true;
                if (parent.room.shouldWork)
                    parent.GoTo(pos, PathType.GoHealingPath);

                this.spotID = spotID;
                if (parent.GetComponent<ClinicCharacterInfo>() != null)
                    parent.GetComponent<ClinicCharacterInfo>().HideClinicSicknessCloud();

                if (parent.queueReceptionSpotID > -1)
                {
                    HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(parent.queueReceptionSpotID);
                    parent.queueReceptionSpotID = -1;
                }
            }

            public override void OnEnter()
            {
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            shouldReturn = false;
                            parent.AI.State = new SittingOnRoomWaitingSpot(parent, spotID);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        try
                        {
                            parent.StopMovement();
                            parent.abortPath();
                            parent.SetAnimationDirection();
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeAnchored:
                        if (!(bool)parameters)
                            return;

                        Vector2i pos = parent.room.ReacquireTakenSpot(spotID);
                        parent.GoTo(pos, PathType.GoHealingPath);
                        break;
                    case StateNotifications.OfficeReadyToWalkIn:
                        shouldReturn = true;
                        parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        break;
                    default:
                        break;
                }
            }

            public override void OnExit()
            {
                if (shouldReturn)
                {
                    if (parent.waitingSpotID != -1)
                    {
                        parent.room.ReturnTakenSpot(parent.waitingSpotID);
                        parent.waitingSpotID = -1;
                    }
                }
                base.OnExit();
            }

            public override string SaveToString()
            {
                return "GTDR!" + spotID.ToString();
            }
        }

        public class SittingOnRoomWaitingSpot : BaseState
        {
            int spotID;
            float time = 0;

            public SittingOnRoomWaitingSpot(ClinicPatientAI parent, int spotID) : base(parent)
            {
                parent.transform.GetChild(1).gameObject.SetActive(false);
                parent.waitingSpotID = spotID;
                this.spotID = spotID;

                if (parent.queueReceptionSpotID > -1)
                {
                    HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotInQueue(parent.queueReceptionSpotID);
                    parent.queueReceptionSpotID = -1;
                }

                parent.abortPath();
                parent.TeleportTo(parent.room.ReacquireTakenSpot(parent.waitingSpotID));

                Vector3 movePos;
                var dir = parent.room.GetSpotDirection(parent.waitingSpotID);

                movePos.x = parent.transform.position.x + dir.x;
                movePos.y = 0;
                movePos.z = parent.transform.position.z + dir.y;
                parent.transform.position = movePos;
                // parent.SetChairsOffestPosition();

                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    parent.StartSittingAnimation(parent.room);
                });
            }

            public override void OnEnter()
            {
                base.OnEnter();

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.aDocQueue);
            }

            public override void OnUpdate()
            {
                time += Time.deltaTime;

                if (time > 4f)
                {
                    parent.SetRandomSitAnimation();
                    time = 0;
                }

                base.OnUpdate();
            }

            public override void OnExit()
            {
                parent.transform.GetChild(1).gameObject.SetActive(true);
                parent.transform.position = new Vector3(parent.room.ReacquireTakenSpot(spotID).x, 0, parent.room.ReacquireTakenSpot(spotID).y);

                if (parent.waitingSpotID != -1)
                {
                    parent.room.ReturnTakenSpot(parent.waitingSpotID);
                    spotID = -1;
                    parent.waitingSpotID = -1;
                }
                base.OnExit();
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.OfficeMoved:
                        parent.TeleportTo(parent.room.ReacquireTakenSpot(spotID));
                        Vector3 movePos;
                        var dir = parent.room.GetSpotDirection(spotID);
                        movePos.x = parent.transform.position.x + dir.x;
                        movePos.y = 0;
                        movePos.z = parent.transform.position.z + dir.y;
                        parent.transform.position = movePos;
                        parent.StartDoctorWaitingSitAnimation(parent.room);
                        break;
                    case StateNotifications.OfficeReadyToWalkIn:
                        parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        break;
                    default:
                        break;
                }
            }

            public override string SaveToString()
            {
                return "SOWS!" + spotID.ToString();
            }
        }

        public class WaitingInReceptionState : BaseState
        {
            Vector3 oldPos;
            int spotID;

            float time = 0;

            public WaitingInReceptionState(ClinicPatientAI parent, int positionID, Vector2i pos) : base(parent)
            {
                parent.GetComponent<ClinicCharacterInfo>().ShowClinicSicknessCloud();
                parent.GoTo(pos, PathType.GoReceptionPath);
                this.spotID = positionID;
            }

            public override void OnEnter()
            {
                //	parent.GetComponent<HospitalCharacterInfo>().
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void OnExit()
            {
                parent.transform.position = oldPos;
                if (spotID != -1)
                {
                    HospitalAreasMapController.HospitalMap.reception.ReturnTakenSpotOnChairs(spotID);
                    spotID = -1;
                }

                base.OnExit();
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch (id)
                {
                    case (int)StateNotifications.FinishedMoving:
                        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open || TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_green_patient_popup_open)
                            NotificationCenter.Instance.TutorialArrowSet.Invoke(new TutorialArrowSetEventArgs(parent.AI.State));

                        var dir = HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotOnChairsDirection(spotID);
                        Vector3 movePos;
                        movePos.x = parent.transform.position.x + dir.x;
                        movePos.y = 0;
                        movePos.z = parent.transform.position.z + dir.y;
                        oldPos = parent.transform.position;
                        parent.transform.position = movePos;
                        parent.CheckAnimationDirection(movePos, oldPos);
                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                        parent.SittingAnim();
                        break;
                    default:
                        break;
                }
            }

            public override string SaveToString()
            {
                return "WIRS!" + spotID.ToString();
            }

            public override void OnUpdate()
            {
                time += Time.deltaTime;

                if (time > 4f)
                {
                    parent.SetRandomSitAnimation();
                    time = 0;
                }

                base.OnUpdate();
            }
        }

        public static class Vector3Ext
        {
            public static Vector3 Parse(string str)
            {
                var p = str.Substring(1, str.Length - 2).Split(',').Select(x => x.TrimStart(' ')).ToList();
                return new Vector3(float.Parse(p[0], CultureInfo.InvariantCulture), float.Parse(p[1], CultureInfo.InvariantCulture), float.Parse(p[2], CultureInfo.InvariantCulture));
            }
        }

        public class WanderingAroundState : BaseState
        {
            Rectangle area;

            AIWanderingMode wanderingMode = AIWanderingMode.Default;

            bool isPlayingHurrayAnimation = false;
            bool is_finished_path = false;
            bool hasToGetNewPath = true;
            bool LoadFromSave = false;
            bool hasFirstFinishedPath = false; // no talking in reception
            float timeWait = 0;

            Decoration currentDecoration = null;
            ClinicPatientAI otherPatient = null;
            Vector2i entrenceDecorationSpot = Vector2i.zero;
            Rotation entrenceDecorationRotation = Rotation.North;

            private IEnumerator<float> HurrayCoroutine;

            int currentDecorationSpot = -1;
            private bool unAnchored = false;
            private bool isPosInsideHospitalOrPatio = false;

            public WanderingAroundState(ClinicPatientAI parent, bool hasToGetNewPath = true, bool LoadFromSave = false) : base(parent)
            {
                this.currentDecorationSpot = -1;
                this.hasToGetNewPath = hasToGetNewPath;
                this.LoadFromSave = LoadFromSave;
                this.hasFirstFinishedPath = false;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                try
                {
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                if (LoadFromSave)
                {
                    parent.TeleportTo(parent.position);
                    hasToGetNewPath = true;
                    //SetRandomSpot();

                    isPosInsideHospitalOrPatio = HospitalAreasMapController.HospitalMap.IsPosInsideHospitalClinicOrPatio(parent.position);

                    if (!isPosInsideHospitalOrPatio) // fix for strange bug of stuck patient outside hospital in WanderingAround (wrong save)
                        parent.TeleportTo(new Vector2i(31, 42)); // teleport near main door
                }

                if (hasToGetNewPath)
                {
                    is_finished_path = false;
                    Vector2i spot = HospitalAreasMapController.HospitalMap.GetWaitingSpot(parent.position);
                    if (spot != Vector2i.zero)
                    {
                        parent.GoTo(spot, PathType.GoWanderingPath);
                        is_finished_path = false;
                    }
                    parent.GetComponent<ClinicCharacterInfo>().HideClinicSicknessCloud();
                }

                this.timeWait = Time.time + GameState.RandomNumber(6, 14);

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                if (HospitalAreasMapController.HospitalMap.GetAreaTypeFromPosition(parent.position) == HospitalArea.Patio)
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.patio);
                else
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.wandering);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        ActionWhenFinishedMoving();
                        break;
                    case StateNotifications.OfficeReadyToWalkIn:
                        parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        break;
                    case StateNotifications.DoctorRoomUnpacked:
                        if (currentDecoration != null)
                            return;

                        parent.StopMovement();
                        parent.abortPath();
                        isPlayingHurrayAnimation = true;
                        parent.SetRandomHurrayAnimation();
                        if (HurrayCoroutine != null)                        
                            Timing.KillCoroutine(HurrayCoroutine);

                        HurrayCoroutine = Timing.RunCoroutine(OnHurrayAnimationEnd());
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        try
                        {
                            ForceEndHurrayAnimation();
                            parent.StopMovement();
                            parent.abortPath();
                            parent.SetAnimationDirection(parent.isFront, parent.isRight);
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            unAnchored = true;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeAnchored:
                        ForceEndHurrayAnimation();
                        currentDecorationSpot = -1;
                        parent.walkingStateManager.State = null;
                        parent.AI.State = new WanderingAroundState(parent);
                        unAnchored = false;
                        break;
                    default:
                        break;
                }
            }

            private void ForceEndHurrayAnimation()
            {
                if (!isPlayingHurrayAnimation)
                    return;

                if (HurrayCoroutine != null)
                    Timing.KillCoroutine(HurrayCoroutine);

                isPlayingHurrayAnimation = false;
            }

            IEnumerator<float> OnHurrayAnimationEnd()
            {
                yield return Timing.WaitForSeconds(3f);
                if (HurrayCoroutine != null)
                    Timing.KillCoroutine(HurrayCoroutine);

                isPlayingHurrayAnimation = false;
                currentDecorationSpot = -1;
                parent.walkingStateManager.State = null;
                parent.AI.State = new WanderingAroundState(parent);
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                if (!unAnchored)
                {
                    if (Time.time > timeWait && is_finished_path == true && parent != null && HospitalAreasMapController.HospitalMap != null)// && isStartedInteractWithDecoration ==false)
                    {
                        if (wanderingMode == AIWanderingMode.Talk)
                            wanderingMode = AIWanderingMode.Default;

                        FreeOtherPatient();
                        FreeDecoraton();

                        if (parent.AI.State.GetType() == typeof(WanderingAroundState))
                        {
                            if (HospitalAreasMapController.HospitalMap.GetAreaTypeFromPosition(parent.position) == HospitalArea.Patio)
                                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.patio);
                            else
                                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.wandering);
                        }

                        if (GameState.RandomFloat(0, 1) > 0.55)
                        {
                            var spot = HospitalAreasMapController.HospitalMap.GetWaitingSpot(parent.position);
                            if (spot != Vector2i.zero)
                            {
                                //Debug.LogError("patient " + parent.name);

                                parent.GoTo(spot, PathType.GoWanderingPath);
                                is_finished_path = false;
                            }
                            else
                                timeWait += GameState.RandomNumber(2, 4);
                        }
                        else
                        {
                            currentDecoration = HospitalAreasMapController.HospitalMap.GetAvailableDecoration(out entrenceDecorationSpot, out entrenceDecorationRotation);

                            if (currentDecoration != null)
                            {
                                is_finished_path = false;
                                currentDecoration.isPatientUsing = true;
                                //Debug.LogWarning("Patient " + parent.name + " found decoration |" + currentDecoration.name + "| in position " + entrenceDecorationSpot.x + "," + entrenceDecorationSpot.y);
                                parent.GoTo(entrenceDecorationSpot, PathType.GoWanderingPath);
                            }
                            else
                                timeWait += GameState.RandomNumber(8, 16);
                        }
                    }
                    else if (wanderingMode == AIWanderingMode.CanTalk && hasFirstFinishedPath && otherPatient == null)       // TALKING WITH OTHER PATIENT CODE
                    {
                        // find near other patient METHOD #1
                        if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
                        {
                            for (int i = 0; i < BasePatientAI.patients.Count; ++i)
                            {
                                if (BasePatientAI.patients[i] != null)
                                {
                                    var odl = Mathf.Sqrt((BasePatientAI.patients[i].transform.position.x - parent.transform.position.x) * (BasePatientAI.patients[i].transform.position.x - parent.transform.position.x) + (BasePatientAI.patients[i].transform.position.z - parent.transform.position.z) * (BasePatientAI.patients[i].transform.position.z - parent.transform.position.z));

                                    if ((odl < 1.1f) && (odl > 0.75f) && BasePatientAI.patients[i].GetType() == typeof(ClinicPatientAI))
                                    {
                                        if ((((ClinicPatientAI)BasePatientAI.patients[i]).AI.State is WanderingAroundState) && (((ClinicPatientAI)BasePatientAI.patients[i]) != parent) && (((ClinicPatientAI)BasePatientAI.patients[i]).AI.State as WanderingAroundState).wanderingMode != AIWanderingMode.Talk)
                                        {
                                            if (AreaMapController.Map.IsPosInsideClinic(BasePatientAI.patients[i].position) == AreaMapController.Map.IsPosInsideClinic(parent.position)) // both in the same area
                                            {
                                                otherPatient = ((ClinicPatientAI)BasePatientAI.patients[i]);

                                                if (otherPatient != null)
                                                {
                                                    var otherPatientWanderingAroundState = (otherPatient.AI.State as WanderingAroundState);

                                                    if (otherPatientWanderingAroundState != null && otherPatientWanderingAroundState.currentDecoration == null)
                                                    {
                                                        BasePatientAI.patients[i].StopMovement();
                                                        parent.abortPath();
                                                        otherPatientWanderingAroundState.entrenceDecorationSpot = Vector2i.zero;
                                                        otherPatientWanderingAroundState.wanderingMode = AIWanderingMode.Talk;
                                                        otherPatientWanderingAroundState.is_finished_path = true;

                                                        //Debug.LogWarning(parent.name + " is talking right now with " + otherPatient.name + " in same state and his movement stopped");

                                                        parent.StopMovement();
                                                        parent.abortPath();
                                                        entrenceDecorationSpot = Vector2i.zero;
                                                        FreeDecoraton();
                                                        wanderingMode = AIWanderingMode.Talk;
                                                        is_finished_path = true;
                                                        try
                                                        {
                                                            parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                                                            otherPatientWanderingAroundState.parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            Debug.LogWarning("Animator - exception: " + e.Message);
                                                        }
                                                        UpdateRotationForPatients(parent, otherPatient);

                                                        //waiting = Time.time;

                                                        this.timeWait = Time.time + (GameState.RandomNumber(6, 14) / 2f);
                                                        otherPatientWanderingAroundState.timeWait = this.timeWait;
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public void RotateToDecoration(Decoration dec)
            {
                if (parent != null && dec != null)
                {
                    if (dec.isSpot)
                    {
                        switch (dec.spotRotation)
                        {
                            case Rotation.South:
                                parent.isFront = true;
                                parent.isRight = false;
                                break;
                            case Rotation.East:
                                parent.isFront = true;
                                parent.isRight = true;
                                break;
                            case Rotation.West:
                                parent.isFront = false;
                                parent.isRight = false;
                                break;
                            case Rotation.North:
                                parent.isFront = false;
                                parent.isRight = true;
                                break;
                            default:
                                break;
                        }

                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                    }
                    else
                    {
                        parent.CheckAnimationDirection(new Vector3(parent.position.x, 0, parent.position.y), new Vector3(dec.position.x, 0, dec.position.y));
                        parent.isRight = !parent.isRight;
                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                    }
                }
            }

            public void FreeDecoraton()
            {
                if (this != null && parent != null && currentDecoration != null)
                {
                    if (currentDecorationSpot != -1)
                    {
                        currentDecoration.FreeSpot(currentDecorationSpot);
                        currentDecorationSpot = -1;

                        if (entrenceDecorationSpot != Vector2i.zero && currentDecoration.isPatientUsing && currentDecoration.isSpot) // exit decoration
                        {
                            parent.position = entrenceDecorationSpot;
                            parent.transform.position = new Vector3(entrenceDecorationSpot.x, 0, entrenceDecorationSpot.y);
                            entrenceDecorationSpot = Vector2i.zero;
                        }
                    }
                    try
                    {
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                    currentDecoration.isPatientUsing = false;
                    currentDecoration = null;
                }
            }

            public void FreeOtherPatient()
            {
                if (otherPatient != null) // free connection with other patient when finished talking
                {
                    if (otherPatient.AI.State is WanderingAroundState)
                        (otherPatient.AI.State as WanderingAroundState).wanderingMode = AIWanderingMode.Default;

                    otherPatient = null;
                }
            }

            public void UpdateRotationForPatients(ClinicPatientAI pat1, ClinicPatientAI pat2)
            {
                if (pat1 != null && pat2 != null)
                {
                    pat1.CheckAnimationDirection(new Vector3(pat1.position.x, 0, pat1.position.y), new Vector3(pat2.position.x, 0, pat2.position.y));
                    pat1.SetAnimationDirection(pat1.isFront, pat1.isRight);

                    pat2.CheckAnimationDirection(new Vector3(pat2.position.x, 0, pat2.position.y), new Vector3(pat1.position.x, 0, pat1.position.y));
                    pat2.SetAnimationDirection(pat2.isFront, pat2.isRight);
                }
            }

            private void SetRandomSpot()
            {
                if (HospitalAreasMapController.HospitalMap.removableDeco == null)
                {
                    var pos = HospitalAreasMapController.HospitalMap.GetWaitingSpot(parent.position);
                    parent.TeleportTo(pos);
                }
                else
                {
                    var pos = HospitalAreasMapController.HospitalMap.GetWaitingSpot(parent.position, false);
                    parent.TeleportTo(pos);
                }

                is_finished_path = false;
            }

            private void ActionWhenFinishedMoving()
            {
                try
                {
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }

                hasFirstFinishedPath = true;
                float r = GameState.RandomFloat(0, 1);

                if (currentDecoration == null || (currentDecoration != null && !currentDecoration.isPatientUsing))
                {
                    // CV: does these checks have any sense in this order? 
                    if (r >= .9f)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Sneeze, 0, 0.0f);
                            SoundsController.Instance.PlayCough();
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else if (r >= .75f)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Streatch, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else if (r >= .6f)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Scratch, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else if (r >= .45f)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Phone, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else if (r >= .3f)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Read, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                    }
                    if (parent.AI.State.GetType() == typeof(WanderingAroundState))
                    {
                        if (HospitalAreasMapController.HospitalMap.GetAreaTypeFromPosition(parent.position) == HospitalArea.Patio)
                            parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.patio);
                        else
                            parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.wandering);
                    }
                }
                else if (currentDecoration != null && parent.position == entrenceDecorationSpot && entrenceDecorationSpot != Vector2i.zero)
                {
                    RotateToDecoration(currentDecoration);

                    if (currentDecoration.isSpot) // rotate in different way if is spot
                    {
                        currentDecorationSpot = currentDecoration.GetSpot(out Vector3 newPos);
                        parent.transform.position = newPos;
                    }

                    SetPatientAnimationForDecoration(currentDecoration);
                    parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.decoration);
                }
                else
                {
                    try
                    { 
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }

                if (r > .7f)
                    wanderingMode = AIWanderingMode.CanTalk;
                else wanderingMode = AIWanderingMode.Default;

                this.timeWait = Time.time + GameState.RandomNumber(3, 7);
                is_finished_path = true;
            }

            private void SetPatientAnimationForDecoration(Decoration dec)
            {
                if (dec.interactionType == DecorationInteractionType.Default)
                {
                    try
                    {
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
                else if (dec.interactionType == DecorationInteractionType.Siting)
                {
                    try
                    {
                        parent.anim.Play(AnimHash.Sit_Idle, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
                else if (dec.interactionType == DecorationInteractionType.Drinking)
                {
                    try
                    {
                        parent.anim.Play(AnimHash.Stand_Drink, 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
            }

            public override void OnExit()
            {
                base.OnExit();

                ForceEndHurrayAnimation();

                wanderingMode = AIWanderingMode.Default;

                FreeDecoraton();
                FreeOtherPatient();
            }

            public override string SaveToString()
            {
                return "WAS";
            }
        }

        public class WanderingAroundReceptionState : BaseState
        {
            Rectangle area;

            bool is_finished_path = false;
            bool hasToGetNewPath = true;
            bool LoadFromSave = true;
            float waiting = -1;

            private Vector2i RandomSpot()
            {
                return new Vector2i(area.x + GameState.RandomNumber(0, area.xSize), area.y + GameState.RandomNumber(0, area.ySize));
            }

            public WanderingAroundReceptionState(ClinicPatientAI parent, bool hasToGetNewPath = true, bool LoadFromSave = false) : base(parent)
            {
                parent.GetComponent<ClinicCharacterInfo>().ShowClinicSicknessCloud();
                area = HospitalAreasMapController.HospitalMap.GetReceptionArea();
                this.hasToGetNewPath = hasToGetNewPath;
                this.is_finished_path = false;
                this.LoadFromSave = LoadFromSave;
            }

            public override void OnEnter()
            {
                HospitalAreasMapController.HospitalMap.reception.waitingForChairSpot.Add(parent);
                base.OnEnter();
                try
                {
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }

                if (hasToGetNewPath)
                {
                    Vector2i spot = RandomSpot();
                    if (spot != Vector2i.zero)
                        parent.GoTo(spot, PathType.GoWanderingPath);
                }
                else
                    waiting = Time.time;

                if (this.LoadFromSave)
                    SetRandomSpot();

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        float r = GameState.RandomFloat(0, 1);
                        if (r >= .9f)
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Sneeze, 0, 0.0f);
                                SoundsController.Instance.PlayCough();
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        else if (r >= .75f)
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Streatch, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        else if (r >= .6f)
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Scratch, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        else if (r >= .45f)
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Phone, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        else if (r >= .3f)
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Read, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        else
                        {
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                        }
                        waiting = Time.time;
                        is_finished_path = true;
                        break;
                    case StateNotifications.OfficeReadyToWalkIn:
                        parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        break;
                    case StateNotifications.ReceptionSpotFree:
                        Vector2i pos;
                        var ID = HospitalAreasMapController.HospitalMap.reception.GetWaitingSpotOnChairs(out pos);
                        parent.waitingSpotID = ID;
                        if (ID > -1)
                            parent.AI.State = new WaitingInReceptionState(parent, ID, pos);
                        break;
                    default:
                        break;
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();

                if (waiting > 0 && Time.time - waiting > 8.0f && is_finished_path == true)
                {
                    waiting = Time.time;

                    if (GameState.RandomFloat(0, 1) > 0.75)
                    {
                        Vector2i spot = RandomSpot();
                        if (spot != Vector2i.zero)
                        {
                            parent.GoTo(spot, PathType.GoWanderingPath);
                            is_finished_path = false;
                        }
                    }
                }
            }

            private void SetRandomSpot()
            {
                var pos = RandomSpot();
                parent.TeleportTo(pos);
                is_finished_path = false;
            }

            public override void OnExit()
            {
                base.OnExit();
                HospitalAreasMapController.HospitalMap.reception.waitingForChairSpot.Remove(parent);
            }

            public override string SaveToString()
            {
                return "WARS";
            }
        }

        public class HealingState : BaseState
        {
            public HealingState(ClinicPatientAI parent) : base(parent) { }

            public override void OnEnter()
            {
                ReferenceHolder.Get().engine.AddTask(parent, () =>
                {
                    parent.StartHealingAnimation();
                    parent.AddHappyEffect(parent.room);
                });

                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.OfficeMoved:
                        parent.StartHealingAnimation();
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        parent.RemoveHappyEffect();
                        break;
                    case StateNotifications.OfficeAnchored:
                        parent.AddHappyEffect(parent.room);
                        break;
                    default:
                        break;
                }
            }

            public override void OnExit()
            {
                base.OnExit();
                parent.StopHealingAnimation();
                parent.RemoveHappyEffect();
            }

            public override string SaveToString()
            {
                return "HS";
            }
        }

        public class WalkingIntoDoctorRoomState : BaseState
        {
            public WalkingIntoDoctorRoomState(ClinicPatientAI parent) : base(parent) { }

            public override void OnEnter()
            {
                base.OnEnter();
                var p = parent.room.GetMachineSpot();

                parent.GoTo(p, PathType.GoHealingPath);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (parent.isGoHome)
                    return;

                switch ((StateNotifications)id)
                {
                    case StateNotifications.FinishedMoving:
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.AI.State = new HealingState(parent);
                        parent.room.PatientReady(parent);
                        break;
                    case StateNotifications.OfficeUnAnchored:
                        parent.StopMovement();
                        parent.abortPath();
                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeAnchored:
                        if (!(bool)parameters)
                            break;
                        Vector2i pos = parent.room.GetMachineSpot();
                        parent.GoTo(pos, PathType.GoHealingPath);
                        break;
                    default:
                        break;
                }
            }

            public override string SaveToString()
            {
                return "WIDRS";
            }

            public override void OnExit()
            {
                base.OnExit();
            }
        }

        public class GoingHomeState : BaseState
        {
            bool goOut = true;
            bool hasToGetNewPath = true;
            float hurrayTimer = 0;
            bool withHurray;
            bool goToSet = false;

            public GoingHomeState(ClinicPatientAI parent, bool hasToGetNewPath = true, bool withHurray = true) : base(parent)
            {
                this.hasToGetNewPath = hasToGetNewPath;
                this.withHurray = withHurray;

                parent.isGoHome |= hasToGetNewPath;

                if (parent.isReceptionCheckIn)
                    HospitalAreasMapController.HospitalMap.reception.FreeReceptionSpot();
            }

            public override void OnEnter()
            {
                base.OnEnter();

                if (hasToGetNewPath)
                {
                    if (parent.waitingSpotID != -1)
                    {
                        parent.room.ReturnTakenSpot(parent.waitingSpotID);
                        parent.waitingSpotID = -1;
                    }

                    parent.isGoHome = true;

                    if (withHurray)
                    {
                        parent.SetRandomHurrayAnimation();

                        if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                            Instantiate(ResourcesHolder.GetHospital().ParticleCure, parent.transform.position, Quaternion.identity);

                        hurrayTimer = 0;
                        ClinicCharacterInfo hearts = parent.GetComponent<ClinicCharacterInfo>();
                        //hearts.SetHeartsActive ();
                        //tututu
                    }
                    else
                        parent.GoTo(new Vector2i(21, 21), PathType.GoHomePath);
                }
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
                parent.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.cured);
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                if (withHurray)
                {
                    hurrayTimer += Time.deltaTime;
                    if (!goToSet && hurrayTimer > 1.75f)    //1.75 ~time of hurray anim
                    {
                        parent.GoTo(new Vector2i(21, 21), PathType.GoHomePath);
                        goToSet = true;
                    }
                }
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (hasToGetNewPath)
                {
                    if ((int)StateNotifications.FinishedMoving == id)
                    {
                        if (goOut)
                        {
                            goOut = false;
                            parent.GoTo(new Vector2i(20, 20), PathType.GoHomePath);
                            return;
                        }
                        parent.IsoDestroy();
                    }
                    else if ((int)StateNotifications.OfficeUnAnchored == id)
                    {
                        parent.StopMovement();
                        parent.abortPath();
                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.AI.State = new GoingHomeState(parent, false);
                    }
                }

                if ((int)StateNotifications.OfficeAnchored == id)
                {
                    parent.walkingStateManager.State = null;
                    parent.AI.State = new GoingHomeState(parent);
                }
            }

            public override string SaveToString()
            {
                return "GHS";
            }

            public override void OnExit()
            {
                base.OnExit();
                if (parent != null)
                    parent.IsoDestroy();

                parent.isGoHome = true;
            }
        }

        public class GreatNameForHospitalState : BaseState
        {
            //bool goOut = false;
            bool hasToGetNewPath = true;
            float hurrayTimer = 0;
            public bool is_finished_path = false;
            private bool start_follow = false;
            WatchingNameState nameState = WatchingNameState.Start;
            bool subscribeNotification = false;

            public GreatNameForHospitalState(ClinicPatientAI parent, bool hasToGetNewPath = true) : base(parent)
            {
                this.hasToGetNewPath = hasToGetNewPath;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                parent.isGoHome = true;
                Debug.LogWarning("GreatNameForHospitalState");
                if (hasToGetNewPath)
                {
                    if (parent.waitingSpotID != -1)
                    {
                        parent.room.ReturnTakenSpot(parent.waitingSpotID);
                        parent.waitingSpotID = -1;
                    }

                    parent.SetRandomHurrayAnimation();
                    hurrayTimer = 0;
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                hurrayTimer += Time.deltaTime;

                if (nameState == WatchingNameState.Start && hurrayTimer > 1.75f)    //1.75 ~time of hurray anim
                {
                    parent.GoTo(new Vector2i(21, 42), PathType.GoHomePath);
                    nameState = WatchingNameState.GoToFrontHospital;
                    is_finished_path = false;
                    ClinicCharacterInfo hearts = parent.GetComponent<ClinicCharacterInfo>();
                    //hearts.SetHeartsActive ();
                }

                if (nameState == WatchingNameState.GoToFrontHospital)
                {
                    if (subscribeNotification && is_finished_path)
                    {
                        if (TutorialController.Instance.CurrentTutorialStepIndex == TutorialController.Instance.GetStepId(StepTag.patient_text_2) && (!TutorialController.Instance.ConditionFulified))
                        {
                            Debug.LogWarning("->");
                            nameState = WatchingNameState.Wait;
                            subscribeNotification = false;
                        }
                    }

                    if ((TutorialController.Instance.CurrentTutorialStepIndex == TutorialController.Instance.GetStepId(StepTag.patient_text_2)) && (!start_follow))
                    {
                        ReferenceHolder.Get().engine.MainCamera.FollowGameObject(parent.transform);
                        start_follow = true;
                    }
                }

                if (TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.newspaper_1) || !TutorialController.Instance.tutorialEnabled)
                {
                    parent.walkingStateManager.State = null;
                    ReferenceHolder.Get().engine.MainCamera.StopFollowing();
                    parent.AI.State = new GoingHomeState(parent, true, false);
                    Debug.Log("GO HOME FIRST PATIENT");
                }
            }

            public bool IsWaitingNearHospitalBilboard()
            {
                if (nameState == WatchingNameState.Wait)
                {
                    Debug.LogWarning("Wait near bilboard");
                    return true;
                }

                return false;
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (hasToGetNewPath)
                {
                    if ((int)StateNotifications.FinishedMoving == id)
                    {
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        is_finished_path = true;
                        subscribeNotification = true;
                    }
                    else if ((int)StateNotifications.OfficeUnAnchored == id)
                    {
                        parent.StopMovement();
                        parent.abortPath();
                        parent.SetAnimationDirection(parent.isFront, parent.isRight);
                        try
                        {
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        parent.AI.State = new GreatNameForHospitalState(parent, false);
                    }
                }

                if ((int)StateNotifications.OfficeAnchored == id)
                {
                    parent.walkingStateManager.State = null;
                    parent.AI.State = new GreatNameForHospitalState(parent, true);
                }
            }

            public override string SaveToString()
            {
                return "GNFH";
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public enum WatchingNameState
            {
                Start = 0,
                GoToFrontHospital = 1,
                Wait = 2,
            }
        }

        public class BaseState : IState
        {
            protected ClinicPatientAI parent;
            public BaseState(ClinicPatientAI parent)
            {
                this.parent = parent;
            }

            public virtual void Notify(int id, object parameters) { }

            public virtual void OnEnter() { }

            public virtual void OnExit() { }

            public virtual void OnUpdate() { }

            public virtual string SaveToString()
            {
                return "";
            }
        }
        #endregion
    }


    public enum StateNotifications
    {
        FinishedMoving,
        AbortState,
        OfficeReadyToWalkIn,
        OfficeMoved,
        OfficeUnAnchored,
        OfficeAnchored,
        ReceptionChanged,
        ReceptionSpotFree,
        GoToDoctorWaitingSpot,
        GoHome,
        FirstInQueue,
        InQueue,
        GoToPlayroomWaitingSpot,
        ReceptionWaitingForCheckInSpotFree,
        ForceRemoveFromDoctorQueue,
        ForceTeleportToHospitalBed,
        DoctorRoomUnpacked,
        CuresDelivered,
        OnChildCollect,
        SendToDiagnose,
        SendToLabor,
        CollectRewardForLabor,
        SpeedUpRewardForLabor,
        BloodTestSpeedUp,
        SpeedUpMotherSpawn,
        SpeedUpWaitingForLabour,
        SpeedUpLabor,
    }

    public enum LoadNotification
    {
        LoadFromSave,
        EmulateTime,
    }

    public enum ReceptionState
    {
        Default = 0,
        WaitnQueue = 1,
        CheckInReception = 2,
    }
}

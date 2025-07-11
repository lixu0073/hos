using UnityEngine;
using IsoEngine;
using System;

namespace Hospital
{
    public enum ChildSpawnMode
    {
        none = 0,
        goPlayroom = 1,
        makeFun = 0,
    }

    public class ChildPatientAI : BasePatientAI
	{
		public StateManager AI;

		public DoctorRoom room = null;
        [HideInInspector]
        public int waitingSpotID = -1;

        // rozwiązanie tymczasowe aby rozróżnić który to dzieciak a który nie (a temporary solution to tell which kid is and which is not)
        float size = 0.85f;
        
        public int spotInPlayroom = -1;
        // [HideInInspector]
        public bool isGoHome = false;

        public void SetDoctorRoom( DoctorRoom room)
        {
            this.room = room;
        }

        public bool iSDoctorRoomWithSickness()
        {
			if (this.GetComponent<ClinicCharacterInfo>().clinicDisease != null && room != null)
                return true;

            return false;
        }

        public override string SaveToString()
		{
            var z = GetComponent<ClinicCharacterInfo>();

            if (AI.State != null)
                return "C" + Checkers.CheckedPosition(position, z.name).ToString() + "!" + Checkers.CheckedChildAIState(AI.State.SaveToString()) + "^" + Checkers.CheckedPatientBIO(z.personalBIO, z.name) + "^" + Checkers.CheckedAmount(z.clinicDisease.id, 0, 8, "clinicDisease").ToString() + "^" + "False";

            return "C" + Checkers.CheckedPosition(position, z.name).ToString() + "!" + "NULL" + "^" + Checkers.CheckedPatientBIO(z.personalBIO, z.name) + "^" + Checkers.CheckedAmount(z.clinicDisease.id, 0, 8, "clinicDisease").ToString() + "^" + "False";
        }

		public override void IsoDestroy()
		{
           // if (HospitalAreasMapController.Map!=null)
           // {
           //     HospitalAreasMapController.Map.playgroud.RemoveKid(this);
           // }
			base.IsoDestroy();
		}

        public void Initialize(Vector2i pos, DoctorRoom room)
        {
            this.room = room;
            base.Initialize(pos);
            base.isKid = true;
            this.gameObject.transform.localScale = new Vector3(size, size, size);
            this.gameObject.GetComponent<BaseCloudController>().SetDefaultScale(size);
            AI = new StateManager();

            GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            GetComponent<ClinicCharacterInfo>().positiveEnergyIcon.SetActive(false);//MEMEME
        }

        public override void Initialize(RotatableObject room, string info)
        {
            var strs = info.Substring(1).Split('!');
			base.Initialize(Vector2i.Parse(strs[0]));
            this.gameObject.transform.localScale = new Vector3(size, size, size);
            this.gameObject.GetComponent<BaseCloudController>().SetDefaultScale(size);
            //print(info);
            this.room = (DoctorRoom)room;
			AI = new StateManager();

            GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
            GetComponent<ClinicCharacterInfo>().positiveEnergyIcon.SetActive(false);//MEMEME

            base.isKid = true;
            if (strs.Length > 1)
			{
				int spotInDoctorRoom;
				switch (strs[1])
				{
					case "GHCS":
                        isGoHome = true;
                        AI.State = new GoHomeChildState(this);
						break;
                    case "GTDR":
                        spotInDoctorRoom = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        if (AI.State != null)
                            AI.State.Notify((int)StateNotifications.AbortState, null);
                        AI.State = new GoingToDoctorRoomState(this, spotInDoctorRoom, this.room.ReacquireTakenSpot(spotInDoctorRoom));
                        break;
                    case "WIDRS":
                        AI.State = new WalkingIntoDoctorRoomState(this);
						break;
					case "HS":
						AI.State = new HealingState(this);
						break;
                    case "WIFOP":
                        AI.State = new WaitInFrontOfPlayroom(this);
                        break;
                    case "IS":
					spotInPlayroom = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        if (AI.State != null)
                            AI.State.Notify((int)StateNotifications.AbortState, null);
                        AI.State = new InitialState(this, spotInPlayroom);
						break;
                    case "PRS":
					spotInPlayroom = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        AI.State = new GoToPlayroomState(this, spotInPlayroom);
                        break;
                    case "PS":
					spotInPlayroom = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        AI.State = new PlayingState(this, spotInPlayroom);
						break;
                    case "SOWS":
                        spotInDoctorRoom = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
                        var pos = this.room.ReacquireTakenSpot(spotInDoctorRoom);
                        TeleportTo(pos);
                        AI.State = new SittingOnRoomWaitingSpot(this, spotInDoctorRoom);
                        break;
                    default:
                        break;
                }
			}
			GameState.Get().childrenList.Add(this.GetComponent<BaseCharacterInfo>());
        }

		public DoctorRoom ReturnRoom()
		{
			return room;
		}

        public void SetPlaying()
        {
            AI.State = new PlayingState(this, spotInPlayroom);
        }

        public void SetGoToPlayroom(int spot)
        {
            spotInPlayroom = spot;

            if (AI.State != null)
                AI.State.Notify((int)StateNotifications.AbortState, null);

            AI.State = new GoToPlayroomState(this, spot);
        }

        public void SetWaitInFronOfPlayroom()
        {
            AI.State = new WaitInFrontOfPlayroom(this);
        }

        public void StartSittingAnimation(RotatableObject room)
        {
            //print("starting sitting");
            base.StartDoctorWaitingSitAnimation(room);
        }
        public void SittingAnim()
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
            try
            {
                SetAnimationDirection(isFront, isRight);
                anim.Play(AnimHash.Sit_Idle, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            //SetRandomSitAnimation();
        }

        public void StartHealingAnimation()
		{
			base.SetHealingAnimation(room,1);
        }

		public void StopHealingAnimation()
		{
            transform.position = new Vector3(room.GetMachineSpot().x,0, room.GetMachineSpot().y);
            position = room.GetMachineSpot();
			//	transform.GetChild(0).gameObject.GetComponent<Animator>().Play(AnimHash.Machine_Blue_Click);
		}

		public void Initialize(Vector2i pos, int spotInPlayroomID)
        {
            spotInPlayroom = spotInPlayroomID;

            base.Initialize(pos);
            base.isKid = true;
            this.gameObject.transform.localScale = new Vector3(size, size, size);
            this.gameObject.GetComponent<BaseCloudController>().SetDefaultScale(size);
            AI = new StateManager();
            if (AI.State != null)
                AI.State.Notify((int)StateNotifications.AbortState, null);
            AI.State = new InitialState(this, spotInPlayroom);

            return;
        }

        protected override void Update()
        {
            base.Update();
            AI.Update();
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
                case StateNotifications.OfficeReadyToWalkIn:
                        AI.State = new WalkingIntoDoctorRoomState(this);
                    break;
                case StateNotifications.GoHome:
                    isGoHome = true;

                    if (AI.State != null)
                        AI.State.Notify((int)StateNotifications.AbortState, null);
                    AI.State = new GoHomeChildState(this);
                    return;
                case StateNotifications.GoToDoctorWaitingSpot:
                    if (waitingSpotID == -1)
                    {
                        Vector2i pos;
                        waitingSpotID = (int)parameters;
                        room.GetWaitingSpotByID(waitingSpotID, out pos);
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

        public void SendToDoctorRoom()
        {
            AI.State = new GoingToDoctorRoomState(this, 0, room.ReacquireTakenSpot(0));
        }

        #region States
        public class SittingOnRoomWaitingSpot : BaseState
        {
            int spotID;
            float time = 0;

            public SittingOnRoomWaitingSpot(ChildPatientAI parent, int spotID) : base(parent)
            {
                parent.transform.GetChild(1).gameObject.SetActive(false);
                parent.waitingSpotID = spotID;
                this.spotID = spotID;

                parent.abortPath();
                parent.TeleportTo(parent.room.ReacquireTakenSpot(parent.waitingSpotID));

                Vector3 movePos;
                var dir = parent.room.GetSpotDirection(parent.waitingSpotID);

                movePos.x = parent.transform.position.x + dir.x;
                movePos.y = 0;
                movePos.z = parent.transform.position.z + dir.y;
                parent.transform.position = movePos;

                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    parent.StartSittingAnimation(parent.room);
                });
            }

			public override void OnEnter()
			{
                base.OnEnter();

                parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.notActive);
				parent.GetComponent<IPersonCloudController> ().SetCloudMessageType (CloudsManager.MessageType.cDocQueue);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (true);//MEMEME
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

                if (spotID != -1)
                {
                    parent.room.ReturnTakenSpot(spotID);
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

        public class GoHomeChildState : BaseState
        {
            bool goOut = true;
            bool hasToGetNewPath = true;
            float hurrayTimer = 0;
            bool withHurray;
            bool goToSet = false;

            public GoHomeChildState(ChildPatientAI parent, bool hasToGetNewPath = true, bool withHurray = true) : base(parent)
            {
                parent.StopMovement();
                parent.abortPath();

                this.hasToGetNewPath = hasToGetNewPath;
                this.withHurray = withHurray;

                parent.isGoHome |= hasToGetNewPath;
            }

            public override void OnEnter()
            {
                base.OnEnter();

                if (parent.waitingSpotID != -1)
                {
                    parent.room.ReturnTakenSpot(parent.waitingSpotID);
                    parent.waitingSpotID = -1;
                }

                if (hasToGetNewPath)
                {
                    parent.isGoHome = true;

                    if (withHurray)
                    {
                        parent.SetRandomHurrayAnimation();

                        if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                            Instantiate(ResourcesHolder.GetHospital().ParticleCure, parent.transform.position, Quaternion.identity);

                        hurrayTimer = 0;
                    }
                    else                    
                        parent.GoTo(new Vector2i(21, 21), PathType.GoHomePath);
                }

				parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (false);//MEMEME
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
                    if (id == (int)StateNotifications.FinishedMoving)
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
                        parent.walkingStateManager.State = null;
                        parent.AI.State = new GoHomeChildState(parent, false);
                    }
                }

                if ((int)StateNotifications.OfficeAnchored == id)
                {
                    parent.walkingStateManager.State = null;
                    parent.AI.State = new GoHomeChildState(parent);
                }
            }

            public override string SaveToString()
            {
                return "GHCS";
            }

            public override void OnExit()
            {
                base.OnExit();

                if (parent != null)
                    parent.IsoDestroy();

                parent.isGoHome = true;
            }
        }

        public class GoingToDoctorRoomState : BaseState
        {
            int spotID;
            bool shouldReturn = true;
            public GoingToDoctorRoomState(ChildPatientAI parent, int spotID, Vector2i pos) : base(parent)
            {

                shouldReturn = true;
                if (parent.room.shouldWork)
                {
                    parent.GoTo(pos, PathType.GoHealingPath);
                    parent.GetComponent<ClinicCharacterInfo>().positiveEnergyIcon.SetActive(true);//MEMEME
                }
                else
                {
                    parent.GetComponent<ClinicCharacterInfo>().positiveEnergyIcon.SetActive(false);
                    parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                }

                this.spotID = spotID;

                parent.GetComponent<ClinicCharacterInfo>().HideClinicSicknessCloud();
                parent.SetAnimationDirection(parent.isFront, parent.isRight);
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
                            parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            parent.walkingStateManager.State = null;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeAnchored:
                        try
                        {
                            if (!(bool)parameters)
                                return;

                            Vector2i pos = parent.room.ReacquireTakenSpot(spotID);
    					    parent.GoTo(pos, PathType.GoHealingPath);
                            parent.GetComponent<ClinicCharacterInfo>().positiveEnergyIcon.SetActive(true);//MEMEME
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    case StateNotifications.OfficeReadyToWalkIn:
                        try
                        {
                            shouldReturn = true;
                            parent.AI.State = new WalkingIntoDoctorRoomState(parent);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Animator - exception: " + e.Message);
                        }
                        break;
                    default:
                        break;
                }
            }

            public override void OnUpdate() { }

            public override void OnExit()
            {
                if (shouldReturn)
                {
                    parent.room.ReturnTakenSpot(spotID);
                    parent.waitingSpotID = -1;
                }
                base.OnExit();
            }

            public override string SaveToString()
            {
                return "GTDR!" + spotID.ToString();
            }
        }

        public class WalkingIntoDoctorRoomState : BaseState
		{
            public WalkingIntoDoctorRoomState(ChildPatientAI parent) : base(parent) { }

			public override void OnEnter()
			{
                base.OnEnter();
                var p = parent.room.GetMachineSpot();

                parent.GoTo(p, PathType.GoHealingPath);
                parent.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (true);//MEMEME
			}

			public override void Notify(int id, object parameters)
			{
				base.Notify(id, parameters);

                if (parent.isGoHome)            
                    return;

                if (parent != null)
                {
                    switch ((StateNotifications)id)
                    {
                        case StateNotifications.FinishedMoving:
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                                parent.AI.State = new HealingState(parent);
                                parent.room.PatientReady(parent);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case StateNotifications.OfficeUnAnchored:
                            try
                            {
                                parent.walkingStateManager.State = null;
                                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case StateNotifications.OfficeAnchored:
                            try
                            {
                                if (!(bool)parameters)
                                    return;
                                Vector2i pos = parent.room.GetMachineSpot();
                                parent.GoTo(pos, PathType.GoHealingPath);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        default:
                            break;
                    }
                }
			}

			public override string SaveToString()
			{
				return "WIDRS";
			}
		}


        public class HealingState : BaseState
        {
            public HealingState(ChildPatientAI parent) : base(parent) { }

            public override void OnEnter()
            {
                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    parent.StartHealingAnimation();
                    parent.AddHappyEffect(parent.room);
                });
				parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (false);//MEMEME

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


        public class InitialState : BaseState
		{
			int spotInPlayroom= -1;
            //private bool is_going = false;

            public InitialState(ChildPatientAI parent, int spotInPlayroom = -1) : base(parent)
			{
                    this.spotInPlayroom = spotInPlayroom;
                    parent.spotInPlayroom = spotInPlayroom;
            }

            public override void OnEnter()
            {
                base.OnEnter();
				parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (false);//MEMEME
			}

            public override void Notify(int id, object parameters)
			{
                base.Notify(id, parameters);

                if (id == (int)StateNotifications.GoToPlayroomWaitingSpot)
                {
                    parent.AI.State = new GoToPlayroomState(parent, parent.spotInPlayroom);
                    Debug.LogWarning("Found path to spot " + parent.spotInPlayroom + " for: " + parent.name);
                }
            }

            public override void OnUpdate()
            {
                if (parent.spotInPlayroom != -1)
                {
                    if (parent.AI.State != null)
                    {
                        parent.AI.State.Notify((int)StateNotifications.GoToPlayroomWaitingSpot, null);
                        return;
                    }
                }

                base.OnUpdate();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override string SaveToString()
			{
				return "IS!" + spotInPlayroom.ToString();
			}
		}


        public class WaitInFrontOfPlayroom : BaseState
        {
            float hurrayTimer = 0;
            float waitingAnimTimer = 0;

            public WaitInFrontOfPlayroom(ChildPatientAI parent) : base(parent) { }

            public override void OnEnter()
            {
                base.OnEnter();
				parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (true);//MEMEME
            }

            public override void OnUpdate()
            {
                if(HospitalAreasMapController.HospitalMap == null || HospitalAreasMapController.HospitalMap.playgroud == null)
                    return;

                if (HospitalAreasMapController.HospitalMap.playgroud.actualLevel == 1 && hurrayTimer > 1.75f) 
                {
                      parent.SetGoToPlayroom(HospitalAreasMapController.HospitalMap.playgroud.GetToySpotCount()-1);
                      return;
                }

                if (HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState == ExternalRoom.EExternalHouseState.enabled)
                {
                    if (hurrayTimer == 0)
                        parent.SetRandomHurrayAnimation();

                    hurrayTimer += Time.deltaTime;
                    waitingAnimTimer = 0;
                }
                else
                {
                    waitingAnimTimer += Time.deltaTime;
                    hurrayTimer = 0;
                    if (waitingAnimTimer > 3f)
                    {
                        parent.SetRandomStandAnimation();

                        waitingAnimTimer = 0;
                    }
                }
                base.OnUpdate();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override string SaveToString()
            {
                return "WIFOP";
            }
        }


        public class GoToPlayroomState : BaseState // SPOT FOR TIMMY
        {
            int spotInPlayroom = -1;
            private bool is_started= false;

            public GoToPlayroomState(ChildPatientAI parent, int spotInPlayroom = -1) : base(parent)
            {
                this.spotInPlayroom = spotInPlayroom;
                parent.spotInPlayroom = spotInPlayroom;
            }

            public override void OnEnter()
            {
                base.OnEnter();

				parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (true);//MEMEME

                parent.GoTo(HospitalAreasMapController.HospitalMap.playgroud.GetToyPositionForID(spotInPlayroom), PathType.GoPlaygroundPath);
                parent.SetAnimationDirection(parent.isFront, parent.isRight);
                is_started = true;
            }

            public override void Notify(int id, object parameters)
            {
                base.Notify(id, parameters);

                if (id == (int)StateNotifications.FinishedMoving && is_started)
                {
                    try
                    {
                        parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                        Debug.LogWarning("Start Playing animations for " + parent.name);
                        parent.AI.State = new PlayingState(parent, spotInPlayroom);
                    }
                    catch(Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
            }

            public override void OnExit()
            {
                base.OnExit();
            }

            public override string SaveToString()
            {
                return "PRS!" + spotInPlayroom.ToString();
            }
        }


        public class PlayingState : BaseState
		{
			int spotInPlayroom;
			float startTime;
			float playTime;
		    //bool playing = false;
			public PlayingState(ChildPatientAI parent, int spotInPlayroom) : base(parent)
			{
				this.spotInPlayroom = spotInPlayroom;
                parent.spotInPlayroom = spotInPlayroom;

                if (spotInPlayroom != -1)   // fix for old saves
                {
                    Vector2i spotPos = HospitalAreasMapController.HospitalMap.playgroud.GetToyPositionForID(spotInPlayroom);

                    if (parent.position != spotPos)
                        parent.TeleportTo(spotPos);
                }
			}

			public override void OnEnter ()
			{
                base.OnEnter();

                parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
				parent.GetComponent<ClinicCharacterInfo> ().positiveEnergyIcon.SetActive (false);//MEMEME

                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    switch (HospitalAreasMapController.HospitalMap.playgroud.GetToySpotType(spotInPlayroom))
                    {
                        case 5:
                            try
                            { 
                                parent.anim.Play(AnimHash.Playroom_slide, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case 4:
                            try
                            {
                                parent.anim.Play(AnimHash.Playroom_bricks2, 0, 0.0f);
                                SetShadow(new Vector3(0.05f, 0.045f, 0.05f));
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case 3:
                            try
                            {
                                parent.anim.Play(AnimHash.Playroom_bricks, 0, 0.0f);
                                SetShadow(new Vector3(0.35f, 0.05f, 0.45f));
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case 2:
                            try
                            {
                                parent.anim.Play(AnimHash.Playroom_horse, 0, 0.0f);
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        case 1:
                            try
                            {
                                parent.anim.Play(AnimHash.Playroom_beanbag, 0, 0.0f);
                                SetShadow(new Vector3(-0.03f, 0.05f, -0.11f));
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                        default:
                            try
                            {
                                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                                SetShadow(new Vector3(0.35f, 0.05f, 0.45f));
                            }
                            catch (Exception e)
                            {
                                Debug.LogWarning("Animator - exception: " + e.Message);
                            }
                            break;
                    }

                    parent.SetAnimationDirection(parent.isFront, parent.isRight);
                    parent.anim.speed = GameState.RandomFloat(0.5f, 1.0f);
                });
            }

			private void SetShadow(Vector3 position) 
			{
				parent.transform.GetChild(1).gameObject.transform.localPosition = position;
			}
	
			public override void OnExit()
			{
				base.OnExit();
            }

			public override string SaveToString()
			{
				return "PS!" + spotInPlayroom.ToString();
			}
		}


		public class BaseState : IState
		{
			protected ChildPatientAI parent;

			public BaseState(ChildPatientAI parent)
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
}
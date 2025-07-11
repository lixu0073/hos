using Hospital;
using IsoEngine;
using Maternity;
using Maternity.UI;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Maternity
{
    public class MaternityLabourRoom : RotatableObject
    {

        public static event Action<MaternityLabourRoom> MaternityLabourRoomAddedToMap;
        public static event Action<MaternityLabourRoom> MaternityLabourRoomRemovedFromMap;
        public static event Action<MaternityLabourRoom> Unwrap;

        private IMaternityFacilityPatient patient;
        private long ID;
        private Vector2i roomEntrance;
        private Vector2i labourPosition = new Vector2i(2, 2);
        private Vector2i motherWithBabyPosition = new Vector2i(2, 2);
        private Transform floor;
        private Renderer mat;
        private GameObject tempFloor;
        private MaternityLabourRoomInfo roomInfo;

        private MaternityLabourRoomObjects objects;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public override void MoveCameraToThisRoom()
        {
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(GetMotherWithBabyPosition(), 1f, false);
        }

        public MaternityWaitingRoomIndicatorsController indicatorController = new MaternityWaitingRoomIndicatorsController();
        private MaternityWaitingRoomIndicatorsPresenter indicatorPresenter;

        public void SetUpIndicators()
        {
            if (GetObj() == null)
                return;
            if (indicatorPresenter != null)
            {
                Destroy(indicatorPresenter.gameObject);
            }
            indicatorPresenter = Instantiate(ResourcesHolder.GetMaternity().WaitingRoomIndicators, GetObj().transform);
            indicatorPresenter.gameObject.transform.SetAsLastSibling();
            indicatorController.SetPresenter(indicatorPresenter);
            indicatorPresenter.transform.position = GetIndicatorPosition();
            indicatorPresenter.onClick = OnClickWorking;
            indicatorController.Start(GetWaitingRoom().bed, this);
        }

        public Vector3 GetIndicatorPosition()
        {
            Vector3 offset = new Vector3(3.96f, 0, 2.92f);
            
            switch (actualRotation)
            {
                case Rotation.North:
                    offset = new Vector3(3.31f, 0, 3.29f);
                    break;
                case Rotation.South:
                    offset = new Vector3(4.09f, 0, 3.15f);
                    break;
                case Rotation.West:
                    offset = new Vector3(3, 0, 3);
                    break;
                case Rotation.East:
                    offset = new Vector3(3.21f, 0, 4.25f);
                    break;
            }
            
            return new Vector3(position.x, 0, position.y) + offset;
        }

        public void DelayedCollectChildAnimations()
        {
            StartCoroutine(DelayedCollectChildAnimationsCall());
        }
        private IEnumerator DelayedCollectChildAnimationsCall()
        {
            yield return new WaitForSeconds(0.8f);
            MaternityLabourRoomObjects objects = GetObjects();
            if (objects != null)
            {
                objects.SpawnBalloons();
            }
            UIController.getMaternity.babyPopup.SetBabyPopupActive(true);
        }

        public MaternityLabourRoomObjects GetObjects()
        {
            if (GetObj() != null)
            {
                return GetObj().GetComponent<MaternityLabourRoomObjects>();
            }
            return null;
        }

        public ShopRoomInfo.RoomColor GetColor()
        {
            if (roomInfo == null)
                roomInfo = (MaternityLabourRoomInfo)info.infos;
            return roomInfo.roomColor;
        }

        public MaternityWaitingRoom GetWaitingRoom()
        {
            foreach (MaternityWaitingRoom waitingRoom in MaternityWaitingRoomController.Instance.Rooms())
            {
                if (GetColor() == waitingRoom.GetColor())
                    return waitingRoom;
            }
            return null;
        }

        public bool HasWorkingLabourRoom()
        {
            MaternityWaitingRoom waitingRoom = GetWaitingRoom();
            return waitingRoom != null && waitingRoom.state == State.working;
        }

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            SetSpots();
            ID = (long)ServerTime.getMilliSecTime();
            SetUpIndicators();
        }
        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            if (patient != null)
            {
                patient.GetPatientAI().Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            }
            foreach (var p in BasePatientAI.patients)
            {
                if (p != null)
                {
                    // update all patients or all hospitalPatient for this room after anchored
                    if (p is MaternityPatientAI && (patient == null || (!((p as MaternityPatientAI).GetPatientID() == patient.GetPatientAI().GetPatientID()))))
                        p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
                }
            }
        }
        public Vector2i GetMachinePosition()
        {
            return new Vector2i(position.x + labourPosition.x, position.y + labourPosition.y);
        }

        public Vector2i GetMotherWithBabyPosition()
        {
            return new Vector2i(position.x + motherWithBabyPosition.x, position.y + motherWithBabyPosition.y);
        }

        public void AssignPatientToLabourRoom(IMaternityFacilityPatient patient)
        {
            this.patient = patient;
        }
        public void RemovePatientFromLabourRoom()
        {
            patient = null;
        }
        public long GetRoomRoomID()
        {
            return ID;
        }
        public Vector2i GetEntrancePosition()
        {
            return new Vector2i(roomEntrance.x + position.x, roomEntrance.y + position.y);
        }
        public override void IsoUpdate()
        {
            base.IsoUpdate();
        }
        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
            {
                EmulateTime((TimePassedObject)parameters);
            }
        }
        public long GetRoomID()
        {
            return ID;
        }

        public override void IsoDestroy()
        {
            /*if (Unwrap != null)
            {
                foreach (Action<MaternityLabourRoom> d in Unwrap.GetInvocationList())
                {
                    Unwrap -= d;
                }
            }*/
            if (patient != null)
            {
                patient.DestroyPatient();
            }
            patient = null;
            if (indicatorController != null)
            {
                indicatorController.OnDestroy();
            }
            MaternityLabourRoomObjects objects = GetObjects();
            if (objects != null)
            {
                objects.RemoveObjects();
            }
            DestroyFloorMaterial();
            OnMaternityLabourRoomRemmoved();
            base.IsoDestroy();
        }

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
        protected override void OnClickWorking()
        {
            TutorialController tc = TutorialController.Instance;
            if (tc != null && tc.GetCurrentTutorialStep() != null && tc.GetCurrentTutorialStep().StepTag == StepTag.maternity_labor_room_completed)
                return;

            MaternityWaitingRoom waitingRoom = GetWaitingRoom();
            if (waitingRoom == null)
            {
                Debug.LogError("No waiting room!");
            }
            else
            {
                if (indicatorController.OverrideOnClickBehaviour())
                {
                    indicatorController.OnIndicatorClicked();
                }
                else
                {
                    UIController.getMaternity.patientCardController.Open(waitingRoom.bed);
                }
            }
        }
        protected override void OnClickWaitForUser()
        {
            base.OnClickWaitForUser();
            int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false, Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });
            UnwrapEvent();
            NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this));
        }

        public void SetupID()
        {
            if (ID == 0)
            {
                ID = (long)ServerTime.getMilliSecTime();
            }
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            string[] saveData = save.Split(';');
            ID = long.Parse(saveData[1]);
            base.LoadFromString(save, timePassed, actionsDone);
            EmulateTime(timePassed);
            SetUpIndicators();
        }
        protected override string SaveToString()
        {
            return base.SaveToString() + ";" + ID.ToString();
        }
        protected override void OnStateChange(State newState, State oldState)
        {
            base.OnStateChange(newState, oldState);
            if (floor != null)
            {
                DestroyFloorMaterial();
                Destroy(tempFloor);
            }
            if (newState == State.building)
            {
                tempFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mat = tempFloor.GetComponent<Renderer>();
                mat.material = new Material(AreaMapController.Map.wallDatabase.materialPrefab);
                mat.material.mainTexture = ResourcesHolder.Get().UnderConstructionTile;
                mat.material.mainTextureScale = Vector2.one * 4;
            }
            if (newState == State.working || newState == State.fresh || newState == State.waitingForUser)
            {
                tempFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
                mat = tempFloor.GetComponent<Renderer>();
                mat.material = new Material(AreaMapController.Map.wallDatabase.materialPrefab);
                var p = info.infos.dummyType;
                if (p == BuildDummyType.MaternityLabourRoom)
                    mat.material.mainTexture = ResourcesHolder.GetMaternity().LabourRoomFloor;
                mat.material.mainTextureScale = Vector2.one;
            }

            if (tempFloor != null)
            {
                floor = tempFloor.transform;
                floor.localScale = Vector3.one * 4;
                floor.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        protected override void InitializeFromSave(string save, Rotations info, TimePassedObject timePassed)
        {
            base.InitializeFromSave(save, info, timePassed);
            SetSpots();
        }
        protected override void AddToMap()
        {
            base.AddToMap();
            SetSpots();

            if (state == State.working)
            {
                if (patient != null)
                {
                    patient.GetPatientAI().Notify((int)StateNotifications.OfficeMoved, obj == null);
                }
            }
            SetFloor(isoObj != null);
            SetupID();
            OnMaternityLabourRoomAdded();
            SetUpIndicators();
            MaternityWaitingRoom room = GetWaitingRoom();
            if (patient != null && patient.GetPatientAI().Person.State.GetTag() != PatientStates.MaternityPatientStateTag.GTLR)
            {
                patient.GetPatientAI().gameObject.SetActive(false);
            }
            if (room != null && room.bed != null && room.bed.GetPatient() != null && room.bed.GetPatient().GetPatientAI() != null && (room.bed.GetPatient().GetPatientAI().Person.State.GetTag() == PatientStates.MaternityPatientStateTag.LF || room.bed.GetPatient().GetPatientAI().Person.State.GetTag() == PatientStates.MaternityPatientStateTag.IL))
            {
                if (isoObj != null)
                {
                    MaternityLabourRoomObjects objects = GetObjects();
                    if (objects != null)
                    {
                        objects.SetIdleCover();
                        patient.GetPatientAI().gameObject.SetActive(true);
                    }
                }
            }
        }

        private void SetFloor(bool val)
        {
            if (floor != null)
            {
                floor.gameObject.SetActive(val);
                ActualiseFloor();
            }
        }
        private void ActualiseFloor()
        {
            if (floor != null)
            {
                //actualRotation
                Vector2 rot = new Vector2(((int)actualRotation + 1) % 2, ((int)actualRotation) % 2);

                if (((actualData.tilesX - 1) % 2) != (actualData.tilesY % 2))
                {
                    floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2) + rot.x * ((actualRotation == Rotation.North || actualRotation == Rotation.West) ? -0.5f : 0.5f), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2) + rot.y * ((actualRotation == Rotation.North || actualRotation == Rotation.West) ? -0.5f : 0.5f));
                }
                else
                {
                    floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));
                }

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
                    floor.localScale = new Vector2(Mathf.Min(actualData.tilesX, actualData.tilesY), Mathf.Max(actualData.tilesY, actualData.tilesX) - 1);
                    floor.rotation = Quaternion.Euler(90, actualData.tilesX > actualData.tilesY ? 90 : 0, 0);
                    mat.material.mainTextureScale = Vector2.one;
                }
                floor.transform.SetParent(transform);
            }
        }
        private void DestroyFloorMaterial()
        {
            EngineController.DestroyMaterial(floor.gameObject);
        }
        private void SetSpots()
        {
            if (state != State.working)
                return;
            foreach (var p in actualData.spotsData)
            {
                switch ((SpotTypes)p.id)
                {
                    case SpotTypes.Door:
                        roomEntrance = new Vector2i(p.x, p.y);
                        break;
                    case SpotTypes.Machine:
                        labourPosition = new Vector2i(p.x, p.y);
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnMaternityLabourRoomAdded()
        {
            var eventToRise = MaternityLabourRoomAddedToMap;
            if (eventToRise != null)
            {
                eventToRise(this);
            }
        }

        private void OnMaternityLabourRoomRemmoved()
        {
            var eventToRise = MaternityLabourRoomRemovedFromMap;
            if (eventToRise != null)
            {
                eventToRise(this);
            }
        }

        private void UnwrapEvent()
        {
            var eventToRise = Unwrap;
            if (eventToRise != null)
            {
                eventToRise(this);
            }
        }
    }

}

using Hospital;
using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using Maternity.UI;
using SimpleUI;

namespace Maternity
{

    public class MaternityWaitingRoom : RotatableObject
    {

        public static event Action<MaternityWaitingRoom> MaternityWaitingRoomAddedToMap;
        public static event Action<MaternityWaitingRoom> MaternityWaitingRoomRemovedToMap;
        public static event Action<MaternityWaitingRoom> Unwrap;
        public MaternityWaitingRoomBed bed;
        public MaternityWaitingRoomIndicatorsController indicatorController = new MaternityWaitingRoomIndicatorsController();
        private MaternityWaitingRoomIndicatorsPresenter indicatorPresenter;

        private MaternityWaitingRoomInfo roomInfo;

        private long ID;
        private Vector2i BedSpot = new Vector2i(2, 2);
        private Vector2i roomEntrance = new Vector2i(0, 0);
        private Vector2i YogaPosition = new Vector2i(2, 2);
        private Vector2i StretchPosition = new Vector2i(2, 2);
        private Vector2i BallPosition = new Vector2i(2, 2);
        private Transform floor;
        private Renderer mat;
        private GameObject tempFloor;
        private MaternityWaitingRoomObjects waitingRoomGameObjects;
        private MaternityWaitingRoomBedObjectController bedObjController;

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
            indicatorPresenter.transform.position = GetIndicatorPosition();
            indicatorController.SetPresenter(indicatorPresenter);
            indicatorPresenter.onClick = OnClickWorking;
            indicatorController.Start(bed, this);
        }

        public override void MoveCameraToThisRoom()
        {
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(GetBedPosition(), 1f, false);
        }

        #region publicMethods
        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            SetSpots();
            InitializeBeds();
        }

        public ShopRoomInfo.RoomColor GetColor()
        {
            if (roomInfo == null)
                roomInfo = (MaternityWaitingRoomInfo)info.infos;
            return roomInfo.roomColor;
        }

        public MaternityLabourRoom GetLabourRoom()
        {
            foreach (MaternityLabourRoom labourRoom in MaternityLabourRoomController.Instance.Rooms())
            {
                if (GetColor() == labourRoom.GetColor())
                    return labourRoom;
            }
            return null;
        }

        private ShopRoomInfo laborRoomInfo = null;
        public ShopRoomInfo GetLabourRoomInfo()
        {
            if (laborRoomInfo != null)
                return laborRoomInfo;
            foreach (ShopRoomInfo info in AreaMapController.Map.drawerDatabase.DrawerItems)
            {
                if (info is MaternityLabourRoomInfo)
                {
                    if (GetColor() == (((MaternityLabourRoomInfo)info).roomColor))
                    {
                        laborRoomInfo = info;
                        return laborRoomInfo;
                    }
                }
            }
            return null;
        }

        public bool HasWorkingLabourRoom()
        {
            MaternityLabourRoom labourRoom = GetLabourRoom();
            return labourRoom != null && labourRoom.state == State.working;
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            if (bed != null && bed.IsBedOccupied())
            {
                bed.GetPatient().GetPatientAI().Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            }
            foreach (var p in BasePatientAI.patients)
            {
                if (p != null)
                {
                    // update all patients or all hospitalPatient for this room after anchored
                    if (p is MaternityPatientAI && bed != null && (bed.GetPatient() == null || (!((p as MaternityPatientAI).GetPatientID() == bed.GetPatient().GetPatientAI().GetPatientID()))))
                        p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
                }
            }
        }
        public Vector2i GetBuildingPosition()
        {
            return position;
        }

        public long GetRoomID()
        {
            return ID;
        }
        public Vector2i GetEntrancePosition()
        {
            return new Vector2i(roomEntrance.x + position.x, roomEntrance.y + position.y);
        }

        public Vector2i GetYogaPosition()
        {
            return new Vector2i(YogaPosition.x + position.x, YogaPosition.y + position.y);
        }

        public Vector2i GetStretchPosition()
        {
            return new Vector2i(StretchPosition.x + position.x, StretchPosition.y + position.y);
        }

        public Vector2i GetBallPosition()
        {
            return new Vector2i(BallPosition.x + position.x, BallPosition.y + position.y);
        }

        public Vector2i GetBedPosition()
        {
            return new Vector2i(BedSpot.x + position.x, BedSpot.y + position.y);
        }

        public Vector3 GetIndicatorPosition()
        {
            GameObject bedGameObject = GetBedGameObject();
            Vector3 bedSpot = bedGameObject != null ? GetBedGameObject().transform.position : Vector3.zero;
            Vector3 offset = new Vector3(1.56f, 0, 1.06f);
            switch (actualRotation)
            {
                case Rotation.West:
                case Rotation.East:
                    offset = new Vector3(1.24f, 0, 1.37f);
                    break;
            }
            return bedSpot + offset;
        }

        public void SetupBed(bool isPatientExist, bool isPatientAway = false, bool isPatientPregnant = false)
        {
            if (bedObjController != null)
            {
                bedObjController.SetupBed(isPatientExist, isPatientAway, isPatientPregnant);
            }
        }

        public void RevealBloodTestBedTable()
        {
            bedObjController.RevealBloodTestBadge();
        }

        public void ShowBloodTestBedTable()
        {
            bedObjController.ShowBloodTestBadge();
        }

        public void HideBloodTestBedTable()
        {
            bedObjController.HideBloodTestBadge();
        }
        public Transform GetBloodResultTable()
        {
            return waitingRoomGameObjects.bloodTestResult;
        }

        public GameObject GetBedGameObject()
        {
            if (waitingRoomGameObjects != null)
            {
                return waitingRoomGameObjects.hospitalBed;
            }
            return null;
        }

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            if (bed != null && bed.StateManager != null && bed.StateManager.State != null)
            {
                bed.StateManager.State.OnUpdate();
            }
        }
        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
            {
                EmulateTime((TimePassedObject)parameters);
            }
        }
        public override void IsoDestroy()
        {
            /*if (Unwrap != null)
            {
                foreach (Action<MaternityWaitingRoom> d in Unwrap.GetInvocationList())
                {
                    Unwrap -= d;
                }
            }*/
            if (bed != null)
            {
                if (bed.StateManager != null && bed.StateManager.State != null)
                    bed.StateManager.State.OnExit();
                bed.Destroy();
            }
            bed = null;
            if (indicatorController != null)
            {
                indicatorController.OnDestroy();
            }
            DestroyFloorMaterial();
            OnMaternityWaitingRemovedAdded();
            base.IsoDestroy();
        }
        #endregion
        #region protectedMethods
        protected override Vector2 GetHoverPosition()
        {
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

            if (indicatorController.OverrideOnClickBehaviour())
            {
                indicatorController.OnIndicatorClicked();
            }
            else
            {
                UIController.getMaternity.patientCardController.Open(bed);
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
            NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this));
            UnwrapEvent();
        }
        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            string[] saveData = save.Split(';');
            if (saveData.Length > 1)
            {
                ID = long.Parse(saveData[1]);
            }
            base.LoadFromString(save, timePassed, actionsDone);
            EmulateTime(timePassed);
        }

        protected override string SaveToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(base.SaveToString());
            builder.Append(";");
            builder.Append(ID.ToString());
            builder.Append(";");
            builder.Append(bed == null ? "" : bed.SaveToString());
            return builder.ToString();
        }

        public void SetupID()
        {
            if (ID == 0)
            {
                ID = (long)ServerTime.getMilliSecTime();
            }
        }

        public override void LoadFromStringAfterAllRoomsLoaded(string str, TimePassedObject timePassed)
        {
            bed = new MaternityWaitingRoomBed(this);
            string[] data = str.Split(';');
            if (data.Length > 2)
            {
                if (!string.IsNullOrEmpty(data[2]))
                {
                    bed.LoadFromString(data[2]);
                }
            }
            EmulateBedTime(timePassed);
            SetUpIndicators();
        }

        public override void EmulateTime(TimePassedObject time)
        {
            base.EmulateTime(time);
            EmulateBedTime(time);
        }

        private void EmulateBedTime(TimePassedObject timePassed)
        {
            if (bed != null)
                bed.EmulateTime(timePassed);
        }

        public override void OnEmulationEnded()
        {
            if (bed != null)
            {
                bed.OnEmulationEnded();
            }
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
                if (p == BuildDummyType.MaternityWaitingRoom)
                    mat.material.mainTexture = ResourcesHolder.GetMaternity().WaitingRoomFloor;
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
            if (GetObj() != null)
            {
                waitingRoomGameObjects = GetObj().GetComponent<MaternityWaitingRoomObjects>();
                bedObjController = GetObj().GetComponent<MaternityWaitingRoomBedObjectController>();
            }
            if (state == State.working)
            {
                if (bed != null)
                {
                    IMaternityFacilityPatient patient = bed.GetPatient();
                    if (patient != null)
                    {
                        patient.GetPatientAI().Notify((int)StateNotifications.OfficeMoved, obj == null);
                        if (patient.GetPatientAI().IsLayingInBed())
                        {
                            patient.GetPatientAI().gameObject.SetActive(isoObj != null);
                        }
                        if (bed.StateManager.State.GetTag() == MaternityWaitingRoomBed.State.NELR || bed.StateManager.State.GetTag() == MaternityWaitingRoomBed.State.WFP || (bed.StateManager.State.GetTag() == MaternityWaitingRoomBed.State.OR && bed.GetPatient().GetPatientAI().Person.State.GetTag() == PatientStates.MaternityPatientStateTag.GTWR))
                        {
                            SetupBed(false);
                        }
                        else if (patient == null || !patient.GetPatientAI().IsLayingInBed())
                        {
                            SetupBed(true, true);
                        }
                    }
                }
            }
            SetFloor(isoObj != null);
            SetupID();
            OnMaternityWaitingRoomAdded();
            SetUpIndicators();
        }
        #endregion
        #region privateMethods
        private void InitializeBeds()
        {
            bed = new MaternityWaitingRoomBed(this);
            bed.LoadFromString();
            SetUpIndicators();
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

                if ((info.infos.dummyType == BuildDummyType.Hospital2xRoom) || (info.infos.dummyType == BuildDummyType.Hospital3xRoom) || ((actualData.tilesX) % 2) == (actualData.tilesY % 2))
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
            foreach (var value in actualData.spotsData)
            {
                switch ((SpotTypes)value.id)
                {
                    case SpotTypes.Door:
                        roomEntrance = new Vector2i(value.x, value.y);
                        break;
                    case SpotTypes.HospitalBed:
                        BedSpot = new Vector2i(value.x, value.y);
                        break;
                    case SpotTypes.BallSpot:
                        BallPosition = new Vector2i(value.x, value.y);
                        break;
                    case SpotTypes.YogaSpot:
                        YogaPosition = new Vector2i(value.x, value.y);
                        break;
                    case SpotTypes.StretchSpot:
                        StretchPosition = new Vector2i(value.x, value.y);
                        break;
                    default:
                        break;
                }
            }
        }
        private void OnMaternityWaitingRoomAdded()
        {
            var eventToRise = MaternityWaitingRoomAddedToMap;
            if (eventToRise != null)
            {
                eventToRise(this);
            }
        }

        private void OnMaternityWaitingRemovedAdded()
        {
            var eventToRise = MaternityWaitingRoomRemovedToMap;
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
        #endregion

    }
}

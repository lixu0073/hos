using Hospital;
using IsoEngine;
using Maternity.PatientStates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Maternity
{
    public class MaternityPatientAI : MaternityBasePersonAI, IMaternityFacilityPatient
    {
        public MaternityStateManager Person;
        public MaternityWaitingRoom waitingRoom;
        public List<IMotherRelatives> relatives;
        private int queueID = -1;
        private const char RELATIVE_SEPARATOR = '#';
        private RotatableObject currentOccupiedRoom;

        public delegate void OnStateChanged();
        public event OnStateChanged onStateChanged;

        public void NotifyStateChanged()
        {
            onStateChanged?.Invoke();
        }

        public BabyCharacterInfo GetBabyInfo()
        {
            return relatives == null || relatives.Count == 0 ? null : relatives[0].GetInfo();
        }

        public int GetQueueID()
        {
            return queueID;
        }
        public void SetQueueID(int newID)
        {
            queueID = newID;
        }

        public override void Initialize(Vector2i pos)
        {
            base.Initialize(pos);
            Person = new MaternityStateManager(this);
        }

        public void Initialize(Vector2i pos, MaternityWaitingRoom destRoom)
        {
            base.Initialize(pos);
            ID = (long)ServerTime.getMilliSecTime() + destRoom.Tag;
            Person = new MaternityStateManager(this);
            waitingRoom = destRoom;
            Person.State = new MaternityPatientGoToWaitingRoomState(destRoom, this);
            MaternityPatientsHolder.Instance.AddPatient(this);
        }

        public static MaternityPatientParsedGeneralData ParsePatientGeneralData(string info)
        {
            var strs = info.Split('!');
            string ID = strs[0];
            Vector2i position = Vector2i.Parse(strs[1]);
            MaternityPatientStateTag stateTag = MaternityPatientStateTag.NULL;
            int queueID = int.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);
            if (strs.Length > 4)
            {
                stateTag = (MaternityPatientStateTag)Enum.Parse(typeof(MaternityPatientStateTag), strs[4]);
            }
            return new MaternityPatientParsedGeneralData(ID, stateTag, position, queueID);
        }

        public override void Initialize(RotatableObject room, string info)
        {
            try
            {
                MaternityPatientParsedGeneralData data = ParsePatientGeneralData(info);
                MaternityBloodTestRoom bloodTestRoom = MaternityBloodTestRoomController.Instance.GetBloodTestRoom();
                base.Initialize(data.position);
                ID = data.patientID;
                Person = new MaternityStateManager(this);
                waitingRoom = (MaternityWaitingRoom)room;
                queueID = data.queueID;
                if (data.stateTag != MaternityPatientStateTag.NULL)
                {
                    MaternityIState state = null;
                    //string[] strs = info.Split('!');
                    switch (data.stateTag)
                    {
                        case MaternityPatientStateTag.GTWR:
                            state = new MaternityPatientGoToWaitingRoomState(waitingRoom, this);
                            break;
                        case MaternityPatientStateTag.WFSTD:
                            state = new MaternityPatientWaitingForSendToDiagnoseState(waitingRoom, this);
                            break;
                        case MaternityPatientStateTag.GO:
                            state = MaternityPatientGoingOutState.GetInstance(this, MaternityPatientGoingOutState.Parse(info));
                            break;
                        case MaternityPatientStateTag.WFC:
                            state = new MaternityPatientWaitingForCuresState(waitingRoom, this, true);
                            break;
                        case MaternityPatientStateTag.WFL:
                            state = MaternityPatientWaitingForLaborState.GetInstance(this, waitingRoom, MaternityPatientWaitingForLaborState.Parse(info));
                            break;
                        case MaternityPatientStateTag.RFL:
                            state = new MaternityPatientReadyForLaborState(waitingRoom, this);
                            break;
                        case MaternityPatientStateTag.IL:
                            state = MaternityPatientInLaborState.GetInstance(this, MaternityPatientInLaborState.Parse(info));
                            break;
                        case MaternityPatientStateTag.LF:
                            state = MaternityPatientLaborFinishedState.GetInstance(this, MaternityPatientLaborFinishedState.Parse(info));
                            break;
                        case MaternityPatientStateTag.RTWR:
                            state = MaternityPatientReturnToWaitingRoomState.GetInstance(waitingRoom, this, MaternityPatientReturnToWaitingRoomState.Parse(info));
                            break;
                        case MaternityPatientStateTag.B:
                            state = MaternityPatientBondingState.GetInstance(waitingRoom, this, MaternityPatientBondingState.Parse(info));
                            break;
                        case MaternityPatientStateTag.GTLR:
                            state = MaternityPatientGoToLaborRoomState.GetInstance(this, MaternityPatientGoToLaborRoomState.Parse(info));
                            break;
                        case MaternityPatientStateTag.WFCR:
                            state = new MaternityPatientWaitingForCollectRewardState(waitingRoom, this);
                            break;
                        case MaternityPatientStateTag.IDQ:
                            state = MaternityPatientInDiagnoseQueueState.GetInstance(waitingRoom, bloodTestRoom, this, MaternityPatientInDiagnoseQueueState.Parse(info));
                            break;
                        case MaternityPatientStateTag.ID:
                            state = MaternityPatientInDiagnoseState.GetInstance(waitingRoom, bloodTestRoom, this, MaternityPatientInDiagnoseState.Parse(info));
                            break;
                        case MaternityPatientStateTag.WFDR:
                            state = new MaternityPatientInWaitingForDiagnoseResultsState(waitingRoom, this);
                            break;
                    }
                    MaternityIState nextState = state.GetNextStateOnLoad();
                    if (nextState != null)
                    {
                        state = nextState;
                    }
                    Person.State = state;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
            MaternityPatientsHolder.Instance.AddPatient(this);
        }

        internal void LoadRelatives(string relatives)
        {
            string[] unparsedData = relatives.Split(RELATIVE_SEPARATOR);
            for (int i = 0; i < unparsedData.Length; i++)
            {
                RelativesType relativeType = (RelativesType)Enum.Parse(typeof(RelativesType), unparsedData[i].Split('/')[0]);
                string relativeInfo = unparsedData[i].Split('/')[1];
                MaternityAISpawner.Instance.LoadMotherRelative(this, relativeType, relativeInfo);
            }
        }

        public void AddRelative(IMotherRelatives relative)
        {
            if (relatives == null)
            {
                relatives = new List<IMotherRelatives>();
            }
            relatives.Add(relative);
        }

        public void EmulateTime(TimePassedObject timePassed)
        {
            if (Person != null && Person.State != null)
                Person.State.EmulateTime(timePassed);
        }

        public void OnEmulationEnded()
        {

        }

        public void LayPatientInBed(int defaultLayInBedAnimation)
        {
            SetDefaultLayInBedAnimation(defaultLayInBedAnimation);
            GameObject bed = waitingRoom.GetBedGameObject();
            if (bed != null)
            {
                LayInBed(waitingRoom, waitingRoom.GetBedGameObject());
            }
        }

        public override string SaveToString()
        {
            var z = GetComponent<MaternityCharacterInfo>();
            string stringToReturn = ID + "!" + Checkers.CheckedPosition(position, z.name) + "!" + Checkers.CheckedAmount(queueID, -1, int.MaxValue, "Queue ID: ") + "!" + z.SaveToString() + "!" + Person.State.SaveToString() + "^" + z.personalBIO + "^";
            if (relatives != null && relatives.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < relatives.Count; i++)
                {
                    if (i != 0)
                    {
                        sb.Append(RELATIVE_SEPARATOR);
                    }
                    sb.Append(relatives[i].SaveToString());
                }
                stringToReturn = stringToReturn + sb.ToString();
            }
            return stringToReturn;
        }

        public override void IsoDestroy()
        {
            if (this == null)
                return;
            var p = GetComponent<MaternityCharacterInfo>();
            MaternityPatientsHolder.Instance.Remove(this);
            //Patients.Remove(p);
            //UIController.getHospital.PatientCard.RemovePatient(p, true);
            //HospitalDataHolder.Instance.RemovePatientFromQueues(this);
            base.IsoDestroy();
        }

        public void RemovePatientFromBed(float timeToSpawn = -1, float baseTimeToSpawn = -1)
        {
            if (waitingRoom != null)
            {
                waitingRoom.bed.RemovePatient(timeToSpawn, baseTimeToSpawn);
            }
        }

        #region Broadcast Data From StateMachine

        public event Action<MaternityPatientWaitingForLaborState.Data> OnDataReceived_WFL;
        public event Action<MaternityPatientBaseLaborState.Data> OnDataReceived_BLS;
        public event Action<MaternityPatientBaseBondingState.Data> OnDataReceived_BBS;
        public event Action<MaternityPatientInDiagnoseState.Data> OnDataRecieved_ID;
        public event Action<MaternityPatientGoingOutState.Data> OnDataReceived_GO;

        public void BroadcastData(MaternityPatientGoingOutState.Data data)
        {
            OnDataReceived_GO?.Invoke(data);
        }

        public void BroadcastData(MaternityPatientWaitingForLaborState.Data data)
        {
            OnDataReceived_WFL?.Invoke(data);
        }

        public void BroadcastData(MaternityPatientBaseLaborState.Data data)
        {
            OnDataReceived_BLS?.Invoke(data);
        }

        public void BroadcastData(MaternityPatientBaseBondingState.Data data)
        {
            OnDataReceived_BBS?.Invoke(data);
        }

        public void BroadcastData(MaternityPatientInDiagnoseState.Data data)
        {
            OnDataRecieved_ID?.Invoke(data);
        }

        #endregion

        protected override void Update()
        {
            base.Update();

            if (Person != null)
                Person.Update();
        }
        public override void Notify(int id, object parameters)
        {
            if (Person.State != null)
                Person.State.Notify(id, parameters);
        }

        protected override void ReachedDestination()
        {
            base.ReachedDestination();
            if (Person.State != null)
                Person.State.Notify((int)StateStatus.FinishedMoving, null);
        }

        public void GetDiagnoseTime()
        {
            if (Person.State.GetTag() == MaternityPatientStateTag.ID)
            {
                Person.State.BroadcastData();
            }
        }

        public MaternityCharacterInfo GetInfoPatient()
        {
            return GetComponent<MaternityCharacterInfo>();
        }

        public MaternityPatientAI GetPatientAI()
        {
            return this;
        }

        public void DestroyPatient()
        {
            IsoDestroy();
        }

        public string GetPatientID()
        {
            return ID;
        }

        public Transform GetMotherBabyPosition()
        {
            return GetComponent<MaternityCharacterInfo>().babyPosition;
        }

        private enum StateStatus
        {
            FinishedMoving,
            OfficeUnAnchored,
            OfficeAnchored
        }

        protected override void LayInBed(RotatableObject room, GameObject bed)
        {
            switch (room.actualData.rotation)
            {
                case Rotation.North:
                    isFront = true;
                    isRight = true;
                    break;

                case Rotation.South:
                    isFront = true;
                    isRight = true;
                    break;

                case Rotation.East:
                    isFront = true;
                    isRight = false;
                    break;

                case Rotation.West:
                    isFront = true;
                    isRight = false;
                    break;
            }
            gameObject.SetActive(true);
            transform.position = bed.transform.GetChild(0).transform.position;
            SetAnimationDirection(isFront, isRight);
            PlayLayInBedAnimation();
        }

        public RotatableObject GetCurrentOccupiedRoom()
        {
            return currentOccupiedRoom;
        }

        public void SetCurrentOccupiedRoom(RotatableObject room)
        {
            currentOccupiedRoom = room;
        }

        public bool IsLayingInBed()
        {
            switch (Person.State.GetTag())
            {
                case MaternityPatientStateTag.NULL:
                    return false;
                case MaternityPatientStateTag.GTWR:
                    return false;
                case MaternityPatientStateTag.WFSTD:
                    return true;
                case MaternityPatientStateTag.GO:
                    return false;
                case MaternityPatientStateTag.WFC:
                    return true;
                case MaternityPatientStateTag.WFL:
                    return true;
                case MaternityPatientStateTag.RFL:
                    return true;
                case MaternityPatientStateTag.IDQ:
                    return true;
                case MaternityPatientStateTag.ID:
                    return true;
                case MaternityPatientStateTag.WFDR:
                    return true;
                case MaternityPatientStateTag.IL:
                    return false;
                case MaternityPatientStateTag.LF:
                    return false;
                case MaternityPatientStateTag.RTWR:
                    return false;
                case MaternityPatientStateTag.B:
                    return true;
                case MaternityPatientStateTag.GTLR:
                    return false;
                case MaternityPatientStateTag.WFCR:
                    return true;
                default:
                    return false;
            }
        }
    }
}

public class MaternityPatientParsedGeneralData
{
    public readonly string patientID;
    public readonly MaternityPatientStateTag stateTag;
    public readonly Vector2i position;
    public readonly int queueID;

    public MaternityPatientParsedGeneralData(string patientID, MaternityPatientStateTag stateTag, Vector2i position, int queueID)
    {
        this.patientID = patientID;
        this.position = position;
        this.queueID = queueID;
        this.stateTag = stateTag;
    }

}

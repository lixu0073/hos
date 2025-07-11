using IsoEngine;
using UnityEngine;
using Hospital;
using Maternity.UI;
using System.Globalization;

namespace Maternity.PatientStates
{
    public class MaternityPatientGoingOutState : MaternityIState
    {
        protected MaternityPatientAI parent;
        protected MaternityPatientStateTag stateTag;
        private Vector2i destination;
        //private bool unAnchored = false;

        public Data data = new Data();

        public enum INDEX
        {
            destination = 5,
            timeLeft = 6,
            baseTimeLeft = 7
        }

        public static MaternityPatientGoingOutStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            Vector2i destinationVector = Vector2i.Parse(strs[(int)INDEX.destination]);
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            float baseTimeLeft = float.Parse(strs[(int)INDEX.baseTimeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientGoingOutStateParsedData(destinationVector, timeLeft, baseTimeLeft);
        }

        public static MaternityPatientGoingOutState GetInstance(MaternityPatientAI parent, MaternityPatientGoingOutStateParsedData data)
        {
            return new MaternityPatientGoingOutState(data.destinationVector, data.timeLeft, parent, data.baseTimeLeft);
        }

        public MaternityPatientGoingOutState(Vector2i destination, float timeLeft, MaternityPatientAI parent, float baseSpawnTime)
        {
            stateTag = MaternityPatientStateTag.GO;
            this.parent = parent;
            this.destination = destination;
            data.timeLeft = timeLeft;
            data.baseSpawnTime = baseSpawnTime;
        }

        public void OnEnter()
        {
            Game.Instance.gameState().IsMaternityFirstLoopCompleted = true;
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WithBaby_Walking);
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_W_Baby);
            parent.GoTo(destination, PathType.GoHomePath);
        }

        public void Notify(int id, object parameters)
        {
            switch ((StateNotifications)id)
            {
                case StateNotifications.SpeedUpMotherSpawn:
                    parent.IsoDestroy();
                    parent.RemovePatientFromBed(0, 0);
                    break;
                case StateNotifications.FinishedMoving:
                    parent.IsoDestroy();
                    parent.RemovePatientFromBed(data.timeLeft, data.baseSpawnTime);
                    break;
                case StateNotifications.OfficeUnAnchored:
                    parent.PlayStandIdleAnimation();
                    parent.walkingStateManager.State = null;
                    //unAnchored = true;
                    break;
                case StateNotifications.OfficeMoved:
                    break;
                case StateNotifications.OfficeAnchored:
                    if (parent.isMovementStopped())
                    {
                        parent.GoTo(destination, PathType.Default);
                    }
                    // parent.UnCoverBed();
                    //unAnchored = false;
                    break;
                default:
                    break;
            }
        }

        public virtual string SaveToString()
        {
            return stateTag.ToString() + "!" + destination.ToString() + "!" + data.timeLeft + "!" + data.baseSpawnTime;
        }

        public virtual MaternityIState GetNextStateOnLoad()
        {
            return null;
        }

        public MaternityPatientStateTag GetTag()
        {
            return stateTag;
        }

        public virtual void EmulateTime(TimePassedObject timePassed)
        {
            data.timeLeft -= timePassed.GetTimePassed();
            if (data.timeLeft < 0)
                data.timeLeft = 0;
        }

        public virtual void BroadcastData()
        {
            parent.BroadcastData(data);
        }

        public void OnExit()
        {
            parent.abortPath();
        }

        public void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            if (data.timeLeft < 0)
                data.timeLeft = 0;
            if (data.timeLeft <= 0)
            {
                parent.IsoDestroy();
                parent.RemovePatientFromBed(0, 0);
            }
            BroadcastData();
        }

        public class Data
        {
            public float timeLeft = 0;
            public float baseSpawnTime = 0;
        }

        public virtual MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.WaitingForPatientIndicator(parent, parent.waitingRoom.bed);
        }

        public void MoveTo()
        {
            parent.waitingRoom.MoveCameraToThisRoom();
        }

        public RotatableObject GetRoom()
        {
            return null;
        }
    }

    public class MaternityPatientGoingOutStateParsedData
    {
        public Vector2i destinationVector;
        public float timeLeft;
        public float baseTimeLeft;

        public MaternityPatientGoingOutStateParsedData(Vector2i destinationVector, float timeLeft, float baseTimeLeft)
        {
            this.destinationVector = destinationVector;
            this.timeLeft = timeLeft;
            this.baseTimeLeft = baseTimeLeft;
        }
    }
}

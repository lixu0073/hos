using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientReturnToWaitingRoomState : MaternityPatientBaseBondingState
    {
        public MaternityPatientReturnToWaitingRoomState(MaternityWaitingRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent, timeLeft)
        {
            stateTag = MaternityPatientStateTag.RTWR;
        }

        public static MaternityPatientReturnToWaitingRoomStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientReturnToWaitingRoomStateParsedData(timeLeft);
        }

        public static MaternityPatientReturnToWaitingRoomState GetInstance(MaternityWaitingRoom room, MaternityPatientAI parent, MaternityPatientReturnToWaitingRoomStateParsedData data)
        {
            return new MaternityPatientReturnToWaitingRoomState(room, parent, data.timeLeft);
        }

        public override void OnEnter()
        {
            MaternityAISpawner.Instance.SpawnBaby(parent);
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WithBaby_Walking);
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_W_Baby);
            parent.GoTo(room.GetBedPosition(), PathType.Default);
        }

        public override void OnExit()
        {
            base.OnExit();
            parent.walkingStateManager.State = null;
            parent.abortPath();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.FinishedMoving:
                    parent.Person.State = GetNextStateOnLoad();
                    break;
                case StateNotifications.OfficeUnAnchored:
                    parent.PlayStandIdleAnimation();
                    parent.walkingStateManager.State = null;
                    parent.abortPath();
                    break;
                case StateNotifications.OfficeMoved:
                    break;
                case StateNotifications.OfficeAnchored:
                    if (parent.isMovementStopped())
                    {
                        parent.GoTo(room.GetBedPosition(), PathType.Default);
                    }
                    break;
                default:
                    break;
            }
        }

        public override MaternityIState GetNextStateOnLoad()
        {
            return new MaternityPatientBondingState(room, parent, data.timeLeft);
        }

    }

    public class MaternityPatientReturnToWaitingRoomStateParsedData
    {
        public float timeLeft;

        public MaternityPatientReturnToWaitingRoomStateParsedData(float timeLeft)
        {
            this.timeLeft = timeLeft;
        }
    }
}
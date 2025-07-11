using Hospital;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Maternity.PatientStates
{
    public class MaternityPatientGoToLaborRoomState : MaternityPatientBaseLaborState
    {
        public MaternityPatientGoToLaborRoomState(MaternityLabourRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent, timeLeft)
        {
            stateTag = MaternityPatientStateTag.GTLR;
        }

        public static MaternityPatientGoToLaborStateParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            string roomTag = strs[(int)INDEX.labourRoomTag];
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientGoToLaborStateParsedData(timeLeft, roomTag);
        }

        public static MaternityPatientGoToLaborRoomState GetInstance(MaternityPatientAI parent, MaternityPatientGoToLaborStateParsedData data)
        {
            return new MaternityPatientGoToLaborRoomState(MaternityLabourRoomController.Instance.Room(data.labourRoomTag), parent, data.timeLeft);
        }

        public override void OnEnter()
        {
            base.OnEnter();
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WOBaby_Walking);
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_WO_Baby);
            parent.GoTo(room.GetMachinePosition(), PathType.Default);
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
                    parent.PlayStandIdleAnimation();
                    parent.Person.State = GetNextStateOnLoad();
                    break;
                case StateNotifications.OfficeUnAnchored:
                    parent.PlayStandIdleAnimation();
                    parent.abortPath();
                    parent.walkingStateManager.State = null;
                    break;
                case StateNotifications.OfficeMoved:
                    // parent.UnCoverBed();
                    break;
                case StateNotifications.OfficeAnchored:
                    if (parent.isMovementStopped())
                    {
                        parent.GoTo(room.GetMachinePosition(), PathType.Default);
                    }
                    // parent.UnCoverBed();
                    break;
                default:
                    break;
            }
        }

        public override MaternityIState GetNextStateOnLoad()
        {
            return new MaternityPatientInLaborState(room, parent, data.timeLeft);
        }

    }

    public class MaternityPatientGoToLaborStateParsedData
    {
        public float timeLeft;
        public string labourRoomTag;

        public MaternityPatientGoToLaborStateParsedData(float timeLeft, string labourRoomTag)
        {
            this.timeLeft = timeLeft;
            this.labourRoomTag = labourRoomTag;
        }
    }
}
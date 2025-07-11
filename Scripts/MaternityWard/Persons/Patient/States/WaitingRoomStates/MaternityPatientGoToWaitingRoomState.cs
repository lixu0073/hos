using Hospital;
using IsoEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;

namespace Maternity.PatientStates
{
    public class MaternityPatientGoToWaitingRoomState : MaternityPatientBaseState<MaternityWaitingRoom>
    {

        public MaternityPatientGoToWaitingRoomState(MaternityWaitingRoom room, MaternityPatientAI parent) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.GTWR;
        }

        public override void OnEnter()
        {
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WOBaby_Walking);
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_WO_Baby);
            parent.GoTo(room.GetBedPosition(), PathType.GoReceptionPath);
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
                    parent.abortPath();
                    parent.walkingStateManager.State = null;
                    break;
                case StateNotifications.OfficeMoved:
                    // parent.UnCoverBed();
                    break;
                case StateNotifications.OfficeAnchored:
                    if (parent.isMovementStopped())
                    {
                        parent.GoTo(room.GetBedPosition(), PathType.Default);
                    }
                    // parent.UnCoverBed();
                    break;
                default:
                    break;
            }
        }

        public override MaternityIState GetNextStateOnLoad()
        {
            return new MaternityPatientWaitingForSendToDiagnoseState(room, parent);
        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.PatientOnWayIndicator(parent, room.bed);
        }

    }
}

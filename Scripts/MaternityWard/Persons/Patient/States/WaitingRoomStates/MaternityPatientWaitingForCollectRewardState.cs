using Hospital;
using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;

namespace Maternity.PatientStates
{
    public class MaternityPatientWaitingForCollectRewardState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWithBaby;
        public MaternityPatientWaitingForCollectRewardState(MaternityWaitingRoom room, MaternityPatientAI parent) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.WFCR;
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, false, false);
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.CollectRewardForLabor:
                    float timeToSpawn = MaternityCoreLoopParametersHolder.GetNextMotherSpawnDuration();
                    AddExpForGiftCollect();
                    parent.Person.State = new MaternityPatientGoingOutState(MaternityCoreLoopParametersHolder.GetMotherSpawnPosition(), timeToSpawn, parent, timeToSpawn);
                    room.SetupBed(false);
                    break;
                case StateNotifications.OfficeMoved:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, false);
                    break;
                case StateNotifications.OfficeAnchored:
                    parent.LayPatientInBed(defaultLayInBedAnimation);
                    parent.position = room.GetBedPosition();
                    room.SetupBed(true, false, false);
                    break;
                default:
                    break;
            }
        }

        public override void OnExit()
        {
        }

        private void AddExpForGiftCollect()
        {
            Game.Instance.gameState().AddResource(ResourceType.Exp, parent.GetInfoPatient().GetExpForStage(MaternityCharacterInfo.Stage.HealingAndBounding), EconomySource.MaternityHealingAndBonding, false);
        }

        public override MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return new MaternityWaitingRoomIndicatorsController.ClaimRewardForLaborIndicator(parent, room.bed);
        }

    }
}

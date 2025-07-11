using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Maternity.UI;
using SimpleUI;

namespace Maternity.PatientStates
{

    public enum MaternityPatientStateTag
    {
        NULL, //NULL
        GTWR, //Goint To Waiting Room
        WFSTD, // Waiting For Send To Diagnose
        GO, // Going Out
        WFC,// Waiting For Cures
        WFL, // Waiting For Labor
        RFL, // Ready For Labor
        IDQ, // In Diagnose Queue
        ID, // In Diagnose
        WFDR, // Waiting For Diagnose Results
        IL, // In Labor
        LF, // Labor Finished
        RTWR, // Return To Waiting Room
        B, // Bonding
        GTLR, // Go To Labor Room
        WFCR //Waiting For Collect Reward
    }

    public abstract class MaternityPatientBaseState<T> : MaternityIState where T : RotatableObject
    {
        protected MaternityPatientAI parent;
        protected T room;
        protected MaternityPatientStateTag stateTag;

        public MaternityPatientBaseState(T room, MaternityPatientAI parent)
        {
            this.parent = parent;
            this.room = room;
            parent.SetCurrentOccupiedRoom(room);
        }

        public RotatableObject GetRoom()
        {
            return room;
        }

        public virtual void Notify(int id, object parameters)
        {

        }

        public virtual void OnEnter()
        {

        }

        public virtual void OnExit()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual string SaveToString()
        {
            return stateTag.ToString() + "!" + room.Tag;
        }

        public virtual MaternityIState GetNextStateOnLoad()
        {
            return null;
        }

        public MaternityPatientStateTag GetTag()
        {
            return stateTag;
        }

        public virtual void EmulateTime(TimePassedObject timePassed) { }

        public virtual void BroadcastData()
        {
            Debug.LogError("Nothing To Broadcast");
        }

        public virtual MaternityWaitingRoomIndicatorsController.BaseIndicator GetWaitingRoomIndicator()
        {
            return null;
        }

        public virtual void MoveTo()
        {
            room.MoveCameraToThisRoom();
        }

        public static void OnChildCollect(MaternityPatientAI parent, MaternityLabourRoom room)
        {
            parent.Person.State = new MaternityPatientReturnToWaitingRoomState(room.GetWaitingRoom(), parent, parent.GetInfoPatient().MaxHealingAndBondingTime);
            room.MoveCameraToThisRoom();
            UIController.getMaternity.patientCardController.Exit();
            UIController.getMaternity.babyPopup.SetBabyView(
                parent.GetPatientAI().GetBabyInfo().AvatarHead,
                parent.GetPatientAI().GetBabyInfo().AvatarBody,
                parent.GetPatientAI().GetBabyInfo().Sex == 0 ? PatientAvatarUI.PatientBackgroundType.boyBaby : PatientAvatarUI.PatientBackgroundType.girlBaby
                );
            int expAmount = parent.GetInfoPatient().GetExpForStage(MaternityCharacterInfo.Stage.InLabor);
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expAmount, EconomySource.MaternityLabourEnded, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, new Vector3(-.1f, .75f, 0), expAmount, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expAmount, currentExpAmount);
            });
            room.DelayedCollectChildAnimations();
        }

        public static void OnSendToLabor(MaternityPatientAI parent)
        {
            int expAmount = parent.GetInfoPatient().GetExpForStage(MaternityCharacterInfo.Stage.WaitingForLabor);
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expAmount, EconomySource.MaternityReadyForLabor, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, new Vector3(-.1f, .75f, 0), expAmount, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expAmount, currentExpAmount);
            });
        }
    }
}
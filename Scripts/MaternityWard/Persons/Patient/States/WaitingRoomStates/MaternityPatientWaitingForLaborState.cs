using Hospital;
using System;
using System.Globalization;
using UnityEngine;

public enum ExerciseStateNotification
{
    roomAnanchored,
    roomUnanchored,
    roomMoved
}

namespace Maternity.PatientStates
{
    public class MaternityPatientWaitingForLaborState : MaternityPatientBaseState<MaternityWaitingRoom>
    {
        public Data data = new Data();
        public ExerciseDataHolder exerciseDataHolder;
        public SubStateManager<MaternityPatientWaitingForLaborState> exercisesStateManager;
        int defaultLayInBedAnimation = AnimHash.Mother_RestingWoBaby;
        //int exerciseCounter = 0;

        public MaternityPatientWaitingForLaborState(MaternityWaitingRoom room, MaternityPatientAI parent, float timeLeft) : base(room, parent)
        {
            stateTag = MaternityPatientStateTag.WFL;
            data.timeLeft = timeLeft;
        }

        public enum INDEX
        {
            timeLeft = 6
        }

        public static MaternityPatientWaitingForLabourParsedData Parse(string data)
        {
            string[] strs = data.Split('!');
            float timeLeft = float.Parse(strs[(int)INDEX.timeLeft], CultureInfo.InvariantCulture);
            return new MaternityPatientWaitingForLabourParsedData(timeLeft);
        }

        public static MaternityPatientWaitingForLaborState GetInstance(MaternityPatientAI parent, MaternityWaitingRoom room, MaternityPatientWaitingForLabourParsedData data)
        {
            return new MaternityPatientWaitingForLaborState(room, parent, data.timeLeft);
        }

        public override void OnEnter()
        {
            parent.LayPatientInBed(defaultLayInBedAnimation);
            parent.position = room.GetBedPosition();
            room.SetupBed(true, true);
            parent.SetDefaultLayInBedAnimation(AnimHash.Mother_RestingWoBaby);
            parent.SetDefaultStandIdleAnimation(AnimHash.Mother_StandIdle_WO_Baby);
            parent.SetDefaultWalkingAnimation(AnimHash.Mother_WOBaby_Walking);
            parent.PlayLayInBedAnimation();
            exercisesStateManager = new SubStateManager<MaternityPatientWaitingForLaborState>();
            exerciseDataHolder = new ExerciseDataHolder(room.GetStretchPosition, room.GetBallPosition, room.GetYogaPosition, parent.GetInfoPatient().MaxPreLabourTime);
            exercisesStateManager.State = exerciseDataHolder.GetExerciseDependingOnTime(data.timeLeft, this, parent);
        }

        public void SetNextExercise(ExerciseDataHolder.Exercises currentExercise)
        {
            int currentExerciseIndex = (int)currentExercise;
            int nextExerciseIndex = currentExerciseIndex + 1;
            if (nextExerciseIndex < Enum.GetValues(typeof(ExerciseDataHolder.Exercises)).Length)
                exercisesStateManager.State = exerciseDataHolder.CreateFullLenghtExerciseSubState((ExerciseDataHolder.Exercises)nextExerciseIndex, this, parent);
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            data.timeLeft -= timePassed.GetTimePassed();
            if (data.timeLeft <= 0)
                data.timeLeft = 0;

            exercisesStateManager.State = exerciseDataHolder.GetExerciseDependingOnTime(data.timeLeft, this, parent);
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            switch ((StateNotifications)id)
            {
                case StateNotifications.OfficeMoved:
                    exercisesStateManager.State.Notify((int)ExerciseStateNotification.roomMoved, null);
                    break;
                case StateNotifications.OfficeAnchored:
                    exercisesStateManager.State.Notify((int)ExerciseStateNotification.roomAnanchored, null);
                    room.SetupBed(true, true);
                    break;
                case StateNotifications.OfficeUnAnchored:
                    exercisesStateManager.State.Notify((int)ExerciseStateNotification.roomUnanchored, null);
                    room.SetupBed(true, true);
                    break;
                case StateNotifications.SpeedUpWaitingForLabour:
                    OnSendToLabor(parent);
                    parent.Person.State = new MaternityPatientGoToLaborRoomState(room.GetLabourRoom(), parent, parent.GetInfoPatient().MaxLabourTime);
                    break;
                default:
                    break;
            }
        }

        public override void OnUpdate()
        {
            data.timeLeft -= Time.deltaTime;
            if (data.timeLeft <= 0)
            {
                data.timeLeft = 0;
                BroadcastData();
                parent.Person.State = new MaternityPatientReadyForLaborState(room, parent);
                return;
            }
            BroadcastData();
            exercisesStateManager.Update();
        }

        public override void OnExit()
        {
            exercisesStateManager.State = null;
            exercisesStateManager = null;
        }

        public override string SaveToString()
        {
            return base.SaveToString() + "!" + data.timeLeft;
        }

        public override void BroadcastData()
        {
            parent.BroadcastData(data);
        }

        public class Data
        {
            public float timeLeft;
        }
    }

    public class MaternityPatientWaitingForLabourParsedData
    {
        public float timeLeft;

        public MaternityPatientWaitingForLabourParsedData(float timeLeft)
        {
            this.timeLeft = timeLeft;
        }
    }
}

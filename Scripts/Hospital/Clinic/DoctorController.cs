using UnityEngine;
using System.Collections.Generic;
using Hospital;
using IsoEngine;
using System;

public class DoctorController : BasePatientAI {

	public StateManager Doctor;
    public GameObject hearts;
    DoctorRoom room;
    public Vector3[] HeartsOffset;
    bool pause = false;

    public void Initialize(DoctorRoom room)
	{
		this.room = room;
		base.Initialize(new Vector2i(0,0));
		Doctor = new StateManager();
        SetWaitingForPatientState();

        if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && ReferenceHolder.GetHospital().globalEventController.GlobalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
            SetHeartsActive();
        else
            SetHeartsDeactive();

        AddListeners();
    }

    private void AddListeners()
    {
        GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
        GlobalEventNotificationCenter.Instance.OnEventStart.Notification += OnEventStart_Notification;

        GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
        GlobalEventNotificationCenter.Instance.OnEventEnd.Notification += OnEventEnd_Notification;
    }

    void OnDestroy()
    {
        GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
        GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
    }

    private void OnEventEnd_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
    {
        if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
            SetHeartsDeactive();
    }

    private void OnEventStart_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
    {
        if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
            SetHeartsActive();
    }

    protected override void Update()
	{
		if(pause)
		{
			return;
		}
		base.Update();
		Doctor.Update();
	}

	public override void Notify(int id, object parameters)
	{
		if (Doctor.State != null)
			Doctor.State.Notify(id, parameters);
	}

    public void SetWaitingForPatientState()
    {
        Doctor.State = new WaitingForPatientState(this);
    }

    public void SetCuringPatientState()
    {
        Doctor.State = new CuringPatientState(this);
    }


    public void SetHeartsTransform(Rotation rot)
    {
        if (hearts != null)
        {
            hearts.transform.localPosition = HeartsOffset[(int)rot];
        }
    }

    public void SetHeartsActive()
    {
        if (hearts == null)
        {
            var heartsCured = ResourcesHolder.GetHospital().ParticleHearts;

            if (heartsCured != null)
            {
                hearts = Instantiate(heartsCured, gameObject.transform);
            }
        }
    }

    public void SetHeartsDeactive()
    {
        if (hearts != null)
        {
            GameObject.Destroy(hearts);
            hearts = null;
        }
    }

    public class WaitingForPatientState : MainState
    {
        public WaitingForPatientState(DoctorController parent) : base(parent)
        {
        }

        public override void OnEnter()
        {
            parent.SetAnimationDirection();
            MoveToChairPos();
            RotateDoctor();
            try {
                parent.anim.Play(AnimHash.Sit_Doctor, 0, 0.0f);
                parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
    //Debug.LogError("DoctorController OnEnter WaitingForPatientState");
}

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)StateNotifications.OfficeUnAnchored)
            {
                try { 
                    parent.anim.Play(AnimHash.Sit_Doctor, 0, 0.0f);
                    parent.pause = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }

            if (id == (int)StateNotifications.OfficeMoved)
            {
                MoveToChairPos();
                RotateDoctor();
            }
            if (id == (int)StateNotifications.OfficeAnchored)
            {
                parent.Doctor.State = new WaitingForPatientState(parent);
                parent.pause = false;
            }
        }


        void MoveToChairPos()
        {
            int rot = (int)parent.room.actualData.rotation;
            //Debug.LogError("MoveToChairPos + " + parent.room.actualRotation);
            Vector3 roomRotPoint = new Vector3(parent.room.actualData.rotationPoint.x, 0, parent.room.actualData.rotationPoint.y);
            parent.transform.position = parent.room.transform.position + roomRotPoint + ((DoctorRoomInfo)parent.room.GetRoomInfo()).DocChairPos[rot];
        }
    }

    public class CuringPatientState : MainState
    {
        public CuringPatientState(DoctorController parent) : base(parent)
        {
        }

        public override void OnEnter()
        {
            //Debug.LogError("DoctorController OnEnter CuringPatientState");
            MoveToCurePos();
            RotateDoctor();
            try { 
                parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.notActive);
			    parent.GetComponent<IPersonCloudController> ().SetCloudMessageType (CloudsManager.MessageType.doctor);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)StateNotifications.OfficeUnAnchored)
            {
                try { 
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                    parent.pause = true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }

            if (id == (int)StateNotifications.OfficeMoved)
            {
                MoveToCurePos();
                RotateDoctor();
            }
            if (id == (int)StateNotifications.OfficeAnchored)
            {
                parent.Doctor.State = new CuringPatientState(parent);
                parent.pause = false;
            }
        }
        
        void MoveToCurePos()
        {
            int rot = (int)parent.room.actualData.rotation;
            Vector3 roomRotPoint = new Vector3(parent.room.actualData.rotationPoint.x, 0, parent.room.actualData.rotationPoint.y);
            parent.transform.position = parent.room.transform.position + roomRotPoint + ((DoctorRoomInfo)parent.room.GetRoomInfo()).DocIdlePos[rot];
        }
    }

    public class MainState : IState
	{
		protected DoctorController parent;

        public MainState(DoctorController parent)
		{
			this.parent = parent;
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
			return "";
		}
        
        public void RotateDoctor()
        {
            switch (parent.room.actualData.rotation)
            {
                case Rotation.North:
                    parent.isFront = false;
                    parent.isRight = true;
                    break;

                case Rotation.South:
                    parent.isFront = true;
                    parent.isRight = false;
                    break;

                case Rotation.East:
                    parent.isFront = true;
                    parent.isRight = true;
                    break;

                case Rotation.West:
                    parent.isFront = false;
                    parent.isRight = false;
                    break;
            }
            parent.SetAnimationDirection(parent.isFront, parent.isRight);
            parent.SetHeartsTransform(parent.room.actualData.rotation);

            //Debug.LogError("Rotated doctor. Actual room rot: " + parent.room.actualData.rotation);
        }
    }
}

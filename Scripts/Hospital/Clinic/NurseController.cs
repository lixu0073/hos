using UnityEngine;
using Hospital;
using IsoEngine;
using System;

public class NurseController : BasePatientAI
{
    public StateManager Nurse;
    public DiagnosticRoom room;
    public GameObject hearts;
#pragma warning disable 0649
    private Vector3 nursePos;
#pragma warning restore 0649

    public void Initialize(Vector2i pos, DiagnosticRoom room)
    {
        this.room = room;
        base.Initialize(pos);
        Nurse = new StateManager();
        //SetAnimationDirection(base.isFront, base.isRight);
        WanderAround();

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

    public override void Initialize(Vector2i pos)
    {
        base.Initialize(pos);
        Nurse = new StateManager();
    }

    protected override void Update()
    {
        if (pause)
            return;

        // Debug.LogWarning(""+anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
        base.Update();
        Nurse.Update();
    }

    public override void Notify(int id, object parameters)
    {
        if (Nurse.State != null)
            Nurse.State.Notify(id, parameters);
    }

    private void WanderAround()
    {
        Nurse.State = new WaitingState(this);
    }

    public void SetRotationRight(bool isRight, bool isFront)
    {
        base.isFront = isFront;
        base.isRight = isRight;

        base.SetAnimationDirection(base.isFront, base.isRight);
    }

    public void SetNurseRotation(DiagnosticRoom roomx, bool isWaiting)
    {
        if (roomx != null)
        {
            if ((roomx.actualRotation == Rotation.South) || (roomx.actualRotation == Rotation.North))
                SetRotationRight(true, isWaiting);
            else
                SetRotationRight(false, isWaiting);
        }
    }

    public void SetNurseRotation(bool isWaiting)
    {
        if (room != null)
        {
            if ((room.actualRotation == Rotation.South) || (room.actualRotation == Rotation.North))
                SetRotationRight(true, isWaiting);
            else 
                SetRotationRight(false, isWaiting);
        }
    }

    public void UpdateNursePosition(DiagnosticRoom room, Vector3 pos)
    {
        //Vector3 tmp = new Vector3(ResourcesHolder.Get().NursePaths[(int)room.actualData.rotation].StartingPosition.x, 0, ResourcesHolder.Get().NursePaths[(int)room.actualData.rotation].StartingPosition.y) + new Vector3(room.position.x, 0, room.position.y);
        //TeleportTo(tmp + nursePos);
        this.transform.position = pos;

        position = new Vector2i((int)pos.x, (int)pos.y);
    }

    public void SetHeartsActive()
    {
        if (hearts != null)
            hearts.SetActive(true);
    }

    public void SetHeartsDeactive()
    {
        if (hearts != null)
            hearts.SetActive(false);
    }

    private int index;
    private int startIndex;
    private int targetwaypoint;
    private PathInfo goPath;
    //private float runTime = 0f;
    private bool pause = false;

    public class StartMachineState : MainState
    {
        public StartMachineState(NurseController parent) : base(parent) { }

		public override void OnEnter()
		{
			parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.disabled);

            parent.SetNurseRotation(false);
            try
            { 
                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        public override void OnUpdate()
        {
            if (parent.pause)
                return;

            if (parent.room.currentPatient == null || ((parent.room.currentPatient != null) && parent.room.currentPatient.DoneHealing()))
            {
                // base.parent.SetAnimationDirection(base.parent.isFront, base.parent.isRight);
                parent.SetNurseRotation(false);
                try
                {
                    parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
                parent.Nurse.State = new WaitingState(parent);
                Debug.LogWarning(parent.Nurse.State.ToString() + " | " + parent.room.isHealing + " | " + parent.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            }

            base.OnUpdate();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            parent.SetNurseRotation(false);
            try
            {
                parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }

            if (id == (int)StateNotifications.OfficeMoved)
            {
                parent.walkingStateManager.State = null;                
                Debug.LogWarning(parent.Nurse.State.ToString() + " | " + parent.room.isHealing + " | " + parent.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            }

            if (id == (int)StateNotifications.OfficeUnAnchored)
            {                
                if (parent.gameObject.activeSelf) Debug.LogWarning(parent.Nurse.State.ToString() + " | " + parent.room.isHealing + " | " + parent.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
                parent.pause = true;
            }
        }
    }


    public class WaitingState : MainState
    {
        public WaitingState(NurseController parent) : base(parent) { }

        public override void OnEnter()
        {
			parent.GetComponent<IPersonCloudController> ().SetCloudState (CloudsManager.CloudState.notActive);
			parent.GetComponent<IPersonCloudController> ().SetCloudMessageType (CloudsManager.MessageType.nurse);

            parent.SetNurseRotation(true);
            try
            {
                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        public override void OnUpdate()
        {
            if (parent.room.isHealing && (parent.room.currentPatient != null) && (!parent.room.currentPatient.DoneHealing()))
            {
                parent.SetNurseRotation(false);
                try
                {
                    parent.anim.Play(AnimHash.Stand_Talk, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }

                parent.Nurse.State = new StartMachineState(parent);
                Debug.LogWarning(parent.Nurse.State.ToString() + " | " + parent.room.isHealing + " | " + parent.anim.GetCurrentAnimatorClipInfo(0)[0].clip.name);
            }

            base.OnUpdate();
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);

            parent.SetNurseRotation(true);
            try
            {
                parent.anim.Play(AnimHash.Stand_Idle, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }

            parent.pause |= id == (int)StateNotifications.OfficeUnAnchored;

            if (id == (int)StateNotifications.OfficeMoved)
            {
                Vector3 tmp = new Vector3(ResourcesHolder.GetHospital().NursePaths[(int)parent.room.actualData.rotation].StartingPosition.x, 0, ResourcesHolder.GetHospital().NursePaths[(int)parent.room.actualData.rotation].StartingPosition.y) + new Vector3(parent.room.position.x, 0, parent.room.position.y);
                parent.TeleportTo(tmp + parent.nursePos);
            }
            if (id == (int)StateNotifications.OfficeAnchored)
            {
                parent.Nurse.State = new WaitingState(parent);
                parent.pause = false;
            }
        }
    }

    public class MainState : IState
    {
        protected NurseController parent;

        public MainState(NurseController parent)
        {
            this.parent = parent;
        }

        public virtual void Notify(int id, object parameters) { }

        public virtual void OnEnter() { }

        public virtual void OnExit() { }

        public virtual void OnUpdate() { }

        public virtual string SaveToString()
        {
            return "";
        }
    }

    private enum StateStatus
    {
        FinishedMoving,
        OfficeUnAnchored,
        OfficeAnchored,
    }
}

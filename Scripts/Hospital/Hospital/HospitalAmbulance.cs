using System;
using System.Collections;

using UnityEngine;

public class HospitalAmbulance : MonoBehaviour
{
    #region Fields

    public event Action<HospitalAmbulance> ReachedDestination;

    [SerializeField]
    private Vector2 startPoint = Vector2.zero;

    [SerializeField]
    private Vector2 middlePoint = Vector2.zero;

    [SerializeField]
    private Vector2 endPoint = Vector2.zero;

    [SerializeField]
    private float drivingInSpeed = 0.0f;

    [SerializeField]
    private float drivingOutSpeed = 0.0f;


    [SerializeField]
    private float stopTime = 3.0f;

    [SerializeField]
    private Vector3 scale1 = Vector3.one;

    [SerializeField]
    private Vector3 scale2 = Vector3.one;

    [SerializeField]
    private GameObject ambulance = null;

    [SerializeField]
    private GameObject halo1 = null;

    [SerializeField]
    private GameObject halo2 = null;

    [SerializeField]
    private Animator ambulanceAnimator = null;

    private StateManager stateManager;

    #endregion

    #region Properties
    public bool IsBlockedByTutorial = true;

    public bool IsWaitingOut
    {
        get
        {
            return stateManager.State is WaitingOutState;
        }
    }

    public bool IsWaitingIn
    {
        get
        {
            return stateManager.State is WaitingInState;
        }
    }

    public bool CanDriveOut = false;

    #endregion

    #region Methods

    private void Start()
    {
        stateManager = new StateManager();
        stateManager.State = new WaitingOutState(this);
    }

    public void ResetAmbulance()
    {
        ReachedDestination = null;
        Start();
    }

    public void DriveIn()
    {
        Debug.LogWarning("DriveIn");
        if (!IsBlockedByTutorial)
            stateManager.State = new DrivingIn1State(this);

        if (GameState.Get().lastSpawnedPatientLevel < Game.Instance.gameState().GetHospitalLevel() && Game.Instance.gameState().GetHospitalLevel() >= 14 && TutorialController.Instance.CurrentTutorialStepTag == StepTag.diagnose_spawn)
        {
            //follow ambulance for tutorial                
            Debug.LogWarning("AMBULANCE COMING WITH DIAGNOSIS!");
            SoundsController.Instance.StartAmbulancev01();
            NotificationCenter.Instance.DiagnoseSpawn.Invoke(new BaseNotificationEventArgs());
        }
        else
        {
            Debug.LogWarning("We currently don't have a tutorial for patient to follow ambulance.");
            Debug.LogWarning("GameState.Get().lastSpawnedPatientLevel = " + GameState.Get().lastSpawnedPatientLevel);
            Debug.LogWarning("Game.Instance.gameState().GetHospitalLevel() = " + Game.Instance.gameState().GetHospitalLevel());
            Debug.LogWarning("TutorialController.Instance.CurrentTutorialStepTag = " + TutorialController.Instance.CurrentTutorialStepTag);

        }
    }

    public void DriveOut()
    {
        Debug.LogWarning("DriveOut");
        stateManager.State = new DrivingOut1State(this);
    }


    #region States

    private class WaitingOutState : BaseState<HospitalAmbulance>
    {
        public WaitingOutState(HospitalAmbulance parent)
            : base(parent)
        {

        }

        public override void OnEnter()
        {
            SoundsController.Instance.StopAmbulancev01();
            parent.ambulance.SetActive(false);
        }

        public override void OnExit()
        {
            parent.ambulance.SetActive(true);
        }
    }

    private class WaitingInState : BaseState<HospitalAmbulance>
    {
        private float timeLeft;

        public WaitingInState(HospitalAmbulance parent)
            : base(parent)
        {

        }

        public override void OnEnter()
        {
            parent.ambulance.transform.localPosition = parent.GetAmbulancePoint(parent.endPoint);
            try { 
            parent.ambulanceAnimator.Play(AnimHash.AmbulanceStop, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.StopAmbulancev01();

            parent.halo1.SetActive(false);
            parent.halo2.SetActive(false);

            timeLeft = parent.stopTime;
            parent.CanDriveOut = false;

            Debug.LogWarning("Ambulance reached hospital notification invoked!");
            NotificationCenter.Instance.AmbulanceReachedHospital.Invoke(new AmbulanceReachedHospitalEventArgs());

            if (parent.ReachedDestination != null)
                parent.ReachedDestination(parent);

        }

        public override void OnUpdate()
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0.0f)
            {
                parent.CanDriveOut = true;
                //parent.stateManager.State = new DrivingOut1State(parent);
            }
        }
    }

    private class DrivingIn1State : BaseState<HospitalAmbulance>
    {
        private float progress;

        public DrivingIn1State(HospitalAmbulance parent)
            : base(parent)
        {

        }

        public override void OnEnter()
        {
            progress = 0.0f;

            parent.ambulance.transform.localPosition = parent.GetAmbulancePoint(parent.startPoint);
            parent.ambulance.transform.localScale = parent.scale1;
            try { 
            parent.ambulanceAnimator.Play(AnimHash.AmbulanceDriveIn1, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.StartAmbulancev01();

            /* if (TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepTag == "follow_ambulace")
                 SoundsController.Instance.StartAmbulancev01();*/
        }

        public override void OnUpdate()
        {
            if (parent.IsBlockedByTutorial)
            {
                Debug.LogWarning("Ambulance blocked. Not moving");
                return;
            }

            parent.ambulance.transform.localPosition = Vector3.Lerp(parent.GetAmbulancePoint(parent.startPoint), parent.GetAmbulancePoint(parent.middlePoint), progress);

            progress += Time.deltaTime * parent.drivingInSpeed;

            if (progress > 1.0f)
            {
                parent.stateManager.State = new DrivingIn2State(parent);
            }
        }
    }

    private class DrivingIn2State : BaseState<HospitalAmbulance>
    {
        private float progress;
        bool animSet = false;

        public DrivingIn2State(HospitalAmbulance parent)
            : base(parent)
        {
            animSet = false;
        }

        public override void OnEnter()
        {
            progress = 0;
            parent.ambulance.transform.localPosition = parent.GetAmbulancePoint(parent.middlePoint);
            parent.ambulance.transform.localScale = parent.scale2;
            try { 
            parent.ambulanceAnimator.Play(AnimHash.AmbulanceDriveOut2, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.StopAmbulancev01();
        }

        public override void OnUpdate()
        {
            parent.ambulance.transform.localPosition = Vector3.Lerp(parent.GetAmbulancePoint(parent.middlePoint), parent.GetAmbulancePoint(parent.endPoint), progress);

            progress += Time.deltaTime * parent.drivingInSpeed * 3;

            if (!animSet && progress > .7f)
            {
                animSet = true;
                try { 
                    parent.ambulanceAnimator.Play(AnimHash.AmbulanceBreak, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }

            if (progress > 1.0f)
            {
                parent.stateManager.State = new WaitingInState(parent);
            }
        }
    }

    private class DrivingOut1State : BaseState<HospitalAmbulance>
    {
        //private float timeLeft;
        private float progress;

        public DrivingOut1State(HospitalAmbulance parent)
            : base(parent)
        {
        }

        public override void OnEnter()
        {
            progress = 0;
            try { 
                parent.ambulanceAnimator.Play(AnimHash.AmbulanceDriveOut2, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            parent.ambulance.transform.localPosition = parent.GetAmbulancePoint(parent.endPoint);
            parent.ambulance.transform.localScale = parent.scale2;
            try { 
                parent.ambulanceAnimator.Play(AnimHash.AmbulanceDriveIn1, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
            SoundsController.Instance.StopAmbulancev01();

        }

        public override void OnUpdate()
        {
            parent.ambulance.transform.localPosition = Vector3.Lerp(parent.GetAmbulancePoint(parent.endPoint), parent.GetAmbulancePoint(parent.middlePoint), progress);

            progress += Time.deltaTime * parent.drivingInSpeed * 3;

            if (progress > 1.0f)
            {
                parent.stateManager.State = new DrivingOut2State(parent);
            }
        }
    }

    private class DrivingOut2State : BaseState<HospitalAmbulance>
    {
        private float progress;

        public DrivingOut2State(HospitalAmbulance parent)
            : base(parent)
        {
        }

        public override void OnEnter()
        {
            progress = 0.0f;

            parent.ambulance.transform.localPosition = parent.GetAmbulancePoint(parent.middlePoint);
            parent.ambulance.transform.localScale = parent.scale1;
            try { 
                parent.ambulanceAnimator.Play(AnimHash.AmbulanceDriveOut2, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }

        public override void OnUpdate()
        {
            parent.ambulance.transform.localPosition = Vector3.Lerp(parent.GetAmbulancePoint(parent.middlePoint), parent.GetAmbulancePoint(parent.startPoint), progress);

            progress += Time.deltaTime * parent.drivingOutSpeed;

            if (progress > 1.0f)
            {
                parent.stateManager.State = new WaitingOutState(parent);
            }
        }
    }

    #endregion
    private Vector3 GetAmbulancePoint(Vector2 point)
    {
        return new Vector3(point.x, ambulance.transform.localPosition.y, point.y);
    }

    private void Update()
    {
        stateManager.Update();
    }


    public Transform GetAmbulanceTransform()
    {
        return ambulance.transform;
    }
    #endregion
}

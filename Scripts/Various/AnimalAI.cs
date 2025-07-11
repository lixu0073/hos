using UnityEngine;
using UnityEngine.EventSystems;

namespace Hospital
{
    public class AnimalAI : MonoBehaviour
    {
        [HideInInspector]
        public StateManager moveStateManager;
        public Animator anim;
        public float width = 10f, height = 5f;
        public float speed = 0.5f;

        public bool movingRight = false;
        public bool movingUP = true;
        public bool hasWaitState = true;
        public AnimaType animalType = AnimaType.Default;

        [HideInInspector]
        public Vector3 startPosition;

        [HideInInspector]
        public Vector3 currentPosition;
        private Vector3 firstMousePos;
        private float touchTime;
#pragma warning disable 0649
        [SerializeField]
        private GameObject hearts;
#pragma warning restore 0649
        /*
        public void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(width, height));
        }
        */

        void Start()
        {
            //kolider = gameObject.AddComponent<CapsuleCollider>();
            gameObject.AddComponent<EventTrigger>();

           // kolider.enabled = true;
           // kolider.radius = 1f;
           // kolider.height = 3f;

            moveStateManager = new StateManager();
            if (anim == null)
            {
                anim = gameObject.transform.GetComponent<Animator>();
            }
            startPosition = gameObject.transform.position;
            StartMovement();
        }

        public void Initialize()
        {
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

        void Update()
        {
            if(moveStateManager != null)
                moveStateManager.Update();
        }

        public void StopMovement()
        {
            if (moveStateManager.State is WalkingState)
                moveStateManager.State = null;
        }

        public void StartMovement()
        {
            if (moveStateManager.State == null)
                moveStateManager.State = new WalkingState(this);
        }

        private void UpdateAnimSide()
        {
            if (movingRight)
                transform.localScale = new Vector3(1, 1, 1);
            else
                transform.localScale = new Vector3(-1, 1, 1);
        }

        public void UpdateMovement(float scale = 1f)
        {
            if (movingRight)
                transform.position += new Vector3(0, 0, 1 * scale) * speed * Time.deltaTime;
            else
                transform.position += new Vector3(0, 0, -(1 * scale)) * speed * Time.deltaTime;

            if (!movingUP)
                transform.position += new Vector3(-(1 * scale), 0, 0) * speed * Time.deltaTime;
            else
                transform.position += new Vector3(1 * scale, 0, 0) * speed * Time.deltaTime;
        }

        public void UpdatePostionWhenCollide()
        {
            float rightEdgeOfFormation = startPosition.z - (0.5f * width);
            float leftEdgeOfFormation = startPosition.z + (0.5f * width);
            float topEdgeOfFormation = startPosition.x + (0.5f * height);
            float bottomEdgeOfFormation = startPosition.x - (0.5f * height);

            if (leftEdgeOfFormation < transform.position.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, leftEdgeOfFormation);
            else if (rightEdgeOfFormation > transform.position.z)
                transform.position = new Vector3(transform.position.x, transform.position.y, rightEdgeOfFormation);

            if (bottomEdgeOfFormation > transform.position.x)
                transform.position = new Vector3(bottomEdgeOfFormation,transform.position.y, transform.position.z);
            else if (topEdgeOfFormation < transform.position.x)
                transform.position = new Vector3(topEdgeOfFormation, transform.position.y, transform.position.z);

            UpdateMovement(2);
        }

        public void OnMouseUp()
        {
            if (!IsoEngine.BaseCameraController.IsPointerOverInterface())
                if ((Input.mousePosition - firstMousePos).magnitude < 10.0f)// && Time.time - touchTime < 0.5f)
                {
                    if (moveStateManager.State == null || (moveStateManager.State != null && moveStateManager.State is WaitingState) || (animalType == AnimaType.Deer))
                    {
					    if (animalType == AnimaType.Deer)
                        {
						    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke (new DailyQuestProgressEventArgs (1, DailyTask.DailyTaskType.TapTheDear));
						    SoundsController.Instance.PlayDeer ();

                            anim.SetTrigger("skip");

					    }
                        else if (animalType == AnimaType.BlueBird)
                        {
						    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke (new DailyQuestProgressEventArgs (1, DailyTask.DailyTaskType.BlueBirdHunting));
						    SoundsController.Instance.PlayBird ();
					    }
                        moveStateManager.State = new WalkingState(this);
                    }
                }
            firstMousePos = Vector3.zero;
        }

        public void OnMouseDown()
        {
            touchTime = Time.time;
            firstMousePos = Input.mousePosition;
        }

        public void SetHeartsActive()
        {
            if (hearts != null)
            {
                hearts.SetActive(true);
            }
        }

        public void SetHeartsDeactive()
        {
            if (hearts != null)
            {
                hearts.SetActive(false);
            }
        }
        #region States

        public class WalkingState : IState
        {
            protected AnimalAI parent;
            //private float travelValue = 0;

            float TimeLeft = 4;

            public WalkingState(AnimalAI parent)
            {
                this.parent = parent;
            }

            public string SaveToString()
            {
                return "";
            }

            public void OnEnter()
            {
                string toCheck = "walking";
                if (IsoEngine.Utils.DoesAnimatorParamExists(parent.anim, toCheck))
                {
                    parent.anim.SetTrigger("walking");
                }
                else
                {
                    Debug.Log("Parameter not found");
                }
                TimeLeft = BaseGameState.RandomNumber(3, 8);
            }

            public void Notify(int id, object parameters) { }

            public void OnUpdate()
            {
                TimeLeft -= Time.deltaTime;

                if (TimeLeft > 0 || parent.hasWaitState == false)
                {
                    // Check if the formation is going outside the playspace...
                    if (Mathf.Abs(parent.startPosition.z - parent.transform.position.z) > 0.5f * parent.width)
                    {
                        parent.movingRight = !parent.movingRight;
                        parent.UpdatePostionWhenCollide();

                        if (parent.hasWaitState == true)
                            parent.moveStateManager.State = new WaitingState(parent);
                    }
                    if (Mathf.Abs(parent.startPosition.x - parent.transform.position.x) > 0.5f * parent.height)
                    {
                        parent.movingUP = !parent.movingUP;
                        parent.UpdatePostionWhenCollide();

                        if (parent.hasWaitState == true)
                            parent.moveStateManager.State = new WaitingState(parent);
                    }
                    parent.UpdateMovement();
                }
                else
                {
                    parent.moveStateManager.State = new WaitingState(parent);
                }

                parent.UpdateAnimSide();

            }

            public void OnExit() { }

        }

        public class WaitingState : IState
        {

            protected AnimalAI parent;

            float TimeLeft = 4;

            public WaitingState(AnimalAI parent)
            {
                this.parent = parent;
            }

            public string SaveToString()
            {
                return "";
            }

            public void OnEnter()
            {
                TimeLeft = BaseGameState.RandomNumber(3, 8);
                parent.anim.SetTrigger("idle");
            }

            public void Notify(int id, object parameters) { }

            public void OnUpdate()
            {
                TimeLeft -= Time.deltaTime;

                if (TimeLeft < 0)
                {
                    parent.moveStateManager.State = new WalkingState(parent);
                }
            }

            public void OnExit()
            {
                //parent.UpdateMovement();
            }
        }

        public enum AnimaType
        {
            Default,
            Deer,
            BlueBird,
            Bird,
            Butterfiles, 
        }
        #endregion States
    }
}

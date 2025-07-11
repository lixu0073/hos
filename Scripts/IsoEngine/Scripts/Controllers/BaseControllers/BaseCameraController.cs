using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using MovementEffects;

namespace IsoEngine
{
    public class LimitedQueue<T> : Queue<T>
    {
        private int limit = -1;

        public int Limit
        {
            get { return limit; }
            set { limit = value; }
        }

        public LimitedQueue(int limit) : base(limit)
        {
            this.Limit = limit;
        }

        public new void Enqueue(T item)
        {
            if (this.Count >= this.Limit)
            {
                this.Dequeue();
            }
            base.Enqueue(item);
        }
    }

    ///<summary>
    ///Base Class to control camera movement. 
    ///</summary>
    public abstract class BaseCameraController : ComponentController
    {
        [Serializable]
        public class CameraSettings
        {
            public bool useAxesToMove;
            public bool ZoomEnabled = true;

            public float ZoomSpeed = 1.0f;
            public bool InverseZoom;
            public bool ZoomToPoint;

            public bool MoveEnabled = true;
            public bool MoveOnEdgeEnabled = false;
            public int EdgeMargin = 5;
            public bool SmoothSwypeEnabled = true;

            public bool RotateEnabled = true;
            public float RotateSpeed = 1.0f;
            public bool RotateAroundMousePoint = false;
            public bool RestrictedRotating = false;
            public float Angle = 45;
            public float MaximumDeviation = 15;
            public float ReturnTime = 1.0f;
            public bool PCRotateScreenSplit = false;
            public bool DraggingEnabled = true;
            public float smoothSwypeDumpTime = 1;
            public float MaxDistance = 40;
            public float FollowingSpeed = 0.6f;

            public float margin = 0.1f;
            public float marginSpeed = 20.0f;
        }

        #region GENERAL-PUBLIC
#pragma warning disable 0649
        [SerializeField] private CameraSettings mobileSettings;
        [SerializeField] private CameraSettings PCSettings;
        public CameraSettings Settings { get; private set; }
        [SerializeField] private bool blockUserInput;
#pragma warning restore 0649

        private int drivenByCoroutine = 0;
        private bool oldValue;
        public AnimationCurve moveCurve;

        public static event Hospital.OnMovementEnded OnCameraMovementFinished;

        public bool BlockUserInput
        {
            get
            {
                return blockUserInput;
            }
            set
            {
                if (drivenByCoroutine > 0)
                {
                    oldValue = value;
                    return;
                }
                blockUserInput = value;
                rotating = false;
                dragging = false;
            }
        }

        /// <summary>
        /// This point should always be in the center of the screen.
        /// </summary>
        [SerializeField] private Vector3 lookingAt = Vector3.zero;

        /// <summary>
        /// This point is always in the center of the screen.
        /// </summary>
        public Vector3 LookingAt
        {
            get { return lookingAt; }
            private set { MoveToPoint(value); }
        }
        private Vector3 Offset = new Vector3(0, 15, 15);
        public Vector3 OffsetInAngles;
        public float distanceFromPoint = 40;
        [NonSerialized] public float MinZoom = 1.0f;
        [NonSerialized] public float MaxZoom = 10.0f;
        [NonSerialized] public float defaultZoom = 7f;
        //private float smoothMoveStep = 0.3f;
        public bool MoveRestrictionEnabled = false;
        public Vector2 MoveRestrictionTo = new Vector2(100, 100);
        public Vector2 MoveRestrictionFrom = new Vector2(0, 0);
        public GameObject restrictionReferencePoint;
        #endregion

        #region GENERAL-PRIVATE
        private Camera cameras;
        private Plane raycastPlane;
        #endregion

        #region Coroutines
        private bool ZommCoroutineStarted;
        private bool SmoothMoveCoroutineStarted;
        private bool SmoothRotateCoroutineStarted;
        private bool SmoothSwypeCoroutineStarted;

        private bool startedMinZoomOut = false;
        private bool startedMaxZoomOut = false;

        public bool CameraMovementStoped = false;
        #endregion

        public float MoveSpeed = 10;

        #region privates
        private Vector3 firstTouchPosition;
        private Vector3 prevPrevPos;
        private Vector3 prevPos;
        public bool dragging = false;
        private Vector2 previousMouseScreenPosition = Vector2.zero;
        public Vector2 actualMouseScreenPosition = Vector2.zero;
        public Vector3 actualMouseWorldPosition = Vector3.zero;
        private Vector3 previousMouseWorldPosition = Vector3.zero;
        private Vector2 firstMouseScreenPosition = Vector2.zero;
        private int previousTouchCount;
        private Vector2 previousTouchOne = Vector2.zero;
        private Vector2 previousTouchTwo = Vector2.zero;
        private float lastDoubleTouchTime;
        private float lastTouchTime;
        private float doubleTapDelay = 0.0f;
        private Vector3 cameraVelocity;
        private float tempFloat;
        private bool rotating;
        private bool SmoothRotateAroundCoroutineStarted;
        private int minMarginX, maxMarginX, minMarginY, maxMarginY;
        private bool firstRun = true;

        LimitedQueue<Vector2> previousPositionsQueue = new LimitedQueue<Vector2>(3);
        #endregion

        public bool IsAndroid()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
# endif
        }

        #region Initialization 
        internal override void Initialize()
        {
            Input.simulateMouseWithTouches = true;
            base.Initialize();

            if (ExtendedCanvasScaler.isPhone())
            {
                if (IsAndroid())
                {
                    MinZoom = 4.75f;
                    MaxZoom = 8.25f;
                    defaultZoom = 6.75f;
                }
                else
                {
#if UNITY_IOS
                    if ((int)UnityEngine.iOS.Device.generation < (int)UnityEngine.iOS.DeviceGeneration.iPadAir2)
                    {
                        MinZoom = 4.75f;
                        MaxZoom = 8.25f;
                        defaultZoom = 6.75f;
                    }
                    else
                    {
                        MinZoom = 4.75f;
                        MaxZoom = 12f;
                        defaultZoom = 8;
                    }
#else
                    MinZoom = 4.75f;
                    MaxZoom = 12f;
                    defaultZoom = 8;
#endif
                }
            }
            else
            {
                if (IsAndroid())
                {
                    MinZoom = 5.5f;
                    MaxZoom = 9f;
                    defaultZoom = 7;
                }
                else
                {
                    MinZoom = 6f;
                    MaxZoom = 14f;
                    defaultZoom = 10;
                }
            }

#if UNITY_STANDALONE || UNITY_EDITOR
            Settings = PCSettings;
#elif UNITY_IOS || UNITY_ANDROID
			Settings = mobileSettings;
#endif
            raycastPlane = new Plane(Vector3.up, Vector3.zero);

            Vector3 temp = new Vector3(0, 0, distanceFromPoint);
            temp = Quaternion.Euler(-OffsetInAngles.x, OffsetInAngles.y, OffsetInAngles.z) * temp;
            Offset = temp;
            transform.position = LookingAt - Offset;
            transform.Translate(0, Offset.y * 2, 0, Space.World);
            transform.LookAt(LookingAt);
            cameras = gameObject.GetComponent<Camera>();
            cameras.orthographicSize = defaultZoom;

            minMarginX = (int)(Screen.width * Settings.margin);
            minMarginY = minMarginX;
            maxMarginX = Screen.width - minMarginX;
            maxMarginY = Screen.height - minMarginY;
        }

        public void ResetCamera()
        {
            LookingAt = new Vector3(29.8f, 0, 40.5f);
            cameras.orthographicSize = 14f;
        }

        public void ResetMaternityCamera()
        {
            LookingAt = new Vector3(55f, 0, 50.5f);
            cameras.orthographicSize = 7.5f;
        }
        #endregion

        #region Utils
        public Vector3 RayCast(Vector2i position)
        {
            return RayCast(new Vector2(position.x, position.y));
        }
        public Vector3 RayCast(Vector2 position)
        {
            Ray ray = cameras.ScreenPointToRay(position);
            raycastPlane.Raycast(ray, out tempFloat);
            return ray.GetPoint(tempFloat);
        }
        #endregion

        public static bool IsPointerOverInterface()
        {
            if (Input.touchCount == 0)
                return EventSystem.current.IsPointerOverGameObject();
            if (Input.touchCount == 1 || Input.GetTouch(0).phase != TouchPhase.Ended)
                return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(1).fingerId);
        }

        bool WasSecondTouch = false;

        private void Update()
        {
            int touchCount = Input.touchCount;
            Vector3 mosueInputPos = Input.mousePosition;

            if (touchCount != previousTouchCount)
            {
                if (startedMinZoomOut || startedMaxZoomOut)
                {
                    if (ZommCoroutineStarted)
                    {
                        Timing.KillCoroutine(SmoothZoomer().GetType());
                        ZommCoroutineStarted = false;
                    }
                    startedMinZoomOut = false;
                    startedMaxZoomOut = false;
                }
                if (touchCount > previousTouchCount)
                    if (ZommCoroutineStarted)
                        Timing.KillCoroutine(SmoothZoomer().GetType());
                previousMouseScreenPosition = mosueInputPos;
                if (touchCount > 0)
                    previousTouchOne = Input.GetTouch(0).position;
                if (touchCount > 1)
                    previousTouchTwo = Input.GetTouch(1).position;
            }
            actualMouseScreenPosition = mosueInputPos;
            actualMouseWorldPosition = RayCast(actualMouseScreenPosition);
            previousMouseWorldPosition = RayCast(previousMouseScreenPosition);

            if (touchCount < 2)
            {
                //SimpleUI.ReferenceHolder.holder.giftSystem.MakeParticle(0, Vector2.zero, Vector2.one, 1);
                if (!blockUserInput && !followingGo)
                    if (!WasSecondTouch)
                        MouseMoved();
                    else
                    {
                        previousPositionsQueue.Clear();
                        WasSecondTouch = false;
                    }

                if (!IsPointerOverInterface())
                {
                    if (!blockUserInput && !followingGo)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            if ((Time.time - lastTouchTime) < doubleTapDelay)
                            {
                                SmoothZoom(MinZoom + 1, 0.5f, false);
                                SmoothMoveToPoint(actualMouseWorldPosition, 0.5f, false);
                            }
                            else
                            {
                                LeftMouseDown();
                                lastTouchTime = Time.time;
                            }
                        }
                        if (Input.GetMouseButtonDown(1))
                            RightMouseDown();
                        if (0.0f != (tempFloat = Input.GetAxis("Mouse ScrollWheel")))
                        {
                            startedMinZoomOut = false;
                            startedMaxZoomOut = false;
                            if (ZommCoroutineStarted)
                                Timing.KillCoroutine(SmoothZoomer().GetType());
                            if (!Settings.ZoomToPoint)
                                ChangeZoom(tempFloat);
                            else
                                SetZoomArountPoint((1 - tempFloat), actualMouseWorldPosition);
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    if (!blockUserInput && !followingGo)
                    {
                        firstMouseScreenPosition = actualMouseScreenPosition;
                        lastTouchTime = Time.time;
                        shouldDrag = false;
                        dragging = false;
                        cameraVelocity = Vector3.zero;
                    }
                }

                if (!blockUserInput && !followingGo)
                {
                    if (Settings.useAxesToMove)
                        CheckAxes();

                    if (Input.GetMouseButtonUp(0))
                        LeftMouseUp();
                    if (Input.GetMouseButtonUp(1))
                        RightMouseUp();
                }

            }
            else if (touchCount == 2)
            {
                WasSecondTouch = true;

                if ((Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved))//&&(previousTouchOne!=Input.GetTouch(0).position && previousTouchTwo!=Input.GetTouch(1).position))
                {
                    var touchOne = RayCast(Input.GetTouch(0).position);
                    var touchTwo = RayCast(Input.GetTouch(1).position);
                    var prevOne = RayCast(previousTouchOne);
                    var prevTwo = RayCast(previousTouchTwo);

                    if (!blockUserInput && !followingGo && !IsPointerOverInterface())
                        OnDoubleSwype(prevOne, touchOne, prevTwo, touchTwo);


                    previousTouchOne = Input.GetTouch(0).position;
                    previousTouchTwo = Input.GetTouch(1).position;
                }
                //dragging = false;
            }
            else if (touchCount >= 3) // In QA builds activates the TestObject that shows Debug options
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began && BaseGameState.testObjects)
                    BaseGameState.testObjects.SetActive(!BaseGameState.testObjects.activeSelf);
            }

            // Debug options triggered from keyboard pressing D
            if (Input.GetKeyDown(KeyCode.D))
            {
                if (BaseGameState.testObjects)
                    BaseGameState.testObjects.SetActive(!BaseGameState.testObjects.activeSelf);
            }

            if (previousPositionsQueue.Count != 0)
            {
                previousMouseScreenPosition = previousPositionsQueue.FirstOrDefault();
            }
            else
            {
                previousMouseScreenPosition = actualMouseScreenPosition;
            }

            previousPositionsQueue.Enqueue(actualMouseScreenPosition);
            previousTouchCount = touchCount;

            if (Settings.MoveOnEdgeEnabled && !dragging && !blockUserInput && !followingGo)
                CheckScreenEdge();
            if (followingGo && followedTransform != null)
            {
                followingTime += Time.deltaTime;

                float followSpeed = Mathf.Clamp(followingTime / 25f, 0, .15f);

                //Vector3 p = lookingAt;
                Vector3 move = Vector3.Lerp(lookingAt, followedTransform.position, followSpeed);
                move.y = 0;
                LookingAt = move;
            }

            if (Settings.RestrictedRotating)
            {
                CheckRotationConstraints();
            }
            if (touchCount < 1 && !Input.GetMouseButton(1))
            {
                if (cameras.orthographicSize < MinZoom + 0.5f && !startedMinZoomOut)
                {
                    startedMinZoomOut = true;
                    SmoothZoom(MinZoom + 0.5f, 1.5f, false);
                }
                if (cameras.orthographicSize > MaxZoom - 1f && !startedMaxZoomOut)
                {

                    //cameras.orthographicSize = MaxZoom - 0.5f;
                    startedMaxZoomOut = true;
                    if (firstRun && ExtendedCanvasScaler.isPhone())
                    {
                        firstRun = false;
                        SmoothZoom(MinZoom, 1.5f, false);
                    }
                    else
                    {
                        SmoothZoom(MaxZoom - 1, 1.5f, false);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (MoveRestrictionEnabled)
                CheckConstraints();
        }

        private void CheckAxes()
        {
            Vector3 move = new Vector3(0, 0, 0);
            move.x -= Input.GetAxis("Horizontal");
            move.z -= Input.GetAxis("Vertical");
            move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * move;
            move.Normalize();
            move *= MoveSpeed * Time.deltaTime * cameras.orthographicSize / 5.0f;
            transform.position -= move;
            lookingAt -= move;
        }

        public Camera GetCamera()
        {
            return cameras;
        }

        #region Constraints
        private void CheckRotationConstraints()
        {
            var p = gameObject.transform.eulerAngles.y;
            float r = 0;
            if (p > Settings.Angle + Settings.MaximumDeviation)
                r = p - (Settings.Angle + Settings.MaximumDeviation);
            if (p < Settings.Angle - Settings.MaximumDeviation)
                r = p - (Settings.Angle - Settings.MaximumDeviation);
            if (r != 0)
                Rotate(-r);

        }
        void CheckScreenEdge()
        {
            if (Settings.MoveEnabled)
            {
                Vector3 move = new Vector3(0, 0, 0);
                if (actualMouseScreenPosition.x < 0 + Settings.EdgeMargin)
                    move.x += 1;
                if (actualMouseScreenPosition.x > Screen.width - Settings.EdgeMargin)
                    move.x -= 1;
                if (actualMouseScreenPosition.y < 0 + Settings.EdgeMargin)
                    move.z += 1;
                if (actualMouseScreenPosition.y > Screen.height - Settings.EdgeMargin)
                    move.z -= 1;
                //print(move);
                if (move.z == 0 && move.x == 0)
                    return;
                //print("moved");
                move = Quaternion.Euler(0, transform.eulerAngles.y, 0) * move;
                move.Normalize();
                move *= MoveSpeed * Time.deltaTime * cameras.orthographicSize / 5.0f;
                transform.position -= move;
                lookingAt -= move;
            }
        }
        #endregion

        #region Input Delegates
        private void RightMouseUp()
        {
            rotating = false;
            SmoothRotation((Settings.Angle - gameObject.transform.eulerAngles.y), Settings.ReturnTime, false);
        }

        private void LeftMouseUp()
        {
            if (dragging && Input.touchCount <= 1)
            {
                dragging = false;

                if (Settings.SmoothSwypeEnabled && (firstMouseScreenPosition - actualMouseScreenPosition).magnitude > 0.1f)
                {
                    if (SmoothSwypeCoroutineStarted)
                        Timing.KillCoroutine(SmoothSwype().GetType());
                    Timing.RunCoroutine(SmoothSwype());
                }
            }
        }

        private void RightMouseDown()
        {
            if (SmoothRotateCoroutineStarted)
            {
                SmoothRotateCoroutineStarted = false;
                Timing.KillCoroutine(SmoothRotate().GetType());
                if (inputDrivenByRotate)
                {
                    inputDrivenByRotate = false;
                    BlockUserByCoroutine(false);
                }
            }
            if (Settings.RotateEnabled)
            {
                rotating = true;
            }
        }

        private void LeftMouseDown()
        {
            if (IsPointerOverInterface())
                return;
            firstMouseScreenPosition = actualMouseScreenPosition;
            if (SmoothSwypeCoroutineStarted)
            {
                Timing.KillCoroutine(SmoothSwype().GetType());

                SmoothSwypeCoroutineStarted = false;
            }
            if (SmoothMoveCoroutineStarted)
            {
                SmoothMoveCoroutineStarted = false;
                if (inputDrivenByMove)
                {
                    BlockUserByCoroutine(false);
                    inputDrivenByMove = false;
                }
                Timing.KillCoroutine(SmoothSwype().GetType());

            }
            if (Settings.DraggingEnabled)
            {
                shouldDrag = false;
                dragging = true;
                cameraVelocity = Vector3.zero;
            }
        }

        bool shouldDrag;
        private void MouseMoved()
        {
            if (Settings.DraggingEnabled && dragging)
            {
                if (!shouldDrag)
                {
                    //Mikko: those positions seem to work fishy on mobile devices...
                    if ((previousMouseScreenPosition - firstMouseScreenPosition).sqrMagnitude > (Screen.width ^ 2 + Screen.height ^ 2) / 40)
                    {
                        if (Hospital.AreaMapController.Map != null)
                            Hospital.AreaMapController.Map.OnTileTouch(Vector2i.zero);
                        shouldDrag = true;
                        previousPositionsQueue.Clear();
                        if (previousPositionsQueue.Count != 0)
                        {
                            previousMouseScreenPosition = previousPositionsQueue.FirstOrDefault();
                        }
                        else
                        {
                            previousMouseScreenPosition = actualMouseScreenPosition;
                        }

                        previousPositionsQueue.Enqueue(actualMouseScreenPosition);
                    }
                }
                else
                {
                    var p = (previousMouseWorldPosition - actualMouseWorldPosition) * 0.3f;
                    p.y = 0;
                    Move(p);
                    cameraVelocity = p / Time.deltaTime;
                }
            }
            if (rotating)
            {

                if (!Settings.RestrictedRotating)
                    Rotate(((actualMouseScreenPosition.x - previousMouseScreenPosition.x) * Settings.RotateSpeed / 5) * (Settings.PCRotateScreenSplit ? (actualMouseScreenPosition.y > Screen.height / 2 ? -1 : 1) : 1));
                else
                    RestrictedRotate(((actualMouseScreenPosition.x - previousMouseScreenPosition.x) * Settings.RotateSpeed / 5) * (Settings.PCRotateScreenSplit ? (actualMouseScreenPosition.y > Screen.height / 2 ? -1 : 1) : 1));
            }
        }

        void OnDoubleSwype(Vector3 prevFirst, Vector3 actFirst, Vector3 prevSecond, Vector3 actSecond)
        {
            var angle = (Mathf.Atan2(actFirst.z - actSecond.z, actFirst.x - actSecond.x)
                                         - Mathf.Atan2(prevFirst.z - prevSecond.z, prevFirst.x - prevSecond.x)) * Mathf.Rad2Deg;
            if (angle > 45)
                angle -= 180;
            if (angle < -45)
                angle += 180;
            float ratio = ((prevFirst - prevSecond).magnitude) / ((actFirst - actSecond).magnitude);

            var point = (prevFirst + prevSecond) / 2 - (actFirst + actSecond) / 2;
            Move(point);

            if (Settings.RotateEnabled)
            {
                if (!Settings.RotateAroundMousePoint)
                {
                    if (!Settings.RestrictedRotating)
                        Rotate(angle);
                    else
                    {
                        RestrictedRotate(angle);
                        SmoothRotation((Settings.Angle - gameObject.transform.eulerAngles.y), Settings.ReturnTime, false);
                    }
                }
                else
                {
                    if (!Settings.RestrictedRotating)
                        RotateAroundPoint(angle, (actFirst + actSecond) / 2);
                    else
                    {
                        RestrictedRotateAroundPoint(angle, (actFirst + actSecond) / 2);
                        SmoothRotationAroundPoint((Settings.Angle - gameObject.transform.eulerAngles.y), Settings.ReturnTime, (actFirst + actSecond) / 2, false);
                    }
                }

            }
            if (Settings.ZoomEnabled)
            {
                if (!Settings.RotateAroundMousePoint)
                    SetZoom(cameras.orthographicSize * ratio);
                else
                    SetZoomArountPoint(ratio, (actFirst + actSecond) / 2);
            }
        }
        #endregion

        #region ZOOMING
        /// <summary>
        /// Force camera to change zoom by provided value. Checks zoom constraints.
        /// </summary>
        /// <param name="p"></param>
        public void ChangeZoom(float p)
        {
            if (Settings.ZoomEnabled)
            {
                cameras.orthographicSize += p * Settings.ZoomSpeed * (Settings.InverseZoom ? 1 : -1);
                cameras.orthographicSize = Mathf.Clamp(cameras.orthographicSize, MinZoom, MaxZoom);
            }
        }

        public void SetZoomArountPoint(float ratio, Vector3 point)
        {
            if (Settings.ZoomEnabled && !((ratio > 1 && cameras.orthographicSize == MaxZoom) || (ratio < 1 && cameras.orthographicSize == MinZoom)))
            {
                ratio = Mathf.Clamp(ratio, MinZoom / cameras.orthographicSize, MaxZoom / cameras.orthographicSize);
                SetZoom(ratio * cameras.orthographicSize);

                Move((lookingAt - point) * (ratio - 1));
            }
        }

        private void BlockUserByCoroutine(bool value)
        {
            if (value)
            {
                if (drivenByCoroutine < 1)
                {
                    oldValue = blockUserInput;
                    blockUserInput = true;
                }
                drivenByCoroutine += 1;
            }
            else
            {
                drivenByCoroutine -= 1;

                if (drivenByCoroutine < 1)
                    BlockUserInput = oldValue;
            }
        }

        /// <summary>
        /// Explicitly sets camera zoom to provided value. Checks zoom constraints.
        /// </summary>
        /// <param name="p"></param>
        public void SetZoom(float p)
        {
            //Debug.Log("SetZoom");
            if (Settings.ZoomEnabled)
                cameras.orthographicSize = Mathf.Clamp(p, MinZoom, MaxZoom);
        }

        bool inputDrivenByZoom = false;
        IEnumerator<float> smoothmove1;
        /// <summary>
        /// Camera will zoom for declared time to achieve specified value.
        /// </summary>
        /// <param name="destination">ending zoom will have this value if constraints allow that</param>
        /// <param name="time">time of zooming</param>
        public void SmoothZoom(float destination, float time, bool BlockUser)
        {
            if (!Settings.ZoomEnabled)
                return;
            if (ZommCoroutineStarted)
            {
                Timing.KillCoroutine(SmoothZoomer().GetType());
                if (inputDrivenByZoom)
                {
                    BlockUserByCoroutine(false);
                    inputDrivenByZoom = false;
                }
            }
            object[] param = new object[3] { destination, time, BlockUser };
            //StartCoroutine("SmoothZoomer", param);
            smoothmove1 = Timing.RunCoroutine(SmoothZoomer(param));
        }

        public IEnumerator<float> SmoothZoomer(object[] SmoothZoomParams = null)
        {
            float time = (float)SmoothZoomParams[1];
            float startTime = Time.time;
            float dest = (float)SmoothZoomParams[0];
            bool blockUser = (bool)SmoothZoomParams[2];
            ZommCoroutineStarted = true;
            if (blockUser)
            {
                inputDrivenByZoom = true;
                BlockUserByCoroutine(true);
            }

            float startSize = cameras.orthographicSize;
            bool finished = false;

            while (!finished)
            {
                float t = (Time.time - startTime) / time;
                float percent = moveCurve.Evaluate(t);

                float newSize = startSize + ((dest - startSize) * percent);

                cameras.orthographicSize = newSize;

                if (Time.time - startTime > time)
                    finished = true;

                yield return 0f;
            }
            startedMinZoomOut = false;
            startedMaxZoomOut = false;
            SetZoom(dest);
            if (blockUser || drivenByCoroutine > 0)
            {
                inputDrivenByZoom = false;
                BlockUserByCoroutine(false);
            }
            ZommCoroutineStarted = false;
            yield return 0f;
        }

        public float GetZoom()
        {
            return cameras.orthographicSize;
        }

        public float GetMaxZoom()
        {
            return Settings.MaxDistance;
        }

        public float GetZoomScale()
        {
            return Mathf.Clamp((float)(cameras.orthographicSize - MinZoom) / (float)(MaxZoom - MinZoom), 0, 1);
        }
        #endregion

        #region MOVING
        /// <summary>
        /// Force camera (LookingAt point) to move by provided vector. Offset will remain the same.
        /// </summary>
        /// <param name="vec"></param>
        public void Move(Vector3 vec)
        {
            if (!Settings.MoveEnabled)
                return;
            lookingAt += vec;
            raycastPlane = new Plane(Vector3.up, new Vector3(0, LookingAt.y, 0));
            transform.position += vec;
            //CheckConstraints();
        }

        /// <summary>
        /// Force camera to move when pointer is on margin
        /// </summary>
        public void MoveOnMargins()
        {
            Vector3 pointerPos = Input.mousePosition;
            Vector3 moveVec = Vector3.zero;
            //Vector3 rayPos = Vector3.zero;
            bool onMargin = false;

            if (pointerPos.x < minMarginX)
            {
                moveVec.x = -1;
                moveVec.z = 1;
                onMargin = true;
            }
            else if (pointerPos.x > maxMarginX)
            {
                moveVec.x = 1;
                moveVec.z = -1;
                onMargin = true;
            }

            if (pointerPos.y < minMarginY)
            {
                moveVec.x = -1;
                moveVec.z = -1;
                onMargin = true;
            }
            else if (pointerPos.y > maxMarginY)
            {
                moveVec.x = 1;
                moveVec.z = 1;
                onMargin = true;
            }

            if (onMargin)
            {
                moveVec.Normalize();
                moveVec *= Settings.marginSpeed * Time.deltaTime;
                //Debug.Log("OnMargin " + pointerPos + "; " + minMarginX + "; " + maxMarginX + "; " + minMarginY + "; " + maxMarginY + "; " + moveVec);
                Settings.MoveEnabled = true;
                Move(moveVec);
                if (MoveRestrictionEnabled)
                    CheckConstraints();
            }
        }

        bool inputDrivenByMove = false;
        /// <summary>
        /// Camera will move to provided point in demanded time. Offset will remain untouched.
        /// </summary>
        /// <param name="destination">will be new LookingAt point</param>
        /// <param name="time">time of motion</param>
        /// 

        public void SmoothMoveToPoint(Vector2i destination, float time, bool BlockUser, bool tweakZoom = false, float finalZoom = 7f)
        {
            //Debug.LogError("SmoothMoveToPoint");
            SmoothMoveToPoint(new Vector3(destination.x, 0, destination.y), time, BlockUser, tweakZoom, finalZoom);
        }

        public void SmoothMoveToPoint(Vector3 destination, float time, bool BlockUser, bool tweakZoom = false, float finalZoom = 7f, RectTransform hover = null)
        {
            //Debug.Log("SmoothMoveToPoint: " + destination);
            if (Vector3.Distance(lookingAt, destination) < .5f)  //prevent from really small camera adjustments, especially when moving camera to show hover
                return;
            if (!Settings.MoveEnabled)
                return;
            if (BlockUser && !TutorialSystem.TutorialController.ShowTutorials)
            {
                StopCameraMoveAnywayAndUnblockPlayerInteraction();
                return;
            }

            if (SmoothMoveCoroutineStarted)
            {
                //StopCoroutine("SmoothMove");
                Timing.KillCoroutine(SmoothMove().GetType());
                if (inputDrivenByMove)
                {
                    BlockUserByCoroutine(false);
                    inputDrivenByMove = false;
                }
            }
            object[] param = new object[6] { destination, time, BlockUser, tweakZoom, finalZoom, hover };
            CameraMovementStoped = false;
            if (SmoothSwypeCoroutineStarted)
            {
                Timing.KillCoroutine(SmoothSwype().GetType());
                SmoothSwypeCoroutineStarted = false;
            }
            Timing.KillCoroutine(SmoothMove().GetType());
            Timing.RunCoroutine(SmoothMove(param));
        }

        public void StopCameraMoveAnywayAndUnblockPlayerInteraction()
        {
            Timing.KillCoroutine(SmoothMove().GetType());
            inputDrivenByMove = false;
            BlockUserByCoroutine(false);
            SmoothMoveCoroutineStarted = false;
            CameraMovementStoped = true;
            BlockUserInput = false;
        }

        public IEnumerator<float> SmoothMove(object[] param = null)
        {
            Debug.Log("In smooth move.");
            if (smoothmove1 != null)
                yield return Timing.WaitUntilDone(smoothmove1);
            SmoothMoveCoroutineStarted = true;
            float time = (float)param[1];
            float startTime = Time.time;
            Vector3 dest = (Vector3)param[0];
            Vector3 startPos = lookingAt;
            bool blockUser = (bool)param[2];

            inputDrivenByMove = true;
            BlockUserByCoroutine(true);

            float distance = (lookingAt - dest).magnitude;

            float x = startPos.x;
            float z = startPos.z;

            bool finished = false;

            bool tweakZoom = (bool)param[3];
            bool skipMidStage = false;
            float finalZoom = (float)param[4];
            float midZoom = finalZoom + (distance / 20f);
            if (midZoom < cameras.orthographicSize)
                skipMidStage = true;
            if (distance < 5f && cameras.orthographicSize == finalZoom)
                tweakZoom = false;

            if (time <= 0)
                time = 1;
            //Draw a line to show movement to the destination
            Debug.DrawLine(startPos, dest, Color.red, 60f, false);
            while (!finished)
            {
                float t = (Time.time - startTime) / (time);

                float percent = moveCurve.Evaluate(t);

                x = startPos.x + ((dest.x - startPos.x) * percent);
                z = startPos.z + ((dest.z - startPos.z) * percent);

                Vector3 newPosition = new Vector3(x, 0.0f, z);

                MoveToPoint(newPosition);
                if (tweakZoom)
                {
                    if (skipMidStage)
                    {
                        SetZoom(Mathf.Lerp(cameras.orthographicSize, finalZoom, t));
                    }
                    else
                    {
                        if (t < .5f)
                            SetZoom(Mathf.Lerp(cameras.orthographicSize, midZoom, t));
                        else
                            SetZoom(Mathf.Lerp(cameras.orthographicSize, finalZoom, (t - .5f) * 2));
                    }
                    if (t >= 1)
                        SetZoom(finalZoom);
                }

                if (Time.time - startTime > time)
                    finished = true;

                yield return 0f;
            }

            inputDrivenByMove = false;
            BlockUserByCoroutine(false);
            SmoothMoveCoroutineStarted = false;
            CameraMovementStoped = true;
            yield return 0f;
        }

        /// <summary>
        /// Explicitly sets the LookingAt point to provided value. Offset will remain untouched.
        /// </summary>
        /// <param name="destination">new LookingAt point</param>
        public void MoveToPoint(Vector3 destination)
        {
            if (!Settings.MoveEnabled)
                return;
            if (lookingAt.y != destination.y)
                raycastPlane.SetNormalAndPosition(raycastPlane.normal, new Vector3(0, destination.y, 0));
            raycastPlane = new Plane(Vector3.up, new Vector3(0, destination.y, 0));
            lookingAt = destination;
            transform.position = lookingAt - Offset;
            transform.Translate(0, Offset.y * 2, 0, Space.World);

            transform.LookAt(lookingAt);
        }

        public IEnumerator<float> SmoothSwype()
        {
            SmoothSwypeCoroutineStarted = true;
            float time = Settings.smoothSwypeDumpTime;
            if (cameraVelocity.magnitude * Settings.smoothSwypeDumpTime > Settings.MaxDistance)
                cameraVelocity *= Settings.MaxDistance / Settings.smoothSwypeDumpTime / cameraVelocity.magnitude;
            while (time > 0)
            {
                Move(cameraVelocity * time / Settings.smoothSwypeDumpTime * Time.deltaTime);
                time -= Time.deltaTime;
                yield return 0f;
            }
            SmoothSwypeCoroutineStarted = false;
            yield return 0f;
        }

        private void CheckConstraints()
        {
            var moveTo = MoveRestrictionTo;
            var moveFrom = MoveRestrictionFrom;
            List<Vector3> rects = new List<Vector3>();
            for (int i = 0; i <= 1; ++i)
                for (int j = 0; j <= 1; ++j)
                {
                    rects.Add(RayCast(new Vector3(i * Screen.width, j * Screen.height, 0)));
                }
            List<Vector3> pointToCheck = new List<Vector3>();
            foreach (var p in rects)
            {
                if (restrictionReferencePoint != null)
                {
                    pointToCheck.Add(restrictionReferencePoint.transform.InverseTransformPoint(p));
                }
                else
                {
                    pointToCheck.Add(p);
                }
            }
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minY = float.MaxValue;
            float maxY = float.MinValue;
            foreach (var p in pointToCheck)
            {
                if (p.x < minX)
                    minX = p.x;
                if (p.x > maxX)
                    maxX = p.x;
                if (p.z < minY)
                    minY = p.z;
                if (p.z > maxY)
                    maxY = p.z;
            }

            float xMarg = (moveTo.x - moveFrom.x) / (maxX - minX);
            float yMarg = (moveTo.y - moveFrom.y) / (maxY - minY);
            Vector3 moveDistance = Vector3.zero;
            if (xMarg < 1 || yMarg < 1)
            {
                SetZoom((xMarg < yMarg ? xMarg : yMarg) * cameras.orthographicSize);
                MaxZoom = (xMarg < yMarg ? xMarg : yMarg) * cameras.orthographicSize;
            }
            else
            {
                IsMoveRestrictedHorizontally = false;
                IsMoveRestrictedVertically = false;
                if (minX < moveFrom.x)
                {
                    moveDistance.x = moveFrom.x - minX;
                    IsMoveRestrictedHorizontally = true;
                }
                if (maxX > moveTo.x)
                {
                    moveDistance.x = moveTo.x - maxX;
                    IsMoveRestrictedHorizontally = true;
                }

                if (minY < moveFrom.y)
                {
                    moveDistance.z = moveFrom.y - minY;
                    IsMoveRestrictedVertically = true;
                }
                if (maxY > moveTo.y)
                {
                    moveDistance.z = (moveDistance.z + moveTo.y - maxY);
                    IsMoveRestrictedVertically = true;
                }
            }

            if (restrictionReferencePoint != null)
                moveDistance = restrictionReferencePoint.transform.TransformPoint(moveDistance);
            Move(moveDistance);
            //CheckIsMoveRestricted(moveDistance);
        }

        public bool IsMoveRestrictedHorizontally = false;
        public bool IsMoveRestrictedVertically = false;

        void CheckIsMoveRestricted(Vector3 moveDistance)
        {
            IsMoveRestrictedHorizontally = false;
            IsMoveRestrictedVertically = false;
            if (moveDistance.x != 0)
            {
                IsMoveRestrictedHorizontally = true;
                //Debug.LogError("cam moveDistance.x = " + moveDistance.x);
            }
            if (moveDistance.z != 0)
            {
                IsMoveRestrictedVertically = true;
                //Debug.LogError("cam moveDistance.z = " + moveDistance.z);
            }
        }

        public void ForceCheckConstraints()
        {
            CheckConstraints();
        }
        #endregion

        #region ROTATING
        public void SmoothRotation(float angle, float time, bool BlockUser)
        {
            if (!Settings.RotateEnabled)
                return;
            if (SmoothRotateCoroutineStarted)
                Timing.KillCoroutine(SmoothRotate().GetType());

            object[] param = new object[3] { angle, time, BlockUser };
            Timing.RunCoroutine(SmoothRotate(param));
        }

        public void SmoothRotationAroundPoint(float angle, float time, Vector3 point, bool BlockUser)
        {
            if (!Settings.RotateEnabled)
                return;
            if (SmoothRotateAroundCoroutineStarted)
                Timing.KillCoroutine(SmoothRotateAroundPoint().GetType());
            object[] param = new object[4] { angle, time, point, BlockUser };
            Timing.RunCoroutine(SmoothRotateAroundPoint(param));
        }

        bool inputDrivenByRotate;
        public IEnumerator<float> SmoothRotate(object[] param = null)
        {
            float time = (float)param[1];
            float dest = (float)param[0];
            bool BlockUser = (bool)param[2];
            float desttemp = dest;
            float realTime = time;
            SmoothRotateCoroutineStarted = true;
            if (BlockUser)
            {
                inputDrivenByRotate = true;
                BlockUserByCoroutine(true);
            }
            while (time > Time.deltaTime)
            {
                float temp = desttemp * Time.deltaTime / realTime;
                Rotate(temp);
                dest -= temp;
                time -= Time.deltaTime;
                yield return 0f;
            }
            Rotate(dest);
            if (BlockUser)
            {
                BlockUserByCoroutine(false);
                inputDrivenByRotate = false;
            }
            SmoothRotateCoroutineStarted = false;
            yield return 0f;
        }

        public IEnumerator<float> SmoothRotateAroundPoint(object[] param = null)
        {
            float time = (float)param[1];
            float dest = (float)param[0];
            Vector3 point = (Vector3)param[2];
            bool BlockUser = (bool)param[3];
            float desttemp = dest;
            float realTime = time;
            SmoothRotateAroundCoroutineStarted = true;
            if (BlockUser)
                BlockUserByCoroutine(true);

            while (time > Time.deltaTime)
            {
                float temp = desttemp * Time.deltaTime / realTime;
                RotateAroundPoint(temp, point);
                dest -= temp;
                time -= Time.deltaTime;
                yield return 0f;
            }
            RotateAroundPoint(dest, point);
            if (BlockUser)
            {
                BlockUserByCoroutine(false);
            }
            SmoothRotateAroundCoroutineStarted = false;
            try
            {
                Timing.KillCoroutine(SmoothRotate().GetType()); // btw why?
                                                                //StopCoroutine("SmoothRotate");
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            yield return 0f;
        }

        public delegate void OnRotationEventHandler(Quaternion rotation);
        public event OnRotationEventHandler OnRotation;

        /// <summary>
        /// Force camera to rotate arount Yaxis coming through LookingAt point.
        /// </summary>
        /// <param name="angle"></param>
        public void Rotate(float angle)
        {
            if (Settings.RotateEnabled)
            {
                transform.RotateAround(LookingAt, new Vector3(0, 1, 0), angle);
                Offset = Quaternion.Euler(0, angle, 0) * Offset;

                if (OnRotation != null)
                    OnRotation(transform.rotation);
            }
        }

        private void RestrictedRotate(float angle)
        {
            var tempo = gameObject.transform.eulerAngles.y - Settings.Angle;
            if (angle * tempo > 0)
            {
                var p = angle * (Settings.MaximumDeviation - Mathf.Abs(tempo)) / Settings.MaximumDeviation;
                Rotate(p);
            }
            else
                Rotate(angle);
        }

        public void RotateAroundPoint(float angle, Vector3 point)
        {
            if (Settings.RotateEnabled)
            {
                LookingAt = RotatePointAroundPivot(lookingAt, point, new Vector3(0, angle, 0));
                Rotate(angle);
            }
        }

        private void RestrictedRotateAroundPoint(float angle, Vector3 point)
        {
            var tempo = gameObject.transform.eulerAngles.y - Settings.Angle;
            RotateAroundPoint(angle * ((angle * tempo > 0) ? (Settings.MaximumDeviation - Mathf.Abs(tempo)) : 1) / Settings.MaximumDeviation, point);
        }

        public Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            Vector3 dir = point - pivot; // get point direction relative to pivot
            dir = Quaternion.Euler(angles) * dir; // rotate it
            point = dir + pivot; // calculate rotated point
            return point; // return it
        }
        #endregion

        #region Following
        private bool followingGo = false;
        private float followingTime = 0f;
        private Transform followedTransform;

        public void FollowGameObject(Transform tr)
        {
            if (!TutorialSystem.TutorialController.ShowTutorials)
            {
                Debug.Log("Do not follow, tutorials disabled.");
                return;
            }
            Debug.Log("Camera will follow go: " + tr.gameObject.name);
            if (followingGo)
                StopFollowing();
            followingGo = true;
            followingTime = 0;
            followedTransform = tr;
        }

        [TutorialTriggerable]
        public void StopFollowing()
        {
            if (!followingGo)
                return;

            Debug.Log("Camera will stop following: " + followedTransform.gameObject.name);
            //print("stopping following");
            followingGo = false;
            followedTransform = null;
        }

        public bool CompareFollowingObjects(Transform tr)
        {
            if (tr == followedTransform)
                return true;
            else return false;
        }
        #endregion
    }
}
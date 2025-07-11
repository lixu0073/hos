using UnityEngine;
using UnityEngine.EventSystems;
using IsoEngine;
using SimpleUI;

namespace Hospital
{
    public class RotatableSimpleController : RotatableController
    {
        private RotatableObject rotatableObject;
        private float touchStartTime;
        private bool draggin = false;
        private Vector3 startMousePos;
        [SerializeField] private bool shouldDisappear = true;
        private bool fresh = false;
        private bool isFromShop = false;
        private static float arrowShowTime = 0.25f;
        private static float upTime = 0.5f;
        public bool draggingEnabled = true;
        private BaseCameraController camController;

        private Vector2i pastPosition;
        private Rotation pastRotation;

        public delegate void StoppedDragging(object args = null);
        public event StoppedDragging stoppedDraggingEvent;

        public delegate void PlacedRoom();
        public event PlacedRoom placedRoomEvent;

        public bool IsDragging { get { return draggin; } }

        //private bool canMove = false;

        //public RotatableSimpleController()
        //{
        //	rotatableObject = gameObject.GetComponent<RotatableObject>();
        //	fresh = true;
        //	if ((rotatableObject is RotatableWithoutBuilding || rotatableObject is Decoration) && !(rotatableObject is ProbeTable))
        //		shouldDisappear = false;
        //	draggin = false;
        //}

        public RotatableSimpleController()
        {
            //rotatableObject = gameObject.GetComponent<RotatableObject>();
            fresh = true;
            //if ((rotatableObject is RotatableWithoutBuilding || rotatableObject is Decoration) && !(rotatableObject is ProbeTable))
            //	shouldDisappear = false;
            draggin = false;
        }

        void Awake()
        {
            rotatableObject = gameObject.GetComponent<RotatableObject>();
            if ((rotatableObject is RotatableWithoutBuilding || rotatableObject is Decoration) && !(rotatableObject is ProbeTable))
                shouldDisappear = false;
        }

        public virtual void OnClick()
        {
            rotatableObject.OnClick();
            if (UIController.get.drawer.IsVisible)
                UIController.get.drawer.SetVisible(false);
            if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
                UIController.get.FriendsDrawer.SetVisible(false);
        }

        public void SetShouldDisappear(bool val)
        {
            shouldDisappear = val;
        }

        //bool rotationShown = false;
        private void ShowRotation(bool isFresh = false)
        {
            if (shouldDisappear)
                TryToBuy();

            //rotationShown = true;
            if (rotatableObject != null)
            {
                if (isFresh)
                    UIController.get.CloseActiveHover();

                if (isFromShop)
                {
                    //temp.Close();
                    //rotationShown = false;
                    rotatableObject.SetAnchored(true);
                    rotatableObject.SetBorderActive(false);
                    isFromShop = false;
                }
                else
                {
                    var temp = RotateHover.Open(rotatableObject, gameObject.GetComponent<RotatableSimpleController>());
                    AreaMapController.Map.ChangeOnTouchType((x) =>
                    {
                        if (!draggin)
                        {
                            temp.Close();
                            //rotationShown = false;
                            rotatableObject.SetAnchored(true);
                            rotatableObject.SetBorderActive(false);
                        }
                        SaveSynchronizer.Instance.MarkToSave(SavePriorities.BuildingMoved);
                    });
                    if (rotatableObject.actualData.tilesX >= 4)
                        temp.SetWorldPointHovering(new Vector3(rotatableObject.position.x + rotatableObject.actualData.rotationPoint.x + .5f, 0, rotatableObject.position.y + rotatableObject.actualData.rotationPoint.y + 1f));
                    else
                        temp.SetWorldPointHovering(new Vector3(rotatableObject.position.x + rotatableObject.actualData.rotationPoint.x, 0, rotatableObject.position.y + rotatableObject.actualData.rotationPoint.y));
                }

                //temp.SetWorldPointHovering(new Vector3(rotatableObject.position.x + rotatableObject.HalfSize.x, 0, rotatableObject.position.y + rotatableObject.HalfSize.y));
                if (RotateHover.activeHover != null)
                    RotateHover.activeHover.UpdateAccordingToMode();
            }
        }

        void TryToBuy(bool afterMissingResources = false)
        {
            rotatableObject.Buy((wasStored) =>
            {
                NotificationCenter.Instance.ObjectAdded.Notification += UIController.get.drawer.UnlockDependedntRoom;
                rotatableObject.StartBuilding();
                NotificationCenter.Instance.ObjectAdded.Invoke(new ObjectBuiltEventArgs(rotatableObject, wasStored));
                ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, rotatableObject.Tag));
                NotificationCenter.Instance.DrawerUpdate.Invoke(new DrawerUpdateEventArgs());
                UIController.get.drawer.LockBuildItem(AreaMapController.Map.GetPrefabInfo(rotatableObject.Tag));
                NotificationCenter.Instance.ObjectAdded.Notification -= UIController.get.drawer.UnlockDependedntRoom;
            }, afterMissingResources);
        }

        public void MissingResourcesCallback(bool usedOffer)
        {
            if (usedOffer)
                TryToBuy(true);
            else
                Destroy();
        }

        UnanchorArrow UnanchorArrow;
        private void Destroy()
        {
            Debug.Log("DestroyRotatable");

            rotatableObject.IsoDestroy();

            GameObject.Destroy(gameObject);
        }

        public void Update()
        {
            if (RotatableObject.visitingMode /*|| EventSystem.current.IsPointerOverGameObject()*/)
                return;

            if (!Input.GetMouseButton(0) && draggin)
            {
                StopDragging();
                return;
            }

            if (draggin)
            {
                camController.MoveOnMargins();
                ReferenceHolder.Get().engine.MainCamera.Settings.MoveEnabled = false;

                Vector2i pos = RotatableObject.map.GetLevel(0).ScreenToTile(Input.mousePosition);

                if ((Input.mousePosition - startMousePos).magnitude > Screen.width / 120f)
                {
                    //Debug.LogWarning("P:" + pos.x + " " + pos.y + " PP:" + pastPosition.x + " " + pastPosition.y);

                    if (rotatableObject.actualData != null && pos != Vector2i.zero)
                    {
                        if ((int)(pos.x - rotatableObject.actualData.rotationPoint.x) != rotatableObject.position.x || (int)(pos.y - rotatableObject.actualData.rotationPoint.y) != rotatableObject.position.y)
                            rotatableObject.MoveTo((int)(pos.x - rotatableObject.actualData.rotationPoint.x), (int)(pos.y - rotatableObject.actualData.rotationPoint.y));
                    }
                }
            }

            //if (rotationShown || !draggingEnabled || touchStartTime < 0 || IsoEngine.BaseCameraController.IsPointerOverInterface() || !canMove)
            if (!draggingEnabled || touchStartTime < 0 || EventSystem.current.IsPointerOverGameObject() || EventSystem.current.IsPointerOverGameObject(0) || UIController.get.isAnyHoverActive() || UIController.get.isAnyToolActive())
                return;

            if ((Input.mousePosition - startMousePos).magnitude > Screen.width / 30)
            {
                if (UnanchorArrow != null)
                    GameObject.Destroy(UnanchorArrow.gameObject);
                UnanchorArrow = null;
                touchStartTime = -1;
                return;
            }

            if (touchStartTime > 0 && Time.time - touchStartTime > arrowShowTime && Time.time - touchStartTime < upTime && !EventSystem.current.IsPointerOverGameObject(0))
                UpdateUnanchorArrow();

            if (!draggin && touchStartTime > 0 && Time.time - touchStartTime > upTime)
            {
                if (UnanchorArrow != null)
                    UnanchorArrow.UpdateArrow(1);

                touchStartTime = -1f;
                StartDragging(false);
                if (UIController.get.drawer != null && UIController.get.drawer.IsVisible)
                    UIController.get.drawer.SetVisible(false);
                if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
                    UIController.get.FriendsDrawer.SetVisible(false);
                if (UnanchorArrow != null)
                {
                    Debug.Log("Unanchored");
                    //UnanchorArrow.UnanchorBounce();
                    //Destroy(UnanchorArrow.gameObject);
                }
            }
        }

        public void UpdateUnanchorArrow()
        {
            if (UnanchorArrow == null)
            {
                UnanchorArrow = GameObject.Instantiate(ResourcesHolder.Get().UnanchorArrow).GetComponent<UnanchorArrow>();
                UnanchorArrow.transform.SetParent(UIController.get.canvas.transform);
                UnanchorArrow.transform.SetAsLastSibling();
                UnanchorArrow.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y + Screen.height / 15, 0);
            }

            UnanchorArrow.UpdateArrow((Time.time - touchStartTime - arrowShowTime) / (upTime - arrowShowTime));
            if (Time.time + 0.583f / 2 - touchStartTime - arrowShowTime > upTime - arrowShowTime)
                UnanchorArrow.UnanchorBounce();
        }

        public RotatableObject GetRotatableObject()
        {
            return rotatableObject;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private static float margin = 10.0f;
#else
		private static float margin=20.0f;
#endif
        public void OnMouseUp()
        {
            //if (!GameState.isHoverOn) {
            //	canMove = true;
            //}
            if (touchStartTime < 0)
                return;
            if (UnanchorArrow != null)
                GameObject.Destroy(UnanchorArrow.gameObject);
            if (Time.time - touchStartTime < 0.5f && (startMousePos - Input.mousePosition).magnitude < margin)
            {
                if (!IsoEngine.BaseCameraController.IsPointerOverInterface())
                    OnClick();
            }
            touchStartTime = -1;
        }

        public void OnMouseDown()
        {
            //if (GameState.isHoverOn) {
            //	canMove = false;
            //} /*else {
            //	canMove = true;
            //}*/

            if (!RotatableObject.visitingMode && !BaseCameraController.IsPointerOverInterface() && Input.touchCount < 2)
            {
                touchStartTime = Time.time;
                startMousePos = Input.mousePosition;
            }
            else if (RotatableObject.visitingMode && !BaseCameraController.IsPointerOverInterface() && Input.touchCount < 2 && rotatableObject != null && rotatableObject.availableInVisitingMode)
            {
                touchStartTime = Time.time;
                startMousePos = Input.mousePosition;
            }
            else
                touchStartTime = -1;

            if (this.rotatableObject.Tag == TutorialController.Instance.GetCurrentStepData().TargetMachineTag && TutorialController.Instance.CurrentTutorialStepTag == StepTag.arrange_text_before)
                TutorialUIController.Instance.HideIndicator();
        }

        public void StartDragging(bool isFromShop)
        {
            if (draggin)
                return;

            camController = ReferenceHolder.Get().engine.MainCamera;

            this.isFromShop = isFromShop;
            if (!isFromShop)
            {
                pastPosition = rotatableObject.position;
                pastRotation = rotatableObject.actualRotation;
            }

            draggin = true;
            rotatableObject.SetAnchored(false);

            if (RotateHover.activeHover != null)
                RotateHover.activeHover.Close();
            //HospitalAreasMapController.Map.ResetOntouchAction();

            if (RotatableObject.EnableManyBorders())
            {
                AreaMapController.Map.SetBorders(rotatableObject.area, true);
                AreaMapController.Map.EnableMutalForStaticDoor();
            }
            else
                rotatableObject.SetBorderActive(true);

            ReferenceHolder.Get().engine.MainCamera.Settings.MoveEnabled = false;
        }

        public void Anchor()
        {
            shouldDisappear = false;
        }

        private void StopDragging()
        {
            stoppedDraggingEvent?.Invoke();

            AreaMapController.Map.SetBorders(rotatableObject.area, false);
            AreaMapController.Map.DisableMutalForStaticDoor();

            bool isTempObj = rotatableObject.isTemporaryObject();

            if (shouldDisappear || isTempObj)
            {
                if (!rotatableObject.ProperlySet || isTempObj)
                {
                    rotatableObject.StopDragging();

                    if (!isTempObj)
                    {
                        UIController.get.drawer.SetOpenAtLast(true);
                        UIController.get.drawer.ToggleVisible();
                        //UIController.getHospital.drawer.SetPastPosition();
                    }

                    if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
                    {
                        for (int i = 0; i < BasePatientAI.patients.Count; ++i)
                        {
                            if (BasePatientAI.patients[i] != null)
                                BasePatientAI.patients[i].Notify((int)StateNotifications.OfficeAnchored, true);
                        }
                    }

                }
                else
                {
                    var decInfo = rotatableObject.GetRoomInfo() as DecorationInfo;
                    var shopInfo = rotatableObject.GetRoomInfo() as ShopRoomInfo;
                    //Debug.LogWarning(shopInfo.Tag);

                    //if (!fresh)
                    ShowRotation();

                    if (((decInfo != null) && (decInfo.isDecoration) && rotatableObject.state == RotatableObject.State.working) || ((shopInfo != null) && (shopInfo.Tag == "ProbTab")))
                    {
                        UIController.get.drawer.SetOpenAtLast(true);
                        UIController.get.drawer.ToggleVisible();
                        //UIController.getHospital.drawer.SetPastPosition();
                    }

                    placedRoomEvent?.Invoke();

                    if (fresh)
                        fresh = false;
                }

            }
            else if (rotatableObject.IsDummy)
            {
                rotatableObject.MoveTo(pastPosition.x, pastPosition.y, pastRotation);
                rotatableObject.SetAnchored(true);
            }
            else
                ShowRotation();

            //UIController.get.drawer.ResetDrawerDragableObject();
            UIController.get.drawer.ResetDrawerDragableObject();
            draggin = false;
            shouldDisappear = false;
            ReferenceHolder.Get().engine.MainCamera.Settings.MoveEnabled = true;
        }

        public void StoreItem()
        {
            Destroy();
            rotatableObject.ToStored();
        }
    }

    public abstract class RotatableController : MonoBehaviour
    {

    }

}

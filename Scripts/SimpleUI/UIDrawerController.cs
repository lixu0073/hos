using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using SimpleUI;
using System.Linq;
using UnityEngine.EventSystems;
using TMPro;
using MovementEffects;
using System;

namespace Hospital
{
    public class UIDrawerController : UIElement, IDrawer
    {
        [SerializeField]
        protected List<DrawerTabScript> drawers = null;
        [SerializeField]
        protected List<Vector2> AreasPositions = null;
        [SerializeField]
        protected List<Transform> tabs = null;
        [SerializeField]
        protected GameObject elementPrefab;
        [SerializeField]
        protected GameObject elementPrefabDouble;
        [SerializeField]
        protected GameObject exitButton;
        [SerializeField]
        protected GameObject background;
        [HideInInspector]
        protected bool openAtLast = false;
        [SerializeField]
        protected GameObject badgeMain;
        [SerializeField]
        protected GameObject badgeLaboratory;
        [SerializeField]
        protected GameObject badgeHospital;
        [SerializeField]
        protected GameObject badgePatio;
        [SerializeField]
        protected RectTransform separator;
        [SerializeField]
        protected List<Rotations> BadgeObjectList = new List<Rotations>();
        [SerializeField]
        protected RectTransform TabContainer;
        protected bool shouldPopulate = false;
        protected List<Rotations> localItemList;
        protected List<DrawerRotationItem> allItems = new List<DrawerRotationItem>();
        protected int actualTab = -1;
        protected GameObject createdElement;
        protected bool isInitalizing;
        protected bool start = false;
        protected bool blocked = false;
        protected int newObjectCounter = 0;
        protected Rotation lastRotation;
        protected RotatableObject rObj;
        protected int unlockedHospitalMachines = 0;
        protected int unlockedLaboratoryMachines = 0;
        protected int unlockedPatioMachines = 0;
        protected IEnumerator<float> CenteringCoroutine;
        [SerializeField]
        protected GameObject paintBadgeClinic = null;
        [SerializeField]
        protected GameObject paintBadgeLab = null;
        protected bool paintBadgeClinicVisible = false;
        protected bool paintBadgeLabVisible = false;
        protected Animator animator;
        [Header("Device specific layout refs")]
        [SerializeField]
        protected RectTransform drawerRect;
        [SerializeField]
        protected RectTransform exitButtonAnchor;
        [SerializeField]
        protected RectTransform tabsRect;
        [SerializeField]
        protected RectTransform[] tabButtons;
        [SerializeField]
        protected RectTransform[] scrollRects;
        public RectTransform[] ScrollRects { get { return scrollRects; } }
        [SerializeField]
        protected GameObject[] tabTitles;
        [SerializeField]
        protected GridLayoutGroup[] grids;
        [SerializeField]
        protected RectTransform RibbonTitle;
        [SerializeField]
        protected int sinleDrawerItemRectHeight;
        #region Mono

        void Awake()
        {
            //SetDeviceLayout();
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            Initialize(false);
            HideMainButtonBadge();
            gameObject.SetActive(false);
            if (ExtendedCanvasScaler.HasNotch())
            {
                //SetupDrawerForIphoneX();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public void Initialize(List<Rotations> list)
        {
            Populate(list);
        }

        #endregion

        protected int tabOpenedBeforeDraggingStarted;

        //public delegate void StartedDragging(RotatableObject rotObject);
        public event RefactoredDrawerController.StartedDragging startedDraggingEvent;

        public RefactoredDrawerController.StartedDragging StartedDraggingEvent
        {
            get { return startedDraggingEvent; }
            set { startedDraggingEvent = value; }
        }

        #region Interface

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        public Vector3 DragingObjectPosition()
        {
            if (rObj != null)
                return new Vector3(rObj.position.x, 0, rObj.position.y);
            return Vector3.zero;
        }

        public RotatableObject DragingObject()
        {
            return rObj;
        }

        public HospitalArea DragingObjectArea()
        {
            if (rObj != null)
            {
                return AreaMapController.Map.GetAreaTypeFromPositionIfNotOnEdge(rObj.position);
            }
            return HospitalArea.Ignore;
        }

        public List<DrawerRotationItem> AllDrawerItems()
        {
            return allItems;
        }

        public bool IsDragingObjectDummy()
        {
            if (rObj != null)
                return rObj.IsDummy;
            return false;
        }

        public bool IsInitalizing()
        {
            return isInitalizing;
        }

        public void SetOpenAtLast(bool open)
        {
            openAtLast = open;
        }

        public void SetLastRotation(Rotation rotation)
        {
            lastRotation = rotation;
        }

        public bool IsPaintBadgeClinicVisible()
        {
            return paintBadgeClinicVisible;
        }

        public bool IsPaintBadgeLabVisible()
        {
            return paintBadgeLabVisible;
        }

        public void BlockDrawer(bool state)
        {
            if (state && IsVisible)
                SetVisible(false);
            blocked = state;

        }

        public override void SetVisible(bool visible)
        {
            if (blocked || UIController.get.isAnyPopupActive())
                return;
            if (visible)
            {
                UIController.get.CloseActiveHover();
                if (UIController.getHospital != null)
                {
                    if (!CampaignConfig.hintSystemEnabled && UIController.getHospital.ObjectivesPanelUI.isSlidIn)
                    {
                        UIController.getHospital.ObjectivesPanelUI.SlideOut();
                    }
                }
                ReferenceHolder.Get().engine.GetMap<AreaMapController>().ChangeOnPressType(x =>
                {
                    HideAllBadgeOnCurrentTab();
                    SetVisible(false);
                });
                UpdateAllItems();
            }
            else
            {
                NotificationCenter.Instance.DrawerClosed.Invoke(new DrawerClosedEventArgs());
                AreaMapController.Map.ResetOnPressAction();
            }
            base.SetVisible(visible);
        }

        public void SwitchToTab(int tab)
        {
            SetTabVisible(tab);
        }

        public virtual void SetTabVisible(int index, bool hideBadges)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].SetAsFirstSibling();
            }
            if (index < tabs.Count)
                tabs[index].SetSiblingIndex(3);


            if ((start) && (actualTab != index) && hideBadges)
                UIController.get.drawer.HideAllBadgeOnCurrentTab();


            SoundsController.Instance.PlayButtonClick(isInitalizing);
            if (index >= 0 && index < drawers.Count)
            {
                if (actualTab != -1)
                {
                    drawers[actualTab].transform.gameObject.SetActive(false);
                    drawers[actualTab].ChangeImage(false);
                    //smooth camera was here
                }
                else
                {
                    for (int i = 0; i < drawers.Count; i++)
                    {
                        drawers[i].ChangeImage(false);
                        drawers[i].ChangeTab(0);
                    }
                }
                drawers[index].transform.gameObject.SetActive(true);
                drawers[index].ChangeImage(true);
                exitButton.transform.SetAsLastSibling();

                LayoutRebuilder.MarkLayoutForRebuild(drawers[index].content.content);
                actualTab = index;
            }

            if (CenteringCoroutine != null)
            {
                Timing.KillCoroutine(CenteringCoroutine.GetType());
                CenteringCoroutine = null;
            }
            if (TutorialController.Instance != null && TutorialController.Instance.tutorialEnabled && (TutorialController.Instance.GetCurrentStepData().NecessaryCondition == Condition.DrawerOpened ||
                                                                                                       TutorialController.Instance.GetCurrentStepData().NecessaryCondition == Condition.ObjectDoesNotExistOnLevel))
                CenterAtTutorialItem();
            else
                CenterAtBadgeOnCurrentTab();

            if (CenteringCoroutine == null)
                CenterToItem(null, true);
            //else
            //Debug.LogError("Not reseting scroll rect pos, because it is centering to another pos");

            UpdateSeparatorPos();
        }

        public void UpdatePriceForRotatableInDrawer(RotatableObject rotObject, int newPrice)
        {
            DrawerRotationItem drawerRotationItemTemp = allItems.Find(x => x.drawerItem.GetInfo.infos == rotObject.GetRoomInfo());
            drawerRotationItemTemp.drawerItem.ChangePrice(newPrice);
        }

        public virtual void CenterCameraToArea(HospitalArea area, bool checkPosition = true)
        {
            //Debug.LogError("CenterCameraToArea  areaID = " + areaID);
            //0 = clinic, 1 = lab, 2 = patio

            Vector3 camLookingAt = ReferenceHolder.Get().engine.MainCamera.LookingAt;
            Vector2i camLookingAti = new Vector2i(Mathf.RoundToInt(camLookingAt.x), Mathf.RoundToInt(camLookingAt.z));

            switch (area)
            {
                case HospitalArea.Clinic:
                    if (checkPosition)
                    {
                        if (AreaMapController.Map.areas != null && AreaMapController.Map.areas.Count > 0)
                        {
                            if (AreaMapController.Map.areas.ContainsKey(area))
                            {
                                if (!AreaMapController.Map.areas[area].ContainsPoint(camLookingAti))
                                {
                                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[0].x, 0, AreasPositions[0].y), 1.0f, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[0].x, 0, AreasPositions[0].y), 1.0f, true);
                    }
                    break;
                case HospitalArea.Laboratory:
                    if (checkPosition)
                    {
                        if (AreaMapController.Map.areas != null && AreaMapController.Map.areas.Count > 0)
                        {
                            if (AreaMapController.Map.areas.ContainsKey(area))
                            {
                                if (!AreaMapController.Map.areas[area].ContainsPoint(camLookingAti))
                                {
                                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[1].x, 0, AreasPositions[1].y), 1.0f, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[1].x, 0, AreasPositions[1].y), 1.0f, true);
                    }
                    break;
                case HospitalArea.Patio:
                    if (checkPosition)
                    {
                        if (AreaMapController.Map.areas != null && AreaMapController.Map.areas.Count > 0)
                        {
                            if (AreaMapController.Map.areas.ContainsKey(area))
                            {
                                if (!AreaMapController.Map.areas[area].ContainsPoint(camLookingAti))
                                {
                                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[2].x, 0, AreasPositions[2].y), 1.0f, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[2].x, 0, AreasPositions[2].y), 1.0f, true);
                    }
                    break;
                default:
                    break;
            }
        }

        public virtual void CenterToItem(RectTransform item, bool toTop = false)
        {
            //Debug.LogError("CenterToItem to Top: " + toTop);
            float targetPosition;
            if (toTop)
                targetPosition = 1f;
            else
            {
                targetPosition = sinleDrawerItemRectHeight * item.transform.GetSiblingIndex() / (scrollRects[actualTab].GetComponent<ScrollRect>().content.childCount * sinleDrawerItemRectHeight - scrollRects[actualTab].rect.height);
                targetPosition = Mathf.Clamp01(1 - targetPosition);
            }

            CenteringCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(targetPosition));
        }

        public void CenterInMaternityTabToItem(RectTransform item)
        {
            Transform itemInLaborWaitingRoomPairTransform = item.transform.parent.parent;
            Transform itemInDrawerTransform = itemInLaborWaitingRoomPairTransform.parent;
            float targetPosition = (float)(itemInDrawerTransform.GetSiblingIndex() * 2 + itemInLaborWaitingRoomPairTransform.GetSiblingIndex()) / (float)10;
            targetPosition = Mathf.Clamp01(1 - targetPosition);
            CenteringCoroutine = Timing.RunCoroutine(CenterToItemCoroutine(targetPosition));
        }

        public void GenerateSimpleRotatableItem(Rotations info)//, out bool isTemporaryRotatable)
        {
            if (rObj != null)
                return;

            System.Type rotatableType = info.infos.GetType();

            //if (rotatableType == typeof(CanInfo))
            //    isTemporaryRotatable = true;
            //else isTemporaryRotatable = false;

            //Debug.LogError("GenerateSimpleRotatableItem " + info);
            pastDrawerPosition = drawers[actualTab].content.verticalNormalizedPosition;

            if (!info.infos.multiple && GameState.BuildedObjects.ContainsKey(info.infos.Tag) && GameState.BuildedObjects[info.infos.Tag] > 0)
                return;

            var pos = ReferenceHolder.Get().engine.Map.GetLevel<HospitalAreasLevelController>(0).ScreenToTile(Input.mousePosition);
            var pref = ReferenceHolder.Get().engine.objects[info[Rotation.North]].GetComponent<IsoObjectPrefabController>().prefabData;

            RotatableObject temp;

            if (rotatableType == typeof(CanInfo))
            {
                temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info);
            }
            else if (rotatableType == typeof(DecorationInfo))
            {
                if ((info.infos as DecorationInfo).isDecoration)
                    temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info, lastRotation);
                else
                    temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info);

                AreaMapController.Map.AddRotatableObject(temp);
            }
            else
            {
                if (info.infos.Area == HospitalArea.Clinic || info.infos.Area == HospitalArea.Hospital)
                {
                    Vector2i newRotatableObjectPos = new Vector2i(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2);
                    Vector2i entrance_spot = new Vector2i(10, 23); // entrence

                    if (newRotatableObjectPos.y - entrance_spot.y >= 3)
                        temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info, Rotation.East);
                    else if (newRotatableObjectPos.y - entrance_spot.y <= -3)
                        temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info, Rotation.West);
                    else
                        temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info, Rotation.South);
                }
                else
                    temp = RotatableObject.GenerateRotatableObject(pos.x - pref.tilesX / 2, pos.y - pref.tilesY / 2, info);

                AreaMapController.Map.AddRotatableObject(temp);
            }

            //isTemporaryRotatable = temp.isTemporaryObject();

            temp.GetComponent<RotatableSimpleController>().StartDragging(true);
            startedDraggingEvent?.Invoke(temp);
            rObj = temp;
            tabOpenedBeforeDraggingStarted = actualTab;
            openAtLast = true;
            ToggleVisible();
        }

        public void SetPastPosition()
        {
            drawers[actualTab].content.verticalNormalizedPosition = pastDrawerPosition;
            //Debug.LogError("SetPastPosition: " + pastDrawerPosition);
        }

        public void ResetDrawerDragableObject()
        {
            rObj = null;
        }

        public override void ToggleVisible()
        {
            base.ToggleVisible();
            ToggleScrollingFunctionality(true);
            if (IsVisible)
            {
                //Debug.LogError("Drawer opened");
                NotificationCenter.Instance.DrawerOpened.Invoke(new DrawerOpenedEventArgs());
                if (TutorialController.Instance != null && TutorialController.Instance.CurrentTutorialStepTag == StepTag.patio_tidy_5 && TutorialController.Instance.ConditionFulified)
                {
                    CenterCameraToArea(HospitalArea.Patio, false);
                    ToggleScrollingFunctionality(false);
                    SetTabVisible(2, false);   //patio
                    StartCoroutine(BlinkTutorialPatio());
                    //NotificationListener.Instance.SubscribeDrawerClosedNotification();
                }
                else if (TutorialController.Instance != null && (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_lab_add
                    || TutorialController.Instance.CurrentTutorialStepTag == StepTag.elixir_mixer_add
                    || TutorialController.Instance.CurrentTutorialStepTag == StepTag.new_cures_2))
                {
                    CenterCameraToArea(HospitalArea.Laboratory, false);
                    SetTabVisible(1, false);   //lab
                    SetDrawersToBuildTab();
                    ToggleScrollingFunctionality(false);
                }
                else if (TutorialController.Instance != null && TutorialController.Instance.CurrentTutorialStepTag == StepTag.build_doctor_text)
                {
                    //no centering on this step
                    SetTabVisible(0, false);   //hospital
                    ToggleScrollingFunctionality(false);
                }
                else if (TutorialController.Instance != null && (TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_waiting_room_drawer_open || TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_labor_room_drawer_open))
                {
                    SetTabVisible(1, false);
                }
                else if (TutorialController.Instance != null && (TutorialController.Instance.CurrentTutorialStepTag == StepTag.yellow_doc_add
                    || TutorialController.Instance.CurrentTutorialStepTag == StepTag.green_doc_add_text
                    || TutorialController.Instance.CurrentTutorialStepTag == StepTag.emma_on_Xray))
                {
                    CenterCameraToArea(HospitalArea.Clinic, false);
                    SetTabVisible(0, false);   //hospital
                    SetDrawersToBuildTab();
                    ToggleScrollingFunctionality(false);
                }
                else if (openAtLast)
                {
                    openAtLast = false;
                    SetTabVisible(tabOpenedBeforeDraggingStarted);
                    //	ShowTabWithBadge();    //why was this here? 
                }
                else
                {
                    ShowAreaTab();
                }
                start = true;

                if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
                {
                    UIController.get.FriendsDrawer.ToggleVisible();
                }
            }
            else
            {
                start = false;
                Debug.Log("Drawer closed");
                UIController.get.drawer.HideAllBadgeOnCurrentTab();
            }
        }

        public void SetDrawersToBuildTab()
        {
            foreach (DrawerTabScript drawer in drawers)
            {
                drawer.ChangeTab(0);
            }
        }

        protected void ToggleScrollingFunctionality(bool turnOn)
        {
            foreach (RectTransform item in scrollRects)
            {
                ScrollRect scrolRect = item.gameObject.GetComponent<ScrollRect>();
                if (scrolRect != null)
                {
                    scrolRect.vertical = turnOn;
                }
            }
        }

        public void UnlockDependedntRoom(ObjectBuiltEventArgs builtObject)
        {
            if (builtObject.obj.info.infos.depeningRoom != null)
            {
                allItems.FirstOrDefault(item =>
                {
                    return item.drawerItem.GetInfo.infos.Tag == builtObject.obj.Tag;
                }).drawerItem.SetUnlocked();
            }
        }

        public DrawerRotationItem FindDependentItem(BaseRoomInfo roomInfo)
        {
            return allItems
              .FirstOrDefault(item =>
              {
                  return item.drawerItem.GetInfo.infos == roomInfo;
              });
        }

        public DrawerItem FindDependentitem(RotatableObject builtObject)
        {
            return allItems
                .FirstOrDefault(item =>
                {
                    return item.rotations.infos == builtObject.GetRoomInfo();
                }).drawerItem;
        }

        public void UnlockItems(List<Rotations> list)
        {
            foreach (Rotations item in list)
            {
                DrawerRotationItem temp = allItems.Find(x => item.infos == x.rotations.infos);
                if (temp != null && !temp.drawerItem.GetInfo.infos.spawnFromParent)
                {
                    temp.drawerItem.SetUnlocked();
                }
            }
        }

        public virtual void SetDrawerItems()
        {
            Dictionary<string, int> save = GameState.BuildedObjects;

            foreach (DrawerRotationItem item in allItems)
            {
                if (item.drawerItem.GetInfo.infos.spawnFromParent)
                {
                    continue;
                }
                if (item != null)
                {
                    if (((ShopRoomInfo)item.rotations.infos).unlockLVL <= Game.Instance.gameState().GetHospitalLevel() || (GameState.StoredObjects.ContainsKey(item.rotations.infos.Tag) && GameState.StoredObjects[item.rotations.infos.Tag] > 0))
                    {
                        //Debug.LogError("this tag: " + item.rotations.infos.Tag);
                        item.drawerItem.SetUnlocked();
                    }
                    else
                    {
                        //Debug.LogError("that tag: " + item.rotations.infos.Tag);
                        item.drawerItem.SetLocked();
                    }
                }
                else
                {
                    //Debug.LogError("else tag: " + item.rotations.infos.Tag);

                    if (!item.rotations.infos.multiple)
                    {
                        item.drawerItem.LockBuild();
                    }
                    else
                    {
                        item.drawerItem.SetBuildedAmount(save[item.rotations.infos.Tag]);
                        item.drawerItem.SetUnlocked();
                    }
                }
            }
        }

        public void LockBuildItem(Rotations element)
        {
            if (element != null)
            {
                DrawerRotationItem temp = allItems.Find(x => element.infos == x.rotations.infos);
                if (temp != null)
                {
                    temp.drawerItem.LockBuild();

                }
            }
        }

        public void Populate(List<Rotations> list)
        {
            localItemList = list.OrderBy(x => ((ShopRoomInfo)x.infos).unlockLVL).ToList();
            shouldPopulate = true;
            InitializeItems();
        }

        public List<DrawerTabScript> GetDrawers()
        {
            return drawers;
        }

        public void AddBadgeForItems(List<Rotations> enableBadgeObjectList)
        {
            ResetAllSubTabsBadges();

            foreach (Rotations item in enableBadgeObjectList)
            {
                if (item.infos.dummyType == BuildDummyType.Decoration && ((ShopRoomInfo)(item.infos)).unlockLVL < 8)
                {
                    continue;
                }
                DrawerRotationItem temp = allItems.Find(x => item.infos == x.rotations.infos);
                temp.drawerItem.ShowBadge();

                ShopRoomInfo localShopInfo = ((ShopRoomInfo)temp.rotations.infos);

                if (localShopInfo.DrawerArea != HospitalAreaInDrawer.Patio && localShopInfo.DrawerArea != HospitalAreaInDrawer.MaternityPatio)
                {
                    IncrementBadgeOnSubTab(localShopInfo.DrawerArea, localShopInfo.SubTabNumber);
                }

                this.BadgeObjectList.Add(item);
            }
        }

        public bool CheckIsBadgeForItem(Rotations enableBadgeObjectList)
        {
            DrawerRotationItem temp = allItems.Find(x => enableBadgeObjectList.infos == x.rotations.infos);

            if (temp.drawerItem.isBadge())
                return true;
            else
                return false;
        }

        public virtual void ShowMainButtonBadge(int count = 0)
        {
            if (count > 0 && Game.Instance.gameState().GetHospitalLevel() >= 5)
            {
                badgeMain.SetActive(true);
                badgeMain.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
                //Debug.Log("ShowMainButtonBadge");
            }
        }

        public virtual void IncrementTabButtonBadges(int areaNo)
        { //1 is for Hospital, 2 is for Lab, 3 is for Patio - dont ask why from 1 
            switch (areaNo)
            {
                case 1:
                    this.unlockedHospitalMachines++;
                    break;
                case 2:
                    this.unlockedLaboratoryMachines++;
                    break;
                case 3:
                    this.unlockedPatioMachines++;
                    break;
            }
            UpdateTabButtonBadges();
        }

        public void DecrementTabButtonBadges(int areaNo)
        {
            switch (areaNo)
            {
                case 1:
                    this.unlockedHospitalMachines--;
                    break;
                case 2:
                    this.unlockedLaboratoryMachines--;
                    break;
                case 3:
                    this.unlockedPatioMachines--;
                    break;
            }
            UpdateTabButtonBadges();
        }

        public void AddTabButtonBadges(int unlockedHospitalMachines = -1, int unlockedLaboratoryMachines = -1, int unlockedPatioMachines = -1)
        {
            this.unlockedHospitalMachines = unlockedHospitalMachines;
            this.unlockedLaboratoryMachines = unlockedLaboratoryMachines;
            this.unlockedPatioMachines = unlockedPatioMachines;

            //Debug.LogWarning("SetTabButtonBadges: " + unlockedHospitalMachines + " , " + unlockedLaboratoryMachines + " , " + unlockedPatioMachines);

            UpdateTabButtonBadges();
        }

        public void SortTabsByBought()
        {
            if (drawers != null && drawers.Count > 0)
            {
                for (int i = 0; i < drawers.Count; i++)
                    drawers[i].SortTabs();
            }
        }

        public virtual void HideAllBadgeOnCurrentTab()
        {
            if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))
            {

                int howManyExist = 0;

                int existOnTab = 0;

                foreach (Rotations currentBadge in BadgeObjectList.ToList())
                {
                    DrawerRotationItem temp = allItems.Find(x => (currentBadge.infos == x.rotations.infos));

                    ShopRoomInfo localShopInfo = ((ShopRoomInfo)temp.rotations.infos);



                    if ((int)localShopInfo.DrawerArea % 3 == actualTab)
                    {
                        howManyExist++;

                        if ((int)localShopInfo.SubTabNumber == drawers[actualTab].GetActiveTab())
                            existOnTab++;
                    }

                    if ((int)localShopInfo.DrawerArea % 3 == actualTab && ((int)localShopInfo.SubTabNumber == drawers[actualTab].GetActiveTab()))
                    {
                        temp.drawerItem.HideBadge();
                        GameState.Get().RemoveBadge(currentBadge);
                        BadgeObjectList.Remove(currentBadge);
                        // HideBadgeOnCurrentSubTab((int)localShopInfo.SubTabNumber);

                        existOnTab--;
                        howManyExist--;

                        //DecrementTabButtonBadges (actualTab + 1); //dont ask why :( OK because same in Increment

                        DecrementButtonBadges(actualTab);
                        if ((actualTab == 0) || (actualTab == 1))
                        {
                            SetBadgeOnSubTab(actualTab, drawers[actualTab].GetActiveTab(), existOnTab);
                        }

                    }

                }

                if (howManyExist < 1)
                {
                    HideTabButtonBadge((HospitalAreaInDrawer)actualTab);
                    ResetAllSubTabsOnCurrentTab(actualTab);
                    // HideBadgeOnAllCurrentSubTab();
                }

                SaveSynchronizer.Instance.InstantSave();
                Debug.Log("HideAllBadgeOnCurrentTab");
            }
            else
            {
                HideTabButtonBadge((HospitalAreaInDrawer)actualTab);
                ResetAllSubTabsOnCurrentTab(actualTab);
                // HideBadgeOnAllCurrentSubTab();
            }
        }

        public virtual void CenterAtBadgeOnCurrentTab()
        {
            //Debug.LogError("CenterAtBadgeOnCurrentTab");
            bool showDeco = false;
            RectTransform tempItemTransform = null;

            if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))

                foreach (Rotations currentBadge in BadgeObjectList)
                {
                    DrawerRotationItem temp = allItems.Find(x => (currentBadge.infos == x.rotations.infos));
                    if (((int)temp.rotations.infos.DrawerArea == actualTab) && (temp.drawerItem.badge.activeSelf))
                    {
                        if (((ShopRoomInfo)temp.rotations.infos).SubTabNumber == 0)
                        {
                            if (drawers[actualTab].GetActiveTab() != 0)
                            {
                                drawers[actualTab].ChangeTab(0, false);
                            }
                            CenterToItem(temp.drawerItem.GetComponent<RectTransform>().parent.parent.gameObject.GetComponent<RectTransform>());//.GetComponent<RectTransform>());
                            showDeco = false;
                            break;
                        }

                        if (((ShopRoomInfo)temp.rotations.infos).SubTabNumber == 1 && !showDeco)
                        {
                            tempItemTransform = temp.drawerItem.GetComponent<RectTransform>().parent.parent.gameObject.GetComponent<RectTransform>();
                            showDeco = true;
                        }
                    }
                }

            if (showDeco && tempItemTransform != null)
            {
                if (drawers[actualTab].GetActiveTab() == 1)
                {
                    CenterToItem(tempItemTransform);
                }
                showDeco = false;
            }
        }

        public void HideAllBadges()
        {
            if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))
                foreach (Rotations currentBadge in BadgeObjectList.ToList())
                {
                    DrawerRotationItem temp = allItems.Find(x => (currentBadge.infos == x.rotations.infos));

                    temp.drawerItem.HideBadge();
                    BadgeObjectList.Remove(currentBadge);
                }

            //Debug.Log("HideAllBadges");
        }

        public void DecrementMainButtonBadge()
        {
            int count = GetMainBadgeCount();
            count--;
            if (count <= 0)
                count = 0;

            badgeMain.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();

            if (count <= 0)
                HideMainButtonBadge();
        }

        public virtual void IncrementMainButtonBadge()
        {
            if (Game.Instance.gameState().GetHospitalLevel() >= 5)
            {
                badgeMain.SetActive(true);
                badgeMain.GetComponentInChildren<TextMeshProUGUI>().text = (GetMainBadgeCount() + 1).ToString();
            }
        }

        public int GetMainBadgeCount()
        {
            return int.Parse(badgeMain.GetComponentInChildren<TextMeshProUGUI>().text, System.Globalization.CultureInfo.InvariantCulture);
        }

        public void UpdateAllItems()
        {
            UpdatePrices(UpdateType.Default);
        }

        public void UpdatePrices(UpdateType updateType = UpdateType.Default) // Deco and cans
        {
            var cans = new List<DrawerRotationItem>();

            if (allItems != null && allItems.Count > 0)
            {
                foreach (DrawerRotationItem item in allItems)
                {
                    item.drawerItem.UpdateItemInfo();

                    if (item.rotations.infos.dummyType == BuildDummyType.Decoration)
                    {
                        if (updateType == UpdateType.Decorations || updateType == UpdateType.Default)
                            item.drawerItem.UpdateDecoPrices();
                    }
                    else if (item.rotations.infos.dummyType == BuildDummyType.Can)
                    {
                        if (updateType == UpdateType.Can || updateType == UpdateType.Default)
                        {
                            CanInfo canInfo = item.rotations.infos as CanInfo;
                            cans.Add(item);
                        }

                    }
                    else if (item.rotations.infos.dummyType == BuildDummyType.Hospital2xRoom)
                    {
                        if (updateType == UpdateType.HospitalRoom || updateType == UpdateType.Default)
                            item.drawerItem.UpdateHospitalRoomPrices();

                    }
                }
            }

            for (int i = 0; i < cans.Count; i++)
                cans[i].drawerItem.UpdateCanPrice(); // 'cuz the same object on two tabs

            cans = null;
        }

        public void UpdateTranslation()
        {
            for (int i = 0; i < drawers.Count; i++)
            {
                for (int j = 0; j < drawers[i].subTabs.Count; j++)
                {
                    for (int k = 0; k < drawers[i].subTabs[j].TabContent.transform.childCount; k++)
                    {
                        drawers[i].subTabs[j].TabContent.transform.GetChild(k).GetComponentInChildren<DrawerItem>().UpdateNames();
                    }
                }
            }

        }

        public int GetActualTab()
        {
            return actualTab;
        }

        public int GetActiveTabOnActualTab()
        {
            return drawers[actualTab].GetActiveTab();
        }

        public virtual void SetPaintBadgeClinicVisible(bool setVisible)
        {
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                paintBadgeClinicVisible = setVisible;
                paintBadgeClinic.SetActive(paintBadgeClinicVisible);
            }
            else
            {
                paintBadgeClinic.SetActive(false);
            }
        }

        public virtual void SetPaintBadgeLabVisible(bool setVisible)
        {
            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                paintBadgeLabVisible = setVisible;
                paintBadgeLab.SetActive(paintBadgeLabVisible);
            }
            else
            {
                paintBadgeLab.SetActive(false);
            }
        }

        #endregion

        protected void SetupDrawerForIphoneX()
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(500, -15);
            background.GetComponent<RectTransform>().sizeDelta += new Vector2(140, 0);
            TabContainer.localPosition = new Vector3(382, 0, 0);
            tabsRect.offsetMin += new Vector2(45, 0);
            separator.offsetMin += new Vector2(-16, 0);
            separator.offsetMax += new Vector2(-38, 0);
            for (int i = 0; i < tabTitles.Length; i++)
            {
                tabTitles[i].gameObject.GetComponent<RectTransform>().localPosition += new Vector3(-10.4f, 0, 0);
            }
            RibbonTitle.localPosition += new Vector3(58.7f, 0, 0);
            exitButton.GetComponent<RectTransform>().localPosition += new Vector3(0, 27.2f, 0);
        }

        protected virtual void SetTabVisible(int index)
        {
            if (TutorialController.Instance.CurrentStepBlocksDrawerUI() || (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patio_tidy_5 && TutorialController.Instance.ConditionFulified))
            {
                return;
            }
            if (actualTab != index)
                animator.SetTrigger("TabButton" + (2 - (index - 1)));
            SetTabVisible(index, true);
        }

        protected void CenterCameraToAreaOnButton(int areaID)
        {
            if (TutorialController.Instance.CurrentStepBlocksDrawerUI() || (TutorialController.Instance.CurrentTutorialStepTag == StepTag.patio_tidy_5 && TutorialController.Instance.ConditionFulified))
            {
                return;
            }
            CenterCameraToArea((HospitalArea)areaID);
        }

        protected void ScrollToTop(RectTransform obj)
        {
            //Debug.LogError("ScrollToTop " + obj);
            drawers[actualTab].content.verticalNormalizedPosition = 0f;
        }

        protected IEnumerator<float> CenterToItemCoroutine(float targetPosition)
        {
            //Debug.LogError("CenterToItem Coroutine " + targetPosition);

            float t = 0f;
            while (drawers[actualTab].content.verticalNormalizedPosition != targetPosition && t < 1f)
            {
                t += Time.deltaTime;
                t = Mathf.Clamp(t, 0f, 1f);
                drawers[actualTab].content.verticalNormalizedPosition = Mathf.Lerp(drawers[actualTab].content.verticalNormalizedPosition, targetPosition, t);
                //Debug.LogWarning(t);
                yield return 0f;
            }

            CenteringCoroutine = null;
        }

        protected void ToggleVisible(int index)
        {
            base.ToggleVisible();

            if (IsVisible)
            {
                if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.build_doctor_text)
                {
                    CenterCameraToArea((HospitalArea)index);
                }
                SetTabVisible(index, false);   //hospital
                start = true;

                if (UIController.get.FriendsDrawer.IsVisible)
                    UIController.get.FriendsDrawer.ToggleVisible();
            }
            else
            {
                start = false;
                Debug.Log("Drawer closed");
                NotificationCenter.Instance.DrawerClosed.Invoke(new DrawerClosedEventArgs());
                UIController.get.drawer.HideAllBadgeOnCurrentTab();
            }
        }

        protected IEnumerator BlinkTutorialPatio()
        {
            //this has to be done in the next frame after the item is removed from GameState.Stored (or someone can find where exactly is drawer reopened and make sure its after removing)
            yield return null;
            TutorialUIController.Instance.StopBlinking();
            foreach (DrawerRotationItem item in AllDrawerItems().Where((x) => x.rotations.infos.Area == HospitalArea.Patio))
            {
                Debug.Log("checking stored amount for tag: " + item.rotations.infos.Tag + " frame: " + Time.frameCount);
                int stored = 0;
                if (GameState.StoredObjects.ContainsKey(item.rotations.infos.Tag))
                    stored = GameState.StoredObjects[item.rotations.infos.Tag];
                if (stored > 0)
                {
                    Debug.Log("Storad amount is  " + stored + " frame: " + Time.frameCount);
                    TutorialUIController.Instance.BlinkImage(item.drawerItem.image);
                    CenterToItem(item.drawerItem.GetComponent<RectTransform>().parent.parent.gameObject.GetComponent<RectTransform>());
                    //NotificationListener.Instance.SubscribeDrawerClosedNotification();
                    yield break;
                }
            }
            //Debug.LogError("DID NOT FIND ANY PATIO DECORATIONS STORED");
        }

        protected virtual void InitializeItems()
        {
            isInitalizing = true;
            if (!shouldPopulate)
                return;
            foreach (var p in localItemList)
            {
                switch (p.infos.DrawerArea)
                {
                    case HospitalAreaInDrawer.Clinic:
                        AddElement((int)p.infos.DrawerArea, p, HospitalAreaInDrawer.Clinic);
                        break;
                    case HospitalAreaInDrawer.Patio:
                        if (p.infos as RemovableDecorationInfo == null)
                            AddElement((int)p.infos.DrawerArea, p, HospitalAreaInDrawer.Patio);
                        break;
                    case HospitalAreaInDrawer.Laboratory:
                        System.Type rotatableType = p.infos.GetType();
                        if (rotatableType == typeof(CanInfo))
                        {
                            AddElement((int)p.infos.DrawerArea, p, HospitalAreaInDrawer.Laboratory);
                            AddElement((int)HospitalAreaInDrawer.Clinic, p, HospitalAreaInDrawer.Clinic);
                        }
                        else
                            AddElement((int)p.infos.DrawerArea, p, HospitalAreaInDrawer.Laboratory);
                        break;
                    default:
                        AddElement((int)p.infos.DrawerArea, p, HospitalAreaInDrawer.Ignore);
                        break;
                }
            }
            localItemList.Clear();
            localItemList = null;
            GC.Collect();
            Resources.UnloadUnusedAssets();
            shouldPopulate = false;
            foreach (var p in drawers)
            {
                p.transform.gameObject.SetActive(false);
            }
            actualTab = -1;
            SetTabVisible(0, false);
            isInitalizing = false;
        }

        protected void AddElement(int drawer, Rotations info, HospitalAreaInDrawer areaInDrawer)
        {
            if (!info.infos.spawnFromParent && drawer < drawers.Count)
            {
                if (info.infos.depeningRoom == null)
                {
                    CreateItem(drawer, info, elementPrefab, areaInDrawer);
                }
                else
                {
                    CreateDoubleItem(drawer, info, areaInDrawer);
                }
            }

        }

        private void CreateDoubleItem(int drawer, Rotations info, HospitalAreaInDrawer areaInDrawer)
        {
            Rotations dependentItem = localItemList
                .Where(x => x.infos == info.infos.depeningRoom).First();
            Transform parentItem = CreateItem(drawer, info, elementPrefabDouble, areaInDrawer);
            Transform childItem = CreateItem(drawer, dependentItem, elementPrefab, areaInDrawer);

            childItem.parent = parentItem;
            childItem.localPosition = new Vector3(0, -145, 0);
            parentItem.GetComponentInChildren<DrawerItem>().depeningItem = childItem.GetComponentInChildren<DrawerItem>();
            parentItem.GetComponentInChildren<DrawerItem>().depeningItem.SetUiDependend(false, false);

        }

        private Transform CreateItem(int drawer, Rotations info, GameObject prefab, HospitalAreaInDrawer areaInDrawer)
        {
            createdElement = Instantiate(prefab);
            DrawerItem drawerItem = createdElement.transform.GetComponentInChildren<DrawerItem>();
            allItems.Add(new DrawerRotationItem(drawerItem, info));
            drawerItem.Initialize(info, this, areaInDrawer);
            if (ExtendedCanvasScaler.HasNotch())
            {
                RectTransform objectTransform = createdElement.transform.GetChild(0).GetComponent<RectTransform>();
                objectTransform.offsetMin += new Vector2(-20, 0);
            }
            drawers[drawer].AddElement(createdElement, (int)((ShopRoomInfo)info.infos).SubTabNumber);

            return createdElement.transform;
        }

        #region BADGES CODE

        protected virtual void UpdateTabButtonBadges()
        {
            if (this.unlockedHospitalMachines > 0)
            {
                badgeHospital.SetActive(true);
                badgeHospital.GetComponentInChildren<TextMeshProUGUI>().text = unlockedHospitalMachines.ToString();

            }
            else
                badgeHospital.SetActive(false);


            if (this.unlockedLaboratoryMachines > 0)
            {
                badgeLaboratory.SetActive(true);
                badgeLaboratory.GetComponentInChildren<TextMeshProUGUI>().text = unlockedLaboratoryMachines.ToString();
            }
            else
            {
                if (badgeLaboratory != null)
                {
                    badgeLaboratory.SetActive(false);
                }
            }

            if (this.unlockedPatioMachines > 0)
            {
                badgePatio.SetActive(true);
                badgePatio.GetComponentInChildren<TextMeshProUGUI>().text = unlockedPatioMachines.ToString();
            }
            else
                badgePatio.SetActive(false);

            //ShowAllSubTabBadgeOnCurrentTab();
        }

        protected virtual void DecrementButtonBadges(int tab)
        {
            if (tab == 0)
            {
                //int count = unlockedHospitalMachines
                //int.Parse(badgeHospital.GetComponentInChildren<TextMeshProUGUI>().text, System.Globalization.CultureInfo.InvariantCulture);
                if (unlockedHospitalMachines > 0)
                {
                    unlockedHospitalMachines--;
                    badgeHospital.GetComponentInChildren<TextMeshProUGUI>().text = unlockedHospitalMachines.ToString();
                }
                else
                {
                    //unlockedHospitalMachines = 0;
                    badgeHospital.SetActive(false);
                }

            }
            else if (tab == 1)
            {
                //  int count = int.Parse(badgeLaboratory.GetComponentInChildren<TextMeshProUGUI>().text, System.Globalization.CultureInfo.InvariantCulture);
                if (unlockedLaboratoryMachines > 0)
                {
                    unlockedLaboratoryMachines--;
                    badgeLaboratory.GetComponentInChildren<TextMeshProUGUI>().text = unlockedLaboratoryMachines.ToString();
                }
                else
                {
                    //count = 0;
                    badgeLaboratory.SetActive(false);
                }
            }
            else if (tab == 2)
            {
                //  int count = int.Parse(badgePatio.GetComponentInChildren<TextMeshProUGUI>().text, System.Globalization.CultureInfo.InvariantCulture);
                if (unlockedPatioMachines > 0)
                {
                    unlockedPatioMachines--;
                    badgePatio.GetComponentInChildren<TextMeshProUGUI>().text = unlockedPatioMachines.ToString();
                }
                else
                {
                    //  count = 0;
                    badgePatio.SetActive(false);
                }
            }
        }

        protected void HideTabButtonBadge(HospitalAreaInDrawer name)
        {
            if (name == HospitalAreaInDrawer.Clinic)
            {
                if (badgeHospital != null)
                    badgeHospital.SetActive(false);
            }
            else if (name == HospitalAreaInDrawer.Laboratory)
            {
                if (badgeLaboratory != null)
                    badgeLaboratory.SetActive(false);
            }
            else if (name == HospitalAreaInDrawer.Patio)
            {
                if (badgePatio != null)
                    badgePatio.SetActive(false);
            }
        }

        protected virtual void HideMainButtonBadge()
        {
            badgeMain.SetActive(false);
        }

        protected void ShowTabWithBadge()
        {
            Debug.Log("ShowTabWithBadge");
            if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))
                foreach (Rotations currentBadge in BadgeObjectList)
                {
                    SetTabVisible((int)currentBadge.infos.DrawerArea, false);
                    break;
                }
        }

        protected virtual void ShowAreaTab()
        {
            //Debug.LogError("ShowAreaTab");
            Vector3 camLookingAt = ReferenceHolder.Get().engine.MainCamera.LookingAt;

            if (camLookingAt.x > 52f && camLookingAt.z > 48f)
            {
                CenterCameraToArea(HospitalArea.Laboratory);
                SetTabVisible(1, false);   //lab
            }                       //48
            else if (camLookingAt.x > 44f && camLookingAt.z > 39f && camLookingAt.z < 51f)      //actual patio area starts from x 45
            {
                CenterCameraToArea(HospitalArea.Patio);
                SetTabVisible(2, false);   //patio
            }
            else
            {
                CenterCameraToArea(HospitalArea.Clinic);
                SetTabVisible(0, false);   //clinic
            }
        }

        protected void SetBadgeOnSubTab(int tab, int subtab, int cout)
        {
            if (cout <= 0)
                drawers[tab].subTabs[subtab].badge.SetActive(false);
            else
                drawers[tab].subTabs[subtab].badge.SetActive(true);

            drawers[tab].subTabs[subtab].badge.GetComponentInChildren<TextMeshProUGUI>().text = cout.ToString();
        }

        protected virtual void IncrementBadgeOnSubTab(HospitalAreaInDrawer area, int subtab)
        {
            int tab = (int)area;
            if (tab != 2)
            {
                TextMeshProUGUI badgeText = drawers[tab].subTabs[subtab].badge.GetComponentInChildren<TextMeshProUGUI>();
                int currentItemCount = 0;
                if (int.TryParse(badgeText.text, out currentItemCount))
                {
                    drawers[tab].subTabs[subtab].badge.SetActive(true);
                    currentItemCount++;
                    badgeText.text = currentItemCount.ToString();
                }
            }
        }

        protected void ResetAllSubTabsBadges()
        {
            for (int i = 0; i < 2; i++)
                ResetAllSubTabsOnCurrentTab(i);
        }

        protected void ResetAllSubTabsOnCurrentTab(int actTab)
        {
            if (actTab == 2)
                return;
            else if (actTab == 1)
                for (int j = 0; j < 2; j++)
                    SetBadgeOnSubTab(actTab, j, 0);
            else
                for (int j = 0; j < 2; j++)
                    SetBadgeOnSubTab(actTab, j, 0);
        }

        protected virtual void CenterAtTutorialItem()
        {
            foreach (DrawerRotationItem item in AllDrawerItems())
            {
                if (item.drawerItem.GetInfo.infos.Tag == TutorialController.Instance.GetCurrentStepData().TargetMachineTag && !(TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.DrawerClosed))
                {
                    CenterToItem(item.drawerItem.GetComponent<RectTransform>().parent.parent.gameObject.GetComponent<RectTransform>());
                    TutorialUIController.Instance.BlinkImage(item.drawerItem.image);
                    //NotificationListener.Instance.SubscribeDrawerClosedNotification();
                    break;
                }
            }
        }

        #endregion

        protected int GetCanStoredAmount()
        {
            int amount = ReferenceHolder.Get().floorControllable.GetOwnedFloorColor().Count - 1 - -ReferenceHolder.Get().floorControllable.PremiumFloorColorCounter;
            if (amount < 0)
                return 0;
            else return amount;
        }

        protected void OnScrollRectUsed()
        {
            //interrupt centering coroutine when user manually wants to move up/down item list
            //this is triggered from Scroll Rects -> Event Triggers -> OnBeginDrag
            if (CenteringCoroutine != null)
            {
                Timing.KillCoroutine(CenteringCoroutine.GetType());
                CenteringCoroutine = null;
            }
        }

        protected virtual void SetDeviceLayout()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
            {
                drawerRect.localScale = new Vector3(1.4f, 1.4f, 1f);
                tabsRect.offsetMax = new Vector3(0, 25);

                exitButtonAnchor.anchorMin = new Vector2(.57f, .31f);
                exitButtonAnchor.anchorMax = new Vector2(.57f, .31f);
                exitButtonAnchor.anchoredPosition = Vector2.zero;

                if (tabButtons.Length > 0)
                    tabButtons[0].anchoredPosition = new Vector3(-172, -240, 0);
                if (tabButtons.Length > 1)
                    tabButtons[1].anchoredPosition = new Vector3(-172, -180, 0);
                if (tabButtons.Length > 2)
                    tabButtons[2].anchoredPosition = new Vector3(-172, -120, 0);

                for (int i = 0; i < tabButtons.Length; i++)
                    tabButtons[i].localScale = new Vector3(.85f, .85f, 1f);

                for (int i = 0; i < tabTitles.Length; i++)
                    tabTitles[i].SetActive(false);

                for (int i = 0; i < scrollRects.Length; i++)
                {
                    scrollRects[i].offsetMin = new Vector3(0, 158);
                    scrollRects[i].offsetMax = new Vector3(0, 8);
                }

                for (int i = 0; i < grids.Length; i++)
                    grids[i].spacing = new Vector2(0, 8);
            }
            else
            {
                drawerRect.localScale = Vector3.one;
                tabsRect.offsetMax = Vector3.zero;

                exitButtonAnchor.anchorMin = new Vector2(.55f, .05f);
                exitButtonAnchor.anchorMax = new Vector2(.55f, .05f);
                exitButtonAnchor.anchoredPosition = Vector2.zero;

                if (tabButtons.Length > 0)
                    tabButtons[0].anchoredPosition = new Vector3(-172, -335, 0);
                if (tabButtons.Length > 1)
                    tabButtons[1].anchoredPosition = new Vector3(-172, -265, 0);
                if (tabButtons.Length > 2)
                    tabButtons[2].anchoredPosition = new Vector3(-172, -195, 0);

                for (int i = 0; i < tabButtons.Length; i++)
                    tabButtons[i].localScale = Vector3.one;

                for (int i = 0; i < tabTitles.Length; i++)
                    tabTitles[i].SetActive(true);

                for (int i = 0; i < scrollRects.Length; i++)
                {
                    scrollRects[i].offsetMin = Vector3.zero;
                    scrollRects[i].offsetMax = Vector3.zero;
                }

                for (int i = 0; i < grids.Length; i++)
                    grids[i].spacing = new Vector2(0, 20);
            }

            if (ExtendedCanvasScaler.HasNotch())
            {
                for (int i = 0; i < 3; i++)
                    tabTitles[i].SetActive(false);
            }

            UpdateSeparatorPos();//
        }

        protected virtual void UpdateSeparatorPos()
        {
            if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
            //if (true)
            {
                if (actualTab == 2)
                {
                    separator.anchorMin = new Vector2(0, .831f);
                    separator.anchorMax = new Vector2(1, .831f);
                }
                else
                {
                    separator.anchorMin = new Vector2(0, .736f);
                    separator.anchorMax = new Vector2(1, .736f);
                }
            }
            else
            {
                if (actualTab == 2)
                {
                    separator.anchorMin = new Vector2(0, .82f);
                    separator.anchorMax = new Vector2(1, .82f);
                }
                else
                {
                    separator.anchorMin = new Vector2(0, .725f);
                    separator.anchorMax = new Vector2(1, .725f);
                }
            }
        }

        public bool IsDraggingAnything()
        {
            return rObj;
        }

        public class DrawerRotationItem
        {
            public DrawerItem drawerItem;
            public Rotations rotations;

            public DrawerRotationItem(DrawerItem i, Rotations r)
            {
                drawerItem = i;
                rotations = r;
            }
        }

    }

    #region Classes

    public enum UpdateType
    {
        Default,
        Can,
        HospitalRoom,
        Decorations
    }

    public class EntryInfo
    {
        public readonly int objID;
        public readonly string title;
        public readonly string description;
        public readonly Sprite image;
        public readonly int tabNumber;
        public readonly int cost;
        public readonly BuildDummyType type;
        public readonly int controllerID;
        public EntryInfo(int ObjID, BuildDummyType DummyType, int ControllerID, string Title, string Descritpion, Sprite Image, int TabNumber, int Cost)
        {
            objID = ObjID;
            title = Title;
            description = Descritpion;
            image = Image;
            tabNumber = TabNumber;
            type = DummyType;
            cost = Cost;
            controllerID = ControllerID;
        }
    }

    #endregion

}



using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using IsoEngine;
using SimpleUI;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using MovementEffects;
using Maternity;
using System.Globalization;

namespace Hospital
{

    public class RotatableObject : MonoBehaviour, IPricedItem, ILevelUnlockableItem, IBuilding
    {
        public enum State
        {
            fresh,
            building,
            waitingForUser,
            working,
        }

        #region VariablesAndProperties
        public string Tag
        {
            get;
            set;
        }
        public bool Anchored
        {
            get;
            set;
        }
        public static AreaMapController map;
        public static bool visitingMode = false;
        public static EngineController eng;
        public IsoObject isoObj;
        private float buildTimeLeft;

        ///<summary>An array witch contains information about checked rotations for current object in MakeDummy.</summary>
        private int[] disabledRotation = { 0, 0, 0, 0 };

        public float BuildStartTime
        {
            get { return buildTimeLeft; }
        }
        public float BuildTime
        {
            get { return buildTime; }
        }

        private float buildTime;
        private BuildingHover buildingHover;
        //private Rotations rots;
        ProgressBarController progressBar;
        Borders perObjectBorder;
        Borders perObjectMutalBorder;

        private static Borders sharedSelectBorder;
        private static Borders sharedMutalBorder;

        public Borders border
        {
            get
            {
                if (EnableManyBorders())
                {
                    return perObjectBorder;
                }

                return sharedSelectBorder;
            }
        }

        public Borders mutalBorder
        {
            get
            {
                if (EnableManyBorders())
                {
                    return perObjectMutalBorder;
                }

                return sharedMutalBorder;
            }
        }

        public static void InitSharedBorders()
        {
            if (EnableManyBorders())
            {
                return;
            }

            if (sharedSelectBorder == null)
            {
                sharedSelectBorder = GameObject.Instantiate(ResourcesHolder.Get().bordersPrefab).GetComponent<Borders>();
                sharedSelectBorder.gameObject.SetActive(false);
            }

            if (sharedMutalBorder == null)
            {
                sharedMutalBorder = GameObject.Instantiate(ResourcesHolder.Get().bordersPrefab).GetComponent<Borders>();
                sharedMutalBorder.gameObject.SetActive(false);
            }
        }

        Rotation defaultRotation;
        protected CollectablesPositions collectablesPos;
        public MasterableProperties masterableProperties;

        private IsoObjectPrefabData _actualData;

        public IsoObjectPrefabData actualData
        {
            get
            {
                return _actualData;
            }
            protected set
            {
                _actualData = value;
                area = _actualData.area;

            }
        }

        protected OnClickVisualEffects onClickVisualEffects;

        public virtual void OnLoadEnded()
        {
            OnEmulationEnded();
        }

        public Rotations info;

        public State st;

        public State state
        {
            get
            {
                return st;
            }
            protected set
            {
                OnStateChange(value, st);
                st = value;

            }
        }

        public Rotation actualRotation
        {
            get;
            set;
        }

        public Rotation firstOnMapRotation
        {
            get;
            private set;
        }

        public void setStateAnywayAsItsWorking()
        {
            if (state != State.working)
                state = State.working;

            AddedToMap = true;
        }

        protected GameObject obj;

        public GameObject GetObj()
        {
            return map.GetObject(position);
        }

        public HospitalArea area
        {
            get;
            set;
        }

        public bool availableInVisitingMode
        {
            get;
            set;
        }

        public Vector2i HalfSize
        {
            get;
            private set;
        }

        public bool AddedToMap
        {
            get;
            private set;
        }

        public bool IsDummy
        {
            get;
            private set;
        }

        public Vector2i position
        {
            get;
            set;
        }

        public bool ProperlySet
        {
            get
            {
                return obj == null;
            }
        }

        protected List<Collectable> collectables;
        Coroutine synchroCoroutine;

        bool hardShow = false;

        protected GameObject temporaryObj;
        protected GameObject firstObj;
        State oldState;

        public delegate void MedicineCollectedAction(bool medicineCollected);
        public static event MedicineCollectedAction OnMedicineCollected;

        public static void FireOnMedicineCollected()
        {
            //this will trigger UpdateAllBedsIndicators on all HospitalBedControllers and will set isNewCureAvailable bool
            if (OnMedicineCollected != null)
                OnMedicineCollected(true);
        }
        #endregion

        public int GetPriceInCoins(float diamondToCoinConversionRate)
        {
            ShopRoomInfo shopRoomInfo = (ShopRoomInfo)info.infos;
            int cost = shopRoomInfo.cost;
            int costInDiamonds = shopRoomInfo.costInDiamonds;
            if (this is Decoration)
            {
                int amountOfDecoOnPlayerPosess;
                BaseGameState.StoredObjects.TryGetValue(Tag, out amountOfDecoOnPlayerPosess);
                int amountOfDecoOnMap;
                AreaMapController.Map.decoAmountMap.TryGetValue(Tag, out amountOfDecoOnMap);
                amountOfDecoOnPlayerPosess += amountOfDecoOnMap;
                DecorationInfo decoInfo = (DecorationInfo)shopRoomInfo;
                cost += (amountOfDecoOnPlayerPosess - 1) * decoInfo.goldIncreasePerOwnedItem;

                cost += Mathf.CeilToInt(costInDiamonds * diamondToCoinConversionRate);
            }
            else if (this is HospitalRoom)
            {
                cost = AlgorithmHolder.GetCostInGoldForHospitalRoom(true);
            }

            return cost;
        }
        public bool IsUnlocked()
        {
            return GameState.Get().hospitalLevel >= GetUnlockLevel();
        }

        public int GetUnlockLevel()
        {
            try
            {
                return ((ShopRoomInfo)info.infos).unlockLVL;
            }
            catch
            {
                return 0;
            }
        }

        public BuildDummyType GetBuildingType()
        {
            return ((ShopRoomInfo)info.infos).dummyType;
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public virtual void SetAnchored(bool value)
        {
            Anchored = value;
        }

        protected virtual void OnStateChange(State newState, State oldState)
        {
        }

        public virtual void IsoDestroy()
        {
            if (Anchored)
                SetAnchored(false);

            RemoveFromMap();
            AreaMapController.Map.RemoveRotatableObject(this);

            if (obj != null)
                GameObject.Destroy(obj);

            if (!AreaMapController.Map.VisitingMode && BaseGameState.BuildedObjects.ContainsKey(Tag))
            {
                BaseGameState.BuildedObjects[Tag] -= 1;
            }
            RemoveBorderMaterials();

            if (temporaryObj != null)
            {
                temporaryObj.SetActive(false);
                GameObject.Destroy(temporaryObj);
            }

            GameObject.Destroy(gameObject);
        }

        private void RemoveBorderMaterials()
        {
            if (border != null && border.gameObject != null)
            {
                border.DestroyMaterial();
                GameObject.Destroy(border.gameObject);
            }
            if (mutalBorder != null && mutalBorder.gameObject != null)
            {
                mutalBorder.DestroyMaterial();
                GameObject.Destroy(mutalBorder.gameObject);
            }
        }

        protected virtual string GetInfo()
        {
            return "";
        }

        static public bool EnableManyBorders()
        {
#if UNITY_IOS
            return UnityEngine.iOS.Device.generation != UnityEngine.iOS.DeviceGeneration.iPhone6;
#else
            return true;
#endif
        }

        public bool ShouldCreateBorders()
        {
            return EnableManyBorders() && !HospitalAreasMapController.HospitalMap.VisitingMode;
        }

        public virtual void Buy(Action<bool> OnSuccess, bool afterMissingResources = false, Action OnFailure = null)
        {
            int cost = ((ShopRoomInfo)info.infos).cost;
            bool isTutorialForMaternity = false;
            if (((ShopRoomInfo)info.infos).Tag == "WaitingRoomBlueOrchid" || ((ShopRoomInfo)info.infos).Tag == "LabourRoomBlueOrchid")
            {
                cost = 0;
                isTutorialForMaternity = true;
            }

            if (((ShopRoomInfo)info.infos).Tag == "ElixirLab")
            {
                if (BaseGameState.BuildedObjects.ContainsKey("ElixirLab"))
                {
                    if (BaseGameState.BuildedObjects["ElixirLab"] == 2)
                    {

                        cost = 3200;
                    }
                }
            }

            int diamondCost = ((ShopRoomInfo)info.infos).costInDiamonds;

            if (this is Decoration)
            {
                int amountOfDecoOnPlayerPosess = 0;
                BaseGameState.StoredObjects.TryGetValue(Tag, out amountOfDecoOnPlayerPosess);
                int amountOfDecoOnMap = 0;
                AreaMapController.Map.decoAmountMap.TryGetValue(Tag, out amountOfDecoOnMap);
                amountOfDecoOnPlayerPosess += amountOfDecoOnMap;
                DecorationInfo decoInfo = ((DecorationInfo)info.infos);
                cost += (amountOfDecoOnPlayerPosess - 1) * decoInfo.goldIncreasePerOwnedItem;

                if (diamondCost == 0) { }
                //UIController.get.drawer.UpdatePriceForRotatableInDrawer(this, cost + decoInfo.goldIncreasePerOwnedItem);
            }
            else if (this is HospitalRoom)
            {
                cost = AlgorithmHolder.GetCostInGoldForHospitalRoom(true);
                if (diamondCost == 0) { }
                //UIController.get.drawer.UpdatePriceForRotatableInDrawer(this, cost);
            }

            if (cost > 0 || isTutorialForMaternity)
            {
                if (BaseGameState.StoredObjects.ContainsKey(Tag))
                {
                    Vector3 pos = new Vector3(HalfSize.x, 0, HalfSize.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[0]);
                    BaseGameState.StoredObjects[Tag]--;
                    UIController.get.drawer.UpdatePriceForRotatableInDrawer(this, cost);
                    if (BaseGameState.StoredObjects[Tag] < 1)
                    {
                        BaseGameState.StoredObjects.Remove(Tag);
                    }

                    AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Restored, Tag, area, this is Decoration, false, -1);
                    OnSuccess?.Invoke(true);
                    return;
                }

                if (Game.Instance.gameState().GetCoinAmount() >= cost)
                {
                    //Debug.Log("I can buy this");
                    if (afterMissingResources)
                        Game.Instance.gameState().RemoveCoins(cost, EconomySource.DrawerPurchaseAfterMissing, true, Tag);
                    else
                        Game.Instance.gameState().RemoveCoins(cost, EconomySource.DrawerPurchase, true, Tag);

                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, actualData.tilesX / 2f, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[0]);
                    //Achievement
                    if (info.infos.GetType() == typeof(DecorationInfo))
                        AchievementNotificationCenter.Instance.CoinsInvestedInDecorating.Invoke(new AchievementProgressEventArgs(cost));

                    if (this is Decoration)
                    {
                        int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
                        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                        Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                        });
                    }

                    SaveSynchronizer.Instance.MarkToSave(SavePriorities.BuildingPurchased);
                    //ToDo Materward
                    //AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Build, Tag, area, false, this.GetType() == typeof(Decoration), -1);
                    if (OnSuccess != null)
                    {
                        OnSuccess.Invoke(false);
                        return;
                    }
                }
                else
                {
                    UIController.get.BuyResourcesPopUp.Open(cost - Game.Instance.gameState().GetCoinAmount(), () =>
                    {
                        gameObject.GetComponent<RotatableSimpleController>().MissingResourcesCallback(true);

                        if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
                        {
                            for (int i = 0; i < BasePatientAI.patients.Count; i++)
                            {
                                if (BasePatientAI.patients[i] != null)
                                {
                                    BasePatientAI.patients[i].Notify((int)StateNotifications.OfficeAnchored, true);
                                }
                            }
                        }
                    }, () =>
                    {
                        gameObject.GetComponent<RotatableSimpleController>().MissingResourcesCallback(false);

                        if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
                        {
                            for (int i = 0; i < BasePatientAI.patients.Count; i++)
                            {
                                if (BasePatientAI.patients[i] != null)
                                {
                                    BasePatientAI.patients[i].Notify((int)StateNotifications.OfficeAnchored, true);
                                }
                            }
                        }

                    });
                    OnFailure?.Invoke();
                    return;
                }
            }
            else if (diamondCost > 0)
            {
                if (BaseGameState.StoredObjects.ContainsKey(Tag))
                {
                    BaseGameState.StoredObjects[Tag]--;
                    if (BaseGameState.StoredObjects[Tag] < 1)
                    {
                        BaseGameState.StoredObjects.Remove(Tag);
                    }

                    AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Restored, Tag, area, this is Decoration, afterMissingResources, -1);
                    OnSuccess?.Invoke(true);
                    return;
                }

                if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
                {
                    Debug.Log("I can buy this " + Time.time);
                    if (afterMissingResources)
                        Game.Instance.gameState().RemoveDiamonds(diamondCost, EconomySource.DrawerPurchaseAfterMissing, Tag);
                    else
                        Game.Instance.gameState().RemoveDiamonds(diamondCost, EconomySource.DrawerPurchase, Tag);

                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, actualData.tilesX / 2f, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, diamondCost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);

                    if (this is Decoration)
                    {
                        int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
                        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                        Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                        {
                            Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                        });
                    }

                    AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Build, Tag, area, this is Decoration, afterMissingResources, -1);
                    OnSuccess?.Invoke(false);
                    return;
                }
                else
                {
                    gameObject.GetComponent<RotatableSimpleController>().MissingResourcesCallback(false);

                    if (BasePatientAI.patients != null && BasePatientAI.patients.Count > 0)
                    {
                        for (int i = 0; i < BasePatientAI.patients.Count; i++)
                        {
                            if (BasePatientAI.patients[i] != null)
                            {
                                BasePatientAI.patients[i].Notify((int)StateNotifications.OfficeAnchored, true);
                            }
                        }
                    }

                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                    OnFailure?.Invoke();
                    return;
                }
            }
            else
            {
                if (BaseGameState.StoredObjects.ContainsKey(Tag))
                {
                    Vector3 pos = new Vector3(HalfSize.x, 0, HalfSize.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[0]);
                    BaseGameState.StoredObjects[Tag]--;
                    if (BaseGameState.StoredObjects[Tag] < 1)
                    {
                        BaseGameState.StoredObjects.Remove(Tag);
                    }

                    AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Restored, Tag, area, this is Decoration, false, -1);
                    OnSuccess?.Invoke(true);
                    return;
                }
            }

            Debug.LogError("I cannot buy this");
            gameObject.GetComponent<RotatableSimpleController>().MissingResourcesCallback(false);
            OnFailure?.Invoke();
        }

        public virtual void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            Tag = info.infos.Tag;
            gameObject.name = Tag;
            this.info = info;
            //rots = info;
            this.state = _state;

            Anchored = false;
            GetMapReference();
            AddController(typeof(RotatableSimpleController), shouldDisappear);
            this.position = position;
            actualRotation = rotation;
            defaultRotation = rotation;
            availableInVisitingMode = info.infos.availableInVisitingMode;
            IsoObjectPrefabController isoController = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>();
            actualData = isoController.prefabData;

            if (ShouldCreateBorders())
            {
                perObjectBorder = GameObject.Instantiate(ResourcesHolder.Get().bordersPrefab).GetComponent<Borders>();
                if (border != null)
                {
                    border.transform.localPosition = new Vector3(-0.5f, 0, -0.5f);
                    SetBorderActive(false);
                }
            }

            HalfSize = new Vector2i((actualData.tilesX - 1) / 2, (actualData.tilesY - 1) / 2);

            if (info.infos.UsesCollectables)
            {
                collectablesPos = Instantiate(info.infos.CollectablesPositions).GetComponent<CollectablesPositions>();
                collectablesPos.transform.SetParent(transform);

            }
            AddToMap();
            AddToObjectsBase();
        }

        private void AddToObjectsBase()
        {
            if (AreaMapController.Map.VisitingMode)
            {
                return;
            }
            var oData = BaseGameState.BuildedObjects;
            if (oData.ContainsKey(Tag))
            {
                oData[Tag] = oData[Tag] + 1;
            }
            else
                oData.Add(Tag, 1);
        }

        private void AddToObjectStored()
        {
            var oData = BaseGameState.StoredObjects;
            if (oData.ContainsKey(Tag))
            {
                oData[Tag] = oData[Tag] + 1;
            }
            else
                oData.Add(Tag, 1);
        }

        protected virtual void InitializeFromSave(string save, Rotations info, TimePassedObject timePassed)
        {
            var strs = save.Split(';');
            //print(tag+" "+save[tag]);
            var settings = strs[0].Split('/');
            var pos = Vector2i.Parse(settings[0]);
            transform.position = new Vector3(pos.x, 0, pos.y);
            Rotation rotation = (Rotation)Enum.Parse(typeof(Rotation), settings[1]);
            var stt = (State)Enum.Parse(typeof(State), settings[2]);
            Tag = info.infos.Tag;
            gameObject.name = Tag;
            //rots = this.info;
            this.info = info;

            Anchored = false;
            GetMapReference();
            AddController(typeof(RotatableSimpleController), false);
            GetComponent<RotatableSimpleController>().Anchor();
            this.position = pos;
            actualRotation = rotation;
            defaultRotation = rotation;
            availableInVisitingMode = info.infos.availableInVisitingMode;

            state = stt;
            IsoObjectPrefabController isoController = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>();
            actualData = isoController.prefabData;

            if (ShouldCreateBorders())
            {
                perObjectBorder = GameObject.Instantiate(ResourcesHolder.Get().bordersPrefab).GetComponent<Borders>();

                if (border != null)
                {
                    border.SetBorderSize(actualData.tilesX, actualData.tilesY);
                    SetBorderActive(false);
                }
            }

            if (info.infos.UsesCollectables)
            {
                collectablesPos = Instantiate(info.infos.CollectablesPositions).GetComponent<CollectablesPositions>();
                collectablesPos.transform.SetParent(transform);

            }
            if (state == State.building)
            {
                buildTimeLeft = float.Parse(settings[3], CultureInfo.InvariantCulture);
                buildTime = ((ShopRoomInfo)info.infos).buildTimeSeconds;
                /*if (timePassed > buildTimeLeft)
                {
                    timePassed -= buildTimeLeft;
                    buildTimeLeft = -1;
                }
                else
                {
                    buildTimeLeft -= timePassed;
                    timePassed = 0;
                }*/
            }
            HalfSize = new Vector2i((actualData.tilesX - 1) / 2, (actualData.tilesY - 1) / 2);

            AddToMap();



            LoadFromString(save, timePassed);

            SetAnchored(true);
            AddToObjectsBase();
            if (settings.Length > 4)
            {
                var collectablesStrs = settings[4].Split('&');

                for (int i = 0; i < collectablesStrs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(collectablesStrs[i]))
                    {
                        var collectableInfo = collectablesStrs[i].Split('!');
                        if (collectableInfo[1] == "null")
                        {
                            CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[0], null, int.Parse(collectableInfo[0], System.Globalization.CultureInfo.InvariantCulture), false);
                        }
                        else if (collectableInfo[1] == "PEnergy")
                        {
                            CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[4], null, int.Parse(collectableInfo[0], System.Globalization.CultureInfo.InvariantCulture), true);
                        }
                        else if (collectableInfo[1] == "BloodResult")
                        {
                            string patientID = collectableInfo[2];
                            CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[6], null, int.Parse(collectableInfo[0], System.Globalization.CultureInfo.InvariantCulture), false, patientID);
                        }
                        else
                        {
                            CreateCollectable(ResourcesHolder.Get().GetSpriteForCure(MedicineRef.Parse(collectableInfo[1])), MedicineRef.Parse(collectableInfo[1]), int.Parse(collectableInfo[0], System.Globalization.CultureInfo.InvariantCulture), false);
                        }

                    }
                }
            }
            if (!VisitingController.Instance.IsVisiting)
            {
                UIController.get.drawer.LockBuildItem(AreaMapController.Map.GetPrefabInfo(Tag));
            }


        }

        protected virtual void LoadFromString(string str, TimePassedObject timePassed, int actionsDone = 0)
        {

        }

        public virtual void LoadFromStringAfterAllRoomsLoaded(string str, TimePassedObject timePassed)
        {

        }

        protected virtual void SetupMasterableProperties() { }

        protected virtual string SaveToString()
        {
            string p = "";
            Checkers.CheckedIsNotDummy(IsDummy, Tag);
            //	try
            //{
            p = Checkers.CheckedPosition(position, Tag).ToString();
            //}
            //	catch (WrongPositionException wrongPositionException)
            /*	{
                    throw new SaveErrorException ("WrongPositionException occured");
                }*/

            p += "/";
            p += Checkers.CheckedActualRotation(actualRotation, Tag).ToString();
            p += "/";
            p += Checkers.CheckedState(state, Tag).ToString();

            if (state == State.building)
            {
                p += "/";
                p += Checkers.CheckedAmount(buildTimeLeft, -1.0f, float.MaxValue, Tag + " buildTimeLeft ").ToString("n2");
            }
            else
                p += "/" + "null";

            p = SaveCollectables(p);
            return p;
        }

        protected virtual string SaveCollectables(string p)
        {
            StringBuilder savedCollectables = new StringBuilder();
            if (collectables != null)
            {
                p += "/";
                for (int i = 0; i < collectables.Count; i++)
                {

                    savedCollectables.Append(Checkers.CheckedAmount(collectables[i].amount, 0, int.MaxValue, "Collectables amount"));
                    savedCollectables.Append("!");
                    if (collectables[i].medicine != null)
                    {
                        savedCollectables.Append(Checkers.CheckedMedicine(collectables[i].medicine, Tag)).ToString();
                    }
                    else
                    {
                        if (collectables[i].isPositiveEnergy)
                            savedCollectables.Append("PEnergy");
                        else
                            savedCollectables.Append("null");
                    }
                    if (i < collectables.Count - 1)
                    {
                        savedCollectables.Append("&");
                    }

                    p += savedCollectables.ToString();
                    savedCollectables.Length = 0;
                    savedCollectables.Capacity = 0;
                }
            }

            return p;
        }

        public virtual void Notify(int id, object parameters = null)
        {

        }

        private void Update()
        {
            IsoUpdate();
        }

        protected void OnDestroy()
        {
            CancelInvoke();
            if (masterableProperties != null)
            {
                masterableProperties.IsoDestroy();
            }
        }

        public virtual void EmulateTime(TimePassedObject timePassed)
        {
            float time = timePassed.GetTimePassed();
            if (state == State.building)
            {
                if (time > buildTimeLeft)
                {
                    time -= buildTimeLeft;
                    buildTimeLeft = 0;
                }
                else
                {
                    buildTimeLeft -= time;
                    time = 0;
                }
            }
        }

        public virtual void OnEmulationEnded()
        {

        }

        public virtual void IsoUpdate()
        {
            //INFO Building or making something
            if (state == State.building)
            {
                buildTimeLeft -= Time.deltaTime;
                if (buildTimeLeft < 0)
                {
                    buildTimeLeft = 0;
                }
                if (progressBar != null)
                    progressBar.SetValue((buildTime - buildTimeLeft));
                if (buildTimeLeft <= 0 && !ReferenceHolder.Get().engine.MainCamera.dragging && !IsDummy)
                {
                    NotificationCenter.Instance.FinishedBuilding.Invoke(new FinishedBuildingEventArgs(this));
                    var ret = Anchored;
                    Anchored = false;
                    RemoveFromMap();
                    Anchored = ret;
                    state = State.waitingForUser;
                    AddToMap();
                    SoundsController.Instance.PlayAlert();
                    if (progressBar != null)
                    {
                        BuildingHover.activeHover.Close();
                        AreaMapController.Map.ResetOnPressAction();
                        progressBar = null;
                    }
                }
            }
        }


        #region Borders

        public void SetBorderActive(bool state, bool hardShow = false)
        {

            if (this.hardShow && !hardShow)
                return;
            if (hardShow)
            {
                if (!state)
                    this.hardShow = false;
                else
                    this.hardShow = true;
            }
            if (border != null)
            {
                if (border.gameObject != null)
                    border.gameObject.SetActive(state);

                border.transform.SetParent(transform);
                border.transform.localPosition = new Vector3(-0.5f, 0, -0.5f);
                border.SetBorderSize(actualData.tilesX, actualData.tilesY);

                border.SetBorderColor(0, actualRotation);

                if (mutalBorder != null)
                    mutalBorder.SetBorderColor(3, actualRotation);

                border.transform.position = new Vector3(border.transform.position.x, 0, border.transform.position.z);
            }

            if (actualData != null && actualData.mutalTiles != MutalType.None)
            {
                if (mutalBorder != null)
                {
                    mutalBorder.gameObject.transform.SetParent(transform);
                    mutalBorder.gameObject.SetActive(state);
                    UpdateMutal();
                }
                else
                {
                    if (ShouldCreateBorders())
                    {
                        perObjectMutalBorder = GameObject.Instantiate(ResourcesHolder.Get().bordersPrefab).GetComponent<Borders>();

                        if (mutalBorder.gameObject != null)
                        {
                            mutalBorder.gameObject.transform.SetParent(transform);
                            mutalBorder.gameObject.SetActive(state);
                        }

                        UpdateMutal();
                    }
                }
            }
        }

        public void UpdateMutal()
        {
            if ((mutalBorder != null) && (border != null) && (actualData != null))
            {
                switch ((MutalType)actualData.mutalTiles)
                {
                    case MutalType.North:
                        mutalBorder.SetBorderSize(1, actualData.tilesY);
                        mutalBorder.transform.localPosition = new Vector3(-0.5f + actualData.tilesX, -0.005f, -0.5f);
                        break;
                    case MutalType.South:
                        mutalBorder.SetBorderSize(1, actualData.tilesY);
                        mutalBorder.transform.localPosition = new Vector3(-1.5f, -0.005f, -0.5f);
                        break;
                    case MutalType.West:
                        mutalBorder.SetBorderSize(actualData.tilesX, 1);
                        mutalBorder.transform.localPosition = new Vector3(-0.5f, -0.005f, -0.5f + actualData.tilesY);
                        break;
                    default:
                        mutalBorder.SetBorderSize(actualData.tilesX, 1);
                        mutalBorder.transform.localPosition = new Vector3(-0.5f, -0.005f, -1.5f);
                        break;
                }
                mutalBorder.name = "MutalBorder";
            }
        }

        public void SetBorderColor(bool ok = true)
        {
            if (Anchored)
            {
                if (border != null)
                {
                    border.SetBorderColor(0, actualRotation);
                    border.transform.position = new Vector3(border.transform.position.x, 0, border.transform.position.z);
                }

                if (mutalBorder != null)
                    mutalBorder.SetBorderColor(3, actualRotation);
                return;
            }

            if (border != null)
                border.SetBorderColor(ok ? 1 : 2, actualRotation);

            if (mutalBorder != null)
                mutalBorder.SetBorderColor(ok ? 3 : 2, actualRotation);
        }

        #endregion

        #region HoversAndCamera

        protected virtual Vector2 GetHoverPosition()
        {
            return new Vector2(position.x, position.y);
        }

        public void MoveCameraToShowHover() //Here we set the position we want to set for the movement.
        {
            RectTransform hoverFrame = GetHoverFrame();
            if (hoverFrame == null)
                hoverFrame = GetBuildingHoverContainer();

            Vector3 newPos = UIController.get.GetCameraPositionForHover(hoverFrame);

            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(newPos.x, 0, newPos.z), 1.0f, false, false, 7, hoverFrame);
        }

        public void MoveCameraToMachine(Vector2 machinePosition)
        {
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(machinePosition.x, 0, machinePosition.y), 1.0f, false);
        }

        public virtual void MoveCameraToThisRoom()
        {
            ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(transform.position.x, 0, transform.position.z), 1.0f, false);
        }

        public virtual RectTransform GetHoverFrame()
        {
            return null;
        }

        public RectTransform GetBuildingHoverContainer()
        {
            if (buildingHover == null)
                return null;

            return buildingHover.hoverFrame;
        }
        #endregion

        #region Interaction 
        public void OnClickSpeedUp(IDiamondTransactionMaker diamondTransactionMaker)
        {
            BuyWithDiamonds(diamondTransactionMaker);
        }

        protected virtual void OnClickBuilding()
        {
            ReferenceHolder.Get().engine.GetMap<AreaMapController>().ChangeOnTouchType(x =>
            {
                BuildingHover.GetActive().Close();
                buildingHover = null;

                if (GetBorder() != null)
                    SetBorderActive(false);

                progressBar = null;
            });
            buildingHover = BuildingHover.Open(buildTime, this, I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)info.infos).ShopTitle));
            progressBar = buildingHover.GetProgressBar();
            if (GetBorder() != null)
                SetBorderActive(true);

            buildingHover.SetWorldPointHovering(new Vector3(GetHoverPosition().x + HalfSize.x, 0, GetHoverPosition().y + HalfSize.y));
            buildingHover.UpdateAccordingToMode();
            MoveCameraToShowHover();

            TutorialController tutorialController = TutorialController.Instance;
            if (tutorialController != null && tutorialController.tutorialEnabled)
            {
                bool isBuildDoctorFinishStepEnabled = tutorialController.GetCurrentStepData().StepTag == StepTag.build_doctor_finish && this is DoctorRoom;
                bool isBuildWaitingRoomBlueOrchidFinishStepEnabled = tutorialController.GetCurrentStepData().StepTag == StepTag.maternity_waiting_room_finish && this is MaternityWaitingRoom;
                bool isBuildLaborRoomBlueOrchidFinishStepEnabled = tutorialController.GetCurrentStepData().StepTag == StepTag.maternity_labor_room_finish && this is MaternityLabourRoom;
                if (isBuildDoctorFinishStepEnabled || isBuildWaitingRoomBlueOrchidFinishStepEnabled || isBuildLaborRoomBlueOrchidFinishStepEnabled)
                {
                    TutorialUIController.Instance.HideIndicator();
                }
            }
        }

        protected virtual void OnClickWorking()
        {
        }

        protected virtual void OnClickWaitForUser()
        {
            Anchored = false;
            RemoveFromMap();
            Anchored = true;
            state = State.working;
            actualData = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>().prefabData;
            AddToMap();
            var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(position.x + actualData.rotationPoint.x, 0, position.y + actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one * 1.4f;
            fp.SetActive(true);
            ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, Tag, ObjectiveRotatableEventArgs.EventType.Unwrap));
        }

        public void OnClick(bool forceClick = false)
        {
            if (!forceClick)
            {
                if (Input.touchCount > 0)
                {
                    if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                        return;
                }
                else
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                        return;
                }
            }

            NotificationCenter.Instance.ObjectSelected.Invoke(new ObjectEventArgs(this));

            switch (state)
            {
                case State.fresh:
                    break;
                case State.building:
                    OnClickBuilding();
                    break;
                case State.waitingForUser:
                    OnClickWaitForUser();
                    break;
                case State.working:
                    OnClickWorking();
                    break;
                default:
                    break;
            }

            SoundsController.Instance.PlaySoundOnSelected(this);
        }

        protected virtual void BuyWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = GetSpeedUpCost();
            //Debug.LogError("BuyWithDiamonds cost: " + cost);
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    if (BuildingHover.activeHover != null)
                        BuildingHover.activeHover.Close();
                    AreaMapController.Map.ResetOntouchAction();
                    progressBar = null;

                    Game.Instance.gameState().RemoveDiamonds(cost, EconomySource.SpeedUpBuilding, Tag);
                    buildTimeLeft = -1;
                    if (actualData != null)
                    {
                        Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1, transform.position.z + actualData.rotationPoint.y);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    }
                    else
                    {
                        Vector3 pos = new Vector3(transform.position.x, 1, transform.position.z);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                        throw new IsoException("Fatal failure - actualData of object problem for BuyWithDiamonds 'cuz it's null !");
                    }
                    NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                    NotificationCenter.Instance.FinishedBuilding.Invoke(new FinishedBuildingEventArgs(this));

                    var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleDiamondBuilding, new Vector3(position.x + actualData.rotationPoint.x, 0, position.y + actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                    fp.transform.localScale = Vector3.one * 1.4f;
                    fp.SetActive(true);
                    Destroy(fp, 5.0f);
                    //  SaveDynamoConnector.Instance.ee();
                }, diamondTransactionMaker);


            }
            else
            {
                if (BuildingHover.activeHover != null)
                    BuildingHover.activeHover.Close();
                AreaMapController.Map.ResetOntouchAction();
                progressBar = null;
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public int GetSpeedUpCost()
        {
            int cost = DiamondCostCalculator.GetCostForBuilding(buildTimeLeft, ((ShopRoomInfo)info.infos).buildTimeSeconds, Tag);
            return cost;
        }
        #endregion


        #region MapManipulation


        private void UpdateDiagnosisSprite()
        {
            if (this.state == State.building || this.state == State.waitingForUser)
            {
                GameObject mapObj = map.GetObject(position);
                if (mapObj != null && info.infos.dummyType == BuildDummyType.DiagnosticRoom)
                {
                    DiagnosticSpriteSetter spriteSetter = mapObj.GetComponent<DiagnosticSpriteSetter>();
                    if (spriteSetter == null)
                        return;

                    //Debug.LogWarning("info.infos.dummyType: " + info.infos.dummyType);
                    //Debug.LogWarning("rotatable obj name: " + gameObject.name);
                    if (((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom == HospitalDataHolder.DiagRoomType.XRay)
                    {
                        spriteSetter.SetBoxFrontSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[0]);
                        spriteSetter.SetBoxBackSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[1]);
                    }
                    else if (((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom == HospitalDataHolder.DiagRoomType.UltraSound)
                    {
                        spriteSetter.SetBoxFrontSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[2]);
                        spriteSetter.SetBoxBackSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[3]);
                    }
                    else if (((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom == HospitalDataHolder.DiagRoomType.MRI)
                    {
                        spriteSetter.SetBoxFrontSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[4]);
                        spriteSetter.SetBoxBackSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[5]);
                    }
                    else if (((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom == HospitalDataHolder.DiagRoomType.Laser)
                    {
                        spriteSetter.SetBoxFrontSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[6]);
                        spriteSetter.SetBoxBackSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[7]);
                    }
                    else if (((DiagnosticRoomInfo)info.infos).TypeOfDiagRoom == HospitalDataHolder.DiagRoomType.LungTesting)
                    {
                        spriteSetter.SetBoxFrontSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[8]);
                        spriteSetter.SetBoxBackSprite(HospitalAreasMapController.HospitalMap.DiagnosisBuildDummiesSprite[9]);
                    }
                }
            }
        }
        private void SetCollider()
        {
            var tempo = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>().prefabData;

            UpdateDiagnosisSprite();

            var tempos = GetComponent<CapsuleCollider>();
            tempos.radius = (tempo.tilesX < tempo.tilesY ? tempo.tilesX / 2.5f : tempo.tilesY / 2.5f);
            tempos.enabled = true;
            tempos.center = new Vector3((tempo.tilesX - 1) / 2.0f, tempos.radius * 0.80f, (tempo.tilesY - 1) / 2.0f);
            tempos.direction = tempo.tilesX > tempo.tilesY ? 0 : 2;
            tempos.height = tempo.tilesX > tempo.tilesY ? tempo.tilesX : tempo.tilesY;
        }

        public bool CanAnchor()
        {
            return map.CanAddObject(position.x, position.y, map.GetObjID(info, actualRotation, state), info.infos.dummyType);
        }

        public virtual void MoveTo(int x, int y)
        {
            if (Anchored)
                throw new IsoException("Fatal failure - moving anchored object!");

            // if (x != position.x && y != position.y)
            // {
            RemoveFromMap();
            position = new Vector2i(x, y);
            transform.position = new Vector3(x, 0, y);
            ClearDisbledRotation();

            if (map.CanAddObject(position.x, position.y, map.GetObjID(info, actualRotation, state), info.infos.dummyType))
            {
                AddToMap();
            }
            else
                RotateRight();

            //if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.MoveRotateRoomStartChanging)
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs());

            UpdateCollectablesPosition();
            //  }
            //NotificationCenter.Instance.ObjectMoved.Invoke(new ObjectEventArgs(this));
            //NotificationCenter.Instance.ExpandConditions.Invoke(new ExpandConditionsEventArgs());
        }

        public virtual void MoveTo(int x, int y, Rotation beforeRotation)
        {
            if (Anchored)
                throw new IsoException("Fatal failure - moving anchored object!");

            RemoveFromMap();
            position = new Vector2i(x, y);
            transform.position = new Vector3(x, 0, y);
            //info = beforeInfo;

            SetRotation(beforeRotation);

            //if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.MoveRotateRoomStartChanging)
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs());

            UpdateCollectablesPosition();
            //NotificationCenter.Instance.ObjectMoved.Invoke(new ObjectEventArgs(this));
            //NotificationCenter.Instance.ExpandConditions.Invoke(new ExpandConditionsEventArgs());
        }

        public void MoveToTemporaryObject(int x, int y)
        {
            if (Anchored)
                throw new IsoException("Fatal failure - moving anchored object!");

            RemoveFromMap();
            position = new Vector2i(x, y);
            transform.position = new Vector3(x, 0, y);

            AddToMapTemporary();

            //if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.MoveRotateRoomStartChanging)
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs());

            UpdateCollectablesPosition();
        }

        public Rotations GetRotations()
        {
            return info;
        }

        public State GetState()
        {
            return state;
        }

        public void MoveBy(int x, int y)
        {

            MoveTo(position.x + x, position.y + y);

        }

        public GameObject GetActualGameObject()
        {
            if (temporaryObj != null)
            {
                return temporaryObj;
            }
            return AreaMapController.Map.GetObject(position);
        }

        private void MakeDummy()
        {
            isoObj = null;
            if (obj != null)
                GameObject.Destroy(obj);
            if (temporaryObj != null)
                GameObject.Destroy(temporaryObj);
            obj = GameObject.Instantiate(eng.objects[map.GetObjID(info, actualRotation, state)]);

            obj.SetActive(true);
            obj.transform.SetParent(gameObject.transform);
            obj.transform.position = transform.position;


            Vector3 offset = new Vector3(-Mathf.Sqrt(2), 2f, -Mathf.Sqrt(2));   //so the object is in front of other objects
            obj.transform.GetChild(0).transform.localPosition += offset;
            SpriteRenderer[] renderers = obj.GetComponentsInChildren<SpriteRenderer>();


            if (this is MedicineProductionMachine)
            {
                MastershipProductionMachineAppearance appearanceController = obj.GetComponent<MastershipProductionMachineAppearance>();
                if (appearanceController != null)
                {
                    appearanceController.TurnGaugeRed();
                }
                MastershipProductionMachineAppearance controller = obj.GetComponent<MastershipProductionMachineAppearance>();
                if (controller != null && masterableProperties != null)
                {
                    controller.SetAppearance(masterableProperties.MasteryLevel, false);
                }
            }

            if (this is VitaminMaker)
            {
                MastershipProductionMachineAppearance appearanceController = obj.GetComponent<MastershipProductionMachineAppearance>();
                if (appearanceController != null)
                {
                    appearanceController.TurnGaugeRed();
                }
                MastershipProductionMachineAppearance controller = obj.GetComponent<MastershipProductionMachineAppearance>();
                if (controller != null && masterableProperties != null)
                {
                    controller.SetAppearance(masterableProperties.MasteryLevel, false);
                }
            }

            //Debug.LogError("OBJECT: " + Tag + " IS SET WRONGLY");
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].color = new Color(249f / 255f, 47f / 255f, 51f / 255f, 1f);

            if (border != null)
            {
                border.SetBorderColor(2, actualRotation);
                border.transform.position = new Vector3(border.transform.position.x, 0, border.transform.position.z);
            }
            if (mutalBorder != null)
                mutalBorder.SetBorderColor(2, actualRotation);
        }


        public virtual void RotateRight()
        {
            RemoveFromMap();
            IsoObjectPrefabData tempObject = actualData;
            if (temporaryObj != null)
            {
                GameObject.Destroy(temporaryObj);
                temporaryObj = null;
            }
            for (int i = 0; i < 4; i++)
            {
                var nextRotation = (Rotation)(((int)actualRotation + 1 + i) % 4);

                if (disabledRotation[(int)nextRotation] == 0)
                {
                    var id = map.GetObjID(info, nextRotation, state);
                    var tempObj = eng.objects[id];
                    tempObject = tempObj.GetComponent<IsoObjectPrefabController>().prefabData;
                    var shift = (actualData.rotationPoint - tempObject.rotationPoint);
                    var nextPosition = new Vector2i((int)(position.x + shift.x), (int)(position.y + shift.y));

                    if (map.CanAddObject(nextPosition.x, nextPosition.y, id, info.infos.dummyType))
                    {
                        //Debug.Log("Object can add is true. Actual rotation: " + actualRotation.ToString() + ". nextRotation: " + nextRotation.ToString());

                        actualData = tempObject;
                        actualRotation = nextRotation;
                        UIController.get.drawer.SetLastRotation(actualRotation);
                        HalfSize = new Vector2i((actualData.tilesX - 1) / 2, (actualData.tilesY - 1) / 2);
                        position = nextPosition;
                        transform.position = new Vector3(position.x, 0, position.y);
                        break;
                    }
                    else disabledRotation[(int)nextRotation] = 1;
                }
            }

            if (border != null)
            {
                border.SetBorderSize(actualData.tilesX, actualData.tilesY);
            }

            UpdateMutal();
            AddToMap();
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs(null, this));
        }

        public bool RotateToAnyAvailableNextRotation()
        {
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs());
            RemoveFromMap();
            IsoObjectPrefabData tempo = actualData;
            if (temporaryObj != null)
            {
                GameObject.Destroy(temporaryObj);
                temporaryObj = null;
            }

            disabledRotation[(int)actualRotation] = 1;

            var current_rot = actualRotation;

            for (int i = 0; i < 4; i++)
            {
                current_rot++;
                var nextRotation = (Rotation)(((int)current_rot) % 4);

                if (disabledRotation[(int)nextRotation] == 0)
                {
                    var id = map.GetObjID(info, nextRotation, state);
                    var tempObj = eng.objects[id];
                    tempo = tempObj.GetComponent<IsoObjectPrefabController>().prefabData;
                    var shift = (actualData.rotationPoint - tempo.rotationPoint);
                    var nextPosition = new Vector2i((int)(position.x + shift.x), (int)(position.y + shift.y));

                    if (map.CanAddObject(nextPosition.x, nextPosition.y, id, info.infos.dummyType))
                    {
                        actualData = tempo;
                        actualRotation = nextRotation;
                        UIController.get.drawer.SetLastRotation(actualRotation);
                        HalfSize = new Vector2i((actualData.tilesX - 1) / 2, (actualData.tilesY - 1) / 2);
                        position = nextPosition;
                        transform.position = new Vector3(position.x, 0, position.y);

                        if (border != null)
                        {
                            border.SetBorderSize(actualData.tilesX, actualData.tilesY);
                        }

                        UpdateMutal();
                        AddToMap();
                        disabledRotation[(int)nextRotation] = 1;
                        return true;
                    }
                    else disabledRotation[(int)nextRotation] = 1;
                }
            }

            if (border != null)
            {
                border.SetBorderSize(actualData.tilesX, actualData.tilesY);
            }

            return false;
        }

        public virtual void SetRotation(Rotation beforRotation)
        {
            //if (TutorialController.Instance.GetCurrentStepData().NotificationType == NotificationType.MoveRotateRoomStartChanging)
            NotificationCenter.Instance.MoveRotateRoomStartChanging.Invoke(new MoveRotateRoomStartChangingEventArgs());

            RemoveFromMap();
            IsoObjectPrefabData tempo = actualData;
            if (temporaryObj != null)
            {
                GameObject.Destroy(temporaryObj);
                temporaryObj = null;
            }

            var id = map.GetObjID(info, beforRotation, state);
            var tempObj = eng.objects[id];
            tempo = tempObj.GetComponent<IsoObjectPrefabController>().prefabData;

            actualData = tempo;
            actualRotation = beforRotation;
            UIController.get.drawer.SetLastRotation(actualRotation);
            HalfSize = new Vector2i((actualData.tilesX - 1) / 2, (actualData.tilesY - 1) / 2);
            transform.position = new Vector3(position.x, 0, position.y);

            if (border != null)
            {
                border.SetBorderSize(actualData.tilesX, actualData.tilesY);
            }
            UpdateMutal();
            AddToMap();
        }

        public Borders GetBorder()
        {
            return border;
        }

        private Vector2i GetDoorPosition(IsoObjectPrefabData tempo)
        {
            foreach (var p in tempo.spotsData)
            {
                switch ((SpotTypes)p.id)
                {
                    case SpotTypes.Door:
                        return new Vector2i(p.x, p.y);
                }
            }

            return new Vector2i(0, 0);
        }

        protected virtual void AddToMap()
        {
            if (state != oldState)
            {
                Anchored = false;
                RemoveFromMap();
                Anchored = true;

                if (temporaryObj != null)
                    GameObject.Destroy(temporaryObj);
                temporaryObj = null;
                if (obj != null)
                    obj.SetActive(false);
                GameObject.Destroy(obj);
                obj = null;
            }
            IsDummy = false;

            if (temporaryObj == null) // WASN'T OBJECT ON MAP BEFORE
            {
                if ((isoObj = map.AddObject(position.x, position.y, map.GetObjID(info, actualRotation, state), null, false, info.infos)) == null) // CHECK IS OBJECT CAN BE ADD ON MAP
                {
                    if (!RotateToAnyAvailableNextRotation()) // IF ANY OTHER ROTATION ISN'T VALID
                    {
                        MakeDummy();
                        IsDummy = true;
                    }
                }
                else
                {
                    HideDummy();
                    ClearDisbledRotation();
                }
            }
            else if ((isoObj = map.AddObjectFromExisting(position, map.GetObjID(info, actualRotation, state), temporaryObj)) == null)  // WAS OBJECT ON MAP BEFORE
            {
                if (!RotateToAnyAvailableNextRotation()) // IF ANY OTHER ROTATION ISN'T VALID
                {
                    MakeDummy();
                    IsDummy = true;
                }
            }
            else
            {
                HideDummy();
                ClearDisbledRotation();
            }

            AddedToMap = true;
            SetCollider();
            UpdateCollectablesPosition();
            if (isoObj != null)
            {
                onClickVisualEffects = isoObj.GetGameObject().GetComponent<OnClickVisualEffects>();
            }
        }

        protected virtual void AddToMapTemporary()
        {
            HospitalArea tmpArea = AreaMapController.Map.GetAreaTypeFromPositionIfNotOnEdge(position);

            if (tmpArea == HospitalArea.Ignore || tmpArea == HospitalArea.Patio)
                IsDummy = true;
            else IsDummy = false;

            isoObj = null;

            if (IsDummy)
                MakeDummy();
            else HideDummy();
        }

        private void HideDummy()
        {
            if (border != null)
            {
                border.SetBorderColor(1, actualRotation);
                border.transform.position = new Vector3(border.transform.position.x, 0.01f, border.transform.position.z);
            }
            if (mutalBorder != null)
                mutalBorder.SetBorderColor(3, actualRotation);

            IsDummy = false;
        }

        protected virtual void RemoveFromMap()
        {
            if (!AddedToMap)
                return;

            oldState = state;

            if (Anchored)
                throw new IsoException("Fatal failure - Removing unanchored object!");
            if (obj == null)
            {
                temporaryObj = map.RemoveObjectAndGetGameObject(position.x, position.y);

                if (temporaryObj != null)
                    temporaryObj.SetActive(false);
            }
            else
            {
                if (obj != null)
                    obj.SetActive(false);
                GameObject.Destroy(obj);
                obj = null;
            }
            AddedToMap = false;
        }

        public virtual void ToStored()
        {
            //Debug.Log("ToSafe");
            AddToObjectStored();
            ShopRoomInfo infos;
            infos = (ShopRoomInfo)info.infos;
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Drawer, transform.position, 1, 0, 1.75f, new Vector3(2, 2, 1), new Vector3(1, 1, 1), infos.ShopImage, null, null);
            //RemoveFromMap ();
            //IsoDestroy();

            //AnalyticsController.instance.ReportBuilding(AnalyticsBuildingAction.Stored, Tag, area, this is Decoration, false, -1);
        }

        #endregion


        #region Generation

        public void AddController(System.Type controllerType, bool shouldDisappear = true)
        {
            gameObject.AddComponent(controllerType);

            var p = gameObject.GetComponent<RotatableSimpleController>();
            p.SetShouldDisappear(shouldDisappear);
        }

        public static RotatableObject CreateRotatableObject(string tag, Vector2i position, Rotation rotation = Rotation.East, State state = State.working, string settings = "")
        {
            var rotations = AreaMapController.Map.GetPrefabInfo(tag);
            var tempGameObject = new GameObject("Rotatable Object");
            tempGameObject.transform.position = new Vector3(position.x, 0, position.y);
            tempGameObject.AddComponent<CapsuleCollider>();
            tempGameObject.AddComponent<EventTrigger>();
            var roomController = rotations.infos.roomController;
            var roomControllerType = roomController.GetType();
            tempGameObject.AddComponent(roomControllerType);
            var rotatableComponent = tempGameObject.GetComponent<RotatableObject>();
            rotatableComponent.Initialize(rotations, position, rotation, state, false);
            if (!string.IsNullOrEmpty(settings))
                rotatableComponent.LoadFromString(";" + settings, new NullableTimePassedObject(0, 0));
            AreaMapController.Map.AddRotatableObject(rotatableComponent);
            return rotatableComponent;
        }

        public static RotatableObject GenerateRotatableObject(int x, int y, Rotations info, Rotation defaultRotation = Rotation.North)
        {
            var temp = new GameObject("Rotatable Object");
            temp.transform.position = new Vector3(x, 0, y);
            temp.AddComponent<CapsuleCollider>();
            temp.AddComponent<EventTrigger>();
            var z = info.infos.roomController;
            var p = z.GetType();
            temp.AddComponent(p);
            var g = temp.GetComponent<RotatableObject>();
            g.Initialize(info, new Vector2i(x, y), defaultRotation);
            return g;
        }

        public static RotatableObject GenerateSingleFromSave(string save, TimePassedObject timePassed)
        {
            var temp = new GameObject("Rotatable Object");
            temp.transform.position = Vector3.zero;
            temp.AddComponent<CapsuleCollider>();
            temp.AddComponent<EventTrigger>();
            var tag = save.Split('$');
            var info = AreaMapController.Map.GetPrefabInfo(tag[0]);
            if (info != null)
            {
                var z = info.infos.roomController;
                var p = z.GetType();
                temp.AddComponent(p);
                var g = temp.GetComponent<RotatableObject>();

                g.InitializeFromSave(tag[1], info, timePassed);
                return g;
            }
            else
            {
                if (AreaMapController.Map == null || AreaMapController.Map.drawerDatabase == null)
                    throw new IsoException("Fatal failure - drawerDatabase is null !");

                var z = AreaMapController.Map.drawerDatabase.AreaBlocker.GetComponent<RotatableObject>();
                temp.AddComponent<Decoration>();
                var g = temp.GetComponent<RotatableObject>();
                g.Tag = tag[0];

                if (!VisitingController.Instance.IsVisiting)
                {
                    Debug.LogError("can't load object with name: " + g + " so i put it in stored object list");
                    g.AddToObjectStored();
                }

                return g;
            }
        }

        #endregion

        #region Save

        public string SaveObject()
        {
            return Tag + "$" + SaveToString();
        }

        #endregion

        public virtual void StartBuilding()
        {
            if (state != State.fresh)
                return;
            buildTimeLeft = buildTime = ((ShopRoomInfo)info.infos).buildTimeSeconds;
            var val = Anchored;
            SetAnchored(false);
            RemoveFromMap();
            state = State.building;
            AddToMap();
            SoundsController.Instance.PlayConstruction();
            //Debug.Log(this.GetType());

            if (this.Tag == "SyrupLab")
                NotificationCenter.Instance.SyrupLabAdded.Invoke(new SyrupLabAddedEventArgs(this, this.Tag));
            else if (this.Tag == "YellowDoc")
                NotificationCenter.Instance.YellowDoctorOfficeAdded.Invoke(new YellowDoctorOfficeAddedEventArgs(this, this.Tag));
            else if (this.Tag == "BlueDoc")
                NotificationCenter.Instance.BlueDoctorOfficeAdded.Invoke(new BlueDoctorOfficeAddedEventArgs(this, this.Tag));
            else if (this.Tag == "GreenDoc")
                NotificationCenter.Instance.GreenDoctorOfficeAdded.Invoke(new GreenDoctorOfficeAddedEventArgs(this, this.Tag));
            else if (this.Tag == "ElixirLab")
                NotificationCenter.Instance.ElixirMixerAdded.Invoke(new ElixirMixerAddedEventArgs(this, this.Tag));
            else if (this.Tag == "Xray")
                NotificationCenter.Instance.XRayAdded.Invoke(new XRayAddedEventArgs(this, this.Tag));
            else if (this.Tag == "WaitingRoomBlueOrchid")
                NotificationCenter.Instance.WaitingRoomBlueOrchidAdded.Invoke(new BlueDoctorOfficeAddedEventArgs(this, this.Tag));
            else if (this.Tag == "LabourRoomBlueOrchid")
                NotificationCenter.Instance.LaborRoomBlueOrchidAdded.Invoke(new BlueDoctorOfficeAddedEventArgs(this, this.Tag));
            else if (this.Tag == "PillLab")
                NotificationCenter.Instance.PillMakerAdded.Invoke(new BaseNotificationEventArgs());
            //NotificationCenter.Instance.ExpandConditions.Invoke(new ExpandConditionsEventArgs());
            SetAnchored(val);


            AreaMapController.Map.ChangeOnTouchType(x =>
            {
                AreaMapController.Map.ResetOntouchAction();
                BuildingHover.activeHover.Close();
                progressBar = null;
            });

            buildingHover = BuildingHover.Open(buildTime, this, I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)info.infos).ShopTitle));
            progressBar = buildingHover.GetProgressBar();

            buildingHover.SetWorldPointHovering(new Vector3(GetHoverPosition().x + HalfSize.x, 0, GetHoverPosition().y + HalfSize.y));
            buildingHover.UpdateAccordingToMode();
            MoveCameraToShowHover();
        }

        public virtual void StopDragging()
        {
            IsoDestroy();
        }

        public virtual bool isTemporaryObject()
        {
            return false;
        }

        private static void GetMapReference()
        {
            if (map != null)
                return;
            eng = ReferenceHolder.Get().engine;
            map = AreaMapController.Map;
        }

        public Type GetTypeOfInfos()
        {
            return info.infos.GetType();
        }

        public ShopRoomInfo GetRoomInfo()
        {
            return (ShopRoomInfo)info.infos;
        }


        private void ClearDisbledRotation()
        {
            for (int i = 0; i < 4; i++)
            {
                disabledRotation[i] = 0;
            }
        }

        private bool IsDisabledAllRotation()
        {
            int dis = 0;

            for (int i = 0; i < 4; i++)
            {
                if (disabledRotation[i] == 1)
                    dis++;
            }

            if (dis == 4)
                return true;
            else return false;
        }

        #region Collectables
        protected void CreateCollectable(Sprite image, MedicineRef medicine, int amount, bool isPositiveEnergy, string patientID = null)
        {
            if (collectables == null)
                collectables = new List<Collectable>();

            if (AreaMapController.Map.VisitingMode)
            {
                return;
            }

            Collectable collectable = ReferenceHolder.Get().giftSystem.CreateCollectable(image, medicine, amount, isPositiveEnergy, patientID);
            collectables.Add(collectable);

            if (!isPositiveEnergy)
            {
                if (medicine != null)
                {
                    MedicineBadgeHintsController.Get().AddSingleMedInProduction(medicine, 1);
                }
            }

            UpdateCollectablesPosition();
        }

        private string GetElixirNameForDoctor(string tagg)
        {
            switch (tagg)
            {
                case "BlueDoc":
                    return "MEDICINE/BLUE_ELIXIR";
                case "GreenDoc":
                    return "MEDICINE/GREEN_MIXTURE";
                case "PinkDoc":
                    return "MEDICINE/PINK_MIXTURE";
                case "PurpleDoc":
                    return "MEDICINE/PURPLE_MIXTURE";
                case "RedDoc":
                    return "MEDICINE/RED_ELIXIR";
                case "SkyDoc":
                    return "MEDICINE/SKY_BLUE_MIXTURE";
                case "SunnyDoc":
                    return "MEDICINE/SUNNY_YELLOW_MIXTURE";
                case "WhiteDoc":
                    return "MEDICINE/WHITE_ELIXIR";
                case "YellowDoc":
                    return "MEDICINE/YELLOW_ELIXIR";
                default:
                    return "XXX";
            }
        }


        public int storageFullCounter = 0;
        protected virtual void CollectCollectable(bool isDoctor = false, bool isKid = false, bool overflowStorage = false)
        {
            if (collectables == null)
                collectables = new List<Collectable>();

            Collectable first = collectables.First();

            if (isDoctor && !first.isPositiveEnergy)
            {
                int coinsCollected = ((DoctorRoom)this).CoinRewardMastered;
                int currentAmount = Game.Instance.gameState().GetCoinAmount();
                Game.Instance.gameState().AddResource(ResourceType.Coin, coinsCollected, EconomySource.DoctorPatientCured, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Coin, first.transform.position + new Vector3(-.1f, .75f, 0), coinsCollected, 0.1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[0], null, () =>
                {
                    Game.Instance.gameState().UpdateCounter(ResourceType.Coin, coinsCollected, currentAmount);
                }, false);

                int expReward = ((DoctorRoom)this).ExpRewardMastered;
                int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.DoctorPatientCured, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, first.transform.position + new Vector3(-.1f, .75f, 0), expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[2], null, () =>
                {
                    Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                }, false);

                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                {
                    ReferenceHolder.Get().giftSystem.SpawnPickUpParticle(first.transform.position);
                }

                // HintsController.Get().RemoveHint(new CollectHint(first, Tag), 1, true);

                collectables.Remove(first);

                NotificationCenter.Instance.DoctorRewardCollected.Invoke(new DoctorRewardCollectedEventArgs());

                //Achievements
                AchievementNotificationCenter.Instance.CoinsMadeByCuringClinicPatients.Invoke(new AchievementProgressEventArgs(coinsCollected));
                AchievementNotificationCenter.Instance.PatientInClinicCured.Invoke(new AchievementProgressEventArgs(1));

                // Daily Quests
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CurePatientsForGivenDoctor, Tag));

                // Global Event
                GlobalEventNotificationCenter.Instance.CurePatientGlobalEvent.Invoke(new GlobalEventCurePatientProgressEventArgs(Tag));

                Destroy(first.gameObject);
                UpdateCollectablesPosition();

                Game.Instance.gameState().UpdateExtraGift(first.transform.position, isDoctor);
            }
            else
            {
                MedicineRef medToAdd = first.medicine;
                if (first.isPositiveEnergy)
                {
                    int positiveEnergyReward = first.amount;
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.PositiveEnergy, first.transform.position + new Vector3(0, .75f, 0), positiveEnergyReward, 0, 2.75f, Vector3.one, new Vector3(1, 1, 1), first.icon.sprite, null, null, false);
                    Game.Instance.gameState().AddResource(ResourceType.PositiveEnergy, positiveEnergyReward, EconomySource.KidCured, true);

                    NotificationCenter.Instance.CollectableCollected.Invoke(new CollectableCollectedEventArgs());

                    //Achievement
                    AchievementNotificationCenter.Instance.KidCured.Invoke(new AchievementProgressEventArgs(1));

                    // Daily Quests
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.CureChildern));

                    if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled())
                    {
                        ReferenceHolder.Get().giftSystem.SpawnPickUpParticle(first.transform.position);
                    }

                    collectables.Remove(first);

                    Destroy(first.gameObject);
                    UpdateCollectablesPosition();

                    Game.Instance.gameState().UpdateExtraGift(first.transform.position, isDoctor);
                }
                else if (Game.Instance.gameState().CanAddResource(medToAdd, first.amount, overflowStorage) > 0)
                {
                    bool isTank = medToAdd.IsMedicineForTankElixir();

                    UIController.get.storageCounter.Add(first.amount, isTank);

                    Game.Instance.gameState().AddResource(medToAdd, first.amount, overflowStorage, EconomySource.ProductionMachine);

                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, first.transform.position + new Vector3(0, .75f, 0), first.amount, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), first.icon.sprite, null, () =>
                    {
                        UIController.get.storageCounter.Remove(first.amount, isTank);
                    });

                    int expCollected = ResourcesHolder.Get().GetEXPForCure(first.medicine);
                    if (expCollected > 0)
                    {
                        int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                        Game.Instance.gameState().AddResource(ResourceType.Exp, expCollected, EconomySource.MedicineProduced, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, first.transform.position + new Vector3(-.1f, .75f, 0), expCollected, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(first.medicine), null, () =>
                        {
                            Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expCollected, currentExpAmount);
                        });
                    }

                    Game.Instance.gameState().UpdateExtraGift(first.transform.position, isDoctor, isTank ? SpecialItemTarget.Tank : SpecialItemTarget.Storage);

                    NotificationCenter.Instance.MedicineExistInStorage.Invoke(new MedicineExistInStorageEventArgs(first.medicine, Game.Instance.gameState().GetCureCount(first.medicine)));
                    NotificationCenter.Instance.CollectableCollected.Invoke(new CollectableCollectedEventArgs());
                    if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.syrup_collect_text && first.medicine.type == MedicineType.Syrop)
                        NotificationCenter.Instance.BlueSyrupCollected.Invoke(new SyrupCollectedEventArgs());

                    AchievementNotificationCenter.Instance.CureProduced.Invoke(new AchievementProgressEventArgs(1));

                    // HintsController.Get().RemoveHint(new CollectHint(first.medicine));

                    MedicineBadgeHintsController.Get().RemoveMedInProduction(medToAdd);
                    HandlePickupParticle(first.transform.position, false);

                    collectables.Remove(first);
                    Destroy(first.gameObject);
                    UpdateCollectablesPosition();

                    storageFullCounter = 0;
                }
                else
                {
                    MedicineRef med = first.medicine;

                    storageFullCounter++;

                    if (med != null)
                    {
                        bool isTankElixir = med.IsMedicineForTankElixir();
                        MessageController.instance.ShowMessage(isTankElixir ? 47 : 9);
                        UIController.getHospital.StorageFullPopUp.preserveHover = true;
                        StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(isTankElixir));
                    }
                    else
                    {
                        if (storageFullCounter <= 1)
                            MessageController.instance.ShowMessage(9);
                        
                        StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(false));
                    }

                    return;
                }
            }
        }

        protected void HandlePickupParticle(Vector3 pos, bool isProbeTable)
        {
            FireOnMedicineCollected();
            if (HospitalBedController.isNewCureAvailable)
            {
                //spawn green particle with sound
                SoundsController.Instance.PlayAlert();

                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled() || HospitalBedController.isNewCureAvailable)
                {
                    ReferenceHolder.Get().giftSystem.SpawnPickUpParticle(pos, HospitalBedController.isNewCureAvailable);
                }

                Debug.Log("DING DING DING DING. This cure enabled healing of one of the patients");
            }
            else if (!isProbeTable)
            {
                //spawn normal particle without sound for machines
                if (DefaultConfigurationProvider.GetConfigCData().IsParticlesInGameEnabled() || HospitalBedController.isNewCureAvailable)
                {
                    ReferenceHolder.Get().giftSystem.SpawnPickUpParticle(pos, HospitalBedController.isNewCureAvailable);
                }
            }

            HospitalBedController.isNewCureAvailable = false;

            //Debug.LogError("isNewCureAvailable = false");
        }

        protected void UpdateCollectablesPosition()
        {
            if (collectables == null || collectables.Count < 1)
            {
                return;
            }

            if (info.infos.CollectablesOffsets != null && info.infos.CollectablesOffsets.Length == 4)
                collectablesPos.GetComponent<RectTransform>().anchoredPosition3D = info.infos.CollectablesOffsets[(int)actualRotation];
            else
                collectablesPos.GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;

            var baseScale = new Vector3(.01f, .01f, .01f);  //this is the scale of CollectabePrefab
            for (int i = 0; i < collectables.Count; i++)
            {
                collectables[i].transform.SetParent(collectablesPos.positions[(int)Mathf.Clamp(i, 0, collectablesPos.positions.Length - 1)]);
                collectables[i].GetComponent<RectTransform>().anchoredPosition3D = Vector3.zero;
                collectables[i].transform.localRotation = Quaternion.identity;
                collectables[i].transform.localScale = baseScale;   //so it's affected by parents scale
                collectables[i].HideGlow();
                collectables[i].StopBumping();
            }
            collectables[0].ShowGlow();
            try
            {
                if (synchroCoroutine != null)
                {
                    StopCoroutine(synchroCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            synchroCoroutine = StartCoroutine(SynchroCollectablesBumping());
        }

        IEnumerator SynchroCollectablesBumping()
        {
            yield return new WaitForSeconds(.15f);
            for (int i = 0; i < collectables.Count; i++)
            {
                if (collectables[i] != null)
                    collectables[i].StartBumping();

                yield return new WaitForSeconds(.15f);
            }
            synchroCoroutine = null;
        }

        public Vector3 GetCurrentCollectablePositions(bool isMachine = false)
        {
            if (info.infos.CollectablesOffsets != null && (int)actualRotation < info.infos.CollectablesOffsets.Length)
            {
                return new Vector3(info.infos.CollectablesOffsets[(int)actualRotation].x - 1.4f, info.infos.CollectablesOffsets[(int)actualRotation].y + 0.75f, info.infos.CollectablesOffsets[(int)actualRotation].z - 2.4f);
            }
            else
            {
                if (!isMachine)
                {
                    return new Vector3(0.5f, 1.75f, -0.3f);
                }
                else return Vector3.zero;
            }
        }
        #endregion
    }

    #region DataAndEnums

    [System.Serializable]
    public struct DummyRotations
    {
        public GameObject North;
        public GameObject East;
        public GameObject South;
        public GameObject West;
    }

    [System.Serializable]
    public struct RotatationsHolder
    {
        public GameObject North;
        public GameObject East;
        public GameObject South;
        public GameObject West;
        public int controllerID;
        public BuildDummyType buildDummy;
        public int buildTime;
        public int cost;
        public Sprite shopImage;
        public string shopTitle;
        public string shopDescription;
        public HospitalArea area;
        public int tabNumber;
    }

    public enum BuildDummyType //matward add building types (Translated from Polish)
    {
        DoctorRoom,
        DiagnosticRoom,
        ProductionDevice,
        Hospital3xRoom,
        HospitalVipRoom,
        Hospital2xRoom,
        NoDummy,
        Decoration,
        Can,
        MaternityWaitingRoom,
        BloodDiagnosticRoom,
        MaternityLabourRoom
    }

    [System.Serializable]
    public struct DummyRotation
    {
        private int[] rot;

        public DummyRotation(int north, int east, int south, int west)
        {
            rot = new int[4] { north, east, south, west };

        }
        public DummyRotation(int[] source)
        {
            rot = source;



        }
        public int this[int index]
        {
            get
            {
                return rot[index];
            }
        }
        public int this[Rotation rotation]
        {
            get
            {
                return rot[(int)rotation];
            }
        }
    }

    [System.Serializable]
    public class Rotations
    {
        [SerializeField]
        private int[] rot;
        public readonly BaseRoomInfo infos;
        public Rotations(int north, int east, int south, int west, BaseRoomInfo Infos)
        {
            rot = new int[4] { north, east, south, west };
            infos = Infos;
        }
        public Rotations(int[] source, ShopRoomInfo Infos)
        {
            rot = source;
            infos = Infos;

        }
        public int this[int index]
        {
            get
            {
                return rot[index];
            }
        }
        public int this[Rotation rotation]
        {
            get
            {
                return rot[(int)rotation];
            }
        }
    }



    #endregion
}


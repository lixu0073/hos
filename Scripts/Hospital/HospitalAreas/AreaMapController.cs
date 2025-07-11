using IsoEngine;
using MovementEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hospital
{
    public abstract class AreaMapController : PFMapController
    {
        private const char PARSING_SPLIT = '$';

        public delegate void OnPress(Vector2i tile);
        public delegate void OnSinglePress();
        protected OnPress delegation;
        protected OnPress onTouch;

        public MapConfiguration mapConfig;
        public WallPrefabsDatabase wallDatabase;
        public List<LongWallItemPrefab> doorPrefabs;
        public wallTop WallTop;
        [SerializeField] protected DrawerDatabase _drawerDatabase;

        public DrawerDatabase drawerDatabase { get { return _drawerDatabase; } }
#pragma warning disable 0649
        [SerializeField] CharacterNames _characterNames;
#pragma warning restore 0649
        public CharacterNames characterNames { get { return _characterNames; } }

        public List<GameObject> treePrefabs;

        public List<DummyRotations> BuildDummies;
        public List<DummyRotations> BuildEndedDummies;

        public List<Rotations> rotationsIDs;
        protected List<Rotations> additionalRotationsIDs;

        public Dictionary<String, Rotations> dRotatoinsIDs;
        public Dictionary<String, Rotations> dAdditionalRotationsIDs;

        public Playground playgroud;

        public TreasureManager treasureManager;

        public BoosterManager boosterManager;

        public List<GameObject> FakeWallPrefab;

        public CasesManager casesManager;

        protected List<GameObject> FakeWall;

        public List<Doors> allDoorsOnMap;

        protected List<Decoration> allDecorationsOnMap = new List<Decoration>();

        public Dictionary<string, int> decoAmountMap = new Dictionary<string, int>();

        protected IsoTileType tileType = IsoTileType.Sprites;
        protected Rectangle wanderingArea;
        protected List<DummyRotation> dummyTypesIDs;
        protected List<DummyRotation> endedDummysIDs;
        internal Dictionary<HospitalArea, GameArea> areas;
        protected int areaBlockerId;

        public Dictionary<HospitalArea, GameArea> GetAreas()
        {
            return areas;
        }

        public int CountBoughtAreas(HospitalArea area)
        {
            foreach (KeyValuePair<HospitalArea, GameArea> pair in areas)
            {
                if (pair.Key == area)
                    return pair.Value.CountBoughtAreas();
            }
            return 0;
        }

        protected PathType[] pathCheckNames;// = { PathType.GoEmergencyPath, PathType.GoPatioPath, PathType.GoHomePath };//PathType.GoWanderingPath, PathType.GoHomeOrGoDoctorRoomPath};

        [HideInInspector]
        public int buildBlockerId;
        public int pathBlockerId;

        /// <summary>
        /// These are used to block path calculation on certain objects, like scanner or x-ray
        /// </summary>
        public List<int> NoPathCalculationObjectIds;

        protected bool reconstructed;
        public bool Reconstructed { get { return reconstructed; } }
        protected bool _shouldReconstruct;

        protected bool visitingMode;
        public bool VisitingMode { get { return visitingMode; } }
        [SerializeField]
        protected bool ReloadGamestate = true;
        public RotatableObject removableDeco;

        public static bool HomeMapLoaded = false;

        protected GameObject expandBorders;

        public IEnumerator<float> updateDoorInteractionCorountine;

        protected TimePassedObject timePassed;

        private static AreaMapController _map;

        public static AreaMapController Map
        {
            get
            {
                if (_map == null)
                {
                    Debug.Log("Scene should contain exactly one HospitalAreasMapController if you want size use this feature.\n_map is null");
                }
                return _map;
            }
        }

        virtual protected void Awake()
        {
            if (_map != null && _map != this)
                Debug.LogWarning("Multiple instances of MaternityMapController were found!");
            else
                _map = this;
        }

        internal override void Initialize()
        {
            base.Initialize();
            rotationsIDs = new List<Rotations>();
            dRotatoinsIDs = new Dictionary<string, Rotations>();
            dummyTypesIDs = new List<DummyRotation>();
            endedDummysIDs = new List<DummyRotation>();
            additionalRotationsIDs = new List<Rotations>();
            dAdditionalRotationsIDs = new Dictionary<string, Rotations>();
            InitializePathTypes();
            foreach (var p in BuildDummies)
            {
                dummyTypesIDs.Add(new DummyRotation(engineController.AddObjectToEngine(p.North), engineController.AddObjectToEngine(p.East), engineController.AddObjectToEngine(p.South), engineController.AddObjectToEngine(p.West)));
            }
            foreach (var p in BuildEndedDummies)
            {
                endedDummysIDs.Add(new DummyRotation(engineController.AddObjectToEngine(p.North), engineController.AddObjectToEngine(p.East), engineController.AddObjectToEngine(p.South), engineController.AddObjectToEngine(p.West)));
            }
            foreach (var p in drawerDatabase.DrawerItems)
            {
                if (p != null)
                {
                    var z = new Rotations(engineController.AddObjectToEngine(p.NorthPrefab), engineController.AddObjectToEngine(p.EastPrefab), engineController.AddObjectToEngine(p.SouthPrefab), engineController.AddObjectToEngine(p.WestPrefab), p);
                    if (p.roomController is Decoration)
                    {
                        MapPrefrefabData(p.EastPrefab.GetComponent<IsoObjectPrefabController>().prefabData);
                        MapPrefrefabData(p.WestPrefab.GetComponent<IsoObjectPrefabController>().prefabData);
                        MapPrefrefabData(p.NorthPrefab.GetComponent<IsoObjectPrefabController>().prefabData);
                        MapPrefrefabData(p.SouthPrefab.GetComponent<IsoObjectPrefabController>().prefabData);
                    }
                    rotationsIDs.Add(z);
                    try
                    {
                        dRotatoinsIDs.Add(z.infos.Tag, z);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.Message);
                    };
                }
            }

            areaBlockerId = engineController.AddObjectToEngine(drawerDatabase.AreaBlocker);
            buildBlockerId = engineController.AddObjectToEngine(drawerDatabase.BuildBlocker);
            pathBlockerId = engineController.AddObjectToEngine(drawerDatabase.PathBlocker);

            foreach (var obj in drawerDatabase.AdditionalObjects)
            {
                var rotation = new Rotations(engineController.AddObjectToEngine(obj.NorthPrefab), engineController.AddObjectToEngine(obj.EastPrefab), engineController.AddObjectToEngine(obj.SouthPrefab), engineController.AddObjectToEngine(obj.WestPrefab), obj);
                additionalRotationsIDs.Add(rotation);
                try
                {
                    dAdditionalRotationsIDs.Add(rotation.infos.Tag, rotation);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                };
            }
            reconstructed = false;
            if (UIController.get != null && UIController.get.drawer != null)
                UIController.get.drawer.Initialize(rotationsIDs);

            RotatableObject.InitSharedBorders();
        }

        public virtual void SaveGame(Save save)
        {
            Game.Instance.gameState().SetSaveFilePrepared(false);
        }

        public virtual void SaveGameInVisigingMode(Save save) { }

        protected abstract void InitializePathTypes();

        private List<Vector2i> results;

        #region various IsoEngine
        public Vector2i GetWaitingSpot(Vector2i currentPos, bool get_with_patio = true)
        {
            if (areas == null || (areas != null && areas.Count == 0))
            {
                Debug.LogError("Can't set Waiting spot for " + currentPos + ". Areas are null");
                return Vector2i.zero;
            }

            RectWallInfo randomArea = null;

            if (get_with_patio)
            {
                if (BaseGameState.RandomNumber(0, 10) > 5)
                    randomArea = areas[HospitalArea.Clinic].AvailableAreas.RandomOther(BaseGameState.RandomNumber(0, 100) / 100);
                else
                    randomArea = areas[HospitalArea.Patio].AvailableAreas.RandomOther(BaseGameState.RandomNumber(0, 100) / 100);
            }
            else
                randomArea = areas[HospitalArea.Clinic].AvailableAreas.RandomOther(BaseGameState.RandomNumber(0, 100) / 100);

            if (results == null)
                results = new List<Vector2i>();

            results.Clear();

            Vector2i vector = new Vector2i(0, 0);

            for (int x = randomArea.rect.x; x < randomArea.rect.xSize + randomArea.rect.x; ++x)
            {
                for (int y = randomArea.rect.y; y < randomArea.rect.y + randomArea.rect.ySize; ++y)
                {
                    vector.x = x;
                    vector.y = y;

                    if (!isAnyObjectExistOnPosition(x, y) && !ReferenceHolder.Get().engine.GetMap<PFMapController>().isAnyPersonOnTile(vector))
                        results.Add(vector);
                }
            }

            if (results.Count == 0)
            {
                return Vector2i.zero;
            }
            int index = BaseGameState.RandomNumber(0, results.Count);

            if (results[index].x < 31)
                Debug.Log("Want to go to: " + results[index].x + " , " + results[index].y);

            return results[index];
        }

        protected IEnumerator<float> UpdateDoorInteractionCorountine()
        {
            while (true)
            {
                for (int i = 0; i < allDoorsOnMap.Count; i++)
                {
                    bool is_Error = false;
                    try
                    {
                        allDoorsOnMap[i].UpdateWithNearPatient();
                    }
                    catch (System.Exception)
                    {
                        is_Error = true;
                    }

                    if (is_Error) continue;
                }

                yield return Timing.WaitForSeconds(0.5f);
            }
        }

        #region press and purchase
        protected int lastTouchFrame = 0;

        public void ChangeOnTouchType(OnPress function)
        {
            lastTouchFrame = Time.frameCount;
            onTouch?.Invoke(new Vector2i(0, 0));
            onTouch = function;
        }

        public void ResetOntouchAction()
        {
            //Debug.LogError("ResetOntouchAction " + Time.frameCount);
            onTouch = null;
        }

        public void ChangeOnPressType(OnPress function)
        {
            delegation?.Invoke(new Vector2i(0, 0));
            delegation = function;
            //var p = Levels[0].GetComponent<BoxCollider>().size;
            //Levels[0].GetComponent<BoxCollider>().size = new Vector3(p.x, 10, p.z);
        }

        public void ResetOnPressAction()
        {
            delegation = null;
            if (Levels != null && Levels.Length > 0)
            {
                var p = Levels[0].GetComponent<BoxCollider>();
                if (p != null)
                    Levels[0].GetComponent<BoxCollider>().size = new Vector3(p.size.x, 0.01f, p.size.z);
                else
                    throw new IsoException("There is no BoxCollider on scene!");
            }
            else
                throw new IsoException("There is no Level on scene!");
        }

        public void ResetAllInputActions()
        {
            ResetOnPressAction();
            ResetOntouchAction();
        }

        public bool isPressActionActive()
        {
            if (delegation != null)
                return true;
            else
                return false;
        }

        IMapArea temp2;
        HospitalArea currentArea;

        public void OnTileTouch(Vector2i touchedTile)
        {
            if (lastTouchFrame == Time.frameCount)
            {
                Debug.Log("Here hover would be closed in the same frame as it was opened. Returning instead.");
                return;
            }

            onTouch?.Invoke(touchedTile);
            onTouch = null;
        }

        public virtual void OnTilePress(Vector2i pressedTile)
        {
            if (delegation != null)
            {
                delegation.Invoke(pressedTile);
                return;
            }
            if (onTouch != null)
            {
                onTouch.Invoke(pressedTile);
                onTouch = null;
                return;
            }
            if (temp2 != null)
                HideTransformBorder();

            temp2 = null;
            currentArea = HospitalArea.Patio;
            if (VisitingMode)
                return;

            foreach (var p in areas)
            {
                temp2 = p.Value.CheckAreaOfPoint(pressedTile);
                if (temp2 != null)
                {
                    if (RequiredLevelTooLow())
                    {
                        ShowMessage();
                        ShowExpandBorder(temp2.GetRectPos(), temp2.GetRectSize());
                    }
                    else
                    {
                        currentArea = p.Key;

                        if (temp2.CanBuy())
                        {
                            UIController.get.ExpandPopUp.Open(temp2);
                            SoundsController.Instance.PlayButtonClick2();
                            ShowExpandBorder(temp2.GetRectPos(), temp2.GetRectSize());
                        }
                        else
                        {
                            MessageController.instance.ShowMessage(5);
                            ShowExpandBorder(temp2.GetRectPos(), temp2.GetRectSize());
                        }
                    }
                    break;
                }
            }
        }

        protected virtual void ShowMessage()
        {
            MessageController.instance.ShowMessage(4);
        }

        protected virtual bool RequiredLevelTooLow()
        {
            return Game.Instance.gameState().GetHospitalLevel() < 6;
        }

        public bool ConfirmPurchase(Action onBought = null, Action onResigned = null)
        {
            if (temp2 != null && temp2.CanBuy())
            {
                bool isLab = temp2.GetExpansionType() == ExpansionType.Lab;
                int expansionCost = DiamondCostCalculator.GetExpansionCost(temp2.GetExpansionType());
                if (Game.Instance.gameState().GetCoinAmount() >= expansionCost)
                {
                    OnCanBuy(onBought, isLab, expansionCost);
                    return true;
                }

                UIController.get.ExpandPopUp.Exit();
                MessageController.instance.ShowMessage(0);
                UIController.get.BuyResourcesPopUp.Open(expansionCost - Game.Instance.gameState().GetCoinAmount(), () =>
                {
                    OnCanBuy(onBought,
                             temp2.GetExpansionType() == ExpansionType.Lab,
                             DiamondCostCalculator.GetExpansionCost(temp2.GetExpansionType()));
                }, () =>
                {
                    if (onResigned != null)
                        onResigned();

                    UIController.get.ExpandPopUp.Reopen();
                });
                return false;
            }

            onResigned?.Invoke();
            return false;
        }

        private void OnCanBuy(Action onBought, bool isLab, int expansionCost)
        {
            foreach (var p in temp2.GetRectangles())
                AddRectToArea(currentArea, p);
            areas[currentArea].Buy(temp2);

            if (isLab)
            {
                Game.Instance.gameState().RemoveCoins(expansionCost, EconomySource.ExpandLab);
                ObjectiveNotificationCenter.Instance.ExpandAreaObjectiveUpdate.Invoke(new ObjectiveExpandAreaEventArgs(1, HospitalArea.Laboratory));
            }
            else
            {
                if (TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.expand_arrow || TutorialSystem.TutorialController.CurrentStep.StepTag == StepTag.expand_text)
                {
                    TutorialSystem.TutorialLogger.Log("Expand arrow step. Set expand to free");
                }
                else
                {
                    Game.Instance.gameState().RemoveCoins(expansionCost, this is MaternityAreasMapController ? EconomySource.ExpandMaternityClinic : EconomySource.ExpandHospital);
                }
                ObjectiveNotificationCenter.Instance.ExpandAreaObjectiveUpdate.Invoke(new ObjectiveExpandAreaEventArgs(1, HospitalArea.Hospital));
            }

            UIController.get.ExpandPopUp.Exit();
            Debug.Log("ConfirmPurchase() enough coins");
            NotificationCenter.Instance.HospitalRoomsExpanded.Invoke(new HospitalRoomsExpandedEventArgs());
            Debug.Log("ConfirmPurchase() sending notif");

            var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, temp2.GetRectPos() + new Vector3(-2, 0, 2), Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one * 1.4f;
            fp.SetActive(true);
            SoundsController.Instance.PlayMagicPoof();

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.ExpandHospital);
            Vector3 pos = temp2.GetRectPos() + new Vector3(-2, 0, 2);
            ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, expansionCost, .1f, ReferenceHolder.Get().giftSystem.particleSprites[0]);

            if (isLab)
                GameState.Get().ExpansionsLab++;
            else
                Game.Instance.gameState().SetExpansionClinicAmount(1);

            int expReward = Mathf.Clamp(Mathf.FloorToInt(expansionCost / 100f), 2, 999);
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            AssignRewardToPlayerForExpansion(expReward, isLab);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(SimpleUI.GiftType.Exp, pos, expReward, .7f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });
            AchievementNotificationCenter.Instance.HospitalExpanded.Invoke(new AchievementProgressEventArgs(1));
            onBought?.Invoke();
        }
        #endregion

        protected abstract void AssignRewardToPlayerForExpansion(int expansionCost, bool IsForLab);

        void ShowExpandBorder(Vector3 pos, Vector3 scale)
        {
            if (expandBorders == null)
            {
                expandBorders = Instantiate(ResourcesHolder.Get().bordersPrefab);
            }
            expandBorders.SetActive(true);
            expandBorders.transform.localScale = new Vector3(scale.x, 1, scale.z);
            expandBorders.transform.position = pos + new Vector3(-.5f - scale.x, 0f, -.5f);

            Timing.KillCoroutine(HideSelectionBorderAfterTime().GetType());
            Timing.RunCoroutine(HideSelectionBorderAfterTime());
        }

        public void HideTransformBorder()
        {
            if (expandBorders != null)
                expandBorders.SetActive(false);
        }

        IEnumerator<float> HideSelectionBorderAfterTime()
        {
            yield return Timing.WaitForSeconds(1.5f);
            HideTransformBorder();
        }

        protected override bool CheckIsoLevelControllerType(Type isoLevelControllerType)
        {
            if (!base.CheckIsoLevelControllerType(isoLevelControllerType))
                return false;

            return typeof(HospitalAreasLevelController).IsAssignableFrom(isoLevelControllerType);
        }

        protected override Type GetIsoLevelControllerType()
        {
            return typeof(HospitalAreasLevelController);
        }
        #endregion

        #region ObjectManipulation
        public Rotations GetPrefabInfo(string tag)
        {
            try
            {
                return dRotatoinsIDs[tag];
            }
            catch (Exception) { }
            try
            {
                return dAdditionalRotationsIDs[tag];
            }
            catch (Exception) { };
            return null;
        }

        public Rotations GetPrefabInfoWithName(string name)
        {
            foreach (var p in rotationsIDs)
            {
                if ((p.infos.EastPrefab != null && p.infos.EastPrefab.name == name) || (p.infos.WestPrefab != null && p.infos.WestPrefab.name == name) || (p.infos.NorthPrefab != null && p.infos.NorthPrefab.name == name) || (p.infos.SouthPrefab && p.infos.SouthPrefab.name == name))
                    return p;
            }
            foreach (var p in additionalRotationsIDs)
            {
                if ((p.infos.EastPrefab != null && p.infos.EastPrefab.name == name) || (p.infos.WestPrefab != null && p.infos.WestPrefab.name == name) || (p.infos.NorthPrefab != null && p.infos.NorthPrefab.name == name) || (p.infos.SouthPrefab && p.infos.SouthPrefab.name == name))
                    return p;
            }
            return null;
        }

        public int GetObjID(Rotations info, Rotation rot, RotatableObject.State state)
        {
            switch (state)
            {
                case RotatableObject.State.fresh:
                    return info[rot];
                case RotatableObject.State.building:
                    return dummyTypesIDs[(int)info.infos.dummyType][rot];
                case RotatableObject.State.waitingForUser:
                    return endedDummysIDs[(int)info.infos.dummyType][rot];
                case RotatableObject.State.working:
                    return info[rot];
                default:
                    return info[rot];
            }
        }

        public RotatableObject FindRotatableObject(string tag, bool isProbeTabForHintSystem = false)
        {
            if (areas != null && areas.Count > 0)
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    var t = areas[areas.Keys.ElementAt(i)].FindRotatableObject(tag, isProbeTabForHintSystem);
                    if (t != null)
                        return t;
                }
            }
            return null;
        }

        public bool FindRotatableObjectExist(string tag, bool isProbeTabForHintSystem = false)
        {
            if (areas != null && areas.Count > 0)
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    var t = areas[areas.Keys.ElementAt(i)].FindRotatableObject(tag, isProbeTabForHintSystem);
                    if (t != null)
                        return true;
                }
            }
            return false;
        }

        public int GetRotatableObjectCounter(string tag)
        {
            int id = 0;

            if (areas != null && areas.Count > 0)
            {
                for (int i = 0; i < areas.Count; i++)
                    id += areas[areas.Keys.ElementAt(i)].GetRotatableObjectCounter(tag);
            }
            return id;
        }

        public Vector3 GetRotatableObjectPos(string tag)
        {
            foreach (var p in areas)
            {
                var t = p.Value.FindRotatableObject(tag);
                if (t != null)
                    return new Vector3(t.position.x, 0, t.position.y);
            }
            Debug.LogWarning("Object not found. Tag = " + tag);
            return Vector3.zero;
        }

        public Vector2i GetRotatableObjectPos2i(string tag)
        {
            foreach (var p in areas)
            {
                var t = p.Value.FindRotatableObject(tag);
                if (t != null)
                    return t.position;
            }
            //Debug.LogWarning("Object not found. Tag = " + tag);
            return Vector2i.zero;
        }

        public RotatableObject GetRotatableObject(Vector2i pos)
        {
            foreach (var p in areas)
            {
                var t = p.Value.GetRotatableObject(pos);
                if (t != null)
                    return t;
            }
            //Debug.LogWarning("Object not found in pos " + pos.x + " " + pos.y);
            return null;
        }

        public int GetAllRotatableObjectSizeFromArea(HospitalArea area)
        {
            return areas[area].GetAllRotatableObjectsSize();
        }

        public int GetAreaSize(HospitalArea area)
        {
            return areas[area].GetAreaSize();
        }

        public int GetAreaToBuyCounter(HospitalArea area)
        {
            if (areas[area].AreasToBuy != null)
                return areas[area].AreasToBuy.Count;

            return 0;
        }

        public RotatableObject isRotatableObject(Vector2i pos)
        {
            foreach (var p in areas)
            {
                var t = p.Value.GetRotatableObject(pos);
                if (t != null)
                    return t;
            }
            //Debug.LogWarning("Object not found in pos " + pos.x + " " + pos.y);
            return null;
        }

        public void SetBorders(HospitalArea area, bool state)
        {
            areas[area].SetBorders(state);
        }

        public void AddRotatableObject(RotatableObject go)
        {
            MapRotatableData(go);
            areas[go.area].AddRotatableObject(go);
        }

        public void RemoveRotatableObject(RotatableObject go)
        {
            areas[go.area].RemoveRotatableObject(go);
        }

        public abstract HospitalArea GetAreaTypeFromPosition(Vector2i pos);
        protected abstract void MapPrefrefabData(IsoObjectPrefabData prefabData);
        protected abstract void MapRotatableData(RotatableObject go);

        public HospitalArea GetAreaTypeFromPositionIfNotOnEdge(Vector2i pos)
        {
            if (areas != null && areas.Count > 0)
            {
                GameArea gameArea = null;

                if (areas.TryGetValue(HospitalArea.Clinic, out gameArea) && areas[HospitalArea.Clinic].ContainsPoint(pos))
                {
                    if (gameArea.ContainsPoint(pos))
                    {
                        if (gameArea.ContainsPoint(new Vector2i(pos.x + 1, pos.y)) && gameArea.ContainsPoint(new Vector2i(pos.x - 1, pos.y)) && gameArea.ContainsPoint(new Vector2i(pos.x, pos.y + 1)) && gameArea.ContainsPoint(new Vector2i(pos.x, pos.y - 1)))
                            return HospitalArea.Clinic;
                    }
                    return HospitalArea.Ignore;
                }

                if (areas.TryGetValue(HospitalArea.Laboratory, out gameArea) && areas[HospitalArea.Laboratory].ContainsPoint(pos))
                {
                    if (gameArea.ContainsPoint(pos))
                    {
                        if (gameArea.ContainsPoint(new Vector2i(pos.x + 1, pos.y)) && gameArea.ContainsPoint(new Vector2i(pos.x - 1, pos.y)) && gameArea.ContainsPoint(new Vector2i(pos.x, pos.y + 1)) && gameArea.ContainsPoint(new Vector2i(pos.x, pos.y - 1)))
                            return HospitalArea.Laboratory;
                    }
                    return HospitalArea.Ignore;
                }

                if (areas.TryGetValue(HospitalArea.MaternityWardClinic, out gameArea) && areas[HospitalArea.MaternityWardClinic].ContainsPoint(pos))
                {
                    if (gameArea.ContainsPoint(pos))
                    {
                        if (areas[HospitalArea.MaternityWardClinic].ContainsPoint(new Vector2i(pos.x + 1, pos.y)) && areas[HospitalArea.MaternityWardClinic].ContainsPoint(new Vector2i(pos.x - 1, pos.y)) && areas[HospitalArea.MaternityWardClinic].ContainsPoint(new Vector2i(pos.x, pos.y + 1)) && areas[HospitalArea.MaternityWardClinic].ContainsPoint(new Vector2i(pos.x, pos.y - 1)))
                            return HospitalArea.MaternityWardClinic;
                    }
                    return HospitalArea.Ignore;
                }
                return HospitalArea.Ignore;
            }
            return HospitalArea.Ignore;
        }

        public IsoObject AddObject(Vector2i position, int objectID, PathType[] pathTypes = null)
        {
            return AddObject(position.x, position.y, objectID, pathTypes);
        }

        public IsoObject AddObjectFromExisting(Vector2i position, int id, GameObject go, PathType[] pathTypes = null)
        {
            var p = new IsoObjectData();
            p.objectID = id;
            p.x = position.x;
            p.y = position.y;
            IsoObjectPrefabData tempo = engineController.objects[id].GetComponent<IsoObjectPrefabController>().prefabData;

            if (pathTypes != null)
                tempo.pathTypes = pathTypes;
            else
                tempo.pathTypes = new PathType[1] { PathType.Default };

            var currentObjectDoorPos = ((HospitalAreasLevelController)Levels[0]).GetDoorPosition(tempo);

            if (tempo.area == HospitalArea.Clinic || tempo.area == HospitalArea.Hospital || tempo.area == HospitalArea.MaternityWardClinic)
            {
                //  var curr_map = ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>();
                if (!CheckArea(p, tempo))
                    return null;

                MessageController.instance.DisableStackedMessage();

                // NEW FASTEST ALGORITHM
                var tmp = Levels[0].AddObjectFromExisting(p, go, tempo.pathTypes);
                bool hasNoMutal = false;

                if (tmp != null)
                {
                    hasNoMutal = !tmp.isObjectHaveAnySpot();

                    if (currentObjectDoorPos.x != -1 && currentObjectDoorPos.y != -1) // check paths for normal object
                    {
                        currentObjectDoorPos = currentObjectDoorPos + position;

                        var canMakePathsToExits = CanMakePathFromPositionToAllExits(currentObjectDoorPos, hasNoMutal);
                        var canMakePathsToOtherRooms = CanMakePathFromPositionToOtherDoors(currentObjectDoorPos);

                        if (canMakePathsToExits == false || canMakePathsToOtherRooms == false)
                        {
                            Levels[0].RemoveObject(tmp);
                            return null;
                        }
                    }
                    else // check paths for decorations object
                    {
                        Vector2i anyObjectDoorPos = GetDoorsPositionMapDependent();
                        var canMakePathsToExits = CanMakePathFromPositionToAllExits(anyObjectDoorPos, true);
                        var canMakePathsToOtherRooms = CanMakePathFromPositionToOtherDoors(anyObjectDoorPos);

                        //Debug.LogError("PP2: " + anyObjectDoorPos.x + " " + anyObjectDoorPos.y + " | " + canMakePathsToExits + " " + canMakePathsToOtherRooms);

                        if (canMakePathsToExits == false || canMakePathsToOtherRooms == false)
                        {
                            Levels[0].RemoveObject(tmp);
                            return null;
                        }
                    }
                    return tmp;
                }
                return null;
            }

            if (!CheckArea(p, tempo))
                return null;
            MessageController.instance.DisableStackedMessage();
            return Levels[0].AddObjectFromExisting(p, go, tempo.pathTypes);
        }

        protected virtual Vector2i GetDoorsPositionMapDependent()
        {
            return Levels[0].GetAnyDoorOfType(HospitalArea.Clinic);
        }

        public IsoObject AddObjectWithoutValidation(int x, int y, int objectID, PathType[] pathTypes = null, bool isDoorObject = false)
        {
            if (pathTypes == null)
                pathTypes = new PathType[] { PathType.Default };

            var temp = new IsoObjectData();
            temp.objectID = objectID;
            temp.x = x;
            temp.y = y;

            var tempo = engineController.objects[objectID].GetComponent<IsoObjectPrefabController>().prefabData;
            tempo.pathTypes = pathTypes;

            if (!CheckArea(temp, tempo))
                return null;

            return Levels[0].AddObjectWithoutValidation(temp, pathTypes);
        }

        public IsoObject AddObjectFromExistingWithoutValidation(Vector2i position, int id, GameObject go, PathType[] pathTypes = null)
        {
            var p = new IsoObjectData();
            p.objectID = id;
            p.x = position.x;
            p.y = position.y;
            var tempo = engineController.objects[id].GetComponent<IsoObjectPrefabController>().prefabData;
            MessageController.instance.DisableStackedMessage();

            if (!CheckArea(p, tempo))
                return null;

            return Levels[0].AddObjectFromExisting(p, go, tempo.pathTypes);
        }

        public IsoObject AddObject(int x, int y, int objectID, PathType[] pathTypes = null, bool isDoorObject = false, BaseRoomInfo info = null)
        {
            if (pathTypes == null)
                pathTypes = new PathType[] { PathType.Default };

            var temp = new IsoObjectData();
            temp.objectID = objectID;
            temp.x = x;
            temp.y = y;
            IsoObjectPrefabData tempo = engineController.objects[objectID].GetComponent<IsoObjectPrefabController>().prefabData;
            tempo.pathTypes = pathTypes;

            var currentObjectDoorPos = GetDoorPosition(tempo);

            if (isDoorObject)
                tempo.area = HospitalArea.Clinic;

            MessageController.instance.DisableStackedMessage();
            if (!CheckArea(temp, tempo, info))
                return null;

            if (tempo.area != HospitalArea.Ignore)//== HospitalArea.Clinic || tempo.area == HospitalArea.Hospital || tempo.area == HospitalArea.MaternityWardClinic)
            {
                // NEW FASTEST ALGORITHM
                var tmp = Levels[0].AddObject(temp, pathTypes);
                bool hasNoMutal = false;

                if (tmp != null)
                {
                    hasNoMutal = !tmp.isObjectHaveAnySpot();

                    if (currentObjectDoorPos.x != -1 && currentObjectDoorPos.y != -1) // check paths for normal object
                    {
                        currentObjectDoorPos = currentObjectDoorPos + new Vector2i(x, y);

                        var canMakePathsToExits = CanMakePathFromPositionToAllExits(currentObjectDoorPos, hasNoMutal);
                        var canMakePathsToOtherRooms = CanMakePathFromPositionToOtherDoors(currentObjectDoorPos);

                        if (canMakePathsToExits == false || canMakePathsToOtherRooms == false)
                        {
                            Levels[0].RemoveObject(tmp);
                            return null;
                        }
                    }
                    else // check paths for decorations object
                    {
                        var anyObjectDoorPos = GetDoorsPositionMapDependent();
                        var canMakePathsToExits = CanMakePathFromPositionToAllExits(anyObjectDoorPos, hasNoMutal);
                        var canMakePathsToOtherRooms = CanMakePathFromPositionToOtherDoors(anyObjectDoorPos);

                        // Debug.LogError("PP2: " + anyObjectDoorPos.x + " " + anyObjectDoorPos.y + " | " + canMakePathsToExits + " " + canMakePathsToOtherRooms);

                        if (anyObjectDoorPos != Vector2i.zero && (canMakePathsToExits == false || canMakePathsToOtherRooms == false))
                        {
                            Levels[0].RemoveObject(tmp);
                            return null;
                        }
                    }

                    return tmp;
                }
                return null;
            }

            return Levels[0].AddObject(temp, pathTypes);
        }

        public List<Vector2i> GetAllDoorObjectPositions()
        {
            var doors = new List<Vector2i>();

            if (allDoorsOnMap != null && allDoorsOnMap.Count > 0)
            {
                for (int i = 0; i < allDoorsOnMap.Count; i++)
                {
                    doors.Add(allDoorsOnMap[i].position);
                }
            }

            return doors;
        }

        public void AddDoorToMap(Doors currentDoor)
        {
            if (allDoorsOnMap == null)
                allDoorsOnMap = new List<Doors>();
            allDoorsOnMap.Add(currentDoor);
        }

        public void RemoveDoorFromMap(Doors currentDoor)
        {
            if ((allDoorsOnMap != null) && allDoorsOnMap.Contains(currentDoor))
                allDoorsOnMap.Remove(currentDoor);
        }

        public Doors GetNearDoorFromMapOfSameRoom(Doors currentDoor, float dist)
        {
            if ((allDoorsOnMap != null) && (allDoorsOnMap.Count > 0))
            {
                float odl = dist;

                for (int i = 0; i < allDoorsOnMap.Count; i++)
                {
                    if ((currentDoor.doorRoomID == allDoorsOnMap[i].doorRoomID) && (currentDoor != allDoorsOnMap[i]))
                    {
                        odl = Mathf.Sqrt((allDoorsOnMap[i].position.x - currentDoor.position.x) * (allDoorsOnMap[i].position.x - currentDoor.position.x) + (allDoorsOnMap[i].position.y - currentDoor.position.y) * (allDoorsOnMap[i].position.y - currentDoor.position.y));
                        if (odl < dist)
                            return allDoorsOnMap[i];
                    }
                }
            }
            return null;
        }

        public abstract bool AddDecorationToMap(Decoration currentDecoration);

        public abstract void RemoveDecorationFromMap(Decoration currentDecoration);

        public bool CanMakePathFromPositionToAllExits(Vector2i doorPos, bool hasNoMutal, bool sendMessage = true)
        {
            var makePathModeFromCurrent = CanMakePathTo(doorPos, pathCheckNames, hasNoMutal);
            if (makePathModeFromCurrent == -1)
            {
                if (sendMessage)
                    MessageController.instance.ShowMessageWithoutStacking(21);
                return false;
            }

            return makePathModeFromCurrent != 0;
        }



        protected abstract bool CanMakePathFromPositionToOtherDoors(Vector2i doorPos);

        public Vector3 CheckIsPositionInDoorNearAndGetCenterOfDoor(Vector2i nextPos, Vector3 currentPos, int searchRange = 1, float range = 2f)
        {
            if ((allDoorsOnMap == null) || (allDoorsOnMap != null && allDoorsOnMap.Count <= 0))
                return Vector3.zero;

            foreach (var door in allDoorsOnMap)
            {
                for (int i = -searchRange; i < searchRange; i++)
                {
                    for (int j = -searchRange; j < searchRange; j++)
                    {
                        if (door.position == new Vector2i(nextPos.x + i, nextPos.y + j))
                        {
                            if ((door.nextDoor != null) && (door.open || door.nextDoor.open))
                            {
                                float posX, posZ;

                                if (door.nextDoor.transform.position.x > door.transform.position.x)
                                    posX = door.transform.position.x + ((door.nextDoor.transform.position.x - door.transform.position.x) / 2f);
                                else
                                    posX = door.nextDoor.transform.position.x + ((door.transform.position.x - door.nextDoor.transform.position.x) / 2f);

                                if (door.nextDoor.transform.position.z > door.transform.position.z)
                                    posZ = door.transform.position.z + ((door.nextDoor.transform.position.z - door.transform.position.z) / 2f);
                                else
                                    posZ = door.nextDoor.transform.position.z + ((door.transform.position.z - door.nextDoor.transform.position.z) / 2f);

                                var centerOfDoors = new Vector3(posX, 0, posZ);
                                var odl = Mathf.Sqrt((centerOfDoors.x - currentPos.x) * (centerOfDoors.x - currentPos.x) + (centerOfDoors.z - currentPos.z) * (centerOfDoors.z - currentPos.z));
                                //Debug.LogWarning(posX + " , " + posZ + " , " + odl);

                                if (odl < range)
                                    return centerOfDoors;
                            }
                        }
                    }
                }
            }
            return Vector3.zero;
        }

        public List<int> CheckIsAnyDoorOnPathAndInWhichPostion(BasePatientAI patient, List<Vector2i> path)
        {
            var positionsOfNodesNearDoor = new List<int>();

            if (path != null && path.Count > 0 && allDoorsOnMap != null && allDoorsOnMap.Count > 0)
            {
                for (int id = 0; id < path.Count; id++)
                {
                    for (int i = 0; i < allDoorsOnMap.Count; i++)
                    {
                        if (id > 1 && id < path.Count - 1)
                        {
                            if (allDoorsOnMap[i].CheckPatient(patient, path[id], path[id - 1], path[id + 1]))
                                positionsOfNodesNearDoor.Add(id);
                        }
                    }
                }
            }
            return positionsOfNodesNearDoor;
        }

        public void EnableMutalForStaticDoor()
        {
            if (allDoorsOnMap != null && allDoorsOnMap.Count > 0)
            {
                for (int i = 0; i < allDoorsOnMap.Count; i++)
                {
                    if (allDoorsOnMap[i].mutalBorder != null)
                        allDoorsOnMap[i].mutalBorder.gameObject.SetActive(true);
                }
            }
        }

        public void DisableMutalForStaticDoor()
        {
            if (allDoorsOnMap != null && allDoorsOnMap.Count > 0)
            {
                for (int i = 0; i < allDoorsOnMap.Count; i++)
                {
                    if (allDoorsOnMap[i].mutalBorder != null)
                        allDoorsOnMap[i].mutalBorder.gameObject.SetActive(false);
                }
            }
        }

        public GameObject RemoveObjectAndGetGameObject(int x, int y)
        {
            return Levels[0].LightRemoveObject(Levels[0].GetObject(x, y));
        }

        public bool RemoveObject(Vector2i pos)
        {
            if (Levels[0].LightRemoveObject(Levels[0].GetObject(pos.x, pos.y)))
                return true;
            return false;
        }
        /// <summary>
        /// Check that the object can be placed on the game area.
        /// </summary>
        /// <param name="objData">Object data.</param>
        /// <param name="prefabData">Prefab data of the object.</param>
        /// <param name="info">BaseRoomInfo. Used to check if dummy is a decoration</param>
        /// <returns>True if object is placeable on the area</returns>
        protected virtual bool CheckArea(IsoObjectData objData, IsoObjectPrefabData prefabData, BaseRoomInfo info = null)
        {
            if (prefabData.area == HospitalArea.Ignore)
                return true;

            //MapPrefrefabData(prefabData);

            if (areas == null || !areas.ContainsKey(prefabData.area))
                return false;

            if (info != null && info.dummyType == BuildDummyType.Decoration)
            {
                bool result = false;
                foreach (var area in areas)
                    if (area.Value.CanContainObject(objData, prefabData))
                    {
                        result = true;
                        break;
                    }
                return result;
            }

            if (areas[prefabData.area].CanContainObject(objData, prefabData) != true)
            {
                if (prefabData.area == HospitalArea.Hospital || prefabData.area == HospitalArea.Clinic)
                {
                    if (areas[HospitalArea.Laboratory].CanContainObject(objData, prefabData) == true)  // for Cans From Clinic
                    {
                        var tmpPrefabRotation = GetPrefabInfoWithName(prefabData.name);

                        if ((tmpPrefabRotation != null) && ((tmpPrefabRotation.infos as CanInfo) != null))
                            return true;
                        MessageController.instance.ShowMessageWithoutStacking(23);
                    }
                    else
                    {
                        MessageController.instance.ShowMessageWithoutStacking(23);
                    }
                }
                else if (prefabData.area == HospitalArea.Laboratory)
                {
                    MessageController.instance.ShowMessageWithoutStacking(24);
                }
                else if (prefabData.area == HospitalArea.Patio)
                {
                    MessageController.instance.ShowMessageWithoutStacking(25);
                }

                return false;
            }

            return true;
        }
        /// <summary>
        /// Can the object be added to the game area.
        /// </summary>
        /// <param name="objectID">Id of the object</param>
        /// <param name="type">Type of the dummy</param>
        /// <returns>True if object can be added</returns>
        public bool CanAddObject(int x, int y, int objectID, BuildDummyType type = BuildDummyType.NoDummy)
        {
            var temp = new IsoObjectData();
            temp.objectID = objectID;
            temp.x = x;
            temp.y = y;
            IsoObjectPrefabData tempo = engineController.objects[objectID].GetComponent<IsoObjectPrefabController>().prefabData;

            //Check that the object is in placeable area. (Decorations can be outside)
            if (!CheckArea(temp, tempo) && type != BuildDummyType.Decoration)
                return false;

            //Check that the object can be added to the current level.
            bool canAdd = Levels[0].CanAddObject(x, y, tempo.tilesX, tempo.tilesY, tempo, false, false, objectID);
            MessageController.instance.DisableStackedMessage();

            return canAdd;
        }

        private Vector2i GetDoorPosition(IsoObjectPrefabData tempo)
        {
            if (tempo == null || tempo.spotsData != null && tempo.spotsData.Length <= 0)
                return new Vector2i(-1, -1); // Vector2i.zero;

            for (int i = 0; i < tempo.spotsData.Length; i++)
            {
                switch ((SpotTypes)tempo.spotsData[i].id)
                {
                    case SpotTypes.Door:
                        return new Vector2i(tempo.spotsData[i].x, tempo.spotsData[i].y);
                }
            }
            return new Vector2i(-1, -1);
        }

        public bool IsInteriorAtPositinoObject(int x, int y)
        {
            return Levels[0].GetData().tileData[x, y].isInterior;
        }
        #endregion

        #region manipulation
        public Rectangle GetWanderingArea()
        {
            return wanderingArea;
        }

        public void ClearAndAddArea(HospitalArea area)
        {
            if (areas.ContainsKey(area))
                areas.Remove(area);
            areas.Add(area, new GameArea(area, this));
        }

        public void AddStaticWallsArea(int x, int y, IsoWallData wall)
        {
            ((HospitalAreasLevelController)Levels[0]).AddStaticWallArea(x, y, wall);
        }

        public void AddRectToArea(HospitalArea area, RectWallInfo rect)
        {
            areas[area].AddRect(rect);
        }

        public void AddAreaToBuy(HospitalArea area, IMapArea areaToAdd)
        {
            areas[area].AddAreaToBuy(areaToAdd);
        }

        public void RemoveRect(HospitalArea area, RectWallInfo rect)
        {
            areas[area].RemoveRect(rect);
        }

        public void RemoveAreaToBuy(HospitalArea area, IMapArea areaToRemove)
        {
            areas[area].RemoveAreaToBuy(areaToRemove);
        }

        public GameObject GetObject(Vector2i pos)
        {
            var p = Levels[0].GetObject(pos.x, pos.y);
            return p != null ? p.GetGameObject() : null;
        }

        public bool isAnyObjectExistOnPosition(int x, int y)
        {
            var p = Levels[0].GetObject(x, y);
            return p != null ? true : false;
        }

        public int GetObjectID(Vector2i pos)
        {
            var p = Levels[0].GetObject(pos.x, pos.y);
            return p != null ? p.objectID : -1;
        }

        public int GetInstanceID(Vector2i pos)
        {
            var p = Levels[0].GetObject(pos.x, pos.y);
            return p != null ? p.GetGameObject().GetInstanceID() : -1;
        }

        public IsoObject GetIsoObject(Vector2i pos)
        {
            var p = Levels[0].GetObject(pos.x, pos.y);
            return p != null ? p : null;
        }
        /// <summary>
        /// Can the set paths be made?
        /// </summary>
        /// <param name="pos">position of the startplace</param>
        /// <param name="doorPathTypes">Paths to take</param>
        /// <param name="isDeco"></param>
        /// <returns>-1 if no paths or there is errors. 1 if all paths are found and accessible. 0 for decoration objects outside clinic(?).</returns>
        public abstract int CanMakePathTo(Vector2i pos, PathType[] doorPathTypes, bool isDeco = false);

        /// <summary>
        /// Can make a path from position to position. Inclusive.
        /// Calculations are made in Invoking thread.
        /// </summary>
        /// <param name="posFrom">Path start position</param>
        /// <param name="posTo">Path end position</param>
        /// <returns>-1 if errors or no path. 1 if there is path and it is ok.</returns>
        public int CanMakePathFromTo(Vector2i posFrom, Vector2i posTo)
        {
            try
            {
                var path = GetLevel<PFLevelController>(0).GetPath(posFrom, posTo, PathType.Default, false);
                return path != null ? 1 : -1;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Used to check if the position of the door is reachable from all exits.
        /// Set the area not passable. Check paths to all exits. Set the area passable back to previous state.
        /// </summary>
        /// <param name="doorPos">Position of the door</param>
        /// <param name="pos">Position of the room</param>
        /// <param name="size">Size of the room</param>
        /// <returns>True if all exits are reachable</returns>
        public abstract bool CheckExitAvailabilityFromNotPlacedRoom(Vector2i doorPos, Vector2i pos, Vector2i size);

        #endregion

        public void PFISODestroy()
        {
            base.IsoDestroy();
        }

        public override void IsoDestroy()
        {
            if (areas != null)
                foreach (var p in areas)
                    p.Value.IsoDestroy();

            ProbeTableTool.collectable.Clear();
            ProbeTableTool.fillable.Clear();

            if (updateDoorInteractionCorountine != null)
            {
                Timing.KillCoroutine(updateDoorInteractionCorountine);
                updateDoorInteractionCorountine = null;
            }

            areas = null;
            reconstructed = false;
            base.IsoDestroy();

            MedicineBadgeHintsController.Get().Reset();
            //HintsController.Get().Reset();
            TutorialUIController.Instance.ResetTutorialUI();

            allDoorsOnMap.Clear();
            allDecorationsOnMap.Clear();
            SaveCacheSingleton.UnlinkFromBaseGameStateEvent();
        }

        #region mariana

        public override void Load()
        {
            base.Load();
            OnDataLoaded();
        }

        public void Save()
        {
        }

        protected abstract void OnDataLoaded();

        #endregion

        public override void CreateMap(IsoMapData mapData)
        {
            _shouldReconstruct = true;
            base.CreateMap(mapData);
            if (!Reconstructed)
                ReconstructFromMapData();
        }

        public void DestroyMap()
        {
            HomeMapLoaded = false;
            //IsoDestroy();
            //TutorialUIController.Instance.HideIndicator();
        }

        public void DestroyPatients()
        {
            BasePatientAI.ResetAllPatients();
            HospitalPatientAI.ResetAllHospitalPatients();
            ClinicPatientAI.ResetAllClinicPatients();
            VIPPersonController.ResetAllVIPPatients();

            // find any patient which isn't on any list then delete him (patient removed from list during game and not destroyed)

            var tmp = GameObject.FindObjectsOfType<BasePatientAI>();
            if (tmp != null && tmp.Length > 0)
            {
                for (int i = 0; i < tmp.Length; i++)
                {
                    if (tmp[i] != null)
                        tmp[i].IsoDestroy();
                }
            }

            tmp = null;
        }

        public abstract void ReloadGame(Save save, bool visitingMode = false, bool reloadGameState = true);

        protected void CountDecorationsFromSecondMap(List<string> items)
        {
            if (items == null)
                return;

            foreach (string key in items)
            {
                string keyParsed = key.Substring(0, key.IndexOf(PARSING_SPLIT));
                if (decoAmountMap.ContainsKey(keyParsed))
                    decoAmountMap[keyParsed]++;
                else
                    decoAmountMap.Add(keyParsed, 1);
            }
        }

        public void StartGame()
        {
            const int levelId = 0;
            var mapData = new IsoMapData { tileType = tileType, levelData = new IsoLevelData[1] };
            var additionalData = new PFMapAdditionalData(new PFMapAdditionalData(null)) { elevatorEntry = new Vector2i(12, 12) };

            mapData.additionalData = additionalData;
            var size = mapConfig.MapSize;

            var levelData = mapData.levelData[levelId] = new IsoLevelData(size.x, size.y);
            levelData.additionalData = new PFLevelAdditionalData(null);


            for (var tileX = 0; tileX < levelData.tileData.GetLength(0); ++tileX)
                for (var tileY = 0; tileY < levelData.tileData.GetLength(1); ++tileY)
                {
                    var tile = levelData.tileData[tileX, tileY] = new IsoTileData(tileX, tileY);
                    tile.isInterior = true;
                }

            levelData.backgroundData = null;
            CreateMap(mapData);
            Load();

            if (removableDeco != null)
                removableDeco.IsoDestroy();

            removableDeco = FindObjectOfType<RemovableDecoration>();

            /*
            if (!VisitingMode)
            {
                HintsController.Get().UpdateAllHintsWithMedicineCount();
            }
            */
            //ReferenceHolder.Get().engine.MainCamera.ResetCamera();
        }

        #region SaveLoad
        protected virtual void ReconstructFromMapData()
        {
            DiamondTransactionController.Instance.Initialize();
        }

        protected void GetDecorationFromDataBase()
        {
            decoAmountMap.Clear();
            for (int i = 0; i < _drawerDatabase.DrawerItems.Count; i++)
            {
                if (_drawerDatabase.DrawerItems[i] is DecorationInfo)
                    decoAmountMap.Add(_drawerDatabase.DrawerItems[i].Tag, 0);
            }
        }

        protected IEnumerator DelayedUpdateMastership()
        {
            yield return null;
            foreach (RotatableObject rotatableWithMasterable in FindObjectsOfType<RotatableObject>().ToList().Where(x => x.masterableProperties != null))
            {
                rotatableWithMasterable.masterableProperties.RefreshMasteryView(false);
            }

            for (int i = 0; i < HospitalPatientAI.Patients.Count; ++i)
            {
                HospitalPatientAI.Patients[i].UpdateReward();
            }
        }

        protected abstract void AddFakeWall();

        public virtual void EmulateMapObjectTime(TimePassedObject timePassed)
        {
            reconstructed = true;
            _shouldReconstruct = false;

            ReferenceHolder.Get().saveLoadManager.StartSaving();
        }

        protected void ReconstructStaticWalls()
        {
            if (mapConfig.StaticAreas != null && mapConfig.StaticAreas.Count > 0)
                foreach (var p in mapConfig.StaticAreas)
                    foreach (var z in p.defaultAreas)
                    {
                        //if (z.doorPositions.Count != 0) {
                        //	AddStaticWallsArea (z.position.x, z.position.y, new IsoWallData (Vector2i.zero, z.size, p.wallType, p.outsideWallType, z.windowType, z.windowType, z.doorType, z.doorPositions, z.windowPositions));
                        //} else {
                        AddStaticWallsArea(z.position.x, z.position.y, new IsoWallData(Vector2i.zero, z.size, p.wallType, p.outsideWallType, z.windowType, z.windowType, z.doorType, new List<int>() { z.doorPosition }, z.windowPositions, z.doorPathTypes));
                        //}
                    }
            if (mapConfig.blockedArea != null && mapConfig.blockedArea.Count > 0)
                foreach (var p in mapConfig.blockedArea)
                {
                    for (int i = p.@from.x; i < p.@from.x + p.size.x; i++)
                        for (int j = p.@from.y; j < p.@from.y + p.size.y; j++)
                            BlockArea(i, j);
                }

            if (mapConfig.fakeWallDoorBlocker != null && mapConfig.fakeWallDoorBlocker.Count > 0)
                foreach (var p in mapConfig.fakeWallDoorBlocker)
                {
                    for (int i = p.@from.x; i < p.@from.x + p.size.x; i++)
                        for (int j = p.@from.y; j < p.@from.y + p.size.y; j++)
                            BlockPath(i, j);
                }
        }

        protected void ReconstructArea(GameArea area, Section source, List<int> boughtAreas)
        {
            foreach (var p in source.defaultAreas)
                area.AddRect(new RectWallInfo(p, source.wallType, source.outsideWallType));

            var lb = source.areas;

            for (int i = 0; i < lb.Count; ++i)
                if (boughtAreas.Contains(lb[i].areaID))
                    area.AddRect(new RectWallInfo(lb[i], source.wallType, source.outsideWallType));
                else
                    area.AddAreaToBuy(new SingleRectArea(new RectWallInfo(lb[i], source.wallType, source.outsideWallType), lb[i].areaID, lb[i].areaname, area, lb[i].expansionType));

            area.boughtAreas = boughtAreas;
        }

        protected void BlockArea(int x, int y)
        {
            AddObject(x, y, areaBlockerId);
        }

        public void BlockBuilding(int x, int y, PathType[] pathTypes = null)
        {
            var temp = new IsoObjectData();
            temp.objectID = buildBlockerId;
            temp.x = x;
            temp.y = y;
            //AddObject(x, y, doorBlockerId, type, true);
            GetLevel<PFLevelController>(0).AddObject(temp, pathTypes);
        }

        public void BlockPath(int x, int y, PathType[] pathTypes = null)
        {
            var temp = new IsoObjectData();
            temp.objectID = pathBlockerId;
            temp.x = x;
            temp.y = y;
            GetLevel<PFLevelController>(0).AddObject(temp, pathTypes);
        }

        public bool IsMapEmpty()
        {
            if (areas == null || areas.Count == 0)
                return true;

            for (int i = 0; i < areas.Count; ++i)
            {
                var area = areas.ElementAt(i);
                if (area.Value == null)
                    return true;
                if (area.Value.IsEmpty())
                    return true;
            }
            return false;
        }

        public virtual Decoration GetAvailableDecoration(out Vector2i pos, out Rotation rot, List<Decoration> decoToConsider = null)
        {
            if (allDecorationsOnMap == null || (allDecorationsOnMap != null && allDecorationsOnMap.Count <= 0))
            {
                pos = new Vector2i(0, 0);
                rot = Rotation.North;
                return null;
            }

            int randVal = BaseGameState.RandomNumber(0, 100);
            if (randVal > 20 && decoToConsider.Count > 0)
            {
                int checkDecoID = BaseGameState.RandomNumber(0, decoToConsider.Count);

                for (int i = checkDecoID; i < decoToConsider.Count; i++)
                {
                    var dec = decoToConsider[i];
                    pos = dec.GetDecorationSpot(out rot);

                    if (dec.isPatientUsing == false)
                    {
                        if (!ReferenceHolder.Get().engine.GetMap<PFMapController>().isAnyPersonOnTile(pos) && pos != Vector2i.zero)
                            return dec;
                        ++i;
                    }
                    else
                        ++i;
                }

                pos = new Vector2i(0, 0);
                rot = Rotation.North;
                return null;
            }

            pos = new Vector2i(0, 0);
            rot = Rotation.North;
            return null;
        }

        protected abstract void ReconstructObjects(Save save, TimePassedObject timePassed);
        /// <summary>
        /// Is position inside the Clinic area?
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public abstract bool IsPosInsideClinic(Vector2i pos);
        public abstract bool IsPosInsidePatio(Vector2i pos);

        public bool IsPosInsideHospitalClinicOrPatio(Vector2i pos)
        {
            return IsPosInsidePatio(pos) || IsPosInsideClinic(pos);
            /*if (IsPosInsidePatio(pos) || IsPosInsideClinic(pos))
                return true;
            return false;*/
        }
        #endregion
    }

    public enum Priorities
    {
        InnerStaticWalls,
        OuterWalls,
        InnerWals,
        OuterStaticWalls,
        SpecialStaticOuter,
        SpecialStaticInner,
    }
    [Serializable]
    public struct wallItemPrefab
    {
        public GameObject left;
        public GameObject right;
    }
    [Serializable]
    public struct wallItemPref
    {
        public Texture left;
        public Texture right;
    }
    [Serializable]
    public class LongWallItemPrefab
    {
        public List<wallItemPrefab> list;
    }
    [Serializable]
    public class LongWallItemPref
    {
        public List<wallItemPref> list;
    }

    public enum wallType
    {
        left,
        right,
        leftCorner,
        rightCorner,
        outerCorner,
        innerCorner,
        leftWindow,
        rightWindow,
    }
    [Serializable]
    public struct wallTop
    {
        public GameObject leftCorner;
        public GameObject rightCorner;
        public GameObject topCorner;
        public GameObject bottonCorner;
        public GameObject tripleNoSouth;
        public GameObject tripleNoNorth;
        public GameObject tripleNoWest;
        public GameObject tripleNoEast;
        public GameObject quadruple;
        public GameObject this[CornerType type]
        {
            get
            {
                switch (type)
                {
                    case CornerType.cornerLeft:
                        return leftCorner;
                    case CornerType.cornerRight:
                        return rightCorner;
                    case CornerType.cornerTop:
                        return topCorner;
                    case CornerType.cornerBotton:
                        return bottonCorner;
                    case CornerType.tripleNoNorth:
                        return tripleNoNorth;
                    case CornerType.tripleNoSouth:
                        return tripleNoSouth;
                    case CornerType.tripleNoWest:
                        return tripleNoWest;
                    case CornerType.tripleNoEast:
                        return tripleNoEast;
                    case CornerType.quadruple:
                        return quadruple;
                    default:
                        break;
                }
                return null;
            }
        }
    }
}
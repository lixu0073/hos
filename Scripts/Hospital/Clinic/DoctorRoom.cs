using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoEngine;
using SimpleUI;
using System;
using MovementEffects;


namespace Hospital
{
    public class DoctorRoom : RotatableObject, IHoverable, MasterablePropertiesClient
    {
        protected LinkedList<BasePatientAI> patients;
        protected DoctorController doctor;
        Transform floor = null;
        public bool isHealing = false;
        public bool shouldWork
        {
            get;
            private set;
        }
        internal List<IsoObjectPrefabData.SpotData> waitingSpots;
        int takenSpotsCount = 0;
        protected Vector2i entrance;
        protected Vector2i machine;
        protected Vector3 machinePosPatient;
        protected Vector3 machinePosKids;
        public bool[] waitingSpotsTaken;
        int cureAmount = 0;
        public int CureAmount { get { return cureAmount; } private set { } }

        public BasePatientAI currentPatient;
        public float curationTime = -1;
        [SerializeField]
        private int queueSize = 3;
        [SerializeField]
        private int maxQueue = 9;
        GameObject machineObject;
        DoctorHover hover = null;
        bool isPatientReady = false;
        ZzzDoc zzz;

        private Sprite hintElixirSprite;
        private bool showHint = false;

        private bool zzInvokeStarted = false;
        private bool zzInvokePlan = false;
        private bool isUnwraping = false;


        #region masteryProperties        

        private int GetMaxQueue()
        {
#if UNITY_IOS
            if (UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhone6)
            {
                if (HospitalAreasMapController.HospitalMap.VisitingMode)
                {
                    return maxQueue / 2;
                }
            }
#endif

            return maxQueue;
        }

        private BalanceableInt CoinRewardBalanceable;

        public int CoinRewardMastered
        {
            get
            {
                if (CoinRewardBalanceable == null)
                {
                    CoinRewardBalanceable = BalanceableFactory.CreateGoldForDoctorRoomsBalanceable(((DoctorRoomInfo)info.infos).cureCoinsReward, masterableProperties);
                }

                return CoinRewardBalanceable.GetBalancedValue();
            }
        }

        private BalanceableInt ExpRewardBalanceable;

        public int ExpRewardMastered
        {
            get
            {
                if (ExpRewardBalanceable == null)
                {
                    ExpRewardBalanceable = BalanceableFactory.CreateXPForDoctorRoomsBalanceable(((DoctorRoomInfo)info.infos).cureXpReward, masterableProperties);
                }

                return ExpRewardBalanceable.GetBalancedValue();
            }
        }

        private int positiveEnergyRewardMastered;

        private BalanceableInt PositiveEnergyRewardBalanceable;

        public int PositiveEnergyRewardMastered
        {
            get
            {
                if (PositiveEnergyRewardBalanceable == null)
                {
                    PositiveEnergyRewardBalanceable = BalanceableFactory.CreateDoctorRoomsPositiveEnergyFromKids(((DoctorRoomInfo)info.infos).CurePositiveEnergyReward, masterableProperties);
                }

                return PositiveEnergyRewardBalanceable.GetBalancedValue();
            }
        }

        private BalanceableInt CureTimeBalanceable;

        public int CureTimeMastered
        {
            get
            {
                if (CureTimeBalanceable == null)
                {
                    CureTimeBalanceable = BalanceableFactory.CreateDoctorRoomsCureTime(((DoctorRoomInfo)info.infos).cureTime, masterableProperties);
                }
                int result = CureTimeBalanceable.GetBalancedValue() - 1;

                return result < 0 ? 0 : result;
            }
        }
        #endregion
        IEnumerator<float> collectingCollectables;
        bool canCollect = true;
        public int QueueSize
        {
            get
            {
                return queueSize;
            }
            private set
            {
                if (value < queueSize)
                    throw new IsoException("you cannot make queue smaller!");
                if (hover != null)
                {
                    if (value <= GetMaxQueue())
                    {
                        hover.EnlargeQueue(value);
                    }
                    if (value == GetMaxQueue())
                    {
                        hover.SetBuyingEnable(false);
                    }
                }
                queueSize = value;
            }
        }

        public int CuredPatients
        {
            get;
            private set;
        }

        public bool infoShowed = false;

        TutorialController tc;



        private void SetFloor(bool val)
        {
            if (floor == null)
                throw new IsoException("doctor floor doesn't exist!!");
            floor.gameObject.SetActive(val);
            floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));
        }

        protected override Vector2 GetHoverPosition()
        {
            float xChange = 0;
            float yChange = 0;
            switch (actualRotation)
            {
                case Rotation.East:
                    yChange = 2f;
                    xChange = 1f;
                    break;
                case Rotation.North:
                    yChange = 1f;
                    break;
                case Rotation.South:
                    xChange = 2f;
                    yChange = 1f;
                    break;
                case Rotation.West:
                    xChange = 1f;
                    break;
            }
            return new Vector2(position.x + xChange, position.y + yChange);
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);


            foreach (var p in BasePatientAI.patients)
            {
                if (p != null)
                {
                    if (!(p is HospitalPatientAI) || (p is HospitalPatientAI && (!(p as HospitalPatientAI).IsInBedState())))
                        p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
                }
            }

            if (doctor != null)
            {
                doctor.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
            }

            HospitalAreasMapController.HospitalMap.DisableMutalForStaticDoor();

            shouldWork = obj == null && Anchored;

        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
            {
                EmulateTime((TimePassedObject)parameters);
            }
        }

        public override void IsoDestroy()
        {
            if (doctor != null)
                doctor.IsoDestroy();
            if (patients != null)
                foreach (var p in patients)
                    p.IsoDestroy();
            if (currentPatient != null)
                currentPatient.IsoDestroy();

            if (zzz != null)
                Destroy(zzz);

            DestroyMaterials();
            base.IsoDestroy();
        }

        private void DestroyMaterials()
        {
            DestroyFloorMaterial();
        }

        private void DestroyFloorMaterial()
        {
            EngineController.DestroyMaterial(floor.gameObject);
        }

        protected override void OnStateChange(State newState, State oldState)
        {
            base.OnStateChange(newState, oldState);
            if (floor != null)
            {
                DestroyFloorMaterial();
                GameObject.Destroy(floor.gameObject);
            }

            if (newState == State.building)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var mat = p.GetComponent<Renderer>();
                mat.material = HospitalAreasMapController.HospitalMap.wallDatabase.materialPrefab;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                mat.GetPropertyBlock(block);
                block.SetTexture("_MainTex", ResourcesHolder.GetHospital().UnderConstructionTile);
                mat.SetPropertyBlock(block);
                mat.material.mainTextureScale = Vector2.one * 4;
                floor = p.transform;
                floor.localScale = Vector3.one * 4;
                floor.rotation = Quaternion.Euler(90, 0, 0);
            }

            if (newState == State.working || newState == State.fresh || newState == State.waitingForUser)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var mat = p.GetComponent<Renderer>();
                mat.material = HospitalAreasMapController.HospitalMap.wallDatabase.materialPrefab;
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                mat.GetPropertyBlock(block);
                block.SetTexture("_MainTex", ResourcesHolder.GetHospital().DoctorFloor);
                mat.SetPropertyBlock(block);
                floor = p.transform;
                floor.localScale = Vector3.one * 4;
                floor.rotation = Quaternion.Euler(90, 0, 0);
            }

            floor.transform.SetParent(transform);
        }


        protected override void AddToMap()
        {
            base.AddToMap();
            ReinitializeSpots();
            if (isoObj != null)
            {
                var p = isoObj.GetGameObject();
                machineObject = p.transform.GetChild(p.transform.childCount - 1).gameObject;
                if (p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.childCount > 1)
                {
                    machinePosPatient = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(0).gameObject.transform.position;
                    machinePosKids = p.transform.GetChild(p.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
                }
            }
            else if (obj != null)
            {
                machineObject = obj.transform.GetChild(obj.transform.childCount - 1).gameObject;
                if (obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.childCount > 1)
                {
                    machinePosPatient = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(0).gameObject.transform.position;
                    machinePosKids = obj.transform.GetChild(obj.transform.childCount - 1).gameObject.transform.GetChild(1).gameObject.transform.position;
                }
            }

            if (state == State.working && patients != null && patients.Count > 0)
            {
                for (int i = 0; i < (patients.Count < waitingSpots.Count ? patients.Count : waitingSpots.Count); i++)
                {
                    patients.ElementAt(i).Notify((int)StateNotifications.OfficeMoved, obj == null);

                    if (waitingSpotsTaken[i] == false)
                        patients.ElementAt(i).Notify((int)StateNotifications.GoToDoctorWaitingSpot, i);
                }
                if (currentPatient != null)
                {
                    currentPatient.Notify((int)StateNotifications.OfficeMoved, obj == null);
                }
            }
            if (doctor != null)
            {
                doctor.Notify((int)StateNotifications.OfficeMoved, obj == null);
            }

            if (isPatientReady)
            {
                if (currentPatient != null)
                {
                    if (currentPatient.GetType() == typeof(ChildPatientAI))
                        StartHealingAnimationForKid();
                    else
                        StartHealingAnimation();
                }
            }
            if (isoObj != null)
                SetFloor(true);
            else
                SetFloor(false);

            tc = TutorialController.Instance;
            if (tc.tutorialEnabled && tc.GetCurrentStepData().NecessaryCondition == Condition.SetTutorialArrow && Tag == "BlueDoc" && tc.GetCurrentStepData().CameraTargetRotatableObjectTag == "BlueDoc")
            {
                //this is so the arrow updates it's position after the rotatable object is rotated.
                TutorialUIController.Instance.ShowIndictator(this);
            }

            DoctorRoomInfo docInf = ((DoctorRoomInfo)info.infos);
            if (docInf != null && docInf.HintOffset.Length == 4)
            {
                switch (docInf.Tag)
                {
                    case "BlueDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().BlueDoctorRoomElixir;
                        break;
                    case "GreenDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().GreenDoctorRoomElixir;
                        break;
                    case "PinkDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().PinkDoctorRoomElixir;
                        break;
                    case "PurpleDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().PurpleDoctorRoomElixir;
                        break;
                    case "RedDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().RedDoctorRoomElixir;
                        break;
                    case "SkyDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().SkyBlueDoctorRoomElixir;
                        break;
                    case "SunnyDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().SunnyYellowDoctorRoomElixir;
                        break;
                    case "WhiteDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().WhiteDoctorRoomElixir;
                        break;
                    case "YellowDoc":
                        hintElixirSprite = ResourcesHolder.GetHospital().YellowDoctorRoomElixir;
                        break;
                    default:
                        break;
                }
            }
            if (masterableProperties != null)
            {
                masterableProperties.RefreshMasteryView(false);
            }
        }
        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            patients = new LinkedList<BasePatientAI>();

            ReinitializeSpots();
            SetupMasterableProperties();

            print("initialized");

            infoShowed = false;
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            var str = save.Split(';');

            List<string> strs = null;
            if (str.Length > 1)
            {
                strs = str[1].Split('/').Where(p => p.Length > 0).ToList();
            }
            else
            {
                return;
            }
            if (strs.Count > 4)
            {
                CuredPatients = int.Parse(strs[4], System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                CuredPatients = 0;
            }

            SetupMasterableProperties();
            masterableProperties.LoadFromString(save, CuredPatients);
            base.LoadFromString(save, timePassed, CuredPatients);


            queueSize = int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture);

            if (queueSize > GetMaxQueue())
                queueSize = GetMaxQueue();

            cureAmount = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
            curationTime = float.Parse(strs[2], System.Globalization.CultureInfo.InvariantCulture);

            patients = new LinkedList<BasePatientAI>();
            if (state == State.working && strs.Count > 3)
            {

                var patientStrs = strs[3].Split('?').ToList();
                var spawner = ReferenceHolder.GetHospital().ClinicAI;
                if (patientStrs[0] != "None" && patientStrs.Count > 0 && strs[3][0] != '?')
                {
                    var infos = patientStrs[0].Split('^');
                    if (string.IsNullOrEmpty(infos[1]))
                    {
                        patients.AddLast(spawner.SpawnPatient(this, infos[0]));
                    }
                    else
                    {
                        patients.AddLast(spawner.LoadPatient(this, infos[0], infos[1], bool.Parse(infos[3]), ResourcesHolder.GetHospital().GetClinicDisease(int.Parse(infos[2], System.Globalization.CultureInfo.InvariantCulture))));
                    }

                }
                if (patientStrs.Count > 1)
                {
                    for (int i = 1; i < Mathf.Min(patientStrs.Count, GetMaxQueue()); i++)
                    {
                        var infos = patientStrs[i].Split('^');
                        if (infos != null && infos.Length > 1)
                        {
                            if (string.IsNullOrEmpty(infos[1]))
                            {
                                patients.AddLast(spawner.SpawnPatient(this, infos[0]));
                            }
                            else
                            {
                                patients.AddLast(spawner.LoadPatient(this, infos[0], infos[1], bool.Parse(infos[3]), ResourcesHolder.GetHospital().GetClinicDisease(int.Parse(infos[2]))));
                            }
                        }
                        else
                        {
                            Debug.LogError("This is bug! inform someone. It is due to fail saving of patient list when string leaves '?' at the end of string without having else");
                        }
                    }
                }
            }
            ReinitializeSpots();

            if (state == State.working)
            {
                doctor = ReferenceHolder.GetHospital().ClinicAI.SpawnDoctor(this);

                var cure = ((ShopRoomInfo)info.infos).cure;
                var medRef = ResourcesHolder.Get().medicines.cures[(int)cure.type].medicines[cure.id].GetMedicineRef();
            }

            if (curationTime > 0)
            {
                if (cureAmount < 1)
                {
                    cureAmount = 1;
                }
                LoadPatientToRoom();
            }

            if (!zzInvokePlan && !zzInvokeStarted && !HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                Invoke("ShowZzz", 3f);
                zzInvokePlan = true;
            }
            EmulateTime(timePassed);
        }

        protected override void SetupMasterableProperties()
        {

            if (masterableProperties != null)
            {
                return;
            }
            masterableProperties = new DoctorRoomMasterableProperties(this);
        }

        public override void EmulateTime(TimePassedObject timePassed)
        {
            base.EmulateTime(timePassed);
            float time = timePassed.GetTimePassed();
            if (currentPatient != null)
            {
                if (cureAmount > 0)
                {
                    var p = CureTimeMastered - curationTime;
                    if (p < 0)
                    {
                        p = 0;
                    }
                    if (p > time)
                    {
                        curationTime += time;
                        time = 0;
                    }
                    else
                    {
                        cureAmount--;
                        NotificationCenter.Instance.PatientCured.Invoke(new PatientCuredEventArgs(currentPatient, ((DoctorRoomInfo)info.infos).Tag));

                        if (!VisitingController.Instance.IsVisiting)
                        {
                            ++CuredPatients;
                            masterableProperties.AddMasteryProgress(1);
                            GameState.Get().PatientsCount.AddPatientsCuredDoctor();
                            if (currentPatient.isKid)
                            {
                                GameState.Get().PatientsCount.AddPatientsCuredKids();
                            }

                            AnalyticsController.instance.ReportDoctorPatient(((DoctorRoomInfo)info.infos), currentPatient.isKid, ExpRewardMastered, CoinRewardMastered, PositiveEnergyRewardMastered);

                            // Objectives
                            ObjectiveNotificationCenter.Instance.DoctorPatientObjectiveUpdate.Invoke(new ObjectiveDoctorPatientEventArgs(1, Tag, currentPatient.isKid));
                            NotifyPatientCured(currentPatient);
                        }

                        Timing.RunCoroutine(CreateCollectableDoc(currentPatient));
                        currentPatient.Notify((int)StateNotifications.AbortState);
                        currentPatient.IsoDestroy();
                        currentPatient = null;
                        doctor.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.disabled);
                        time -= p;
                        curationTime = -1;
                    }
                }
                else
                {
                }
                while (cureAmount > 0 && time > 0)
                {
                    MoveNextPatientToRoom();
                    if (time >= CureTimeMastered)
                    {
                        cureAmount--;
                        if (!VisitingController.Instance.IsVisiting)
                        {
                            ++CuredPatients;
                            masterableProperties.AddMasteryProgress(1);
                            GameState.Get().PatientsCount.AddPatientsCuredDoctor();
                            if (currentPatient.isKid)
                            {
                                GameState.Get().PatientsCount.AddPatientsCuredKids();
                            }

                            AnalyticsController.instance.ReportDoctorPatient(((DoctorRoomInfo)info.infos), currentPatient.isKid, ExpRewardMastered, CoinRewardMastered, PositiveEnergyRewardMastered);

                            // Objectives
                            ObjectiveNotificationCenter.Instance.DoctorPatientObjectiveUpdate.Invoke(new ObjectiveDoctorPatientEventArgs(1, Tag, currentPatient.isKid));
                            NotifyPatientCured(currentPatient);
                        }

                        if (currentPatient != null)
                        {
                            Timing.RunCoroutine(CreateCollectableDoc(currentPatient));
                        }
                        if (currentPatient != null)
                        {
                            currentPatient.Notify((int)StateNotifications.ForceRemoveFromDoctorQueue);
                            currentPatient.IsoDestroy();
                            currentPatient = null;
                        }
                        time -= CureTimeMastered;
                        curationTime = -1;
                    }
                    else
                    {
                        curationTime += time;
                        time = 0;
                    }
                }

                if (currentPatient != null)
                {
                    if (currentPatient.GetType() == typeof(ChildPatientAI))
                        ReferenceHolder.Get().engine.AddTask(StartHealingAnimationForKid);
                    else
                        ReferenceHolder.Get().engine.AddTask(StartHealingAnimation);
                }
            }
        }

        private void NotifyPatientCured(BasePatientAI patient)
        {
            switch (Tag)
            {
                case "SkyDoc":
                    ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithDiseaseEventArgs(1, DiseaseType.Lungs, false));
                    break;
                case "WhiteDoc":
                    ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithDiseaseEventArgs(1, DiseaseType.Skin, false));
                    break;
                case "YellowDoc":
                    ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithDiseaseEventArgs(1, DiseaseType.Tummy, false));
                    break;
                case "RedDoc":
                    ObjectiveNotificationCenter.Instance.HospitalPatientWithDiseaseObjectiveUpdate.Invoke(new ObjectiveHospitalPatientWithDiseaseEventArgs(1, DiseaseType.Nose, false));
                    break;
                default:
                    break;
            }
        }

        private bool IsGoHomeDependsOfTimeEmulation(float time)
        {
            var p = CureTimeMastered - curationTime;
            if (p > time)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected override string SaveToString()
        {
            var tmp = "";
            tmp += Checkers.CheckCureAmount(currentPatient, cureAmount);

            if (patients != null && patients.Count > 0)
            {
                for (int i = 0; i < patients.Count; i++)
                {
                    tmp += patients.ElementAt(i).SaveToString();
                    if (i < patients.Count - 1)
                    {
                        tmp += "?";
                    }
                }
            }
            return base.SaveToString() + ";" + Checkers.CheckedAmount(queueSize, 3, GetMaxQueue(), "QueueSize") + "/" + Checkers.CheckedAmount(cureAmount, 0, GetMaxQueue(), "cureAmount") + "/" + Checkers.CheckedAmount(curationTime, -1.0f, float.MaxValue, "curationTime").ToString("n2") + "/" + (tmp.Length > 0 ? tmp : "None") + "/" + CuredPatients.ToString() + ";" + masterableProperties.SaveToStringMastership();
        }

        private void ReinitializeSpots()
        {
            if (state != State.working)
                return;
            waitingSpots = new List<IsoObjectPrefabData.SpotData>();
            foreach (var p in actualData.spotsData)
            {
                switch ((SpotTypes)p.id)
                {
                    case SpotTypes.Door:
                        entrance = new Vector2i(p.x, p.y);
                        break;
                    case SpotTypes.CorridorChair:
                        waitingSpots.Add(p);
                        break;
                    case SpotTypes.InteriorChair:
                        break;
                    case SpotTypes.DoctorChair:
                        break;
                    case SpotTypes.Machine:
                        machine = new Vector2i(p.x, p.y);
                        break;
                    default:
                        break;
                }
            }
            if (waitingSpotsTaken == null || waitingSpotsTaken.Length != waitingSpots.Count)
                waitingSpotsTaken = new bool[waitingSpots.Count];
        }

        private void ShowQueuePopup()
        {
            ReferenceHolder.Get().engine.GetMap<HospitalAreasMapController>().ChangeOnTouchType((x) =>
            {
                CloseQueuePopup();
                if (GetBorder() != null)
                    SetBorderActive(false);
            });

            hover = DoctorHover.Open();

            hover.Initialize(((ShopRoomInfo)info.infos).cure, new Vector2(position.x + actualData.rotationPoint.x, position.y + actualData.rotationPoint.y), patients, queueSize, this);
            hover.ActualizePatientList(currentPatient, cureAmount);
            hover.SetBuyingEnable(GetMaxQueue() != queueSize);
            hover.UpdateAccordingToMode();
            tc = TutorialController.Instance;
            if (tc.tutorialEnabled && tc.GetCurrentStepData().NecessaryCondition == Condition.SetTutorialArrow && Tag == "BlueDoc" && tc.GetCurrentStepData().CameraTargetRotatableObjectTag == "BlueDoc")
            {
                TutorialUIController.Instance.HideIndicator();
            }
        }


        public void HealNow(IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = DiamondCostCalculator.GetCostForAction(CureTimeMastered - curationTime, CureTimeMastered, Tag, DoctorHover.hover.tutorialMode);
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpDoctor, Tag);
                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0f, ReferenceHolder.Get().giftSystem.particleSprites[1]);

                    if (isHealing)
                    {
                        curationTime = CureTimeMastered;

                        if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.doctor_speed_up && hover != null)
                            hover.Close();
                        NotificationCenter.Instance.PatientCured.Invoke(new PatientCuredEventArgs(currentPatient, ((DoctorRoomInfo)info.infos).Tag));
                    }
                }, diamondTransactionMaker);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void ShowDoctorHoover()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            base.OnClickWorking();
            ShowQueuePopup();
        }


        public GameObject GetMachineObject()
        {
            return machineObject;
        }

        public DoctorMachineType ReturnMachineType()
        {
            return ((DoctorRoomInfo)info.infos).MachineColor;
        }

        public string ReturnRoomName()
        {
            return I2.Loc.ScriptLocalization.Get(((DoctorRoomInfo)info.infos).ShopTitle);
        }
        public Vector3 ReturnTubePosition()
        {
            return ((DoctorRoomInfo)info.infos).ElixirTubePositions[(int)actualRotation];
        }

        public void StartHealingAnimationForKid()
        {

            if (info.infos.Tag == "YellowDoc" || info.infos.Tag == "RedDoc")
            {
                try { 
                    machineObject.GetComponent<Animator>().Play(AnimHash.MachineWorkKid, 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }else
            {
                StartHealingAnimation();
            }
        }

        public void StartHealingAnimation()
        {
            try { 
                machineObject.GetComponent<Animator>().Play(AnimHash.MachineWork, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
}
        public void StopHealingAnimation()
        {
            try { 
                doctor.SetWaitingForPatientState();
                machineObject.GetComponent<Animator>().Play(AnimHash.MachineClick, 0, 0.0f);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Animator - exception: " + e.Message);
            }
        }
        private void CloseQueuePopup(int i = 0)
        {
            DoctorHover.GetActive().Close();

            ClearHoverVariable();
        }
        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            base.OnClickWorking();

            TutorialController tc = TutorialController.Instance;
            if (tc.tutorialEnabled && tc.CurrentTutorialStepIndex < tc.GetStepId(StepTag.elixir_deliver))
                return;

            if (collectables != null && collectables.Count > 0)
            {
                if (tc.tutorialEnabled)
                {
                    //to block quickly collecting before tut step increases and not lock tutorial 
                    //on step doctor_speed_up when user leaves game and patient is healed when offline

                    //if (tc.CurrentTutorialStepTag == StepTag.patient_text_1)
                    //   return;
                    /*elseif (tc.CurrentTutorialStepTag < StepTag.patient_text_1)
                    {
                        tc.SetStep(StepTag.patient_text_1);
                    }*/

                    if (canCollect)
                    {
                        if (collectingCollectables != null)
                        {
                            Timing.KillCoroutine(collectingCollectables);
                            collectingCollectables = null;
                        }
                        collectingCollectables = Timing.RunCoroutine(CollectCollectablesCoroutine());
                    }

                }
                else
                {
                    SetBorderActive(true);
                    if (hover == null)
                        ShowQueuePopup();
                    MoveCameraToShowHover();
                }
            }
            else
            {
                SetBorderActive(true);
                if (hover == null)
                    ShowQueuePopup();
                MoveCameraToShowHover();
            }
        }

        protected override void OnClickWaitForUser()
        {
            if (isUnwraping == false)
            {
                Timing.RunCoroutine(DoctorRoomUnwrap());
                ObjectiveNotificationCenter.Instance.RotatableBuildObjectiveUpdate.Invoke(new ObjectiveRotatableEventArgs(1, Tag, ObjectiveRotatableEventArgs.EventType.Unwrap));
                AchievementNotificationCenter.Instance.ClinicRoomBuilt.Invoke(new AchievementProgressEventArgs(1));
                foreach (var p in BasePatientAI.patients)
                {
                    if (p != null)
                    {
                        p.Notify((int)StateNotifications.DoctorRoomUnpacked);
                    }
                }

            }
        }

        IEnumerator<float> DoctorRoomUnwrap()
        {
            isUnwraping = true;
            map.GetObject(new Vector2i(position.x, position.y)).GetComponent<Animator>().SetTrigger("Unwrap");

            //CHANGED BACK CUZ NOT ANIMATION WHILE UNWRAPING
            yield return Timing.WaitForSeconds(0.5f);
            var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(position.x + actualData.rotationPoint.x, 0, position.y + actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one * 1.4f;
            fp.SetActive(true);

            yield return Timing.WaitForSeconds(.1f);

            //CHANGED
            Anchored = false;
            RemoveFromMap();
            Anchored = true;
            state = State.working;
            actualData = eng.objects[map.GetObjID(info, actualRotation, state)].GetComponent<IsoObjectPrefabController>().prefabData;
            AddToMap();

            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_yellow_patient_popup_open && this.Tag == "YellowDoc")
            {
                TutorialController.Instance.SetStep(StepTag.yellow_doc_elixirs);
            }
            else if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.first_green_patient_popup_open && this.Tag == "GreenDoc")
            {
                TutorialController.Instance.SetStep(StepTag.elixir_mixer_add);
            }

            NotificationCenter.Instance.DummyRemoved.Invoke(new DummyRemovedEventArgs(this, ReturnRoomName()));
            patients = new LinkedList<BasePatientAI>();
            doctor = ReferenceHolder.GetHospital().ClinicAI.SpawnDoctor(this);

            //if (curr)
            ClinicPatientAI.UpdatePatientsWithoutRoom(this);
            //HospitalAreasMapController._map.playgroud.UpdateKidWithoutRoom(this);

            int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
            //Debug.LogError(expReward);
            int currentAmount = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false, Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentAmount);
            });

            var cure = ((ShopRoomInfo)info.infos).cure;
            var medRef = ResourcesHolder.Get().medicines.cures[(int)cure.type].medicines[cure.id].GetMedicineRef();

            isUnwraping = false;
        }

        public void AddCure()
        {
            if (hover != null)
                hover.HideTempElixirAdded();

            var cure = ((ShopRoomInfo)info.infos).cure;

            if (cureAmount + 1 > queueSize)
            {
                Debug.Log("EVERY PATIENT HAS A CURE");
                MessageController.instance.ShowMessage(3);
                return;
            }

            if (GameState.Get().GetCureCount(cure) < 1)
            {
                print("Not enough medicines in storage");

                if (Game.Instance.gameState().GetHospitalLevel() > 1)
                {
                    List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(1, ResourcesHolder.Get().medicines.cures[(int)cure.type].medicines[cure.id]));
                    UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                    {
                        if (hover == null)
                        {
                            hover = DoctorHover.Open();
                            hover.Initialize(((ShopRoomInfo)info.infos).cure, new Vector2(position.x + actualData.rotationPoint.x, position.y + actualData.rotationPoint.y), patients, queueSize, this);
                            hover.ActualizePatientList(currentPatient, cureAmount);
                            hover.SetBuyingEnable(GetMaxQueue() != queueSize);
                            hover.UpdateAccordingToMode();
                        }

                        AddCure();
                    }, () =>
                    {
                        if (hover == null)
                        {
                            hover = DoctorHover.Open();
                        }
                    }, null);
                }
                else
                {
                    MessageController.instance.ShowMessage(2);
                }
                return;
            }

            GameState.Get().GetCure(cure, 1, EconomySource.DoctorRoom);
            cureAmount += 1;
            SoundsController.Instance.PlayCureInSlot();
            if (hover != null)
                hover.ActualizePatientList(currentPatient, cureAmount);
            HideZzz();

            try
            {
                Vector3 pos = hover.GetAvatarPosition(currentPatient == null, cureAmount - 1);
                hover.BumpAvatar(currentPatient == null, cureAmount - 1);
                ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, 1, 0f, ResourcesHolder.Get().GetMedicineInfos(cure).image, false);
            }
            catch
            {
                throw new IsoException("Transform child out of bounds! Not bouncing avatar");
            }
        }


        public int GetWaitingSpot(out Vector2i pos)
        {
            for (int i = 0; i < waitingSpotsTaken.Length; i++)
                if (!waitingSpotsTaken[i])
                {
                    waitingSpotsTaken[i] = true;
                    if (actualRotation == Rotation.East)
                        pos = new Vector2i(waitingSpots[i].x, waitingSpots[i].y - 1) + position;
                    else if (actualRotation == Rotation.West)
                        pos = new Vector2i(waitingSpots[i].x, waitingSpots[i].y + 1) + position;
                    else if (actualRotation == Rotation.North)
                        pos = new Vector2i(waitingSpots[i].x + 1, waitingSpots[i].y) + position;
                    else
                        pos = new Vector2i(waitingSpots[i].x - 1, waitingSpots[i].y) + position;

                    takenSpotsCount += 1;
                    return i;
                }
            pos = new Vector2i(-1, -1);
            return -1;
        }


        public bool GetWaitingSpotByID(int id, out Vector2i pos)
        {
            if (id >= 0 && id < waitingSpots.Count)
            {
                if (waitingSpotsTaken[id] == false)
                    takenSpotsCount += 1;

                waitingSpotsTaken[id] = true;

                if (actualRotation == Rotation.East)
                    pos = new Vector2i(waitingSpots[id].x, waitingSpots[id].y - 1) + position;
                else if (actualRotation == Rotation.West)
                    pos = new Vector2i(waitingSpots[id].x, waitingSpots[id].y + 1) + position;
                else if (actualRotation == Rotation.North)
                    pos = new Vector2i(waitingSpots[id].x + 1, waitingSpots[id].y) + position;
                else
                    pos = new Vector2i(waitingSpots[id].x - 1, waitingSpots[id].y) + position;

                return true;
            }
            else
            {
                pos = new Vector2i(-1, -1);
                return false;
            }
        }

        public Vector2i ReacquireTakenSpot(int id)
        {
            if ((id == -1) || (id >= waitingSpotsTaken.Length))
            {
                Debug.LogError("Problem with reacquireT spot direction in: " + this.name);
                return new Vector2i(0, 0);
            }
            else
            {
                if (!waitingSpotsTaken[id])
                {
                    waitingSpotsTaken[id] = true;
                    takenSpotsCount += 1;
                }

                if (actualRotation == Rotation.East)
                    return new Vector2i(waitingSpots[id].x, waitingSpots[id].y - 1) + position;
                else if (actualRotation == Rotation.West)
                    return new Vector2i(waitingSpots[id].x, waitingSpots[id].y + 1) + position;
                else if (actualRotation == Rotation.North)
                    return new Vector2i(waitingSpots[id].x + 1, waitingSpots[id].y) + position;
                else
                    return new Vector2i(waitingSpots[id].x - 1, waitingSpots[id].y) + position;
            }
        }

        public Vector2 GetSpotDirection(int id)
        {
            if (id < waitingSpots.Count && id > -1)
            {
                return waitingSpots[id].direction;
            }
            else
            {
                return new Vector2(0, 0);
            }
        }

        public Vector3 GetMachinePosition(int kids = 0)
        {
            if (kids == 0)
            {
                return machinePosPatient;
            }
            else
                return machinePosKids;
        }

        public Vector2i GetMachineSpot()
        {
            return machine + position;

        }

        public void ReturnTakenSpot(int id)
        {
            if (waitingSpotsTaken != null && waitingSpotsTaken.Length > id && id >= 0)
            {
                if (takenSpotsCount > 0)
                    takenSpotsCount -= 1;

                waitingSpotsTaken[id] = false;
                if (patients != null && patients.Count > 0 && patients.Count >= waitingSpotsTaken.Length)
                {
                    patients.ElementAt(waitingSpotsTaken.Length - 1).Notify((int)StateNotifications.GoToDoctorWaitingSpot, id);
                }
            }

        }
        public Vector2i GetEntrancePosition()
        {
            return entrance + position;
        }
        public Vector2i GetParentWaitingSpot()
        {
            return position + new Vector2i(2, 2);
        }

        private BalanceableFloat moreKidsBalanceable;

        private float MoreKids
        {
            get
            {
                if (moreKidsBalanceable == null)
                {
                    moreKidsBalanceable = BalanceableFactory.CreateSpawnKidsChanceBalanceable();
                }

                return moreKidsBalanceable.GetBalancedValue();
            }
        }

        private void SpawnNextPatient()
        {
            if (GetMaxQueue() + (currentPatient == null ? 0 : 1) <= patients.Count)
                return;

            float chanceToDrawKid = Mathf.Max(MoreKids, 0f);

            if (Debug.isDebugBuild)
                Debug.Log("BALANCE -> DRAW KID: " + chanceToDrawKid);

            if (((GameState.Get().KidsToSpawn > 0 && !VisitingController.Instance.IsVisiting) || GameState.RandomFloat(0f, 1f) < chanceToDrawKid) && HospitalAreasMapController.HospitalMap.playgroud.CanGetKids())
            {
                patients.AddLast(ReferenceHolder.GetHospital().ClinicAI.SpawnKid(this, GameState.RandomNumber(2) % 2 == 0));

                if (GameState.Get().KidsToSpawn > 0 && !VisitingController.Instance.IsVisiting)
                {
                    GameState.Get().KidsToSpawn--;
                }
            }
            else
            {
                patients.AddLast(ReferenceHolder.GetHospital().ClinicAI.SpawnPatient(this, -1, GameState.RandomNumber(2) % 2 == 0));
            }

            if (patients != null && patients.Count > 1 && patients.Count <= waitingSpotsTaken.Length)
                patients.ElementAt(patients.Count - 1).Notify((int)StateNotifications.GoToDoctorWaitingSpot, patients.Count - 1);

        }

        public override RectTransform GetHoverFrame()
        {
            if (hover == null)
                return null;

            return hover.hoverFrame;
        }

        public DoctorHover GetHover()
        {
            return hover;
        }

        public void EnlargeQueue()
        {
            if (shouldWork)
            {
                Debug.Log("queue");
                QueueSize = QueueSize + 1;
                NotificationCenter.Instance.QueueExtended.Invoke(new QueueExtendedEventArgs());
            }
        }

        public void AddPatientToQueue(BasePatientAI patient)
        {
            patients.AddLast(patient);
        }

        public void PatientReady(BasePatientAI patient)
        {
            isPatientReady = true;

            if (patient.GetType() == typeof(ChildPatientAI))
                StartHealingAnimationForKid();
            else
                StartHealingAnimation();
        }

        public override void MoveTo(int x, int y)
        {
            base.MoveTo(x, y);
            foreach (var p in patients)
                p.Notify((int)StateNotifications.OfficeMoved);
            if (doctor != null)
                doctor.Notify((int)StateNotifications.OfficeMoved, null);
            if (currentPatient != null)
                currentPatient.Notify((int)StateNotifications.OfficeMoved);
        }

        public override void MoveTo(int x, int y, Rotation beforeRotation)
        {
            base.MoveTo(x, y, beforeRotation);
            foreach (var p in patients)
                p.Notify((int)StateNotifications.OfficeMoved);
            if (doctor != null)
                doctor.Notify((int)StateNotifications.OfficeMoved, null);
            if (currentPatient != null)
            {
                currentPatient.Notify((int)StateNotifications.OfficeMoved);
            }
        }

        private void MoveNextPatientToRoom()
        {
            //cureAmount -= 1;
            if (cureAmount < 0)
            {
                cureAmount = 0;
            }

            if (patients != null && patients.Count > 0)
            {
                currentPatient = patients.First();
                patients.RemoveFirst();
                currentPatient.Notify((int)StateNotifications.OfficeReadyToWalkIn);
            }

            if (patients != null && patients.Count > 0)
            {
                patients.First.Value.Notify((int)StateNotifications.FirstInQueue);
            }

            curationTime = 0;
            isHealing = true;

            doctor.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
            doctor.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.doctor);
        }

        public void DissmisPatientIfPossibleOrForceSpawnInNextSpawn()
        {

            if (patients != null && patients.Count > 0)
            {
                if (queueSize == GetMaxQueue())
                {
                    if (!VisitingController.Instance.IsVisiting)
                        GameState.Get().KidsToSpawn++;
                    return;
                }
                int iterator = 0;
                for (LinkedListNode<BasePatientAI> it = patients.First; it != null;)
                {
                    LinkedListNode<BasePatientAI> next = it.Next;
                    if (iterator == queueSize)
                    {
                        if (it.Value is ClinicPatientAI)
                        {
                            ((ClinicPatientAI)it.Value).GoHome(false);
                        }

                        it.Value = ReferenceHolder.GetHospital().ClinicAI.SpawnKid(this, GameState.RandomNumber(100) > 50);
                        break;
                    }
                    it = next;
                    ++iterator;
                }
            }

        }

        private void LoadPatientToRoom()
        {
            currentPatient = patients.First();
            patients.RemoveFirst();
            currentPatient.Notify((int)StateNotifications.OfficeReadyToWalkIn);

            if (patients != null && patients.Count > 0)
                patients.First.Value.Notify((int)StateNotifications.FirstInQueue);

            isHealing = true;
            doctor.GetComponent<IPersonCloudController>().SetCloudState(CloudsManager.CloudState.notActive);
            doctor.GetComponent<IPersonCloudController>().SetCloudMessageType(CloudsManager.MessageType.doctor);
        }

        public bool IsCuring()
        {
            return shouldWork && curationTime < CureTimeMastered && currentPatient != null;
        }

        public string GetClientTag()
        {
            if (info != null && info.infos != null)
            {
                return info.infos.Tag;
            }
            return string.Empty;
        }

        public float GetTimeToEndCuring()
        {
            return (CureTimeMastered - curationTime) + (cureAmount - 1) * CureTimeMastered;
        }

        public float GetTimeToEndCuringCurrentPatient()
        {
            return CureTimeMastered - curationTime;
        }

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            bool done = false;

            if (shouldWork && curationTime >= 0)
            {
                curationTime += Time.deltaTime;
            }
            if (shouldWork)
            {
                if (state == State.working && currentPatient == null && cureAmount > 0 && patients.Count > 0 && HospitalAreasMapController.HospitalMap.reception.canSpawnPatients)
                {
                    MoveNextPatientToRoom();
                    done = true;
                }

                if (currentPatient != null && curationTime >= CureTimeMastered)
                {
                    {
                        NotificationCenter.Instance.PatientCured.Invoke(new PatientCuredEventArgs(currentPatient));

                        currentPatient.Notify((int)StateNotifications.GoHome, true);

                        if (hover != null)
                        {
                            if (!currentPatient.isKid)
                            {
                                if (currentPatient.sprites.Sex == 0)
                                    SoundsController.Instance.PlayMaleAdultCured();
                                else
                                    SoundsController.Instance.PlayFemaleAdultCured();
                            }
                            else
                            {
                                if (GameState.RandomFloat(0, 1) < 0.5f)
                                {
                                    SoundsController.Instance.PlayChildCured();
                                }
                                else
                                {
                                    SoundsController.Instance.PlayChildCured2();
                                }
                            }

                        }

                        if (cureAmount > 0)
                        {
                            Timing.RunCoroutine(CreateCollectableDoc(currentPatient));
                        }

                        cureAmount--;

                        if (cureAmount < 0)
                        {
                            cureAmount = 0;
                        }

                        if (!HospitalAreasMapController.HospitalMap.VisitingMode)
                        {
                            CuredPatients++;
                            masterableProperties.AddMasteryProgress(1);
                            GameState.Get().PatientsCount.AddPatientsCuredDoctor();
                            if (currentPatient.isKid)
                            {
                                GameState.Get().PatientsCount.AddPatientsCuredKids();
                            }
                            AnalyticsController.instance.ReportDoctorPatient(((DoctorRoomInfo)info.infos), currentPatient.isKid, ExpRewardMastered, CoinRewardMastered, PositiveEnergyRewardMastered);

                            ObjectiveNotificationCenter.Instance.DoctorPatientObjectiveUpdate.Invoke(new ObjectiveDoctorPatientEventArgs(1, Tag, currentPatient.isKid));
                            NotifyPatientCured(currentPatient);
                        }
                    }

                    currentPatient = null;
                    curationTime = -1;
                    done = true;
                    isHealing = false;
                    doctor.SetWaitingForPatientState();


                    isPatientReady = false;
                    //if (cureAmount > 0)
                    {
                        StopHealingAnimation();
                        SaveSynchronizer.Instance.MarkToSave(SavePriorities.DoctorPatientHealed);
                    }
                }
                if (state == State.working && GetMaxQueue() > patients.Count + (currentPatient == null ? 0 : 1) && HospitalAreasMapController.HospitalMap.reception.canSpawnPatients)
                {
                    SpawnNextPatient();
                    done = true;
                }
            }
            if (hover != null && done)
                hover.ActualizePatientList(currentPatient, cureAmount);

            if (!HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                if (currentPatient == null && state != State.building && state != State.waitingForUser && Game.Instance.gameState().GetHospitalLevel() >= 3)
                {
                    if (!zzInvokePlan && !zzInvokeStarted)
                    {
                        Invoke("ShowZzz", 90f);
                        zzInvokePlan = true;
                    }
                    UpdateZzz();
                }
                else
                    HideZzz();
            }
        }


        IEnumerator<float> CreateCollectableDoc(BasePatientAI patient)
        {
            bool isKid = false;
            if (patient.GetType() == typeof(ChildPatientAI))
                isKid = true;

            if (isKid)
                CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[4], null, PositiveEnergyRewardMastered, true);

            CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[0], null, 0, false);
            if (collectables != null && collectables.Count > 0)
            {
                GameObject tempCollectable = collectables[collectables.Count - 1].gameObject;
                tempCollectable.SetActive(false);
                yield return Timing.WaitForSeconds(2.1f);
                if (collectables != null && collectables.Count > 0 && tempCollectable != null)
                {
                    tempCollectable.SetActive(true);
                    tempCollectable = null;
                }
            }
        }

        IEnumerator<float> CollectCollectablesCoroutine()
        {
            if (canCollect)
            {
                canCollect = false;
                while (collectables.Count > 0)
                {
                    CollectCollectable(true);
                    yield return Timing.WaitForSeconds(0.15f);
                }
                canCollect = true;
            }
        }

        void UpdateZzz()
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                return;
            }
            if (showHint == true && zzz != null)
            {
                if ((collectables == null || (collectables != null && collectables.Count == 0)) && (hover == null || !hover.isActiveAndEnabled))
                {
                    if (((DoctorRoomInfo)info.infos) != null && ((DoctorRoomInfo)info.infos).HintOffset.Length == 4)
                    {
                        zzz.SetPosition(((DoctorRoomInfo)info.infos).HintOffset[(int)actualRotation]);
                        if (!zzz.IsShown())
                        {
                            zzz.Show();
                        }
                    }
                    else
                    {
                        zzz.transform.localPosition = Vector3.zero;
                        if (zzz.IsShown())
                        {
                            zzz.Hide();
                        }
                    }
                }
                else
                {
                    if (zzz.IsShown())
                    {
                        zzz.Hide();
                    }
                }
            }
        }

        void ShowZzz()
        {
            if (zzz == null)
            {
                zzz = Instantiate(ResourcesHolder.GetHospital().ZzzDocPrefab).GetComponent<ZzzDoc>();
                zzz.transform.SetParent(transform);
                zzz.transform.localRotation = Quaternion.Euler(45, 45, 0);

                zzz.SetPosition(((DoctorRoomInfo)info.infos).HintOffset[(int)actualRotation]);
                zzz.SetElixirSprite(hintElixirSprite);
            }


            zzInvokePlan = false;
            showHint = true;
            zzInvokeStarted = true;

        }

        void HideZzz()
        {
            if (zzz == null)
                return;
            if (zzInvokeStarted || zzz.IsShown())
            {
                CancelInvoke("ShowZzz");
                zzz.Hide();

                showHint = false;
                zzInvokeStarted = false;
            }
        }

        public bool CanShowHint()
        {
            if (showHint)
            {
                int currentQueue = cureAmount + 1;
                if (currentQueue <= queueSize)
                    return true;
                else return false;
            }
            return false;
        }

        public void ClearHoverVariable()
        {
            hover = null;
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
    }

    public enum SpotTypes
    {
        Door,
        CorridorChair,
        InteriorChair,
        DoctorChair,
        Machine,
        EmptySpot,
        DiagnosticConsole,
        HospitalBed,
        YogaSpot,
        StretchSpot,
        BallSpot
    }
}
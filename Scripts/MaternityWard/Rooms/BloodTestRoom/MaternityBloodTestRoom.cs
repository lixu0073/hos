using Hospital;
using IsoEngine;
using MovementEffects;
using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Maternity
{
    public class MaternityBloodTestRoom : RotatableObject, IStateChangable
    {
        public static event Action<MaternityBloodTestRoom> MaternityBloodTestRoomAddedToMap;
        public MaternityBloodTestRoomStateManager<MaternityBloodTestRoom> WorkingStateManager;
        public event Action OnStateChanged;

        [SerializeField] private GameObject NurseObject;

        private Queue<IMaternityFacilityPatient> patientsQueue;
        private SortedDictionary<long, IMaternityFacilityPatient> patientsFromSaveSortedViadiagnoseTimeAdded;
        Animator doctorAnimator;
        Animator machineAnimator;
        Transform floor = null;
        BloodTestHover hover;
        [SerializeField] private int MaxQueueSize = 9;
        private int queueSize = 3;
        public int QueueSize { get { return queueSize; } private set { } }
        private bool isUnwraping;
        private bool canCollect = true;
        private long ID;
        public int BloodTestAnalyzed = 0;
        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;
        private Coroutine collectingCollectables;
        private float delayTimeForCreatingCollectables = 0.5f;
        private float delyTimeForCollectingNormalCollectables = 0.15f;
        private float delyTimeForCollectingInstantCollectables = 0.75f;

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public int GetQueueSize()
        {
            return patientsQueue.Count;
        }

        public long GetRoomID()
        {
            return ID;
        }

        #region Rotatable_Region
        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
                EmulateTime((TimePassedObject)parameters);
        }

        public bool CanAddPatientToQueue()
        {
            return patientsQueue.Count < queueSize;
        }

        private void SetFloor(bool val)
        {
            if (floor == null)
                throw new IsoException("doctor floor doesn't exist!!");
            floor.gameObject.SetActive(val);
            floor.position = new Vector3(position.x + actualData.rotationPoint.x + 5 / Mathf.Sqrt(2), 0.05f - 5, position.y + actualData.rotationPoint.y + 5 / Mathf.Sqrt(2));
        }

        private void DestroyMaterials()
        {
            DestroyFloorMaterial();
        }

        private void DestroyFloorMaterial()
        {
            EngineController.DestroyMaterial(floor.gameObject);
        }

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North, State _state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, _state, shouldDisappear);
            patientsQueue = new Queue<IMaternityFacilityPatient>();
        }

        protected override void AddToMap()
        {
            base.AddToMap();
            OnBloodTestRoomAdded();
            SetFloor(isoObj != null);
            if (isoObj != null)
            {
                GetObj().transform.GetChild(0).gameObject.SetActive(true);
                doctorAnimator = map.GetObject(position).transform.GetChild(0).GetChild(0).gameObject.GetComponent<Animator>();
                machineAnimator = map.GetObject(position).transform.GetChild(2).gameObject.GetComponent<Animator>();
                if (WorkingStateManager != null && WorkingStateManager.State.GetTag() == BloodTestRoomState.TBS)
                {
                    try
                    { 
                        doctorAnimator.Play("Scientist_wait_summary", 0, 0.0f);
                        machineAnimator.SetBool("Working", true);
                        machineAnimator.Play("Blood_Lab_Machine_work", 0, 0.0f);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Animator - exception: " + e.Message);
                    }
                }
            }
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            if (WorkingStateManager != null)
                WorkingStateManager.State.Notify(value ? (int)BloodTestRoomNotifications.OfficeUnanchored : (int)BloodTestRoomNotifications.OfficeAnchored, null);

            foreach (var p in BasePatientAI.patients)
            {
                if (p != null)
                {
                    if (p is MaternityPatientAI)
                        p.Notify(value ? (int)StateNotifications.OfficeAnchored : (int)StateNotifications.OfficeUnAnchored, obj == null);
                }
            }
        }

        protected override string SaveToString()
        {
            return base.SaveToString() + ";" + Checkers.CheckedAmount(queueSize, 3, MaxQueueSize, "QueueSize") + "/" + Checkers.CheckedAmount(BloodTestAnalyzed, 0, int.MaxValue, "amountOfCuredPatients");
        }

        protected override string SaveCollectables(string p)
        {
            StringBuilder savedCollectables = new StringBuilder();
            if (collectables != null)
            {
                p += "/";
                for (int i = 0; i < collectables.Count; i++)
                {
                    savedCollectables.Append(Checkers.CheckedAmount(collectables[i].amount, 0, int.MaxValue, "Collectables amount"));
                    savedCollectables.Append("!");
                    savedCollectables.Append("BloodResult");
                    savedCollectables.Append("!");
                    savedCollectables.Append(collectables[i].patientID);
                    if (i < collectables.Count - 1)
                        savedCollectables.Append("&");

                    p += savedCollectables.ToString();
                    savedCollectables.Length = 0;
                    savedCollectables.Capacity = 0;
                }
            }

            return p;
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            patientsQueue = new Queue<IMaternityFacilityPatient>();
            string[] saveData = save.Split(';');
            if (saveData.Length > 1)
            {
                string[] dataForMachine = saveData[1].Split('/');
                queueSize = Int32.Parse(dataForMachine[0]);
                BloodTestAnalyzed = Int32.Parse(dataForMachine[1]);
            }
            InitializeStateMachine();
            EmulateTime(timePassed);
        }

        public override void OnLoadEnded()
        {
            AddSortedPatientsToQueue();
            if (patientsQueue.Count > 0)
                WorkingStateManager.State.Notify((int)BloodTestRoomNotifications.PatientAdded, null);
        }

        private void AddSortedPatientsToQueue()
        {
            if (patientsFromSaveSortedViadiagnoseTimeAdded != null && patientsFromSaveSortedViadiagnoseTimeAdded.Count > 0)
            {
                foreach (KeyValuePair<long, IMaternityFacilityPatient> item in patientsFromSaveSortedViadiagnoseTimeAdded)
                {
                    patientsQueue.Enqueue(item.Value);
                }
            }
        }

        private void InitializeStateMachine()
        {
            if (WorkingStateManager == null)
            {
                WorkingStateManager = new MaternityBloodTestRoomStateManager<MaternityBloodTestRoom>(this);
                WorkingStateManager.State = new MaternityBloodTestWaitingForBloodSample(this);
            }
        }

        public int GetAmountOfBloodTestsPerformed()
        {
            return BloodTestAnalyzed;
        }

        public override void EmulateTime(TimePassedObject time)
        {
            base.EmulateTime(time);
        }

        protected override void OnStateChange(State newState, State oldState)
        {
            base.OnStateChange(newState, oldState);
            if (floor != null)
            {
                DestroyFloorMaterial();
                Destroy(floor.gameObject);
            }

            if (newState == State.building)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var mat = p.GetComponent<Renderer>();
                mat.material = new Material(AreaMapController.Map.wallDatabase.materialPrefab);
                mat.material.mainTexture = ResourcesHolder.Get().UnderConstructionTile;
                mat.material.mainTextureScale = Vector2.one * 4;
                floor = p.transform;
                floor.localScale = Vector3.one * 4;
                floor.rotation = Quaternion.Euler(90, 0, 0);
            }

            if (newState == State.working || newState == State.fresh || newState == State.waitingForUser)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                var mat = p.GetComponent<Renderer>();
                mat.material = new Material(AreaMapController.Map.wallDatabase.materialPrefab);
                mat.material.mainTexture = ResourcesHolder.GetMaternity().BloodDiagnosisRoomFloor;
                floor = p.transform;
                floor.localScale = Vector3.one * 4;
                floor.rotation = Quaternion.Euler(90, 0, 0);
            }

            floor.transform.SetParent(transform);
        }

        public override void IsoDestroy()
        {
            DestroyMaterials();
            base.IsoDestroy();
        }

        public string ReturnRoomName()
        {
            return I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)info.infos).ShopTitle);
        }

        public void RedirectTo(bool openHover = true)
        {
            base.OnClickWorking();
            if (openHover)
                ShowDiagnosisHoover(false);
            else
                MoveCameraToThisRoom();
        }

        protected override void OnClickWorking()
        {
            TutorialController tc = TutorialController.Instance;
            if (tc != null && tc.GetCurrentTutorialStep() != null && tc.GetCurrentTutorialStep().StepTag == StepTag.maternity_labor_room_completed)
                return;

            base.OnClickWorking();
            if (collectables != null && collectables.Count > 0)
            {
                if (canCollect)
                {
                    if (collectingCollectables != null)
                    {
                        try
                        {
                            StopCoroutine(collectingCollectables);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                        }
                        collectingCollectables = null;
                    }
                    collectingCollectables = StartCoroutine(CollectCollectablesCoroutine());
                }
            }
            else
            {
                SetBorderActive(true);
                BounceDiagnosticObject();
                ShowDiagnosisHoover(false);
            }
        }

        protected override void OnClickWaitForUser()
        {
            if (isUnwraping == false)
                Timing.RunCoroutine(RoomUnwrap());

            InitializeStateMachine();
            ID = (long)ServerTime.getMilliSecTime();
        }

        IEnumerator<float> RoomUnwrap()
        {
            isUnwraping = true;
            map.GetObject(new Vector2i(position.x, position.y)).GetComponent<Animator>().SetTrigger("Unwrap");

            //CHANGED BACK CUZ NOT ANIMATION WHILE UNWRAPING
            yield return Timing.WaitForSeconds(0.5f);
            var fp = Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(position.x + actualData.rotationPoint.x, 0, position.y + actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
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

            patientsQueue = new Queue<IMaternityFacilityPatient>();

            int expReward = ((ShopRoomInfo)info.infos).buildXPReward;
            int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
            Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.BuildingBuilt, false);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
            {
                Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
            });
            isUnwraping = false;
        }

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            //if (hover != null)
            //{
            //    hover.ActualizePatientList(bloodTestSamplesQueue);
            //}
        }

        public override void MoveTo(int x, int y)
        {
            base.MoveTo(x, y);
        }

        public override void MoveTo(int x, int y, Rotation beforeRotation)
        {
            base.MoveTo(x, y, beforeRotation);
        }
        #endregion

        public bool CanEnlargeQueue()
        {
            int amountToIncrease = 1;
            int maxWaitingSlots = MaxQueueSize - 1;

            int newQueueSize = queueSize + amountToIncrease;
            if (newQueueSize > MaxQueueSize)
            {
                hover.SetBuyingEnable(false);
                return false;
            }
            else
                return true;
        }

        public void EnlargeQueueByOne()
        {
            int amountToIncrease = 1;
            int maxWaitingSlots = MaxQueueSize - 1;
            int newQueueSize = queueSize + amountToIncrease;
            queueSize += amountToIncrease;
            if (hover != null)
            {
                hover.EnlargeQueue(amountToIncrease);
                hover.ActualizePatientList(patientsQueue);
                if (newQueueSize - 1 == maxWaitingSlots)
                    hover.SetBuyingEnable(false);
            }
        }

        public void HealNow(int diamondCost, IDiamondTransactionMaker diamondTransactionMaker)
        {
            int cost = diamondCost;
            if (Game.Instance.gameState().GetDiamondAmount() >= cost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                {
                    Game.Instance.gameState().RemoveDiamonds(cost, EconomySource.SpeedUpBloodTest);
                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0f, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    patientsQueue.Peek().GetPatientAI().Notify((int)StateNotifications.BloodTestSpeedUp, null);
                    if (hover != null)
                        hover.ActualizePatientList(patientsQueue);
                }, diamondTransactionMaker);
            }
            else
            {
                MessageController.instance.ShowMessage(1);
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public int GetSpeedUpDiagnoseCost(float timeLeft)
        {
            if (patientsQueue.Count <= 0)
                return 0;
            return DiamondCostCalculator.GetCostForAction(timeLeft, ((MaternityBloodTestRoomInfo)info.infos).GetDiagnoseTime());
        }

        public void AddBloodTestToQueue(IMaternityFacilityPatient patient)
        {
            if (patientsQueue.Count < queueSize)
            {
                patientsQueue.Enqueue(patient);
                if (patientsQueue.Count == 1)
                {
                    patient.GetPatientAI().Notify((int)StateNotifications.FirstInQueue, null);
                    WorkingStateManager.State.Notify((int)BloodTestRoomNotifications.PatientAdded, null);
                }
            }
        }

        public void AddPatientFromSaveToQueue(IMaternityFacilityPatient patient, long timeDiagnoseSend)
        {
            if (patientsFromSaveSortedViadiagnoseTimeAdded == null)
                patientsFromSaveSortedViadiagnoseTimeAdded = new SortedDictionary<long, IMaternityFacilityPatient>();

            patientsFromSaveSortedViadiagnoseTimeAdded.Add(timeDiagnoseSend, patient);
        }

        private bool PatientInWFDRState()
        {
            bool result = patientsQueue != null && patientsQueue.Count > 0;

            if(result)
            {
                IMaternityFacilityPatient peek = patientsQueue.Peek();

                result = peek != null && peek.GetPatientAI() != null && peek.GetPatientAI().Person != null && peek.GetPatientAI().Person.State != null;

                if(result)
                    result = peek.GetPatientAI().Person.State.GetTag() == PatientStates.MaternityPatientStateTag.WFDR;
            }

            return result;
        }

        public void OnPatientHealed()
        {
            //if (patientsQueue.Peek().GetPatientAI().Person.State.GetTag() == PatientStates.MaternityPatientStateTag.WFDR)
            if (PatientInWFDRState())
            {
                if(doctorAnimator != null) doctorAnimator.SetTrigger("ResultsReady");
                if(machineAnimator != null) machineAnimator.SetTrigger("ResultsReady");
                StartCoroutine(CreateCollectable(patientsQueue.Peek().GetPatientID()));
                IMaternityFacilityPatient dequeuedPatient = patientsQueue.Dequeue();
                IMaternityFacilityPatient newPatient = null;
                BloodTestAnalyzed++;
                if (patientsQueue.Count > 0)
                {
                    WorkingStateManager.State = new MaternityBloodTestRoomTestingBloodSample(this, patientsQueue.Peek());
                    patientsQueue.Peek().GetPatientAI().Notify((int)StateNotifications.FirstInQueue, 0);
                    newPatient = patientsQueue.Peek();
                }
                else
                {
                    WorkingStateManager.State.Notify((int)BloodTestRoomNotifications.QueueEmpty, null);
                    PatientStates.MaternityPatientInDiagnoseState.ResetDiagnosedTimeToEmulate();
                }
                if (hover != null)
                {
                    hover.OnPatientDequeue(dequeuedPatient, newPatient);
                    hover.ActualizePatientList(patientsQueue);
                }
            }
        }

        public void StartHealingAnimation()
        {
            StartCoroutine(PlayHealingAnimations());
        }

        private IEnumerator PlayHealingAnimations()
        {
            yield return new WaitForEndOfFrame();
            doctorAnimator.ResetTrigger("ResultsReady");
            machineAnimator.ResetTrigger("ResultsReady");
            doctorAnimator.SetBool("Working", true);
            machineAnimator.SetBool("Working", true);
        }

        public void StartIdleAnimation()
        {
            StartCoroutine(PlayIdleAnimations());
        }

        private IEnumerator PlayIdleAnimations()
        {
            yield return new WaitForEndOfFrame();
            doctorAnimator.SetBool("Working", false);
            machineAnimator.SetBool("Working", false);
        }

        public IMaternityFacilityPatient GetFirstPatient()
        {
            return patientsQueue.Peek();
        }

        public void ShowDiagnosisHoover(bool newPatient = false)
        {
            StartCoroutine(ShowQueuePopup(newPatient));
        }

        IEnumerator ShowQueuePopup(bool newPatient = false)
        {
            yield return new WaitForEndOfFrame();
            ReferenceHolder.Get().engine.GetMap<AreaMapController>().ChangeOnTouchType((x) =>
            {
                CloseQueuePopup();
                if (GetBorder() != null)
                    SetBorderActive(false);
            });

            yield return new WaitForEndOfFrame();   //so hover properly closes and destroys avatars

            hover = BloodTestHover.Open();
            hover.gameObject.transform.localScale = Vector3.one * 0.5f;
            int howManyAwaitingPatientsForDiagnosis = 0;
            foreach (MaternityPatientAI item in MaternityPatientsHolder.Instance.GetPatientsList())
            {
                if (item.Person.State.GetTag() == PatientStates.MaternityPatientStateTag.WFSTD)
                    ++howManyAwaitingPatientsForDiagnosis;
            }

            hover.Initialize(new Vector2(position.x + actualData.rotationPoint.x, position.y + actualData.rotationPoint.y), patientsQueue, queueSize, this, howManyAwaitingPatientsForDiagnosis); //matward trzeba wyliczyc ile matek czeka na diagnoze ale nei jest wyslanych.
            hover.SetBuyingEnable(MaxQueueSize > QueueSize);
            hover.UpdateAccordingToMode();
            MoveCameraToShowHover();
        }

        public void ShowBloodTestHover()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            base.OnClickWorking();
            gameObject.SetActive(true);
            StartCoroutine(ShowQueuePopup());
        }

        private void CloseQueuePopup()
        {
            BloodTestHover.GetActive().Close();
            hover = null;
        }

        public override RectTransform GetHoverFrame()
        {
            if (hover == null)
                return null;

            return hover.hoverFrame;
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

        public BloodTestHover GetHover()
        {
            return hover;
        }

        IEnumerator<float> CreateCollectable(string patientID)
        {
            CreateCollectable(ReferenceHolder.Get().giftSystem.particleSprites[6], null, 1, false, patientID);

            if (collectables != null && collectables.Count > 0)
            {
                GameObject tempCollectable = collectables[collectables.Count - 1].gameObject;
                tempCollectable.SetActive(false);
                yield return Timing.WaitForSeconds(delayTimeForCreatingCollectables);
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
                    CollectCollectable();
                    yield return Timing.WaitForSeconds(delyTimeForCollectingNormalCollectables);
                }
                canCollect = true;
            }
        }

        public void CollectInstantHealCollectable()
        {
            StartCoroutine(CollectCollectableFromInstantHeal());
        }

        IEnumerator<float> CollectCollectableFromInstantHeal()
        {
            if (canCollect)
            {
                canCollect = false;
                yield return Timing.WaitForSeconds(delyTimeForCollectingInstantCollectables);
                CollectCollectable();
                canCollect = true;
            }
        }

        protected override void CollectCollectable(bool isDoctor = false, bool isKid = false, bool overflowStorage = false)
        {
            if (collectables != null && collectables.Count > 0)
            {
                Collectable first = collectables[collectables.Count - 1];
                int amountOfCollectable = 1;
                MaternityWaitingRoom waitingRoom = MaternityWaitingRoomController.Instance.GetMaternityWaitingRoomForPatientID(first.patientID);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.BloodTest, first.transform.position + new Vector3(-.1f, .75f, 0), amountOfCollectable, 0.1f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[6], null, waitingRoom.RevealBloodTestBedTable, true, first.patientID);
                MaternityPatientAI patientToNotyfi = MaternityWaitingRoomController.Instance.GetPatientByID(first.patientID);
                int expReward = ((MaternityBloodTestRoomInfo)GetRoomInfo()).GetExpRewardForBloodTest();
                if (patientToNotyfi != null)
                    expReward = patientToNotyfi.GetInfoPatient().GetExpForStage(MaternityCharacterInfo.Stage.Diagnose);

                int currentExpReward = Game.Instance.gameState().GetExperienceAmount();
                Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.BloodTestCollected, false);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, first.transform.position + new Vector3(-.1f, .75f, 0), expReward, 0f, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                {
                    Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpReward);
                });
                if (patientToNotyfi != null)
                    patientToNotyfi.Notify((int)StateNotifications.CuresDelivered, null);

                Destroy(first.gameObject);
                collectables.RemoveAt(collectables.Count - 1);
            }
        }

        void BounceDiagnosticObject(bool isBeingBuilt = false)
        {
            //Timing.KillCoroutine(BounceCoroutine());
            Timing.RunCoroutine(BounceCoroutine(isBeingBuilt));
        }

        IEnumerator<float> BounceCoroutine(bool isBeingBuilt = false)
        {
            //Debug.Log("BounceCoroutine");
            float bounceTime = .15f;
            float timer = 0f;
            Transform targetTransform;
            //if (isBeingBuilt)
            //    targetTransform = isoObj.GetGameObject().transform;
            //else
                targetTransform = isoObj.GetGameObject().transform;

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            targetScale = normalScale * 1.05f; //new Vector3(1.1f, 1.1f, 1.1f);

            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
                //scale up
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(normalScale, targetScale, timer / bounceTime);
                    yield return 0;
                }
                timer = 0f;
                //scale down
                while (timer < bounceTime)
                {
                    timer += Time.deltaTime;
                    targetTransform.localScale = Vector3.Lerp(targetScale, normalScale, timer / bounceTime);
                    yield return 0;
                }
            }
            else yield return 0;
        }

        private void OnBloodTestRoomAdded()
        {
            MaternityBloodTestRoomAddedToMap?.Invoke(this);
        }

        public void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }

    public enum BloodTestRoomState
    {
        WFBS, // Waiting for blood sample
        TBS   // Testing blood sample
    }
}


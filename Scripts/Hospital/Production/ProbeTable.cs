using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using IsoEngine;
using SimpleUI;
using System;
using MovementEffects;
using System.Globalization;

namespace Hospital
{
    public class ProbeTable : RotatableWithoutBuilding, IFillable, ICollectable, IHoverable
    {
        #region privates

        Sprite defaultSprite;
        public MedicineRef producedElixir = null;
        private float productionTime;

        public float ProductionTime
        {
            get { return productionTime; }
        }

        private float productionTimeLeft;

        public float ProductionTimeLeft
        {
            get { return productionTimeLeft; }
        }

        private bool shouldWork = true;
        TableState tableState;
        ProbeTableHover hover;
        private GameObject bubble;
        private GameObject selection;
        private SpriteRenderer selectionRenderer;
        private Animator anim;

        // Coroutine collectBounce;

        private Vector3 positionStart = Vector3.zero;

        public GameObject Selection
        {
            get { return selection; }
            private set { }
        }

        #endregion

        public bool IsReadyToCollect()
        {
            return tableState == TableState.waitingForUser;
        }

        public bool ReadyToFill()
        {
            return tableState == TableState.empty && state == State.working;
        }

        #region Initialization

        private void Init()
        {
            tableState = TableState.empty;
            defaultSprite = ResourcesHolder.GetHospital().baseProbeTableImage;
            if (isoObj != null)
            {
                selection = isoObj.GetGameObject().transform.GetChild(0).transform.GetChild(1).gameObject;
                selectionRenderer = selection.GetComponent<SpriteRenderer>();
            }

            positionStart = gameObject.transform.GetChild(0).localPosition;
        }

        public override void StartBuilding()
        {
            base.StartBuilding();
            ProbeTableTool.fillable.Add(this);
            ProbeTableTool.collectable.Add(this);

            if (!TutorialController.Instance.IsNonLinearStepCompleted(StepTag.NL_newspaper_probe_tables) &&
                !VisitingController.Instance.IsVisiting)
            {
                if (FindObjectsOfType<ProbeTable>().Length >= 21)
                    NotificationCenter.Instance.TwentyProbeTables.Invoke(new BaseNotificationEventArgs());
            }
        }

        public override void IsoDestroy()
        {
            ProbeTableTool.fillable.Remove(this);
            ProbeTableTool.collectable.Remove(this);
            HospitalAreasMapController.HospitalMap.RemoveProbeTableFromMap(this);
            base.IsoDestroy();
        }

        public override void Initialize(Rotations info, Vector2i position, Rotation rotation = Rotation.North,
            State state = State.fresh, bool shouldDisappear = true)
        {
            base.Initialize(info, position, rotation, state, shouldDisappear);
            Init();
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            base.LoadFromString(save, timePassed);

            StartBuilding();
            Init();
            SetBaseTableSprite();

            var str = save.Split(';');

            if (str.Length < 2)
                return;
            var p = str[1].Split('/');
            tableState = (TableState)Enum.Parse(typeof(TableState), p[0]);
            if (tableState != TableState.empty)
            {
                producedElixir = MedicineRef.Parse(p[1]);
                productionTime = (ResourcesHolder.Get().GetMedicineInfos(producedElixir) as BaseElixirInfo)
                    .ProductionTime;
                productionTimeLeft = Mathf.Clamp(float.Parse(p[2], CultureInfo.InvariantCulture), 0, productionTime);

                MedicineBadgeHintsController.Get().AddSingleMedInProduction(producedElixir, 1);
                //HintsController.Get().RemoveHint(new CollectHint(producedElixir));

                SetElixirSprite();

                /*
                if (tableState == TableState.waitingForUser)
                {
                    HintsController.Get().AddHint(new CollectHint(producedElixir));
                    // anim.SetTrigger("Ready");
                }
                */
                //ShowBubbles();
            }

            EmulateTime(timePassed);
        }

        protected override string SaveToString()
        {
            return base.SaveToString() + ";" + Checkers.CheckedTableState(tableState) + "/" + (producedElixir == null
                ? "-"
                : Checkers.CheckedMedicine(producedElixir, Tag).ToString() + "/" + Checkers
                    .CheckedAmount(productionTimeLeft, -1, productionTime, Tag + " productionTimeLeft: ")
                    .ToString("n2"));
        }

        public override void Notify(int id, object parameters)
        {
            base.Notify(id, parameters);
            if (id == (int)LoadNotification.EmulateTime)
            {
                EmulateTime((TimePassedObject)parameters);
            }
        }

        public override void EmulateTime(TimePassedObject time)
        {
            base.EmulateTime(time);
            if (state == State.working)
            {
                productionTimeLeft = Mathf.Clamp(Mathf.Max(productionTimeLeft - time.GetTimePassed(), 0), -1,
                    productionTime);
            }
        }

        #endregion

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        #region OnMapManipulations

        protected override void AddToMap()
        {
            base.AddToMap();

            if (GetActualGameObject() != null)
                anim = GetActualGameObject().GetComponent<Animator>();

            if (isoObj != null)
            {
                selection = isoObj.GetGameObject().transform.GetChild(0).transform.GetChild(1).gameObject;

                selectionRenderer = selection.GetComponent<SpriteRenderer>();

                if (producedElixir != null)
                    SetElixirSprite();
                else
                    SetBaseTableSprite();

                if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.new_probe_tables)
                    NotificationCenter.Instance.ObjectExistOnLevel.Invoke(new ObjectExistOnLevelEventArgs());
            }
            else
                selection = null;

            HospitalAreasMapController.HospitalMap.AddProbeTableToMap(this);
        }

        public override void SetAnchored(bool value)
        {
            base.SetAnchored(value);
            shouldWork = obj == null && Anchored;
        }

        #endregion

        public bool IsArrowEnable()
        {
            foreach (Transform child in transform)
                if (child.CompareTag("Arrow"))
                {
                    return true;
                }

            return false;
        }

        #region Interaction

        public override void IsoUpdate()
        {
            base.IsoUpdate();
            if (tableState == TableState.producing && producedElixir != null)
            {
                productionTimeLeft -= Time.deltaTime;
                if (shouldWork)
                {
                    SetElixirSprite();
                    if (productionTimeLeft < 0)
                    {
                        productionTimeLeft = 0;
                        StartWaiting();
                    }

                    if (hover != null)
                    {
                        hover.SetProductionBar((int)productionTime - (int)productionTimeLeft, (int)productionTime);
                        hover.SetSpeedUpCostText(productionTimeLeft, productionTime);
                    }
                }
                else if (productionTimeLeft < 0)
                {
                    productionTimeLeft = 0;
                }
            }
        }

        private void HideBubbles()
        {
            if (bubble != null)
                Destroy(bubble);
        }

        private void ShowBubbles(bool updateHint = false)
        {
            if (bubble != null)
            {
                Destroy(bubble);
            }

            bubble = Instantiate(((BaseElixirInfo)ResourcesHolder.Get().GetMedicineInfos(producedElixir)).Bubbles);
            bubble.transform.SetParent(GetActualGameObject().transform);
            bubble.transform.localPosition = new Vector3(0.1f, 0.9f, .1f);

            bubble.transform.localScale = new Vector3(1, 1, 1);
            bubble.transform.rotation = Quaternion.Euler(270, 0, 0);

            if (anim != null)
            {
                anim.speed = GameState.RandomFloat(0.75f, 1f);
                //anim.SetTrigger("Ready");
                Timing.RunCoroutine(SetReadyCoroutine());
            }
        }

        public void SpeedUpWithDiamonds(IDiamondTransactionMaker diamondTransactionMaker, Action onSuccess)
        {
            if (tableState == TableState.producing)
            {
                int cost = DiamondCostCalculator.GetCostForAction(productionTimeLeft, productionTime);
                if (Game.Instance.gameState().GetDiamondAmount() >= cost)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(cost, delegate
                    {
                        GameState.Get().RemoveDiamonds(cost, EconomySource.SpeedUpProbeTable, Tag);
                        productionTimeLeft = 0;

                        Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, 1,
                            transform.position.z + actualData.rotationPoint.y);
                        ReferenceHolder.Get().giftSystem.CreateItemUsed(pos + new Vector3(.5f, 0, .5f), cost, 0,
                            ReferenceHolder.Get().giftSystem.particleSprites[1]);
                        NotificationCenter.Instance.BoughtWithDiamonds.Invoke(new BoughtWithDiamondsEventArgs());
                        onSuccess?.Invoke();
                    }, diamondTransactionMaker);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
        }

        public void SelectForSpeedUp()
        {
            OnClickWorking();
        }

        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            TutorialController tc = TutorialController.Instance;
            if (!tc.tutorialEnabled ||
                (tc.tutorialEnabled && (tc.CurrentTutorialStepTag == StepTag.elixir_collect_text) ||
                 (tc.CurrentTutorialStepIndex >= tc.GetStepId(StepTag.elixir_seed_text_after)) ||
                 (!tc.tutorialEnabled)))
            {
                Debug.Log("ProbeTable OnClickWorking()");
                SoundsController.Instance.PlayProbeTableSelect();
                base.OnClickWorking();

                anim.speed = 1f;
                anim.SetTrigger("Click");
                ShowHover();
            }
        }

        public override RectTransform GetHoverFrame()
        {
            if (hover == null)
                return null;
            return hover.hoverFrame;
        }

        /// <summary>
        /// Show the probe table hover.
        /// </summary>
        private void ShowHover()
        {
            //Set touch elsewhere to close the hover
            HospitalAreasMapController.HospitalMap.ChangeOnTouchType((x) => { CloseHover(); });

            var probeHover = ProbeTableHover.Open(this);
            probeHover.Initialize(this, tableState);

            hover = probeHover;
            if (selection != null)
            {
                selection.SetActive(true);
                if (tableState == TableState.empty)
                {
                    selectionRenderer.sprite = ResourcesHolder.GetHospital().ProbeSelectionEmpty;
                }
                else
                {
                    selectionRenderer.sprite = ResourcesHolder.GetHospital().ProbeSelectionFull;
                }
            }

            hover.UpdateAccordingToMode();
            MoveCameraToShowHover();
        }

        /// <summary>
        /// Close the probe table hover.
        /// </summary>
        public void CloseHover()
        {
            if (selection != null)
                selection.SetActive(false);
            if (hover != null)
                hover.Close();
            hover = null;
        }

        #endregion

        public TableState GetTableState()
        {
            return tableState;
        }

        private void StartWaiting()
        {
            tableState = TableState.waitingForUser;
            ShowBubbles(true);
            if (hover != null)
                hover.ChangedFromProductionToWaiting();
        }

        #region Collect/Fill Tool interaction

        public bool Collect(Vector2i position)
        {
            if (tableState != TableState.waitingForUser || this.position != position)
                return false;
            if (!GameState.Get().CanAddAmountForTankStorage(1))
            {
                MessageController.instance.ShowMessage(47);
                if (hover)
                    hover.Close();
                StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(true));
                return false;
            }

            SoundsController.Instance.PlayCollectElixir();

            //var canvas = UIController.get.canvas;
            var pos = Utils.GetScreenPosition(transform.position);
            UIController.get.storageCounter.Add(producedElixir.IsMedicineForTankElixir());
            MedicineRef medToAdd = producedElixir;

            if (producedElixir != null)
            {
                MedicineBadgeHintsController.Get().RemoveMedInProduction(producedElixir);
                // HintsController.Get().RemoveHint(new CollectHint(producedElixir));
            }

            GameState.Get().AddResource(medToAdd, 1, false, EconomySource.ProbeTable);

            bool isMedicineForTankElixir = producedElixir.IsMedicineForTankElixir();
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, transform.position, 1, 1f, 1.75f,
                Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(producedElixir), null,
                () => { UIController.get.storageCounter.Remove(1, isMedicineForTankElixir); });

            int expReward = ResourcesHolder.Get().GetEXPForCure(producedElixir);
            int currentExpReward = Game.Instance.gameState().GetExperienceAmount();
            GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.MedicineProduced, false, Tag);
            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, transform.position, expReward, 1.5f,
                1.75f, Vector3.one, new Vector3(1, 1, 1), null, null,
                () => { GameState.Get().UpdateCounter(ResourceType.Exp, expReward, currentExpReward); });
            GameState.Get().UpdateExtraGift(transform.position, false, SpecialItemTarget.Tank);
            HandlePickupParticle(transform.position, true);

            // GetActualGameObject().transform.GetChild(2).GetComponent<ParticleSystem>().Play(); //Particle system pop
            NotificationCenter.Instance.MedicineExistInStorage.Invoke(
                new MedicineExistInStorageEventArgs(producedElixir, GameState.Get().GetCureCount(producedElixir)));

            //Achievement
            AchievementNotificationCenter.Instance.ElixirCollected.Invoke(new TimedAchievementProgressEventArgs(1,
                Convert.ToInt32((ServerTime.Get().GetServerTime()
                    .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))).TotalSeconds)));

            GameState.Get().CuresCount.AddProducedElixirs(1);

            tableState = TableState.empty;

            HideBubbles();

            if (anim != null)
            {
                anim.speed = 1f;
                anim.SetTrigger("Collect");
            }

            Invoke("SwapSpritesOnCollect",
                39f / 120f); //39/120 because thats the animation point when it should be fired (but we dont have a way of firing animation event with methods in this script)

            if (producedElixir != null)
            {
                StartCoroutine(FireCollectionParticles(producedElixir));
            }

            producedElixir = null;

            NotificationCenter.Instance.BluePotionsCollected.Invoke(new BluePotionsCollectedEventArgs());

            gameObject.transform.GetChild(0).localPosition = positionStart;

            return true;
        }

        void SwapSpritesOnCollect()
        {
            SetBaseTableSprite();
        }

        IEnumerator FireCollectionParticles(MedicineRef producedElixir)
        {
            yield return new WaitForSeconds(25f / 120f); //in this frame of collect animation this would be fired
            GameObject explosion =
                Instantiate(((BaseElixirInfo)ResourcesHolder.Get().GetMedicineInfos(producedElixir)).Explosion);
            explosion.transform.SetParent(GetActualGameObject().transform);
            explosion.transform.localPosition = Vector3.zero; //so the position is taken from explosion prefab
            explosion.transform.rotation = Quaternion.Euler(270, 0, 0);
            Destroy(explosion, 3f);
        }

        public bool Fill(Vector2i position, MedicineRef medicine)
        {
            if (tableState != TableState.empty || this.position != position ||
                (TutorialController.Instance.tutorialEnabled && TutorialController.Instance.CurrentTutorialStepIndex <
                    TutorialController.Instance.GetStepId(StepTag.elixir_seed_text_after)))
                return false;

            var p = ResourcesHolder.Get().GetMedicineInfos(medicine) as BaseElixirInfo;
            if (p == null || !GameState.Get().GetPanacea(p.PanaceaAmount))
            {
                MessageController.instance.ShowMessage(10);
                NotificationCenter.Instance.NotEnoughPanacea.Invoke(new BaseNotificationEventArgs());
                return false;
            }

            SoundsController.Instance.PlaySeedElixir();
            tableState = TableState.producing;
            producedElixir = medicine;
            productionTime = productionTimeLeft = p.ProductionTime;
            SetElixirSprite();
            anim.speed = 1f;
            anim.SetTrigger("Click");

            MedicineBadgeHintsController.Get().AddSingleMedInProduction(producedElixir, 1);
            //HintsController.Get().RemoveHint(new CollectHint(producedElixir));
            //   HintsController.Get().UpdateAllHintsWithMedicineCount();

            //var pos = Utils.ScreenToCanvasPosition(ReferenceHolder.Get().engine.MainCamera.GetCamera().WorldToScreenPoint(new Vector3(position.x, 0, position.y)));
            var pos = new Vector3(position.x, 0, position.y);
            ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, p.PanaceaAmount, 0f,
                ReferenceHolder.Get().giftSystem.particleSprites[3]);
            NotificationCenter.Instance.ProductionStarted.Invoke(new ProductionStartedEventArgs());

            SaveSynchronizer.Instance.MarkToSave(SavePriorities.ElixirSeeded);
            return true;
        }

        #endregion

        #region Setting Sprites

        public void SetBaseTableSprite()
        {
            if (isoObj == null)
                return;
            var go = isoObj.GetGameObject();
            var ch = go.transform.GetChild(0).GetChild(0);
            ch.GetComponent<SpriteRenderer>().sprite = defaultSprite;
        }

        private SpriteRenderer cachedSpriteRenderer = null;

        private SpriteRenderer GetCachedSpriteRenderer()
        {
            if (isoObj == null)
            {
                return null;
            }

            if (cachedSpriteRenderer == null)
            {
                cachedSpriteRenderer = isoObj.GetGameObject().transform.GetChild(0).GetChild(0)
                    .GetComponent<SpriteRenderer>();
            }

            return cachedSpriteRenderer;
        }

        private void ClearCachedSpriteRenderer()
        {
            cachedSpriteRenderer = null;
        }

        protected void SetElixirSprite()
        {
            var p = ResourcesHolder.Get()
                .GetTableSpriteForBaseElixir(producedElixir, 1 - productionTimeLeft / productionTime);
            if (isoObj != null)
            {
                SpriteRenderer sr = GetCachedSpriteRenderer();
                if (sr)
                {
                    if (sr.sprite != (p != null ? p : defaultSprite))
                    {
                        sr.sprite = p != null ? p : defaultSprite;
                    }
                }

                if (tableState == TableState.waitingForUser)
                {
                    ShowBubbles();
                }
            }
        }

        public override void RotateRight()
        {
            ClearCachedSpriteRenderer();
            base.RotateRight();
        }

        IEnumerator<float> SetReadyCoroutine()
        {
            while (!HospitalAreasMapController.HospitalMap.IsLoaded)
            {
                yield return 0f;
            }

            anim.SetTrigger("Ready");
        }

        /*
        IEnumerator BounceForCollectCoroutine()
        {
            //Debug.Log("BounceCoroutine");
            float bounceTime = .1f;
            float timer = 0f;
            yield return new WaitForSeconds(GameState.RandomFloat(0f, 1f));

            while (true)
                {
                    timer += Time.deltaTime;
                    gameObject.transform.GetChild(0).localPosition = new Vector3(positionStart.x, positionStart.y + Mathf.Sin(timer)/ bounceTime, positionStart.z);
                    Debug.Log(gameObject.transform.GetChild(0).localPosition.y);
                    yield return 0;
                }
        }

        void BounceObjectForCollect()
        {
            gameObject.transform.GetChild(0).localPosition = positionStart;

            if (collectBounce!=null)
            {
                StopCoroutine(collectBounce);
                collectBounce = null;
            }
            collectBounce = StartCoroutine(BounceForCollectCoroutine());
        }
        */
    }

    #endregion
}

public enum TableState
{
    empty,
    producing,
    waitingForUser,
}
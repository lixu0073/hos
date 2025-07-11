using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MovementEffects;

namespace Hospital
{
    public class PanaceaCollector : ProductionRotatable
    {
        TutorialController tc;
        Animator machineAnim;
        GameObject plusOne;
        public bool blockedByTutorial = true;

        public delegate void PanaceaAmountChanged();
        public static event PanaceaAmountChanged OnPanaceaAmountChanged;

        private int backFromLoadTime = 0;
        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;
        private bool firstBuilded = true;
        //private bool canUnlock = true;
        private Hospital.BalanceableFloat productionSpeedFactor;

        void Start()
        {
            tc = TutorialController.Instance;
            GameState.Get().PanaceaCollector = this;
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        protected override int GetMaxLevel()
        {
            return base.GetMaxLevel();
        }

        public int collectingSpeed { get { return GetCollectionRatePerLevel(); } }

        float startTime = -1;
        public void Reset(int amount, bool isForLevelUp = false)
        {
            StartCoroutine(FillPanaceaCollector(amount, isForLevelUp));
        }

        protected override void LoadFromString(string save, TimePassedObject timePassed, int actionsDone = 0)
        {
            if (GameState.Get().PanaceaCollector == null)
                GameState.Get().PanaceaCollector = this;

            base.LoadFromString(save, timePassed);

            var str = save.Split(';');

            if (str.Length > 1)
            {
                var p = str[1].Split('/');
                int lvl = int.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture);
                int amount = Mathf.Max(int.Parse(p[1], 0, System.Globalization.CultureInfo.InvariantCulture));

                float collectEverySecs = 3600f / collectingSpeed;
                backFromLoadTime = (int)(timePassed.GetTimePassed() % collectEverySecs);

                amount += Mathf.RoundToInt(timePassed.GetTimePassed() * collectingSpeed / 3600);

                if (amount > maximumAmount)
                    amount = maximumAmount;

                SetLevel(lvl);
                actualAmount = amount;
            }
            else
            {
                actualLvl = 1;
                actualAmount = 0;
            }
            SetIndicator();
        }

        IEnumerator FillPanaceaCollector(int targetAmount, bool isForLevelUp)
        {
            ShowPlusOneIcon(isForLevelUp);
            while (actualAmount < targetAmount)
            {
                ++actualAmount;
                SetIndicator();
                yield return new WaitForSeconds(.1f);
            }
            ShowPlusOneIcon(isForLevelUp);

            actualAmount = targetAmount;

            OnPanaceaAmountChanged?.Invoke();
        }

        public void SetPanaceaFull(bool forLvlUp = false)
        {
            if (forLvlUp)
                Reset(maximumAmount, true);
            else
            {
                actualAmount = maximumAmount;

                if (UIController.getHospital.PanaceaPopUp.gameObject.activeSelf)
                    UIController.getHospital.PanaceaPopUp.Refresh();
                startTime = Time.time;
                SetIndicator();
                SetAnimation();
                ShowPlusOneIcon();

                OnPanaceaAmountChanged?.Invoke();
            }
        }

        protected override void AddToMap()
        {
            base.AddToMap();
            if (ProperlySet)
                SetIndicator();

            if (isoObj != null)
            {
                var p = isoObj.GetGameObject();
                machineAnim = p.transform.GetChild(0).gameObject.GetComponent<Animator>();
                SetAnimation();
            }
            else
                Debug.Log("ISO Obje is null. Not starting animating");

            plusOne = ReferenceHolder.GetHospital().plusOnePanacea;
        }

        void SetAnimation()
        {
            if (this == null || machineAnim == null)
                return;

            if ((float)actualAmount / maximumAmount < 1 && !blockedByTutorial)
                machineAnim.SetBool("IsWorking", true);
            else
                machineAnim.SetBool("IsWorking", false);
        }

        protected override void RemoveFromMap()
        {
            base.RemoveFromMap();
        }

        public override void IsoUpdate()
        {
            if (collectingSpeed != 0 && (!blockedByTutorial && Time.time - (startTime - backFromLoadTime) > 3600 / collectingSpeed && actualAmount < maximumAmount))
            {
                backFromLoadTime = 0;
                actualAmount += 1;
                if (UIController.getHospital.PanaceaPopUp.gameObject.activeSelf)
                    UIController.getHospital.PanaceaPopUp.Refresh();

                startTime = Time.time;
                SetIndicator();
                SetAnimation();
                ShowPlusOneIcon();

                OnPanaceaAmountChanged?.Invoke();
            }
        }

        public override void EmulateTime(TimePassedObject time)
        {
            /*
            if (currentPatient != null)
            {
                var p = curationDuration - curationTime;
                if (p > time)
                {
                    curationTime += time;
                    time = 0;
                }
                else
                {
                    NotificationCenter.Instance.PatientCured.Invoke(new PatientCuredEventArgs(currentPatient, ((DoctorRoomInfo)info.infos).Tag));
                    StartCoroutine(CreateCollectableDoc(currentPatient));
                    currentPatient.Notify((int)StateNotifications.GoHome);
                    currentPatient = null;

                    time -= p;
                }
                while (cureAmount > 0 && time > 0)
                {
                    MoveNextPatientToRoom();
                    if (time > curationDuration)
                    {
                        StartCoroutine(CreateCollectableDoc(currentPatient));
                        currentPatient.Notify((int)StateNotifications.GoHome);
                        currentPatient = null;
                        time -= curationDuration;
                        curationTime = -1;

                    }
                    else
                    {
                        curationTime += time;
                        time = 0;
                    }
                }

            }
            */
        }

        void ShowPlusOneIcon(bool isForLvlUP = false)
        {
            plusOne.SetActive(false);
            plusOne.SetActive(true);
            plusOne.GetComponent<PanaceaCollectorIndicator>().Show(isForLvlUP);
            plusOne.transform.position = transform.position + new Vector3(0f, 2.5f, 0f);
            SoundsController.Instance.PlayPanaceaBubble();
        }

        private void ShowPopup()
        {
            UIController.getHospital.PanaceaPopUp.Open(this);
        }

        protected override void OnClickWorking()
        {
            if (UIController.get.drawer.IsVisible || UIController.get.FriendsDrawer.IsVisible)
            {
                Debug.Log("Click won't work because drawer is visibile");
                return;
            }

            if (!blockedByTutorial)
                ShowPopup();
        }

        public void Unblock()
        {
            blockedByTutorial = false;            
            if (Tools.Utils.ContainsParameter<AnimatorControllerParameter[]>(machineAnim.parameters, "Start"))
                machineAnim.SetTrigger("Start");
            var fp = (GameObject)Instantiate(ResourcesHolder.GetHospital().ParticlePanaceaStart, transform.position, Quaternion.Euler(0, 0, 0));
            fp.transform.localScale = Vector3.one;
            fp.SetActive(true);
            Reset((int)(maximumAmount * 0.9f));
            SetAnimation();
        }

        public ProductionMachineInfo GetCollectorData()
        {
            return (ProductionMachineInfo)info.infos;
        }

        public int GetActualLevel()
        {
            return actualLvl;
        }

        public int GetCapacityPerLevel(bool next = false)
        {
            return base.GetAmountOnLevel(next);
        }

        public int GetCollectionRatePerLevel(bool next = false)
        {
            float factor = GetProductionSpeedFactor().GetBalancedValue();
            return (int)(base.GetCollectionRateOnLevel(next) * factor);
        }

        public int GetMaxLVL()
        {
            // Panacea max level is one less than number of level settings because it starts from 0
            return base.GetMaxLevel() - 1;
        }

        public int GetCollectionRatePerHour(bool next = false)
        {
            return (int)(3600f / GetCollectionRateOnLevel(next));
        }

        public bool GetPanacea(int amount)
        {
            if (amount <= actualAmount)
            {
                actualAmount -= amount;
                SetIndicator();
                SetAnimation();

                OnPanaceaAmountChanged?.Invoke();

                return true;
            }
            return false;
        }

        void BounceCollectorObject()
        {
            //Timing.KillCoroutine(BounceCoroutine());
            Timing.RunCoroutine(BounceCoroutine());
        }

        private BalanceableFloat GetProductionSpeedFactor()
        {
            if (productionSpeedFactor == null)
                productionSpeedFactor = BalanceableFactory.CreatePanaceaCollectorFillRateBalanceable();

            return productionSpeedFactor;
        }

        IEnumerator<float> BounceCoroutine()
        {
            float bounceTime = .15f;
            float timer = 0f;
            Transform targetTransform = isoObj.GetGameObject().transform;
            if (firstBuilded)
            {
                firstBuilded = false;
                normalScale = Vector3.zero;
            }

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            targetScale = normalScale * 1.1f; //new Vector3(1.1f, 1.1f, 1.1f);

            //scale up
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
    }
}
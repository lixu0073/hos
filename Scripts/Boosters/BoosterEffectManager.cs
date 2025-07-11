using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    /// <summary>
    /// 增益效果管理器，负责管理和控制游戏中所有增益道具的视觉效果。
    /// 处理医生、医院房间、实验室和诊断站的增益效果激活、粒子特效控制等功能。
    /// </summary>
    public class BoosterEffectManager : MonoBehaviour
    {
        public List<MonoBehaviour> doctorObjects;        
        public List<MonoBehaviour> productionObjects;
        public List<MonoBehaviour> diagnosticsObjects;
        public List<HospitalBedController> hospitalRoomBoosters;

        [SerializeField] private bool doctorCoinBooster = false;
        [SerializeField] private bool doctorXPBooster = false;
        [SerializeField] private bool doctorCoinXPBooster = false;

        [SerializeField] private bool hospitalRoomCoinBooster = false;
        [SerializeField] private bool hospitalRoomXPBooster = false;
        [SerializeField] private bool hospitalRoomCoinXPBooster = false;

        [SerializeField] private bool happyHourEffect = false;

        [SerializeField] private bool labSpeedUpEffect = false;
        [SerializeField] private bool doctorSpeedUpEffect = false;
        [SerializeField] private bool diagnosticSpeedUpEffect = false;
        [SerializeField] private bool diagnosticCostEffect = false;
        [SerializeField] private bool diagnosticFreeEffect = false;
        [SerializeField] private float diagnosticCost = 1000.0f;

        [SerializeField] private bool expForDoctors = false;
        [SerializeField] private bool expForTreatmentRooms = false;
        [SerializeField] private bool coinsForDoctors = false;
        [SerializeField] private bool coinsForTreatmentRooms = false;
#pragma warning disable 0414
        [SerializeField] private BoosterType hospitalRoomBooster = BoosterType.Coin;
#pragma warning restore 0414
        private float effectTimeElapsed = 0.0f;
        private float effectUpdateRate = 1.0f;

        #region static
        private static BoosterEffectManager instance;

        public static BoosterEffectManager Instance
        {
            get
            {
                if (instance == null)
                    Debug.LogWarning("No instance of BoosterEffectManager was found on scene!");
                return instance;
            }
        }
        #endregion        

        private void Awake()
        {
            if (instance != null)
                Debug.LogWarning("There are possibly multiple instances of BoosterEffectManager on scene!");

            instance = this;

            doctorObjects = new List<MonoBehaviour>();
            productionObjects = new List<MonoBehaviour>();
            diagnosticsObjects = new List<MonoBehaviour>();
            hospitalRoomBoosters = new List<HospitalBedController>();
        }

        private void Update()
        {
            if(Game.Instance.gameState().GetHospitalLevel() >= 9)
            {
                effectTimeElapsed += Time.deltaTime;
                if (effectTimeElapsed >= effectUpdateRate)
                {
                    effectTimeElapsed = 0.0f;
                    CheckEffects();
                }
            }
        }

        private void CheckEffects()
        {   
            //activate booster effect if booster is active
            ActivateDoctorBoosters();
            ActivateHospitalRoomsBoosters();
            ActivateLabEffect("Booster_Happy_Hour_Minute", happyHourEffect);

            //activate event effect if effect is active
            CheckEventsStatus();
            ActivateLabEffect("Booster_SpeedUp_Wind", labSpeedUpEffect);
            ActivateDoctorEffect("Booster_SpeedUp_Doctor", doctorSpeedUpEffect);
            ActivateDiagnosticsEffect("Booster_SpeedUp", diagnosticSpeedUpEffect);
            ActivateDiagnosticsEffect("Booster_Positive", diagnosticCostEffect);
            ActivateDiagnosticsEffect("Booster_Free", diagnosticFreeEffect);
        }

        private void CheckEventsStatus()
        {
            //check for active events
            labSpeedUpEffect = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.medicineProductionTime_FACTOR);
            doctorSpeedUpEffect = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.doctorsWorkingTime_FACTOR);
            diagnosticSpeedUpEffect = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.diagnosticStationsTime_FACTOR);
            diagnosticCostEffect = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.costOfDiagnosis_FACTOR);
            diagnosticCost = StandardEventConfig.GetValueFromPartialEvent(StandardEventKeys.costOfDiagnosis_FACTOR, 1000.0f);
            diagnosticFreeEffect = (diagnosticCostEffect && (diagnosticCost < 0.001f));
            if(diagnosticFreeEffect) diagnosticCostEffect = false; //turn off cost effect if free effect is in effect

            //doctors and treatment rooms
            expForDoctors = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.expForDoctors_FACTOR);
            expForTreatmentRooms = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.expForTreatmentRooms_FACTOR);
            coinsForDoctors = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.coinsForDoctorRooms_FACTOR);
            coinsForTreatmentRooms = StandardEventConfig.IsPartialDataIsActiveInEvent(StandardEventKeys.coinsForTreatmentRooms_FACTOR);
        }

        //only for boosters and not events effects
        public void DeactivateAllBoosters()
        {
            //doctors
            ActivateDoctorBoosters(BoosterType.Coin, false);
            ActivateDoctorBoosters(BoosterType.Exp, false);
            ActivateDoctorBoosters(BoosterType.CoinAndExp, false);

            //hospital rooms
            ActivateHospitalRoomBoosters(BoosterType.Coin, false);
            ActivateHospitalRoomBoosters(BoosterType.Exp, false);
            ActivateHospitalRoomBoosters(BoosterType.CoinAndExp, false);

            //lab
            ActivateHappyHourBoosters(false);
        }

        public void ActivateHospitalRoomsBoosters()
        {
            ActivateHospitalRoomBoosters(BoosterType.Coin, hospitalRoomCoinBooster);
            ActivateHospitalRoomBoosters(BoosterType.Exp, hospitalRoomXPBooster);
            ActivateHospitalRoomBoosters(BoosterType.CoinAndExp, hospitalRoomCoinXPBooster);
        }

        public void ActivateHospitalRoomBoosters(BoosterType type, bool activate)
        {
            switch (type)
            {
                case BoosterType.Coin:
                    hospitalRoomCoinBooster = (activate || coinsForTreatmentRooms);
                    activate = hospitalRoomCoinBooster;
                    break;
                case BoosterType.Exp:
                    hospitalRoomXPBooster = (activate || expForTreatmentRooms);
                    activate = hospitalRoomXPBooster;
                    break;
                case BoosterType.CoinAndExp:
                    hospitalRoomCoinXPBooster = activate;
                    activate = hospitalRoomCoinXPBooster;
                    break;
                default:
                    break;
            }
            
            foreach (HospitalBedController hbc in hospitalRoomBoosters)
            {
                if (hbc != null) ActivateHospitalRoomBooster(hbc, type, activate);
            }
        }

        private void ActivateHospitalRoomBooster(HospitalBedController hbc, BoosterType type, bool activate)
        {
            //activate effect only on a proper 2 bed treatment room
            if (hbc == null || hbc.Beds.Count != 2) return;

            //deactivate order overrides OccupiedBed status
            bool bedATaken = (hbc.Beds[1].Bed.transform.GetChild(3).gameObject.activeSelf);
            bool bedBTaken = (hbc.Beds[0].Bed.transform.GetChild(3).gameObject.activeSelf);
            bool activateA = (activate) ? bedATaken : false;
            bool activateB = (activate) ? bedBTaken : false;

            switch (type)
            {
                case BoosterType.Coin:
                    ActivateHospitalBedroomsEffect(hbc, activateA, activateB, "Booster_DoubleCoins");
                    break;
                case BoosterType.Exp:
                    ActivateHospitalBedroomsEffect(hbc, activateA, activateB, "Booster_DoubleXP");
                    break;
                case BoosterType.CoinAndExp:
                    ActivateHospitalBedroomsEffect(hbc, activateA, activateB, "Booster_Premium_Coins_XP");
                    break;
                default:
                    break;
            }
        }

        private void ActivateHospitalBedroomsEffect(HospitalBedController hbc, bool activateA, bool activateB, string effectName)
        {
            Transform boosterWrapperA = hbc.transform.Find("Booster_Wrapper_A");
            Transform boosterWrapperB = hbc.transform.Find("Booster_Wrapper_B");

            if (boosterWrapperA == null || boosterWrapperB == null) return;

            Transform effectA = boosterWrapperA.Find(effectName);
            Transform effectB = boosterWrapperB.Find(effectName);

            if (effectA == null || effectB == null) return;

            ActivateParticleEmissionInChildren(effectA, activateA);
            ActivateParticleEmissionInChildren(effectB, activateB);
        }

        public void ActivateDoctorBoosters()
        {
            ActivateDoctorBoosters(BoosterType.Coin, doctorCoinBooster);
            ActivateDoctorBoosters(BoosterType.Exp, doctorXPBooster);
            ActivateDoctorBoosters(BoosterType.CoinAndExp, doctorCoinXPBooster);
        }

        public void ActivateDoctorBoosters(BoosterType type, bool activate)
        {
            switch (type)
            {
                case BoosterType.Coin:
                    doctorCoinBooster = (activate || coinsForDoctors);
                    ActivateDoctorEffect("Booster_DoubleCoins", doctorCoinBooster);
                    break;
                case BoosterType.Exp:
                    doctorXPBooster = (activate || expForDoctors);
                    ActivateDoctorEffect("Booster_DoubleXP", doctorXPBooster);
                    break;
                case BoosterType.CoinAndExp:
                    doctorCoinXPBooster = activate;
                    ActivateDoctorEffect("Booster_Premium_Coins_XP", doctorCoinXPBooster);
                    break;
                default:
                    break;
            }
        }

        public void ActivateHappyHourBoosters(bool activate)
        {
            happyHourEffect = activate;
            ActivateLabEffect("Booster_Happy_Hour_Minute", happyHourEffect);
        }
        
        private void ActivateDoctorEffect(string boosterName, bool activate)
        {
            ActivateEffect(doctorObjects, boosterName, activate, "Machine_Click");
        }

        private void ActivateLabEffect(string boosterName, bool activate)
        {
            ActivateEffect(productionObjects, boosterName, activate, "Idle");
        }

        private void ActivateDiagnosticsEffect(string boosterName, bool activate)
        {
            ActivateEffect(diagnosticsObjects, boosterName, activate, "Idle");
        }        

        private void ActivateEffect(List<MonoBehaviour> mastershipObjects, string effectName, bool activate, string idleAnimName)
        {
            bool activateEffect = activate;

            foreach (MonoBehaviour mb in mastershipObjects)
            {
                if(mb != null)
                {
                    Transform[] children = mb.GetComponentsInChildren<Transform>(true); //this returns children and children of children, unlike Find()
                    Transform booster = null;

                    //find effect/booster transform in children
                    foreach (Transform t in children)
                    {
                        if (t.name.Equals(effectName))
                        {
                            booster = t;
                            break;
                        }
                    }

                    //check if booster/effect should be activated, it should be activated only when in use
                    if (activate)
                    {
                        Animator anim = mb.GetComponentInChildren<Animator>();
                        if (anim != null)
                        {
                            activateEffect = !anim.GetCurrentAnimatorStateInfo(0).IsName(idleAnimName);
                        }
                    }

                    if (booster != null) ActivateParticleEmissionInChildren(booster, activateEffect);
                    else Debug.LogError(string.Format("Couldn't find booster {0} for {1}", effectName, mb.name));
                }
            }
        }

        private void ActivateParticleEmissionInChildren(Transform parent, bool activate)
        {
            ParticleSystem[] particleSystems = parent.GetComponentsInChildren<ParticleSystem>(true); //includes parent besides its children
            foreach (ParticleSystem ps in particleSystems)
            {
                var em = ps.emission;
                em.enabled = activate;
            }
        }
    }
}
using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using MovementEffects;
using I2.Loc;

namespace Hospital
{
    public class VIPPopUp : UIElement
    {
        public GameObject onTheWayView;
        public GameObject speedUpView;

        private int bedId;
        private int currentWaitTimer;
        private IEnumerator<float> updateWaitTimeCorountine;
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI vipNameInfoWauOut;
        [SerializeField] private TextMeshProUGUI vipWaitTimeInfo;
#pragma warning restore 0649
        [SerializeField] TextMeshProUGUI SpeedUpButtonText = null;

        [SerializeField] private RequiredCureController[] requiredCures = null;

        [SerializeField]
        [TermsPopup]
        private string unknownCureTooltipTerm = "-";
        [SerializeField]
        [TermsPopup]
        private string freeTerm = "-";

        [SerializeField] private GameObject currencyIcon = null;
        [SerializeField] private GameObject speedupButton = null;

        public enum Mode { Normal, Tutorial }
        Mode currentMode = Mode.Normal;

        [TutorialTriggerable]
        public void SetMode(Mode mode)
        {
            currentMode = mode;
            UpdateSpeedUpButton();
        }

        private void OnDisable()
        {
        }

        public void Open(HospitalCharacterInfo vipInfo, int bedId = -1, bool onWayIn = false)
        {
            gameObject.SetActive(true);
            StartCoroutine(OpenCoroutine(vipInfo, bedId, onWayIn));
        }

        private IEnumerator OpenCoroutine(HospitalCharacterInfo vipInfo, int bedId = -1, bool onWayIn = false)
        {
            this.bedId = bedId;

            yield return null; // CV: to force 1-frame delay before render TMPro

            if (vipInfo != null)
            {
                if (onWayIn)
                {
                    if (!vipInfo.Name.Contains("_"))
                        vipNameInfoWauOut.text = vipInfo.Name.ToUpper() + " " + vipInfo.Surname.ToUpper() + "\n" + I2.Loc.ScriptLocalization.Get("VIP_ON_THE_WAY_IN");
                    else
                        vipNameInfoWauOut.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + vipInfo.Name).ToUpper() + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + vipInfo.Surname).ToUpper() + "\n" + I2.Loc.ScriptLocalization.Get("VIP_ON_THE_WAY_IN");
                }
                else
                {
                    if (!vipInfo.Name.Contains("_"))
                        vipNameInfoWauOut.text = vipInfo.Name.ToUpper() + " " + vipInfo.Surname.ToUpper() + "\n" + I2.Loc.ScriptLocalization.Get("VIP_ON_THE_WAY_IN");
                    else
                        vipNameInfoWauOut.text = I2.Loc.ScriptLocalization.Get("PATIENT_NAME/" + vipInfo.Name).ToUpper() + " " + I2.Loc.ScriptLocalization.Get("PATIENT_SURNAME/" + vipInfo.Surname).ToUpper() + "\n" + I2.Loc.ScriptLocalization.Get("VIP_ON_THE_WAY_OUT");
                }

                SetLayout();
            }
            else if (onWayIn)
            {
                vipNameInfoWauOut.text = "VIP " + I2.Loc.ScriptLocalization.Get("VIP_ON_THE_WAY_IN");
                SetLayout();
            }
            else
            {
                NotificationCenter.Instance.VipSpeedupPopupOpened.Invoke(new BaseNotificationEventArgs());
                SetRequiredCures(GetRequiredCuresData());
                SetLayout(true);
            }

            if (updateWaitTimeCorountine != null)
            {
                Timing.KillCoroutine(updateWaitTimeCorountine);
                updateWaitTimeCorountine = null;
            }

            if (bedId != -1)
            {
                updateWaitTimeCorountine = Timing.RunCoroutine(updateTimeCorountine());
            }

            UpdateSpeedUpButton();

            yield return base.Open(true, true);

            if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.vip_speedup_2)
                TutorialUIController.Instance.BlinkImage(speedupButton.GetComponent<Image>(), 1.2f, true);
        }

        public void SetLayout(bool isSpeedUp = false)
        {
            if (isSpeedUp)
            {
                onTheWayView.gameObject.SetActive(false);
                speedUpView.gameObject.SetActive(true);
            }
            else
            {
                onTheWayView.gameObject.SetActive(true);
                speedUpView.gameObject.SetActive(false);
            }
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            if (updateWaitTimeCorountine != null)
            {
                Timing.KillCoroutine(updateWaitTimeCorountine);
                updateWaitTimeCorountine = null;
            }

            base.Exit(hidePopupWithShowMainUI);
        }

        public void ButtonExit()
        {
            if (currentMode == Mode.Tutorial)
                return;

            Exit();
        }

        public void SpeedButton()
        {
            if (currentMode == Mode.Tutorial)
            {
                HospitalAreasMapController.HospitalMap.hospitalBedController.SpeedBedWaitingForID(bedId);
                Exit();
                NotificationCenter.Instance.VipSpeedupPopupClosed.Invoke(new BaseNotificationEventArgs());
                return;
            }

            if (Game.Instance.gameState().GetDiamondAmount() >= HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().CurrentSpeedUpFee)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().CurrentSpeedUpFee, delegate
                {
                    HospitalAreasMapController.HospitalMap.hospitalBedController.SpeedBedWaitingForID(bedId);
                    GameState.Get().RemoveDiamonds(HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().CurrentSpeedUpFee, EconomySource.SpeedUpVIP);
                    AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.VIP.ToString(), (int)FunnelStepVip.VipSpeedUpSpawn, FunnelStepVip.VipSpeedUpSpawn.ToString());
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void SetRequiredCures(RequiredCureData[] data)
        {
            int length = Mathf.Min(data.Length, requiredCures.Length);

            for (int i = 0; i < requiredCures.Length; ++i)
            {
                requiredCures[i].gameObject.SetActive(false);
            }

            for (int i = 0; i < length; ++i)
            {
                requiredCures[i].gameObject.SetActive(true);
                requiredCures[i].Initialize(data[i]);
            }
        }

        private RequiredCureData[] GetRequiredCuresData()
        {
            VIPSystemManager vipSystemManager = ReferenceHolder.GetHospital().vipSystemManager;

            KeyValuePair<MedicineDatabaseEntry, int>[] requiredMedicines = vipSystemManager.GetRequiredMedicines();

            RequiredCureData[] data = new RequiredCureData[requiredMedicines.Length];

            int curesVisible = Mathf.Min(vipSystemManager.heliMastership.MasteryLevel, vipSystemManager.vipMastership.MasteryLevel);

            curesVisible = Mathf.Min(curesVisible, data.Length);

            for (int i = 0; i < curesVisible; ++i)
            {
                int index = i;
                data[i] = new RequiredCureData()
                {
                    strategy = new KnownRequiredCureViewStrategy(),
                    cureSprite = requiredMedicines[i].Key.image,
                    onPointerDown = () =>
                    {

                    },
                };
                if (requiredMedicines[i].Key.GetMedicineRef().type == MedicineType.BasePlant)
                {
                    data[i].onPointerDown = () =>
                    {
                        FloraTooltip.Open(requiredMedicines[index].Key.GetMedicineRef());
                    };
                }
                else
                {
                    data[i].onPointerDown = () =>
                    {
                        TextTooltip.Open(requiredMedicines[index].Key.GetMedicineRef());
                    };
                }
            }

            for (int i = curesVisible; i < data.Length; ++i)
            {
                int index = i;
                data[i] = new RequiredCureData()
                {
                    strategy = new UnknownRequiredCureViewStrategy(),
                    onPointerDown = () =>
                    {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get(unknownCureTooltipTerm));
                    },
                };
            }

            return data;
        }

        private void UpdateSpeedUpButton()
        {
            if (currentMode == Mode.Tutorial)
            {
                currencyIcon.SetActive(false);
                SpeedUpButtonText.text = I2.Loc.ScriptLocalization.Get(freeTerm);
            }
            else
            {
                currencyIcon.SetActive(true);
                SpeedUpButtonText.text = (HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VIPSystemManager>().CurrentSpeedUpFee.ToString());
            }
        }

        private void OnPointerDownOnVisible()
        {
        }

        private void OnPointerDownOnInvisible()
        {
        }

        IEnumerator<float> updateTimeCorountine()
        {
            while (true)
            {
                if (bedId != -1)
                {
                    currentWaitTimer = HospitalAreasMapController.HospitalMap.hospitalBedController.GetWaitTimerForBed(bedId);
                }
                vipWaitTimeInfo.text = UIController.GetFormattedShortTime(currentWaitTimer);
                if (currentWaitTimer <= 0)
                {
                    bedId = -1;
                    Exit();
                    NotificationCenter.Instance.VipSpeedupPopupClosed.Invoke(new BaseNotificationEventArgs());
                    yield break;
                }
                yield return Timing.WaitForSeconds(1);
            }
        }
    }
}
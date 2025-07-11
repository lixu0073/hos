using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using MovementEffects;
using System;
using System.Collections;

namespace Hospital
{
    public class CasesPopUpController : UIElement
    {
        public GameObject[] CaseImages = new GameObject[3];
        public GameObject[] Storages = new GameObject[2];
        public GameObject[] CollectCaseButtons = new GameObject[2];
        public GameObject[] CollDeliContainers = new GameObject[3];
        public GameObject[] GetNowButtons = new GameObject[3];
        public Animator[] ExtendedInfoAnim = new Animator[3];
        public GameObject[] Badges = new GameObject[3];
        public TextMeshProUGUI[] CaseCounterTexts = new TextMeshProUGUI[3];
        public Image[] CollectImage = new Image[2];

        [SerializeField] private GameObject Timer = null;
#pragma warning disable 0414
        [SerializeField] private GameObject DeliverCaseIButton = null;
        [SerializeField] private GameObject DeliverCaseIIButton = null;
        [SerializeField] private GameObject DeliverCaseIIIButton = null;
#pragma warning restore 0414
        [SerializeField] private Slider TimerSlider = null;

#pragma warning disable 0414
        [SerializeField] private Image CaseIBackground = null;
        [SerializeField] private Image CaseIIBackground = null;
        [SerializeField] private Image CaseIIIBackground = null;
#pragma warning restore 0414

        [SerializeField] private TextMeshProUGUI TimerText = null;
        [SerializeField] private TextMeshProUGUI BoostPriceText = null;
        [SerializeField] private TextMeshProUGUI CaseIIPriceText = null;
        [SerializeField] private TextMeshProUGUI CaseIIIPriceText = null;

        [SerializeField] private Sprite smallBox1 = null;
        [SerializeField] private Sprite smallBox2 = null;

        [SerializeField] private Sprite smallBox1Gray = null;
        [SerializeField] private Sprite smallBox2Gray = null;

        [SerializeField] private Color amountDefaultColor = Color.white;
        [SerializeField] private Color amountDeficientColor = Color.white;
#pragma warning disable 0414
        [SerializeField] private RectTransform exitButtonTransform = null;
#pragma warning restore 0414

        private int DiamondTransactionButtonStarter = -1;

        public void OpenCasesPopUp()
        {
            if (VisitingController.Instance.IsVisiting)
                return;

            gameObject.SetActive(true);
            StartCoroutine(Open());
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            yield return base.Open(isFadeIn, preservesHovers);
            RefreshPopUP();
            Timing.KillCoroutine(CheckTimer().GetType());
            Timing.RunCoroutine(CheckTimer());
            if (HospitalAreasMapController.HospitalMap.casesManager is HospitalCasesManager)
            {
                HospitalCasesManager hospitalcasesmanager = HospitalAreasMapController.HospitalMap.casesManager as HospitalCasesManager;
                bool[] canDeliver = hospitalcasesmanager.canDeliver;
                if (canDeliver[0] || canDeliver[1])
                    NotificationCenter.Instance.GiftReady.Invoke(new BaseNotificationEventArgs());
            }
            whenDone?.Invoke();
        }

        public void ButtonExit()
        {
            Exit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).CheckAlert();
            
            NotificationCenter.Instance.BoxPopupClosed.Invoke(new BaseNotificationEventArgs());

            base.Exit(hidePopupWithShowMainUI);
            Timing.KillCoroutine(CheckTimer().GetType());
        }
        public void RefreshPrices()
        {
            int[] getNowPrices = ((HospitalCasesManager)AreaMapController.Map.casesManager).getNowPrices;
            BoostPriceText.SetText(getNowPrices[0].ToString());
            CaseIIPriceText.SetText(getNowPrices[1].ToString());
            CaseIIIPriceText.SetText(getNowPrices[2].ToString());
        }
        public void RefreshPopUP(bool refreshStorage = true)
        {
            bool[] canDeliver = ((HospitalCasesManager)AreaMapController.Map.casesManager).canDeliver;
            bool[] canCollect = ((HospitalCasesManager)AreaMapController.Map.casesManager).canCollect;
            int[] amount = ((HospitalCasesManager)AreaMapController.Map.casesManager).casesStorage;

            int[] getNowPrices = ((HospitalCasesManager)AreaMapController.Map.casesManager).getNowPrices;

            #region first card
            CaseCounterTexts[0].gameObject.SetActive(false);

            if (canDeliver[0])
            {
                Badges[0].SetActive(true);

                Timer.SetActive(false);
                Timing.KillCoroutine(CheckTimer().GetType());

                CollDeliContainers[0].SetActive(true);
                GetNowButtons[0].SetActive(false);
                if (canCollect[0])
                    CollectImage[0].material = null;
                else
                    CollectImage[0].material = ResourcesHolder.Get().GrayscaleMaterial;
            }
            else
            {
                Badges[0].SetActive(false);

                Timer.SetActive(true);
                Timing.KillCoroutine(CheckTimer().GetType());
                Timing.RunCoroutine(CheckTimer());

                CollDeliContainers[0].SetActive(false);
                GetNowButtons[0].SetActive(true);
                BoostPriceText.SetText(getNowPrices[0].ToString());
            }
            #endregion

            #region second card
            CaseCounterTexts[1].SetText(amount[0].ToString() + "<size=20>/4</size>");
            if (canDeliver[1])
            {
                if (refreshStorage)
                {
                    Badges[1].SetActive(true);

                    Storages[0].SetActive(false);
                    GetNowButtons[1].SetActive(false);
                    CollDeliContainers[1].SetActive(true);
                    if (canCollect[1])
                        CollectImage[1].material = null;
                    else
                        CollectImage[1].material = ResourcesHolder.Get().GrayscaleMaterial;
                }
            }
            else
            {
                Badges[1].SetActive(false);

                Storages[0].SetActive(true);
                if (refreshStorage)
                {
                    RefreshStorage(0);
                }
                CollDeliContainers[1].SetActive(false);
                GetNowButtons[1].SetActive(true);
                CaseIIPriceText.SetText(getNowPrices[1].ToString());
            }
            #endregion

            #region third card
            CaseCounterTexts[2].SetText(amount[1].ToString() + "<size=20>/4</size>");
            if (canDeliver[2])
            {
                if (refreshStorage)
                {
                    Badges[2].SetActive(true);
                    Storages[1].SetActive(false);
                    CollDeliContainers[2].SetActive(true);
                    GetNowButtons[2].SetActive(false);
                }
            }
            else
            {
                Badges[2].SetActive(false);

                Storages[1].SetActive(true);
                //actualize storage
                if (refreshStorage)
                    RefreshStorage(1);

                CollDeliContainers[2].SetActive(false);
                GetNowButtons[2].SetActive(true);
                CaseIIIPriceText.SetText(getNowPrices[2].ToString());
            }
            #endregion
        }

        public void RefreshStorage(int storageID)
        {
            int[] amount = ((HospitalCasesManager)AreaMapController.Map.casesManager).casesStorage;

            Sprite colorBox;
            Sprite grayBox;
            if (storageID == 0)
            {
                colorBox = smallBox1;
                grayBox = smallBox1Gray;
            }
            else if (storageID == 1)
            {
                colorBox = smallBox2;
                grayBox = smallBox2Gray;
            }
            else
                return;

            for (int i = 0; i < amount[storageID]; ++i)
            {
                Storages[storageID].transform.GetChild(i).GetComponent<Image>().sprite = colorBox;
            }
            for (int i = amount[storageID]; i < 4; ++i)
            {
                Storages[storageID].transform.GetChild(i).GetComponent<Image>().sprite = grayBox;
            }
        }

        public void CollectButton(int caseID)
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).CollectButton(caseID);
        }

        public void DeliverButton(int caseID)
        {
            ((HospitalCasesManager)AreaMapController.Map.casesManager).DeliverButton(caseID);
        }

        public void OnInfoButtonDown(int infoID)
        {
            ExtendedInfoAnim[infoID].SetBool("Show", true);
        }
        public void OnInfoButtonUP(int infoID)
        {
            ExtendedInfoAnim[infoID].SetBool("Show", false);
        }

        public void GetNowButton(int buttonID)
        {
            if (DiamondTransactionButtonStarter != buttonID)
            {
                InitializeID();
                DiamondTransactionButtonStarter = buttonID;
            }
            ((HospitalCasesManager)AreaMapController.Map.casesManager).GetNowButton(buttonID, this);
        }

        private string SecToTime(int sec)
        {
            string time = "";
            int hours = sec / 3600;
            if (hours > 0)
                time += hours + "h ";
            int minutes = (sec % 3600) / 60;
            if (minutes > 0 || hours > 0)
                time += minutes + "m ";
            int seconds = sec - hours * 3600 - minutes * 60;
            if (seconds > 0 || minutes > 0 || hours > 0)
                time += seconds + "s";
            return time;
        }

        private void ForceCasesIdle()
        {
            for (int i = 0; i < 3; ++i)
            {
                CaseImages[i].GetComponent<Animator>().SetTrigger("ForceIdle");
            }
        }

        IEnumerator<float> CheckTimer()
        {
            int totalTime = ((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryIntervalSeconds;
            int timeLeft;
            for (; ; )
            {
                timeLeft = ((HospitalCasesManager)AreaMapController.Map.casesManager).deliveryIntervalSeconds - (Convert.ToInt32((long)ServerTime.getTime()) - ((HospitalCasesManager)AreaMapController.Map.casesManager).countingStartTime);
                if (!((HospitalCasesManager)AreaMapController.Map.casesManager).canDeliver[0])
                {
                    TimerText.text = SecToTime(timeLeft);
                    TimerSlider.value = (totalTime - timeLeft) / (float)totalTime;
                    yield return Timing.WaitForSeconds(1);
                }
                else
                {
                    RefreshPopUP();
                    yield break;
                }
            }
        }

    }
}
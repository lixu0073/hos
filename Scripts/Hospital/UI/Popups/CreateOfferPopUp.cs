using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Globalization;
using SimpleUI;
using TMPro;
using MovementEffects;

namespace Hospital
{
    public class CreateOfferPopUp : UIElement
    {
        #region Fields

        #region UIReferences
#pragma warning disable 0649
        [SerializeField] private GameObject addSaleItemPrefab;
        [SerializeField] private Transform content;
        [SerializeField] RectTransform scrollContentRect = null;

        [SerializeField] private TextMeshProUGUI storageText;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI amount;
        [SerializeField] private TextMeshProUGUI gold;
        [SerializeField] private TextMeshProUGUI counter;
        [SerializeField] private TextMeshProUGUI notUsedCounter;
        [SerializeField] GameObject scrollBar = null;
        
        [SerializeField] private Button putOnSale;
        [SerializeField] private Button speedUpAd;
        [SerializeField] private Toggle advertise;
#pragma warning restore 0649
        [SerializeField] private Slider storageBar = null;        
        #endregion

        private bool advertAvailable;
        private Vector3 normalScale = Vector3.zero;
        private Vector3 targetScale = Vector3.zero;
        //private int beforeIndex = -1;

        int localAmount = 0;
        int localGold = 0;
        OfferItem localItem;
        int maxAmount = 0;
        GameObject offerToModify;

        float advertisementTimer = 0;

        float buttonDownTimer = 0;
        int buttonDownIterations = 0;
        bool plusAmountDown;
        bool minusAmountDown;
        bool plusCostDown;
        bool minusCostDown;
        int actualTab = 0;
        int tabIndex = 0;
        int itemIndex = 0;
        bool isElixirTank = false;
        #endregion

        public int PromotionDurationInHours;

        [SerializeField] private List<StorageTab> tabs = null;

        [SerializeField] private TextMeshProUGUI Title = null;

        private class OfferItem
        {
            public MedicineRef Medicine;
            public int Amount;
            public OfferItem(MedicineRef m, int a)
            {
                this.Medicine = m;
                this.Amount = a;
            }
        }

        public IEnumerator Open(GameObject offer, int index, bool isElixirTank = false)
        {
            this.isElixirTank = isElixirTank;
            this.itemIndex = index;

            if (!isElixirTank)
                tabIndex = 0;
            else
                tabIndex = 1;

            PharmacyManager.Instance.PauseUserOffersRefresh();
            advertAvailable = ReferenceHolder.GetHospital().Pharmacy.IsFreeAdAvaiable;

            yield return null;

            SetAdvertAvailable(advertAvailable);

            ReferenceHolder.GetHospital().Pharmacy.OnFreeAdStateChanged += OnAdvertStateChange;

            advertise.isOn = true;
            advertise.enabled = true;

            offerToModify = offer;

            ActualiseIndicator();
            ResetContent();

            int storageResourcesCounter = 0;
            MedicineRef firstMedInStorage = null;
            int firstMedInStorageVal = -1;

            foreach (var p in GameState.Get().EnumerateResourcesMedRef())
            {
                if (p.Key.IsMedicineForTankElixir() == isElixirTank)
                {
                    if (firstMedInStorage == null)
                    {
                        firstMedInStorage = p.Key;
                        firstMedInStorageVal = p.Value;
                    }

                    var temp = Instantiate(addSaleItemPrefab);
                    var tempTransform = temp.transform;

                    tempTransform.GetChild(0).GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(p.Key);
                    tempTransform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = p.Value.ToString();
                    OfferItem indic = new OfferItem(p.Key, p.Value);
                    temp.GetComponent<Button>().onClick.AddListener(delegate ()
                    {
                        SelectItem(indic, index);
                    });
                    tempTransform.SetParent(content);
                    tempTransform.localScale = new Vector3(1, 1, 1);

                    temp.SetActive(true);
                    storageResourcesCounter++;
                }
            }

            LayoutRebuilder.MarkLayoutForRebuild(content.gameObject.GetComponent<RectTransform>());

            UpdateTabVisible();
            yield return base.Open();

            int rowCount = Mathf.CeilToInt(storageResourcesCounter / 3f);

            if (storageResourcesCounter <= 3)
                scrollContentRect.sizeDelta = new Vector2(272f, 85f);
            else scrollContentRect.sizeDelta = new Vector2(272f, 85f * rowCount);

            OfferItem selectFirst = null;
            if (firstMedInStorage != null)
            {
                if (ResourcesHolder.Get().GetLvlForCure(firstMedInStorage) <= Game.Instance.gameState().GetHospitalLevel())
                    selectFirst = new OfferItem(firstMedInStorage, firstMedInStorageVal);
            }

            if (selectFirst != null)
                SelectItem(selectFirst, index);

            if (isElixirTank)
                Title.SetText(I2.Loc.ScriptLocalization.Get("CREATE_OFFER_TITLE_TANK"));
            else            
                Title.SetText(I2.Loc.ScriptLocalization.Get("CREATE_OFFER_TITLE"));

            advertisementTimer = 1;
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            Timing.KillCoroutine(BounceCoroutineForSelectedIcon().GetType());

            ReferenceHolder.GetHospital().Pharmacy.OnFreeAdStateChanged -= OnAdvertStateChange;
            base.Exit(hidePopupWithShowMainUI);
            UIController.getHospital.PharmacyPopUp.transform.SetAsLastSibling();
            PharmacyManager.Instance.ResumeUserOffersRefresh();
        }

        /// <summary>
        /// Sets UI elements to match advert avaiability state
        /// </summary>
        /// <param name="state"></param>
        private void SetAdvertAvailable(bool state)
        {
            counter.enabled = !state;
            //counterHeader.enabled = !state;
            notUsedCounter.enabled = state;
            speedUpAd.gameObject.SetActive(!state);
            advertise.gameObject.SetActive(state);
        }

        /// <summary>
        /// invoked when pharmacy advert changes state(avaiable/unavaiable).
        /// </summary>
        /// <param name="avaiable"></param>
        private void OnAdvertStateChange(bool available)
        {
            print("get event on advert state changed");
            advertAvailable = available;
            SetAdvertAvailable(available);
        }

        public void ToggleAdvert(bool state)
        {
           // var p = advertise.colors;
           // p.normalColor = p.highlightedColor = advertise.isOn ? Color.green : Color.white;
           // advertise.colors = p;
        }

        void SelectItem(OfferItem item, int index)
        {
            itemName.text = ResourcesHolder.Get().GetNameForCure(item.Medicine).ToUpper();
            itemIcon.sprite = ResourcesHolder.Get().GetSpriteForCure(item.Medicine);
            itemIcon.GetComponent<PointerDownListener>().SetDelegate(() =>
            {
                TextTooltip.Open(item.Medicine);
            });

            localItem = item;
            localGold = ResourcesHolder.Get().GetDefaultPriceForCure(item.Medicine);
            if (item.Amount == 1)
            {
                amount.text = "1x";
                localAmount = 1;
            }
            else
            {
                localAmount = Mathf.Clamp(item.Amount / 2, 1, 10);
                amount.text = localAmount + "x";
            }
            maxAmount = Mathf.Clamp(item.Amount, 1, 10);
            localGold *= localAmount;
            gold.text = localGold.ToString();
            
            putOnSale.onClick.RemoveAllListeners();
            putOnSale.onClick.AddListener(delegate ()
            {
                CreateSale(item, index);
            });

            BounceIcon();
        }

        private void CreateSale(OfferItem item, int index)
        {
            PharmacyOrder order;
            Debug.LogWarning("advertise.isOn = " + advertise.isOn);
            if (advertise.isOn && ReferenceHolder.GetHospital().Pharmacy.IsFreeAdAvaiable)
            {
                if (!ReferenceHolder.GetHospital().Pharmacy.UseAdvert())
                {
                //    MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("ERROR_INFO"));
                //    return;
                }
                order = new PharmacyOrderAdvertised((long)ServerTime.getTime() + 60*60*PromotionDurationInHours);
                order.requiredLevel = ResourcesHolder.Get().GetLvlForCure(item.Medicine);
				AchievementNotificationCenter.Instance.AdPlaced.Invoke(new AchievementProgressEventArgs(1));
            }
            else
            {
                order = new PharmacyOrderStandard((long)ServerTime.getTime() + 60*60*PromotionDurationInHours);
                order.requiredLevel = ResourcesHolder.Get().GetLvlForCure(item.Medicine);
            }
            //Debug.LogError(PromotionDurationInHours);
            order.amount = localAmount;
            order.medicine = item.Medicine;
			order.pricePerUnit = localGold;
            order.runSpawnAnim = true;
            order.runDescentAnim = true;
            order.UserID = CognitoEntry.SaveID;
            order.ID = ServerTime.Get().GetServerTime().ToString(CultureInfo.InvariantCulture);
            order.sortOrder = index;
            Guid guid = Guid.NewGuid();
            order.UUID = guid.ToString();
            
            UIController.getHospital.PharmacyPopUp.AddToOffersLists(offerToModify, order);
            Exit();
        }

        public static long UnixTime(DateTime date, int addOffset = 0)
        {
            var timeSpan = (date - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds + addOffset;
        }

        public void SpeedUp()
		{
            if (Game.Instance.gameState().GetDiamondAmount() >= 1)
            {
                ReferenceHolder.GetHospital().Pharmacy.SpeedUpAdvert(this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
		}

        public void UpdateTabVisible()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                tabs[i].GetComponent<RectTransform>().SetAsFirstSibling();
            }

            tabs[tabIndex].GetComponent<RectTransform>().SetSiblingIndex(2);

            if (tabIndex >= 0 && tabIndex < tabs.Count)
            {
                if (actualTab != -1)
                {
                    tabs[actualTab].ChangeTabButton(false);
                }
                else
                {
                    for (int i = 0; i < tabs.Count; i++)
                        tabs[i].ChangeTabButton(false);
                }

                tabs[tabIndex].ChangeTabButton(true);
                actualTab = tabIndex;
            }
        }

        public void SetTabVisible(int index)
        {
            this.tabIndex = index;
            
            StartCoroutine(Open(offerToModify, itemIndex, (index == 0) ? false : true));
        }

        public int GetActualTab()
        {
            return actualTab;
        }

        public void ResetContent()
        {
            if (content.childCount > 0)
            {
                for (int i = 0; i < content.childCount; i++)
                    Destroy(content.GetChild(i).gameObject);
            }
        }

        void Update()
        {
            CheckAdvertismentChange();
            CheckQuickPlusMinus();
        }

        void CheckAdvertismentChange()
        {
            advertisementTimer += Time.deltaTime;
            if (!advertAvailable && advertise.isOn && advertisementTimer > .5f)
            {
                counter.text = I2.Loc.ScriptLocalization.Get("CREATE_OFFER_ADVERTISE") + "\n" + UIController.GetFormattedTime((int)ReferenceHolder.GetHospital().Pharmacy.TimeTillFreeAd);
                advertisementTimer = 0;
            }
           // else if (advertAvaiable && advertisementTimer <= .5f)
           //     counter.text = "ADVERTISE IN 3H";
        }

        public void ActualiseIndicator()
        {
            if (isElixirTank)
            {
                storageBar.value = GameState.Get().elixirTankAmount / (float)GameState.Get().maximimElixirTankAmount;
                storageText.text = GameState.Get().elixirTankAmount.ToString() + " /" + GameState.Get().maximimElixirTankAmount.ToString();
            }
            else
            {
                storageBar.value = GameState.Get().elixirStorageAmount / (float)GameState.Get().maximimElixirStorageAmount;
                storageText.text = GameState.Get().elixirStorageAmount.ToString() + " /" + GameState.Get().maximimElixirStorageAmount.ToString();
            }
        }

        void CheckQuickPlusMinus()
        {
            if (!minusAmountDown && !plusAmountDown && !minusCostDown && !plusCostDown)
                return;

            buttonDownTimer += Time.deltaTime;
            if (buttonDownTimer >= .5f)
            {
                if (minusAmountDown)
                {
                    DecreaseAmount();
                    buttonDownTimer -= .15f;
                }
                else if (plusAmountDown)
                {
                    IncreaseAmount();
                    buttonDownTimer -= .15f;
                }
                else if (minusCostDown)
                {
                    DecreaseCost();
                    buttonDownIterations++;
                    if (buttonDownIterations < 5)
                        buttonDownTimer -= .1f;
                    else if (buttonDownIterations > 50)
                    {
                        DecreaseCost();
                        DecreaseCost();
                    }
                }
                else if (plusCostDown)
                {
                    IncreaseCost();
                    buttonDownIterations++;
                    if (buttonDownIterations < 5)
                        buttonDownTimer -= .1f;
                    else if (buttonDownIterations > 50)
                    {
                        IncreaseCost();
                        IncreaseCost();
                    }
                }
            }
        }

        void IncreaseAmount()
        {
            if ((localAmount + 1) <= maxAmount)
            {
                localAmount += 1;
                amount.text = localAmount + "x";
                AdjustCost(localAmount-1);
            }
        }

        void DecreaseAmount()
        {
            if (localAmount - 1 > 0)
            {
                localAmount -= 1;
                amount.text = localAmount + "x";
                AdjustCost(localAmount+1);
            }
        }

        void AdjustCost(int previousAmount)
        {
            int adjustedGold = (int)((float)localGold / (float)previousAmount * (float)localAmount);
			localGold = Mathf.Clamp(adjustedGold, 1, ResourcesHolder.Get().GetMaxPriceForCure(localItem.Medicine) * localAmount);
            gold.text = localGold.ToString();
        }

        void IncreaseCost()
        {
            if ((localGold + 1) <= ResourcesHolder.Get().GetMaxPriceForCure(localItem.Medicine) * localAmount)
            {
                localGold += 1;
                gold.text = localGold.ToString();
            }
        }

        void DecreaseCost()
        {
			if (localGold - 1 >= 0 && (localGold - 1) >= ResourcesHolder.Get().GetMinPriceForCure(localItem.Medicine))
            {
                localGold -= 1;
                gold.text = localGold.ToString();
            }
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void ButtonPlusAmount()
        {
            //IncreaseAmount();
        }

        public void ButtonMinusAmount()
        {
            //DecreaseAmount();
        }

        public void ButtonPlusCost()
        {
            //IncreaseCost();
        }

        public void ButtonMinusCost()
        {
            //DecreaseCost();
        }

        public void PlusAmountDown()
        {
            plusAmountDown = true;
            buttonDownTimer = 0;
            IncreaseAmount();
        }

        public void PlusAmountUp()
        {
            plusAmountDown = false;
        }

        public void MinusAmountDown()
        {
            minusAmountDown = true;
            buttonDownTimer = 0;
            DecreaseAmount();
        }

        public void MinusAmountUp()
        {
            minusAmountDown = false;
        }

        public void PlusCostDown()
        {
            plusCostDown = true;
            buttonDownTimer = 0;
            buttonDownIterations = 0;
            IncreaseCost();
        }

        public void PlusCostUp()
        {
            plusCostDown = false;
        }

        public void MinusCostDown()
        {
            minusCostDown = true;
            buttonDownTimer = 0;
            buttonDownIterations = 0;
            DecreaseCost();
        }

        public void MinusCostUp()
        {
            minusCostDown = false;
        }

        public void ShowScrollbar()
        {
            scrollBar.SetActive(true);
            Timing.KillCoroutine(HideScrollbar().GetType());
            Timing.RunCoroutine(HideScrollbar());
        }

        IEnumerator<float> HideScrollbar()
        {
            yield return Timing.WaitForSeconds(0.5f);
            scrollBar.SetActive(false);
        }

        void BounceIcon()
        {
            Timing.RunCoroutine(BounceCoroutineForSelectedIcon());
        }

        IEnumerator<float> BounceCoroutineForSelectedIcon()
        {
            float bounceTime = .1f;
            float timer = 0f;
            RectTransform targetTransform = itemIcon.gameObject.GetComponent<RectTransform>();

            if (normalScale == Vector3.zero)
                normalScale = targetTransform.localScale;

            if (targetScale == Vector3.zero)
                targetScale = targetTransform.localScale * 1.1f;

            //scale up
            if (normalScale != Vector3.zero && normalScale != Vector3.zero)
            {
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

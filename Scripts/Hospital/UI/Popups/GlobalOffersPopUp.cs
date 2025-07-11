using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SimpleUI;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{
    public class GlobalOffersPopUp : UIElement
    {
#pragma warning disable 0649
        [SerializeField]
        private ScrollRect scroll;
        [SerializeField]
        private ScrollRect scrollSmall;
#pragma warning restore 0649
        public Transform content;
        public Transform contentSmall;
        public GameObject GlobalPlayerOffer;
        public GameObject EmptyOffer;
        public GameObject AddSlotOffer;

        [SerializeField]
        private GameObject DailyDealOffer = null;
        public ScrollRect ScrollPanel;
#pragma warning disable 0649
        [SerializeField]
        private TextMeshProUGUI counter;
#pragma warning restore 0649
        public GameObject RefreshButton;
        public GameObject FreeRefrshButton;
        public GameObject FreeRefrshText;
        public GameObject Spinner;
        public Toggle friendsOffersToggle;
        public Animator friendsOffersAnim;
        public RectTransform refreshTimer;

        string visitingHospitalName;
        private bool phoneUI = false;

        private class Offer
        {
            public PharmacyOrderAdvertised Order;
            public GameObject Card;
            public Offer(PharmacyOrderAdvertised o, GameObject c)
            {
                this.Order = o;
                this.Card = c;
            }
        }

        public IEnumerator Open(string visitingHospitalName = null)
        {
            if (ExtendedCanvasScaler.isPhone() || ExtendedCanvasScaler.HasNotch())
            {
                scroll.gameObject.SetActive(false);
                scrollSmall.gameObject.SetActive(true);
                scrollSmall.horizontalNormalizedPosition = 0;
                phoneUI = true;
            }
            else
            {
                phoneUI = false;
                scroll.gameObject.SetActive(true);
                scroll.horizontalNormalizedPosition = 0;
                scrollSmall.gameObject.SetActive(false);
            }

            UIController.getHospital.PharmacyPopUp.Exit(false);
            yield return base.Open(false, false);
            this.visitingHospitalName = visitingHospitalName;

            AccountManager.OnFacebookStateUpdate += AccountManager_OnFacebookStateUpdate;

            FreeRefrshButton.SetActive(false);
            FreeRefrshText.SetActive(true);
            RefreshButton.SetActive(false);

            friendsOffersToggle.isOn = false;
            friendsOffersToggle.onValueChanged.RemoveAllListeners();
            friendsOffersToggle.onValueChanged.AddListener(ChangeFriendsOffersToggle);

            GetGlobalOffers();
            NotificationCenter.Instance.PharmacyOffersClicked.Invoke(new PharmacyOffersClickedEventArgs());

            UIController.get.CountersToFront();
            if (VisitingController.Instance.IsVisiting)
                UIController.get.SetCounters(true);
        }

        public void ChangeFriendsOffersToggle(bool value)
        {
            if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
            {
                RefreshOffers(ReferenceHolder.GetHospital().Pharmacy.orders, false);
            }

            if (value)
            {
                try
                { 
                    friendsOffersAnim.Play("Bump", 0, 0.0f);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Animator - exception: " + e.Message);
                }
            }
        }

        private void GetGlobalOffers()
        {
            Pharmacy pharmacy = ReferenceHolder.GetHospital().Pharmacy;
            if (!AccountManager.HasInternetConnection())
            {
                Exit();
                BaseUIController.ShowInternetConnectionProblemPopup(this);
                return;
            }
            ClearOffers();
            SetLoading(true);
            pharmacy.GetActualGlobalOffers((loadFromServer) =>
            {
                SetLoading(false);
                if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
                {
                    RefreshOffers(pharmacy.orders, loadFromServer);
                }
            }, (ex) =>
            {
                SetLoading(false);
                Exit();
                BaseUIController.ShowServerOrInternetConnectionProblem(ex);
            });
        }

        void SetLoading(bool isLoading)
        {
            Spinner.SetActive(isLoading);
            friendsOffersToggle.interactable = !isLoading;
            RefreshButton.GetComponent<Button>().interactable = !isLoading;
            FreeRefrshButton.GetComponent<Button>().interactable = !isLoading;
        }

        private void AccountManager_OnFacebookStateUpdate()
        {
            if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
            {
                RefreshOffers(ReferenceHolder.GetHospital().Pharmacy.orders, false);
            }
        }

        public void Exit()
        {
            base.Exit();
            OnExit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            OnExit();
        }

        private void OnExit()
        {
            friendsOffersToggle.onValueChanged.RemoveAllListeners();
            AccountManager.OnFacebookStateUpdate -= AccountManager_OnFacebookStateUpdate;
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;

            UIController.get.CountersToBack();
            if (VisitingController.Instance.IsVisiting)
                UIController.get.SetCounters(false);
        }

        public void FreeRefresh()
        {
            if (ReferenceHolder.GetHospital().Pharmacy.CanRefreshGlobalOffers())
            {
                SetLoading(true);
                ClearOffers();

                if (DailyQuestSynchronizer.Instance.WeekCounter == 1)
                {
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.BuyGoods));
                }

                ReferenceHolder.GetHospital().Pharmacy.TryRefreshGlobalOffers((loadFromServer) =>
                {
                    if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
                    {
                        Debug.Log("Global offers: " + ReferenceHolder.GetHospital().Pharmacy.orders.Count);
                        RefreshOffers(ReferenceHolder.GetHospital().Pharmacy.orders);
                        SetLoading(false);
                    }
                }, (ex) =>
                {
                    SetLoading(false);
                });
                timer += 1;
            }
        }

        public void SpeedUpAndRefresh()
        {
            if (ReferenceHolder.GetHospital().Pharmacy.CanSpedUpGlobalOffers())
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(ReferenceHolder.GetHospital().Pharmacy.SpeedUpCost, delegate
                {
                    ClearOffers();
                    SetLoading(true);

                    if (DailyQuestSynchronizer.Instance.WeekCounter == 1)
                    {
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.BuyGoods));
                    }

                    ReferenceHolder.GetHospital().Pharmacy.BoostPageRefresh((loadFromServer) =>
                    {
                        if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
                        {
                            Debug.Log("Global offers: " + ReferenceHolder.GetHospital().Pharmacy.orders.Count);
                            SetLoading(false);
                            RefreshOffers(ReferenceHolder.GetHospital().Pharmacy.orders);
                        }
                    }, (ex) =>
                    {
                        SetLoading(false);
                    });

                    timer += 1;
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        private void ClearOffers()
        {
            if (content != null)
            {
                foreach (Transform child in content)
                {
                    Destroy(child.gameObject);
                }
            }

            if (contentSmall != null)
            {
                foreach (Transform child in contentSmall)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void RefreshOffersVisible()
        {
            if (ReferenceHolder.GetHospital().Pharmacy.orders != null)
                RefreshOffers(ReferenceHolder.GetHospital().Pharmacy.orders);
        }

        private void RefreshOffers(List<PharmacyOrderAdvertised> offers, bool loadFromServer = true)
        {
            ClearOffers();
            DailyDealSaveData dailyDealSaveData = ReferenceHolder.GetHospital().dailyDealController.GetSaveData();

            bool isFriendsMode = friendsOffersToggle.isOn;
            int slotsCount = GameState.Get().GetGlobalOffersSlotsCount();
            int maxSlotsCount = DefaultConfigurationProvider.GetConfigCData().GlobalOffersMaxSlotsCount;
            if (isFriendsMode)
            {
                slotsCount = GameState.Get().GetFriendsOffersSlotsCount();
                maxSlotsCount = DefaultConfigurationProvider.GetConfigCData().FriendsOffersMaxSlotsCount;
            }
            int currentSlotIndex = 0;
            for (int i = 0; i < offers.Count; ++i)
            {
                var offer = offers[i];
                if (offer.medicine == null)
                    continue;
                if (!((offer.isFriendOffer && isFriendsMode) || (!offer.isFriendOffer && !isFriendsMode)))
                {
                    continue;
                }
                if (!isFriendsMode && currentSlotIndex == dailyDealSaveData.OcuupyIndex && dailyDealSaveData.CurrentDailyDeal != null)
                {
                    AddDailyDealOffer();
                }
                else
                {
                    AddSingleOffer(offer, loadFromServer);
                }
                ++currentSlotIndex;
                if (currentSlotIndex >= slotsCount)
                {
                    break;
                }
            }

            if (!isFriendsMode && currentSlotIndex - 1 < dailyDealSaveData.OcuupyIndex && dailyDealSaveData.CurrentDailyDeal != null)
            {
                AddDailyDealOffer();
                ++currentSlotIndex;
            }

            for (int i = currentSlotIndex; i < slotsCount; ++i)
            {
                CreateEmptyCard();
                ++currentSlotIndex;
            }

            if (currentSlotIndex < maxSlotsCount)
            {
                CreateAddSlotsButton(!isFriendsMode, currentSlotIndex);
            }
        }

        private void CreateEmptyCard()
        {
            GameObject emptyCard = Instantiate(EmptyOffer) as GameObject;

            if (phoneUI)
                emptyCard.transform.SetParent(contentSmall);
            else
                emptyCard.transform.SetParent(content);
            emptyCard.transform.localScale = Vector3.one;
        }

        private void CreateAddSlotsButton(bool fromGlobalOffers, int index)
        {
            PharmacyGlobalOfferAddSlot addSlot = Instantiate(AddSlotOffer).GetComponent<PharmacyGlobalOfferAddSlot>();
            addSlot.SetPrice(AlgorithmHolder.GetCostForNewGlobalSlot(index, fromGlobalOffers ? DefaultConfigurationProvider.GetConfigCData().GlobalOffersInitSlotsCount : DefaultConfigurationProvider.GetConfigCData().FriendsOffersInitSlotsCount));
            addSlot.button.onClick.AddListener(delegate ()
            {
                if (fromGlobalOffers)
                    TryToAddNewEmptySlotInGlobalOffers(index);
                else
                    TryToAddNewEmptySlotInFriendsOffers(index);
            });

            if (phoneUI)
                addSlot.transform.SetParent(contentSmall);
            else
                addSlot.transform.SetParent(content);
            addSlot.transform.localScale = Vector3.one;
        }

        public void TryToAddNewEmptySlotInGlobalOffers(int index)
        {
            int diamondCost = AlgorithmHolder.GetCostForNewGlobalSlot(index, DefaultConfigurationProvider.GetConfigCData().GlobalOffersInitSlotsCount);
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondCost, delegate
                {
                    GameState.Get().RemoveDiamonds(diamondCost, EconomySource.PharmacyExtendSlotsGlobalOffers);
                    AddNewEmptySlotInGlobalOffers();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void TryToAddNewEmptySlotInFriendsOffers(int index)
        {
            int diamondCost = AlgorithmHolder.GetCostForNewGlobalSlot(index, DefaultConfigurationProvider.GetConfigCData().FriendsOffersInitSlotsCount);
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondCost, delegate
                {
                    GameState.Get().RemoveDiamonds(diamondCost, EconomySource.PharmacyExtendSlotsFriendsOffers);
                    AddNewEmptySlotInFriendsOffers();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void AddNewEmptySlotInGlobalOffers()
        {
            GameState.Get().GlobalOffersSlotsCount++;
            RefreshOffersVisible();
        }

        public void AddNewEmptySlotInFriendsOffers()
        {
            GameState.Get().FriendsOffersSlotsCount++;
            RefreshOffersVisible();
        }

        private void AddDailyDealOffer()
        {
            DailyDealOfferUI dailyDealOffer = Instantiate(DailyDealOffer).GetComponent<DailyDealOfferUI>();
            dailyDealOffer.SetDailyDealOfferUI(ReferenceHolder.GetHospital().dailyDealController.GetSaveData().CurrentDailyDeal);
            if (phoneUI)
            {
                dailyDealOffer.transform.SetParent(contentSmall);
            }
            else
            {
                dailyDealOffer.transform.SetParent(content);
            }
            dailyDealOffer.transform.localScale = new Vector3(1, 1, 1);
        }

        private void AddSingleOffer(PharmacyOrderAdvertised offer, bool loadFromServer = true)
        {
            PharmacyGlobalOffer globalOffer = Instantiate(GlobalPlayerOffer).GetComponent<PharmacyGlobalOffer>();

            globalOffer.hospitalNameText.text = offer.GetHospitalName(visitingHospitalName);
            if (offer.medicine == null)
            {
                globalOffer.itemNameText.text = "Update Game!";
            }
            else
            {
                globalOffer.itemNameText.text = ResourcesHolder.Get().GetNameForCure(offer.medicine);
                globalOffer.itemIcon.sprite = ResourcesHolder.Get().GetSpriteForCure(offer.medicine);
            }
            globalOffer.amountText.text = offer.amount + "x";
            globalOffer.priceText.text = (offer.pricePerUnit).ToString();
            if (offer.pricePerUnit < ResourcesHolder.Get().GetDefaultPriceForCure(offer.medicine) * offer.amount)
                globalOffer.discountBadge.enabled = true;
            else
                globalOffer.discountBadge.enabled = false;

            globalOffer.SetHelpBadges(offer.HasPlantationHelpRequest(), offer.HasEpidemyHelpRequest(), offer.HasTreatmentHelpRequest());

            globalOffer.buyerLevelText.text = offer.GetLevel().ToString();

            if ((offer.bought || offer.bougthBuyWise) && DefaultConfigurationProvider.GetConfigCData().GlobalOffersCheckingOfferState)
            {
                globalOffer.SetSoldState(true);
            }
            else
            {
                globalOffer.SetSoldState(false);
            }

            if (AccountManager.Instance.IsFacebookConnected && !string.IsNullOrEmpty(offer.GetFacebookID()))
            {
                CacheManager.GetUserDataByFacebookID(offer.GetFacebookID(), (login, avatar) =>
                {
                    if (globalOffer != null)
                    {
                        globalOffer.hospitalNameText.text = login;
                        globalOffer.buyerAvatar.sprite = avatar;
                    }
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
            if (phoneUI)
            {
                globalOffer.transform.SetParent(contentSmall);
            }
            else
            {
                globalOffer.transform.SetParent(content);
            }
            globalOffer.transform.localScale = new Vector3(1, 1, 1);

            Offer tempOffer = new Offer(offer, globalOffer.gameObject);
            if (offer.medicine != null)
            {
                globalOffer.button.onClick.AddListener(delegate ()
                {
                    offer.isVisited = true;
                    Exit();
                    VisitingController.Instance.Visit(tempOffer.Order.UserID, true);
                    AnalyticsController.instance.ReportSocialVisit(VisitingEntryPoint.Pharmacy, tempOffer.Order.UserID);
                });
            }

            if (!loadFromServer && !offer.bought && !offer.bougthBuyWise)
            {
                globalOffer.CheckingAvailability(offer);
            }

            globalOffer.SetVisited(offer.isVisited);
        }

        public void ReturnToPharmacy()
        {
            Exit();
            if (Pharmacy.visitingMode)
            {
                UIController.getHospital.PharmacyPopUp.Open(true, SaveLoadController.SaveState.ID, SaveLoadController.SaveState.HospitalName);
            }
            else
            {
                UIController.getHospital.PharmacyPopUp.Open();
            }
        }

        public void CloseGlobalPharmacy()
        {
            Exit();
        }

        float timer = 1f;
        void Update()
        {
            timer += Time.deltaTime;
            if (counter != null && timer > .5f)
            {
                if (Spinner.activeSelf)
                {
                    FreeRefrshText.SetActive(false);
                    RefreshButton.SetActive(false);
                    FreeRefrshButton.SetActive(false);
                    friendsOffersToggle.gameObject.SetActive(false);
                    counter.text = "";
                }
                else if (ReferenceHolder.GetHospital().Pharmacy.IsSpeedUpEnabled())
                {
                    FreeRefrshButton.SetActive(false);
                    FreeRefrshText.SetActive(true);
                    RefreshButton.SetActive(true);
                    friendsOffersToggle.gameObject.SetActive(true);
                    counter.text = UIController.GetFormattedTime((int)ReferenceHolder.GetHospital().Pharmacy.TimeTillPageRefresh);
                    refreshTimer.anchoredPosition = new Vector2(238, 37);
                }
                else
                {
                    FreeRefrshText.SetActive(false);
                    RefreshButton.SetActive(false);
                    FreeRefrshButton.SetActive(true);
                    friendsOffersToggle.gameObject.SetActive(true);
                    counter.text = "";
                    refreshTimer.anchoredPosition = new Vector2(169, 37);
                }
                timer = 0;
            }
        }
    }
}

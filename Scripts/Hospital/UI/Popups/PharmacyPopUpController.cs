using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using SimpleUI;
using TMPro;
using MovementEffects;

namespace Hospital
{
    public class PharmacyPopUpController : UIElement
    {
        public GameObject pharmacyNameGO;
        public GameObject globalOffers;
        public TextMeshProUGUI pharmacyNameText;
        public GameObject PlayerOfferPrefab;
        public GameObject CreateOfferPrefab;
        public GameObject AddMoreOffersPrefab;
        public Transform content;
        public Transform contentSmall;
        public GameObject Spinner;
        public Sprite drWiseAvatar;
        public Sprite DefaultAvatar;

        public int salesUnlocked;
        public Image closeButtonImage;

        public GameObject tutorialArrowClose;
        public Animator tutorialTextBuyAnim;

        private int localSales;
        private bool phoneUI = false;

        private IEnumerable<PharmacyOrder> localOrders;
        private List<Offer> localOffers;
        private List<PharmacyOrder> orders = new List<PharmacyOrder>();

        public List<PharmacyOrder> GetOrders()
        {
            return orders;
        }

        private IEnumerator<float> InitializeOrdersCoroutine;

        public delegate void OnActionCallback();
        private class Offer
        {
            public PharmacyOrder Order;
            public GameObject Card;
            public Offer(PharmacyOrder o, GameObject c)
            {
                this.Order = o;
                this.Card = c;
            }
        }
        bool visiting = false;
        string visitingID;
        string visitingHospitalName;

        public WisePharmacyManager wisePharmacyManager = new WisePharmacyManager();

        void Awake()
        {
            //Debug.LogError("elo");
            //wisePharmacyManager = GetComponent<WisePharmacyManager>();
        }

        private bool IsWiseHospital()
        {
            return visiting && visitingID == "SuperWise";
        }

        public bool firstLaunch = true;

        public void Open(bool visitMode = false, string visitingID = null, string visitingHospitalName = null)
        {
            if (UIController.get.drawer.IsVisible)
            {
                UIController.get.drawer.SetVisible(false);
                return;
            }
            if (UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.SetVisible(false);
                return;
            }

            firstLaunch = true;
            Spinner.SetActive(false);
            if (ExtendedCanvasScaler.isPhone() || ExtendedCanvasScaler.HasNotch())
            {
                gameObject.transform.GetChild(0).gameObject.SetActive(false);
                gameObject.transform.GetChild(1).gameObject.SetActive(true);
                phoneUI = true;
            }
            else
            {
                phoneUI = false;
                gameObject.transform.GetChild(0).gameObject.SetActive(true);
                gameObject.transform.GetChild(1).gameObject.SetActive(false);
            }

            KillOrdersCoroutine();
            ClearAllPharmacyChildrenInContent();
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, OnPostOpen(visitMode, visitingID, visitingHospitalName)));
        }

        private Action OnPostOpen(bool visitMode = false, string visitingID = null, string visitingHospitalName = null)
        {
            visiting = visitMode;
            this.visitingID = visitingID;
            this.visitingHospitalName = visitingHospitalName;
            if (visiting && String.IsNullOrEmpty(visitingID))
            {
                throw new IsoEngine.IsoException("UserID string does not contain anything or is null");
            }
            localOffers = new List<Offer>();
            localSales = salesUnlocked;
            SetPharmacyName();

            tutorialArrowClose.SetActive(false);
            tutorialTextBuyAnim.gameObject.SetActive(false);
            if (IsWiseHospital())
            {
                orders = new List<PharmacyOrder>();
                foreach (WiseOrder order in wisePharmacyManager.GetOffers())
                {
                    orders.Add(order);
                }
                SetUpPharmacy(orders);

                NotificationCenter.Instance.PharmacyOpened.Invoke(new PharmacyOpenedEventArgs());
                if (Game.Instance.gameState().GetHospitalLevel() == 6 && !TutorialController.Instance.IsTutorialStepCompleted(StepTag.wise_thank_you))
                {
                    BlinkCloseButton();
                    ShowBuyText();
                }
            }
            else
            {
                AccountManager.OnFacebookStateUpdate += AccountManager_OnFacebookStateUpdate;

                PharmacyManager.Instance.ResumeUserOffersRefresh();
                Spinner.SetActive(true);
                if (AccountManager.HasInternetConnection())
                {
                    if (visiting || (!visiting && !DeveloperParametersController.Instance().parameters.PharmacyMigrationOffersOn))
                    {
                        StartSynchronizingOffers();
                    }
                    else
                    {
                        Debug.LogError("start migrating offers");
                        MigrationOffersUseCase.Instance.Execute(() =>
                        {
                            StartSynchronizingOffers();
                        }, (ex) =>
                        {
                            Exit();
                            BaseUIController.ShowInternetConnectionProblemPopup(this);
                        });
                    }

                    NotificationCenter.Instance.PharmacyOpened.Invoke(new PharmacyOpenedEventArgs());
                    if (!visitMode)
                    {
                        GameState.Get().HomePharmacyVisited = true;
                    }
                }
                else
                {
                    Exit();
                    BaseUIController.ShowInternetConnectionProblemPopup(this);
                }
            }

            if (Game.Instance.gameState().GetHospitalLevel() < 7)
                globalOffers.SetActive(false);
            else
                globalOffers.SetActive(true);

            if (visiting)
                UIController.get.SetCounters(true);
            UIController.get.CountersToFront();

            return null;
        }

        private void StartSynchronizingOffers()
        {
            PharmacyManager.Instance.BindUserOffersRefreshCallback(getProperUserId(), (x, y) =>
            {
                List<PharmacyOrder> list = new List<PharmacyOrder>();
                list.AddRange(x.Select(z => (PharmacyOrder)z));
                list.AddRange(y.Select(s => (PharmacyOrder)s));
                list = list.OrderBy(o => o.sortOrder).ToList();
                orders = list;
                if (visiting)
                {
                    ReferenceHolder.GetHospital().Pharmacy.HasNewOffers = Pharmacy.HasOffersToSell(x, y);
                }
                Spinner.SetActive(false);
                SetUpPharmacy(list);
            }, (ex) =>
            {
                BaseUIController.ShowServerOrInternetConnectionProblem(ex);
                Spinner.SetActive(false);
                Exit();
            });
        }

        private void AccountManager_OnFacebookStateUpdate()
        {
            SetUpPharmacy(orders);
        }

        void ClearAllPharmacyChildrenInContent()
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in contentSmall)
            {
                Destroy(child.gameObject);
            }
        }

        void SetPharmacyName()
        {
            if (visiting)
            {
                pharmacyNameGO.SetActive(true);
                pharmacyNameText.text = string.Format(I2.Loc.ScriptLocalization.Get("PHARMACY_TITLE"), visitingHospitalName);
            }
            else
            {
                pharmacyNameGO.SetActive(false);
            }
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            PharmacyManager.Instance.UnbindAllCallbacks();
            AccountManager.OnFacebookStateUpdate -= AccountManager_OnFacebookStateUpdate;
            base.Exit(hidePopupWithShowMainUI);
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
            NotificationCenter.Instance.PharmacyClosed.Invoke(new PharmacyClosedEventArgs());

            if (Game.Instance.gameState().GetHospitalLevel() == 7)
                TutorialUIController.Instance.StopBlinking();   //global offers button is blinking after tutorial step: pharmacy_offers

            UIController.get.CountersToBack();
            if (visiting)
                UIController.get.SetCounters(false);

            if (!visiting)
            {
                bool hasSoldOffers = false;
                foreach (PharmacyOrder order in orders)
                {
                    if (order.bougthBuyWise || order.bought)
                    {
                        hasSoldOffers = true;
                        break;
                    }
                }
                ReferenceHolder.GetHospital().Pharmacy.SetSoldOffersState(hasSoldOffers);
            }
        }

        public void ButtonExit()
        {
            Exit();
        }

        private string getProperUserId()
        {
            return visiting ? visitingID : CognitoEntry.SaveID;
        }

        void SetUpPharmacy(List<PharmacyOrder> orders)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            ClearAllPharmacyChildrenInContent();
            KillOrdersCoroutine();
            InitializeOrdersCoroutine = Timing.RunCoroutine(InitializeOrders(orders));
        }

        private PharmacyOrder GetOfferWithIndex(int index)
        {
            foreach (PharmacyOrder order in orders)
            {
                if (order.sortOrder == index)
                {
                    return order;
                }
            }
            return null;
        }

        private void UpdateOrders(List<PharmacyOrder> orders)
        {
            if (!visiting)
            {
                for (int i = 0; i < salesUnlocked; ++i)
                {
                    PharmacyOrder order = GetOfferWithIndex(i);
                    if (order == null)
                    {
                        AddToContent(CreateCreateOffer(i));
                    }
                    else
                    {
                        AddToContent(CreatePlayerOffer(order, order.runSpawnAnim));
                        order.runSpawnAnim = false;
                    }
                }
                AddToContent(AddBuyMoreOffers());
            }
            else
            {
                for (int i = 0; i < salesUnlocked; ++i)
                {
                    PharmacyOrder order = GetOfferWithIndex(i);
                    if (order == null || order.medicine == null)
                    {
                        AddToContent(CreateCreateOffer(i));
                    }
                    else
                    {
                        AddToContent(CreatePlayerOffer(order, order.runSpawnAnim));
                        order.runSpawnAnim = false;
                    }
                }
            }
        }


        private IEnumerator<float> InitializeOrders(List<PharmacyOrder> orders)
        {
            if (firstLaunch)
            {
                float step = 0.07f;
                if (!visiting)
                {
                    for (int i = 0; i < salesUnlocked; ++i)
                    {
                        PharmacyOrder order = GetOfferWithIndex(i);
                        if (order == null)
                        {
                            AddToContent(CreateCreateOffer(i));
                            yield return Timing.WaitForSeconds(step);
                        }
                        else
                        {
                            AddToContent(CreatePlayerOffer(order, true));
                            yield return Timing.WaitForSeconds(step);
                        }
                    }
                    AddToContent(AddBuyMoreOffers());

                }
                else
                {
                    for (int i = 0; i < salesUnlocked; ++i)
                    {
                        PharmacyOrder order = GetOfferWithIndex(i);
                        if (order == null || order.medicine == null)
                        {
                            AddToContent(CreateCreateOffer(i));
                            yield return Timing.WaitForSeconds(step);
                        }
                        else
                        {
                            AddToContent(CreatePlayerOffer(order, true));
                            yield return Timing.WaitForSeconds(step);
                        }
                    }
                }
                firstLaunch = false;
            }
            else
            {
                UpdateOrders(orders);
            }
        }

        private void AddToContent(GameObject item)
        {
            if (item == null)
                return;
            if (phoneUI)
            {
                item.transform.SetParent(contentSmall);
            }
            else
            {
                item.transform.SetParent(content);
            }
            item.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        private GameObject CreatePlayerOffer(PharmacyOrder order, bool ProcessSpawnAnimations = false)
        {
            if (order.medicine == null)
                return null;
            GameObject offerGO = Instantiate(PlayerOfferPrefab);
            PharmacyPlayerOffer playerOffer = offerGO.GetComponent<PharmacyPlayerOffer>();

            bool IsOfferBought = CacheManager.IsOfferBought(order);

            if (order.runDescentAnim)
            {
                order.runDescentAnim = false;
                playerOffer.SetupItemDescent(order.amount, ResourcesHolder.Get().GetSpriteForCure(order.medicine));
                playerOffer.ActivateItemDescent();
            }
            else
            {
                playerOffer.DeactivateItemDescent();
            }
            if (ProcessSpawnAnimations)
            {
                playerOffer.SetAnimator("Bounce");
            }
            playerOffer.SetItemIcon(true, ResourcesHolder.Get().GetSpriteForCure(order.medicine));
            playerOffer.SetAmountText(true, order.amount);
            playerOffer.SetPriceText(order.pricePerUnit);
            if (!visiting)
            {
                playerOffer.SetAdBadge(order is PharmacyOrderAdvertised && !order.IsExpired());
            }
            if (order.bought)
            {
                playerOffer.SetAdBadge(false);
                if (visiting)
                {
                    playerOffer.SetGrayscale(true);
                    playerOffer.SetAmountText(false, 0);
                    playerOffer.SetBuyer(false, null, null, 1);
                }
                else
                {
                    playerOffer.SetGrayscale(false);
                    playerOffer.SetItemIcon(false, null);
                    playerOffer.SetBuyer(true, order.bougthBuyWise ? drWiseAvatar : DefaultAvatar, order.GetBuyerHospitalName(), order.bougthBuyWise ? 100 : order.GetBuyerLevel());
                    if (!order.bougthBuyWise)
                    {
                        if (AccountManager.Instance.IsFacebookConnected && !string.IsNullOrEmpty(order.GetBuyerFacebookID()))
                        {
                            CacheManager.GetUserDataByFacebookID(order.GetBuyerFacebookID(), (login, avatar) =>
                            {
                                if (offerGO != null && playerOffer != null)
                                {
                                    playerOffer.SetBuyer(true, avatar, login, order.GetBuyerLevel());
                                }
                            }, (ex) =>
                            {
                                Debug.LogError(ex.Message);
                            });
                        }
                    }
                }
            }
            else
            {
                playerOffer.SetBuyer(false, null, null, 1);
            }

            if (IsOfferBought && visiting)
            {
                playerOffer.SetAdBadge(false);
                playerOffer.SetGrayscale(true);
                playerOffer.SetAmountText(false, 0);
                playerOffer.SetBuyer(false, null, null, 1);
            }

            if (visiting && order.pricePerUnit < ResourcesHolder.Get().GetDefaultPriceForCure(order.medicine) * order.amount && !order.bought && !IsOfferBought)
                playerOffer.SetDiscountBadge(true);
            else
                playerOffer.SetDiscountBadge(false);

            int levelNeeded = ResourcesHolder.Get().GetLvlForCure(order.medicine);
            if (levelNeeded > Game.Instance.gameState().GetHospitalLevel())
            {
                playerOffer.SetLockedByLevel(true, levelNeeded);
                playerOffer.SetAmountText(false, 0);
            }
            else
                playerOffer.SetLockedByLevel(false, 0);

            Offer offer = new Offer(order, offerGO);
            if (order.bought && !visiting)
            {
                playerOffer.button.onClick.AddListener(delegate
                {
                    ClaimSoldOffer(order, playerOffer);
                });
            }
            else if (!visiting)
            {
                playerOffer.button.onClick.AddListener(delegate
                {
                    UIController.getHospital.ModifyOfferPopUp.Open(order, offerGO);
                });
            }
            else if (!order.bought && levelNeeded <= Game.Instance.gameState().GetHospitalLevel())
            {
                if (!IsOfferBought)
                {
                    playerOffer.button.onClick.AddListener(delegate
                    {
                        Buy(offer);
                    });
                }
            }
            if (visiting)
            {
                playerOffer.SetAdBadge(false);
                if (order.bought || IsOfferBought)
                {
                    playerOffer.SetGrayscale(true);
                    playerOffer.SetAmountText(false, 0);
                    if (levelNeeded <= Game.Instance.gameState().GetHospitalLevel())
                        playerOffer.SetSoldVisit(true);
                }
            }
            localOffers.Add(offer);
            return offerGO;
        }

        private void MarkSold(Offer order)
        {
            order.Card.GetComponent<Button>().onClick.RemoveAllListeners();
            order.Card.transform.GetChild(6).gameObject.SetActive(true);
            if (visiting && order.Card.gameObject != null)
            {
                PharmacyPlayerOffer playerOffer = order.Card.gameObject.GetComponent<PharmacyPlayerOffer>();
                playerOffer.SetGrayscale(true);
                playerOffer.SetAmountText(false, 0);
                playerOffer.SetSoldVisit(true);
            }
            order.Order.bought = true;


            CheckTutorialText();
        }

        void CheckTutorialText()
        {
            //on level 6 in wise's pharmacy there's a tutorial text saying to buy offers. It has to be disabled after all offers are purchased.
            if (Game.Instance.gameState().GetHospitalLevel() != 6 || !IsWiseHospital())
                return;

            bool allBought = true;
            for (int i = 0; i < salesUnlocked; ++i)
            {
                if (!orders[i].bought)
                {
                    allBought = false;
                    break;
                }
            }
            if (allBought)
                UIController.getHospital.PharmacyPopUp.HideBuyText();
        }

        private void Buy(Offer order)
        {
            if (order == null)
            {
                return;
            }
            int amount = order.Order.amount;
            int totalPrice = order.Order.pricePerUnit;

            if (Game.Instance.gameState().GetCoinAmount() >= totalPrice)
            {
                bool isTankStorageitem = order.Order.medicine.IsMedicineForTankElixir();

                if ((order.Order.medicine.IsMedicineForTankElixir() && GameState.Get().CanAddAmountForTankStorage(order.Order.amount)) || (!order.Order.medicine.IsMedicineForTankElixir() && GameState.Get().CanAddAmountForElixirStorage(order.Order.amount)))
                {
                    if (order.Card != null && order.Card.gameObject != null)
                    {
                        PharmacyPlayerOffer playerOffer = order.Card.gameObject.GetComponent<PharmacyPlayerOffer>();
                        if (playerOffer != null)
                        {
                            playerOffer.SetupItemDescent(totalPrice);
                            playerOffer.ActivateItemDescent();
                            playerOffer.SetAdBadge(false);
                            playerOffer.SetDiscountBadge(false);

                            Vector3 pos = ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition);
                            UIController.get.storageCounter.Add(isTankStorageitem, true);
                            ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Medicine, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), amount, .5f, 1.75f, new Vector3(2, 2, 2), new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(order.Order.medicine), null, () =>
                            {
                                UIController.get.storageCounter.Remove(amount, isTankStorageitem);
                            });
                        }
                    }

                    MarkSold(order);
                    GameState.Get().RemoveCoins(totalPrice, EconomySource.PharmacyItemPurchased);

                    if (order.Order is WiseOrder)
                    {
                        GameState.Get().AddResource(order.Order.medicine, order.Order.amount, false, EconomySource.PharmacyBuy);
                        DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.BuyInPharmacy));
                        wisePharmacyManager.SetBouthOfferAtSortOrder(order.Order.sortOrder);
                        ReferenceHolder.GetHospital().Pharmacy.UpdateNewOffersState();
                        return;
                    }

                    GameState.Get().AddResource(order.Order.medicine, order.Order.amount, false, EconomySource.PharmacyBuy);
                    CacheManager.SetOfferBought(order.Order);
                    PharmacyManager.Instance.PauseUserOffersRefresh();
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.BuyInPharmacy));

                    if (order.Order is PharmacyOrderAdvertised)
                    {
                        PharmacyManager.Instance.BuyAdvertisedOrder((PharmacyOrderAdvertised)order.Order, () =>
                        {
                            ReferenceHolder.Get().engine.AddTask(() =>
                            {
                                ReferenceHolder.GetHospital().Pharmacy.UpdateNewOffersState();
                            });
                        }, exception =>
                        {
                            ReferenceHolder.Get().engine.AddTask(() =>
                            {
                                ReferenceHolder.GetHospital().Pharmacy.UpdateNewOffersState();
                            });
                        });
                    }
                    else
                    {
                        PharmacyManager.Instance.BuyStandardOrder((PharmacyOrderStandard)order.Order, () =>
                        {
                            ReferenceHolder.Get().engine.AddTask(() =>
                            {
                                ReferenceHolder.GetHospital().Pharmacy.UpdateNewOffersState();
                            });
                        }, exception =>
                        {
                            ReferenceHolder.Get().engine.AddTask(() =>
                            {
                                ReferenceHolder.GetHospital().Pharmacy.UpdateNewOffersState();
                            });
                        });
                    }
                }
                else
                {
                    if (order.Order.medicine.IsMedicineForTankElixir() && !GameState.Get().CanAddAmountForTankStorage(order.Order.amount))
                    {
                        MessageController.instance.ShowMessage(47);
                        StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(true));
                    }
                    else
                    {
                        MessageController.instance.ShowMessage(9);
                        StartCoroutine(UIController.getHospital.StorageFullPopUp.Open(false));
                    }
                }
            }
            else
            {
                UIController.getHospital.BuyResourcesPopUp.Open(totalPrice - Game.Instance.gameState().GetCoinAmount(), () =>
                {
                    Buy(order);
                }, null);
                MessageController.instance.ShowMessage(I2.Loc.ScriptLocalization.Get("NOT_ENOUGH_COINS_INFO"));
            }

        }

        private void TryToAddMedicine(bool isTankStorage, bool MedicineReachedPanel, bool ServerResponded, int amount)
        {
            if (MedicineReachedPanel && ServerResponded)
            {
                UIController.get.storageCounter.Remove(amount, isTankStorage);
            }
        }

        private void ClaimSoldOffer(PharmacyOrder offer, PharmacyPlayerOffer playerOffer)
        {
            bool giftReached = false;
            bool serverResponded = false;
            Vector2 pos = Vector3.zero;
            if (playerOffer != null && playerOffer.gameObject != null)
            {
                pos = new Vector2((playerOffer.gameObject.transform.position.x - Screen.width / 2) / UIController.get.transform.localScale.x, (playerOffer.gameObject.transform.position.y - Screen.height / 2) / UIController.get.transform.localScale.y);
            }
            orders.Remove(offer);
            KillOrdersCoroutine();
            ClearAllPharmacyChildrenInContent();
            InitializeOrdersCoroutine = Timing.RunCoroutine(InitializeOrders(orders));
            ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Coin, pos, offer.pricePerUnit, .4f, 2f, new Vector3(1f, 1f, 1), new Vector3(1, 1, 1), null, null, () =>
            {
                giftReached = true;
                TryUpdateCoinCounter(giftReached, serverResponded, offer.pricePerUnit, offer);
            });
            PharmacyManager.Instance.PauseUserOffersRefresh();
            GameState.Get().AddCoins(offer.pricePerUnit, EconomySource.PharmacyItemSold, false);
            NotificationCenter.Instance.PharmacySoldItemClaimed.Invoke(new ResourceAmountChangedEventArgs(offer.medicine, offer.amount,
                ResourcesHolder.GetHospital().GetMedicinesOfType(offer.medicine.type).Count, EconomySource.PharmacyItemSold));
            // add claim transaction
            GameState.Get().AddToPendingClaimedOrderTransactionsList(offer);
            PharmacyManager.Instance.ClaimCoinsForSoldOrder(offer, () =>
            {
                ReferenceHolder.Get().engine.AddTask(() =>
                {
                    serverResponded = true;
                    TryUpdateCoinCounter(giftReached, serverResponded, offer.pricePerUnit, offer);
                    AchievementNotificationCenter.Instance.CoinsMadeOutOfPharmacySales.Invoke(new AchievementProgressEventArgs(offer.pricePerUnit));
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.SellInThePharmacy));
                    if (orders.Any())
                    {
                        orders.Remove(offer);
                        ClearAllPharmacyChildrenInContent();
                        KillOrdersCoroutine();
                        InitializeOrdersCoroutine = Timing.RunCoroutine(InitializeOrders(orders));
                    }
                });
            }, (ex) =>
            {
                GameState.Get().RemoveFromPendingClaimedOrderTransactionsList(offer);
                GameState.Get().RemoveCoins(offer.pricePerUnit, EconomySource.RollbackPharmacyCollect, true);
            });
        }

        private void TryUpdateCoinCounter(bool giftReached, bool serverResponded, int amount, PharmacyOrder order)
        {
            if (serverResponded)
            {
                // remove claim transaction
                GameState.Get().RemoveFromPendingClaimedOrderTransactionsList(order);
            }
            if (giftReached && serverResponded)
            {
                int amountBeforeReward = Game.Instance.gameState().GetCoinAmount() - amount;
                GameState.Get().UpdateCounter(ResourceType.Coin, amount, amountBeforeReward);
            }
        }

        private GameObject AddBuyMoreOffers()
        {
            PharmacyBuyMoreOffers buyMore = Instantiate(AddMoreOffersPrefab).GetComponent<PharmacyBuyMoreOffers>();
            int price = DiamondCostCalculator.GetPharmacyMoreOffersCost(salesUnlocked);
            buyMore.SetPrice(price);
            buyMore.button.onClick.AddListener(delegate ()
            {
                AddMoreOffers(price, buyMore.gameObject);
            });
            return buyMore.gameObject;
        }

        private GameObject CreateCreateOffer(int index)
        {
            GameObject temp = Instantiate(CreateOfferPrefab);
            if (visiting)
            {
                temp.transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                temp.GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    if (IsVisible)
                    {
                        if (GameState.Get().EnumerateResourcesMedRef().Any())
                        {                            
                            StartCoroutine(UIController.getHospital.CreateOfferPopUp.Open(temp, index, GameState.Get().elixirStorageAmount == 0));
                        }
                        else
                        {
                            MessageController.instance.ShowMessage(11);
                        }
                    }
                });
            }
            return temp;
        } 

        private void AddMoreOffers(int value, GameObject obj)
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= value)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(value, delegate
                {
                    salesUnlocked++;
                    localSales++;
                    GameState.Get().RemoveDiamonds(value, EconomySource.PharmacyExtendSlots);
                    GameObject temp = CreateCreateOffer(salesUnlocked - 1);
                    temp.transform.SetParent(obj.transform.parent);
                    temp.transform.SetSiblingIndex(obj.transform.GetSiblingIndex());
                    temp.transform.localScale = new Vector3(1, 1, 1);
                    Destroy(obj);
                    AddToContent(AddBuyMoreOffers());
                }, this);
            }
            else
            {
                //Exit();
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void ModifyPlayerOffer(GameObject card, PharmacyOrder offer)
        {
            PharmacyManager.Instance.PauseUserOffersRefresh();

            PharmacyOrder newOrder = new PharmacyOrderAdvertised();
            newOrder.requiredLevel = offer.requiredLevel;
            newOrder.amount = offer.amount;
            newOrder.medicine = offer.medicine;
            newOrder.pricePerUnit = offer.pricePerUnit;
            newOrder.UserID = offer.UserID;
            newOrder.ID = offer.ID;
            newOrder.expirationDate = (long)ServerTime.getTime() + 60 * 60 * 3;
            newOrder.bought = offer.bought;
            newOrder.bougthBuyWise = offer.bougthBuyWise;
            newOrder.buyerSaveID = offer.buyerSaveID;
            newOrder.ownerUser = offer.ownerUser;
            newOrder.buyerUser = offer.buyerUser;
            newOrder.sortOrder = offer.sortOrder;
            newOrder.runDescentAnim = offer.runDescentAnim;
            newOrder.runSpawnAnim = offer.runSpawnAnim;
            newOrder.UUID = offer.UUID;

            for (int i = 0; i < orders.Count; ++i)
            {
                if (orders[i] == offer)
                {
                    orders[i] = newOrder;
                    break;
                }
            }

            AchievementNotificationCenter.Instance.AdPlaced.Invoke(new AchievementProgressEventArgs(1));

            PharmacyManager.Instance.PostAdvertisedOrder(newOrder as PharmacyOrderAdvertised);
            if (offer is PharmacyOrderStandard)
            {
                PharmacyManager.Instance.RemoveStandardOrder(offer as PharmacyOrderStandard);
            }

            GameState.Get().LastPromotionOfferAdd = (long)ServerTime.getTime();
            PublicSaveManager.Instance.UpdatePublicSaveForEvent();

            ClearAllPharmacyChildrenInContent();
            KillOrdersCoroutine();
            InitializeOrdersCoroutine = Timing.RunCoroutine(InitializeOrders(orders));
        }

        private void KillOrdersCoroutine()
        {
            if (InitializeOrdersCoroutine != null)
            {
                Timing.KillCoroutine(InitializeOrdersCoroutine);
                InitializeOrdersCoroutine = null;
            }
        }

        public void AddToOffersLists(GameObject card, PharmacyOrder offer)
        {
            PharmacyOrder databaseOffer = null;
            if (offer is PharmacyOrderAdvertised)
            {
                databaseOffer = offer as PharmacyOrderAdvertised;
                GameState.Get().AddNewOrderToPendingAddOrderTransactionsList(databaseOffer);
                GameState.Get().GetCure(offer.medicine, offer.amount, EconomySource.PharmacySell);
                PharmacyManager.Instance.PostAdvertisedOrder((PharmacyOrderAdvertised)databaseOffer, () =>
                {
                    GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(databaseOffer);
                    GameState.Get().LastPromotionOfferAdd = (long)ServerTime.getTime();
                    PublicSaveManager.Instance.UpdatePublicSaveForEvent();
                }, (ex) =>
                {
                    GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(databaseOffer);
                    GameState.Get().AddResource(databaseOffer.medicine, offer.amount, true, EconomySource.PharmacyRefund);
                });
            }
            else
            {
                databaseOffer = offer as PharmacyOrderStandard;
                if (databaseOffer == null)
                {
                    return;
                }
                GameState.Get().AddNewOrderToPendingAddOrderTransactionsList(databaseOffer);
                GameState.Get().GetCure(offer.medicine, offer.amount, EconomySource.PharmacySell);
                PharmacyManager.Instance.PostStandardOrder((PharmacyOrderStandard)databaseOffer, () =>
                {
                    GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(databaseOffer);
                    GameState.Get().LastStandardOfferAdd = (long)ServerTime.getTime();
                    PublicSaveManager.Instance.UpdatePublicSaveForEvent();
                }, (ex) =>
                {
                    GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(databaseOffer);
                    GameState.Get().AddResource(databaseOffer.medicine, offer.amount, true, EconomySource.PharmacyRefund);
                });
            }
            if (databaseOffer != null)
            {
                databaseOffer.isLocalOffer = true;
                orders.Add(databaseOffer);
                SetUpPharmacy(orders);
            }
        }

        public void RemoveOffer(GameObject card, PharmacyOrder offer)
        {
            GameState.Get().AddOrderToPendingDeleteOrderTransactionsList(offer);
            GameState.Get().RemoveDiamonds(ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost, EconomySource.PharmacyAdvert);
            if (offer is PharmacyOrderStandard)
            {
                PharmacyManager.Instance.RemoveStandardOrder(offer as PharmacyOrderStandard, () =>
                {
                    GameState.Get().RemoveOrderFromPendingDeleteOrderTransactionsList(offer);
                }, (ex) =>
                {
                    GameState.Get().RemoveOrderFromPendingDeleteOrderTransactionsList(offer);
                    GameState.Get().AddDiamonds(ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost, EconomySource.PharmacyRefund, true);
                });
            }
            else
            {
                PharmacyManager.Instance.RemoveAdvertisedOrder(offer as PharmacyOrderAdvertised, () =>
                {
                    GameState.Get().RemoveOrderFromPendingDeleteOrderTransactionsList(offer);
                }, (ex) =>
                {
                    GameState.Get().RemoveOrderFromPendingDeleteOrderTransactionsList(offer);
                    GameState.Get().AddDiamonds(ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost, EconomySource.PharmacyRefund, true);
                });
            }
            if (orders.Count > 0)
            {
                RemoveOffer(offer);
                ClearAllPharmacyChildrenInContent();
                UpdateOrders(orders);
            }
        }

        public void RemoveOffer(PharmacyOrder offer)
        {
            foreach (PharmacyOrder order in orders)
            {
                if (order.ID == offer.ID && order.sortOrder == offer.sortOrder)
                {
                    orders.Remove(order);
                    return;
                }
            }
        }

        public void ButtonGlobalOffers()
        {
            StartCoroutine(UIController.getHospital.GlobalOffersPopUp.Open(visitingHospitalName));
        }

        void ShowBuyText()
        {
            //this is for tutorial on level 6 at wise's pharmacy to show players they can close the pop up (duh) :)
            CancelInvoke("DelayedBuyText");
            Invoke("DelayedBuyText", 2f);
        }

        void DelayedBuyText()
        {
            tutorialTextBuyAnim.gameObject.SetActive(true);
            tutorialTextBuyAnim.SetBool("IsVisible", true);
        }

        void HideBuyText()
        {
            CancelInvoke("DelayedBuyText");
            tutorialTextBuyAnim.SetBool("IsVisible", false);
        }

        void BlinkCloseButton()
        {
            //this is for tutorial on level 6 at wise's pharmacy to show players they can close the pop up (duh) :)
            CancelInvoke("DelayedBlinkClose");
            Invoke("DelayedBlinkClose", 10f);
        }

        void DelayedBlinkClose()
        {
            //tutorialArrowClose.SetActive(true);
        }
    }
}
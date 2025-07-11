using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hospital
{
    public class Pharmacy : SuperObject
    {
        public int GlobalOffersBoardIntervalDurationInSeconds;
        private long LastRefreshTime = 0;
        public float AdTime;
        public int SpeedUpCost;
        public int DeleteOfferCost;
        public int listSize;
        public float TimeTillFreeAd { get; private set; }
        public float debugTime;
        public float TimeTillPageRefresh = 0;
        public GameObject NotificationIcon;
        public GameObject hearts;
#pragma warning disable 0649
        [SerializeField] private GameObject crossGlow;
        [SerializeField] private GameObject roof;
#pragma warning restore 0649
        // private int scanRand = 0;
        [SerializeField] private List<PharmacyOrderAdvertised> offerz;
        public List<PharmacyOrderAdvertised> offers
        {
            get { return offerz; }
            private set { offerz = value; }
        }

        public List<PharmacyOrderAdvertised> orders = null;

        public delegate void PharmacyState(bool avaiable);

        public delegate void OnActionCallback(bool loadFromServer = true);
        public delegate void OnFailureCallback(Exception exception = null);

        public event PharmacyState OnFreeAdStateChanged;
        public event PharmacyState OnPageRefreshStateChanged;

        private Animator animator;

        private bool hasNewOffers;
        public bool HasNewOffers
        {
            set
            {
                hasNewOffers = value;
                UpdateNewOffersBadgeVisibility(value);
            }
        }

        private bool hasSoldOffers;
        public bool HasSoldOffers
        {
            set
            {
                hasSoldOffers = value;
                UpdateNewOffersBadgeVisibility(value);
            }
        }

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public override void OnClick()
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

            if (visitingMode)
            {
                UIController.getHospital.PharmacyPopUp.Open(true, SaveLoadController.SaveState.ID, SaveLoadController.SaveState.HospitalName);
                return;
            }

            if (Game.Instance.gameState().GetHospitalLevel() >= 7)
            {
                if (!TutorialController.Instance.IsTutorialStepCompleted(StepTag.pharmacy_tap_to_open) || roof.gameObject.activeInHierarchy)
                {
                    TurnOnCrossGlowHideRoof();
                    GameObject go = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, transform.position + new Vector3(-1f, 0f, 0.6f), Quaternion.Euler(0, 0, 0));
                    go.SetActive(true);
                    SoundsController.Instance.PlayCheering();
                    NotificationCenter.Instance.PharmacyOpened.Invoke(new PharmacyOpenedEventArgs());
                    UIController.getHospital.PharmacyPopUp.Open();
                }
                else if (!ConnectFBPopupController.OpenIfPossible(/*this*/))
                {
                    UIController.getHospital.PharmacyPopUp.Open();
                }
            }
            else
            {
                UIController.getHospital.LockedFeatureArtPopUpController.Open(LockedFeature.Pharmacy);
            }
        }

        public override void IsoDestroy() { }

        // metoda wywoływana podczas ładowania save'a - nie wywołuje się gdy kogoś odwiedzamy (Method called when save loading - not called when we visit someone)
        public void LoadState(Save save, TimePassedObject timePassed)
        {
            TimeTillPageRefresh = save.PharmacyPageRefreshTime;

            LastRefreshTime = save.PharmacyLastRefreshBoardTime;
            TimeTillFreeAd = save.PharmacyTime;
            if (TimeTillFreeAd > 0)
            {
                TimeTillFreeAd = Math.Max(0, TimeTillFreeAd - timePassed.GetTimePassed());
            }

            var unparsedOrders = save.PharmacyOffers;
            // PrintOrders(unparsedOrders);
            orders = unparsedOrders?.Select((x) => PharmacyOrderAdvertised.Parse(x)).ToList();

            // wyłączenie ikonki notyfikującej o nowych ofertach - włączone tylko podczas odwiedzania (Deactivation of the new offers notification icon - only enabled when visiting)
            HasNewOffers = false;
        }

        public void EmulateTime(TimePassedObject timePassed)
        {
            if (TimeTillFreeAd > 0)
            {
                TimeTillFreeAd = Math.Max(0, TimeTillFreeAd - timePassed.GetTimePassed());
            }
        }

        public void SetTimeTillFreeAd()
        {
            TimeTillFreeAd = AdTime;
        }

        private void PrintOrders(List<string> orders)
        {
            if (orders != null)
            {
                foreach (string unparsedOrder in orders)
                {
                    Debug.LogError(unparsedOrder);
                }
            }
        }

        public bool IsGlobalOffersExpiredOrDosentSet()
        {
            return LastRefreshTime == 0 || ServerTime.Get().GetUnixServerTime() >= LastRefreshTime + GlobalOffersBoardIntervalDurationInSeconds;
        }

        private OnActionCallback OnSuccessGetActualGlobalOffers = null;
        private OnFailureCallback OnFailureGetActualGlobalOffers = null;

        private bool IsGetActualGlobalOffersDelegatesSet()
        {
            return OnSuccessGetActualGlobalOffers != null && OnFailureGetActualGlobalOffers != null;
        }

        private void UnbindGetActualGlobalOffersDelegates()
        {
            OnSuccessGetActualGlobalOffers = null;
            OnFailureGetActualGlobalOffers = null;
        }

        public void GetActualGlobalOffers(OnActionCallback success, OnFailureCallback failure)
        {
            bool isDelagatesSet = IsGetActualGlobalOffersDelegatesSet();
            OnSuccessGetActualGlobalOffers = success;
            OnFailureGetActualGlobalOffers = failure;
            if (isDelagatesSet)
                return;
            if (IsGlobalOffersExpiredOrDosentSet())
            {
                if (DailyQuestSynchronizer.Instance.WeekCounter == 1)
                    DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.BuyGoods));

                ExecuteRefreshGlobalOffers((loadFromServer) =>
                {
                    UpdateLastTimePageRefresh();
                    UpdateTimeToReload();
                    OnSuccessGetActualGlobalOffers?.Invoke();
                    UnbindGetActualGlobalOffersDelegates();
                }, (ex) =>
                {
                    OnFailureGetActualGlobalOffers?.Invoke(ex);
                    UnbindGetActualGlobalOffersDelegates();
                });
            }
            else
            {
                TryToBindUserDataToOffers((loadFromServer) =>
                {
                    UpdateTimeToReload();
                    OnSuccessGetActualGlobalOffers?.Invoke(false);
                    UnbindGetActualGlobalOffersDelegates();
                }, (ex) =>
                {
                    OnFailureGetActualGlobalOffers?.Invoke(ex);
                    UnbindGetActualGlobalOffersDelegates();
                });
            }
        }

        private void UpdateTimeToReload()
        {
            TimeTillPageRefresh = LastRefreshTime + GlobalOffersBoardIntervalDurationInSeconds - ServerTime.Get().GetUnixServerTime();
        }

        private void TryToBindUserDataToOffers(OnActionCallback onSuccess, OnFailureCallback onFailure)
        {
            if (orders == null || orders.Count == 0)
                onSuccess?.Invoke();

            PharmacyManager.Instance.BindUsersToGlobalOffers(orders, (advertised, standard) =>
            {
                onSuccess?.Invoke();
            }, (ex) =>
            {
                onFailure?.Invoke(ex);
            });
        }

        private void UpdateLastTimePageRefresh()
        {
            long CurrentTime = ServerTime.Get().GetUnixServerTime();
            if (LastRefreshTime == 0)
                LastRefreshTime = CurrentTime;
            else
                LastRefreshTime = CurrentTime - ((CurrentTime - LastRefreshTime) % GlobalOffersBoardIntervalDurationInSeconds);
        }

        #region Notyfikacja o nowych ofertach koło apteki przy odwiedzaniu (Notification of new offers near the pharmacy when visiting)
        // sprawdzanie podczas odwiedzania czy gracz ma jakieś oferty na sprzedaż (Checking if a player has any offers for sale when visiting)
        public void UpdateNewOffersState()
        {
            animator.ResetTrigger("Pharmacy Alert");
            animator.SetTrigger("Pharmacy Idle");
            if (visitingMode && SaveLoadController.SaveState.Level >= 7)
            {
                PharmacyManager.Instance.RefreshUserOrders(SaveLoadController.SaveState.ID, (x, y) =>
                {
                    HasNewOffers = HasOffersToSell(x, y);
                    if (hasNewOffers)
                    {
                        animator.ResetTrigger("Pharmacy Idle");
                        animator.SetTrigger("Pharmacy Alert");
                    }
                    else
                    {
                        animator.SetTrigger("Pharmacy Idle");
                    }
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
        }

        public void SetSoldOffersState(bool showBadge)
        {
            animator.ResetTrigger("Pharmacy Alert");
            animator.SetTrigger("Pharmacy Idle");
            if (!visitingMode && SaveLoadController.SaveState.Level >= 7)
            {
                HasSoldOffers = showBadge;
                if (hasSoldOffers)
                {
                    animator.ResetTrigger("Pharmacy Idle");
                    animator.SetTrigger("Pharmacy Alert");
                }
                else
                {
                    animator.SetTrigger("Pharmacy Idle");
                }
            }
        }

        public void UpdateSoldOffersState()
        {
            animator.ResetTrigger("Pharmacy Alert");
            animator.SetTrigger("Pharmacy Idle");
            if (!visitingMode && SaveLoadController.SaveState.Level >= 7)
            {
                PharmacyManager.Instance.RefreshUserOrders(SaveLoadController.SaveState.ID, (x, y) =>
                {
                    HasSoldOffers = HasOffersSold(x, y);

                    if (hasSoldOffers)
                    {
                        animator.ResetTrigger("Pharmacy Idle");
                        animator.SetTrigger("Pharmacy Alert");
                    }
                    else
                    {
                        animator.SetTrigger("Pharmacy Idle");
                    }
                }, (ex) =>
                {
                    Debug.LogError(ex.Message);
                });
            }
        }

        public static bool HasOffersToSell(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standards)
        {
            if (advertised != null)
            {
                for (int i = 0; i < advertised.Count; ++i)
                {
                    if (!advertised[i].bought && !CacheManager.IsOfferBought(advertised[i]))
                    {
                        return true;
                    }
                }
            }
            if (standards != null)
            {
                for (int i = 0; i < standards.Count; ++i)
                {
                    if (!standards[i].bought && !CacheManager.IsOfferBought(standards[i]))
                    {
                        return true;
                    }
                }
            }
            if (VisitingController.Instance.IsVisiting && SaveLoadController.SaveState.ID == "SuperWise")
            {
                List<WiseOrder> wiseOffers = UIController.getHospital.PharmacyPopUp.wisePharmacyManager.GetOffers();
                if (wiseOffers != null)
                {
                    for (int i = 0; i < wiseOffers.Count; ++i)
                    {
                        if (!wiseOffers[i].bought)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public static bool HasOffersSold(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standards)
        {
            ConfirmAddOffersCallback(advertised, standards);
            ConfirmClaimOffersCallback(advertised, standards);
            ConfirmDeleteOffersCallback(advertised, standards);
            if (advertised != null)
            {
                for (int i = 0; i < advertised.Count; ++i)
                {
                    if (advertised[i].bought || advertised[i].bougthBuyWise)
                    {
                        return true;
                    }
                }
            }
            if (standards != null)
            {
                for (int i = 0; i < standards.Count; ++i)
                {
                    if (standards[i].bought || standards[i].bougthBuyWise)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void ConfirmClaimOffersCallback(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standards)
        {
            if (visitingMode || GameState.Get().PendingPharmacyClaimOrderTransactions.Count == 0)
                return;

            List<PharmacyOrder> list = new List<PharmacyOrder>();
            list.AddRange(advertised.Select(a => (PharmacyOrder)a));
            list.AddRange(standards.Select(s => (PharmacyOrder)s));
            foreach (KeyValuePair<string, int> pair in GameState.Get().PendingPharmacyClaimOrderTransactions)
            {
                ConfirmClaimedCallback(list, pair.Key, pair.Value);
            }
        }

        private static void ConfirmClaimedCallback(List<PharmacyOrder> orders, string ID, int prize)
        {
            for (int i = 0; i < orders.Count; ++i)
            {
                if (ID == orders[i].ID)
                {
                    GameState.Get().RemoveFromPendingClaimedOrderTransactionsList(orders[i]);
                    GameState.Get().RemoveCoins(prize, EconomySource.RollbackPharmacyCollect, true);
                    return;
                }
            }
            GameState.Get().RemoveFromPendingClaimedOrderTransactionsList(ID);
        }

        public static void ConfirmDeleteOffersCallback(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standards)
        {
            if (visitingMode || GameState.Get().PendingPharmacyDeleteOrderTransactions.Count == 0)
                return;

            List<PharmacyOrder> list = new List<PharmacyOrder>();
            list.AddRange(advertised.Select(a => (PharmacyOrder)a));
            list.AddRange(standards.Select(s => (PharmacyOrder)s));
            foreach (string ID in GameState.Get().PendingPharmacyDeleteOrderTransactions)
            {
                ConfirmDeleteOfferCallback(list, ID);
            }
        }

        private static void ConfirmDeleteOfferCallback(List<PharmacyOrder> orders, string ID)
        {
            for (int i = 0; i < orders.Count; ++i)
            {
                if (ID == orders[i].ID)
                {
                    GameState.Get().RemoveOrderFromPendingDeleteOrderTransactionsList(orders[i]);
                    // REFUND - RETURN DIAMONDS
                    GameState.Get().AddDiamonds(ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost, EconomySource.PharmacyRefund, true);
                    return;
                }
            }
            GameState.Get().PendingPharmacyDeleteOrderTransactions.Remove(ID);
        }

        public static void ConfirmAddOffersCallback(List<PharmacyOrderAdvertised> advertised, List<PharmacyOrderStandard> standards)
        {
            if (visitingMode || GameState.Get().PendingPharmacyAddOrderTransactions.Count == 0)
                return;

            List<PharmacyOrder> list = new List<PharmacyOrder>();
            list.AddRange(advertised.Select(a => (PharmacyOrder)a));
            list.AddRange(standards.Select(s => (PharmacyOrder)s));
            foreach (string shortSave in GameState.Get().PendingPharmacyAddOrderTransactions)
            {
                ConfirmAddOfferCallback(list, shortSave);
            }
        }

        private static void ConfirmAddOfferCallback(List<PharmacyOrder> orders, string shortSave)
        {
            for (int i = 0; i < orders.Count; ++i)
            {
                if (shortSave == orders[i].ShortSaveString())
                {
                    GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(orders[i]);
                    return;
                }
            }
            PharmacyOrder order = PharmacyOrder.GetInstance(shortSave);
            if (order != null && GameState.Get().RemoveOrderFromPendingAddOrderTransactionsList(order))
            {
                // REFUND - RETURN ITEM TO STORAGE
                GameState.Get().GetCure(order.medicine, order.amount, EconomySource.PharmacySell);
            }
        }

        private void UpdateNewOffersBadgeVisibility(bool visible)
        {
            NotificationIcon.SetActive(visible);
        }
        #endregion

        public void SaveState(Save save)
        {
            save.PharmacyLastRefreshBoardTime = LastRefreshTime;

            save.PharmacyPageRefreshTime = Checkers.CheckedAmount(TimeTillPageRefresh, -1, AdTime, "TimeTillPageRefresh"); // AdTime do zmiany
            save.PharmacyTime = Checkers.CheckedAmount(TimeTillFreeAd, -1, AdTime, "TimeTillFreeAd"); // AdTime do zmiany

            if (orders != null && orders.Count > 0)
                save.PharmacyOffers = orders.Where(x => x.medicine != null).Select(x => x.ToString()).ToList();
            else
                save.PharmacyOffers = null;
        }

        public bool IsFreeAdAvaiable
        {
            get { return TimeTillFreeAd <= 0; }
        }

        public bool IsPageRefreshAvaiable
        {
            get { return TimeTillPageRefresh <= 0; }
        }

        public void SpeedUpAdvert(IDiamondTransactionMaker diamondTransactionMaker)
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= SpeedUpCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(SpeedUpCost, delegate
                {
                    GameState.Get().RemoveDiamonds(SpeedUpCost, EconomySource.PharmacyAdvert);
                    TimeTillFreeAd = -1;
                    if (OnFreeAdStateChanged != null)
                        OnFreeAdStateChanged.Invoke(true);
                }, diamondTransactionMaker);
            }
        }

        public bool SpeedUpPageRefresh() // No need to add diamonds to transaction. This method is not used?!? TBD.
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= SpeedUpCost)
            {
                GameState.Get().RemoveDiamonds(SpeedUpCost, EconomySource.PharmacyRefresh);
                TimeTillPageRefresh = -1;
                OnPageRefreshStateChanged?.Invoke(true);
                return true;
            }
            return false;
        }

        public bool UseAdvert()
        {
            if (TimeTillFreeAd > 0)
                return false;
            OnFreeAdStateChanged?.Invoke(false);
            TimeTillFreeAd = AdTime;//do zmiany (to be changed)
            return true;
        }

        void Update()
        {
            if (TimeTillFreeAd >= 0)
            {
                TimeTillFreeAd -= Time.deltaTime;
                if (TimeTillFreeAd <= 0)
                {
                    OnFreeAdStateChanged?.Invoke(true);
                }
            }
            debugTime = TimeTillPageRefresh;
            if (TimeTillPageRefresh >= 0)
            {
                TimeTillPageRefresh -= Time.deltaTime;
                if (TimeTillPageRefresh <= 0)
                {
                    OnPageRefreshStateChanged?.Invoke(true);
                }
            }
        }

        public static int MAX_ORDERS = 16;

        public void TryRefreshGlobalOffers(OnActionCallback onSuccess = null, OnFailureCallback onFailure = null)
        {
            if (!CanRefreshGlobalOffers())
                return;

            bool isDelagatesSet = IsGetActualGlobalOffersDelegatesSet();
            OnSuccessGetActualGlobalOffers = onSuccess;
            OnFailureGetActualGlobalOffers = onFailure;
            if (isDelagatesSet)
                return;

            ExecuteRefreshGlobalOffers((loadFromServer) =>
            {
                LastRefreshTime = ServerTime.Get().GetUnixServerTime();
                TimeTillPageRefresh = GlobalOffersBoardIntervalDurationInSeconds;
                OnSuccessGetActualGlobalOffers?.Invoke();
                UnbindGetActualGlobalOffersDelegates();
            }, (ex) =>
            {
                OnFailureGetActualGlobalOffers?.Invoke(ex);
                UnbindGetActualGlobalOffersDelegates();
            });
        }

        private void ExecuteRefreshGlobalOffers(OnActionCallback onSuccess = null, OnFailureCallback onFailure = null)
        {
            GlobalOffersUseCase.Instance.GetOffers(() =>
            {
                orders = GlobalOffersUseCase.Instance.Offers;
                PharmacyManager.Instance.ProcessGetGlobalOffers(orders, (list) =>
                {
                    orders = list;
                    onSuccess?.Invoke();
                },
                (ex) =>
                {
                    Debug.LogError(ex.Message);
                    onFailure?.Invoke(ex);
                });
            },
            (ex) =>
            {
                Debug.LogError(ex.Message);
                onFailure?.Invoke(ex);
            });
        }

        public bool CanRefreshGlobalOffers()
        {
            return TimeTillPageRefresh <= 0 || orders == null;
        }

        public bool IsSpeedUpEnabled()
        {
            return TimeTillPageRefresh > 0;
        }

        public bool CanSpedUpGlobalOffers()
        {
            return Game.Instance.gameState().GetDiamondAmount() >= SpeedUpCost;
        }

        public void BoostPageRefresh(OnActionCallback onSuccess = null, OnFailureCallback onFailure = null)
        {
            bool isDelagatesSet = IsGetActualGlobalOffersDelegatesSet();
            OnSuccessGetActualGlobalOffers = onSuccess;
            OnFailureGetActualGlobalOffers = onFailure;
            if (isDelagatesSet)
                return;

            ExecuteRefreshGlobalOffers((loadFromServer) =>
            {
                LastRefreshTime = ServerTime.Get().GetUnixServerTime();
                TimeTillPageRefresh = GlobalOffersBoardIntervalDurationInSeconds;
                OnSuccessGetActualGlobalOffers?.Invoke();
                GameState.Get().RemoveDiamonds(ReferenceHolder.GetHospital().Pharmacy.SpeedUpCost, EconomySource.PharmacyRefresh); // No need to add diamond transaction as this was checked before callback.
                UnbindGetActualGlobalOffersDelegates();
            }, (ex) =>
            {
                OnFailureGetActualGlobalOffers?.Invoke(ex);
                UnbindGetActualGlobalOffersDelegates();
            });
        }

        public void TurnOnCrossGlowHideRoof()
        {
            crossGlow.SetActive(true);
            roof.SetActive(false);
        }

        private void GameEventsController_OnEventEnded()
        {
            SetHeartsDeactive();
        }

        public void TurnOffCrossGlowShowRoof()
        {
            crossGlow.SetActive(false);
            roof.SetActive(true);
        }

        public void OnLoad()
        {
            if (ReferenceHolder.GetHospital().globalEventController.IsGlobalEventActive() && ReferenceHolder.GetHospital().globalEventController.GlobalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsActive();
            else
                SetHeartsDeactive();

            AddListeners();

            if (HospitalAreasMapController.HospitalMap.VisitingMode)
            {
                TurnOnCrossGlowHideRoof();
            }
            else
            {
                if (Game.Instance.gameState().GetHospitalLevel() > 6)
                {
                    if (TutorialController.Instance.IsTutorialStepCompleted(StepTag.pharmacy_tap_to_open))
                    {
                        TurnOnCrossGlowHideRoof();
                    }
                    else
                    {
                        TurnOffCrossGlowShowRoof();
                    }
                }
                else
                {
                    TurnOffCrossGlowShowRoof();
                }
            }
        }

        private void AddListeners()
        {
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification += OnEventStart_Notification;

            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification += OnEventEnd_Notification;
        }

        void OnDestroy()
        {
            GlobalEventNotificationCenter.Instance.OnEventStart.Notification -= OnEventStart_Notification;
            GlobalEventNotificationCenter.Instance.OnEventEnd.Notification -= OnEventEnd_Notification;
        }

        private void OnEventEnd_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
        {
            if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsDeactive();
        }

        private void OnEventStart_Notification(GlobalEventOnStateChangeEventArgs eventArgs)
        {
            if (eventArgs.globalEventExtras == GlobalEvent.GlobalEventExtras.ValentineHearts)
                SetHeartsActive();
        }

        public void SetHeartsActive()
        {
            if (hearts != null)
            {
                if (!roof.activeSelf)
                    hearts.SetActive(true);
                else hearts.SetActive(false);
            }
        }

        public void SetHeartsDeactive()
        {
            if (hearts != null)
                hearts.SetActive(false);
        }

    }
}

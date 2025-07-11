using UnityEngine;
using UnityEngine.UI;
using SimpleUI;
using TMPro;

namespace Hospital
{
    public class ModifyOfferPopUp : UIElement
    {
        PharmacyOrder currentOrder;
        GameObject currentCard;

        public GameObject advertisedInfo;
        public GameObject notAdvertisedInfo;

        public Image itemIcon;
        public TextMeshProUGUI amountText;
        public TextMeshProUGUI costText;

        public TextMeshProUGUI FreeAdText;
        public TextMeshProUGUI Counter;
        public TextMeshProUGUI DeleteCost;

        public GameObject CreateAdvert;
        public GameObject SpeedUpAdvert;


        public void Open(PharmacyOrder order, GameObject card)
        {
            if (order.isLocalOffer)            
                return;

            PharmacyManager.Instance.PauseUserOffersRefresh();
            StartCoroutine(base.Open(true, true, () =>
            {
                currentOrder = order;
                currentCard = card;

                DeleteCost.text = ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost.ToString();
                SetItemInfo();
                SetAdvertised(order.GetType() == typeof(PharmacyOrderAdvertised) && !order.IsExpired());
                timer = 2;
            }));
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            UIController.getHospital.PharmacyPopUp.transform.SetAsLastSibling();
            PharmacyManager.Instance.ResumeUserOffersRefresh();
        }

        void SetItemInfo()
        {
            itemIcon.sprite = ResourcesHolder.Get().GetSpriteForCure(currentOrder.medicine);
            itemIcon.GetComponent<PointerDownListener>().SetDelegate(() =>
            {
                TextTooltip.Open(currentOrder.medicine, false, true);
            });

            costText.text = (currentOrder.pricePerUnit).ToString();
            amountText.text = currentOrder.amount.ToString();
        }

        void SetAdvertised(bool isAdvertised)
        {
            if (isAdvertised)
            {
                advertisedInfo.SetActive(true);
                notAdvertisedInfo.SetActive(false);
            }
            else
            {
                advertisedInfo.SetActive(false);
                notAdvertisedInfo.SetActive(true);
            }
        }

        public void DeleteOffer()
        {
            if (Game.Instance.gameState().GetDiamondAmount() >= ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(ReferenceHolder.GetHospital().Pharmacy.DeleteOfferCost, delegate
                {
                    UIController.getHospital.PharmacyPopUp.RemoveOffer(currentCard, currentOrder);
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void AdvertiseOffer()
        {
            // currentOrder.advertised = true;
            if (ReferenceHolder.GetHospital().Pharmacy.UseAdvert())
            {
                ReferenceHolder.GetHospital().Pharmacy.SetTimeTillFreeAd();
                UIController.getHospital.PharmacyPopUp.ModifyPlayerOffer(currentCard, currentOrder);
                Exit();
            }
        }

        float timer = 0;
        void Update()
        {
            timer += Time.deltaTime;

            if (timer > 1f)
            {
                timer = 0f;
                if (ReferenceHolder.GetHospital().Pharmacy.TimeTillFreeAd >= 0f)
                {
                    CreateAdvert.SetActive(false);
                    SpeedUpAdvert.SetActive(true);
                    Counter.text = UIController.GetFormattedTime((int)ReferenceHolder.GetHospital().Pharmacy.TimeTillFreeAd);
                    FreeAdText.text = "FREE AD IN:";
                }
                else
                {
                    CreateAdvert.SetActive(true);
                    SpeedUpAdvert.SetActive(false);
                    Counter.text = "";
                    FreeAdText.text = "";
                }
            }
        }

        public void SpeedUp()
        {
            int diaCost = ReferenceHolder.GetHospital().Pharmacy.SpeedUpCost;

            if (Game.Instance.gameState().GetDiamondAmount() >= diaCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diaCost, delegate
                {
                    GameState.Get().RemoveDiamonds(ReferenceHolder.GetHospital().Pharmacy.SpeedUpCost, EconomySource.PharmacyAdvert);
                    ReferenceHolder.GetHospital().Pharmacy.SetTimeTillFreeAd();
                    UIController.getHospital.PharmacyPopUp.ModifyPlayerOffer(currentCard, currentOrder);
                    Exit();
                }, this);
            }
            else
            {
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        public void ButtonExit()
        {
            Exit();
        }
    }
}


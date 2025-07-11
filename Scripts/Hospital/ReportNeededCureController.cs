using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Hospital;
using SimpleUI;
using System;

public class ReportNeededCureController : MonoBehaviour, IDiamondTransactionMaker
{
#pragma warning disable 0649
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI amountText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] Transform Button;
#pragma warning restore 0649
    private MedicineRef medicine;
    private int price;
    private int amount;

    private bool canClick;
    bool onNextLevelPopup;
    bool onReportPopup;
    private Guid ID;

    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    public void SetMedicine(MedicineRef medicine, int amount)
    {
        this.medicine = medicine;
        this.amount = amount;
        price = ResourcesHolder.Get().GetDiamondPriceForCure(medicine) * amount;
        Icon.sprite = ResourcesHolder.Get().GetSpriteForCure(medicine);
        amountText.text = ("X" + amount);
        priceText.text = price.ToString();
        canClick = true;
        onNextLevelPopup = UIController.getHospital.NextLevelPopUp.gameObject.activeSelf;
        onReportPopup = UIController.get.reportPopup.gameObject.activeSelf;
        InitializeID();
    }
    public void ShowTooltip()
    {
        if (medicine != null)
        {
            MedicineTooltip.Open(medicine);
        }
    }

    public void BuyMissingResource()
    {
        if (!canClick)
            return;

        if (Game.Instance.gameState().GetDiamondAmount() >= price)
        {
            DiamondTransactionController.Instance.AddDiamondTransaction(price, delegate
            {
                canClick = false;
                var gs = GameState.Get();
                bool isTank = medicine.IsMedicineForTankElixir();

                EconomySource economySource = EconomySource.MissingResources;
                gs.RemoveDiamonds(price, economySource);

                Canvas canvas = UIController.get.canvas;

                Vector2 startPoint = new Vector2((Icon.transform.position.x - Screen.width / 2) / canvas.transform.localScale.x, (Icon.transform.position.y - Screen.height / 2) / canvas.transform.localScale.y);

                gs.AddResource(medicine, amount, true, EconomySource.MissingResourcesBuy);
                HospitalAreasMapController.HospitalMap.hospitalBedController.UpdateAllBedsIndicators(true);

                UIController.get.storageCounter.Add(amount, isTank);
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.Medicine, startPoint, amount, 0, 1.75f, new Vector3(2, 2, 1), new Vector3(1, 1, 1), Icon.sprite, null, () =>
                {
                    UIController.get.storageCounter.Remove(amount, isTank);
                    if (onNextLevelPopup)
                    {
                        UIController.getHospital.NextLevelPopUp.RefreshCures();
                    }
                    if (onReportPopup)
                    {
                        UIController.get.reportPopup.SetCuresContent();
                    }
                });
                StartCoroutine(DelayedRefresh());

                if (onNextLevelPopup)
                {
                    UIController.getHospital.NextLevelPopUp.CheckCureReady(Button.position);
                }
                if (onReportPopup)
                {
                    UIController.get.reportPopup.CheckCureReady(Button.position);
                }
            }, this);
        }
        else
        {
            if (onNextLevelPopup)
                UIController.getHospital.NextLevelPopUp.Exit();
            if (onReportPopup)
                UIController.get.reportPopup.Exit();

            bool alreadyExecuted = false;
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
            UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds, () =>
            {
                if (onNextLevelPopup)
                {
                    alreadyExecuted = onReportPopup; // To prevent executing again this same Open once it is finished

                    StartCoroutine(UIController.getHospital.NextLevelPopUp.Open(true, false, () =>
                    {
                        UIController.getHospital.NextLevelPopUp.SetTabVisible(1);
                        if (onReportPopup)
                        {
                            StartCoroutine(UIController.get.reportPopup.Open());
                        }
                    }));
                }
                if (!alreadyExecuted && onReportPopup)
                {
                    StartCoroutine(UIController.get.reportPopup.Open());
                }
            });
        }
    }

    IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(0.25f);
        gameObject.SetActive(false);
    }

    public void InitializeID()
    {
        ID = Guid.NewGuid();
    }

    public Guid GetID()
    {
        return ID;
    }

    public void EraseID()
    {
        ID = Guid.Empty;
    }
}

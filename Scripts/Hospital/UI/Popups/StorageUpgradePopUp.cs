using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System;

namespace Hospital
{
    public class StorageUpgradePopUp : UIElement
    {
        #region localizationKeys
        //private string TitleKey = "TitleUpgrade";
        //private string InfoKey = "CureStorageToLvl";
        //private string StorageKey = "CureStorageMinus";
        //private string CapacityKey = "Capacity";
        //private string MaxLvlKey = "MaxLvl";
        //private string MaxKey = "MAX";
        #endregion

        #region localizationText
        public TextMeshProUGUI AmountText;
        public TextMeshProUGUI UpdateText;
        #endregion

        [SerializeField] Slider AmountIndicator = null;

        private OnEvent onUpgrade = null;

        // [SerializeField]
        // private CustomBarController CapacityBar = null;
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI[] itemAmountTexts;
        [SerializeField] private TextMeshProUGUI[] itemPriceTexts;
        [SerializeField] private GameObject[] itemMissing;
        [SerializeField] private GameObject[] itemGotIt;

        [SerializeField] private Image itemImage1;
        [SerializeField] private Image itemImage2;
        [SerializeField] private Image itemImage3;
#pragma warning restore 0649
        [SerializeField] private Button UpgradeButton = null;

        private int[] actualAmounts = new int[3];
        private readonly MedicineRef[] itemRefs = {
            new MedicineRef(MedicineType.Special, 0),
            new MedicineRef(MedicineType.Special, 1),
            new MedicineRef(MedicineType.Special, 2),
            new MedicineRef(MedicineType.Special, 4),
            new MedicineRef(MedicineType.Special, 5),
            new MedicineRef(MedicineType.Special, 6),
        };

        private bool isElixirTank = false;
        private int CurenltyItemIDSendingDiamondTransaction;

        public void Upgrade()
        {
            UpgradeButton.interactable = false;
            IGameState gs = Game.Instance.gameState();
            MedicineRef[] items = new MedicineRef[3];

            int amountNeeded;

            if (this.isElixirTank)
            {
                items[0] = itemRefs[3];    //gum
                items[1] = itemRefs[4];    //metal
                items[2] = itemRefs[5];   //pipe
                amountNeeded = gs.ElixirTank.actualLevel;
            }
            else
            {
                items[0] = itemRefs[0];    //spanner
                items[1] = itemRefs[1];    //hammer
                items[2] = itemRefs[2];    //screwdriver
                amountNeeded = gs.ElixirStore.actualLevel;
            }

            int firstItemDifference = amountNeeded - gs.GetCureCount(items[0]);
            int secondItemDifference = amountNeeded - gs.GetCureCount(items[1]);
            int thirdItemDifference = amountNeeded - gs.GetCureCount(items[2]);

            if (firstItemDifference > 0 || secondItemDifference > 0 || thirdItemDifference > 0)
            {
                List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

                if (firstItemDifference > 0)
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(firstItemDifference, ResourcesHolder.Get().medicines.cures[(int)items[0].type].medicines[items[0].id]));//ResourcesHolder.Get().medicines.cures[(int)(MedicineType.Special)].medicines[2]));

                if (secondItemDifference > 0)
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(secondItemDifference, ResourcesHolder.Get().medicines.cures[(int)items[1].type].medicines[items[1].id]));//ResourcesHolder.Get().medicines.cures[(int)(MedicineType.Special)].medicines[1]));

                if (thirdItemDifference > 0)
                    missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(thirdItemDifference, ResourcesHolder.Get().medicines.cures[(int)items[2].type].medicines[items[2].id]));//ResourcesHolder.Get().medicines.cures[(int)(MedicineType.Special)].medicines[0]));

                UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                {
                    //Upgrade();
                    ActualizeInfo(false);
                    UpgradeButton.interactable = true;
                }, () =>
                {
                    UpgradeButton.interactable = true;
                }, null);
            }
            else
            {
                gs.GetCure(items[0], amountNeeded, EconomySource.StorageUpgrade);
                gs.GetCure(items[1], amountNeeded, EconomySource.StorageUpgrade);
                gs.GetCure(items[2], amountNeeded, EconomySource.StorageUpgrade);

                if (this.isElixirTank)
                    gs.ElixirTank.Upgrade(1);
                else
                    gs.ElixirStore.Upgrade(1);

                ExitAfterUpgrade();

                onUpgrade?.Invoke();
            }
        }

        public void BuyMissing(int itemID)
        {
            if (this.isElixirTank)
                itemID += 3;
            if (CurenltyItemIDSendingDiamondTransaction != itemID)
            {
                CurenltyItemIDSendingDiamondTransaction = itemID;
                InitializeID();
            }
            IGameState gs = Game.Instance.gameState();
            MedicineRef extraMedicine = itemRefs[itemID];

            int neededCounter, diamondPrice;

            if (this.isElixirTank)
            {
                neededCounter = gs.ElixirTank.actualLevel - gs.GetCureCount(extraMedicine);
                diamondPrice = ResourcesHolder.Get().medicines.cures[(int)extraMedicine.type].medicines[extraMedicine.id].diamondPrice * neededCounter;

                if (gs.GetDiamondAmount() >= diamondPrice)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(diamondPrice, delegate
                    {
                        UIController.get.storageCounter.Add(false);
                        gs.RemoveDiamonds(diamondPrice, EconomySource.MissingResourcesStorage);
                        gs.AddResource(extraMedicine, neededCounter, true, EconomySource.StorageUpgrade);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), neededCounter, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(extraMedicine), null, () =>
                         {
                             UIController.get.storageCounter.Remove(neededCounter, false);
                         });

                        ActualizeInfo(false);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
            else
            {
                neededCounter = gs.ElixirStore.actualLevel - GameState.Get().GetCureCount(extraMedicine);
                diamondPrice = ResourcesHolder.Get().medicines.cures[(int)extraMedicine.type].medicines[extraMedicine.id].diamondPrice * neededCounter;

                if (gs.GetDiamondAmount() >= diamondPrice)
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(diamondPrice, delegate
                    {
                        UIController.get.storageCounter.Add(false);
                        gs.RemoveDiamonds(diamondPrice, EconomySource.MissingResourcesStorage);
                        gs.AddResource(extraMedicine, neededCounter, true, EconomySource.StorageUpgrade);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Special, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), neededCounter, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ResourcesHolder.Get().GetSpriteForCure(extraMedicine), null, () =>
                         {
                             UIController.get.storageCounter.Remove(neededCounter, false);
                         });

                        ActualizeInfo(false);
                    }, this);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
            }
        }

        public void ActualiseIndicator()
        {
            if (this.isElixirTank)
            {
                AmountIndicator.value = Game.Instance.gameState().ElixirTank.actualAmount / (float)Game.Instance.gameState().ElixirTank.maximumAmount;
                AmountText.text = I2.Loc.ScriptLocalization.Get("CREATE_OFFER_TITLE_TANK") + ": " + Game.Instance.gameState().ElixirTank.actualAmount.ToString() + " /" + Game.Instance.gameState().ElixirTank.maximumAmount.ToString();
            }
            else
            {
                AmountIndicator.value = Game.Instance.gameState().ElixirStore.actualAmount / (float)Game.Instance.gameState().ElixirStore.maximumAmount;
                AmountText.text = I2.Loc.ScriptLocalization.Get("CURE_STORAGE_U") + ": " + Game.Instance.gameState().ElixirStore.actualAmount.ToString() + " /" + Game.Instance.gameState().ElixirStore.maximumAmount.ToString();
            }
        }

        void ActualizeInfo(bool onOpen)
        {
            InitializeID();
            int actLvl, maxLvl, amountOnLvl;
            if (this.isElixirTank)
            {
                actLvl = Game.Instance.gameState().ElixirTank.actualLevel;
                maxLvl = Game.Instance.gameState().ElixirTank.GetMaxLVL();
                amountOnLvl = Game.Instance.gameState().ElixirTank.GetAmountOnLevel(true);
            }
            else
            {
                actLvl = Game.Instance.gameState().ElixirStore.actualLevel;
                maxLvl = Game.Instance.gameState().ElixirStore.GetMaxLVL();
                amountOnLvl = Game.Instance.gameState().ElixirStore.GetAmountOnLevel(true);
            }

            if (actLvl < maxLvl)
            {
                if (this.isElixirTank)
                    UpdateText.text = I2.Loc.ScriptLocalization.Get("INCREASE_TANK") + " " + amountOnLvl;
                else
                    UpdateText.text = I2.Loc.ScriptLocalization.Get("INCREASE_STORAGE") + " " + amountOnLvl;
            }
            else
                UpdateText.text = I2.Loc.ScriptLocalization.Get("MAX");

            ActualiseIndicator();
            UpdateItemsNeededInfo(onOpen);
            SetUpgradeButton();
            //tutaj jakieś wypełnianie bara
            ActualiseIndicator();
        }

        void UpdateItemsNeededInfo(bool onOpen)
        {
            IGameState gs = Game.Instance.gameState();
            MedicineRef[] items = new MedicineRef[3];

            int amountNeeded, maxLevelAmount;

            if (this.isElixirTank)
            {
                items[0] = itemRefs[3];    //gum
                items[1] = itemRefs[4];    //metal
                items[2] = itemRefs[5];    //pipe
                amountNeeded = gs.ElixirTank.actualLevel;
                maxLevelAmount = gs.ElixirTank.GetMaxLVL();
            }
            else
            {
                items[0] = itemRefs[0];    //spanner
                items[1] = itemRefs[1];    //hammer
                items[2] = itemRefs[2];    //screwdriver
                amountNeeded = gs.ElixirStore.actualLevel;
                maxLevelAmount = gs.ElixirStore.GetMaxLVL();
            }

            if (onOpen)
            {
                this.itemImage1.sprite = ResourcesHolder.Get().GetSpriteForCure(items[0]);
                this.itemImage2.sprite = ResourcesHolder.Get().GetSpriteForCure(items[1]);
                this.itemImage3.sprite = ResourcesHolder.Get().GetSpriteForCure(items[2]);
            }

            for (int i = 0; i < 3; i++)
            {
                if (amountNeeded < maxLevelAmount)
                {
                    actualAmounts[i] = gs.GetCureCount(items[i]);
                    Debug.LogWarning("Actual amount for item " + i + " = " + actualAmounts[i]);
                    itemAmountTexts[i].text = actualAmounts[i] + "/" + amountNeeded;
                    itemPriceTexts[i].text = (ResourcesHolder.Get().medicines.cures[(int)items[i].type].medicines[items[i].id].diamondPrice * (amountNeeded - gs.GetCureCount(items[i]))).ToString();

                    if (actualAmounts[i] >= amountNeeded)
                    {
                        itemMissing[i].SetActive(false);
                        itemGotIt[i].SetActive(true);
                        itemAmountTexts[i].color = new Color(.157f, .165f, .165f);
                    }
                    else
                    {
                        itemMissing[i].SetActive(true);
                        itemGotIt[i].SetActive(false);
                        itemAmountTexts[i].color = Color.red;
                    }
                }
                else
                {
                    itemAmountTexts[i].text = (amountNeeded - 1).ToString() + "/" + (amountNeeded - 1).ToString();
                    itemMissing[i].SetActive(false);
                    itemGotIt[i].SetActive(true);
                    itemAmountTexts[i].color = new Color(.157f, .165f, .165f);
                }
            }
        }

        public void Open(OnEvent onUpgrade, bool isElixirTank)
        {
            UpgradeButton.interactable = true;
            this.isElixirTank = isElixirTank;
            //Debug.LogError("Open OnEvent");
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, () =>
            {
                this.onUpgrade = onUpgrade;
                ActualizeInfo(true);
            }));
        }

        public void Open(bool isElixirTank)
        {
            UpgradeButton.interactable = true;
            this.isElixirTank = isElixirTank;
            //Debug.LogError("Open OnEvent");
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, () =>
            {
                ActualizeInfo(true);
            }));
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            UpgradeButton.interactable = true;
            //Debug.LogError("Open overrided");
            yield return base.Open();
            ActualizeInfo(true);
            whenDone?.Invoke();
        }

        void SetUpgradeButton()
        {
            if (this.isElixirTank)
            {
                if (Game.Instance.gameState().ElixirTank.GetMaxLVL() == Game.Instance.gameState().ElixirTank.actualLevel)
                {
                    UpgradeButton.interactable = false;
                    UpgradeButton.enabled = false;
                    UpgradeButton.GetComponent<Image>().material = ResourcesHolder.Get().GrayscaleMaterial;
                }
            }
            else if (Game.Instance.gameState().ElixirStore.GetMaxLVL() == Game.Instance.gameState().ElixirStore.actualLevel)
            {
                UpgradeButton.interactable = false;
                UpgradeButton.enabled = false;
                UpgradeButton.GetComponent<Image>().material = ResourcesHolder.Get().GrayscaleMaterial;
            }
        }

        public void ButtonExit()
        {
            Exit();
        }

        public void ExitAfterUpgrade()
        {
            base.Exit();
        }
    }
}

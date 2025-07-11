using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Hospital
{
    public abstract class BaseUpgradeConfirmPopup : UIElement
    {
        [SerializeField] protected Button UpgradeButton;
        [SerializeField] protected Transform Content;
        [SerializeField] protected UpgradeItemUI CardPrefab;
        [SerializeField] protected EconomySource diamondSpendEconomySource;
        [SerializeField] protected EconomySource addResourceEconomySource;

        private OnEvent onUpgrade = null;

        private List<Card> cards = new List<Card>();

        public virtual IEnumerator Open(OnEvent onUpgrade, List<CardModel> cardModels, Action whenDone = null)
        {
            yield return base.Open(false);
            this.onUpgrade = onUpgrade;
            Init(cardModels);

            whenDone?.Invoke();
        }

        public virtual void Open(OnEvent onUpgrade, List<CardModel> cardModels, VitaminCollectorModel model)
        {
            StartCoroutine(this.Open(onUpgrade, cardModels));
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            // gameObject.transform.SetAsLastSibling();
            onUpgrade = null;
        }

        public void ButtonExit()
        {
            Exit();
        }

        public virtual void Upgrade()
        {
            List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines = new List<KeyValuePair<int, MedicineDatabaseEntry>>();
            int missingPositiveEnergy = -1;
            foreach (Card card in cards)
            {
                if (card.model is MedicineCardModel)
                {
                    if (!card.model.HaveEnought())
                        missingMedicines.Add(new KeyValuePair<int, MedicineDatabaseEntry>(card.model.RequiredAmount - card.model.CurrentAmount, ResourcesHolder.Get().GetMedicineInfos(((MedicineCardModel)card.model).med)));
                    continue;
                }
                if (card.model is PositiveEnergyCardModel)
                {
                    if (!card.model.HaveEnought())
                        missingPositiveEnergy = Math.Max(card.model.RequiredAmount - card.model.CurrentAmount, 0);
                    continue;
                }
            }
            if (missingMedicines.Count > 0 || missingPositiveEnergy > 0)
            {
                UIController.get.BuyResourcesPopUp.Open(missingMedicines, false, false, false, () =>
                {
                    foreach (Card card in cards)
                    {
                        card.model.UpdateCurrent();
                        card.Refresh();
                    }
                }, null, null, 1, false, missingPositiveEnergy);
            }
            else
            {
                foreach (Card card in cards)
                {
                    card.Consume();
                }
                NotifyUpgraded();
                Exit();
            }
        }

        private void NotifyUpgraded()
        {
            onUpgrade?.Invoke();
        }

        private void Init(List<CardModel> cardModels)
        {
            ClearContent();
            foreach (CardModel model in cardModels)
            {
                Card card = new Card();
                card.diamondSpendEconomySource = diamondSpendEconomySource;
                card.addResourceEconomySource = addResourceEconomySource;
                card.model = model;
                card.ui = Instantiate(CardPrefab, Content) as UpgradeItemUI;
                card.Refresh();
                cards.Add(card);
            }
        }

        private void ClearContent()
        {
            foreach (Card card in cards)
            {
                Destroy(card.ui.gameObject);
            }
            cards.Clear();
        }

        public class Card : IDiamondTransactionMaker
        {
            public CardModel model;
            public UpgradeItemUI ui;
            private Guid ID;

            public EconomySource diamondSpendEconomySource;
            public EconomySource addResourceEconomySource;

            public Card()
            {
                InitializeID();
            }

            public void Refresh()
            {
                ui.SetImage(model.GetSprite());
                ui.SetGotIt(model.HaveEnought());
                ui.SetAmount(model.CurrentAmount + "/" + model.RequiredAmount);
                ui.buyMissingButton.onClick.RemoveAllListeners();
                if (!model.HaveEnought())
                {
                    ui.buyMissingButton.onClick.AddListener(OnMissingClick);
                    ui.SetPrice(model.GetPrice());
                }
            }

            public void Consume()
            {
                model.Consume(addResourceEconomySource);
            }

            private void OnMissingClick()
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= model.GetPrice())
                {
                    DiamondTransactionController.Instance.AddDiamondTransaction(model.GetPrice(), delegate
                    {
                        model.BuyMissing(diamondSpendEconomySource, addResourceEconomySource);
                        Refresh();
                    }, this);

                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                }
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

        #region Card Models

        public class MedicineCardModel : CardModel
        {
            public MedicineRef med;

            public MedicineCardModel(int requiredAmount, MedicineRef med) : base(requiredAmount)
            {
                this.med = med;
                UpdateCurrent();
            }

            public override void UpdateCurrent()
            {
                CurrentAmount = Math.Min(RequiredAmount, GameState.Get().GetCureCount(med));
            }

            public override void Consume(EconomySource addResourceEconomySource)
            {
                GameState.Get().GetCure(med, RequiredAmount, EconomySource.StorageUpgrade);
            }

            public override Sprite GetSprite()
            {
                return ResourcesHolder.Get().GetSpriteForCure(med);
            }

            public override int GetPrice()
            {
                return ResourcesHolder.Get().GetMedicineInfos(med).diamondPrice * (RequiredAmount - CurrentAmount);
            }

            protected override void Collect(EconomySource addResourceEconomySource, int amount)
            {
                bool isTankTool = med.IsMedicineForTankElixir();
                UIController.get.storageCounter.Add(isTankTool);
                GameState.Get().AddResource(med, amount, true, addResourceEconomySource);
                ReferenceHolder.Get().giftSystem.CreateGiftParticle(med.type == MedicineType.Special ? GiftType.Special : GiftType.Medicine, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), amount, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), GetSprite(), null, () =>
                {
                    UIController.get.storageCounter.Remove(amount, isTankTool);
                });
            }

        }

        public class PositiveEnergyCardModel : CardModel
        {
            public PositiveEnergyCardModel(int requiredAmount) : base(requiredAmount)
            {
                UpdateCurrent();
            }

            public override void UpdateCurrent()
            {
                CurrentAmount = Math.Min(RequiredAmount, GameState.Get().PositiveEnergyAmount);
            }

            public override void Consume(EconomySource addResourceEconomySource)
            {
                GameState.Get().RemovePositiveEnergy(RequiredAmount, addResourceEconomySource);
            }

            public override Sprite GetSprite()
            {
                return ResourcesHolder.Get().PESprite;
            }

            public override int GetPrice()
            {
                return ResourcesHolder.Get().GetMedicineInfos(new MedicineRef(MedicineType.Fake, 0)).diamondPrice * (RequiredAmount - CurrentAmount);
            }

            protected override void Collect(EconomySource addResourceEconomySource, int amount)
            {
                ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(GiftType.PositiveEnergy, new Vector2(0, 0), amount, .5f, 1.75f, Vector3.one, new Vector3(1, 1, 1), ReferenceHolder.Get().giftSystem.particleSprites[4]);
                GameState.Get().AddResource(ResourceType.PositiveEnergy, amount, addResourceEconomySource, true);
            }

        }

        public abstract class CardModel
        {
            public int CurrentAmount { get; protected set; }
            public int RequiredAmount { get; private set; }

            public bool HaveEnought()
            {
                return CurrentAmount >= RequiredAmount;
            }

            public CardModel(int requiredAmount)
            {
                RequiredAmount = requiredAmount;
            }

            public abstract Sprite GetSprite();
            public abstract void Consume(EconomySource addResourceEconomySource);
            public abstract int GetPrice();
            protected abstract void Collect(EconomySource addResourceEconomySource, int amount);
            public abstract void UpdateCurrent();

            public void BuyMissing(EconomySource diamondSpendEconomySource, EconomySource addResourceEconomySource)
            {
                int amount = RequiredAmount - CurrentAmount;
                GameState.Get().RemoveDiamonds(GetPrice(), diamondSpendEconomySource);
                Collect(addResourceEconomySource, amount);
                UpdateCurrent();
            }
        }

        #endregion

    }
}

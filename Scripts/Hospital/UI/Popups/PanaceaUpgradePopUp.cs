using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{

    public class PanaceaUpgradePopUp : UIElement
    {
        [SerializeField] private TextMeshProUGUI InfoText = null;
        [SerializeField] private Image InfoImage = null;
        [SerializeField] private TextMeshProUGUI UpgradeCost = null;
        [SerializeField] private TextMeshProUGUI CapacityFromAmountText = null;
        [SerializeField] private TextMeshProUGUI CapacityToAmountText = null;
        [SerializeField] private TextMeshProUGUI CollectRateFromAmountText = null;
        [SerializeField] private TextMeshProUGUI CollectRateToAmountText = null;

        [SerializeField] private Button upgradeButton = null;

        PanaceaCollector collector;
        OnEvent onUpgrade;

        [TutorialTrigger]
        public event System.EventHandler panaceaCollectorUpgraded;

        public void Open(PanaceaCollector collector, OnEvent onUpgrade)
        {
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, true, () =>
            {
                this.onUpgrade = onUpgrade;
                this.collector = collector;
                Refresh();
            }));
        }

        void Refresh()
        {
            upgradeButton.interactable = true;
            SetInfoText();
            SetInfoImage();
            SetUpgradeCost();
            SetValues();
            TutorialUIController.Instance.StopBlinking();
        }

        public void SetInfoText()
        {
            InfoText.text = I2.Loc.ScriptLocalization.Get("PANACEA_COLLECTOR_UPGRADE") + " " + ((collector.actualLevel) + 1).ToString().ToUpper();
        }

        void SetInfoImage()
        {
            InfoImage.sprite = collector.GetCollectorData().infoScreenSprites[Mathf.Clamp(collector.GetActualLevel(), 1, collector.GetMaxLVL() - 1)];
        }

        void SetUpgradeCost()
        {
            int cost = collector.GetCollectorData().Levels[collector.actualLevel].UpgradeCost;
            UpgradeCost.text = cost > 0 ? cost.ToString() : "FREE";// - 1].UpgradeCost.ToString();
        }

        private void SetValues()
        {
            if (collector.GetActualLevel() >= collector.GetMaxLVL())
            {
                Debug.LogError("COLLECTOR IS ALREADY MAX LEVEL. THIS SHOULD NOT HAPPEN.");
                return;
            }

            CapacityFromAmountText.text = collector.GetCapacityPerLevel(false).ToString();
            CapacityToAmountText.text = collector.GetCapacityPerLevel(true).ToString();
            CollectRateFromAmountText.text = collector.GetCollectionRatePerLevel(false).ToString();
            CollectRateToAmountText.text = collector.GetCollectionRatePerLevel(true).ToString() + " / h";
        }

        public void Upgrade()
        {
            upgradeButton.interactable = false;

            if (collector.GetActualLevel() <= collector.GetMaxLVL())
            {
                var upgradeCost = collector.GetCollectorData().Levels[collector.actualLevel].UpgradeCost;// - 1].UpgradeCost;
                if (Game.Instance.gameState().GetCoinAmount() >= upgradeCost)
                {
                    GameState.Get().RemoveCoins(upgradeCost, EconomySource.PanaceaUpgrade);
                    SoundsController.Instance.PlayObjectUpgrade();
                    collector.Upgrade(1);
                    //Refresh();
                    collector.SetPanaceaFull();
                    //Achievement
                    AchievementNotificationCenter.Instance.PanaceaCollectorUpgraded.Invoke(new AchievementProgressEventArgs(1));

                    if (panaceaCollectorUpgraded != null)
                        panaceaCollectorUpgraded(this, null);
                    /*	int expReward = collector.GetCollectorData().Levels[collector.actualLevel - 1].UpgradeExp;
                        GameState.Get().AddResource(ResourceType.Exp, expReward, EconomySource.PanaceaUpgrade, false);
                        ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, collector.transform.position, expReward, 0f, 1.75f,Vector3.one, new Vector3 (1, 1, 1), null, null, () =>
                        {
                            GameState.Get().UpdateCounter(ResourceType.Exp, expReward);
                        });*/

                    ExitAfterUpgrade();

                    onUpgrade?.Invoke();
                }
                else
                {
                    UIController.get.BuyResourcesPopUp.Open(upgradeCost - Game.Instance.gameState().GetCoinAmount(), () =>
                    {
                        Upgrade();
                    }, () =>
                    {
                        upgradeButton.interactable = true;
                    });
                }
            }
        }

        public void ButtonExit()
        {
            Exit();
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
            HospitalAreasMapController.HospitalMap.ResetOnPressAction();
            UIController.getHospital.PanaceaPopUp.Open(collector);
        }

        public void ExitAfterUpgrade()
        {
            base.Exit();
            ReferenceHolder.Get().engine.MainCamera.BlockUserInput = false;
            HospitalAreasMapController.HospitalMap.ResetOnPressAction();
            // UIController.get.PanaceaPopUp.Open(collector);
        }
    }
}
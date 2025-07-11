using UnityEngine;
using SimpleUI;
using UnityEngine.UI;
using TMPro;

namespace Hospital
{
    public class PanaceaPopUp : UIElement
    {
#pragma warning disable 0649
        [SerializeField] private TextMeshProUGUI LevelText;
#pragma warning restore 0649
        [SerializeField] private TextMeshProUGUI RateValueText = null;
        [SerializeField] private TextMeshProUGUI CapacityValuesText = null;
        [SerializeField] private Image CapacityBarFill = null;
        [SerializeField] private Image InfoImage = null;
        [SerializeField] private Button UpgradeButton = null;
        [SerializeField] private Sprite upgradeSpriteActive = null;
        [SerializeField] private Sprite upgradeSpriteInactive = null;

        [SerializeField] private GameObject UpgradeInfo = null;
        [SerializeField] private GameObject UpgradeMaxLevelReached = null;
        PanaceaCollector collector;

        public void Open(PanaceaCollector collector)
        {
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, true, () =>
            {
                this.collector = collector;
                //HintsController.Get().TryToHideIndicator();
                LevelText.text = (I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + collector.actualLevel);
                if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.lab_collector_tap && GetUpgradeButton().GetComponent<Image>() != null)
                {
                    TutorialUIController.Instance.BlinkImage(GetUpgradeButton().GetComponent<Image>());
                    //TutorialUIController.Instance.ShowTutorialArrowUIAfterPopupOpens(this.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForPanaceaUpgrade, this);
                }
                Refresh();
            }));
        }

        public void Refresh()
        {
            CapacityValuesText.text = collector.actualAmount + "/" + collector.maximumAmount;
            RateValueText.text = collector.GetCollectionRatePerLevel() + " / h";

            SetFillBars();
            SetUpgradeButton();
            SetInfoImage();
        }

        [TutorialCondition]
        public bool PanaceaLevelReached(int lvl)
        {
            if (collector == null)
                collector = GameState.Get().PanaceaCollector;

            return collector.actualLevel >= lvl;
        }

        void SetFillBars()
        {
            float collectionFrac = (float)collector.actualAmount / (float)collector.maximumAmount;
            CapacityBarFill.fillAmount = collectionFrac;
        }

        void SetUpgradeButton()
        {
            if (collector.actualLevel >= collector.GetMaxLVL())
            {
                UpgradeButton.gameObject.SetActive(false);
                UpgradeInfo.SetActive(false);
                UpgradeMaxLevelReached.SetActive(true);
            }
            else
            {
                UpgradeButton.gameObject.SetActive(true);
                UpgradeInfo.SetActive(true);
                UpgradeMaxLevelReached.SetActive(false);

                if (TutorialController.Instance.CurrentTutorialStepIndex < TutorialController.Instance.GetStepId(StepTag.lab_collector_tap))
                    UpgradeButton.image.sprite = upgradeSpriteInactive;
                else
                    UpgradeButton.image.sprite = upgradeSpriteActive;
            }
        }

        void SetInfoImage()
        {
            InfoImage.sprite = collector.GetCollectorData().infoScreenSprites[Mathf.Clamp(collector.GetActualLevel(), 0, collector.GetCollectorData().infoScreenSprites.Count - 1)];// - 1];
        }

        public void ButtonUpgrade()
        {
            if (TutorialController.Instance.CurrentTutorialStepIndex >= TutorialController.Instance.GetStepId(StepTag.lab_collector_tap))
            {
                Exit();

                UIController.getHospital.PanaceaUpgradePopUp.Open(collector, () =>
                {
                    var fp = (GameObject)Instantiate(ResourcesHolder.Get().ParticleUnpack, new Vector3(collector.position.x + collector.actualData.rotationPoint.x, 0, collector.position.y + collector.actualData.rotationPoint.y) + new Vector3(-5, 5 * Mathf.Sqrt(2), -5), Quaternion.Euler(0, 0, 0));
                    fp.transform.localScale = Vector3.one * 1.4f;
                    fp.SetActive(true);
                    base.Exit();
                });
            }
            else
                MessageController.instance.ShowMessage(46);
        }

        public void ButtonExit()
        {
            Exit();
        }

        public GameObject GetUpgradeButton()
        {
            return UpgradeButton.gameObject;
        }
    }
}

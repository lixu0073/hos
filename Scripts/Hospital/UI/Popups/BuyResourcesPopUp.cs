using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine.UI;
using SimpleUI;

namespace Hospital
{
    public class BuyResourcesPopUp : UIElement
    {
        private List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines;
        [SerializeField] private RectTransform content = null;
        [SerializeField] private TextMeshProUGUI cost = null;
        [SerializeField] private GameObject itemPrefab = null;
        [SerializeField] private CanvasGroup canvasGroup = null;

        private int diamondCost;
        private int missingCoins;
        private int missingPositive;
        private OnEvent onBought = null;
        private OnEvent onResigned = null;
        private bool isCoins = false;
        private bool isPositive = false;
        private bool offerUsed = false;
        private bool isHidden = false;

        #region Treatment Help
        [SerializeField] private Button askFriendsButton = null;
        [SerializeField] private TextMeshProUGUI askFriendsText = null;
        [SerializeField] private TextMeshProUGUI infoText = null;

        private Color defaultFaceColor;
        private Color defaultOutlineColor;
        private Color defaultUnderlayColor;
        #endregion

        private void Awake()
        {
            defaultFaceColor = askFriendsText.fontMaterial.GetColor(ShaderUtilities.ID_FaceColor);
            defaultOutlineColor = askFriendsText.fontMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
            defaultUnderlayColor = askFriendsText.fontMaterial.GetColor(ShaderUtilities.ID_UnderlayColor);
        }

        public void Open(List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines, bool requiresDiagnosis, bool InDiagnosisState, bool canRequestHelp, OnEvent onBought,
                         OnEvent onResigned, UnityAction onHelpRequested, float modifier = 1f, bool showAskFriendsButton = false, int missingPositiveEnergy = -1, int _missingCoins = -1)
        {
            TreatmentRoomHelpController.onRefresh -= RefreshHelpRequest;
            TreatmentRoomHelpController.onRefresh += RefreshHelpRequest;

            Debug.Log("BuyResourcesPopUp Open.");
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingResources;

            var res = (UIController.getHospital != null && (UIController.getHospital.PatientCard.isActiveAndEnabled || UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)) ||
                      (UIController.getMaternity != null && (UIController.getMaternity.patientCardController.isActiveAndEnabled));

            gameObject.SetActive(true);
            StartCoroutine(base.Open(!res, !res, OnPostOpen(missingMedicines, requiresDiagnosis, InDiagnosisState, canRequestHelp, onBought, onResigned, onHelpRequested, modifier, showAskFriendsButton, missingPositiveEnergy, _missingCoins)));
        }

        private Action OnPostOpen(List<KeyValuePair<int, MedicineDatabaseEntry>> missingMedicines, bool requiresDiagnosis, bool InDiagnosisState, bool canRequestHelp, OnEvent onBought,
                         OnEvent onResigned, UnityAction onHelpRequested, float modifier = 1f, bool showAskFriendsButton = false, int missingPositiveEnergy = -1, int _missingCoins = -1)
        {
            Initialize(missingMedicines, requiresDiagnosis, InDiagnosisState, onBought, onResigned, modifier, missingPositiveEnergy, _missingCoins);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            if (requiresDiagnosis)
                SetAskFriendsButtonActive(false);
            else
                SetAskFriendsButtonActive(showAskFriendsButton);
            bool canBeRequested = false;

            if (ReferenceHolder.GetHospital() != null)            
                canBeRequested = ReferenceHolder.GetHospital().treatmentRoomHelpController.CheckIfHelpRequestPossible() && canRequestHelp;
            
            SetAskFriendsRequestsButton(onHelpRequested, !canBeRequested);
            SetInfoText(canBeRequested);

            return null;
        }

        private void RefreshHelpRequest()
        {
            if (ReferenceHolder.GetHospital() != null && UIController.get.BuyResourcesPopUp.gameObject.activeSelf)
            {
                bool canBeRequested = ReferenceHolder.GetHospital().treatmentRoomHelpController.CheckIfHelpRequestPossible();
                SetAskFriendsButtonGrayscale(!canBeRequested);
            }
        }

        public void Open(int missingAmount, OnEvent onBought, OnEvent onResigned, missingResourceType type = missingResourceType.coin)
        {
            Debug.Log("BuyResourcesPopUp Open.");
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingResources;

            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, true, () =>
            {
                Initialize(missingAmount, onBought, onResigned, type);
                SetAskFriendsButtonActive(false);
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
                SetInfoText(false);
            }));
        }

        //for medicines
        void Initialize(List<KeyValuePair<int, MedicineDatabaseEntry>> missing, bool requiresDiagnosis, bool InDiagnosisState, OnEvent onBought, OnEvent onResigned, float modifier = 1f, int missingPositiveEnergy = -1, int _missingCoins = -1)
        {
            this.onBought = onBought;
            this.onResigned = onResigned;

            isCoins = false;
            isPositive = false;
            offerUsed = false;
            missingCoins = 0;
            missingPositive = 0;

            diamondCost = 0;
            missingMedicines = missing;

            foreach (var medicine in missing)
            {
                GameObject temp = Instantiate(itemPrefab);
                temp.transform.SetParent(content);
                temp.transform.localScale = Vector3.one;

                DiseaseType dt = DiseaseType.None;
                if (medicine.Value.Disease != null)
                    dt = medicine.Value.Disease.DiseaseType;
                if ((requiresDiagnosis || InDiagnosisState) && medicine.Value.IsDiagnosisMedicine())
                {
                    AddDiagnoseCost(dt, InDiagnosisState);
                    temp.GetComponent<Image>().sprite = UIController.getHospital.PatientCard.GetDiagnosisSprite(medicine.Value.Disease.DiseaseType);
                    temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = "";
                }
                else
                {
                    temp.GetComponent<Image>().sprite = medicine.Value.image;
                    temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = medicine.Key.ToString();
                }

                diamondCost += medicine.Value.diamondPrice * medicine.Key;
            }

            int positiveEnergydiamondCost = 0;
            if (missingPositiveEnergy > 0)
            {
                GameObject temp = Instantiate(itemPrefab);
                temp.transform.SetParent(content);
                temp.transform.localScale = Vector3.one;

                isPositive = true;
                missingPositive = missingPositiveEnergy;
                temp.GetComponent<Image>().sprite = ReferenceHolder.Get().giftSystem.particleSprites[4];
                positiveEnergydiamondCost = DiamondCostCalculator.GetMissingPositiveCost(missingPositive);
                temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = missingPositive.ToString();
            }

            int coinsInDiamondCost = 0;
            if (_missingCoins > 0)
            {
                GameObject temp = Instantiate(itemPrefab);
                temp.transform.SetParent(content);
                temp.transform.localScale = Vector3.one;

                isCoins = true;
                temp.GetComponent<Image>().sprite = ReferenceHolder.Get().giftSystem.particleSprites[0];
                missingCoins = _missingCoins;
                coinsInDiamondCost = DiamondCostCalculator.GetMissingCoinsCost(missingCoins);
                temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = missingCoins.ToString();
            }

            diamondCost = (int)(diamondCost * modifier);
            diamondCost += positiveEnergydiamondCost;
            diamondCost += coinsInDiamondCost;

            cost.text = diamondCost.ToString();
            UnHide();
        }

        void AddDiagnoseCost(DiseaseType dt, bool InDiagnosisState)
        {
            HospitalDataHolder hdh = HospitalDataHolder.Instance;
            int diagnoseTime = 0;
            int positiveEnergy = 0;
            diagnoseTime = HospitalDataHolder.Instance.NeededCureTime((int)dt);

            switch (dt)
            {
                case DiseaseType.Bone:
                    positiveEnergy = hdh.XRayRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Lungs:
                    positiveEnergy = hdh.LungTestingRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Kidneys:
                    positiveEnergy = hdh.LaserRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Ear:
                    positiveEnergy = hdh.UltrasoundRoom.GetPositiveEnergyCost();
                    break;
                case DiseaseType.Brain:
                    positiveEnergy = hdh.MRIRoom.GetPositiveEnergyCost();
                    break;
                default:
                    break;
            }

            diamondCost += DiamondCostCalculator.GetCostForAction(diagnoseTime, diagnoseTime);
            if (!InDiagnosisState)   //add this cost only when player used Positive Energy on the patient
                diamondCost += DiamondCostCalculator.GetMissingPositiveCost(positiveEnergy);
        }

        //for coins
        void Initialize(int missingAmount, OnEvent onBought, OnEvent onResigned, missingResourceType type = missingResourceType.coin)
        {
            this.onBought = onBought;
            this.onResigned = onResigned;

            offerUsed = false;

            GameObject temp = Instantiate(itemPrefab);
            temp.transform.SetParent(content);
            temp.transform.localScale = Vector3.one;

            temp.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = missingAmount.ToString();
            missingMedicines = null;

            switch (type)
            {
                case missingResourceType.coin:
                    isCoins = true;
                    isPositive = false;
                    temp.GetComponent<Image>().sprite = ReferenceHolder.Get().giftSystem.particleSprites[0];
                    missingPositive = 0;
                    missingCoins = missingAmount;
                    diamondCost = DiamondCostCalculator.GetMissingCoinsCost(missingAmount);
                    break;
                case missingResourceType.positive:
                    isCoins = false;
                    isPositive = true;
                    temp.GetComponent<Image>().sprite = ReferenceHolder.Get().giftSystem.particleSprites[4];
                    missingCoins = 0;
                    missingPositive = missingAmount;
                    diamondCost = DiamondCostCalculator.GetMissingPositiveCost(missingAmount);
                    break;
            }

            cost.text = diamondCost.ToString();

            if (diamondCost > Game.Instance.gameState().GetDiamondAmount())
                AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPMissingResources.ToString(), (int)FunnelStepIAPMissingResources.MissingResourcesPopUp, FunnelStepIAPMissingResources.MissingResourcesPopUp.ToString());
        }

        private void BuyMedicines()
        {
            EconomySource economySource = EconomySource.MissingResources;
            if (missingMedicines.Count >= 0 && missingMedicines[0].Value.GetMedicineRef() == new MedicineRef(MedicineType.Special, 3)) //shovel
                economySource = EconomySource.MissingResourcesShovel;
            Game.Instance.gameState().RemoveDiamonds(diamondCost, economySource);

            foreach (var medicine in missingMedicines)
            {
                Game.Instance.gameState().AddResource(medicine.Value.GetMedicineRef(), medicine.Key, true, EconomySource.MissingResourcesBuy);

                ProductionHoverDraggableElement[] listOfDragableElement = FindObjectsOfType<ProductionHoverDraggableElement>();
                foreach (ProductionHoverDraggableElement element in listOfDragableElement)
                {
                    if (element.GetMedicine() == medicine.Value.GetMedicineRef())
                    {
                        element.UpdateBadgeVisibility();
                        break;
                    }
                }
                listOfDragableElement = null;
            }
        }

        public void BuyResources()
        {
            if (!gameObject.activeSelf)
                return;
            if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
            {
                DiamondTransactionController.Instance.AddDiamondTransaction(diamondCost, delegate
                {
                    var gs = Game.Instance.gameState();

                    if (isCoins && missingMedicines != null && missingMedicines.Count > 0)
                    {
                        int missingCoinCostInDiamonds = DiamondCostCalculator.GetMissingCoinsCost(missingCoins);
                        gs.RemoveDiamonds(missingCoinCostInDiamonds, EconomySource.MissingCoins);
                        gs.AddResource(ResourceType.Coin, missingCoins, EconomySource.MissingCoins, true);
                        diamondCost -= missingCoinCostInDiamonds;
                        BuyMedicines();
                    }
                    else if (isCoins)
                    {
                        gs.RemoveDiamonds(diamondCost, EconomySource.MissingCoins);
                        gs.AddResource(ResourceType.Coin, missingCoins, EconomySource.MissingCoins, true);
                    }
                    else if (isPositive && missingMedicines != null && missingMedicines.Count > 0)
                    {
                        gs.RemoveDiamonds(DiamondCostCalculator.GetMissingPositiveCost(missingPositive), EconomySource.MissingPositive);
                        gs.AddResource(ResourceType.PositiveEnergy, missingPositive, EconomySource.MissingPositive, true);
                        diamondCost -= DiamondCostCalculator.GetMissingPositiveCost(missingPositive);
                        BuyMedicines();
                    }
                    else if (isPositive)
                    {
                        gs.RemoveDiamonds(diamondCost, EconomySource.MissingPositive);
                        gs.AddResource(ResourceType.PositiveEnergy, missingPositive, EconomySource.MissingPositive, true);
                    }
                    else
                    {
                        BuyMedicines();
                    }

                    offerUsed = true;
                    Exit();
                }, this);
                transform.SetAsLastSibling();
            }
            else
            {
                Hide();
                AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                transform.SetAsLastSibling();
                UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
            }
        }

        void Hide()
        {
            isHidden = true;
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void UnHide()
        {
            isHidden = false;
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            transform.SetAsLastSibling();
        }

        public bool IsHidden()
        {
            return isHidden;
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            TreatmentRoomHelpController.onRefresh -= RefreshHelpRequest;

            if (UIController.getHospital != null)
            {
                if (UIController.getHospital.PatientCard.isActiveAndEnabled)
                    UIController.getHospital.PatientCard.transform.SetAsLastSibling();
                else if (UIController.getHospital.bubbleBoyEntryOverlayUI.isActiveAndEnabled)
                    UIController.getHospital.bubbleBoyEntryOverlayUI.transform.SetAsLastSibling();
                else if (UIController.getHospital.LockedFeatureArtPopUpController.isActiveAndEnabled)
                    UIController.getHospital.LockedFeatureArtPopUpController.transform.SetAsLastSibling();
                else if (UIController.getHospital.StorageUpgradePopUp.isActiveAndEnabled)
                    UIController.getHospital.StorageUpgradePopUp.transform.SetAsLastSibling();
                else if (UIController.getHospital.PanaceaUpgradePopUp.isActiveAndEnabled)
                    UIController.getHospital.PanaceaUpgradePopUp.transform.SetAsLastSibling();
                else if (UIController.get.ExpandPopUp.isActiveAndEnabled)
                    UIController.get.ExpandPopUp.transform.SetAsLastSibling();
                else if (UIController.getHospital.PharmacyPopUp.isActiveAndEnabled)
                    UIController.getHospital.PharmacyPopUp.transform.SetAsLastSibling();
                else if (UIController.getHospital.EpidemyOnPopUp.isActiveAndEnabled)
                    UIController.getHospital.EpidemyOnPopUp.transform.SetAsLastSibling();
            }
            else if (UIController.getMaternity != null && UIController.getMaternity.patientCardController.isActiveAndEnabled)
                UIController.getMaternity.patientCardController.transform.SetAsLastSibling();
            base.Exit(hidePopupWithShowMainUI);

            if (!offerUsed)
            {
                AnalyticsController.instance.ReportDecisionPoint(DecisionPoint.missing_resources_closed, 1f);

                onResigned?.Invoke();
            }
            else if (offerUsed)
            {
                onBought?.Invoke();
            }


            for (int i = 0; i < content.childCount; i++)
                Destroy(content.GetChild(i).gameObject);

            //Debug.LogError("Exit missing resources");

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public void ButtonExit()
        {
            Exit();
        }

        public enum missingResourceType
        {
            coin,
            positive
        }
        #region Treatment Help


        public void SetAskFriendsRequestsButton(UnityAction action, bool setGrayscale)
        {
            UIController.SetButtonClickSoundInactiveSecure(askFriendsButton.gameObject, setGrayscale);
            if (askFriendsButton == null)
            {
                Debug.LogError("askFriendButton is null");
                return;
            }
            askFriendsButton.onClick.RemoveAllListeners();
            askFriendsButton.onClick.AddListener(() =>
            {
                UIController.PlayClickSoundSecure(askFriendsButton.gameObject);
            });
            askFriendsButton.onClick.AddListener(action);

            SetAskFriendsButtonGrayscale(setGrayscale);
        }

        private void SetInfoText(bool helpRequestVisible)
        {
            UIController.SetTMProUGUITextSecure(infoText, I2.Loc.ScriptLocalization.Get(helpRequestVisible ? "BUY_RESOURCES_INFO_OR_HELP" : "BUY_RESOURCES_INFO"));
        }

        private void SetAskFriendsButtonActive(bool setActive)
        {
            UIController.SetGameObjectActiveSecure(askFriendsButton.gameObject, setActive);
        }

        private void SetAskFriendsButtonGrayscale(bool setGrayscale)
        {
            SetAskFriendsButtonBackgroundGrayscale(setGrayscale);
            SetAskFriendsTextGrayscale(setGrayscale);
        }

        private void SetAskFriendsButtonBackgroundGrayscale(bool setGrayscale)
        {
            UIController.SetImageSpriteSecure(askFriendsButton.image, setGrayscale ? ResourcesHolder.Get().blue9SliceButton : ResourcesHolder.Get().pink9SliceButton);
            UIController.SetImageGrayscale(askFriendsButton.image, setGrayscale);
        }

        private void SetAskFriendsTextGrayscale(bool setGrayscale)
        {
            UIController.SetTMProUGUITextGrayscaleFace(askFriendsText, setGrayscale, defaultFaceColor);
            UIController.SetTMProUGUITextGrayscaleOutline(askFriendsText, setGrayscale, defaultOutlineColor);
            UIController.SetTMProUGUITextGrayscaleUnderlay(askFriendsText, setGrayscale, defaultUnderlayColor);
        }
        #endregion
    }
}
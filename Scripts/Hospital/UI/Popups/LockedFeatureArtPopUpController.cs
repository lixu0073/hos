using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using MovementEffects;

namespace Hospital
{
    public enum LockedFeature
    {
        Plantation,
        Epidemy,
        VIP,
        BubbleBoy,
        FreeEnumSpot,
        Pharmacy,
        KidsRoom,
        MaternityWard,
        NurseRoom
    }

    public class LockedFeatureArtPopUpController : UIElement
    {
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI specificInfoText;
        public Transform ArtImage;
#pragma warning disable 0649
        [SerializeField]
        private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmPrice;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button unlockNowButton;
        [SerializeField] private LockedFeatureRequiredItem requiredItemPrefab;
        [SerializeField] private GameObject requiredContainer;
        [SerializeField] private Transform requiredContent;
#pragma warning restore 0649
        [SerializeField] private Image currencyIcon = null;

        GameObject retrievedGameObject;
        string graphicPath = "";

        public void Open(LockedFeature feature, bool canUnlock = false, bool isComingSoon = false, OnEvent confirmation = null)
        {
            if (HospitalAreasMapController.HospitalMap.VisitingMode)
                return;
            
            requiredContainer.SetActive(false);
            unlockNowButton.gameObject.SetActive(false);
            gameObject.SetActive(true);
            StartCoroutine(base.Open(true, false, OnPostOpen(feature, canUnlock, isComingSoon, confirmation)));            
        }

        private System.Action OnPostOpen(LockedFeature feature, bool canUnlock = false, bool isComingSoon = false, OnEvent confirmation = null)
        {
            currencyIcon.sprite = ResourcesHolder.Get().coinSprite;

            switch (feature)
            {
                case LockedFeature.Plantation:
                    graphicPath = "LockedFeatureGameObjects/Plantation_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    titleText.text = I2.Loc.ScriptLocalization.Get("PLANTATION");
                    specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_PLANTATION_INFO"), "15");
                    confirmButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);
                    break;

                case LockedFeature.MaternityWard:
                    graphicPath = "LockedFeatureGameObjects/MaternityWard_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath, !isComingSoon && canUnlock));
                    if (isComingSoon)
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("MATERNITY_CENTER").ToUpper();
                        specificInfoText.text = I2.Loc.ScriptLocalization.Get("COMING_SOON_INFO");
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                    }
                    else if (canUnlock)
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("MATERNITY_CENTER").ToUpper();
                        specificInfoText.text = (I2.Loc.ScriptLocalization.Get("RENOWATE") + " " + I2.Loc.ScriptLocalization.Get("MATERNITY_CENTER") + "?").ToUpper();
                        unlockNowButton.gameObject.SetActive(true);
                        ClearRequiredItemsContent();
                        requiredContainer.SetActive(true);
                        LockedFeatureRequiredItem itemCoins = Instantiate(requiredItemPrefab, requiredContent) as LockedFeatureRequiredItem;
                        itemCoins.SetUp(HospitalAreasMapController.HospitalMap.maternityWard.Info.RenovationCost);
                        foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in HospitalAreasMapController.HospitalMap.maternityWard.RequiredItems)
                        {
                            LockedFeatureRequiredItem item = Instantiate(requiredItemPrefab, requiredContent) as LockedFeatureRequiredItem;
                            item.SetUp(pair.Key.GetMedicineRef(), pair.Value);
                        }
                        unlockNowButton.onClick.RemoveAllListeners();
                        unlockNowButton.onClick.AddListener(() => confirmation.Invoke());
                        continueButton.gameObject.SetActive(false);
                        confirmButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("MATERNITY_CENTER");
                        specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_MATERNITY_INFO"), HospitalAreasMapController.HospitalMap.maternityWard.Info.UnlockLvl.ToString());
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                    }
                    break;

                case LockedFeature.Epidemy:
                    graphicPath = "LockedFeatureGameObjects/Epidemic_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    if (isComingSoon)
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("EPIDEMIC_CENTER");
                        specificInfoText.text = I2.Loc.ScriptLocalization.Get("COMING_SOON_INFO");
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                    }
                    else if (canUnlock)
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("EPIDEMIC_CENTER");
                        specificInfoText.text = (I2.Loc.ScriptLocalization.Get("RENOWATE") + " " + I2.Loc.ScriptLocalization.Get("EPIDEMIC_CENTER") + "?").ToUpper();
                        confirmButton.gameObject.SetActive(true);
                        confirmPrice.text = HospitalAreasMapController.HospitalMap.epidemy.GetComponent<EpidemyObjectController>().EpidemyObjectInfo.RenovationCost.ToString();
                        confirmButton.onClick.RemoveAllListeners();
                        confirmButton.onClick.AddListener(() => confirmation.Invoke());
                        continueButton.gameObject.SetActive(false);
                    }
                    else
                    {
                        titleText.text = I2.Loc.ScriptLocalization.Get("EPIDEMIC_CENTER");
                        specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_EPIDEMIC_INFO"), "17");
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                    }
                    break;

                case LockedFeature.VIP:
                    graphicPath = "LockedFeatureGameObjects/Vip_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    titleText.text = I2.Loc.ScriptLocalization.Get("VIP_ROOM");
                    if (canUnlock)
                    {
                        confirmButton.gameObject.SetActive(true);

                        if (HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VipRoom>().roomInfo.costResource == ResourceType.Diamonds)
                        {
                            currencyIcon.sprite = ResourcesHolder.Get().diamondSprite;
                        }

                        confirmPrice.text = HospitalAreasMapController.HospitalMap.vipRoom.GetComponent<VipRoom>().roomInfo.RenovationCost.ToString();
                        confirmButton.onClick.RemoveAllListeners();
                        confirmButton.onClick.AddListener(() => confirmation.Invoke());
                        continueButton.gameObject.SetActive(false);
                        NotificationCenter.Instance.VipPopUpOpen.Invoke(new BaseNotificationEventArgs());
                        specificInfoText.text = (I2.Loc.ScriptLocalization.Get("RENOWATE") + " " + I2.Loc.ScriptLocalization.Get("VIP_ROOM") + "?").ToUpper();
                    }
                    else
                    {
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                        specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_VIP_INFO"), "9");
                    }
                    break;

                case LockedFeature.Pharmacy:
                    graphicPath = "LockedFeatureGameObjects/Pharmacy_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    titleText.text = I2.Loc.ScriptLocalization.Get("PHARMACY");
                    specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_PHARMACY_INFO"), " 7");
                    confirmButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);
                    break;

                case LockedFeature.KidsRoom:
                    graphicPath = "LockedFeatureGameObjects/Kids_Room_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    titleText.text = I2.Loc.ScriptLocalization.Get("KIDS_ROOM");
                    if (canUnlock)
                    {
                        confirmButton.gameObject.SetActive(true);
                        confirmPrice.text = HospitalAreasMapController.HospitalMap.playgroud.roomInfo.RenovationCost.ToString();
                        confirmButton.onClick.RemoveAllListeners();
                        confirmButton.onClick.AddListener(() => confirmation.Invoke());
                        continueButton.gameObject.SetActive(false);
                        specificInfoText.text = (I2.Loc.ScriptLocalization.Get("RENOWATE") + " " + I2.Loc.ScriptLocalization.Get("KIDS_ROOM") + "?").ToUpper();
                    }
                    else
                    {
                        confirmButton.gameObject.SetActive(false);
                        continueButton.gameObject.SetActive(true);
                        specificInfoText.text = string.Format(I2.Loc.ScriptLocalization.Get("UNLOCK_KIDS_ROOM_INFO"), " 12");
                    }
                    break;

                case LockedFeature.NurseRoom:
                    graphicPath = "LockedFeatureGameObjects/Nurse_Room_Art";
                    Timing.RunCoroutine(CreateGraphicObject(graphicPath));
                    titleText.text = I2.Loc.ScriptLocalization.Get("MATERNITY_NURSE_ROOM");
                    break;

                default:
                    confirmButton.gameObject.SetActive(false);
                    continueButton.gameObject.SetActive(true);
                    break;
            }

            return null;
        }

        private void ClearRequiredItemsContent()
        {
            for(int i=0; i < requiredContent.childCount; ++i)
            {
                Destroy(requiredContent.GetChild(i).gameObject);
            }
        }

        private IEnumerator<float> CreateGraphicObject(string graphicPath, bool isMaternityUnlock = false)
        {
            ResourceRequest rr = Resources.LoadAsync(graphicPath);
            while (!rr.isDone)
                yield return 0;
            if (!UIController.get.PoPUpArtsFromResources.TryGetValue(graphicPath, out retrievedGameObject))
            {
                retrievedGameObject = Instantiate(rr.asset) as GameObject;
                UIController.get.PoPUpArtsFromResources.Add(graphicPath, retrievedGameObject);
            }
            retrievedGameObject.transform.SetParent(ArtImage);
            retrievedGameObject.transform.localScale = Vector3.one;
            RectTransform rt = retrievedGameObject.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector3.zero;
            rt.offsetMax = Vector3.zero;
            rt.offsetMin = Vector3.zero;
            rt.offsetMax = new Vector2(rt.offsetMin.x, -27);
            if(isMaternityUnlock)
                rt.offsetMax = new Vector2(rt.offsetMin.x, 71);

            rt.offsetMin = new Vector2(rt.offsetMax.x, 26);
            HideOtherArt();
            retrievedGameObject.SetActive(true);
            graphicPath = "";
        }

        private void HideOtherArt()
        {
            foreach (Transform child in ArtImage)
            {
                if (!child.name.Equals(retrievedGameObject.name)) child.gameObject.SetActive(false);
            }
        }

        public void ButtonExit()
        {
            base.Exit();
        }

        protected override void DisableOnEnd()
        {
            base.DisableOnEnd();
            if (!IsVisible && !IsAnimating)
            {
                HideGameObjectArt();
            }
        }

        private void HideGameObjectArt()
        {
            if (retrievedGameObject)
            {
                retrievedGameObject.SetActive(false);
                retrievedGameObject = null;
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using SimpleUI;
using I2.Loc;
using System;

namespace Hospital
{
    public class NextLevelPopUp : UIElement
    {
        public TextMeshProUGUI titleText;
        public RectTransform contentUnlocks;
        public RectTransform singleUnlocks;
        public RectTransform thisRecttransform;

        public GameObject levelUpGiftPrefab;
        public GameObject levelUpGiftCurrencyPrefab;
        public GameObject levelUpUnlockChainPrefab;
        public GameObject levelUpUnlockChainPanaceaPrefab;
        public GameObject levelUpUnlockSinglePrefab;
        public GameObject featureBlockPrefab;
        public ScrollRect unlocksScrollRect;

        public ShopRoomInfo probeTable;
        public ShopRoomInfo hospitalRoom;

        List<string> unlockedItems;

        int unlockedHospitalMachines = 0;
        int unlockedLaboratoryMachines = 0;
        int unlockedPatioMachines = 0;
        int unlockedSingles = 0;
        Coroutine scrollCoroutine = null;
        int nextLevel;
#pragma warning disable 0649
        [SerializeField] private GameObject nextLevelContent;
        [SerializeField] GameObject reportContent;
#pragma warning restore 0649
        [SerializeField] private List<StorageTab> tabs = null;

        [TermsPopup] [SerializeField] private string newSignTerm = "-";
        [TermsPopup] [SerializeField] private string signTooltipTitleTerm = "-";
        [TermsPopup] [SerializeField] private string signTooltipDescriptionTerm = "-";

        private void OnEnable()
        {
            if (ExtendedCanvasScaler.HasNotch())
            {
                if ((float)Screen.width / (float)Screen.height < 1.44f) // 4:3 aspect ratio or really close
                    thisRecttransform.anchoredPosition = new Vector2(0f, -45.0f);
                else
                    thisRecttransform.anchoredPosition = new Vector2(0f, -23.0f);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
        }

        public override IEnumerator Open(bool isFadeIn = true, bool preservesHovers = false, Action whenDone = null)
        {
            nextLevel = Game.Instance.gameState().GetHospitalLevel() + 1;
            if (nextLevel < 2)
                yield break;

            yield return base.Open(true, false);
            SetTabVisible(0);
            UIController.get.ExpBarToFront();

            List<MedicineRef> unlockedMedicines = ResourcesHolder.Get().medicines.cures.SelectMany((x) =>
            {
                return x.medicines.Where((y) =>
                {
                    return (y.minimumLevel == nextLevel && x.type != MedicineType.Fake);
                });
            }).Select((z) =>
            {
                return z.GetMedicineRef();
            }).ToList();
            List<Rotations> unlockedMachines = ResourcesHolder.Get().GetMachinesForLevel(nextLevel);
            List<Rotations> additionalMachines = ResourcesHolder.Get().GetAdditionalMachines(nextLevel);
            CreateContent(unlockedMedicines, unlockedMachines, additionalMachines);

            if (DailyQuestSynchronizer.Instance.WeekCounter == 1)
                DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.WhatsNext));

            whenDone?.Invoke();
        }

        void CreateContent(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines, List<Rotations> additionalMachines)
        {
            titleText.text = string.Format(I2.Loc.ScriptLocalization.Get("NEXT_LEVEL_INFO"), nextLevel.ToString());
            if (ExtendedCanvasScaler.isPhone()) //due to larger xp bar scale the text needs to go down a little so it's not obscured by the exp bar
                titleText.rectTransform.anchoredPosition = new Vector2(0, -62);
            else
                titleText.rectTransform.anchoredPosition = new Vector2(0, -48);

            unlockedSingles = 0;
            CreateUnlockChains(unlockedMedicines, unlockedMachines);
            CreateFeatureBlock();
            CreateGiftsIcons();
            AddSinglesSize();

            for (int i = 0; i < additionalMachines.Count; i++)
            {
                if (additionalMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                    unlockedHospitalMachines++;
                else if (additionalMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                    unlockedLaboratoryMachines++;
                else if (additionalMachines[i].infos.DrawerArea == HospitalAreaInDrawer.Patio)
                    unlockedPatioMachines++;
            }
            unlockedMachines.AddRange(additionalMachines);
            try { 
                if (scrollCoroutine != null)
                {
                    StopCoroutine(scrollCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
            scrollCoroutine = StartCoroutine(ScrollUpEffect());
        }

        void CreateFeatureBlock()
        {
            //Debug.LogError("CreateFeatureBlock");
            if (nextLevel == 7 || nextLevel == 9 || nextLevel == 12 || nextLevel == 15 || nextLevel == 17 || nextLevel == 23)
            {
                LevelUpNewFeature nf = (Instantiate(featureBlockPrefab, contentUnlocks) as GameObject).GetComponent<LevelUpNewFeature>();
                nf.Initialize(nextLevel);
                contentUnlocks.sizeDelta += new Vector2(0, 160);
            }
        }

        void ClearPopUp()
        {
            //Debug.LogError ("Clearing level up pop up");
            singleUnlocks.SetAsFirstSibling();
            for (int i = 1; i < contentUnlocks.childCount; i++)     //i = 1 because we dont want to destroy first object (SingleUnlocks)
                Destroy(contentUnlocks.GetChild(i).gameObject);
            for (int i = 0; i < singleUnlocks.childCount; i++)
                Destroy(singleUnlocks.GetChild(i).gameObject);

            unlockedHospitalMachines = 0;
            unlockedLaboratoryMachines = 0;
            unlockedPatioMachines = 0;
            if (unlockedItems != null)
                unlockedItems.Clear();
        }

        void CreateGiftsIcons()
        {
            LevelUpGifts.LevelUpGift giftsThisLevel = null;

            int nextLevel = Game.Instance.gameState().GetHospitalLevel() + 1;

            if (nextLevel < 50)
            {
                giftsThisLevel = ResourcesHolder.Get().levelUpGifts.Gifts[Mathf.Clamp(nextLevel, 0, 49)];
            }
            else
            {
                giftsThisLevel = new LevelUpGifts.LevelUpGift();
                giftsThisLevel.resources = new LevelUpGifts.GiftedResources[2] { new LevelUpGifts.GiftedResources() { type = ResourceType.Coin, amount = 0 }, new LevelUpGifts.GiftedResources() { type = ResourceType.Diamonds, amount = 0 } };
                if (nextLevel % 3 == 0)
                {
                    giftsThisLevel.resources[1].amount = DefaultConfigurationProvider.GetConfigCData().DiamondAmountPerLevelUpAfter50;
                }
                else
                {
                    giftsThisLevel.resources[0].amount = Mathf.CeilToInt(nextLevel * DefaultConfigurationProvider.GetConfigCData().GoldFactorForLevelUpRewardAfter50);
                }
            }

            BaseGiftableResource gift = LevelUpGiftsConfig.GetLevelUpGift(nextLevel);

            int giftIndex = -1;

            if (gift != null)
            {
                GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                temp.transform.GetChild(1).GetComponent<Image>().sprite = gift.GetMainImageForGift();
                temp.transform.GetChild(2).gameObject.SetActive(false);
                temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get(gift.GetLocalizationKey());
                temp.transform.SetParent(singleUnlocks);

                giftIndex = temp.transform.GetSiblingIndex();

                //TODO - ogarnąć skale
                float scale = singleUnlocks.childCount > 1 ? 2 : 3.5f;
                temp.transform.localScale = new Vector3(scale / 2, scale / 2, scale / 2);
                temp.transform.GetChild(0).localScale = new Vector3(1.9f, 1.9f, 1.9f);
                //temp.transform.GetChild(0).GetChild(0).localScale = new Vector3(1 / scale, 1 / scale, 1 / scale);
            }

            if (giftsThisLevel != null)
            {
                if (giftsThisLevel.resources != null && giftsThisLevel.resources.Length > 0)
                {
                    foreach (var resourceGift in giftsThisLevel.resources)
                    {
                        if (resourceGift.amount > 0)
                        {
                            GameObject temp = Instantiate(levelUpGiftCurrencyPrefab);
                            temp.transform.GetChild(0).GetComponent<Image>().sprite = ReferenceHolder.Get().giftSystem.particleSprites[(int)resourceGift.type];
                            temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+ " + resourceGift.amount;

                            temp.transform.SetParent(singleUnlocks);
                            temp.transform.localScale = new Vector3(1, 1, 1);

                            if (resourceGift.type == ResourceType.PositiveEnergy)
                            {
                                temp.GetComponent<PointerDownListener>().SetDelegate(() =>
                                {
                                    TextTooltip.Open(I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/POSITIVE_ENERGY"), I2.Loc.ScriptLocalization.Get("SPECIAL_ITEMS/POSITIVE_ENERGY_TOOLTIP"));
                                });
                            }

                            unlockedSingles++;
                        }
                    }
                }

                if (giftsThisLevel.medicines != null && giftsThisLevel.medicines.Length > 0)
                {
                    foreach (var medicineGift in giftsThisLevel.medicines)
                    {
                        if (medicineGift.amount > 0)
                        {
                            GameObject temp = Instantiate(levelUpGiftPrefab);
                            temp.transform.GetChild(0).GetComponent<Image>().sprite = ResourcesHolder.Get().GetSpriteForCure(medicineGift.medRef);
                            if (temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>() != null)
                                temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+ " + medicineGift.amount;

                            temp.transform.SetParent(singleUnlocks);
                            temp.transform.localScale = new Vector3(1, 1, 1);
                            temp.GetComponent<PointerDownListener>().SetDelegate(() =>
                            {
                                TextTooltip.Open(medicineGift.medRef, false, true);
                            });

                            unlockedSingles++;
                        }
                    }
                }
            }

            if (giftIndex >= 0)
            {
                float scale = singleUnlocks.childCount > 1 ? 2 : 3.5f;
                singleUnlocks.GetChild(giftIndex).transform.localScale = Vector3.one * (scale / 2f);
            }
        }

        void CreateUnlockChains(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines)
        {
            if (unlockedItems == null)
                unlockedItems = new List<string>();     //list holds names of all already instantiated items in a chain so we don't spawn 2 same chains because machine and syrup is unlocked.
            else
                unlockedItems.Clear();

            contentUnlocks.sizeDelta = new Vector2(0, 40);

            foreach (var medicine in unlockedMedicines)
            {
                if (unlockedItems.Contains(ResourcesHolder.Get().GetMedicineInfos(medicine).Name))
                    continue;
                if (medicine.type == MedicineType.Special || medicine.type == MedicineType.Fake)
                    continue;

                unlockedItems.Add(ResourcesHolder.Get().GetMedicineInfos(medicine).Name);
                unlockedItems.Add(ResourcesHolder.Get().GetMedicineInfos(medicine).producedIn.Tag);
                if (ResourcesHolder.Get().GetMedicineInfos(medicine).doctorRoom != null)
                    unlockedItems.Add(ResourcesHolder.Get().GetMedicineInfos(medicine).doctorRoom.Tag);

                contentUnlocks.sizeDelta += new Vector2(0, 120);
                GameObject temp = Instantiate(levelUpUnlockChainPrefab);
                temp.transform.SetParent(contentUnlocks);
                temp.transform.localScale = new Vector3(1, 1, 1);
                LevelUpUnlockChain chain = temp.GetComponent<LevelUpUnlockChain>();
                chain.SetChain(ResourcesHolder.Get().GetMedicineInfos(medicine));
                //Debug.LogError("Created chain on level up for medicine: " + ResourcesHolder.Get().GetMedicineInfos(medicine).Name);
            }

            if (nextLevel == 2)
            {
                contentUnlocks.sizeDelta += new Vector2(0, 120);
                GameObject temp = Instantiate(levelUpUnlockChainPanaceaPrefab);
                temp.transform.SetParent(contentUnlocks);
                temp.transform.localScale = new Vector3(1, 1, 1);
            }

            foreach (Rotations machine in unlockedMachines)
            {
                if (machine.infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                    unlockedHospitalMachines++;
                else if (machine.infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                    unlockedLaboratoryMachines++;
                else if (machine.infos.DrawerArea == HospitalAreaInDrawer.Patio)
                    unlockedPatioMachines++;

                if (unlockedItems.Contains(machine.infos.Tag))
                {
                    //Debug.LogError("Machine: " + machine.infos.Tag + " already in another chain");
                    continue;
                }
                unlockedItems.Add(machine.infos.Tag);
                //Debug.Log("New single item(no chain): " + machine.infos.Tag);

                unlockedSingles++;
                GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                temp.transform.SetParent(singleUnlocks);
                temp.transform.localScale = new Vector3(1, 1, 1);
                temp.transform.GetChild(1).GetComponent<Image>().sprite = ((ShopRoomInfo)(machine.infos)).ShopImage;
                temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)(machine.infos)).ShopTitle);

                string hoverTitle = I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)(machine.infos)).ShopTitle);
                string hoverDesc = I2.Loc.ScriptLocalization.Get(((ShopRoomInfo)(machine.infos)).ShopDescription);
                temp.transform.GetChild(0).GetComponent<PointerDownListener>().SetDelegate(() =>
                {
                    TextTooltip.Open(hoverTitle, hoverDesc);
                });
            }

            if (nextLevel > 2)
            {
                if (GetProbeTableAmountOnLevel(nextLevel) != GetProbeTableAmountOnLevel(nextLevel - 1))
                {
                    unlockedSingles++;
                    GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                    temp.transform.SetParent(singleUnlocks);
                    temp.transform.localScale = new Vector3(1, 1, 1);
                    temp.transform.GetChild(1).GetComponent<Image>().sprite = probeTable.ShopImage;
                    temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get(probeTable.ShopTitle);
                    temp.transform.GetChild(0).GetComponent<PointerDownListener>().SetDelegate(() =>
                    {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get(probeTable.ShopTitle), I2.Loc.ScriptLocalization.Get(probeTable.ShopDescription));
                    });
                }

                if (GetHospitalRoomOnLevel(nextLevel) != GetHospitalRoomOnLevel(nextLevel - 1))
                {
                    unlockedSingles++;
                    GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                    temp.transform.SetParent(singleUnlocks);
                    temp.transform.localScale = new Vector3(1, 1, 1);
                    temp.transform.GetChild(1).GetComponent<Image>().sprite = hospitalRoom.ShopImage;
                    temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get(hospitalRoom.ShopTitle);
                    temp.transform.GetChild(0).GetComponent<PointerDownListener>().SetDelegate(() =>
                    {
                        TextTooltip.Open(I2.Loc.ScriptLocalization.Get(hospitalRoom.ShopTitle), I2.Loc.ScriptLocalization.Get(hospitalRoom.ShopDescription));
                    });
                }

                List<HospitalSignInfo> signs = ResourcesHolder.GetHospital().signsDatabase.signs;
                for (int i = 0; i < signs.Count; ++i)
                {
                    if (signs[i].unlockLevel == nextLevel)
                    {
                        unlockedSingles++;
                        GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                        temp.transform.SetParent(singleUnlocks);
                        temp.transform.localScale = new Vector3(1, 1, 1);
                        temp.transform.GetChild(1).GetComponent<Image>().sprite = signs[i].miniature;
                        temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = ScriptLocalization.Get(newSignTerm);
                        temp.transform.GetChild(0).GetComponent<PointerDownListener>().SetDelegate(() =>
                        {
                            TextTooltip.Open(ScriptLocalization.Get(signTooltipTitleTerm), ScriptLocalization.Get(signTooltipDescriptionTerm));
                        });
                    }
                }
            }

            singleUnlocks.SetAsLastSibling();
            if ((unlockedMachines == null) || (unlockedMachines.Count <= 0))
            {
                UIController.get.drawer.HideAllBadges();
                return;
            }
        }

        void AddSinglesSize()
        {
            //Debug.LogError("sizeDelta before singles: " + contentUnlocks.sizeDelta);
            //Debug.LogError("singles count: " + singleUnlocks.childCount);
            contentUnlocks.sizeDelta += new Vector2(0, 130 * Mathf.CeilToInt(singleUnlocks.childCount / 3f));
            //Debug.LogError("sizeDelta after singles: " + contentUnlocks.sizeDelta);
        }

        IEnumerator ScrollUpEffect()
        {
            //Debug.LogError("ScrollUpEffect");
            float normPos = 0;
            while (normPos < 1)
            {
                normPos += Time.deltaTime / 2;
                unlocksScrollRect.verticalNormalizedPosition = normPos;
                yield return null;
            }
        }

        public int GetProbeTableAmountOnLevel(int lvl)
        {
            if (probeTable.MaxAmountOnLVL.Length > 0)
            {
                int output_amount = 0;
                foreach (ObjectLevelAmount am in probeTable.MaxAmountOnLVL)
                {
                    if (am.Level <= lvl)
                    {
                        output_amount = am.Amount;
                    }
                    else
                        return output_amount;
                }
                return output_amount;
            }
            else
                return probeTable.multipleMaxAmount;
        }

        public int GetHospitalRoomOnLevel(int lvl)
        {
            if (hospitalRoom.MaxAmountOnLVL.Length > 0)
            {
                int output_amount = 0;
                foreach (ObjectLevelAmount am in hospitalRoom.MaxAmountOnLVL)
                {
                    if (am.Level <= lvl)
                    {
                        output_amount = am.Amount;
                    }
                    else
                        return output_amount;
                }
                return output_amount;
            }
            else
                return hospitalRoom.multipleMaxAmount;
        }

        public override void Exit(bool hidePopupWithShowMainUI = true)
        {
            base.Exit(hidePopupWithShowMainUI);
            ClearPopUp();
            try
            { 
                if (scrollCoroutine != null)
                {
                    StopCoroutine(scrollCoroutine);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }

        public void ButtonExit()
        {
            Exit();
        }

        protected override void DisableOnEnd()
        {
            base.DisableOnEnd();

            if (!IsVisible)
                UIController.get.ExpBarToBack();
        }

        private void ShowTab(int tabID)
        {
            if (tabID == 0)
            {
                tabs[0].transform.SetAsLastSibling();
                tabs[1].transform.SetAsFirstSibling();
                tabs[0].ChangeTabButton(true);
                tabs[1].ChangeTabButton(false);
                nextLevelContent.SetActive(true);
                reportContent.SetActive(false);
            }
            else if (tabID == 1)
            {
                tabs[1].transform.SetAsLastSibling();
                tabs[0].transform.SetAsFirstSibling();
                tabs[1].ChangeTabButton(true);
                tabs[0].ChangeTabButton(false);
                nextLevelContent.SetActive(false);
                reportContent.SetActive(true);
                reportContent.GetComponent<ReportPopupController>().SetData();
            }
        }

        public void SetTabVisible(int tabID)
        {
            ShowTab(tabID);
        }

        public void RefreshCures()
        {
            reportContent.GetComponent<ReportPopupController>().SetCuresContent();
        }

        public void CheckCureReady(Vector3 position)
        {
            reportContent.GetComponent<ReportPopupController>().CheckCureReady(position);
        }

        public void HideCuresNeeded()
        {
            reportContent.GetComponent<ReportPopupController>().HideCuresNeeded();
        }

    }
}

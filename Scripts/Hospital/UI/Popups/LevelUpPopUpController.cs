using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleUI;
using TMPro;
using TutorialSystem;
using System;

namespace Hospital
{
    public class LevelUpPopUpController : UIElement, ITutorialTrigger
    {
        public GameObject levelUpGiftPrefab;
        public GameObject levelUpGiftCurrencyPrefab;
        public GameObject levelUpUnlockChainPrefab;
        public GameObject levelUpUnlockChainPanaceaPrefab;
        public GameObject levelUpUnlockSinglePrefab;
        public GameObject featureBlockPrefab;
        public RectTransform contentUnlocks;
        public RectTransform singleUnlocks;
        public ScrollRect unlocksScrollRect;

        [SerializeField] protected TextMeshProUGUI levelInfo = null;
        [SerializeField] protected ShopRoomInfo probeTable;
        [SerializeField] protected ShopRoomInfo hospitalRoom;
        [SerializeField] protected LevelUpFireworks levelUpFireworks;

        [SerializeField] private Button continueButton = null;

        protected List<string> unlockedItems;

        protected int unlockedHospitalMachines = 0;
        protected int unlockedLaboratoryMachines = 0;
        protected int unlockedPatioMachines = 0;
        protected int unlockedSingles = 0;
        protected Coroutine scrollCoroutine = null;
        protected Coroutine openCoroutine = null;

        public bool lvlUPOpened = false;

        public Dictionary<string, TutorialTriggerEvent> triggerEvents = new Dictionary<string, TutorialTriggerEvent>()
        {
            {
                "Levelup_Popup_Closed", new TutorialTriggerEvent()
            }
        };
        public Dictionary<string, TutorialTriggerEvent> TriggerEvents { get { return triggerEvents; } }

        [TutorialTrigger]
        public event System.EventHandler levelUpPopupClosed;

        [TutorialCondition]
        public bool PopupFinishedOpening() { return openCoroutine == null; }

        public void Open(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines, List<Rotations> additionalMachines)
        {
            lvlUPOpened = false;

            CoroutineInvoker.Instance.StartCoroutine(base.Open(false, true, () =>
            {
                continueButton.interactable = true;
                try
                {
                    if (openCoroutine != null)
                    {
                        StopCoroutine(openCoroutine);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
                }
                openCoroutine = StartCoroutine(OpenCoroutine(unlockedMedicines, unlockedMachines, additionalMachines));
            }));
        }

        protected virtual IEnumerator OpenCoroutine(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines, List<Rotations> additionalMachines)
        {
            levelInfo.text = I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + Game.Instance.gameState().GetHospitalLevel();
            unlockedSingles = 0;
            CreateUnlockChains(unlockedMedicines, unlockedMachines);
            CreateFeatureBlock();
            CreateGiftsIcons();
            AddSinglesSize();

            for (int i = 0; i < additionalMachines.Count; i++)
            {
                BaseRoomInfo infos = additionalMachines[i].infos;
                if (infos.dummyType == BuildDummyType.Decoration && ((ShopRoomInfo)(infos)).unlockLVL < 8)
                    continue;
                
                if (infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                    unlockedHospitalMachines++;
                else if (infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                    unlockedLaboratoryMachines++;
                else if (infos.DrawerArea == HospitalAreaInDrawer.Patio)
                    unlockedPatioMachines++;
            }

            unlockedMachines.AddRange(additionalMachines);

            UIController.get.drawer.HideAllBadges();
            UIController.get.drawer.AddBadgeForItems(unlockedMachines);
            UIController.getHospital.drawer.AddBadgeForItems(unlockedMachines);
            UIController.get.drawer.AddTabButtonBadges(unlockedHospitalMachines, unlockedLaboratoryMachines, unlockedPatioMachines);

            yield return new WaitForSeconds(1f);
            HospitalUIPrefabController.Instance.HideMainUI();
            yield return new WaitForSeconds(1f);
            UIController.get.AddPopupFade(this);
            SoundsController.Instance.PlayLvlUp();
            levelUpFireworks.Fire();

            //resize camera so level up fireworks are proper size
            //ReferenceHolder.Get().engine.MainCamera.SmoothZoom(7f, 1f, false);
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
            scrollCoroutine = StartCoroutine(ScrollUpEffect());

            openCoroutine = null;
            lvlUPOpened = true;
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

        void CreateFeatureBlock()
        {
            //Debug.LogError("CreateFeatureBlock");
            int level = Game.Instance.gameState().GetHospitalLevel();
            if (level == 7 || level == 9 || level == 12 || level == 15 || level == 17 || level == 23)
            {
                LevelUpNewFeature nf = Instantiate(featureBlockPrefab, contentUnlocks).GetComponent<LevelUpNewFeature>();
                nf.Initialize(level);
                contentUnlocks.sizeDelta += new Vector2(0, 160);
            }
        }

        void CreateGiftsIcons()
        {
            LevelUpGifts.LevelUpGift giftsThisLevel = null;

            int hospitalLevel = Game.Instance.gameState().GetHospitalLevel();

            if (hospitalLevel < 50)
            {
                giftsThisLevel = ResourcesHolder.Get().levelUpGifts.Gifts[Mathf.Clamp(Game.Instance.gameState().GetHospitalLevel(), 0, 49)];
            }
            BaseGiftableResource gift = LevelUpGiftsConfig.GetLevelUpGift(hospitalLevel);

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

            if (Game.Instance.gameState().GetHospitalLevel() == 2)
            {
                contentUnlocks.sizeDelta += new Vector2(0, 120);
                GameObject temp = Instantiate(levelUpUnlockChainPanaceaPrefab);
                temp.transform.SetParent(contentUnlocks);
                temp.transform.localScale = new Vector3(1, 1, 1);
                LevelUpUnlockChain chain = temp.GetComponent<LevelUpUnlockChain>();
                if (chain != null)
                {
                    chain.SetPanaceaChainTooltips();
                }
            }

            foreach (Rotations machine in unlockedMachines)
            {
                BaseRoomInfo infos = machine.infos;
                if (!(infos.dummyType == BuildDummyType.Decoration && ((ShopRoomInfo)(infos)).unlockLVL < 8))
                {
                    if (infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                        unlockedHospitalMachines++;
                    else if (infos.DrawerArea == HospitalAreaInDrawer.Laboratory)
                        unlockedLaboratoryMachines++;
                    else if (infos.DrawerArea == HospitalAreaInDrawer.Patio)
                        unlockedPatioMachines++;
                }

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

            if (Game.Instance.gameState().GetHospitalLevel() > 2)
            {
                if (GetProbeTableAmountOnLevel(Game.Instance.gameState().GetHospitalLevel()) != GetProbeTableAmountOnLevel(Game.Instance.gameState().GetHospitalLevel() - 1))
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

                if (GetHospitalRoomOnLevel(Game.Instance.gameState().GetHospitalLevel()) != GetHospitalRoomOnLevel(Game.Instance.gameState().GetHospitalLevel() - 1))
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
                if (ResourcesHolder.GetHospital() != null)
                {
                    List<HospitalSignInfo> signs = ResourcesHolder.GetHospital().signsDatabase.signs;
                    for (int i = 0; i < signs.Count; ++i)
                    {
                        if (signs[i].unlockLevel == Game.Instance.gameState().GetHospitalLevel())
                        {
                            unlockedSingles++;
                            GameObject temp = Instantiate(levelUpUnlockSinglePrefab);
                            temp.transform.SetParent(singleUnlocks);
                            temp.transform.localScale = new Vector3(1, 1, 1);
                            temp.transform.GetChild(1).GetComponent<Image>().sprite = signs[i].miniature;
                            temp.transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = I2.Loc.ScriptLocalization.Get("SIGN_FOR_LEVELUP");
                            temp.transform.GetChild(0).GetComponent<PointerDownListener>().SetDelegate(() =>
                            {
                                TextTooltip.Open(I2.Loc.ScriptLocalization.Get("HOSPITAL_POPUP_TITLE"), I2.Loc.ScriptLocalization.Get("HOSPITAL_SIGN_TOOLTIP_DESC"));
                            });
                            //ReferenceHolder.Get().HospitalNameSign.setNewBadgeActive(true);
                            ApplyNewSign(signs[i].signName);
                        }
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

        private void ApplyNewSign(string signName)
        {
            if (GameAssetBundleManager.instance.hospitalSign.GetInfo(ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()).type != CustomizableHospitalSignDatabase.SignType.Premium)
            {
                ReferenceHolder.GetHospital().signControllable.SetCurrentSignName(signName);
                ReferenceHolder.GetHospital().signControllable.AddSignCustomization();
            }
        }

        void AddSinglesSize()
        {
            //Debug.LogError("sizeDelta before singles: " + contentUnlocks.sizeDelta);
            //Debug.LogError("singles count: " + singleUnlocks.childCount);
            contentUnlocks.sizeDelta += new Vector2(0, 140 * Mathf.CeilToInt(singleUnlocks.childCount / 3f));
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
                        output_amount = am.Amount;
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
            UIController.get.drawer.ShowMainButtonBadge(unlockedHospitalMachines + unlockedLaboratoryMachines + unlockedPatioMachines);
            DailyQuestNotificationCenter.Instance.dailyQuestTaskUpdate.Invoke(new DailyQuestProgressEventArgs(1, DailyTask.DailyTaskType.LevelUp));
            base.Exit(hidePopupWithShowMainUI);
            ClearPopUp();
            GameState.Get().GiveLevelUpGifts();

            if (TriggerEvents.ContainsKey("Levelup_Popup_Closed") && TriggerEvents["Levelup_Popup_Closed"] != null)
                TriggerEvents["Levelup_Popup_Closed"].Invoke(this, new TutorialTriggerArgs());

            levelUpPopupClosed?.Invoke(this, null);

            NotificationCenter.Instance.LevelReachedAndClosed.Invoke(new LevelReachedAndClosedEventArgs(Game.Instance.gameState().GetHospitalLevel()));
            NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Invoke(new LevelReachedAndClosedEventArgs(Game.Instance.gameState().GetHospitalLevel()));
            //Debug.LogError("LevelReachedAndClosedNonLinear.Invoke");

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

        public void ButtonContinue()
        {
            continueButton.interactable = false;

            Exit();
        }

        protected void CallBaseExit(bool hidePopupWithShowMainUI)
        {
            base.Exit(hidePopupWithShowMainUI);
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
            if (featureBlockPrefab.GetComponent<LevelUpNewFeature>().retrievedGameObject)
            {
                featureBlockPrefab.GetComponent<LevelUpNewFeature>().retrievedGameObject.SetActive(false);
                //Destroy(featureBlockPrefab.GetComponent<LevelUpNewFeature>().retrievedGameObject);
                //featureBlockPrefab.GetComponent<LevelUpNewFeature>().retrievedGameObject = null;
                //Resources.UnloadUnusedAssets();
            }
        }
    }
}

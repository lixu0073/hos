using Hospital;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPopUpControllerMaternity : LevelUpPopUpController
{
    private void OnDisable()
    {
        StopAllCoroutines(); //Stops all corutines on this MonoBehaviour
    }

    protected override IEnumerator OpenCoroutine(List<MedicineRef> unlockedMedicines, List<Rotations> unlockedMachines, List<Rotations> additionalMachines)
    {
        levelInfo.text = I2.Loc.ScriptLocalization.Get("LEVEL_U") + " " + Game.Instance.gameState().GetMaternityLevel();
        unlockedSingles = 0;
        CreateUnlockChains(unlockedMachines);
        //CreateFeatureBlock();
        CreateGiftsIcons();
        AddSinglesSize();

        unlockedMachines.AddRange(additionalMachines);

        UIController.get.drawer.HideAllBadges();
        UIController.get.drawer.AddBadgeForItems(unlockedMachines);
        //UIController.getHospital.drawer.AddBadgeForItems(unlockedMachines);
        UIController.get.drawer.AddTabButtonBadges(unlockedHospitalMachines, unlockedLaboratoryMachines, 0);

        yield return new WaitForSeconds(1f);
        HospitalUIPrefabController.Instance.HideMainUI();
        yield return new WaitForSeconds(1f);
        UIController.get.AddPopupFade(this);
        SoundsController.Instance.PlayLvlUp();
        levelUpFireworks.Fire();
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

    //void CreateFeatureBlock()
    //{
    //    //Debug.LogError("CreateFeatureBlock");
    //    int level = Game.Instance.gameState().GetHospitalLevel();
    //    if (level == 7 || level == 9 || level == 12 || level == 15 || level == 17)
    //    {
    //        LevelUpNewFeature nf = (Instantiate(featureBlockPrefab, contentUnlocks) as GameObject).GetComponent<LevelUpNewFeature>();
    //        nf.Initialize(level);
    //        contentUnlocks.sizeDelta += new Vector2(0, 160);
    //    }
    //}

    void CreateGiftsIcons()
    {
        LevelUpGifts.LevelUpGift giftsThisLevel = MaternityGameState.Get().GetLevelUpGifts();

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
            if (giftsThisLevel.decorations != null && giftsThisLevel.decorations.Length > 0)
            {
                foreach (var decorationGift in giftsThisLevel.decorations)
                {
                    if (decorationGift.amount > 0)
                    {
                        GameObject temp = Instantiate(levelUpGiftPrefab);
                        temp.transform.GetChild(0).GetComponent<Image>().sprite = decorationGift.medRef.ShopImage;
                        if (temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>() != null)
                            temp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "+ " + decorationGift.amount;

                        temp.transform.SetParent(singleUnlocks);
                        temp.transform.localScale = new Vector3(1, 1, 1);

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
    }

    void CreateUnlockChains(List<Rotations> unlockedMachines)
    {
        if (unlockedItems == null)
            unlockedItems = new List<string>();     //list holds names of all already instantiated items in a chain so we don't spawn 2 same chains because machine and syrup is unlocked.
        else
            unlockedItems.Clear();

        contentUnlocks.sizeDelta = new Vector2(0, 40);

        foreach (Rotations machine in unlockedMachines)
        {
            BaseRoomInfo infos = machine.infos;

            if (!(infos.dummyType == BuildDummyType.Decoration && ((ShopRoomInfo)(infos)).unlockLVL < 8))
            {
                if (infos.DrawerArea == HospitalAreaInDrawer.Clinic || infos.DrawerArea == HospitalAreaInDrawer.MaternityClinic)
                    unlockedHospitalMachines++;
            }

            if (unlockedItems.Contains(machine.infos.Tag))
                continue;

            unlockedItems.Add(machine.infos.Tag);

            unlockedSingles++;

            if (infos.depeningRoom != null)
            {
                GameObject temp = Instantiate(levelUpUnlockChainPrefab);
                temp.transform.SetParent(contentUnlocks);
                temp.transform.localScale = new Vector3(1, 1, 1);

                LevelUpUnlockChain chain = temp.GetComponent<LevelUpUnlockChain>();
                chain.InitiailizeMaternityChain((ShopRoomInfo)infos);
            }
        }

        singleUnlocks.SetAsLastSibling();
        if ((unlockedMachines == null) || (unlockedMachines.Count <= 0))
        {
            UIController.get.drawer.HideAllBadges();
            return;
        }
    }

    //Szmury not a part of maternity code
    //private void ApplyNewSign(string signName)
    //{
    //    if (GameAssetBundleManager.instance.hospitalSign.GetInfo(ReferenceHolder.GetHospital().signControllable.GetCurrentSignName()).type != CustomizableHospitalSignDatabase.SignType.Premium)
    //    {
    //        ReferenceHolder.GetHospital().signControllable.SetCurrentSignName(signName);
    //        ReferenceHolder.GetHospital().signControllable.AddSignCustomization();
    //    }
    //}

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

    public new int GetHospitalRoomOnLevel(int lvl)
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
        ClearPopUp();
        Game.Instance.gameState().GiveLevelUpGifts();

        NotificationCenter.Instance.LevelReachedAndClosed.Invoke(new LevelReachedAndClosedEventArgs(Game.Instance.gameState().GetMaternityLevel()));
        NotificationCenter.Instance.LevelReachedAndClosedNonLinear.Invoke(new LevelReachedAndClosedEventArgs(Game.Instance.gameState().GetMaternityLevel()));
        //Debug.LogError("LevelReachedAndClosedNonLinear.Invoke");

        try
        {
            if (scrollCoroutine != null)
                StopCoroutine(scrollCoroutine);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
        }
        CallBaseExit(hidePopupWithShowMainUI);
    }

    protected override void DisableOnEnd()
    {
        base.DisableOnEnd();
        if (!IsVisible && !IsAnimating)
            HideGameObjectArt();
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


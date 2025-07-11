using Amazon.Runtime.Internal.Util;
using Hospital;
using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MovementEffects;
using TMPro;

public class MaternityShopDrawer : UIDrawerController
{
    private int indexOffsetForMaternity = 0;

    public override void CenterCameraToArea(HospitalArea area, bool checkPosition = true)
    {
        Vector3 camLookingAt = ReferenceHolder.Get().engine.MainCamera.LookingAt;
        Vector2i camLookingAti = new Vector2i(Mathf.RoundToInt(camLookingAt.x), Mathf.RoundToInt(camLookingAt.z));

        switch (area)
        {
            case HospitalArea.MaternityWardClinic:
                if (checkPosition)
                {
                    if (AreaMapController.Map.areas != null && AreaMapController.Map.areas.Count > 0)
                    {
                        if (AreaMapController.Map.areas.ContainsKey(area))
                        {
                            if (!AreaMapController.Map.areas[area].ContainsPoint(camLookingAti))
                            {

                                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[0].x, 0, AreasPositions[0].y), 1.0f, true);
                            }
                        }
                    }
                }
                else
                {
                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[0].x, 0, AreasPositions[0].y), 1.0f, true);
                }
                break;
            default:
                if (checkPosition)
                {
                    if (AreaMapController.Map.areas != null && AreaMapController.Map.areas.Count > 0)
                    {
                        if (AreaMapController.Map.areas.ContainsKey(area))
                        {
                            if (!AreaMapController.Map.areas[area].ContainsPoint(camLookingAti))
                            {

                                ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[1].x, 0, AreasPositions[1].y), 1.0f, true);
                            }
                        }
                    }
                }
                else
                {
                    ReferenceHolder.Get().engine.MainCamera.SmoothMoveToPoint(new Vector3(AreasPositions[1].x, 0, AreasPositions[1].y), 1.0f, true);
                }
                break;
        }

    }

    protected override void CenterAtTutorialItem()
    {
        foreach (UIDrawerController.DrawerRotationItem item in AllDrawerItems())
        {
            if (item.drawerItem.GetInfo.infos.Tag == TutorialController.Instance.GetCurrentStepData().TargetMachineTag)
            {
                List<Rotations> single = new List<Rotations>();
                single.Add(item.rotations);
                if (item.drawerItem.depeningItem != null)
                {
                    CenterToItem(item.drawerItem.GetComponent<RectTransform>());
                }
                else
                {
                    CenterToItem(item.drawerItem.GetComponent<RectTransform>().parent.gameObject.GetComponent<RectTransform>());
                }
                TutorialUIController.Instance.BlinkImage(item.drawerItem.image);

                // VV May need to be restored! VV

                //if (TutorialController.Instance.CurrentTutorialStepTag == StepTag.maternity_labor_room_drawer_open)
                //    TutorialUIController.Instance.ShowTutorialArrowUI(this.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForMaternitySecondRoom, 0, TutorialUIController.TutorialPointerAnimationType.swipe_right);
                //else
                //    TutorialUIController.Instance.ShowTutorialArrowUI(this.GetComponent<RectTransform>(), TutorialUIController.UIPointerPositionForDrawerItems, 0, TutorialUIController.TutorialPointerAnimationType.swipe_right);

                ToggleScrollingFunctionality(false);
                //NotificationListener.Instance.SubscribeDrawerClosedNotification();
                break;
            }
        }
    }

    public override void HideAllBadgeOnCurrentTab()
    {
        if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))
        {

            int howManyExist = 0;
            int existOnTab = 0;

            foreach (Rotations currentBadge in BadgeObjectList.ToList())
            {
                DrawerRotationItem temp = allItems.Find(x => (currentBadge.infos == x.rotations.infos));
                ShopRoomInfo localShopInfo = ((ShopRoomInfo)temp.rotations.infos);

                int currentDrawerArea = (int)localShopInfo.DrawerArea;

                currentDrawerArea = MapDrawerArea(localShopInfo.DrawerArea, currentDrawerArea);

                if (currentDrawerArea == actualTab)
                {
                    howManyExist++;

                    if ((int)localShopInfo.SubTabNumber == drawers[actualTab].GetActiveTab())
                        existOnTab++;
                }

                if (currentDrawerArea == actualTab && ((int)localShopInfo.SubTabNumber == drawers[actualTab].GetActiveTab()))
                {
                    temp.drawerItem.HideBadge();
                    Game.Instance.gameState().RemoveBadge(currentBadge);
                    BadgeObjectList.Remove(currentBadge);
                    // HideBadgeOnCurrentSubTab((int)localShopInfo.SubTabNumber);

                    existOnTab--;
                    howManyExist--;

                    //DecrementTabButtonBadges (actualTab + 1); //dont ask why :( OK because same in Increment

                    DecrementButtonBadges(actualTab);
                    if ((actualTab == 0) || (actualTab == 1))
                    {
                        SetBadgeOnSubTab(actualTab, drawers[actualTab].GetActiveTab(), existOnTab);
                    }

                }

            }

            if (howManyExist < 1)
            {
                HideTabButtonBadge((HospitalAreaInDrawer)actualTab);
                ResetAllSubTabsOnCurrentTab(actualTab);
                // HideBadgeOnAllCurrentSubTab();
            }

            SaveSynchronizer.Instance.InstantSave();
            Debug.Log("HideAllBadgeOnCurrentTab");
        }
        else
        {
            HideTabButtonBadge((HospitalAreaInDrawer)actualTab);
            ResetAllSubTabsOnCurrentTab(actualTab);
            // HideBadgeOnAllCurrentSubTab();
        }
    }

    public override void IncrementMainButtonBadge()
    {
        badgeMain.SetActive(true);
        badgeMain.GetComponentInChildren<TextMeshProUGUI>().text = (GetMainBadgeCount() + 1).ToString();
    }


    public override void ShowMainButtonBadge(int count = 0)
    {
        if (count > 0)
        {
            badgeMain.SetActive(true);
            badgeMain.GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
        }
    }

    protected override void DecrementButtonBadges(int tab)
    {
        if (tab == 1)
        {
            if (unlockedHospitalMachines > 0)
            {
                unlockedHospitalMachines--;
                badgeHospital.GetComponentInChildren<TextMeshProUGUI>().text = unlockedHospitalMachines.ToString();
            }
            if (unlockedHospitalMachines == 0)
            {
                badgeHospital.SetActive(false);
            }

        }
        else if (tab == 0)
        {
            if (unlockedPatioMachines > 0)
            {
                unlockedPatioMachines--;
                badgePatio.GetComponentInChildren<TextMeshProUGUI>().text = unlockedPatioMachines.ToString();
            }
            if (unlockedPatioMachines == 0)
            {
                badgePatio.SetActive(false);
            }
        }
    }

    public override void ToggleVisible()
    {
        base.ToggleVisible();
        if (IsVisible)
        {
            //Debug.LogError("Drawer opened");
            NotificationCenter.Instance.DrawerOpened.Invoke(new DrawerOpenedEventArgs());
            //if (openAtLast)
            //{
            //    openAtLast = false;
            //}
            //else
            //{
            //    ShowAreaTab();
            //}
            start = true;

            if (UIController.get.FriendsDrawer != null && UIController.get.FriendsDrawer.IsVisible)
            {
                UIController.get.FriendsDrawer.ToggleVisible();
            }
        }
        else
        {
            start = false;
            Debug.Log("Drawer closed");
            NotificationCenter.Instance.DrawerClosed.Invoke(new DrawerClosedEventArgs());
            UIController.get.drawer.HideAllBadgeOnCurrentTab();
        }
    }


    protected override void ShowAreaTab()
    {
        Vector3 camLookingAt = ReferenceHolder.Get().engine.MainCamera.LookingAt;
        if (camLookingAt.x < 40f && camLookingAt.x > 23f && camLookingAt.z > 45f && camLookingAt.z < 60f && Game.Instance.gameState().GetMaternityLevel() >= 3)
        {
            CenterCameraToArea(HospitalArea.MaternityWardPatio);
            SetTabVisible(0, false);
        }
        else
        {
            CenterCameraToArea(HospitalArea.MaternityWardClinic);
            SetTabVisible(1, false);
        }

    }

    public void SetMaternityTabVisible(int index)
    {
        if (index == 0 && Game.Instance.gameState().GetMaternityLevel() < 3)
        {
            MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("UNLOCKS_AT_LEVEL"), "3"));
            return;
        }

        SetTabVisible(index);
    }

    protected override void SetTabVisible(int index)
    {
        if (actualTab != index)
            animator.SetTrigger("TabButton" + (index + 1));

        SetTabVisible(index, true);
    }

    protected void CenterMaternityCameraToAreaOnButton(int areaID)
    {
        CenterCameraToArea((HospitalArea)areaID);
    }

    public override void SetPaintBadgeClinicVisible(bool setVisible) { }
    public override void SetPaintBadgeLabVisible(bool setVisible) { }

    protected override void InitializeItems()
    {
        isInitalizing = true;
        if (!shouldPopulate)
            return;
        foreach (Rotations item in localItemList)
        {
            switch (item.infos.DrawerArea)
            {
                case HospitalAreaInDrawer.MaternityClinic:
                    AddElement(1, item, HospitalAreaInDrawer.MaternityClinic);
                    break;
                case HospitalAreaInDrawer.Clinic:
                    AddElement(1, item, HospitalAreaInDrawer.MaternityClinic);
                    break;
                case HospitalAreaInDrawer.Patio:
                    if (item.infos as RemovableDecorationInfo == null)
                        AddElement(0, item, HospitalAreaInDrawer.MaternityPatio);
                    break;
                default:
                    AddElement((int)item.infos.DrawerArea, item, HospitalAreaInDrawer.Ignore);
                    break;
            }
        }
        shouldPopulate = false;
        foreach (var p in drawers)
        {
            p.transform.gameObject.SetActive(false);
        }
        actualTab = -1;
        SetTabVisible(0, false);
        isInitalizing = false;
    }

    public override void CenterAtBadgeOnCurrentTab()
    {
        bool showDeco = false;
        RectTransform tempItemTransform = null;

        if ((BadgeObjectList != null) && (BadgeObjectList.Count > 0))

            foreach (Rotations currentBadge in BadgeObjectList)
            {
                DrawerRotationItem temp = allItems.Find(x => (currentBadge.infos == x.rotations.infos));
                if (temp.rotations.infos.DrawerArea == HospitalAreaInDrawer.MaternityClinic)
                {
                    indexOffsetForMaternity = 2;
                }
                else if (temp.rotations.infos.DrawerArea == HospitalAreaInDrawer.Patio)
                {
                    indexOffsetForMaternity = 4;
                }
                if (((int)temp.rotations.infos.DrawerArea - indexOffsetForMaternity == actualTab) && (temp.drawerItem.badge.activeSelf))
                {
                    if (((ShopRoomInfo)temp.rotations.infos).SubTabNumber == 0)
                    {
                        if (drawers[actualTab].GetActiveTab() != 0)
                        {
                            drawers[actualTab].ChangeTab(0, false);
                        }
                        if (temp.drawerItem.depeningItem != null)
                        {
                            CenterToItem(temp.drawerItem.GetComponent<RectTransform>());
                        }
                        else
                        {
                            CenterToItem(temp.drawerItem.GetComponent<RectTransform>().parent.gameObject.GetComponent<RectTransform>());
                        }
                        showDeco = false;
                        break;
                    }

                    if (((ShopRoomInfo)temp.rotations.infos).SubTabNumber == 1 && !showDeco)
                    {
                        tempItemTransform = temp.drawerItem.GetComponent<RectTransform>();
                        showDeco = true;
                    }
                }
            }

        if (showDeco && tempItemTransform != null)
        {
            if (drawers[actualTab].GetActiveTab() == 1)
            {
                CenterToItem(tempItemTransform);
            }
            showDeco = false;
        }
    }

    public override void CenterToItem(RectTransform item, bool toTop = false)
    {
        //Debug.LogError("CenterToItem to Top: " + toTop);
        float targetPosition;
        if (toTop)
            targetPosition = 1f;
        else
        {

            float a = item.transform.parent.parent.GetSiblingIndex();
            float b = scrollRects[actualTab].GetComponent<ScrollRect>().content.childCount;
            targetPosition = sinleDrawerItemRectHeight * a / (b * sinleDrawerItemRectHeight - scrollRects[actualTab].rect.height / 2);
            //targetPosition = (float)item.transform.parent.parent.GetSiblingIndex() / (float)drawers[actualTab].content.content.transform.childCount;
            targetPosition = Mathf.Clamp01(1 - targetPosition);
        }

        if (CenteringCoroutine != null)
            MovementEffects.Timing.KillCoroutine(CenteringCoroutine);

        CenteringCoroutine = MovementEffects.Timing.RunCoroutine(CenterToItemCoroutine(targetPosition));
    }

    public override void IncrementTabButtonBadges(int areaNo)
    {
        switch (areaNo)
        {
            case 0:
                this.unlockedHospitalMachines++;
                break;
            case 1:
                this.unlockedPatioMachines++;
                break;
        }
        UpdateTabButtonBadges();
    }

    protected override void UpdateTabButtonBadges()
    {
        if (this.unlockedHospitalMachines > 0)
        {
            badgeHospital.SetActive(true);
            badgeHospital.GetComponentInChildren<TextMeshProUGUI>().text = unlockedHospitalMachines.ToString();

        }
        else
            badgeHospital.SetActive(false);


        if (this.unlockedPatioMachines > 0)
        {
            badgePatio.SetActive(true);
            badgePatio.GetComponentInChildren<TextMeshProUGUI>().text = unlockedPatioMachines.ToString();
        }
        else
            badgePatio.SetActive(false);

    }

    protected override void IncrementBadgeOnSubTab(HospitalAreaInDrawer area, int subtab)
    {
        int tab = MapDrawerArea(area, 1);
        TextMeshProUGUI badgeText = drawers[tab].subTabs[subtab].badge.GetComponentInChildren<TextMeshProUGUI>();
        int currentItemCount = int.Parse(badgeText.text, System.Globalization.CultureInfo.InvariantCulture);
        drawers[tab].subTabs[subtab].badge.SetActive(true);
        currentItemCount++;
        badgeText.text = currentItemCount.ToString();
    }

    public override void SetDrawerItems()
    {
        Dictionary<string, int> save = GameState.BuildedObjects;

        foreach (DrawerRotationItem item in allItems)
        {
            if (item != null)
            {
                if (item.drawerItem.GetInfo.infos.spawnFromParent)
                {
                    continue;
                }
                if (!save.ContainsKey(item.rotations.infos.Tag))
                {
                    if (item.rotations.infos.DrawerArea == HospitalAreaInDrawer.Clinic)
                    {
                        item.rotations.infos.Area = HospitalArea.MaternityWardClinic;
                    }
                    if (item.rotations.infos.DrawerArea == HospitalAreaInDrawer.Clinic || item.rotations.infos.DrawerArea == HospitalAreaInDrawer.Patio)
                    {
                        if (((ShopRoomInfo)item.rotations.infos).unlockLVL <= Game.Instance.gameState().GetHospitalLevel() || (GameState.StoredObjects.ContainsKey(item.rotations.infos.Tag) && GameState.StoredObjects[item.rotations.infos.Tag] > 0))
                        {
                            item.drawerItem.SetUnlocked();
                        }
                        else
                        {
                            item.drawerItem.SetLocked();
                        }

                    }
                    else
                    {
                        if (((ShopRoomInfo)item.rotations.infos).unlockLVL <= Game.Instance.gameState().GetMaternityLevel() || (GameState.StoredObjects.ContainsKey(item.rotations.infos.Tag) && GameState.StoredObjects[item.rotations.infos.Tag] > 0))
                        {
                            item.drawerItem.SetUnlocked();
                        }
                        else
                        {
                            //Debug.LogError("that tag: " + item.rotations.infos.Tag);
                            item.drawerItem.SetLocked();
                        }
                    }
                }
                else
                {
                    item.drawerItem.SetUnlocked();
                }
            }
            else
            {
                //Debug.LogError("else tag: " + item.rotations.infos.Tag);

                if (!item.rotations.infos.multiple)
                {
                    item.drawerItem.LockBuild();
                }
                else
                {
                    item.drawerItem.SetBuildedAmount(save[item.rotations.infos.Tag]);
                    item.drawerItem.SetUnlocked();
                }
            }
        }
    }

    protected override void SetDeviceLayout()
    {
        if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
        {
            drawerRect.localScale = new Vector3(1.4f, 1.4f, 1f);
            tabsRect.offsetMax = new Vector3(0, 25);

            exitButtonAnchor.anchorMin = new Vector2(.57f, .31f);
            exitButtonAnchor.anchorMax = new Vector2(.57f, .31f);
            exitButtonAnchor.anchoredPosition = Vector2.zero;

            if (tabButtons.Length > 0)
                tabButtons[1].anchoredPosition = new Vector3(-172, -180, 0);
            if (tabButtons.Length > 1)
                tabButtons[0].anchoredPosition = new Vector3(-172, -240, 0);
            if (tabButtons.Length > 2)
                tabButtons[2].anchoredPosition = new Vector3(-172, -120, 0);

            for (int i = 0; i < tabButtons.Length; i++)
                tabButtons[i].localScale = new Vector3(.85f, .85f, 1f);

            for (int i = 0; i < tabTitles.Length; i++)
                tabTitles[i].SetActive(false);

            for (int i = 0; i < scrollRects.Length; i++)
            {
                scrollRects[i].offsetMin = new Vector3(0, 158);
                scrollRects[i].offsetMax = new Vector3(0, 8);
            }

            //for (int i = 0; i < grids.Length; i++)
            //    grids[i].spacing = new Vector2(0, 8);
        }
        else
        {
            drawerRect.localScale = Vector3.one;
            tabsRect.offsetMax = Vector3.zero;

            exitButtonAnchor.anchorMin = new Vector2(.55f, .05f);
            exitButtonAnchor.anchorMax = new Vector2(.55f, .05f);
            exitButtonAnchor.anchoredPosition = Vector2.zero;

            if (tabButtons.Length > 0)
                tabButtons[1].anchoredPosition = new Vector3(-172, -265, 0);
            if (tabButtons.Length > 1)
                tabButtons[0].anchoredPosition = new Vector3(-172, -335, 0);
            if (tabButtons.Length > 2)
                tabButtons[2].anchoredPosition = new Vector3(-172, -195, 0);

            for (int i = 0; i < tabButtons.Length; i++)
                tabButtons[i].localScale = Vector3.one;

            for (int i = 0; i < tabTitles.Length; i++)
                tabTitles[i].SetActive(true);

            for (int i = 0; i < scrollRects.Length; i++)
            {
                scrollRects[i].offsetMin = Vector3.zero;
                scrollRects[i].offsetMax = Vector3.zero;
            }

            //for (int i = 0; i < grids.Length; i++)
            //    grids[i].spacing = new Vector2(0, 20);
        }

        if (ExtendedCanvasScaler.HasNotch())
        {
            for (int i = 0; i < tabTitles.Length; i++)
            {
                tabTitles[i].SetActive(false);
            }
        }

        UpdateSeparatorPos();
    }

    protected override void UpdateSeparatorPos()
    {
        if (ExtendedCanvasScaler.isPhone() && !ExtendedCanvasScaler.HasNotch())
        //if (true)
        {
            if (actualTab == 0)
            {
                separator.anchorMin = new Vector2(0, .831f);
                separator.anchorMax = new Vector2(1, .831f);
            }
            else
            {
                separator.anchorMin = new Vector2(0, .736f);
                separator.anchorMax = new Vector2(1, .736f);
            }
        }
        else
        {
            if (actualTab == 0)
            {
                separator.anchorMin = new Vector2(0, .82f);
                separator.anchorMax = new Vector2(1, .82f);
            }
            else
            {
                separator.anchorMin = new Vector2(0, .725f);
                separator.anchorMax = new Vector2(1, .725f);
            }
        }
    }

    public void TutorialChangeRoomStatus(bool isEnabled)
    {

        DrawerItem tutorialItem = allItems.FirstOrDefault(item =>
        {
            return item.drawerItem.GetInfo.infos.Tag == "LabourRoomBlueOrchid";
        }).drawerItem;
        tutorialItem.SetUiDependend(isEnabled, true);
    }

    private int MapDrawerArea(HospitalAreaInDrawer areInDrawer, int currentDrawerArea)
    {
        if (areInDrawer == HospitalAreaInDrawer.MaternityClinic)
        {
            return 1;
        }
        if (areInDrawer == HospitalAreaInDrawer.Patio)
        {
            return 0;
        }
        return currentDrawerArea;
    }
    /// <summary>
    /// This needs to be copied here because we don't use RefactoredDrawer in Maternity.
    /// If we add support in the future, this can be deleted and refactored drawer call used instead.
    /// 
    /// Okay this is very confusing. We don't want to give decorations to player that they already have? forceShow fixes this.
    /// </summary>
    public void GivePlayerSelectedDecorations(ShopRoomInfo[] decos, bool forceShow = false)
    {
        if (decos == null || decos.Length == 0)
        {
            Debug.LogError("Granted deco list is null or empty!");
            return;
        }

        Vector3 startPoint = Vector3.zero;
        float particleDelay = 0f;
        int amount;
        for (int i = 0; i < decos.Length; i++)
        {
            if (BaseGameState.StoredObjects.TryGetValue(decos[i].Tag, out amount))
            {
                if ((amount <= 0 && !BaseGameState.BuildedObjects.ContainsKey(decos[i].Tag)) || forceShow)
                {
                    MaternityGameState.Get().AddToObjectStored(decos[i], 1);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Drawer, startPoint, 1, particleDelay, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[i].ShopImage, null, null);
                    particleDelay += .35f;
                }
            }
            else
            {
                if (!BaseGameState.BuildedObjects.ContainsKey(decos[i].Tag) || forceShow)
                {
                    MaternityGameState.Get().AddToObjectStored(decos[i], 1);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticleUI(SimpleUI.GiftType.Drawer, startPoint, 1, particleDelay, 1.75f, new Vector3(3.2f, 3.2f, 1), new Vector3(2, 2, 1), decos[i].ShopImage, null, null);
                    particleDelay += .35f;
                }
            }
        }
    }

}

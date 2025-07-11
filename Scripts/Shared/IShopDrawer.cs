using Hospital;
using SimpleUI;
using UnityEngine;
using System.Collections.Generic;

public interface IShopDrawer
{
    Vector3 DragingObjectPosition();
    RotatableObject DragingObject();
    HospitalArea DragingObjectArea();
    List<UIDrawerController.DrawerRotationItem> AllDrawerItems();
    bool IsDragingObjectDummy();
    bool IsInitalizing();
    void SetOpenAtLast(bool open);
    void SetLastRotation(Rotation rotation);
    bool IsPaintBadgeClinicVisible();
    bool IsPaintBadgeLabVisible();
    void BlockDrawer(bool state);
    void SetVisible(bool visible);
    void SetTabVisible(int index, bool hideBadges);
    void UpdatePriceForRotatableInDrawer(RotatableObject rotObject, int newPrice);
    void CenterCameraToArea(HospitalArea area, bool checkPosition = true);
    void CenterToItem(RectTransform item, bool toTop = false);
    void GenerateSimpleRotatableItem(Rotations info);
    void SetPastPosition();
    void ResetDrawerDragableObject();
    void ToggleVisible();
    void UnlockItems(List<Rotations> list);
    void SetDrawerItems();
    void LockBuildItem(Rotations element);
    void Populate(List<Rotations> list);
    List<DrawerTabScript> GetDrawers();
    void AddBadgeForItems(List<Rotations> enableBadgeObjectList);
    bool CheckIsBadgeForItem(Rotations enableBadgeObjectList);
    void ShowMainButtonBadge(int count = 0);
    void IncrementTabButtonBadges(int areaNo);
    void DecrementTabButtonBadges(int areaNo);
    void AddTabButtonBadges(int unlockedHospitalMachines = -1, int unlockedLaboratoryMachines = -1, int unlockedPatioMachines = -1);
    void SortTabsByBought();
    void HideAllBadgeOnCurrentTab();
    void CenterAtBadgeOnCurrentTab();
    void HideAllBadges();
    void DecrementMainButtonBadge();
    void IncrementMainButtonBadge();
    int GetMainBadgeCount();
    void UpdateAllItems();
    void UpdatePrices(UpdateType updateType = UpdateType.Default);
    void UpdateTranslation();
    int GetActualTab();
    int GetActiveTabOnActualTab();
    void SetPaintBadgeClinicVisible(bool setVisible);
    void SetPaintBadgeLabVisible(bool setVisible);

}

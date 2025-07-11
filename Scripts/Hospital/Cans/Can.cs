using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IsoEngine;
using System.Linq;
using MovementEffects;
using SimpleUI;
using System;

namespace Hospital
{
    public class Can : RotatableWithoutAddToMap
    {
        private HospitalArea coloredHospitalArea;
        private Vector4 color;

        protected override void AddToMap()
        {
            state = State.fresh;
            base.AddToMapTemporary();
        }

        public override void StopDragging()
        {
            if (!IsDummy)
                Buy(null);
            else
            {
                MessageController.instance.ShowMessage(53);
                UIController.get.drawer.SetOpenAtLast(true);
                UIController.get.drawer.ToggleVisible();
                base.StopDragging();
            }
        }

        private void ColorFloor()
        {
            HospitalArea areaFromPos = AreaMapController.Map.GetAreaTypeFromPositionIfNotOnEdge(this.position);

            Vector4 canColor = (info.infos as CanInfo).canColor;
            ReferenceHolder.Get().floorControllable.AddHospitalFloorColor(areaFromPos, canColor, new Vector3(this.position.x, 0, this.position.y));

            this.coloredHospitalArea = areaFromPos;
            this.color = canColor;

            AnalyticsController.instance.ReportFloorColored(info.infos.Tag, areaFromPos);
        }

        private bool IsColorBought(Action<bool> onSuccess)
        {
            bool result = false;

            if (ReferenceHolder.Get().floorControllable.IsFloorColorBought(info.infos.Tag))
            {
                ColorFloor();
                ReferenceHolder.Get().floorControllable.SetCurrentFloorColor(info.infos.Tag, coloredHospitalArea);
                ReferenceHolder.Get().floorControllable.CanAnimationRefresh = true;

                base.StopDragging();
                //UIController.get.drawer.UpdatePrices(UpdateType.Can);

                SaveSynchronizer.Instance.MarkToSave(SavePriorities.BuildingPurchased);
                onSuccess?.Invoke(true);

                result = true;
            }

            return result;
        }

        public override void Buy(Action<bool> onSuccess = null, bool afterMissingResources = false, Action OnFailure = null)
        {
            if (Tag == ReferenceHolder.Get().floorControllable.GetCurrentFloorColorName(AreaMapController.Map.GetAreaTypeFromPositionIfNotOnEdge(this.position)))
            {
                string colorName = I2.Loc.ScriptLocalization.Get(AreaMapController.Map.drawerDatabase.GetObjectNameFromShopInfo(Tag));

                MessageController.instance.ShowMessage(string.Format(I2.Loc.ScriptLocalization.Get("FLOOR_COLOR_EXIST"), colorName));
                ReferenceHolder.Get().floorControllable.CanAnimationRefresh = false;
                UIController.get.drawer.ToggleVisible();
                base.StopDragging();
                OnFailure?.Invoke();
            }

            int cost = ((ShopRoomInfo)info.infos).cost;
            int diamondCost = ((ShopRoomInfo)info.infos).costInDiamonds;


            if (IsColorBought(onSuccess))
            {
                return;
            }


            if (cost > 0)
            {
                //int boughtCans = ReferenceHolder.Get().customizationController.GetOwnedFloorColor().Count - 1 - ReferenceHolder.Get().customizationController.PremiumFloorColorCounter;
                //if (boughtCans < 0)
                //    boughtCans = 0;

                //cost = cost + (info.infos as CanInfo).goldIncreasePerOwnedItem * boughtCans;
                cost = AlgorithmHolder.GetCostInGoldForPaint();
                int expReward = AlgorithmHolder.GetExpForBuingPaint();

                if (Game.Instance.gameState().GetCoinAmount() >= cost)
                {
                    ColorFloor();
                    ReferenceHolder.Get().floorControllable.AddNewFloorColorToPlayerCollection(info.infos.Tag);
                    ReferenceHolder.Get().floorControllable.SetCurrentFloorColor(info.infos.Tag, coloredHospitalArea);
                    ReferenceHolder.Get().floorControllable.CanAnimationRefresh = true;

                    if (afterMissingResources)
                        Game.Instance.gameState().RemoveCoins(cost, EconomySource.FloorColorAfterMissing);
                    else
                        Game.Instance.gameState().RemoveCoins(cost, EconomySource.FloorColorAfterMissing);

                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, actualData.tilesX / 2f, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, cost, 0, ReferenceHolder.Get().giftSystem.particleSprites[0]);
                    int currentExpAmount = Game.Instance.gameState().GetExperienceAmount();
                    Game.Instance.gameState().AddResource(ResourceType.Exp, expReward, EconomySource.FloorColor, false);
                    ReferenceHolder.Get().giftSystem.CreateGiftParticle(GiftType.Exp, ReferenceHolder.Get().engine.MainCamera.GetCamera().ScreenToWorldPoint(Input.mousePosition), expReward, 0, 1.75f, Vector3.one, new Vector3(1, 1, 1), null, null, () =>
                    {
                        Game.Instance.gameState().UpdateCounter(ResourceType.Exp, expReward, currentExpAmount);
                    });
                    base.StopDragging();
                    //UIController.get.drawer.UpdatePrices(UpdateType.Can);
                    SaveSynchronizer.Instance.MarkToSave(SavePriorities.BuildingPurchased);
                    //UIController.get.drawer.SortTabsByBought();
                    onSuccess?.Invoke(false);
                }
                else
                {
                    UIController.get.BuyResourcesPopUp.Open(cost - Game.Instance.gameState().GetCoinAmount(), () =>
                    {
                        ReferenceHolder.Get().floorControllable.CanAnimationRefresh = false;
                        Buy(null);
                    }, () =>
                    {
                        ReferenceHolder.Get().floorControllable.CanAnimationRefresh = false;
                        base.StopDragging();
                    });

                    OnFailure?.Invoke();
                }
            }
            else if (diamondCost > 0)
            {
                if (Game.Instance.gameState().GetDiamondAmount() >= diamondCost)
                {
                    ColorFloor();
                    ReferenceHolder.Get().floorControllable.AddNewFloorColorToPlayerCollection(info.infos.Tag);
                    ReferenceHolder.Get().floorControllable.SetCurrentFloorColor(info.infos.Tag, coloredHospitalArea);
                    ReferenceHolder.Get().floorControllable.CanAnimationRefresh = true;
                    ReferenceHolder.Get().floorControllable.IncreasePremiumFloorColorCounter();

                    Debug.Log("I can buy this " + Time.time);
                    if (afterMissingResources)
                        Game.Instance.gameState().RemoveDiamonds(diamondCost, EconomySource.FloorColorAfterMissing);
                    else
                        Game.Instance.gameState().RemoveDiamonds(diamondCost, EconomySource.FloorColor);

                    Vector3 pos = new Vector3(transform.position.x + actualData.rotationPoint.x, actualData.tilesX / 2f, transform.position.z + actualData.rotationPoint.y);
                    ReferenceHolder.Get().giftSystem.CreateItemUsed(pos, diamondCost, 0, ReferenceHolder.Get().giftSystem.particleSprites[1]);
                    //UIController.get.drawer.UpdatePrices(UpdateType.Can);
                    base.StopDragging();
                    SaveSynchronizer.Instance.MarkToSave(SavePriorities.BuildingPurchased);
                    //UIController.get.drawer.SortTabsByBought();
                    onSuccess?.Invoke(false);
                }
                else
                {
                    AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.MissingDiamonds;
                    UIController.get.IAPShopUI.Open(IAPShopSection.sectionDiamonds);
                    ReferenceHolder.Get().floorControllable.CanAnimationRefresh = false;
                    base.StopDragging();
                    OnFailure?.Invoke();
                }
            }
            else
            {
                base.StopDragging();
                ReferenceHolder.Get().floorControllable.CanAnimationRefresh = false;
                OnFailure?.Invoke();
            }
        }

        public override bool isTemporaryObject()
        {
            return true;
        }
    }
}
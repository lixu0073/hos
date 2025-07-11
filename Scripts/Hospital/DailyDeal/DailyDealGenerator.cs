using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Hospital
{
    public class DailyDealGenerator
    {
        public DailyDeal GenerateDailyDeal()
        {
            if (DailyDealParser.Instance.GetDailyDealData() == null) {
                return null;
            }

            List<DailyDeal> deals = DailyDealParser.Instance.GetDailyDealData().GetDailyDeals();
            List<DailyDeal> dealsToDraw = new List<DailyDeal>();

            for (int i = 0; i < deals.Count; ++i) {
                if (CheckIfItemNeeded(deals[i].Item)) {
                    dealsToDraw.Add(deals[i]);
                }
            }

            if (dealsToDraw.Count == 0) {
                dealsToDraw = deals;

            }

            for (int i = 0; i < dealsToDraw.Count; ++i) {
                if (dealsToDraw[i].Item.type == MedicineType.Fake && dealsToDraw[i].Item.id == 0 && HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState != ExternalRoom.EExternalHouseState.enabled) {
                    dealsToDraw.RemoveAt(i);
                    --i;
                }
                if (dealsToDraw[i].Item.type == MedicineType.Special && dealsToDraw[i].Item.id == 3 && Game.Instance.gameState().GetHospitalLevel() < HospitalAreasMapController.HospitalMap.greenHouse.unlockLevels[0])
                {
                    dealsToDraw.RemoveAt(i);
                    --i;
                }
            }

            return dealsToDraw[Random.Range(0, dealsToDraw.Count)];
        }

        

        private bool CheckIfItemNeeded(MedicineRef item) {
            if (item.type == MedicineType.Fake) { 
                if (item.id == 0) { //16(00) is for fake positiveEnergy
                    return CheckPositiveEnergyNeed();
                }
            }

            if (item.type == MedicineType.Special) {
                if (item.id == 3) { //15(03) is for shovels
                    return CheckShovelNeed();
                }   
                return CheckUpgradeToolNeed(item);
            }
            return false;
        }

        #region checkShovel
        private bool CheckShovelNeed() {
            bool shovelNeeded = false;
            if (Game.Instance.gameState().GetHospitalLevel() < HospitalAreasMapController.HospitalMap.greenHouse.unlockLevels[0])
            {
                return false;
            }

            MedicineRef item = MedicineRef.Parse("15(03)"); //15(03) is for shovels
            int fieldsAmount = HospitalAreasMapController.HospitalMap.greenHouse.GetFieldOnlyToExcavateAmount();
            int shovelAmount = GameState.Get().GetCureCount(item);

            DailyDealData.ShovelParams shovelParams = DailyDealParser.Instance.GetDailyDealData().GetShovelParams();
            
            if (fieldsAmount >= shovelParams.FieldTreshold && shovelAmount <= shovelParams.ShovelTreshold)
            {
                shovelNeeded = true;
            } else if (shovelAmount <= shovelParams.ShovelMinAmount) {
                shovelNeeded = true;
            }

            return shovelNeeded;
        }
        #endregion

        #region check upgradeTools
        private bool CheckUpgradeToolNeed(MedicineRef item)
        {
            bool upgradeToolNeeded = false;
            int missingTools = 0;

            SpecialItemTarget itemTarget = item.GetSpecialItemTarget();
            if (itemTarget == SpecialItemTarget.Tank) {
                missingTools = Game.Instance.gameState().ElixirTank.actualLevel - Game.Instance.gameState().GetCureCount(item);
            } else {
                missingTools = Game.Instance.gameState().ElixirStore.actualLevel - Game.Instance.gameState().GetCureCount(item);
            }


            DailyDealData.ToolParams toolParams = DailyDealParser.Instance.GetDailyDealData().GetUpgradeToolParams();
            if (missingTools > 0 && missingTools <= toolParams.UpgradeToolTreshold) {
                upgradeToolNeeded = true;
            }

            return upgradeToolNeeded;
        }

        #endregion

        #region checkPositiveEnergy
        private bool CheckPositiveEnergyNeed() {
            bool positiveEnergyNeeded = false;
            if (HospitalAreasMapController.HospitalMap.playgroud.ExternalHouseState != ExternalRoom.EExternalHouseState.enabled) {
                return false;
            }

            int playerPositiveEnergyAmout = 0;
            int neededPositiveEnergyAmout = 0;

            //to implement calculation of positiveEnergy amounts
            playerPositiveEnergyAmout = GameState.Get().PositiveEnergyAmount;
            neededPositiveEnergyAmout = CalcPositiveEnergyNeeded();

            DailyDealData.PositiveEnergyParams positiveEnergyParams = DailyDealParser.Instance.GetDailyDealData().GetPositiveEnergyParams();
            if (playerPositiveEnergyAmout <= positiveEnergyParams.PositiveEnergyTreshold * neededPositiveEnergyAmout) {
                positiveEnergyNeeded = true;
            }

            return positiveEnergyNeeded;
        }

        private int CalcPositiveEnergyNeeded() {
            int positiveEnergyNeeded = 0;
            
            positiveEnergyNeeded += GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType.Laser);
            positiveEnergyNeeded += GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType.LungTesting);
            positiveEnergyNeeded += GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType.MRI);
            positiveEnergyNeeded += GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType.UltraSound);
            positiveEnergyNeeded += GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType.XRay);

            return positiveEnergyNeeded;
        }

        private int GetNeededPositiveEnergyForDiagRoom(HospitalDataHolder.DiagRoomType roomType) {
            if (!HospitalDataHolder.Instance.DiagnosisRoomIsBuilt(roomType)) {
                return 0;
            }

            if (HospitalDataHolder.Instance.DiagnosisPatientExists(roomType) && HospitalDataHolder.Instance.ReturnDiseaseQueue(roomType).Count == 0)
            {
                return HospitalDataHolder.Instance.NeededPositiveEnergy(roomType);
            }
            return 0;
        }

        #endregion
    }
}

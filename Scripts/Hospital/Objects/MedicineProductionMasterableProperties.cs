using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hospital
{
    public class MedicineProductionMasterableProperties : MasterableProperties
    {
        public MedicineProductionMachine prodMachineClient;

        public MedicineProductionMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is MedicineProductionMachine)
            {
                prodMachineClient = clientInfo as MedicineProductionMachine;
            }
            SetAppearanceController();
            Init(clientInfo);
        }

        public override int CalcTimeToMastershipUpgrade()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                return int.MaxValue;
            }

            float time = 0;
            int medicinesLeft = MasteryGoal - MasteryProgress;
            if (medicinesLeft > prodMachineClient.productionQueue.Count + 1 || prodMachineClient.actualMedicine == null)
            {
                return int.MaxValue;
            }
            for (int i = 0; i < medicinesLeft - 1; ++i)
            {
                time += ResourcesHolder.Get().GetMedicineInfos(prodMachineClient.productionQueue.ElementAt(i)).ProductionTime;
            }
            return Mathf.CeilToInt(prodMachineClient.ActualMedicineProductionTime + time);
        }

        protected override void SetAppearanceController()
        {
            GameObject room = AreaMapController.Map.GetObject(new Vector2i(prodMachineClient.position.x, prodMachineClient.position.y));
            if (room == null)
            {
                return;
            }
            appearanceController = GetAppearanceController<MastershipProductionMachineAppearance>(room);
        }

        protected override void UpdateMasteryMultipliers()
        {
            if (string.Compare(masterableClient.GetClientTag(), "ElixirLab") == 0)
            {
                productionTimeMultiplier = MasteryLevel > 0 ? ((MasterableElixirLabConfigData)masterableConfigData).ProductionTimeMultipliers[MasteryLevel - 1] : 1;
            }
            else
            {
                coinRewardMultiplier = MasteryLevel > 0 ? ((MasterableProductionMachineConfigData)masterableConfigData).GoldMultiplier : 1;
                expRewardMultiplier = MasteryLevel > 1 ? ((MasterableProductionMachineConfigData)masterableConfigData).ExpMultiplier : 1;
                productionTimeMultiplier = MasteryLevel > 2 ? ((MasterableProductionMachineConfigData)masterableConfigData).ProductionTimeMultiplier : 1;

                for (int i = 0; i < HospitalPatientAI.Patients.Count; ++i)
                {
                    HospitalPatientAI.Patients[i].UpdateReward();
                }
            }
        }
    }
}

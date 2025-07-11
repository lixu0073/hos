using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Hospital
{
    public class VitaminMakerMasterableProperties : MasterableProperties
    {
        public VitaminMaker vitMakerClient;

        public VitaminMakerMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is VitaminMaker)
            {
                vitMakerClient = clientInfo as VitaminMaker;
            }
            SetAppearanceController();
            Init(clientInfo);
        }

        public override void LoadFromString(string save, int actionsDone = 0)
        {
            var str = save.Split(';');
            sendEventToDelta = false;
            if (str.Length > 2)
            {
                List<string> strs = str[2].Split('/').Where(p => p.Length > 0).ToList();
                if (strs.Count > 1)
                {
                    int level = int.Parse(strs[0], System.Globalization.CultureInfo.InvariantCulture);
                    int progress = int.Parse(strs[1], System.Globalization.CultureInfo.InvariantCulture);
                    if (level == 0 && progress < actionsDone && actionsDone > 0)
                    {
                        SetMasteryLevel(level);
                        SetMasteryProgress(actionsDone);
                    }
                    else
                    {
                        SetMasteryLevel(level);
                        SetMasteryProgress(progress);
                    }

                }
                else
                {
                    SetMasteryLevel(0);
                    SetMasteryProgress(actionsDone);
                }
            }
            else
            {
                SetMasteryLevel(0);
                SetMasteryProgress(actionsDone);
            }
            sendEventToDelta = true;
        }

        public override int CalcTimeToMastershipUpgrade()
        {
            if (GameState.Get().hospitalLevel < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                return int.MaxValue;
            }

            float time = 0;
            int medicinesLeft = MasteryGoal - MasteryProgress;
            int vitaminToBeGenerated = 0;
            for (int i = 0; i < vitMakerClient.vitaminModels.Count; i++)
            {
                vitaminToBeGenerated += (vitMakerClient.vitaminModels[i].maxCapacity - (int)vitMakerClient.vitaminModels[i].capacity);
            }
            if (medicinesLeft > vitaminToBeGenerated || vitMakerClient.IsCollectorFull())
            {
                return int.MaxValue;
            }

            VitaminCollectorModelComparer vitComparer = new VitaminCollectorModelComparer();
            List<VitaminCollectorModel> sortedModels = new List<VitaminCollectorModel>(vitMakerClient.vitaminModels);
            sortedModels.Sort(vitComparer);

            for (int i = 0; i < sortedModels.Count; i++)
            {
                if (vitMakerClient.vitaminModels[i].FillRatio != 0 && vitMakerClient.vitaminModels[i].maxCapacity != 0)
                {
                    int timeOfProduction = Mathf.CeilToInt(3600f / vitMakerClient.vitaminModels[i].FillRatio);
                    float normalizedTimeToFinishCurrentProduction = Mathf.CeilToInt(vitMakerClient.vitaminModels[i].capacity) - vitMakerClient.vitaminModels[i].capacity;

                    int amountOfVitaminsInQueue = vitMakerClient.vitaminModels[i].maxCapacity - Mathf.CeilToInt(vitMakerClient.vitaminModels[i].capacity);
                    int allVitaminsInProcess = amountOfVitaminsInQueue + (normalizedTimeToFinishCurrentProduction > 0 ? 1 : 0);

                    if (medicinesLeft - allVitaminsInProcess >= 0)
                    {
                        medicinesLeft -= allVitaminsInProcess;
                        time += amountOfVitaminsInQueue * timeOfProduction + normalizedTimeToFinishCurrentProduction * timeOfProduction;
                        if (medicinesLeft == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        time += (medicinesLeft - 1) * timeOfProduction + normalizedTimeToFinishCurrentProduction * timeOfProduction;
                        break;
                    }
                }
                else
                {
                    return int.MaxValue;
                }
            }
            return Mathf.CeilToInt(time);
        }

        protected override void SetAppearanceController()
        {
            GameObject room = AreaMapController.Map.GetObject(new IsoEngine.Vector2i(vitMakerClient.position.x, vitMakerClient.position.y));
            if (room == null)
            {
                return;
            }
            appearanceController = GetAppearanceController<MastershipProductionMachineAppearance>(room);
        }

        protected override void UpdateMasteryMultipliers()
        {
            coinRewardMultiplier = MasteryLevel > 0 ? ((MasterableProductionMachineConfigData)masterableConfigData).GoldMultiplier : 1;
            expRewardMultiplier = MasteryLevel > 1 ? ((MasterableProductionMachineConfigData)masterableConfigData).ExpMultiplier : 1;
            productionTimeMultiplier = MasteryLevel > 2 ? ((MasterableProductionMachineConfigData)masterableConfigData).ProductionTimeMultiplier : 1;


            for (int i = 0; i < vitMakerClient.vitaminModels.Count; i++)
            {
                //vitaminModels[i].UpgradeFillRatioDueToMastery(ProductionTimeMultiplier);
                vitMakerClient.vitaminModels[i].SetFillRatioMultiplierBalanceable(this);
            }


            for (int i = 0; i < HospitalPatientAI.Patients.Count; ++i)
            {
                HospitalPatientAI.Patients[i].UpdateReward();
            }
        }
    }
}


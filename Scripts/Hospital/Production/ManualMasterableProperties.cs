using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public abstract class ManualMasterableProperties : MasterableProperties
    {
        public bool CanBeUpgraded { get; private set; }

        public ManualMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
        }

        public void UpgradeOnPlayersDemand()
        {
            if (CanBeUpgraded)
            {
                int progressLeft = MasteryProgress - MasteryGoal;
                AddMasteryLevel(1);
                SetMasteryProgress(progressLeft);
            }
        }

        public override void CheckMasteryProgress()
        {
            if (MasterableConfigData == null)
            {
                Debug.LogError("MasterableConfigData is null");
                return;
            }

            if (MasteryGoal <= 0)
            {
                return;
            }

            if (IsMaxed())
            {
                CanBeUpgraded = false;
                return;
            }

            CanBeUpgraded = MasteryProgress >= MasteryGoal;
        }


        public int GetItemsRequiredAmount()
        {
            return masterableConfigData.MasteryPrices[Mathf.Min(MasteryLevel, masterableConfigData.MasteryPrices.Length - 1)];
        }

        public abstract override int CalcTimeToMastershipUpgrade();

        protected abstract override void SetAppearanceController();

        protected abstract override void UpdateMasteryMultipliers();

    }
}


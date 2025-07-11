using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class HeliPadMasterableProperties : ManualMasterableProperties
    {
        VIPHelipadMasterableClient client;
        public float vipCoolDownTimeMultiplier;
        public float afterUpgradeVipCoolDownTimeMultiplier;
        public float vipCoolDownPenalizedTimeMultiplier;

        public HeliPadMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is VIPHelipadMasterableClient)
            {
                client = clientInfo as VIPHelipadMasterableClient;
            }
            SetAppearanceController();
            Init(client);
        }

        public override void LoadFromString(string save, int actionsDone = 0)
        {
            var str = save.Split('/');
            sendEventToDelta = false;
            if (str.Length > 1)
            {
                SetMasteryLevel(int.Parse(str[0], System.Globalization.CultureInfo.InvariantCulture));
                SetMasteryProgress(int.Parse(str[1], System.Globalization.CultureInfo.InvariantCulture));
            }
            else
            {
                SetMasteryLevel(0);
                SetMasteryProgress(actionsDone);
            }
            sendEventToDelta = true;
        }

        public override void SetMasteryProgress(int progress)
        {
            base.SetMasteryProgress(progress);
            ReferenceHolder.GetHospital().vipSystemManager.InvokeOnUpgradeUpdate();
        }

        public override void AddMasteryProgress(int amount)
        {
            base.AddMasteryProgress(amount);
            ReferenceHolder.GetHospital().vipSystemManager.InvokeOnUpgradeUpdate();
        }

        public override int CalcTimeToMastershipUpgrade()
        {
            return int.MaxValue;
        }

        protected override void SetAppearanceController()
        {
            if (client == null)
            {
                return;
            }
            appearanceController = client.gameObject.GetComponent<MastershipHeliPadAppearance>();
        }

        protected override void UpdateMasteryMultipliers()
        {
            float[] tempVipArrivalTimeMultipliers = ((MasterableVIPHelipadConfigData)masterableConfigData).VipArrivalTimeMultipliers;
            vipCoolDownTimeMultiplier = MasteryLevel > 0 ? tempVipArrivalTimeMultipliers[MasteryLevel - 1] : 1;
            afterUpgradeVipCoolDownTimeMultiplier = tempVipArrivalTimeMultipliers[Mathf.Min(MasteryLevel, tempVipArrivalTimeMultipliers.Length - 1)];
        }

    }

}


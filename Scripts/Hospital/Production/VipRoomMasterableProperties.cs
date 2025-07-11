using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Hospital
{
    public class VipRoomMasterableProperties : ManualMasterableProperties
    {
        VIPRoomMasterableClient client;
        public float vipCureTimeMultiplier;
        public float afterUpgradeVipCureTimeMultiplier;

        public VipRoomMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is VIPRoomMasterableClient)
            {
                client = clientInfo as VIPRoomMasterableClient;
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
            appearanceController = client.gameObject.GetComponent<MastershipVipRoomAppearance>();
        }

        protected override void UpdateMasteryMultipliers()
        {
            float[] tempPatientInBedTimeMultipliers = ((MasterableVIPRoomConfigData)masterableConfigData).PatientInBedTimeMultipliers;

            vipCureTimeMultiplier = MasteryLevel > 0 ? tempPatientInBedTimeMultipliers[MasteryLevel - 1] : 1;
            afterUpgradeVipCureTimeMultiplier = tempPatientInBedTimeMultipliers[Mathf.Min(MasteryLevel, tempPatientInBedTimeMultipliers.Length - 1)];
        }
    }
}

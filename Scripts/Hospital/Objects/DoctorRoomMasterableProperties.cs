using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class DoctorRoomMasterableProperties : MasterableProperties
    {
        private float positiveEnergyRewardMultiplier = 1f;
        public float PositiveEnergyRewardMultiplier { get { return positiveEnergyRewardMultiplier; } }

        public DoctorRoom doctorRoom;

        public DoctorRoomMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is DoctorRoom)
            {
                doctorRoom = clientInfo as DoctorRoom;
            }
            SetAppearanceController();
            Init(clientInfo);
        }

        public override int CalcTimeToMastershipUpgrade()
        {
            if (Game.Instance.gameState().GetHospitalLevel() < DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                return int.MaxValue; ;
            }

            int patientsLeft = MasteryGoal - MasteryProgress;
            if (patientsLeft > doctorRoom.CureAmount || doctorRoom.currentPatient == null)
            {
                return int.MaxValue;
            }

            return Mathf.CeilToInt(doctorRoom.CureTimeMastered - doctorRoom.curationTime + (patientsLeft - 1) * doctorRoom.CureTimeMastered);
        }

        protected override void UpdateMasteryMultipliers()
        {
            coinRewardMultiplier = MasteryLevel > 0 ? ((MasterableDoctorRoomConfigData)masterableConfigData).GoldMultiplier : 1;
            expRewardMultiplier = MasteryLevel > 1 ? ((MasterableDoctorRoomConfigData)masterableConfigData).ExpMultiplier : 1;
            positiveEnergyRewardMultiplier = MasteryLevel > 2 ? ((MasterableDoctorRoomConfigData)masterableConfigData).PositiveEnergyMultiplier : 1;
            productionTimeMultiplier = MasteryLevel > 3 ? ((MasterableDoctorRoomConfigData)masterableConfigData).CureTimeMultiplier : 1;
        }

        protected override void SetAppearanceController()
        {
            GameObject room = AreaMapController.Map.GetObject(new Vector2i(doctorRoom.position.x, doctorRoom.position.y));
            if (room == null)
            {
                return;
            }
            appearanceController = GetAppearanceController<MastershipDoctorRoomAppearance>(room);
        }
    }
}

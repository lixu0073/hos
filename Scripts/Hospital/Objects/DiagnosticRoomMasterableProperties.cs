using IsoEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class DiagnosticRoomMasterableProperties : MasterableProperties
    {
        public DiagnosticRoom diagRoom;

        public DiagnosticRoomMasterableProperties(MasterablePropertiesClient clientInfo) : base(clientInfo)
        {
            if (clientInfo is DiagnosticRoom)
            {
                diagRoom = clientInfo as DiagnosticRoom;
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

            int patientsLeft = MasteryGoal - MasteryProgress;

            if ((diagRoom.privateQueue != null && patientsLeft > diagRoom.privateQueue.Count) || diagRoom.currentPatient == null)
            {
                return int.MaxValue;
            }

            return Mathf.CeilToInt(diagRoom.DiagnosisTimeMastered - diagRoom.currentPatient.GetDiagnoseTime() + (patientsLeft - 1) * diagRoom.DiagnosisTimeMastered);
        }

        protected override void SetAppearanceController()
        {
            GameObject room = AreaMapController.Map.GetObject(new Vector2i(diagRoom.position.x, diagRoom.position.y));
            if (room == null)
            {
                return;
            }
            appearanceController = GetAppearanceController<MastershipDiagnosticRoomAppearance>(room);
        }

        protected override void UpdateMasteryMultipliers()
        {
            float diagnosisTimeMultiplier = MasteryLevel > 0 ? ((MasterableDiagnosticMachineConfigData)MasterableConfigData).ProductionTimeMultipliers[MasteryLevel - 1] : 1;

            productionTimeMultiplier = diagnosisTimeMultiplier;
        }
    }
}


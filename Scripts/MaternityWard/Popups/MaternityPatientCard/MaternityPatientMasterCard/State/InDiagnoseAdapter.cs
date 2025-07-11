using Maternity.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.Adapter
{
    public class InDiagnoseAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public InDiagnoseAdapter(MaternityPatientMasterCardController controller, IMaternityTreatmentPanelUI ui) : base(controller, ui) { }

        public override void SetUp()
        {
            base.SetUp();
            ui.SetDiagnoseInProgressView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Diagnose).ToString(),
                CheckButton,
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.buttonCheckKey).ToUpper(),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.patientDiagnoseKey),
                I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.analysingKey)
                );
            SetListeners();
        }

        private void CheckButton()
        {
            ClosePopupAndRedirectToBloodTestRoom(true);
        }

        private void SetListeners()
        {
            controller.Ai.OnDataRecieved_ID += Ai_OnDataReceived_ID;
        }

        private void Ai_OnDataReceived_ID(PatientStates.MaternityPatientInDiagnoseState.Data data)
        {
            int timeLeft = (int)Math.Ceiling(data.timeLeft);
            ui.SetTimerDiagnoseTreatmentStagePanel(UIController.GetFormattedTime(timeLeft));
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if(controller.Ai)
                controller.Ai.OnDataRecieved_ID -= Ai_OnDataReceived_ID;
        }

    }
}

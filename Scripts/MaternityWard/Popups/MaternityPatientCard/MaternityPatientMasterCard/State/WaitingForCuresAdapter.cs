using System;
using System.Collections;
using System.Collections.Generic;
using Maternity.UI;
using UnityEngine;
using Hospital;
using SimpleUI;

namespace Maternity.Adapter
{
    public class WaitingForCuresAdapter : MaternityPatientMasterCardBaseAdapter
    {
        public WaitingForCuresAdapter(MaternityPatientMasterCardController masterController, IMaternityTreatmentPanelUI masterUI) : base(masterController, masterUI) { }

        public override void SetUp()
        {
            base.SetUp();
            List<TreatmentPanelData> cures = new List<TreatmentPanelData>();
            foreach (KeyValuePair<MedicineDatabaseEntry, int> entry in controller.Info.RequiredCures)
            {
                cures.Add(new TreatmentPanelData(entry.Key, entry.Value, 0));
            }
            string stageTitle = I2.Loc.ScriptLocalization.Get(ResourcesHolder.GetMaternity().maternityLocKeys.vitaminesListTitleKey);
            ui.SetVitaminesView(
                GetPatientInfo(),
                controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Vitamines).ToString(),
                OnActionButtonCliked,
                ResourcesHolder.GetMaternity().cureIcon,
                stageTitle,
                cures
            );
            if (!Game.Instance.gameState().IsMaternityFirstLoopCompleted)
                ui.SetCureButtonCurePanel(OnActionButtonCliked, I2.Loc.ScriptLocalization.Get("FREE").ToUpper(), null, true);
        }

        protected virtual void OnActionButtonCliked()
        {
            if(!Game.Instance.gameState().IsMaternityFirstLoopCompleted)
            {
                controller.Ai.Notify((int)StateNotifications.CuresDelivered, null);
                AddExperience(controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Vitamines), EconomySource.MaternityMotherVitaminized);
                return;
            }
            List<KeyValuePair<int, MedicineDatabaseEntry>> missingItems = new List<KeyValuePair<int, MedicineDatabaseEntry>>();

            foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in controller.Info.RequiredCures)
            {
                MedicineRef item = pair.Key.GetMedicineRef();
                int currentCureCount = Game.Instance.gameState().GetCureCount(item);
                if (pair.Value > currentCureCount)
                {
                    missingItems.Add(new KeyValuePair<int, MedicineDatabaseEntry>(pair.Value - currentCureCount, pair.Key));
                }
            }

            if (missingItems.Count > 0)
            {
                UIController.get.BuyResourcesPopUp.Open(missingItems, false, false, false, () =>
                {
                    CuresSuccessDelivered();
                }, null, null);
            }
            else
            {
                CuresSuccessDelivered();
            }
        }

        private void CuresSuccessDelivered()
        {
            foreach (KeyValuePair<MedicineDatabaseEntry, int> pair in controller.Info.RequiredCures)
            {
                Game.Instance.gameState().GetCure(pair.Key.GetMedicineRef(), pair.Value, EconomySource.CureMaternityPatient);
            }
            controller.Ai.Notify((int)StateNotifications.CuresDelivered, null);
            AddExperience(controller.Info.GetExpForStage(MaternityCharacterInfo.Stage.Vitamines), EconomySource.MaternityMotherVitaminized);
        }

    }
}

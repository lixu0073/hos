using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{
    public class MaternityStagesPanelUI : MonoBehaviour
    {
        private const float XMIN = 0.95f, XMAX = 1.15f;

        [SerializeField]
        private MaternityStageIndicatorUI diagnoseStage = null;
        [SerializeField]
        private MaternityStageIndicatorUI vitaminesStage = null;
        [SerializeField]
        private MaternityStageIndicatorUI waitingForLaborStage = null;
        [SerializeField]
        private MaternityStageIndicatorUI inLaborStage = null;
        [SerializeField]
        private MaternityStageIndicatorUI healingAndBondingStage = null;

        [SerializeField]
        private RectTransform currentStageIndicator = null;

        public void SetStagesPanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetDiagnoseStageHighlighted()
        {
            SetStageIndicatorsHighlighted(diagnoseStage.stageID);
            SetCheckmarksActive(diagnoseStage.stageID);
            ChangeStageIndicator(new Vector2(XMIN, 0.8f), new Vector2(XMAX, 1f));

        }

       

        public void SetVitaminesStageHighlighted()
        {
            SetStageIndicatorsHighlighted(vitaminesStage.stageID);
            SetCheckmarksActive(vitaminesStage.stageID);
            ChangeStageIndicator(new Vector2(XMIN, 0.6f), new Vector2(XMAX, 0.8f));
        }

        public void SetWaitingForLaborStageHighlighted()
        {
            SetStageIndicatorsHighlighted(waitingForLaborStage.stageID);
            SetCheckmarksActive(waitingForLaborStage.stageID);
            ChangeStageIndicator(new Vector2(XMIN, 0.4f), new Vector2(XMAX, 0.6f));
        }

        public void SetInLaborStageHighlighted()
        {
            SetStageIndicatorsHighlighted(inLaborStage.stageID);
            SetCheckmarksActive(inLaborStage.stageID);
            ChangeStageIndicator(new Vector2(XMIN, 0.2f), new Vector2(XMAX, 0.4f));
        }

        public void SetHealingAndBondingStageHighlighted()
        {
            SetStageIndicatorsHighlighted(healingAndBondingStage.stageID);
            SetCheckmarksActive(healingAndBondingStage.stageID);
            ChangeStageIndicator(new Vector2(XMIN, 0.0f), new Vector2(XMAX, 0.2f));
        }

        private void SetStageIndicatorsHighlighted(int id)
        {
            diagnoseStage.SetStageIndicatorHighlighted(id == diagnoseStage.stageID);
            vitaminesStage.SetStageIndicatorHighlighted(id == vitaminesStage.stageID);
            waitingForLaborStage.SetStageIndicatorHighlighted(id == waitingForLaborStage.stageID);
            inLaborStage.SetStageIndicatorHighlighted(id == inLaborStage.stageID);
            healingAndBondingStage.SetStageIndicatorHighlighted(id == healingAndBondingStage.stageID);
        }

        private void SetCheckmarksActive(int id)
        {
            diagnoseStage.SetCheckmarkActive(id > diagnoseStage.stageID);
            vitaminesStage.SetCheckmarkActive(id > vitaminesStage.stageID);
            waitingForLaborStage.SetCheckmarkActive(id > waitingForLaborStage.stageID);
            inLaborStage.SetCheckmarkActive(id > inLaborStage.stageID);
            healingAndBondingStage.SetCheckmarkActive(id > healingAndBondingStage.stageID);
        }


        private void ChangeStageIndicator(Vector2 xyMinAchros, Vector2 XyMaxAnchors)
        {
            currentStageIndicator.offsetMax = Vector2.zero;
            currentStageIndicator.offsetMin = Vector2.zero;
            currentStageIndicator.anchorMin = xyMinAchros; 
            currentStageIndicator.anchorMax = XyMaxAnchors;
        }
    }
}

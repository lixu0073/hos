using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{

    public class MastershipProductionMachineAppearance : MonoBehaviour, MastershipRoomAppearance
    {
        [SerializeField]
        private ProductionMachineGauge gaugeController = null;
        [SerializeField]
        private Animator machineAnimator = null;

        public void SetAppearance(int masteryLevel, bool showAnimation = true)
        {
            if (gaugeController == null)
            {
                return;
            }

            if (showAnimation)
            {
                gaugeController.MasteryAnimation(masteryLevel, "Mastership");
            }
            else
            {
                gaugeController.SetAppearance(masteryLevel);
            }
        }

        public void TurnGaugeRed()
        {
            if (machineAnimator != null)
            {
                machineAnimator.SetTrigger("TurnRed");
            }
        }

        private void Start()
        {
            if (BoosterEffectManager.Instance.productionObjects != null) BoosterEffectManager.Instance.productionObjects.Add(this);
        }

        private void OnDestroy()
        {
            if (BoosterEffectManager.Instance.productionObjects != null) BoosterEffectManager.Instance.productionObjects.Remove(this);
        }
    }
}

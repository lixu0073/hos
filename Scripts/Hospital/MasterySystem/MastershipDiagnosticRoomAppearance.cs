using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class MastershipDiagnosticRoomAppearance : MonoBehaviour, MastershipRoomAppearance
    {
        [SerializeField]
        private DiagnosticRoomStars starsController = null;

        public void SetAppearance(int masteryLevel, bool showAnimation = true)
        {
            if (starsController == null)
            {
                return;
            }
            starsController.SetAppearance(masteryLevel);
        }

        private void Start()
        {
            if (BoosterEffectManager.Instance.diagnosticsObjects != null) BoosterEffectManager.Instance.diagnosticsObjects.Add(this);
        }

        private void OnDestroy()
        {
            if (BoosterEffectManager.Instance.diagnosticsObjects != null) BoosterEffectManager.Instance.diagnosticsObjects.Remove(this);
        }
    }
}

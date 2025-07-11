using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class MastershipVipRoomAppearance : MonoBehaviour, MastershipRoomAppearance
    {
        // Properties that has references to all changable objects
        // ScriptableObject that will contain all pictures agregated to list
        [SerializeField]
        private VisualsUpgradeController[] visualUpgradeControllers = null;

        public void SetAppearance(int masteryLevel, bool showAnimation = true)
        {
            if (visualUpgradeControllers == null)
            {
                return;
            }

            for (int i = 0; i < visualUpgradeControllers.Length; ++i)
            {
                visualUpgradeControllers[i].SetLevel(masteryLevel);
            }

            if (HospitalAreasMapController.HospitalMap.fakeWallVisualUpgrade != null)
            {
                HospitalAreasMapController.HospitalMap.fakeWallVisualUpgrade.SetLevel(masteryLevel);
            }
        }
    }
}

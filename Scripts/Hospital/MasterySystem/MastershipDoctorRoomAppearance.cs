using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hospital
{
    public class MastershipDoctorRoomAppearance : MonoBehaviour, MastershipRoomAppearance
    {

        [SerializeField]
        private DoctorRoomStars starsController = null;

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
            if (BoosterEffectManager.Instance.doctorObjects != null) BoosterEffectManager.Instance.doctorObjects.Add(this);
        }

        private void OnDestroy()
        {
            if (BoosterEffectManager.Instance.doctorObjects != null) BoosterEffectManager.Instance.doctorObjects.Remove(this);
        }
    }
}

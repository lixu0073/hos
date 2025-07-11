using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleUI
{
    public abstract class MastershipHover : BaseHover
    {
        [SerializeField]
        private GameObject mastershipButton = null;

        [SerializeField]
        private StarsController stars3 = null;
        [SerializeField]
        private StarsController stars4 = null;

        private StarsController activeStars = null;
        
        public abstract void ButtonMastership();

        public abstract void UpdateStars();

        protected void SetMastershipButton(Hospital.MasterableProperties masterableObject) {
            if (mastershipButton == null) {
                return;
            }

            if (Game.Instance.gameState().GetHospitalLevel() >= DefaultConfigurationProvider.GetConfigCData().MastershipMinLevel)
            {
                mastershipButton.SetActive(true);
                ShowStars(masterableObject);
            } else {
                mastershipButton.SetActive(false);
            }

        }

        private void ShowStars(Hospital.MasterableProperties masterableObject)
        {
            ChoseStarsContent(masterableObject.MasterableConfigData.MasteryGoals.Length < 4);
            SetStars(masterableObject.MasteryLevel);
        }

        protected void ChoseStarsContent(bool is3)
        {
            if (stars3 == null)
            {
                Debug.LogError("stars3 is null");
                return;
            }

            if (stars4 == null)
            {
                Debug.LogError("stars4 is null");
                return;
            }

            if (is3)
            {
                activeStars = stars3;
                stars3.gameObject.SetActive(true);
                stars4.gameObject.SetActive(false);
            }
            else {
                activeStars = stars4;
                stars3.gameObject.SetActive(false);
                stars4.gameObject.SetActive(true);
            }
        }

        protected void SetStars(int level)
        {
            if (activeStars == null)
            {
                Debug.LogError("activeStars is null");
                return;
            }

            activeStars.SetStarsVisible(level);
        }
    }
}


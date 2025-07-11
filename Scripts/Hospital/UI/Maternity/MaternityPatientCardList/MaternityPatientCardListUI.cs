using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{
    public class MaternityPatientCardListUI : MonoBehaviour, IMaternityPatientCardListUI
    {
        [SerializeField]
        private MaternityBedPanelUI bedPanelPrefab = null;

        [SerializeField]
        private Transform content = null;

        private List<MaternityBedPanelUI> bedPanels = new List<MaternityBedPanelUI>();

        public void SetPatientCardListActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void ClearSelectedIndicators()
        {
            foreach(MaternityBedPanelUI bedPanelUI in bedPanels)
            {
                bedPanelUI.SetSelectedIndicatorActive(false);
            }
        }

        public MaternityBedPanelUI AddBedPanel()
        {
            MaternityBedPanelUI bedPanel = Instantiate(bedPanelPrefab, content);
            bedPanels.Add(bedPanel);
            return bedPanel;
        }

        public void ClearList()
        {
            for (int i = 0; i < bedPanels.Count; ++i)
            {
                Destroy(bedPanels[i].gameObject);
            }

            bedPanels.Clear();
        }
    }
}

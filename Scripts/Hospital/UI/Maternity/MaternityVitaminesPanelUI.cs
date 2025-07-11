using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maternity.UI
{
    public class MaternityVitaminesPanelUI : MonoBehaviour
    {
        [SerializeField]
        private TreatmentPanel treatmentPanelPrefab = null;

        private List<TreatmentPanel> treatmentPanels = new List<TreatmentPanel>();

        public void SetVitaminesPanelActive(bool setActive)
        {
            gameObject.SetActive(setActive);
        }

        public void SetVitaminesPanel(List<TreatmentPanelData> treatmentDataList)
        {
            ClearTreatmentPanels();

            for (int i = 0; i < treatmentDataList.Count; ++i)
            {
                treatmentPanels.Add(Instantiate(treatmentPanelPrefab, transform));
                treatmentPanels[i].Initialize(treatmentDataList[i]);
            }
        }

        private void ClearTreatmentPanels()
        {
            for (int i = 0; i < treatmentPanels.Count; ++i)
            {
                Destroy(treatmentPanels[i].gameObject);
            }

            treatmentPanels.Clear();
        }
    }
}

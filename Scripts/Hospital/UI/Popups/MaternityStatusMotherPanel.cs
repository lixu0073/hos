using System.Collections.Generic;
using UnityEngine;

public class MaternityStatusMotherPanel : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private List<MaternityStatusMotherPanelElement> panelElements;
#pragma warning restore 0649

    public void InitializeData(MaternityStatusMotherPanelData data)
    {
        for (int i = 0; i < panelElements.Count; i++)
        {
            int amountOfMothersForStatus = data.GetAmountOfMothersForStatus(panelElements[i].GetStatusType());
            panelElements[i].SetActive(amountOfMothersForStatus > 0);
            panelElements[i].SetAmountText(amountOfMothersForStatus);
            if (panelElements[i].GetStatusType() == Maternity.PatientStates.MaternityPatientStateTag.WFCR)
            {
                panelElements[i].SetPresent(data.presentType);
            }
        }
    }
}

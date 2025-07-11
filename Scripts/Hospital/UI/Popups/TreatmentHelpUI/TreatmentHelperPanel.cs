using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hospital;

public class TreatmentHelperPanel : MonoBehaviour {
    [SerializeField]
    private GameObject curesPanel = null;

    [SerializeField]
    private GameObject providedMedicinePrefab = null;

    [SerializeField]
    private ProviderFriendCardController friendController = null;
    
    public void SetTreatmentHelperPanel(TreatmentProviderInfo provider) {
        SetPlayerAvatar(provider);
        SetCuresPanel(provider.GetProvidedMedicines());
    }

    private void SetPlayerAvatar(IFollower provider) {
        friendController.Initialize(provider, VisitingEntryPoint.TreatmentHelpSummary);
    }

    private void SetCuresPanel(List<ProvidedMedicineInfo> medicines) {
        if (curesPanel != null && curesPanel.transform.childCount > 0)
        {
            for (int i = 0; i < curesPanel.transform.childCount; ++i)
            {
                Destroy(curesPanel.transform.GetChild(i));
            }
        }

        if (medicines == null) {
            Debug.LogError("medicines list is null");
            return;
        }

        for (int i = 0; i < medicines.Count; ++i)
        {
            GameObject cure = Instantiate(providedMedicinePrefab, curesPanel.transform);
            ProvidedMedicineView view = cure.GetComponent<ProvidedMedicineView>();
            if (view != null) {
                view.SetMedicine(medicines[i].medicine, medicines[i].donatedAmount);
            }
        }
    }
}

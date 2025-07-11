using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleUI;

public class TreatmentHelpSummaryUI : UIElement
{
    [SerializeField] private Transform helpersList = null;
    [SerializeField] private GameObject treatmentHelperPanelPrefab = null;

    private OnEvent onExit = null;
    
    private void SetOKButtonPink()
    {
        Transform OKButton = transform.GetChild(5);
        if (OKButton == null)        
            return;
        
        Image OKBackground = OKButton.GetComponent<Image>();
        if (OKBackground == null)
            return;
        
        UIController.SetImageSpriteSecure(OKBackground, ResourcesHolder.Get().pink9SliceButton);
    }

    public void Open(List<TreatmentProviderInfo> providers, OnEvent onExit)
    {
        this.onExit = onExit;
        SetHelpersList(providers);
        SetOKButtonPink(); //zuo
        gameObject.SetActive(true);
        StartCoroutine(base.Open());
    }

    private void SetHelpersList(List<TreatmentProviderInfo> providers)
    {
        for (int i = 0; i < helpersList.childCount; ++i)
        {
            Destroy(helpersList.GetChild(i).gameObject);
        }

        if (providers == null)
        {
            Debug.LogError("medicines list is null");
            return;
        }

        for (int i = 0; i < providers.Count; ++i)
        {
            GameObject providerPanel = Instantiate(treatmentHelperPanelPrefab, helpersList);
            TreatmentHelperPanel view = providerPanel.GetComponent<TreatmentHelperPanel>();
            if (view != null)
            {
                view.SetTreatmentHelperPanel(providers[i]);
            }
        }
    }

    public void ExitButton()
    {
        onExit?.Invoke();
        base.Exit();
    }

    public void OKButton()
    {
        onExit?.Invoke();
        base.Exit();
    }

}

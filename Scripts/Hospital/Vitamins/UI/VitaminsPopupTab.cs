using Hospital;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VitaminsPopupTab : PopupTab
{
#pragma warning disable 0649
    [SerializeField]
    List<VitaminUI> VitaminsRepresentations;
#pragma warning restore 0649
    [SerializeField]
    ScrollRect scroll = null;

    public void SetupVitaminViews(List<VitaminCollectorModel> models)
    {
        List<VitaminCollectorModel> sortedModels = System.Linq.Enumerable.ToList(System.Linq.Enumerable.OrderBy(models, x => x.GetVitaminCollectorUnlockLevel()));
        for (int i = 0; i < sortedModels.Count; i++)
        {
            if (sortedModels[i] == null) Debug.LogError("model == null!");
            VitaminsRepresentations[i].VitaminSetup(sortedModels[i]);
        }
    }

    protected override void OnPopupOpen()
    {
        if (VitaminsRepresentations != null && VitaminsRepresentations.Count > 0)
        {
            for (int i = 0; i < VitaminsRepresentations.Count; i++)
            {
                VitaminsRepresentations[i].OnOpen();
            }
        }
    }

    protected override void PrepareContent()
    {
        scroll.horizontalNormalizedPosition = 0;
        NotificationCenter.Instance.VitaminMakerPopupOpen.Invoke(new BaseNotificationEventArgs());
        if (VitaminsRepresentations!=null && VitaminsRepresentations.Count > 0)
        {
            for (int i = 0; i < VitaminsRepresentations.Count; i++)
            {
                VitaminsRepresentations[i].PrepareContent();
            }
        }
    }

    protected override void OnPopupClose()
    {
        if (VitaminsRepresentations != null && VitaminsRepresentations.Count > 0)
        {
            for (int i = 0; i < VitaminsRepresentations.Count; i++)
            {
                VitaminsRepresentations[i].OnClose();
            }
        }
    }

    protected override void OnTabSwitchFromCurrent()
    {
        if (VitaminsRepresentations != null && VitaminsRepresentations.Count > 0)
        {
            for (int i = 0; i < VitaminsRepresentations.Count; i++)
            {
                VitaminsRepresentations[i].OnClose();
            }
        }
    }
}

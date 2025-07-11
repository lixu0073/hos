using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PopupTab : MonoBehaviour {
    public void PopupOpen() {
        OnPopupOpen();
    }
    public void PopupClose()
    {
        OnPopupClose();
    }
    public void SetTabSelected(bool selected) {
        gameObject.SetActive(selected);
        if (selected)
        {
            PrepareContent();
        }
    }

    public void CloseTab()
    {
        OnTabSwitchFromCurrent();
    }

    protected abstract void PrepareContent();
    protected abstract void OnPopupOpen();
    protected abstract void OnPopupClose();
    protected abstract void OnTabSwitchFromCurrent();
}

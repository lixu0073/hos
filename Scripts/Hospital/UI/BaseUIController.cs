using SimpleUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUIController<DataType, PopupUI> : MonoBehaviour
{
    public DataType data;

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
    protected PopupUI GetPopupUI()
    {
        if (gameObject == null)
        {
            return default(PopupUI);
        }
        return gameObject.GetComponent<PopupUI>();
    }

    public PopupUI GetPopup()
    {
        return GetPopupUI();
    }

    public virtual void Initialize(DataType dataType)
    {
        if (dataType != null)
        {
            data = dataType;
            OnViewInitialize();
            gameObject.SetActive(true);
        }
    }

    public virtual void DeInitialize()
    {
        OnViewDeInitialize();
    }
    public virtual void RefreshDataWhileOpened(DataType dataType)
    {
        if (data != null)
        {
            data = dataType;
            OnRefreshDataWhileOpened();
        }
    }

    protected abstract void OnViewInitialize();
    protected abstract void OnViewDeInitialize();
    protected abstract void OnRefreshDataWhileOpened();
}

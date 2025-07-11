using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseUIInitializer<DataType, PopupController> : MonoBehaviour, IPopupInicializer
{
    protected PopupController popupController;
    public virtual void Initialize(Action OnSuccess, Action OnFailure)
    {
        ApplyPopupController();
    }
    public virtual void DeInitialize()
    {
    }
    protected void ApplyPopupController()
    {
        if (popupController == null)
        {
            popupController = GetComponent<PopupController>();
            if (popupController == null)
            {
                AddPopupControllerRuntime();
            }
        }
    }
    protected void ReinitializeWhileOpened()
    {
        if (gameObject.activeInHierarchy)
        {
            Refresh(PreparePopupData());
        }
    }
    protected abstract void AddPopupControllerRuntime();
    protected abstract DataType PreparePopupData();
    protected abstract void Refresh(DataType dataType);

}

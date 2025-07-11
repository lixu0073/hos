using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MaternityStatusPopupBaseStrategy
{
    public virtual void SetupPanel(MaternityStatusPopup panel, MaternityStatusData data)
    {
        ClearPanel(panel);
    }

    private void ClearPanel(MaternityStatusPopup panel)
    {
        panel.ClearPanel();
    }

}

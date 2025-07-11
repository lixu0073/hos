using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityStatusPopupEmptyStrategy : MaternityStatusPopupBaseStrategy
{

    public override void SetupPanel(MaternityStatusPopup panel, MaternityStatusData data)
    {
        base.SetupPanel(panel, data);
        panel.SetEmptyPanel();
        panel.SetGoToMaternityButton(data.goTomaternityButtonAction);
    }
}

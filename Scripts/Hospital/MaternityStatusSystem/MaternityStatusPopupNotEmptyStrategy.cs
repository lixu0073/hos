using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityStatusPopupNotEmptyStrategy : MaternityStatusPopupBaseStrategy
{
    public override void SetupPanel(MaternityStatusPopup panel, MaternityStatusData data)
    {
        base.SetupPanel(panel, data);
        panel.SetPanelWithInfo();
        panel.GetVitaminPanel().InitializeData(data.vitData);
        panel.GetMotherPanel().InitializeData(data.motherData);
        panel.SetGoToMaternityButton(data.goTomaternityButtonAction);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MaternityStatusData
{
    public MaternityStatusVitaminPanelData vitData;
    public MaternityStatusMotherPanelData motherData;
    public UnityAction goTomaternityButtonAction;
    public MaternityStatusPopupBaseStrategy mainPopupStrategy = new MaternityStatusPopupEmptyStrategy();
}

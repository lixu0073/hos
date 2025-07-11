using Hospital;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class BusStopObject : SuperObject
{
    public override void IsoDestroy() {}

    bool ProcessingBackToMainMap = false;

    public override void OnClick()
    {
        if (ProcessingBackToMainMap)
            return;
        ProcessingBackToMainMap = true;
        SaveSynchronizer.Instance.InstantSave();
        LocalNotificationController.Instance.CacheNotifications();
        AnalyticsController.instance.ReportChangeScene(false, "MainScene");
        UIController.get.LoadingPopupController.Open(0, 0, 0);
        AreaMapController.Map.IsoDestroy();
        Invoke("RedirectToMainMap", 0.4f);
    }

    private void RedirectToMainMap()
    {
        SceneManager.LoadScene("RedirectToMainScene");
    }

}
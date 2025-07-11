using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tenjin第三方归因平台控制器，负责跟踪和分析用户行为数据。
/// 主要功能包括：用户获取追踪、应用内购买数据上报、关键事件监控等。
/// </summary>
public class TenjinController : MonoBehaviour
{

    public static TenjinController instance;
    const string tenjinApiKey = "QYWJLEJEXUWRXFYIA4OA5XDXCAT2UFHR";

    void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        Tenjin.getInstance(tenjinApiKey).OptIn();
        Tenjin.getInstance(tenjinApiKey).Connect();
    }

    //void OnApplicationPause(bool pauseStatus)
    //{
    //    if (pauseStatus)
    //    {
    //        //do nothing
    //    }
    //    else
    //    {
    //        Tenjin.getInstance(tenjinApiKey).OptIn();
    //        Tenjin.getInstance(tenjinApiKey).Connect();
    //    }
    //}

    public void ReportAdsWatched(int adsWatched)
    {
        Tenjin.getInstance(tenjinApiKey).SendEvent("ads_watched", adsWatched.ToString());
    }

    public void ReportHelpsCounter(int helpsCounter)
    {
        Tenjin.getInstance(tenjinApiKey).SendEvent("helps_amount", helpsCounter.ToString());
    }

    public void ReportFBInvite(int level)
    {
        Tenjin.getInstance(tenjinApiKey).SendEvent("invite_fb_friends_on_level", level.ToString());
    }

    public void ReportLevelUp(int level)
    {
        int[] levelsToTrack = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110 };
        if (levelsToTrack.Contains(level))
        {
            int gameplayTime = (int)GameState.Get().GameplayTimer;
            Debug.Log("Reporting tenjin level up event for level: " + level + " gameplay time: " + gameplayTime);
            Tenjin.getInstance(tenjinApiKey).SendEvent("level" + level, gameplayTime.ToString());
        }
    }

    public void SendFirstPurchaseEvent()
    {
        Tenjin.getInstance(tenjinApiKey).SendEvent("FirstPurchase");
    }

    public void ReportIAP(Product product)
    {
        //uncomment after testing receipt validation
        if (Debug.isDebugBuild || DeveloperParametersController.Instance().parameters.TenjinTestBuild)
            return;

#if UNITY_IPHONE
        Tenjin.getInstance(tenjinApiKey).Transaction(
            product.definition.id, 
            product.metadata.isoCurrencyCode, 
            1, 
            (double)product.metadata.localizedPrice, 
            product.transactionID,
            IAPReceiptHelper.GetBase64EncodedReceipt(product.receipt),
            null        //null on iOS
            );
#else
        Tenjin.getInstance(tenjinApiKey).Transaction(
            product.definition.id,
            product.metadata.isoCurrencyCode,
            1,
            (double)product.metadata.localizedPrice,
            null,       //null on Android
            IAPReceiptHelper.GetPurchaseDataFromReceipt(product.receipt),
            IAPReceiptHelper.GetSignatureFromReceipt(product.receipt)
            );
#endif
    }
}

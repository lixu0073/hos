using UnityEngine;
using System.Collections;
using Hospital;
using System;

/// <summary>
/// 新手礼包控制器，负责管理游戏新手礼包的显示和购买逻辑。
/// 根据玩家等级和购买状态控制新手礼包的显示时机和优惠内容。
/// </summary>
public class StarterPack : MonoBehaviour
{
    const int CHECK_INTERVAL = 60;     //[seconds] time between checking for new offer

    bool offerActive;
    //private static DecisionPointCalss decisionPointCalss;
#pragma warning disable 0649
    Coroutine _showHardWithDelay;
#pragma warning restore 0649

    private void OnDisable()
    {
        if (_showHardWithDelay != null)
        {
            try
            {
                StopCoroutine(_showHardWithDelay);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Coroutine stopping crashed: " + e.Message);
            }
        }
    }

    public void ActivateStarterPack()
    {
       // this.imageMessage.GetGameobject().SetActive(false);
        offerActive = true;
        CheckForStarterPack();
    }

    //public void SetDecisionPointClass(DecisionPointCalss dpc)
    //{
    //    decisionPointCalss = dpc;
    //}

    public void CheckForStarterPack()
    {
        Debug.LogError("CheckForStarterPack used to come from DDNA");
        CancelInvoke("CheckForStarterPack");
        Invoke("CheckForStarterPack", CHECK_INTERVAL);

        IGameState gameState;
        try
        {
            gameState = Game.Instance.gameState();
        }
        catch
        {
            return;
        }

        if (Game.Instance.gameState() == null || gameState.GetHospitalLevel() < 6 || gameState.IsStarterPackUsed() || gameState.GetHospitalLevel() > 10)
            return;            
        
        if (AreaMapController.Map != null && AreaMapController.Map.VisitingMode)
            return;
        
        if (offerActive)
            ShowButton();
        else
        {
            HideButton();
            //AnalyticsController.instance.ReportStarterPack();
        }
    }

    public void ShowButton()
    {
        UIController.get.starterPackButton.SetActive(true);
        if (UIController.get.timedOfferButton != null)
            UIController.get.timedOfferButton.gameObject.SetActive(false);
    }

    public void HideButton()
    {
        UIController.get.starterPackButton.SetActive(false);
    }
    
    public void ShowVGP()
    {
        if ( Game.Instance.gameState().IsStarterPackUsed())
        {
            HideButton();
            return;
        }

        //TODO
        //if (decisionPointCalss != null)
        //{
            AnalyticsController.currentIAPFunnel = CurrentIAPFunnel.VGP;
            AnalyticsController.instance.ReportFunnel(AnalyticsFunnel.IAPVGP.ToString(), (int)FunnelStepIAPVGP.VGPOpen, FunnelStepIAPVGP.VGPOpen.ToString());

        //    decisionPointCalss.imageMessage.GetGameobject().SetActive(true); // CV

        //    decisionPointCalss.ShowWithText();
        //}
        //else
        //{
            offerActive = false;
            HideButton();
            CheckForStarterPack();
        //}
    }

    public void ShowStarterPackHard(float delay)
    {
        if (Game.Instance.gameState().IsStarterPackUsed())
            return;

        //Debug.LogError("Starter Pack ShowStarterPackHard delay: " + delay + " Time.time = " + Time.time);
        StartCoroutine(ShowHardWithDelay(delay));
    }

    IEnumerator ShowHardWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //Debug.LogError("ShowHardWithDelay Time.time = " + Time.time);
        ShowVGP();
    }

    public void ReportPurchased()
    {
        Game.Instance.gameState().SetStarterPackUsed(true);
        HideButton();
        AnalyticsController.instance.ReportStarterPackPurchased();
    }

    public static void OnCloseImage() { }
}

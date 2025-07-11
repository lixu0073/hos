using UnityEngine;
using MovementEffects;
using System.Collections.Generic;
using Hospital;

public class HospitalUIPrefabController : MonoBehaviour {

    private static HospitalUIPrefabController instance = null;
    public static HospitalUIPrefabController Instance { get { return instance; } }

    public Animator mainUIAnim;
    public bool isHidden = true;
    protected bool withCounters = true;
    protected bool reopenObjectives = false;

    protected Vector2 canvasScale;

	void Start () {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        canvasScale = new Vector2(gameObject.transform.localScale.x, gameObject.transform.localScale.y);

    }
	
    public void ShowMainUI()
    {
        if (isHidden)
        {
            //Debug.LogError("ShowMainUI");
            isHidden = false;
            if (withCounters)
            {
                mainUIAnim.ResetTrigger("Hide");
                mainUIAnim.ResetTrigger("ShowWithoutCounters");
                mainUIAnim.ResetTrigger("HideWithoutCounters");
                mainUIAnim.SetTrigger("Show");
            }
            else
            {
                mainUIAnim.ResetTrigger("Hide");
                mainUIAnim.ResetTrigger("Show");
                mainUIAnim.ResetTrigger("HideWithoutCounters");
                mainUIAnim.SetTrigger("ShowWithoutCounters");
            }
            
            if (!CampaignConfig.hintSystemEnabled && reopenObjectives)
            {
                UIController.getHospital.ObjectivesPanelUI.SlideIn(1f);
                reopenObjectives = false;
            }
        }
    }

    public void ShowMainUI(float time)
    {
        //Debug.LogError("ShowMainUI time: " + time);

        Timing.RunCoroutine(ShowMainUIAfterTime(time));
    }

    public virtual void HideMainUI(bool withCounters = true)
    {
        this.withCounters = withCounters;
        if (!isHidden)
        {
            //Debug.LogError("HideMainUI");
            isHidden = true; 
            if (withCounters)
            {
                mainUIAnim.ResetTrigger("ShowWithoutCounters");
                mainUIAnim.ResetTrigger("HideWithoutCounters");
                mainUIAnim.ResetTrigger("Show");
                mainUIAnim.SetTrigger("Hide");
            }
            else
            {
                mainUIAnim.ResetTrigger("ShowWithoutCounters");
                mainUIAnim.ResetTrigger("Show");
                mainUIAnim.ResetTrigger("Hide");
                mainUIAnim.SetTrigger("HideWithoutCounters");
            }

            if (!CampaignConfig.hintSystemEnabled && UIController.getHospital != null && UIController.getHospital.ObjectivesPanelUI.isSlidIn)
            {
                UIController.getHospital.ObjectivesPanelUI.SlideOut();
                reopenObjectives = true;
            }
        }
    }

    public bool GetState()
    {
        return isHidden;
    }

    private IEnumerator<float> ShowMainUIAfterTime(float time)
    {
        yield return Timing.WaitForSeconds(time);
        ShowMainUI();
        yield return 0;
       // Debug.LogError("ShowAnimAfterTime");

    }

    public Vector2 GetScale()
    {
        return canvasScale;
    }
}

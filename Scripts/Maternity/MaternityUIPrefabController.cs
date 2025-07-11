using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaternityUIPrefabController : HospitalUIPrefabController
{

    public override void HideMainUI(bool withCounters = true)
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
        }
    }

}

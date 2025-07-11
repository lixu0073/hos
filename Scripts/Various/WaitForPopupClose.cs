using SimpleUI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForPopupClose : CustomYieldInstruction
{
    AnimatorMonitor popup;

    public override bool keepWaiting
    {
        get
        {
            return !popup.IsAnimating;
        }
    }

    public WaitForPopupClose(AnimatorMonitor popup)
    {
        this.popup = popup;
    }
}

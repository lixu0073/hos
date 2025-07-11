using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForVisualEffectEnded : CustomYieldInstruction
{
    public VisualEffect visualEffectToWait;

    public override bool keepWaiting
    {
        get
        {
            return !visualEffectToWait.HasEnded();
        }
    }

    public WaitForVisualEffectEnded(VisualEffect visualEffect)
    {
        visualEffectToWait = visualEffect;
    }
}

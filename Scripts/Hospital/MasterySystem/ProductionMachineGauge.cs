using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductionMachineGauge : MonoBehaviour {
    [SerializeField]
    private SpriteRenderer Gauge = null;

    private int mastery;

    public void MasteryAnimation(int masteryLevel, string trigger)
    {
        mastery = masteryLevel;
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger(trigger);
    }

    public void SetMasteryApperance()
    {
        SetAppearance(mastery);
    }

    public void SetAppearance(int masteryLevel)
    {
        if (masteryLevel == 0) {
            Gauge.sprite = ResourcesHolder.Get().gauge0;
            return;
        }
        if (masteryLevel == 1)
        {
            Gauge.sprite = ResourcesHolder.Get().gauge1;
            return;
        }
        if (masteryLevel == 2)
        {
            Gauge.sprite = ResourcesHolder.Get().gauge2;
            return;
        }
        if (masteryLevel == 3)
        {
            Gauge.sprite = ResourcesHolder.Get().gauge3;
            return;
        }
    }


  
}

using MovementEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWorks : OnClickVisualEffects
{
    public ParticleSystem particleSystem;
    public GameObject BoxWick;

    protected override void ToggleReadyToUseEffects(bool isReadyToUse)
    {
        BoxWick.SetActive(isReadyToUse);
    }
}

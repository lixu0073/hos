using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GlobalEventTimer {

    public int CurrentContribution { get { return currentContribution; } private set { } }
    protected int currentContribution;

    public abstract void StartCountingContribution();

    public abstract void StopCountingContribution();
}

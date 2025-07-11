using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public interface ITimerClient
{
    void TimeHasElapsed(ElapsedEventArgs timerEvents);
}

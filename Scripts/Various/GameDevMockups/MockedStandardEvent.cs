using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MockedStandardEvent
{
    [Tooltip("String will be parsed to StandardEventKeys")]
    public string eventKey;
    [Tooltip("When key is generalData: <eventID>#<startTime>#<EndTime>#<ImageDecPoint>. When key is partialData: <value>#<minLevel>")]
    public string eventValue;
    [Tooltip("Check if you want to load it for test")]
    public bool loadEvent;
}

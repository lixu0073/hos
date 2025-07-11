using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MockedBalancedValue
{
    [Tooltip("String will be parsed to BalancableKeys")]
    public string balanceKey;
    [Tooltip("Scheme: <value>")]
    public string Value;
    [Tooltip("Load value")]
    public bool loadValue;
}

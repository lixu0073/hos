using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITabControllerClient
{
    void SetTabContentActive(Action onOpen, Action onFailOpen);
    void DeactiveTabContent();
}
